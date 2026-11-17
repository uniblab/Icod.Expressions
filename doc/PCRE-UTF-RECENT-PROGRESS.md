# Recent UTF proof progress

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
