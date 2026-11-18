# Paired-input managed allocation evidence

Base merged PR283 `43a90da82c38e4159e4f865178655265bf7250ad`, tree
`4ba16a2a2a3764033bb2b892eaf3339b7516d3b0`;104,760 prior tests.
Advance bounded P09 no-transcoding/allocation evidence without changing the
matcher/compiler, public API, dependencies or package identity. No agents,
Python or local SDK installation. Preserve all376 old C# test blobs, A05
source35/corpus, #7/#102 and issue #7 footer. Grep last; user owns merge/tags.

## Native-first semantic profiles

Independently build pinned PCRE2 10.48
7978954dbd2efc6f2196869290553cf1871b4ce6 with SLJIT
de0259c7aaf36aa40cba8014f3fad3edde9307f9. An original checked-in C probe
constructs subjects directly, avoiding the older line-protocol length ceiling.
Four explicitly anchored profiles run at1,024 and65,536 bytes:

| Profile | Modes / input | Native captures |
|---|---|---|
| raw-code-unit | Raw FF00 repetitions; anchored code-unit capture | 0:1,0:1 |
| utf8-dot | UTF Greek lambda repetitions; anchored dot capture | 0:2,0:2 |
| ucp-letter | UTF/UCP lambda repetitions; anchored letter property | 0:2,0:2 |
| utf8-no-match | UTF lambda repetitions; anchored ASCII Z | no-match |

All eight native outcomes agree with these literals; JIT return0 /positive
size. Native output SHA256
3ec6e5d2e40227b11951239711fbf4470d64065a5651f72cc7ede4bc80f00c1d.
The xUnit profiles repeat native byte assertions before the consumer/probe
stub. No allocation expectation is inferred from a native timing or memory
metric. The C probe records only status/captures/JIT facts.

## Measurement contract and limits

