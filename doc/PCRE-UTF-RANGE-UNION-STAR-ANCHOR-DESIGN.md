# UTF range-union star anchoring

Continue merged272 at a90946818ae9831c2ede700f5ff1db9c6cc34924,
tree 5e0e37541b9de12ac82dca3932d277ecb6685bd0; baseline 100,340 tests.
Execute inline: no subagents, local .NET SDK installation or routine approval
pauses. Updating the development branch and PR is part of this work; the user
owns merge, tags and package publication. Keep #7/#102 open and Grep last.
Preserve all 331 prior test files/326 C# sources and frozen35. Leave issue7's
footer untouched under its existing publication block.

## Native behavior

Pinned PCRE2 10.48 at 7978954dbd2efc6f2196869290553cf1871b4ce6, with
SLJIT de0259c7aaf36aa40cba8014f3fad3edde9307f9, sorts and merges retained
ranges in pcre2_compile_class.c before XCLASS_HIGH_ANY/full-bitmap lowering.
An ordinary positive class whose ranges cover the complete 0..0x10FFFF
interval lowers to ALLANY. Both ascending and reversed
`[\x00-\xFF\x{100}-\x{10FFFF}]*?B` forms miss on tolerant UTF C3A942 from
offset1, while disabling dot-star anchoring matches2:1. Overlapping ranges,
three/four-piece unions and literal bridges have the same provenance.

A union excluding only D800..DFFF still retains different native lowering:
`[\x00-\x{D7FF}\x{E000}-\x{10FFFF}]*?B` matches2:1 at that offset with
either anchoring policy. Semantic coverage of valid scalars is insufficient.
The proof requires complete integer-interval coverage, including that gap.
Negative outer classes, holes at zero/max/interior and algebra are excluded.

The independently built native oracle reproduces all 455 merged272 groups /
55,384 protocols, including signed reset spans, before generating new cases.
All JIT_COMPLETE calls return0 and matching profiles have positive JIT sizes.

## Bounded proof and compatibility boundaries

Modify only PcreByteProgram.cs. Keep direct Any, single-full-range and the
existing full-range-member scan before the new proof, with their exact
predicate/member charges and fallback. Add a third compile-local counter,
MaximumInstructions=4096, shared across leading paths and all coverage passes.
It is independent of the predicate and explicit-member counters and is not
a public admission or runtime-work limit.

For a positive ordinary Source:Class with more than one retained range,
start coveredEnd=-1. Before each pass, admit the entire range list only if its
count fits the remaining coverage budget. Check cancellation and charge each
visited member, extending coveredEnd when Start<=coveredEnd+1. Scan every
admitted member even after coverage reaches0x10FFFF. Return true only after
the complete pass establishes that endpoint. Stop after a complete pass makes
no progress; if another whole pass will not fit, decline the new proof. This
handles arbitrary input order without sorting, allocation or changed nodes.

An explicit0..0x10FFFF member remains owned by the established member proof.
If the new scan sees one, decline after the complete pass, even if coverage
was otherwise established. This prevents the new budget from rescuing an
exhausted old member proof and preserves every frozen272 boundary/work case.
Old successful member/constant paths return before coverage is attempted.

Compute coverage as an additional result without bypassing established
Any/predicate prechecks or visits. Even a successful coverage proof must keep
the original predicate charges when positive Any already established the old
proof. An exhausted predicate budget may still use independent range coverage;
a later predicate-only class retains its original exhausted-budget fallback.
Every unsuccessful or declined coverage proof keeps the old result. Neither
old counter is charged by these additional passes. Preserve source/outer-negation/exact endpoint guards,
casing closure, entry/global-AST bounds, capture31/32 maps, cuts/calls/targets,
NoDot/COMMIT/start/possessification and all validation/cancellation priorities.
No parser, AST, VM, API, dependencies, workflows, syntax admission, instructions,
runtime allocation/state/work charge or public limit changes.

## Native-first fixtures and expected RED

530 matching groups /29,225 protocols,15 diagnostics and76 supplementary
cases add621 tests, reaching100,961. The new fixtures vary complete/reordered/
overlapping/bridged unions, properties, casing/UCP/scopes, stars/captures/
references/cuts/targets/policies/marks and owned sources. Exclude5,225 strict
research rows before native matching:2,926 malformed-whole-subject rows and
2,299 split offsets. They are not matching expectations.

