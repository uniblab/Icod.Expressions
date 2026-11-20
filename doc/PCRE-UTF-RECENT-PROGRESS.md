# Recent UTF proof progress

## PR297 — interpreted named SKIP and returned call history

The [returned-history record](PCRE-RETURNED-SKIP-STATUS.md) makes interpreted
root named-SKIP lookup see MARK history retained by returned subroutine calls.
The native corpus contains 2,048 outcomes across 32 definitions. Two resource
checks bring the suite to 106,844 tests. A fourteenth package record exercises
the same policies. Broader conformance and roadmap gates remain open.

## PR296 — compiled-anchor reference routing

The [compiled-anchor record](PCRE-COMPILED-ANCHOR-STATUS.md) separates native
compile defaults from call-time anchor flags. The C probe and shared native
test adapter apply this routing. The corpus adds 3,072 native outcomes and
48 tests, bringing the suite to 106,810 cases. A thirteenth package record
checks the same compiled-default and call-policy combinations.

## PR295 — interpreted THEN assertion catches

The [THEN scope record](PCRE-INTERPRETED-THEN-STATUS.md) covers failed THEN
inside a called positive assertion. Interpreted matching catches the assertion
and can retry an earlier callee choice. The corpus contains 3,072 native
outcomes and 52 new tests, bringing the suite to 106,762 cases. The twelfth
package record checks the same definitions.

## PR294 — interpreted callee cut capture rollback

The [capture rollback record](PCRE-INTERPRETED-CUT-STATUS.md) covers selected
return captures when a called group fails after a control verb. Match-time
anchor dispatch restores the caller values through the existing journal.
The corpus contains 3,072 native outcomes and 52 new tests, bringing the
suite to 106,710 cases. The eleventh package record uses the same corpus.

## PR293 — compiled internal consumer contract

The [internal contract](PCRE-INTERNAL-CONTRACT.md) records eleven consumer
boundary types, option defaults, diagnostic values, and current acceptance
gaps. PR and main CI compare the compiled declaration snapshot alongside the
public extraction API. Existing runtime and package cases provide behavioral
evidence. Broader P09 conformance, hostile-input, and performance closure remains open.

## PR292 — interpreted word boundaries at malformed UTF starts

The [interpreted boundary addendum](PCRE-INTERPRETED-BOUNDARY-STATUS.md)
records 3,072 native outcomes and 52 tests. Match-time anchoring allows word
boundary assertions to evaluate the empty initial malformed UTF fragment.
The tenth package smoke record runs the same corpus against the packed library.

## PR291: interpreted optional lookbehind at UTF barriers

