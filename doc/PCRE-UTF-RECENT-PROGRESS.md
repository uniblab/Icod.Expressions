# Recent UTF proof progress

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