Native dynamic preflight:40 groups/120 literal checks, all JIT_COMPLETE0 and
positive JIT sizes. Exercise64 reversed ranges/4096 visits versus65/4225;
4096 ascending members versus4097; shared4096/4097 totals; independence from
exhausted member/predicate budgets; preservation of old paths after coverage
exhaustion; complete scans after early coverage; oversized-list Any fallback
without partial charge; and termination after a no-progress pass. Beyond the
new proof bound, assert valid-subject admission and work fallback without
claiming malformed-UTF inference parity. Include exact observed/unobserved
work limits, cancellation, diagnostics, instruction4090/4091, capture/depth256
and erased/global-AST fallback.

Expected extended tests-only RED:199 failures /100,762 passes /100,961 total, zero skips.
176 matching identities require the new proof, including three COMMIT entry
cases that do not differ under the native disabled-anchor comparison. The
initial196 prediction omitted these three, without changing any fixture.
Add23 supplementary failures:8 work,2 ownership,
2 coverage boundaries,1 shared boundary,1 member-budget independence-of-passes,
1 exhausted-member independence,1 exhausted-predicate independence,5 old-path
fallback cases,1 oversized-list fallback and1 no-progress budget boundary.
All100,340 prior tests and422 new controls must pass. Check exact identities
and freeze all six new C# Git blobs before production; do not edit prior tests.

## Observed RED and pre-production freeze

Tests-only head2e9828fb4f0d530cf8fa7af7c4bbb5a5b511652e, tree
81d5e229c5ba29477bfaab38f307b2f0f363faf7, proposed merge
c59e47e0b18884b5277c656f390027e89750e1b2. Run44132407409, Linux x64
job134212743286:199 failures/100,758 passes/100,957 total, zero skips and
zero compiler/analyzer warnings/errors. Every failure is in the new reference
class; none has a native-assertion stack frame. All23 supplementary identities
match the prediction. All100,340 prior tests and418 new controls pass.

The three additional matching IDs101/125/138 are lazy-star COMMIT suffixes.
Native prevents retry from a split UTF offset even when NoDotStarAnchor is
enabled. The existing bounded dot-star entry proof also preserves the
zero-consumption COMMIT attempt at the requested byte; the new range-union
proof must feed that same existing metadata. Six explicit native compilations
/12 literal checks confirm the split miss and aligned2:1 match under both
policies. This changes the predicted failure inventory, not test expectations
or production scope. The full matching failure IDs are:
7,8,19,20,34,35,46,47,61,62,73,74,83,85,87,89,91,93,95,101,107,117,118,123,124,125,129,130,138,145,146,152,153,154,155,160,161,167,169,171,173,175,177,179,181,183,185,187,189,191,193,195,197,199,201,203,205,207,209,211,213,215,217,219,221,223,225,227,229,231,233,235,237,239,241,243,245,247,249,251,253,255,257,259,261,263,265,267,269,271,273,275,277,279,281,283,285,287,289,291,293,295,297,299,301,303,305,307,309,311,312,314,316,318,320,322,324,326,328,330,332,334,336,338,340,342,344,346,348,350,353,355,357,359,365,367,413,415,417,419,1002,1004,1006,1010,1012,1014,1018,1020,1022,1026,1028,1030,1034,1036,1038,1042,1044,1046,1050,1052,1054,1058,1060,1062,1114,1116,1118,1122,1124,1126,1130,1132,1134,1138,1140,1142.

Freeze these five Git blobs before production; paths are under tests/Pcre/:

| File | Git blob |
| --- | --- |
| PcreUtfRangeUnionStarAnchorProtocols1.cs | 5e28825ea3c9258553cf5bcbe5ab87023ea41152 |
| PcreUtfRangeUnionStarAnchorProtocols2.cs | 70dd7bc48f2800159961e309a9325dc737bfbf04 |
| PcreUtfRangeUnionStarAnchorProtocols3.cs | d314e0e7b00d3e9b7bb0b87b696e017ef9379af3 |
| PcreUtfRangeUnionStarAnchorProtocols4.cs | 31c34e9c1206a2ada275582d0eb4e64f0cf1bd75 |
| PcreUtfRangeUnionStarAnchorReferenceTests.cs | d327bb9f994c31f012d39147da9dc72a6a04bd9a |

All331 prior test files/326 C# sources and frozen35 match the merged base.
No fixture correction or test-source edit follows RED. Production remains
unchanged at this checkpoint; compilation/execution use native CI hosts.

## Pre-upload review: preserve successful old predicate charges

