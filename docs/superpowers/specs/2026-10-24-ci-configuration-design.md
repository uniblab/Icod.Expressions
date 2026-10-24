# CI configuration and architecture design

## Intent

Keep developer builds in Debug, validate pull requests in Staging, validate `main` in Release, and build and publish tags in Release. Exercise the solution on native x64 and ARM64 runners for Linux, Windows, and macOS before a release is published.

## Constraints

- Keep the existing single `Icod.Expressions.csproj` NuGet package and .NET 8/9/10 test coverage.
- Preserve the frozen GNU characterization checks and source-pinned consumer checks in PR validation.
- Do not change `Icod.CommandFramework` or consumer production paths.
- Local `build.sh`, `build.cmd`, and `Invoke-Build.ps1` continue to default to Debug.
- A tag release must require a successful six-lane `main` Release run on its exact commit and then pass its own six-lane Release validation before publication.

## Approaches

One workflow could select Staging or Release from the event name, but it would diverge from the shared `.github` template's separate PR and main workflows. Separate workflows let the event and configuration be explicit and keep the release gate tied to `main.yaml`; use this approach. A reusable workflow would reduce duplication but add an indirection to a small repository-specific matrix.

## Implementation

Use `ubuntu-latest`, `windows-latest`, and `macos-latest` for their native x64, x64, and ARM64 lanes, and `ubuntu-24.04-arm`, `windows-11-arm`, and `macos-15-intel` for the complementary lanes. PR and main workflows each build and test all six in Staging and Release respectively. Run external source and package parity checks on their supported PR hosts; pack the one architecture-independent library on Linux x64. Tag validation runs Release on all six, and the package job waits for it. Update the release guard from five jobs in `pull-request.yml` to six jobs in `main.yaml`.

## Verification

Statically check event triggers, configuration values, six unique runner/architecture pairs, no Debug CI builds, and release dependencies; run the PR's actual six jobs and inspect each job before calling it ready. Local .NET execution depends on the SDK being available.