The [optional rewind addendum](PCRE-OPTIONAL-REWIND-STATUS.md) records3,072
native outcomes/52 additions (106,606 total). Call-anchor interpreter dispatch
permits shorter available variable-lookbehind context once minimum is met;
default/JIT barrier behavior and all existing retry/state/work policies stay
fixed. See [design](PCRE-OPTIONAL-REWIND-DESIGN.md), [plan](PCRE-OPTIONAL-REWIND-PLAN.md)
and [PR291](https://github.com/uniblab/Icod.Expressions/pull/291). RED/reconciliation/
freeze precede implementation; ninth package record and six native/support gates
precede ready. Final evidence/completion remains external without source/docs/
test edits after GREEN. #7/#102/footer fixed; broader gates open; Grep last.

## PR290: multiline caret at interpreted UTF fragment ends

The [fragment caret addendum](PCRE-FRAGMENT-CARET-STATUS.md) records native48x64=
3,072 outcomes and52 tests (106,554 total). Per-call anchor interpreter dispatch
rejects ordinary multiline caret at a nonterminal fragment end; default/JIT and
alternate-caret retain their behavior. Preserve physical/scoped ends, state,
original bytes and shared work/cancellation. See [design](PCRE-FRAGMENT-CARET-DESIGN.md),
[plan](PCRE-FRAGMENT-CARET-PLAN.md) and [PR290](https://github.com/uniblab/Icod.Expressions/pull/290).
Semantic RED/freeze precede implementation; eighth package record and all six
native/support gates precede ready state. Final evidence/completion stay external
without source/docs/test edits after GREEN. Broader gates open; #7/#102/footer
fixed; Grep last; user owns merge/tags/publication.

## PR289: COMMIT fragment restarts and physical end anchors

The [COMMIT fragment addendum](PCRE-COMMIT-FRAGMENT-STATUS.md) records this
bounded P09 hostile-input increment across all five shared inventories. A reached
root COMMIT abandons the current interpreted tolerant fragment, allowing later
valid fragments; physical z/Z remains distinct from call EndAnchored acceptance.
Existing default/scoped cuts, state, work and byte coordinates are preserved.

Pinned native52x64=3,328 outcomes /56 additions gives106,502 tests. Semantic
tests-only RED32/106,470 passes follows all native/default comparisons;24 new
controls and all106,446 old tests pass. New catalog/assertion/XML sources freeze
before the one-file matcher correction. A seventh package record reuses the
unchanged consumer and all six earlier records. See [design](PCRE-COMMIT-FRAGMENT-DESIGN.md),
[plan](PCRE-COMMIT-FRAGMENT-PLAN.md), addendum and
[PR289](https://github.com/uniblab/Icod.Expressions/pull/289). Whole-branch/baseline
review precedes final qualification; final identities/raw metrics/results/plan
completion/cleanup/ready remain external without source/docs/test edits after
GREEN. #7/#102/footer fixed; broader P09/native16/P10-P18/P00/P01/Stage C and
actual migrations stay open. Grep last; user merges/tags/publishes.

## PR288: immutable per-call match policy

The [per-call policy addendum](PCRE-MATCH-POLICY-STATUS.md) records this P09
increment across all five shared inventories. One compiled byte pattern accepts
six additive per-call restrictions without program cloning or shared mutation.
Preserve absolute anchors, compiled/source defaults, original bytes, validation,
work and cancellation; defer the same bounded optional-query pruning to effective
empty policy. Native56x64=3,584 outcomes /67 additions gives106,446 tests.

Refined tests-only RED67/106,379 passes follows complete native/default checks;
new catalog/consumer/assertion/C/XML/value sources freeze before implementation.
The sixth package record requires56 definitions/3,584 protocols, preserving all
five earlier records. Whole-branch review and all final six-host/full/frozen/
all-target/package/support gates remain required. See [design](PCRE-MATCH-POLICY-DESIGN.md),
[plan](PCRE-MATCH-POLICY-PLAN.md), addendum and
[PR288](https://github.com/uniblab/Icod.Expressions/pull/288). Final identities/raw
metrics/results/completion/cleanup/ready stay external without source/docs/test
edits after GREEN. #7/#102/footer fixed; broader P09/native16/P10-P18/P00/P01/
Stage C and actual migrations remain open. Grep last; user merges/tags/publishes.

## PR287: named captures on bound match results

The [named result inventory addendum](PCRE-NAMED-CAPTURE-RESULT-STATUS.md) records
this P09 increment for all five shared inventories. Successful results bind the
compiled metadata and expose first-participating native named-substring spans.
Empty/unset/absent states, ordinal lookup and original bytes remain explicit;
parser reference ordering and all matching/work behavior stay fixed.

Independent native preflight976 outcomes /985 new tests gives106,379 total.
Corrected tests-only RED is707 /105,672 passes /zero skips after native/existing
span checks; new source blobs freeze before binding. Require final six-host
full/frozen/five-package/build/support gates. See [design](PCRE-NAMED-CAPTURE-RESULT-DESIGN.md),
[plan](PCRE-NAMED-CAPTURE-RESULT-PLAN.md), addendum and
[PR287](https://github.com/uniblab/Icod.Expressions/pull/287). Final identities/raw
metrics/results/completion/cleanup/ready stay external without source/docs/test
edits after GREEN. #7/#102/footer fixed; broader P09/native16/P10-P18/P01/Stage C
and actual migrations remain open. Grep last; user owns merge/tags/publication.

## PR286: immutable internal capture metadata

The [capture metadata inventory addendum](PCRE-CAPTURE-METADATA-STATUS.md) records
this P09 increment for all five shared inventories. Compiled patterns gain owned,
stable read-only capture counts and ordinal name-to-slot mappings; execution,
parser declaration order and existing diagnostics/work remain fixed.

Independent pinned native preflight has214 count/name/match protocols. All-six
successful-build RED isolates222 missing-accessor failures /105,172 passes /
zero skips /105,394 total, with all old/native match checks passing. Freeze new
assertion/data/consumer/C/XML sources before implementation. Whole-branch review
and final six-host/full/frozen/four-package/support gates remain required.

See [design](PCRE-CAPTURE-METADATA-DESIGN.md),
[plan](PCRE-CAPTURE-METADATA-PLAN.md) and
[PR286](https://github.com/uniblab/Icod.Expressions/pull/286). Final identities,
results/completion/cleanup/ready stay external without source/docs/test edits
after GREEN. #7/#102/footer fixed; broader P09/native16/P10-P18/P01/Stage C and
actual migrations remain open. Grep last; user owns merge/tags/publication.

## PR285: bounded anchored tolerant UTF fragment storage

Tolerant UTF validation currently records every valid fragment before matching.
This increment retains only the first real fragment ending at/after the requested
byte when caller or proven dot-star anchoring already prevents later ordinary
retry. Preserve real end-at-start context and existing synthetic nullable entries.
Unanchored fragment collection/search stays identical. Full subject validation,
every Step, strict diagnostics, cancellation, captures and forward SKIP remain.
No streaming rescan, transcoding, parser/proof/VM change or new public quota.

Independent pinned PCRE2/SLJIT preflight records 1,568 all-offset short protocols
(364 groups) plus 12 large-subject outcomes, all JIT zero/positive size. Six
paired allocation profiles exercise plain, observed and cancellable paths;
eight controls preserve whole-validation budgets/cancellation/strict late errors.
390 additions give 105,171 tests. Observed successful-build RED on all six hosts
is exactly 18 allocation-growth failures /105,153 passes /zero skips, with native/
old cases passing first. New data/assertion/consumer/C/XML sources were frozen
before the three anchored fragment-retention calls and private guard changed.

The isolated package gate adds six raw allocation profiles while preserving
42+1+2 and the earlier four-profile results, fresh feeds/caches and all workflow
call sites. At 1KiB/64KiB require at most 4KiB extra per call after warmup.
No claim extends to unanchored fragments, backtracking/capture storage, native
memory, controlled-host timing or the entire P09 release contract.

See [design](PCRE-ANCHORED-FRAGMENT-STORAGE-DESIGN.md),
[plan](PCRE-ANCHORED-FRAGMENT-STORAGE-PLAN.md) and
[PR285](https://github.com/uniblab/Icod.Expressions/pull/285). Review the whole
branch/seven inventories before final six-host 105,171/frozen214/120, all target
builds and actual package/compatibility/Gnulib/support/script gates, with zero
failures/skips/coded compiler warnings/errors. Final raw metrics/identities,
completion/cleanup/ready remain external on PR285; no source/docs/test edits
after GREEN. Prior C# tests/A05 and #7/#102/footer fixed; sole old-test-file
exception adds two Compile links. Wider P09, native16/P10-P18, remaining P01,
Stage C and actual migration remain open. Grep last; user merges/tags/publishes.

## PR284: paired-input allocation evidence

Bounded P09 evidence now compares four anchored raw/UTF/UCP paths at1KiB and
64KiB, with exact native captures independently established by eight pinned
PCRE2 C probes (JIT0/positive size). The package-only consumer prepares inputs/
patterns outside measurement, uses original padded byte views and validates
complete results during every callback. No matcher/producer change is needed.

A synchronous current-thread managed-allocation probe warms both sizes,
alternates eight measured calls per size and records raw totals. Allow at most
4KiB additional allocation per call. Whole-byte-copy and UTF16-decode fault
controls must exceed that allowance. Callback bounds, original-view identity,
argument guards, cancellation/errors and no-op allocation controls are explicit.
This is covered-profile growth evidence, not absolute zero allocation, native/
retained heap accounting, timing acceptance or proof for every pattern/mode.

21 additions give104,781 total. Successful-build RED isolates21 missing-helper
failures /104,760 passes /zero skips, with native assertions passing first.
Freeze profile/assertion/C/XML sources before implementing only the new helpers;
freeze completed helpers afterward. All376 old C# test blobs and the producer
stay fixed; the sole old-test-file exception adds three Compile source links.

The existing isolated package action retains its42+1+2 protocol checks and
requires a second structured result with four exact profile names, sizes,
iterations, nonnegative counters and passing growth bounds on all six hosts.
Existing workflow call sites, release behavior and package dependencies stay
fixed. Actual configured compatibility/Gnulib/support checks remain mandatory.

See [design](PCRE-ALLOCATION-EVIDENCE-DESIGN.md),
[plan](PCRE-ALLOCATION-EVIDENCE-PLAN.md) and
[PR284](https://github.com/uniblab/Icod.Expressions/pull/284). Review the whole
branch and all seven inventories before final six-host104,781/frozen214/120,
package/allocation and support qualification with zero failures/skips/coded
warnings/errors. Final identities, raw metrics, cleanup and completion stay
external on PR284; no source/docs/test edits after GREEN. Wider P09 contract/
hostile/performance gates, native16/P10-P18, remaining P01, Stage C and actual
consumer migration remain open. #7/#102/footer fixed; Grep last; user merges,
tags and publishes.

## PR283: internal PCRE package-only consumer gate

P09 now has a bounded package-consumer contract using the actual candidate
NuGet binary. The explicitly named Icod.Expressions.PackageSmoke friend keeps
PCRE internal; it is not a new public API. Existing GNU/POSIX sample assertions
remain intact. No matcher/compiler/Unicode source changes are introduced.

32 protocols reuse immutable native-first raw/UTF/UCP fixtures, including
binary/NUL input, supplementary values, captures, lookarounds, repeats and
marks. Ten managed diagnostic/resource cases, one observed-view control and
two cancellation controls give 45 additions (104,760 total). Padded memory
views and overwritten caller pattern buffers exercise authoritative view
coordinates and compiler ownership. The observed callback check is specific
path evidence, not a blanket allocation/performance/no-transcoding freeze.

Observed successful-build RED isolates exactly 45 missing-consumer failures /
104,715 passes /zero skips; every native literal assertion passes. Freeze the
sample case table, new assertions and test-project Compile links before
implementing only the exercise; freeze that completed helper afterward. All
375 prior C# test blobs and A05 source35/corpus stay fixed; the sole prior-test
file exception adds two source links to the existing test project XML.

One common action packs to a fresh unique feed, restores only that candidate
with an isolated cache, builds net8/net9/net10 and executes net10 on all six
native PR/main hosts. Require the structured42-protocol/one-view/two-cancellation
record with zero failures. Main's artifact verification and release workflow
remain unchanged. Metadata/sample/workflow paths require actual full
compatibility/Gnulib gates, not classifier skips.

See [design](PCRE-PACKAGE-CONSUMER-DESIGN.md),
[plan](PCRE-PACKAGE-CONSUMER-PLAN.md) and
[PR283](https://github.com/uniblab/Icod.Expressions/pull/283). Whole-branch review
and all seven inventories precede final six-host104,760/frozen214/120 methods,
package, compatibility and support qualification with zero failures/skips/
coded warnings/errors. Final identities/evidence, cleanup and completion stay
external on PR283; no source/docs/test edits after GREEN. P09 remains open for
the wider contract/hostile/performance freeze; native16/P10-P18, remaining P01,
Stage C and actual consumer migration remain open. Keep #7/#102/footer fixed;
Grep last; user owns merge/tags/publication.

## PR282: pinned Perl overlap and bounded reducer

P00/P01 gains a declared Perl v5.40.1 / Unicode 15.0.0 comparison scope and
an original development runner, with 340 literal overlap cases (221 matches,
119 no-matches). All 680 independently built pinned PCRE2-8/16 probes agree
with Perl's scalar captures, retaining each native start/capture coordinate.
No mismatching rows were discarded. The corpus covers ordered choice,
repetition, captures/references, assertions, recursion/conditions, Unicode
literals and search starts. It makes no general Perl/PCRE or UCD15/17 parity
claim; wider modes, error/mark/callout/partial/iteration families remain open.

CI compares the literals through native8/native16, the direct native16 bridge
and managed bytes. The pinned Perl interpreter remains an independent
reproducible development preflight; no new CI dependency/workflow is added.
All cases/runner are original, with source-release and actual binary provenance.

A test-only native-unit reducer owns its buffers, preserves the start offset,
uses one explicit oracle-evaluation budget and propagates cancellation/errors.
Deterministic chunk deletion reaches a one-unit local fixed point, rather
than promising a globally smallest pair. The callback decides mismatch class
and rejects invalid/unrelated candidates. Byte/char/ushort controls and a
native-backed coordinate-comparison fault supply 31 reducer cases.

371 additions bring the full suite to 104,715. Observed successful-build RED
isolates 31 missing-reducer failures / 104,684 passes / zero skips. Freeze all
three new case/assertion sources and the Perl runner before implementing only
the reducer, then freeze its completed helper. Preserve all 379 old test files /
371 C# sources, the production tree and A05 source35/corpus.

See [design](PCRE-PERL-OVERLAP-REDUCER-DESIGN.md),
[plan](PCRE-PERL-OVERLAP-REDUCER-PLAN.md) and
[PR282](https://github.com/uniblab/Icod.Expressions/pull/282). Whole-branch review
and all seven inventories precede final six-host qualification: all three
library targets, full net10 plus frozen214/120 methods, zero failures/skips/
coded warnings/errors and configured support/package/script gates. Final
identities/evidence, cleanup and completion stay externally on PR282 without
source/docs/test edits after GREEN. P01 remains partial; P09-P18, Stage C and
actual consumer migrations remain open. #7/#102 and issue #7 footer stay fixed;
Grep last; user owns merge/tags/publication.

## PR281: direct raw 16-bit native reference

A test-only adapter now calls the existing PCRE.NET 1.6.0 native
16-bit exports, with explicit raw/UTF options and effective-option checks.
This removes the public string API's forced-UTF restriction from the reference
oracle. It does not implement managed 16-bit execution or change production
source, public API, dependencies, packaging or workflows.

Independent pinned PCRE2 10.48/SLJIT probes provide 136 raw/UTF protocols:
54 matches, 62 no-matches, 14 compile errors and six match errors. Reuse all
150 frozen dual-width UTF cases. Include isolated surrogate code units,
starts inside surrogate pairs, wide literals/classes, UCP, references,
recursion/conditions, lookarounds, signed K ranges, names/marks and source UTF
controls. Native code-unit offsets remain native; no production transcoding.

The bounded C64 ABI copies owned capture/name/mark results before releasing
native programs and buffers; compile/match errors remain distinct from no-match.
ABI/ownership facts and six managed-input guards bring the addition to
294 tests (104,344 total). Successful-build tests-only RED isolates exactly
294 missing-adapter failures / 104,050 passes / zero skips. Freeze the two
case/assertion sources before implementing only the helper, then freeze it.

All 368 prior C# test sources, old C harness, A05 source35/corpus and production
tree stay fixed. The only prior test-file exception corrects the native README
include path to the generated pinned header and links the new bridge. All
310 old protocols and 136
new outcomes reproduce unchanged. See [design](PCRE-RAW16-NATIVE-ORACLE-DESIGN.md),
[plan](PCRE-RAW16-NATIVE-ORACLE-PLAN.md) and
[PR281](https://github.com/uniblab/Icod.Expressions/pull/281).

Whole-branch review and all seven inventories precede final six-host
qualification: net8/net9/net10 builds, full net10 plus frozen 214 cases /
120 methods, zero failures/skips/coded warnings/errors and all configured
support gates. Final source/merge identities, evidence, cleanup and completion
remain external on PR281; no source/docs/test edits after GREEN. Full P01,
Perl/reducer, P09-P18, Stage C normalization and real consumer migration remain
open. Keep #7/#102 and issue #7 footer unchanged; Grep last; user owns merge,
tags and publication.

## PR280: native dual-width fixture contract

P00/P01 now has one reusable native UTF-8/UTF-16 case catalog with explicit
pattern/subject encodings, native start offsets, both capture protocols,
normalized scalar captures, names, marks and compile errors. The existing
managed byte matcher is checked against its pinned byte results. There is
no managed 16-bit matcher, public API, dependency or workflow change.

An independent pinned PCRE2-8/16 C harness records 140 shared matching cases,
ten compile errors, eight width-specific malformed-subject errors and two raw
16-bit witnesses. All matching compiles require JIT zero/positive size.
PCRE.NET 1.6.0 remains the six-host CI oracle; its string API always adds UTF,
so raw 16-bit results are not misrepresented as string-API coverage.

The test-only coordinate adapter uses independent Rune decoding, not matcher
navigation. It rejects malformed/different scalar inputs, split byte/scalar
or surrogate boundaries, invalid endpoints/protocols and arithmetic overflow.
Original byte/UTF-16 offsets remain pinned separately; empty/unset and reversed
ranges stay distinct. Arrays and encoding conversion belong only to test
infrastructure; no production subject transcoding is introduced.

202 new cases (104,050 total) comprise 150 shared/diagnostic rows, 44 coordinate
controls and eight width-specific errors. Observed tests-only RED exactly isolates
194 missing-adapter failures / 103,856 passes; all native assertions, 103,848
prior tests and eight malformed-subject controls pass. Five fixture/assertion
sources are frozen before implementing only the helper; its final blob is frozen too.
Preserve all 367 prior test files / 362 C# sources and A05 source 35/corpus.

See [design](PCRE-DUAL-WIDTH-FIXTURE-CONTRACT-DESIGN.md),
[plan](PCRE-DUAL-WIDTH-FIXTURE-CONTRACT-PLAN.md) and
[PR280](https://github.com/uniblab/Icod.Expressions/pull/280). Whole-branch review
and all seven inventories precede six-host qualification: net8/net9/net10
builds, full net10 plus frozen 214 cases / 120 methods, zero failures/skips/coded
warnings/errors and all configured support gates. Final identities/evidence and
plan completion stay external without source/docs/test edits after GREEN.
Raw 16-bit CI, Perl/reducer and full P01 closure remain open; P09-P18, Stage C
normalization and real consumer migration are not closed by this contract.
Keep #7/#102 and issue #7 footer unchanged; Grep last; user merges/tags/publishes.


## PR279: finite ASCII union star anchoring

Case-sensitive positive ordinary UTF classes such as `[\D0-9]`, finite
negative classes plus containing positive classes, and disjoint negative
classes preserve native ALLANY star entry. Exact typed finite ASCII predicates,
low 0..127 literal ranges and a negative finite member establish byte coverage
plus high-character inclusion. Caseless, unsupported/Unicode/binary members,
negative Ascii, wider ranges and exact same-kind pairs retain earlier owners
or conservative fallback. No general semantic-universality inference is made.

An eighth independent compile-local 4,096 combined predicate/range counter
is shared across leading paths. Every old proof runs first; old successes
consume no new budget. Precheck entire lists, scan completely with cancellation
and per-item charges, and use existing static bitmaps plus constant-time
low-range ulong masks. No allocation per scan, sorting, parser/matcher/VM,
runtime work, API, dependency, workflow or public quota change. Prior proof
bodies and PR278 iterator-fragment recovery remain byte-for-byte unchanged.

426 groups / 22,214 native protocols, 15 diagnostics and 56 supplementary cases
add 497 tests (103,848 total). Tests-only RED exactly matches 149 matching plus
16 supplementary identities: 165 failures / 103,683 passes / zero skips, native
assertion frames or coded warnings/errors. All 103,351 prior tests and 332 new
controls pass; freeze all five new C# blobs before production. Preserve all
362 prior test files / 357 C# sources and A05 source 35/corpus identities.

See [design](PCRE-UTF-FINITE-ASCII-STAR-ANCHOR-DESIGN.md),
[plan](PCRE-UTF-FINITE-ASCII-STAR-ANCHOR-PLAN.md) and
[PR279](https://github.com/uniblab/Icod.Expressions/pull/279). Whole-branch
author review and all seven inventories precede final qualification. Build
net8/net9/net10; execute full net10 and frozen 214 cases / 120 methods on six
native hosts with all configured support/package/script gates. Final identities,
results and plan completion remain external on the PR, with no source/docs/test
edits after final GREEN. Keep #7/#102 open; Grep last; user owns merge, tags and
publication. Issue #7 footer unchanged; broader optimizer/release/consumer gates
remain open.


## PR278: UCP xdigit iterator fragment recovery

A bounded direct-root proof retains native start-study recovery for unbounded
UCP xdigit complement iterators before selected ASCII literal continuations.
NoDotStarAnchor does not disable this iterator behavior; NoStartOptimize does.
The owned program bool and one integer cutoff reuse the existing fragment
loop after whole-subject validation, preserving its requested zero-width
candidate. Matching does not consume malformed bytes or infer anchoring.

Require exact complementary UnicodeHexDigit predicates, low ASCII ranges,
combined 4,096 list visits and a bounded 4,096 retained-AST walk over the
proved head plus literal/capture/mark/repeat tails. Preserve erased-node
inspection and cancellation. Greedy marked/no-tail/optional, caseless K/S,
non-ASCII continuation, Turkish/surrogate and wider group/call/cut/assertion
shapes remain outside the proof. Initial raw native consumption research
remains on immutable tests-only source; aligned controls retain their spans.

428 groups / 22,936 native protocols, 15 diagnostics and 42 supplementary cases
add 485 tests (103,351 total). Refined RED is exactly 132 managed failures /
103,219 passes; all 116 matching and 16 supplementary identities match. All
102,866 prior tests and 353 new controls pass; native/compiler warning/error
counts are zero. Freeze five new C# blobs before production; preserve all
357 prior test files / 352 C# sources and A05 source 35. All established ALLANY
bodies/budgets, VM instruction handling and Step placement remain unchanged.

See [design](PCRE-UTF-XDIGIT-FRAGMENT-RECOVERY-DESIGN.md), [plan](PCRE-UTF-XDIGIT-FRAGMENT-RECOVERY-PLAN.md) and
[PR278](https://github.com/uniblab/Icod.Expressions/pull/278). Build
net8/net9/net10; execute net10 only on six native hosts with frozen 214 cases /
120 methods and all configured support/package/script gates. All seven
inventories precede final qualification; results, exact identities and final
plan completion stay external on the PR without source/docs/test edits after
GREEN. Keep #7/#102 open; Grep last; user owns merge/tags/publication. Issue
#7 footer unchanged. Broader optimizer/release/consumer readiness remains open.


## PR277: built-in class complement star anchoring

Positive ordinary UTF classes with exact complementary finite ASCII POSIX
classes, horizontal/vertical whitespace lists or UCP POSIX blank preserve
native ALLANY star entry. Member order, duplicates, additional union members,
casing and ASCII restrictions retain typed source provenance. General Unicode
property pairs and UCP xdigit raw entry remain outside this proof; their valid
aligned controls and immutable initial research are documented.

A seventh independent compile-local 4,096-predicate counter is shared across
leading paths. The new overload calls every established proof first and returns
old successes without new charges. Full-list precheck, exact metadata, explicit
positive/negative mask bits, complete scans after early pairs and cancellation
bound the new work. Literal ranges consume no new budget. Exhaustion declines
inference without changing admission or public limits. Existing production
bodies, runtime state and work charging, parser, VM, API and workflows stay fixed.

402 groups / 21,952 native protocols, 15 diagnostics and 58 supplementary cases
add 475 tests (102,866 total). Refined tests-only RED is exactly 142 managed
failures / 102,724 passes: 123 matching and 19 supplementary identities. All
102,391 old tests and 333 new controls pass; native assertions and compiler
warnings/errors are zero. Freeze all five new C# blobs before production;
preserve 352 prior test files / 347 C# sources and A05 source 35.

See [design](PCRE-UTF-CLASS-COMPLEMENT-STAR-ANCHOR-DESIGN.md), [plan](PCRE-UTF-CLASS-COMPLEMENT-STAR-ANCHOR-PLAN.md) and
[PR277](https://github.com/uniblab/Icod.Expressions/pull/277). Build
net8/net9/net10, execute net10 only on six native hosts with frozen 214 cases /
120 methods and all configured support gates. All seven inventories precede
final qualification; exact identities/results and final plan completion stay
external on the PR without source/docs/test edits after GREEN. #7/#102 stay
open; Grep last; user owns merge/tags/publication. Issue #7 footer unchanged.


## PR276: mixed binary/POSIX ASCII star classes

Positive ordinary UTF classes containing positive binary ASCII plus negative
POSIX ASCII retain native ALLANY star entry. Reordered/aliased/duplicate/cased
members and extra union predicates/literals preserve byte spans at malformed
fragments and split offsets. Opposite-polarity and binary-only complements
retain XCLASS behavior; membership equality alone does not prove lowering.

A sixth independent compile-local4096 predicate counter is shared across
leading paths. The new overload calls the unchanged five-/four-counter proofs
first, preserving every established result and charge. Only old failures
attempt a complete, prechecked, cancellation-aware scan for the exact mixed
pair. Scan past early markers; literal ranges are not charged by the new
proof. Already proven classes consume no new work; exhaustion remains fallback.
Runtime state/work, instructions, admission and public limits are unchanged.

324 matching groups/21,456 native protocols,15 diagnostics and93 supplementary
cases add432 tests (102,391 total). Tests-only RED exactly109 failures/102,282
passes; all101,959 prior tests and323 new controls pass. Five C# blobs are
frozen before production;347 prior test files/342 C# sources/frozen35 remain
unchanged. See [design](PCRE-UTF-MIXED-ASCII-STAR-ANCHOR-DESIGN.md),
[plan](PCRE-UTF-MIXED-ASCII-STAR-ANCHOR-PLAN.md) and
[PR276](https://github.com/uniblab/Icod.Expressions/pull/276).
Build net8/net9/net10; execute net10 only on six native hosts. All seven
inventories precede qualification; final evidence/identities remain external
on the PR without source/docs/test edits after GREEN. #7/#102 remain open;
Grep last; user owns merge/tags/package publication. Issue7 footer unchanged.


## PR275: POSIX ASCII unions in UTF star classes

Positive ordinary UTF classes preserve native ALLANY star entry for POSIX
ASCII complement pairs, positive ASCII plus128..0x10FFFF literal coverage,
and negated ASCII plus0..127 coverage. Exact POSIX Ascii nodes remain distinct
from binary ASCII properties, whose semantically equivalent unions can retain
XCLASS provenance. Mixed-source and case-closed one-sided cohorts remain
separate valid-aligned controls without raw split/malformed parity claims.

A fifth independent compile-local4096 counter bounds complete predicate and
range passes across leading paths. An overload invokes the unchanged four-
counter proof first and returns its success, preserving every existing path
and charge without consuming new work. Only old failures attempt the positive
Source:Class POSIX proof. Whole-list prechecks, per-visit cancellation, complete
scans past early pairs/coverage and no-progress/exhaustion fallback remain.
One-sided source-domain guards preserve older literal-union budget ownership.
Instructions, runtime state/work, admission and public limits are unchanged.

506 matching groups/30,390 native protocols,45 diagnostics and174 supplementary
cases add725 tests (101,959 total). Tests-only RED exactly173 failures/101,786
passes, with all101,234 old tests and552 new controls passing. Seven new C#
blobs are frozen before production;340 prior test files/335 C# sources and
frozen35 are unchanged. Native-first evidence, source guards and review gates
are in the [design](PCRE-UTF-POSIX-ASCII-STAR-ANCHOR-DESIGN.md),
[plan](PCRE-UTF-POSIX-ASCII-STAR-ANCHOR-PLAN.md) and
[PR275](https://github.com/uniblab/Icod.Expressions/pull/275).
Build net8/net9/net10, execute net10 only on six native hosts. All seven
inventories precede final qualification; final identities/evidence stay on the
PR without source/docs edits after GREEN. #7/#102 remain open; Grep last;
user owns merge/tags/package publication. Issue7 footer unchanged.


## PR274: ASCII plus high ranges in UTF star classes

Positive ordinary UTF classes with an exact positive ASCII predicate and
retained ranges covering128..0x10FFFF now preserve native ALLANY star entry.
Reordered/split/overlapping ranges and property aliases retain native byte
spans at tolerant split offsets. Negative outer/negative-only ASCII, gaps,
case-closed source cohorts and ranges below128 stay outside the new proof.

A fourth independent compile-local4096 budget counts every predicate and
complete range pass across leading paths. Precheck the complete first combined
list, then every whole range pass; scan past early ASCII/coverage, check
cancellation and stop on no progress or exhaustion. Lower ranges cannot
rescue an exhausted literal-union proof. Preserve every old constant/member/
union/predicate result and charge, including successful old Any scans.
Runtime state/work, instructions, admission and public limits are unchanged.

186 matching groups/11,090 native protocols,15 diagnostics and72 supplementary
cases add273 tests (101,234 total). Tests-only RED is exactly68 failures/
101,166 passes; all100,961 old tests and205 new controls pass. Three new C#
blobs are frozen before production;337 prior test files/332 C# sources and
frozen35 remain unchanged. See [design](PCRE-UTF-ASCII-RANGE-STAR-ANCHOR-DESIGN.md),
[plan](PCRE-UTF-ASCII-RANGE-STAR-ANCHOR-PLAN.md) and
[PR274](https://github.com/uniblab/Icod.Expressions/pull/274).
Build net8/net9/net10, execute net10 only on six native hosts. All seven
inventories precede final qualification; final evidence/identities remain on
the PR without source/docs edits after GREEN. #7/#102 remain open; Grep last;
user owns merge/tags/package publication. Issue7 footer unchanged. Deferred
[OffsetLimit research](PCRE-OFFSET-LIMIT-RESEARCH.md) adds no public API.


## PR273: complete range unions in UTF star classes

Ordinary positive UTF classes whose retained ranges jointly cover the complete
0..0x10FFFF interval qualify for native ALLANY star anchoring. Reordered,
overlapping and literal-bridged unions use bounded complete-list frontier
passes, without sorting or allocation. Holes, including a surrogate-only gap,
negative outer classes and general algebra remain outside this proof.

A new compile-local 4096-visit coverage budget is shared across leading paths
and passes, independently of the existing predicate/member counters. Precheck
each whole list, check cancellation and charge every visit, scan past early
coverage, and stop on no progress or insufficient budget. Explicit full-range
members remain owned by the old member proof. Retain the existing predicate
precheck and complete scan even when coverage independently succeeds, so no
successful old Any proof loses its original charges. Declined coverage retains
the old result; instructions, runtime work and public limits are unchanged.

530 matching groups/29,225 native protocols,15 diagnostics and76 supplementary
cases add621 tests (100,961 total). Preserve331 prior test files/326 C# sources
and frozen35. Native-first RED/frozen-source/author-review evidence is in the
[design](PCRE-UTF-RANGE-UNION-STAR-ANCHOR-DESIGN.md),
[plan](PCRE-UTF-RANGE-UNION-STAR-ANCHOR-PLAN.md) and
[PR273](https://github.com/uniblab/Icod.Expressions/pull/273).
Build net8/net9/net10; execute net10 only on six native hosts. All seven
inventories precede final qualification; final evidence and identities remain
on the PR without source/docs edits after GREEN. #7/#102 stay open; Grep last;
user owns merge/tags/package publication. Issue7's footer remains untouched.


## PR272: explicit full-range members in UTF star classes

An ordinary positive UTF class with multiple retained ranges now preserves
native ALLANY star anchoring when an admitted member is exactly 0..0x10FFFF.
For example, `[\x00-\x{10FFFF}x]*?B` preserves the native miss on tolerant
UTF `éB` from byte offset1. The uncharged single-range path remains first.
The new complete-list scan checks cancellation and charges every admitted
member against an independent 4096-range budget shared across leading paths.
Oversized lists are prechecked without partial charge; declined range proofs
fall through to the existing Any/predicate rules and their original charges.
Negative outer classes, near-full ranges and general coverage/algebra retain
their existing rules. Instructions, runtime work, admission and public limits
are unchanged.

Native 455 matching groups/55,384 protocols, 15 diagnostics and 55 supplementary
cases add 525 tests (100,340 total). Tests-only RED is exactly 152 failures/
100,188 passes, with all 99,815 prior tests and 373 new controls passing;
all four new C# blobs are frozen before production. Preserve 327 prior test
files/322 C# sources/frozen35. Build net8/net9/net10; execute net10 only on six
native hosts. Full qualification and source identities remain external on
[PR272](https://github.com/uniblab/Icod.Expressions/pull/272).
[Design](PCRE-UTF-FULLRANGE-MEMBER-STAR-ANCHOR-DESIGN.md) /
[plan](PCRE-UTF-FULLRANGE-MEMBER-STAR-ANCHOR-PLAN.md).
#7/#102 remain open; Grep last; user owns merge/tag/publication. Issue7's
footer remains untouched under its existing publication block.


Continues [issue102](https://github.com/uniblab/Icod.Expressions/issues/102).
Its description reached GitHub's body limit. Earlier history remains there;
this index preserves the recent links and gives subsequent tranches room to grow.

The counts describe each tranche's committed test corpus. Open the linked PR
for its current review status, native checks, exact source commit and final CI
audit. A row is not a claim that an unreviewed PR is merged or release-ready.

| PR | New cases | Full suite | Scope |
| --- | ---: | ---: | --- |
| [147](https://github.com/uniblab/Icod.Expressions/pull/147) | 950 | 17256 | Finite and empty nullable-group continuation |
| [148](https://github.com/uniblab/Icod.Expressions/pull/148) | 913 | 18169 | Nullable alternative-group continuation |
| [149](https://github.com/uniblab/Icod.Expressions/pull/149) | 1203 | 19372 | Finite group-count study |
| [150](https://github.com/uniblab/Icod.Expressions/pull/150) | 1117 | 20489 | Guarded positive group counts |
| [151](https://github.com/uniblab/Icod.Expressions/pull/151) | 1443 | 21932 | Unbounded nullable group loops |
| [152](https://github.com/uniblab/Icod.Expressions/pull/152) | 1414 | 23346 | Word-boundary PLUS proofs and first required-copy ownership |
| [153](https://github.com/uniblab/Icod.Expressions/pull/153) | 179 | 23525 | Boundaries before general starter classes |
| [154](https://github.com/uniblab/Icod.Expressions/pull/154) | 199 | 23724 | Erased primitives before general starter classes |
| [155](https://github.com/uniblab/Icod.Expressions/pull/155) | 223 | 23947 | Retained zero groups/assertions before general starter classes |
| [156](https://github.com/uniblab/Icod.Expressions/pull/156) | 377 | 24324 | Finite optional primitives and proven root stars before general starter classes |
| [157](https://github.com/uniblab/Icod.Expressions/pull/157) | 297 | 24621 | Bitmap-skipped negative lookahead and lookbehind prefixes |
| [158](https://github.com/uniblab/Icod.Expressions/pull/158) | 752 | 25373 | Exact assertion copies and optional negative-ahead class bodies |
| [161](https://github.com/uniblab/Icod.Expressions/pull/161) | 1092 | 26465 | Mandatory bitmap heads in optional negative lookahead and finite scoped groups |
| [162](https://github.com/uniblab/Icod.Expressions/pull/162) | 817 | 27282 | Mandatory heads across optional group alternatives and nested bodies |
| [163](https://github.com/uniblab/Icod.Expressions/pull/163) | 987 | 28269 | Mandatory heads in finite optional numbered and named captures |
| [164](https://github.com/uniblab/Icod.Expressions/pull/164) | 1364 | 29633 | Atomic/skipped bracket prefixes and equivalent negative-count exits |
| [165](https://github.com/uniblab/Icod.Expressions/pull/165) | 432 | 30065 | Positive ahead through skipped assertion and boundary prefixes |
| [166](https://github.com/uniblab/Icod.Expressions/pull/166) | 495 | 30560 | Positive ahead before mandatory CR-excluding type heads |
| [167](https://github.com/uniblab/Icod.Expressions/pull/167) | 35 | 30595 | Positive ahead before mandatory CR-excluding literal repeats |
| [168](https://github.com/uniblab/Icod.Expressions/pull/168) | 36 | 30631 | Positive ahead before mandatory CR-excluding direct classes |
| [169](https://github.com/uniblab/Icod.Expressions/pull/169) | 42 | 30673 | Positive ahead before mandatory CR-excluding class repeats |
| [170](https://github.com/uniblab/Icod.Expressions/pull/170) | 72 | 30745 | Positive ahead before mandatory CR-excluding type repeats |
| [171](https://github.com/uniblab/Icod.Expressions/pull/171) | 30 | 30775 | Positive ahead before one-level mandatory groups with direct heads |
| [172](https://github.com/uniblab/Icod.Expressions/pull/172) | 36 | 30811 | Positive ahead before one-level mandatory groups with repeat heads |
| [173](https://github.com/uniblab/Icod.Expressions/pull/173) | 42 | 30853 | Positive ahead before one-level mandatory groups with boundary/lookahead prefixes |
| [174](https://github.com/uniblab/Icod.Expressions/pull/174) | 49 | 30902 | Positive ahead before one-level mandatory groups with lookbehind prefixes |
| [175](https://github.com/uniblab/Icod.Expressions/pull/175) | 36 | 30938 | Positive ahead before one nested mandatory group head |
| [176](https://github.com/uniblab/Icod.Expressions/pull/176) | 27 | 30965 | Positive ahead through bounded deep mandatory groups |
| [177](https://github.com/uniblab/Icod.Expressions/pull/177) | 18 | 30983 | Positive ahead before mandatory repeated group heads |
| [178](https://github.com/uniblab/Icod.Expressions/pull/178) | 15 | 30998 | Literal positive ahead before zero-minimum group repeats |
| [179](https://github.com/uniblab/Icod.Expressions/pull/179) | 15 | 31013 | Word boundary between literal positive ahead and zero-minimum group repeat |
| [180](https://github.com/uniblab/Icod.Expressions/pull/180) | 30 | 31043 | Two boundaries between literal positive ahead and zero-minimum group repeat |
| [181](https://github.com/uniblab/Icod.Expressions/pull/181) | 30 | 31073 | Negative literal assertion between positive ahead and zero-minimum group repeat |
| [182](https://github.com/uniblab/Icod.Expressions/pull/182) | 150 | 31223 | Two-node boundary and negative assertion prefixes before zero-minimum group repeat |
| [183](https://github.com/uniblab/Icod.Expressions/pull/183) | 180 | 31403 | Positive literal assertions after leading lookahead before zero-minimum group repeat |
| [184](https://github.com/uniblab/Icod.Expressions/pull/184) | 303 | 31706 | Bounded zero-width chains before zero-minimum group repeats |
| [185](https://github.com/uniblab/Icod.Expressions/pull/185) | 315 | 32021 | Wide scalar literal assertions before zero-minimum group repeats |
| [186](https://github.com/uniblab/Icod.Expressions/pull/186) | 600 | 32621 | Multi-scalar literal assertions before zero-minimum group repeats |
| [187](https://github.com/uniblab/Icod.Expressions/pull/187) | 270 | 32891 | Positive class lookahead before zero-minimum group repeats |
| [188](https://github.com/uniblab/Icod.Expressions/pull/188) | 210 | 33101 | Middle class lookaheads before zero-minimum group repeats |
| [189](https://github.com/uniblab/Icod.Expressions/pull/189) | 180 | 33281 | Middle class lookbehinds before zero-minimum group repeats |
| [190](https://github.com/uniblab/Icod.Expressions/pull/190) | 270 | 33551 | Leading class lookbehinds before zero-minimum group repeats |
| [191](https://github.com/uniblab/Icod.Expressions/pull/191) | 180 | 33731 | Leading literal lookbehinds before zero-minimum group repeats |
| [192](https://github.com/uniblab/Icod.Expressions/pull/192) | 180 | 33911 | Leading two-scalar lookbehinds before zero-minimum group repeats |
| [193](https://github.com/uniblab/Icod.Expressions/pull/193) | 270 | 34181 | Bounded leading literal lookbehinds before zero-minimum group repeats |
| [194](https://github.com/uniblab/Icod.Expressions/pull/194) | 390 | 34571 | CR-containing leading literal lookbehind guard and native capture controls |
| [195](https://github.com/uniblab/Icod.Expressions/pull/195) | 240 | 34811 | Single-scalar negative lookahead and lookbehind before zero-minimum group repeats |
| [196](https://github.com/uniblab/Icod.Expressions/pull/196) | 240 | 35051 | Bounded multi-scalar negative lookahead and lookbehind before zero-minimum group repeats |
| [197](https://github.com/uniblab/Icod.Expressions/pull/197) | 420 | 35471 | Caseless bounded negative lookahead and lookbehind before zero-minimum group repeats |
| [198](https://github.com/uniblab/Icod.Expressions/pull/198) | 300 | 35771 | Caseless positive lookahead and lookbehind before zero-minimum group repeats |
| [199](https://github.com/uniblab/Icod.Expressions/pull/199) | 540 | 36311 | Caseless positive class lookahead and lookbehind before zero-minimum group repeats |
| [200](https://github.com/uniblab/Icod.Expressions/pull/200) | 540 | 36851 | Caseless negative class lookahead and lookbehind before zero-minimum group repeats |
| [201](https://github.com/uniblab/Icod.Expressions/pull/201) | 1080 | 37931 | Negative class middle assertions before zero-minimum group repeats |
| [202](https://github.com/uniblab/Icod.Expressions/pull/202) | 1680 | 39611 | Already-supported CR/LF-excluding negated sets in negative assertions: capture reference coverage |
| [203](https://github.com/uniblab/Icod.Expressions/pull/203) | 888 | 40499 | Negated-range negative assertions: endpoint-implicit ranges and newline-admitting native controls |
| [204](https://github.com/uniblab/Icod.Expressions/pull/204) | 2560 | 43059 | Negative scalar-class assertions: newline membership and Unicode predicate unions |
| [205](https://github.com/uniblab/Icod.Expressions/pull/205) | 2720 | 45779 | Bounded regular negative assertion bodies: sequences, alternatives, groups and repeats |
| [272](https://github.com/uniblab/Icod.Expressions/pull/272) | 525 | 100340 | Explicit full-range members in UTF star classes |
| [273](https://github.com/uniblab/Icod.Expressions/pull/273) | 621 | 100961 | Complete range unions in UTF star classes |

Before a tranche is ready: six native OS/architecture Staging jobs, full suite
and frozen214 on net10 only, builds for net8/net9/net10, no failed/skipped tests or compiler warnings/errors,
all configured gates, seven equal component workloads, immutable tests and
frozen35 source checks. Full source-pinned Compatibility has its own configured
path gate; a skipped job is not a completed full qualification.

## Preserved issue102 footer

Copied verbatim before replacing only that footer with a link to this index:

```markdown


## Recent progress

[147](https://github.com/uniblab/Icod.Expressions/pull/147); [148](https://github.com/uniblab/Icod.Expressions/pull/148); [149](https://github.com/uniblab/Icod.Expressions/pull/149); [150](https://github.com/uniblab/Icod.Expressions/pull/150); [151](https://github.com/uniblab/Icod.Expressions/pull/151); [152](https://github.com/uniblab/Icod.Expressions/pull/152); [153](https://github.com/uniblab/Icod.Expressions/pull/153).

Six native jobs/net8/9/10; full/frozen214; zero test failures/skips/build warnings/errors. Audits/scopes/counts: PRs; #102 open.
```

Issue102 remains open for wider bitmap availability and unsupported placements;
unsafe UTF100 remains separate. No full PCRE, release or Grep readiness claim.
Publication and all known consumer migrations precede CommandFramework removal;
Grep remains last.
