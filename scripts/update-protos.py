#!/usr/bin/env python3
"""
Fetches .proto files from SteamTracking/GameTracking-Deadlock and regenerates
the compiled C++ protobuf files (protobuf/*.pb.cc/h) and managed proto sources
(managed/protos/*.proto).

Works on Linux, Windows, and macOS. Requires git and either a local protoc
(from sourcesdk submodule) or downloads protoc 3.21.12 automatically.
"""

import os
import platform
import shutil
import subprocess
import sys
import tempfile
import zipfile
from pathlib import Path
from urllib.request import urlretrieve

REPO_URL = "https://github.com/SteamTracking/GameTracking-Deadlock.git"
PROTOC_VERSION = "3.21.12"

# Proto files to compile to C++ (.pb.cc/.pb.h)
CPP_PROTOS = [
    "base_gcmessages",
    "citadel_clientmessages",
    "citadel_gcmessages_common",
    "citadel_usermessages",
    "gameevents",
    "gcsdk_gcmessages",
    "netmessages",
    "network_connection",
    "networkbasetypes",
    "networksystem_protomessages",
    "source2_steam_stats",
    "steammessages",
    "steammessages_steamlearn.steamworkssdk",
    "steammessages_unified_base.steamworkssdk",
    "valveextensions",
]

# Additional proto files to copy for managed/protos/ (beyond CPP_PROTOS)
EXTRA_MANAGED_PROTOS = [
    "citadel_gameevents",
    "citadel_usercmd",
    "te",
    "usercmd",
    "usermessages",
]


def get_project_root() -> Path:
    return Path(__file__).resolve().parent.parent


def find_local_protoc(project_root: Path) -> tuple[Path, Path] | None:
    """Try to find protoc and its include dir from the sourcesdk submodule."""
    system = platform.system()
    if system == "Windows":
        candidate = project_root / "sourcesdk" / "devtools" / "bin" / "win64" / "protoc.exe"
    elif system == "Darwin":
        candidate = project_root / "sourcesdk" / "devtools" / "bin" / "osx64" / "protoc"
    else:
        candidate = project_root / "sourcesdk" / "devtools" / "bin" / "linuxsteamrt64" / "protoc"

    include_dir = project_root / "sourcesdk" / "thirdparty" / "protobuf" / "src"

    if candidate.exists() and include_dir.exists():
        return candidate, include_dir
    return None


def download_protoc(dest_dir: Path) -> tuple[Path, Path]:
    """Download protoc binary and includes for the current platform."""
    system = platform.system()
    machine = platform.machine().lower()

    if system == "Windows":
        asset = "protoc-{ver}-win64.zip"
    elif system == "Darwin":
        if machine == "arm64":
            asset = "protoc-{ver}-osx-aarch_64.zip"
        else:
            asset = "protoc-{ver}-osx-x86_64.zip"
    else:
        if machine == "aarch64":
            asset = "protoc-{ver}-linux-aarch_64.zip"
        else:
            asset = "protoc-{ver}-linux-x86_64.zip"

    asset = asset.format(ver=PROTOC_VERSION)
    url = f"https://github.com/protocolbuffers/protobuf/releases/download/v{PROTOC_VERSION}/{asset}"

    zip_path = dest_dir / asset
    print(f"Downloading protoc {PROTOC_VERSION} from {url} ...")
    urlretrieve(url, zip_path)

    extract_dir = dest_dir / "protoc"
    with zipfile.ZipFile(zip_path) as zf:
        zf.extractall(extract_dir)

    if system == "Windows":
        protoc = extract_dir / "bin" / "protoc.exe"
    else:
        protoc = extract_dir / "bin" / "protoc"
        protoc.chmod(0o755)

    include_dir = extract_dir / "include"
    return protoc, include_dir


def clone_upstream(dest: Path) -> Path:
    """Shallow-clone the upstream repo and return the Protobufs directory."""
    print(f"Cloning {REPO_URL} (shallow) ...")
    subprocess.run(
        ["git", "clone", "--depth", "1", "--filter=blob:none", "--sparse", REPO_URL, str(dest)],
        check=True,
    )
    subprocess.run(
        ["git", "sparse-checkout", "set", "Protobufs"],
        cwd=dest,
        check=True,
    )
    return dest / "Protobufs"


def copy_managed_protos(upstream_dir: Path, managed_dir: Path) -> None:
    """Copy .proto files to managed/protos/, updating only files that already exist."""
    copied = 0
    for existing in sorted(managed_dir.glob("*.proto")):
        src = upstream_dir / existing.name
        if src.exists():
            shutil.copy2(src, existing)
            copied += 1
        else:
            print(f"  Warning: {existing.name} not found upstream, skipping")
    print(f"Copied {copied} .proto files to {managed_dir}")


def compile_cpp_protos(
    protoc: Path, protobuf_include: Path, upstream_dir: Path, output_dir: Path
) -> None:
    """Compile .proto files to C++ using protoc."""
    print(f"Compiling {len(CPP_PROTOS)} proto files with {protoc} ...")
    for name in CPP_PROTOS:
        proto_file = f"{name}.proto"
        src = upstream_dir / proto_file
        if not src.exists():
            print(f"  Warning: {proto_file} not found upstream, skipping")
            continue

        result = subprocess.run(
            [
                str(protoc),
                f"-I{protobuf_include}",
                f"-I{upstream_dir}",
                f"--cpp_out={output_dir}",
                proto_file,
            ],
            cwd=upstream_dir,
            capture_output=True,
            text=True,
        )
        if result.returncode != 0:
            print(f"  Error compiling {proto_file}:\n{result.stderr}", file=sys.stderr)
            sys.exit(1)
        print(f"  Compiled {proto_file}")

    print(f"Generated C++ files in {output_dir}")


def main() -> None:
    project_root = get_project_root()
    protobuf_dir = project_root / "protobuf"
    managed_protos_dir = project_root / "managed" / "protos"

    if not protobuf_dir.exists():
        print(f"Error: {protobuf_dir} not found. Run from the project root.", file=sys.stderr)
        sys.exit(1)

    tmpdir = Path(tempfile.mkdtemp(prefix="deadworks-protos-"))
    try:
        # Find or download protoc
        local = find_local_protoc(project_root)
        if local:
            protoc, protobuf_include = local
            print(f"Using local protoc: {protoc}")
        else:
            print("Local protoc not found, downloading ...")
            protoc, protobuf_include = download_protoc(tmpdir)
            print(f"Using downloaded protoc: {protoc}")

        # Clone upstream
        clone_dir = tmpdir / "upstream"
        upstream_protos = clone_upstream(clone_dir)

        # Update managed protos
        if managed_protos_dir.exists():
            copy_managed_protos(upstream_protos, managed_protos_dir)
        else:
            print(f"Warning: {managed_protos_dir} not found, skipping managed protos")

        # Compile C++ protos
        compile_cpp_protos(protoc, protobuf_include, upstream_protos, protobuf_dir)

        print("\nDone! Review changes with: git diff --stat")
    finally:
        shutil.rmtree(tmpdir, ignore_errors=True)


if __name__ == "__main__":
    main()
