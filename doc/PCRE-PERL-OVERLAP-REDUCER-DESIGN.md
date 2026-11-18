# Pinned Perl overlap and differential reducer

Base merged PR281 `f578d1eea423e4cc7f1de3c94edbb22d2ec17ded`, tree
`e0998dd6136498a7a041c03327d22882f8d2a9ed`; 104,344 existing tests.
This advances the open P00/P01 overlap/reducer seam before P09/P10 work.
It does not implement a managed 16-bit backend or close those release gates.
Work inline without agents, Python or local SDK installation. Preserve all
379 prior test files / 371 C# sources, the production tree, frozen A05 source35/
corpus, #7/#102 and issue #7 footer. Grep last; user owns merge/tags/publication.

## Independent overlap contract

Pin Perl v5.40.1 and its Unicode 15.0.0 database. Release tag object
`8efa696b85175eac77a8c3e2b4140194fe62bd0f` points to source commit
`f722f11a2b1a34e28743e331bad6dd7da41e2086`. The local reference interpreter
reports v5.40.1 / x86_64-linux-gnu-thread-multi / threads enabled; its binary
SHA256 is b85210f03baeb859ee20b1324e59497f27f319eda9e2d3d1ecd163a79d884a7d.
This records the actual development binary, not a bit-identical source-build
claim. The checked-in development-only runner rejects another release/UCD.

41 author-designed templates (40 distinct patterns) produce 340 valid-Unicode
matching/start fixtures: 221 matches and 119 no-matches. Perl /g receives a
fresh original subject and explicit scalar position per row; no slicing or
repeated-match iteration is used. Record every numbered capture, including
trailing unset slots, as scalar start:length. The selected overlap covers
ordered alternatives, repetition, captures/branch reset, named references,
assertions/lookbehind, recursion/DEFINE/conditions, anchors, K and Unicode
literals. PCRE2 uses explicit UTF/UCP, LF/Unicode BSR, JIT and original native
start offsets. The independent mapper separates scalar meaning from byte/
16-bit coordinates; Perl character offsets are never called native byte offsets.

Unmodified pinned PCRE2 10.48 `7978954dbd2efc6f2196869290553cf1871b4ce6`
and SLJIT `de0259c7aaf36aa40cba8014f3fad3edde9307f9` build the frozen C
oracle against its generated header. All 680 native probes have JIT return
zero / positive size and agree with Perl after independent normalization.
No rows were removed for a mismatch. Two identical Perl runs reproduce the
same output SHA256 d38d9a3c75ae9acbf44e402294daa13747e01584e61bec2e3b463f9b2b9b81bf;
the paired native output SHA256 is c32ad2ae331750a36c708db1aec21412b395872a337418c3aef232afffd9316f.

The overlap is deliberately bounded: valid compiled patterns, complete single
matches and numbered captures on the selected inputs. There is no claim that
Perl and PCRE2 agree generally. Unicode 15/17 property/folding differences,
malformed UTF, split starts, compile diagnostics, marks, duplicate-name maps,
callouts, partial/substitution/iteration and locale/code execution are not
normalized into this comparison. Existing native fixtures retain their own
contracts. No third-party test data or implementation is imported: all cases
and the runner are original. Perl source links identify the reference release.

CI runs frozen literal expectations through PCRE.NET native8/native16, the
existing direct native16 bridge and managed bytes. Perl itself is a pinned
independent development preflight, not a newly required six-host dependency.
The runner, input reconstruction and literal corpus remain reproducible.

## Reducer contract

Add a test-only generic native-unit reducer over owned copies of bounded
pattern/subject memories (at most 4,096 units each). Byte, char and ushort
witnesses remain values in their original width; the reducer performs no
encoding conversion, parsing, regex execution or diagnostic reclassification.
A deterministic callback owns the precise mismatch classification and must
reject invalid/unrelated candidates. A fixed original start offset is retained;
subject candidates shorter than it never reach the callback.

Deterministic complement deletion tries coarse chunks and refines to single
units in each field, revisiting both fields until a complete pass accepts no
removal. Each accepted witness is strictly smaller. Completed means one-unit
local minimality under that predicate, not a globally smallest pair or AST-
aware reduction. Simultaneous field changes may expose smaller witnesses that
this skeleton cannot reach; explicitly test that limitation.