Before uploading production, review identified a required ordering detail:
returning immediately after successful coverage would bypass the old
positive-Any predicate scan and could incorrectly free its shared budget for
a later predicate-only class. Compute the coverage flag first, then retain
the old predicate precheck and complete scan, and combine their final results.
All original fast paths still return before the new proof as before.

Add four controls in a separate sixth C# file without modifying the five
frozen files. An initial full range union with4093 versus4094 predicates,
followed by a three-predicate Any class, must preserve the old4096/4097
boundary. Native preflight adds4 groups/12 literal checks, JIT0. These are
expected-green controls on unchanged production; the extended RED must keep
all199 existing managed failure identities and pass100,762 of100,961 tests.
The unuploaded production sketch was saved and removed before this second
tests-only checkpoint. Freeze the sixth blob after that result.

## Extended RED and sixth-file freeze

Extended tests-only head c0b452cdf81ddfbe39b0e5309812452d5b2c3e17, tree
1bcc97374327d4b863536ac9a91f3e83b5472bf7, run44134667566/Linux x64
job134221089886: exactly199 failures/100,762 passes/100,961 total, zero skips
and zero compiler/analyzer warnings/errors. Failure identities are identical
to the first RED. All four predicate-charge controls pass, every100,340 prior
test and422 new controls pass, and all five previously frozen blobs match.
No test-source correction was needed.

Freeze before production: tests/Pcre/PcreUtfRangeUnionStarAnchorPredicateBudgetTests.cs
is Git blob605613a48c88816232a06cfbac6f8611067b84c4. All six new C# files,
all331 prior test files/326 C# sources and frozen35 are now frozen together.
Production remains unchanged at this second checkpoint.

## Implementation and whole-branch author review

Only PcreByteProgram.cs changes production. A third local4096 counter is
passed by reference into IsUtfAllAny. Direct Any, exact single-full-range and
admitted explicit-full-member paths keep their early returns and old charges.
HasUtfFullRangeUnion rejects empty lists, prechecks complete passes, checks
cancellation/charges each member, tracks a monotone covered endpoint, rejects
explicit full members before accepting coverage, and stops on no progress or
budget exhaustion. The caller restricts this proof to positive ordinary
classes with multiple ranges. Endpoints from canonical UTF parsing are bounded
to0..0x10FFFF, so coveredEnd+1 cannot overflow.

Coverage is held as a flag while the old predicate precheck/scan still runs.
An exhausted old predicate precheck can return that independent flag; an
admitted positive predicate scan returns hasPositive||hasFullRangeUnion after
retaining every old visit. Negative paths and constant singleton/pair paths
keep their existing results. This resolves the pre-upload charge review
finding and passes the same source-level checks before qualification.

Reviewed source/polarity/hole/surrogate/case boundaries; arbitrary ordering,
overlap, bridges, complete scans/early coverage/no progress; single/shared and
independent4096/4097 budgets; old-member refusal and every fallback; capture
31/32 maps, global cuts/calls/targets and entry/AST limits; NoDot/COMMIT/start/
possessification; exact work/limits, ownership/captures/marks, diagnostics,
validation/cancellation and instruction/capture/depth capacities. No unresolved
introduced review finding remains. All six new files and all prior tests are
unchanged. Parser/AST/emitter/matcher/VM/API/dependencies/workflows remain fixed.

All seven inventories and frozen-source/corpus/A05/classifier/main-job/
whitespace checks precede final source qualification. Final source publication,
GREEN, identities and ready readback complete the plan externally on PR273;
no source/docs edits follow final GREEN. #7/#102 stay open, issue7's footer
stays unchanged, and the user owns merge/tags/package publication.

## Review, qualification and delivery

Review the whole branch inline against the range/source/polarity/hole/case
boundaries, monotone progress/complete-pass accounting, independent/shared
budgets, old-member refusal, every old fallback and global entry guard. Keep
the existing frozen-source/corpus/A05/classifier/main-job/whitespace gates.
Update all seven inventories before final source qualification: this design,
the plan and the five shared PCRE inventories.

Build net8/net9/net10; execute net10 only on six native OS/architecture hosts.
Require the full100,961 suite, frozen214 outcomes/120 methods, zero failures/
skips/compiler/analyzer warnings/errors and all configured support gates.
Publish exact reviewed source with a branch-head lease. Record final GREEN,
source/base/tree/proposed-merge/frozen identities and ready readback externally
on the PR; do not make source/docs edits after final GREEN. Delete only checked
task scratch after preserving the evidence needed to reproduce this work.
