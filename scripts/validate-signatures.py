#!/usr/bin/env python3
"""
Scans Deadlock game DLLs for memory signatures defined in config/deadworks_mem.jsonc.
Reports which signatures match, which are broken, and attempts fuzzy recovery for broken ones.

Ports the pattern matching logic from deadworks/src/Memory/Scanner.cpp to Python,
and the PE parsing from deadworks/src/Lib/Module.hpp.
"""

import json
import os
import re
import struct
import sys
from pathlib import Path

# Default Deadlock installation paths (relative to Steam library)
DLL_PATHS = {
    "engine2.dll": "game/bin/win64/engine2.dll",
    "server.dll": "game/citadel/bin/win64/server.dll",
}

DEFAULT_DEADLOCK_DIRS = [
    Path.home() / ".steam/steam/steamapps/common/Deadlock",
    Path.home() / ".local/share/Steam/steamapps/common/Deadlock",
]


def find_deadlock_dir() -> Path | None:
    """Find the Deadlock installation directory."""
    env_dir = os.environ.get("DEADLOCK_DIR")
    if env_dir:
        p = Path(env_dir)
        if p.exists():
            return p

    for d in DEFAULT_DEADLOCK_DIRS:
        if d.exists():
            return d

    return None


def extract_text_section(dll_path: Path) -> tuple[bytes, int]:
    """
    Parse PE headers to extract the .text section contents and its virtual address.
    Returns (section_bytes, virtual_address).
    Only reads headers + the .text section itself, not the entire DLL.
    """
    with open(dll_path, "rb") as f:
        # Read enough for DOS + PE headers and section table
        header = f.read(4096)

        if header[:2] != b"MZ":
            raise ValueError(f"{dll_path} is not a valid PE file")

        e_lfanew = struct.unpack_from("<I", header, 0x3C)[0]

        if header[e_lfanew : e_lfanew + 4] != b"PE\x00\x00":
            raise ValueError(f"{dll_path} has invalid PE signature")

        coff_offset = e_lfanew + 4
        number_of_sections = struct.unpack_from("<H", header, coff_offset + 2)[0]
        size_of_optional_header = struct.unpack_from("<H", header, coff_offset + 16)[0]

        section_start = coff_offset + 20 + size_of_optional_header

        for i in range(number_of_sections):
            offset = section_start + i * 40
            name = header[offset : offset + 8].rstrip(b"\x00").decode("ascii", errors="replace")
            virtual_size, virtual_address, raw_size, raw_offset = struct.unpack_from(
                "<IIII", header, offset + 8
            )

            if name == ".text":
                size = min(virtual_size, raw_size)
                f.seek(raw_offset)
                return f.read(size), virtual_address

    raise ValueError(f"No .text section found in {dll_path}")


def parse_signature(sig_str: str) -> list[int | None]:
    """
    Parse a hex signature string into a pattern.
    '?' or '??' become None (wildcard), hex bytes become int values.
    Mirrors deadworks Scanner::ParseSignature().
    """
    pattern = []
    for token in sig_str.split():
        if not token:
            continue
        if token in ("?", "??"):
            pattern.append(None)
        else:
            pattern.append(int(token, 16))
    return pattern


def pattern_to_regex(pattern: list[int | None]) -> bytes:
    """Convert a parsed pattern to a bytes regex for efficient scanning."""
    parts = []
    for b in pattern:
        if b is None:
            parts.append(b".")
        else:
            parts.append(re.escape(bytes([b])))
    return b"".join(parts)


def find_pattern(text_data: bytes, pattern: list[int | None], limit: int = 0) -> list[int]:
    """
    Find occurrences of a pattern in the .text section.
    If limit > 0, stop after finding that many matches.
    """
    regex = re.compile(pattern_to_regex(pattern))
    offsets = []
    for m in regex.finditer(text_data, re.DOTALL):
        offsets.append(m.start())
        if limit and len(offsets) >= limit:
            break
    return offsets


def fuzzy_search(text_data: bytes, pattern: list[int | None], min_prefix_len: int = 6) -> list[dict]:
    """
    For broken signatures, search for the longest non-wildcard prefix.
    Returns candidate matches with context.
    """
    # Extract the longest prefix of concrete bytes
    prefix = []
    for b in pattern:
        if b is None:
            break
        prefix.append(b)

    candidates = []

    if len(prefix) >= min_prefix_len:
        regex = re.escape(bytes(prefix))
        for m in re.finditer(regex, text_data, re.DOTALL):
            # Extract context around match for manual inspection
            start = m.start()
            end = min(start + len(pattern), len(text_data))
            context_bytes = text_data[start:end]
            context_hex = " ".join(f"{b:02X}" for b in context_bytes)
            candidates.append({
                "offset": start,
                "context": context_hex,
                "prefix_len": len(prefix),
            })
            if len(candidates) >= 5:
                break

    return candidates