Count the original check and every oracle invocation in one budget (1..65,536;
default 1,024). At exhaustion, return the last accepted witness and a distinct
EvaluationLimit status. A nonreproducing original returns NotReproduced after
one check. Check cancellation before/after callbacks; propagate callback
exceptions rather than treating them as no mismatch. Oracle runtime is the
caller's responsibility; this synchronous skeleton cannot preempt a callback.
Results own their buffers. Pure callback behavior is required for determinism.

31 reducer cases cover both fields, byte/char/ushort units, fixed offsets,
argument rejection, cancellation, callback errors, exact evaluation caps,
ownership, deterministic traces, empty inputs, fixed points and the local-
minimality limit. A native-backed integration shrinks a deliberately faulty
consumer comparison of raw byte/word capture coordinates to the single scalar
U+00E9; independent normalization then proves that the native engines agree.
This is a comparison-fault witness, not an alleged matcher defect.

## Qualification and freeze

340 overlap cases plus 31 reducer cases add 371 tests: 104,715 total. Publish
a tests-only draft with a missing reducer; expected successful-build RED is
31 missing-reducer failures / 104,684 passes / zero skips. Reconcile identities
and causes before freezing the three fixture/assertion sources and Perl runner.
Implement only the reducer helper afterward, then freeze it too.

Review the whole branch and update this design/plan plus the five shared
inventories before final qualification. Build net8/net9/net10; run full net10
and frozen214 cases /120 methods on all six hosts with zero failures/skips/
coded warnings/errors, plus configured support/package/script gates. Keep final
source/tree/base/merge identities, plan completion, cleanup and ready readback
on PR282. No source/docs/test edits after final GREEN. P01 remains partial;
P09-P18, Stage C and real consumer migration are not closed by this increment.

## Successful-build RED and source freeze

Tests-only source 0763d93fb2e803b5810625a9a45bcd7a6051ba72, tree
a539b6a438fdf9f40f0aa8f02a1334e6bca12a34; proposed merge
a631879298f470ce82d09a45f691d09858e8a8a7 against the base above.
Run44388013559, Windows x64 job135097751072 builds net8/net9/net10 with
zero coded warnings/errors, then reports31 failures /104,684 passes /
104,715 total /zero skips. All340 overlap cases and104,344 prior tests pass.
All31 failures reach the missing Reduce helper: four minimum rows, five budget
rows, eight argument rows and14 facts. Eleven are expected exception-type
mismatches (arguments8/cancellation2/oracle-error1); the underlying cause of
every failure is NotImplementedException, with no native-literal assertion failure.

Freeze these sources before implementing only the helper:

- tests/Pcre/PcrePerlOverlapCases.cs: 9d17cb50bb731f9e763b7cd58d7626597b07deba.
- tests/Pcre/PcrePerlOverlapReferenceTests.cs: 8d4ce4d5f38bf93d0b3c0c95bd26c82fdfda8f8b.
- tests/Pcre/PcreFixtureReducerTests.cs: e120a9a1c8ff4eecf1bbedce0d2cdff4de51e77b.
- tests/Pcre/Native/perl-overlap.pl: f4979cb96faf939978fa6688d2fbe73f3d033764.

The README reconstruction also reproduces all340 Perl rows byte-for-byte; five
invalid row/hex/UTF/start/compile controls fail before producing a record.
Final source/helper identity and remaining host evidence stay externally on
PR282. No frozen assertion, runner or native expected output is revised.

## Prequalification author review and completed helper freeze

Completed reducer helper Git blob: addceed519f99a81fec0f8991229d3a696c043ab.
Reviewed the complete 13-file branch against the merged base: literal Perl/
native protocols and release/binary provenance, Unicode/native coordinate
separation, original-case licensing, runner input/compile/timeout behavior,
fixed start offsets, strictly shrinking candidates, complete unchanged-pass
minimality, budget-before-call boundaries, owned buffers and pre/post callback
cancellation/error propagation. The synchronous oracle remains caller-bounded;
no AST/global-minimum or production matching/resource claim is made.

All 379 prior test files /371 C# blobs and production src tree
4224ea95d1cee7583799e639c4e3bb6be6c50fea remain identical. Frozen A05 source35/
corpus and all existing helper/assertion contracts stay fixed. The four new
frozen sources above remain identical. This design/plan and the five shared
inventories, including the refreshed gate ledger, precede final source upload.

Local A05 identity, CI classifier, main-job prerequisite (eight required jobs/
thirteen rejection controls) and whitespace gates pass. No local SDK is
installed. Full builds/tests/frozen/parity/API/package and script evidence are
required from six-host native CI; final plan completion and cleanup remain
external on PR282 without source/docs/test edits after GREEN.
