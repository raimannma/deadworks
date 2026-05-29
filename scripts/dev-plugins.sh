#!/usr/bin/env bash
#
# dev-plugins.sh — build a Deadworks plugin into the hot-reload folder and
# rebuild it on every source change.
#
# The running server (started with docker-compose.dev.yaml) watches the same
# folder via PluginLoader's FileSystemWatcher and hot-reloads any DLL that
# changes — no server restart needed.
#
# Usage:
#   ./scripts/dev-plugins.sh <plugin-dir-or-csproj> [output-dir]
#
# Examples:
#   ./scripts/dev-plugins.sh examples/plugins/RollTheDicePlugin
#   ./scripts/dev-plugins.sh ../my-plugins/CoolPlugin/CoolPlugin.csproj
#   ONESHOT=1 ./scripts/dev-plugins.sh examples/plugins/TagPlugin   # build once, no watch
#
# Requires the .NET 10 SDK on the host (dotnet --version).
set -euo pipefail

REPO="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
OUT="${2:-${REPO}/plugins-live}"

if [ $# -lt 1 ]; then
    echo "usage: $0 <plugin-dir-or-csproj> [output-dir]" >&2
    exit 2
fi

# Resolve the target to a .csproj.
TARGET="$1"
if [ -d "$TARGET" ]; then
    CSPROJ="$(find "$TARGET" -maxdepth 1 -name '*.csproj' ! -name '*.Tests.csproj' | head -n1)"
elif [ -f "$TARGET" ]; then
    CSPROJ="$TARGET"
else
    echo "error: '$TARGET' is not a directory or file" >&2
    exit 1
fi

if [ -z "${CSPROJ:-}" ] || [ ! -f "$CSPROJ" ]; then
    echo "error: no .csproj found for '$TARGET'" >&2
    exit 1
fi

CSPROJ="$(cd "$(dirname "$CSPROJ")" && pwd)/$(basename "$CSPROJ")"
mkdir -p "$OUT"

echo "Plugin:  $CSPROJ"
echo "Output:  $OUT  (bind-mounted to the container as /opt/live-plugins)"
echo

# `publish -o` writes the plugin DLL + its full dependency closure flat into the
# watched folder, while honoring the plugin's <Private>false</Private> reference
# to DeadworksManaged.Api — so the host's copy of the API is used (preserving
# type identity) and the watched folder stays clean (only plugin + its deps).
#
# Build args:
#   DeadlockManagedDir -> a throwaway dir so the csproj's Windows-only
#     "DeployToGame" copy target succeeds quietly instead of erroring on /plugins.
#   NoWarn=CS1591      -> silence the API project's missing-XML-doc warnings.
DEPLOY_NOOP="${TMPDIR:-/tmp}/deadworks-deploy-noop"
PUBLISH_ARGS=(-o "$OUT" --nologo -p:DeadlockManagedDir="$DEPLOY_NOOP" -p:NoWarn=CS1591)

if [ "${ONESHOT:-0}" = "1" ]; then
    exec dotnet publish "$CSPROJ" "${PUBLISH_ARGS[@]}"
else
    echo "Watching for changes — edit + save to rebuild and hot-reload. Ctrl-C to stop."
    exec dotnet watch --project "$CSPROJ" publish "${PUBLISH_ARGS[@]}"
fi
