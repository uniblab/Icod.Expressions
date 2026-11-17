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

Every unsuccessful, declined or exhausted coverage proof continues to the
established Any/predicate inference. Neither old counter is charged by these
additional passes. Preserve source/outer-negation/exact endpoint guards,
casing closure, entry/global-AST bounds, capture31/32 maps, cuts/calls/targets,
NoDot/COMMIT/start/possessification and all validation/cancellation priorities.
No parser, AST, VM, API, dependencies, workflows, syntax admission, instructions,
runtime allocation/state/work charge or public limit changes.

## Native-first fixtures and expected RED

530 matching groups /29,225 protocols,15 diagnostics and72 supplementary
cases add617 tests, reaching100,957. The new fixtures vary complete/reordered/
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

Expected tests-only RED:196 failures /100,761 passes /100,957 total, zero skips.
173 matching identities differ from native disabled-anchor behavior without an
old Any/caseless fast path. Add23 supplementary failures:8 work,2 ownership,
2 coverage boundaries,1 shared boundary,1 member-budget independence-of-passes,
1 exhausted-member independence,1 exhausted-predicate independence,5 old-path
fallback cases,1 oversized-list fallback and1 no-progress budget boundary.
All100,340 prior tests and421 new controls must pass. Check exact identities
and freeze all five new C# Git blobs before production; do not edit prior tests.

## Review, qualification and delivery

Review the whole branch inline against the range/source/polarity/hole/case
boundaries, monotone progress/complete-pass accounting, independent/shared
budgets, old-member refusal, every old fallback and global entry guard. Keep
the existing frozen-source/corpus/A05/classifier/main-job/whitespace gates.
Update all seven inventories before final source qualification: this design,
the plan and the five shared PCRE inventories.

Build net8/net9/net10; execute net10 only on six native OS/architecture hosts.
Require the full100,957 suite, frozen214 outcomes/120 methods, zero failures/
skips/compiler/analyzer warnings/errors and all configured support gates.
Publish exact reviewed source with a branch-head lease. Record final GREEN,
source/base/tree/proposed-merge/frozen identities and ready readback externally
on the PR; do not make source/docs edits after final GREEN. Delete only checked
task scratch after preserving the evidence needed to reproduce this work.