Use [GC.GetAllocatedBytesForCurrentThread](https://learn.microsoft.com/en-us/dotnet/api/system.gc.getallocatedbytesforcurrentthread?view=net-10.0)
as a synchronous callback allocation counter. It measures managed allocations
charged to the calling thread, not retained heap, native memory or other
threads. No elapsed-time/throughput threshold or physical-host performance
baseline is claimed.

A new test/sample-only probe receives original small/large memory views and
an Action callback. Require large length greater than small, maximum262,144
bytes and iterations1..32. Empty small is permitted. Reject invalid arguments
before any callback. Eight warmup calls per size precede alternating small/
large measurement order. Count only callback allocations: exclude counter,
result construction and preparation. Total visits16+2*iterations are bounded.
Check cancellation before/after each visit; propagate callback failures.
Synchronous callback runtime remains caller-controlled, not preemptible.

Consumer profiles prepare/compile outside measurement, use visible memory
slices with invalid outside sentinels and verify exact complete-match/capture
semantics on every callback. Anchoring prevents unrelated candidate/state
growth; strict valid UTF still validates the large original view. Measure
matching plus fixed capture validation overhead. No observer is attached.
Require large minus small allocations <=4,096 bytes per call, at eight measured
calls per size. This is a deliberately loose fixed allowance compared with
64,512 additional source bytes. It is a covered-profile growth gate, not an
absolute zero-allocation promise or proof for every pattern/input/mode.
Tolerant fragmentation and capture/backtracking-state growth remain separate.

Two deliberately allocating callbacks (whole-byte copy and UTF-16 decode)
must violate the allowance. They demonstrate that the probe catches the
specific regressions it is meant to reject. Additional controls check no-op
allocation counts, exact visit bounds, argument ceilings, cancellation/errors,
authoritative input-view identity, empty baseline and maximum input size.
No forced GC, benchmark-time assertion or per-runtime dependency-version test.

## Tests-first and integration

21 new tests give104,781 total. Publish a tests-only draft with missing probe/
consumer implementations: expected successful-build RED21 /104,760 passes /
zero skips, with all native assertions passing first. Reconcile identities/
causes; freeze profile data, assertions, C probe and the test-project XML links
before implementation. Implement only the two new helpers and freeze them.

The sole old-test-file exception adds three source links to the existing
net10 test project; old links/references/dependencies/frameworks remain fixed.
Source-link the same helpers into tests; the existing package-only sample
executes them against the fresh candidate binary. Keep the previous42 protocol,
one-view/two-cancellation exercise and its frozen helper/data unchanged.

The sample emits one additional pcre-allocation-smoke record with four named
profiles and raw small/large managed byte totals. Extend the existing shared
six-host action to require exact names/sizes/iterations, nonnegative counters,
the declared fixed allowance and four passing results. Keep its isolated feed/
cache and existing package protocol enforcement intact. Existing PR/main/Intel
call sites and release workflow remain unchanged. No new producer/friend/API,
project/tool dependency or CI jobs are required.

## Qualification and review

Review the whole branch and this design/plan plus all five shared inventories
before final source qualification. Preserve the production src tree
`d1790dfc3fc83625ecb4915942ca0e83219cb25b` and PCRE subtree
`cbe160ce6605be45840127ce36ba5aca09b66bf7` exactly. Run local preservation/gate
checks. Six native104,781 full net10/frozen214 cases/120 methods, all library/
consumer target builds, existing42+1+2 package results and four allocation
profiles per host must pass with zero failures/skips/coded warnings/errors.
Actual configured compatibility/Gnulib/support/script gates remain required.

Record final source/tree/base/merge identities, raw allocation evidence, plan
completion, cleanup and ready readback externally on PR284. No source/docs/test
edits after final GREEN. P09 remains open for wider contract/hostile/performance
release closure; no native16/P10-P18, remaining P01, Stage C or downstream
migration gate closes merely from these four allocation profiles.

## Successful-build RED and fixture freeze

Tests-only source5baf2c8b4cf405bb86c5ff5cb7a9ef8ef921801b, tree
70ed29ba576aa12815f6ecc5bdce5bff15126e56; proposed merge
febca97bceb40f94309eedc13089eaf1579d4619. Run44416467605, Linux x64
job135197967386 builds net8/net9/net10 with zero coded warnings/errors and
reports21 failed /104,760 passed /104,781 total /zero skips. All failures
reach missing new helpers:17 probe /4 consumer. Nine are expected exception-
type mismatches (arguments6/cancellation2/callback-error1). Every native profile
assertion and every old case passes. Freeze before helper implementation:

- samples/PackageSmoke/PcreAllocationCases.cs: ed492a89b7258c4e1bfea14cd971bb562d485a5f.
- tests/Pcre/PcreAllocationEvidenceTests.cs: 05e642b7ba33f0a590bfe3a15875d08a8ab2d6b6.
- tests/Icod.Expressions.Tests.csproj: 4a2043c47136153c82baae69bc5de5ac34adba05.
- tests/Pcre/Native/allocation-subject-oracle.c: a3d943a93e9eb7b8fdd27772153c9344dfb93ebc.

## Completed-helper freeze and prequalification review

Probe blob 61cf27896c4f98004c278b6d59c613f78721f2b0; consumer blob
f22d10421d5a813954fc523371ec13e77aba8725. Reviewed the whole 17-file branch: native setup
provenance, frozen capture semantics, callback-only counter boundaries, original
views, eight warmups per size, alternating order, exact bounded visits, argument
limits, pre/post cancellation and error propagation. Reviewed compilation/input
preparation outside measurement, fixed capture validation overhead, anchoring,
padded slices and growth allowance relative to fault-control input deltas.

Reviewed sample output and strict shared-action enforcement for four exact
names, input sizes, iterations, counters and bounds; existing42+1+2 result and
isolated feed/cache remain enforced. Existing workflow/release call sites,
package identity and dependencies stay fixed. Managed current-thread counters
do not represent native/retained heap or controlled-host performance.

All376 old C# test blobs stay identical. Only three documented test-project
source links change among386 prior test files. Production src remains
d1790dfc3fc83625ecb4915942ca0e83219cb25b; old package helper/data and A05
source35/corpus remain fixed. The four new fixture/assertion/native/XML blobs
above are still frozen. All seven inventories were reviewed before upload.
Local A05 identity, classifier, eight-job/thirteen-rejection prerequisite and
whitespace checks pass. Real hosted compiler, PowerShell and six-host allocation
records provide final qualification; no local SDK was installed. Final metrics,
identities, cleanup and ready readback stay on PR284, with no source/docs/test
edits after final GREEN.
