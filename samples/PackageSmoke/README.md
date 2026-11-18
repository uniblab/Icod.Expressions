# Package-only GNU/POSIX and internal PCRE consumer

This executable references only the Icod.Expressions NuGet package. It keeps
its existing GNU/POSIX checks and adds 42 PCRE protocols, one observed byte-view
control and two cancellation controls. There is no project reference, native
PCRE.NET runtime dependency, copied matcher or reflection in this consumer.

PCRE remains experimental and internal. The explicitly named
Icod.Expressions.PackageSmoke friend is for this repository's package gate;
it is not an exported or stable PCRE API. See the
[bounded candidate contract](../../doc/PCRE-PACKAGE-CONSUMER-DESIGN.md).
The two exercise/data source files are also linked into tests; the packaged
executable compiles them against the candidate package itself.

The cases check raw NUL/high bytes, UTF-8/supplementary values, UCP, original
visible byte offsets, numbered/unset captures, marks, pattern ownership,
malformed UTF, start/work-limit diagnostics and the declared unsupported UTF
code-unit escape. Padded views exclude invalid outside sentinels. The observed
control checks the original buffer on that execution path; broader no-transcoding,
hostile-input and performance release gates remain open.

PR/main CI builds all three library and consumer targets and executes net10
on Windows/Linux/macOS x64/ARM64. Each job packs a fresh already-built candidate,
uses a unique local feed and an isolated empty package cache, and checks one
complete structured result. Existing main package-artifact verification and
release publication remain separate. The release workflow retains its existing
isolated sample invocation.

For a local PowerShell verification after building the solution:

```powershell
$pcreSmokeRoot = Join-Path ([IO.Path]::GetTempPath()) ('icod-pcre-smoke-' + [Guid]::NewGuid().ToString('N'))
$pcreSmokeFeed = Join-Path $pcreSmokeRoot 'feed'
$pcreSmokeCache = Join-Path $pcreSmokeRoot 'cache'
New-Item -ItemType Directory -Path $pcreSmokeFeed, $pcreSmokeCache | Out-Null
dotnet pack Icod.Expressions.csproj -c Staging --no-build --no-restore -o $pcreSmokeFeed
$env:NUGET_PACKAGES = $pcreSmokeCache
dotnet restore samples/PackageSmoke/PackageSmoke.csproj --source $pcreSmokeFeed --no-cache --force-evaluate
dotnet build samples/PackageSmoke/PackageSmoke.csproj -c Staging --no-restore
dotnet run --project samples/PackageSmoke/PackageSmoke.csproj -c Staging --no-build --no-restore -f net10.0
```

The PCRE line is:

```json
{"kind":"pcre-package-smoke","cases":42,"view":1,"cancellation":2,"failed":0}
```

Any assertion, compile/match/resource or cancellation-contract mismatch exits
with failure. The sample performs no timing assertions or dependency-version
checks. User publication/tag decisions are unchanged.
