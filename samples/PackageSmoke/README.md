# Package-only GNU/POSIX and internal PCRE consumer

This executable references only the Icod.Expressions NuGet package. It runs
GNU/POSIX checks, 42 byte PCRE protocols and 66 native word PCRE protocols.
It also checks observed input views, cancellation and reuse. The executable
uses managed package entry points directly.

PCRE remains experimental and internal. The explicitly named
Icod.Expressions.PackageSmoke friend is for this repository's package gate;
it is not an exported or stable PCRE API. See the
[bounded candidate contract](../../doc/PCRE-PACKAGE-CONSUMER-DESIGN.md).
The exercise and data files are also linked into tests. The packaged
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

The same invocation also emits `pcre-allocation-smoke`: four anchored raw/UTF/
UCP profiles compare 1,024- and 65,536-byte original views, with eight measured
calls per size after warmup. Each row reports raw managed allocation totals.
The fixed allowance is 4,096 additional bytes per call. Byte-copy and UTF-16
conversion regression controls live in the test suite. No timing threshold is
used; controlled-host timing and wider P09 release evidence remain separate.
See [allocation contract](../../doc/PCRE-ALLOCATION-EVIDENCE-DESIGN.md) for the
current-thread counter scope, callback bounds and covered-profile limitations.

## Anchored tolerant UTF fragment storage

Six additional profiles alternate valid A and malformed FF bytes at 1,024 and
65,536 bytes. Caller anchoring covers match, miss, a late requested offset and
a zero-width match at an invalid requested byte; implicit dot-star anchoring
covers match and miss. Expected complete capture spans come from an independent
pinned native PCRE2 C preflight. The original padded input view remains authoritative.

The existing callback-only allocation probe warms each size eight times and
alternates eight measured calls. A separate pcre-fragment-allocation-smoke JSON
record carries six raw small/large totals. The shared package gate checks exact
names, input sizes, iterations and growth <=4,096 bytes per call on all six
native hosts. Preparation/compilation are excluded; match/capture checks are
included. This bounds covered anchored fragment metadata growth, without claiming
constant unanchored storage, zero total allocation or controlled-host performance.
The earlier 42+1+2 protocol and four-profile allocation records remain required.

## Internal capture metadata

Compiled patterns expose CaptureInfo with the capture count excluding group zero,
unique names in UTF-16 ordinal order, and exact ordinal name lookup returning
sorted distinct slot numbers. Duplicate names and branch-reset aliases retain
all declared slots. Unknown names return an empty list; null is an argument error.
The views are owned, read-only and stable across matches and diagnostics.
This remains an internal experimental API for the package friend consumer.

The consumer verifies 214 native-first count/name/match protocols, including
raw/UTF/UCP, caller/source options, Unicode ordering, DEFINE/recursion/assertion
captures, unset/erased groups, literal source, binary/NUL, 256 slots and 128-byte
names. It overwrites caller pattern bytes after compilation and uses padded
subject views. A fourth pcre-capture-info-smoke JSON record is required alongside
the existing protocol and both allocation records on all six native hosts.
Metadata lookup identifies slots; selecting a participating named capture remains
a separate consumer policy.

## Named captures on match results

Successful byte matches carry the compiled CaptureInfo snapshot. GetCapture(name)
returns the first participating named slot in native table order,
including participating empty captures. Null represents absent/all-unset names;
the metadata distinguishes them. Names use exact ordinal lookup; a null query
is an argument error. Returned spans use original byte coordinates and lookup
does not copy or decode subject text. This remains an internal experimental API.

The fifth pcre-named-capture-smoke result requires 976 native-first protocols,
reusing all 214 frozen metadata inputs and adding native named-substring API
selection/span/byte literals. Preserve all four earlier records. Duplicate-name
substring lookup preserves native table order, including historical2,1 in selected
branch-reset cases; canonical numeric metadata remains sorted separately. The same metadata reference is bound at both success paths;
no post-match record clone is used. The fixed result reference can increase
absolute allocation; paired-input growth allowances remain unchanged.