def strip_jsonc_comments(text: str) -> str:
    """Remove // line comments from JSONC content, preserving strings."""
    return re.sub(
        r'"(?:[^"\\]|\\.)*"|//[^\n]*',
        lambda m: m.group(0) if m.group(0).startswith('"') else "",
        text,
    )


def main() -> None:
    import argparse

    parser = argparse.ArgumentParser(description="Scan Deadlock DLLs for memory signatures")
    parser.add_argument("--deadlock-dir", type=Path, help="Path to Deadlock installation")
    parser.add_argument("--config", type=Path, help="Path to deadworks_mem.jsonc")
    parser.add_argument("--json", action="store_true", help="Output results as JSON")
    args = parser.parse_args()

    project_root = Path(__file__).resolve().parent.parent
    config_path = args.config or project_root / "config" / "deadworks_mem.jsonc"
    deadlock_dir = args.deadlock_dir or find_deadlock_dir()

    if not deadlock_dir:
        print("Error: Deadlock installation not found. Set DEADLOCK_DIR or use --deadlock-dir.", file=sys.stderr)
        sys.exit(1)

    if not config_path.exists():
        print(f"Error: Config not found at {config_path}", file=sys.stderr)
        sys.exit(1)

    print(f"Deadlock directory: {deadlock_dir}")
    print(f"Config: {config_path}")
    print()

    # Load config
    config_text = config_path.read_text()
    config = json.loads(strip_jsonc_comments(config_text))
    signatures = config.get("signatures", {})

    # Cache loaded DLL text sections
    dll_cache: dict[str, tuple[bytes, int]] = {}

    results = {}

    for name, sig_data in signatures.items():
        library = sig_data.get("library", "")
        pattern_str = sig_data.get("windows", "")

        if not pattern_str:
            results[name] = {"status": "SKIP", "reason": "no windows pattern"}
            continue

        dll_rel = DLL_PATHS.get(library)
        if not dll_rel:
            results[name] = {"status": "SKIP", "reason": f"unknown library {library}"}
            continue

        dll_path = deadlock_dir / dll_rel
        if not dll_path.exists():
            results[name] = {"status": "SKIP", "reason": f"{dll_path} not found"}
            continue

        if library not in dll_cache:
            try:
                dll_cache[library] = extract_text_section(dll_path)
                print(f"Loaded {library} .text section ({len(dll_cache[library][0]):,} bytes)")
            except Exception as e:
                results[name] = {"status": "ERROR", "reason": str(e)}
                continue

        text_data, text_vaddr = dll_cache[library]
        pattern = parse_signature(pattern_str)
        offsets = find_pattern(text_data, pattern, limit=2)

        if len(offsets) == 1:
            rva = text_vaddr + offsets[0]
            results[name] = {"status": "MATCH", "rva": f"0x{rva:X}", "offset": offsets[0]}
        elif len(offsets) > 1:
            rvas = [f"0x{text_vaddr + o:X}" for o in offsets]
            results[name] = {"status": "MULTIPLE", "count": len(offsets), "rvas": rvas}
        else:
            candidates = fuzzy_search(text_data, pattern)
            results[name] = {
                "status": "BROKEN",
                "pattern": pattern_str,
                "candidates": candidates,
            }

    # Derive summary from results
    matched = sum(1 for r in results.values() if r["status"] in ("MATCH", "MULTIPLE"))
    broken = sum(1 for r in results.values() if r["status"] in ("BROKEN", "ERROR"))
    skipped = sum(1 for r in results.values() if r["status"] == "SKIP")

    if args.json:
        output = {
            "summary": {"matched": matched, "broken": broken, "skipped": skipped, "total": len(signatures)},
            "results": results,
        }
        print(json.dumps(output, indent=2))
    else:
        print(f"\n{'=' * 60}")
        print(f"Results: {matched} matched, {broken} broken, {skipped} skipped (of {len(signatures)} total)")
        print(f"{'=' * 60}\n")

        for name, result in results.items():
            status = result["status"]
            if status == "MATCH":
                print(f"  OK  {name} @ {result['rva']}")
            elif status == "MULTIPLE":
                print(f"  OK  {name} @ {result['rvas'][0]} ({result['count']} matches)")
            elif status == "BROKEN":
                print(f"  BROKEN  {name}")
                if result["candidates"]:
                    print(f"         {len(result['candidates'])} fuzzy candidate(s):")
                    for c in result["candidates"]:
                        print(f"           offset 0x{c['offset']:X} (prefix {c['prefix_len']} bytes)")
                        print(f"           {c['context'][:80]}...")
                else:
                    print("         No fuzzy candidates found")
            elif status == "SKIP":
                print(f"  SKIP  {name}: {result['reason']}")
            elif status == "ERROR":
                print(f"  ERROR  {name}: {result['reason']}")


if __name__ == "__main__":
    main()
