# Faster macOS Intel CI with complete framework verification

Main run42775146129 took10m47s on macOS x64:57s SDK setup,155s build,
394s tests/result verification and25s parity. Three framework hosts share one
Intel runner. Preserve six-platform native coverage and all45,779 cases and
frozen214 per framework. Keep Debug/Staging/Release and single-library packaging.

Run Intel net8/net9/net10 as independent native jobs on PR/main. Each restores
and builds only its framework, using SDK10/C#13 plus its target SDK/runtime when
needed, and runs full tests and extraction parity. Each verifies fresh results
against a contract-approved selected framework and uploads its TRX. Keep the
other five native jobs unchanged. Install only SDK10 for the net10-only job.

Keep the existing macOS x64 status as an aggregate: require all three children
successful, download the three TRX files from this run, and apply the original
full three-framework contract. Missing/duplicate/failed/skipped/substituted
results still fail. Child verification retains invocation freshness checks.
Artifacts use one-day retention and low compression cost; no outputs are cached.

Extend Invoke-Tests with optional Framework/ResultsDirectory and the validator
with optional Framework checked against the original contract. Defaults and all
original checks stay. Refuse to reuse an explicit result directory. A shared
composite action supplies native Intel restore/build/test/parity/upload steps.

Main now has nine jobs representing six platforms. Update tag release's exact
main prerequisite to require nine exact expected names, all successful. Its own
six-platform Release validation stays. Preserve the macOS x64 required check
name so GitHub settings need no change.

Disabling theory pre-enumeration would group case identities and conflict with
strict TRX checks. Disabling source mapping alone cannot help: VSTest already
emits CollectSourceInformation=false for CLI runs. Isolate framework work instead.
Compare cold exact-head timings with10m47s main and previous Staging run616;
runner variation means timing is evidence, not a duration promise.

No regex/test-data/dependency/API/version/consumer changes, no subagents, Python,
local SDK or PowerShell installation. User owns review/merge; no publication.