## Per-call match policy

One compiled pattern accepts an immutable PcreMatchPolicy value on each call.
Anchored, EndAnchored, NotEmpty, NotEmptyAtStart, NotBol and NotEol add
restrictions to compiled/source defaults. The original Match entry keeps those
defaults, and interleaved or concurrent calls retain independent policies.
Line-boundary flags affect caret/dollar, preserving absolute A/Z/z/G anchors.
Captures and marks keep their original byte coordinates, including signed K
ranges. No program clone, subject conversion or new runtime dependency is used.
This remains an internal experimental contract, without an iteration API.

The sixth pcre-match-policy-smoke record requires56 compiled definitions and
3,584 native-first protocols. Each definition reuses one compiled pattern for
all64 policies, interleaving the original entry after every call and preserving
owned pattern/metadata and padded subject-view checks. All five earlier package
records, isolated candidate feed/cache and net8/9/10 builds remain required;
actual consumer execution remains net10 only on all six native hosts.

Native match-time anchoring disables JIT. In tolerant UTF this selects fragment
entry recovery and fragment-local end anchoring; anchored failure stops before a
forward SKIP retry. The call profile can retain multiple fragments. Default calls
retain the earlier compiled/JIT-profile behavior and measured allocation bounds.
Wider malformed-input assertion/verb/start-study conformance remains open.

## Committed fragments and physical end anchors

Tolerant matching with per-call anchoring can continue to the next valid fragment
after a reached root COMMIT rejects the current fragment. Absolute z/Z anchors
still require the physical subject end, independently of fragment-local call
EndAnchored acceptance. Existing scoped cuts, work budgets, cancellation and
capture/mark ownership remain shared by the same VM.

The seventh pcre-commit-fragment-smoke result requires52 definitions and3,328
native-first policy protocols. It reuses the unchanged per-call consumer, with
one compiled pattern per definition and the original entry interleaved after
every policy. Preserve all six earlier records, isolated candidate feeds/caches,
all target builds and net10-only execution. This remains an internal bounded
contract; broader hostile-input/release conformance stays open.

## Raw 16-bit and UTF-16 package consumer

The same executable compiles literal and regex patterns from original word
memory and searches sliced subjects. The 74 word protocols include isolated
surrogates in raw mode, supplementary UTF-16 captures, NUL and high units,
unset groups, call and branch rollback, named marks, signed `\K` endpoints,
successful empty matches, no-match results and controlled compile/match errors.
Six newline cases check CRLF, ANYCRLF and ANY with original capture coordinates.
Eight grapheme cases check combining marks, emoji, raw surrogate units, regional
context, atomic CRLF consumption and greedy bounded/unbounded retries.
Eight script-run cases check supplementary scripts, ordinary/atomic retries,
decimal-set identity, directional Han transitions, raw Unknown and early acceptance.
Eight extended-class cases check Perl and opt-in UTS operators, high raw units,
supplementary scalars, UCP folding, restricted casing and CRLF search entry.
Invalid surrogate sentinels sit outside each visible pattern and subject view.

The compiler's owned source survives overwriting the original pattern buffer.
Two observed-view controls change the subject during matching. Two reuse
controls retain capture ranges, names and labels across caller buffer mutation
and subsequent matches. Six cancellation controls cover pre-cancelled compile
and match calls and interruption during matching, followed by reuse.

The word result is emitted once, after every check passes.

```json
{"kind":"pcre-word-package-smoke","cases":74,"view":2,"cancellation":6,"reuse":2,"failed":0}
```

Tests link the same consumer code and compare all 62 matching/no-match
protocols with the pinned PCRE2-16 reference. The 12 diagnostic protocols pin
the managed error codes and original-unit offsets. PR and main jobs require
the complete word result on all six native hosts after an isolated package
restore. The package friend assembly remains the entry point for these internal
PCRE checks. Public PCRE APIs and general cross-width release qualification
remain scheduled work.
