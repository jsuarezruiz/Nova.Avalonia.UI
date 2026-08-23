#!/usr/bin/env bash

set -euo pipefail

if [[ $# -lt 1 || -z "$1" ]]; then
  printf 'Usage: %s <version> [output-directory]\n' "$0" >&2
  exit 64
fi

version="$1"

if [[ ! "$version" =~ ^[0-9]+\.[0-9]+\.[0-9]+(-[0-9A-Za-z][0-9A-Za-z.-]*)?(\+[0-9A-Za-z][0-9A-Za-z.-]*)?$ ]]; then
  printf 'Version must be a valid SemVer value, for example 1.0.0 or 1.1.0-preview.1.\n' >&2
  exit 64
fi

repository_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
output_directory="${2:-$repository_root/artifacts/packages/$version}"
release_notes="See the v$version release notes at https://github.com/jsuarezruiz/Nova.Avalonia.UI/releases/tag/v$version."

if [[ -n "$(git -C "$repository_root" status --porcelain)" && "${ALLOW_DIRTY:-}" != "1" ]]; then
  printf 'The working tree has uncommitted changes. Commit them before packing so Source Link matches the package sources.\n' >&2
  printf 'Set ALLOW_DIRTY=1 only when creating local validation packages.\n' >&2
  exit 1
fi

projects=(
  "$repository_root/src/Nova.Avalonia.UI/Nova.Avalonia.UI.csproj"
  "$repository_root/src/Nova.Avalonia.UI.BarcodeGenerator/Nova.Avalonia.UI.BarcodeGenerator.csproj"
  "$repository_root/src/Nova.Avalonia.UI.CodeViewer/Nova.Avalonia.UI.CodeViewer.csproj"
)

mkdir -p "$output_directory"

for project in "${projects[@]}"; do
  dotnet pack "$project" \
    --configuration Release \
    -p:Version="$version" \
    -p:PackageVersion="$version" \
    -p:PackageReleaseNotes="$release_notes" \
    -p:TreatWarningsAsErrors=true \
    --output "$output_directory"
done

printf 'Packages created in %s\n' "$output_directory"
