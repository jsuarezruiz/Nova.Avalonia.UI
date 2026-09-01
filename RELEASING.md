# Releasing Nova Avalonia UI

Releases are created from version tags on `main`. The tag is the source of truth for the NuGet and assembly version, so no workflow or project file needs to be edited for each release.

## One-time setup

Create a NuGet.org API key that can push new packages and package versions matching `Nova.Avalonia.UI*`. Add it to the GitHub repository as an Actions secret named `NUGET_API_KEY`.

## Create a release

Make sure the intended commit is on `main` and its build has passed. Then create and push an annotated tag:

```bash
git switch main
git pull --ff-only
git tag -a v1.0.0 -m "Release v1.0.0"
git push origin v1.0.0
```

Use the next version for later releases, such as `v1.1.0`. Release tags must use the `vMAJOR.MINOR.PATCH` format.

Pushing the tag starts the release workflow. It verifies that the tag points to `main`, builds the desktop and browser galleries, runs the tests, and packs all three libraries with the version from the tag. If validation succeeds, it:

1. Prepares a draft GitHub Release with generated release notes and attaches the `.nupkg` and `.snupkg` files.
2. Publishes the NuGet and symbol packages to NuGet.org.
3. Publishes the GitHub Release after every NuGet package succeeds.

The publish command skips versions that already exist. If publishing only partially succeeds, the draft remains unpublished and a rerun reuses it, skips packages already on NuGet.org, and publishes the GitHub Release after the remaining packages succeed.

## Build packages without releasing

Create local packages by passing the version explicitly:

```bash
./build/pack-nuget.sh 1.0.0
```

The script requires a clean working tree so Source Link refers to the exact source used to build the packages. Set `ALLOW_DIRTY=1` only for local validation artifacts that will not be published.
