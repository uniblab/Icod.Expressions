# Caller Case Policies Implementation Plan

Execute inline under the standing continuous workflow.
**Goal:** Expose existing restricted/Turkish comparison policies as caller options.
**Architecture:** Initialize two existing prefix settings from immutable caller
options; retain the shared parser/compiler/comparison and validation paths.
**Tech stack:** C#13/net8/9/10, PCRE2 10.48, CI RED/GREEN.
**Spec:** [design](PCRE-CALLER-CASE-POLICIES-DESIGN.md).

## Global constraints

Merged241 e27a301cc8493ac457db8c2a5f2b95fd58e3f2e8/treecbdd3ee4735486e5db4bb8a5c885a058ed9dc524;
72,857 tests/framework,257 earlier files. Preserve GNU/BRE/ERE, frozen35/214
outcomes/120methods, AST/IR/opcodes/matcher/journals/shared limits/exported API,
dependencies/workflows. UTF8+LF; no Python/subagents/SDK installs. Reuse linked
worktree; user owns merge/tag/publication; Grep last. All repository docs precede
qualification; final evidence goes on PR242/issue7 only, with no docs-only CI run.

## Review focus

1. Caller and source policies combine before validation without enabling i/UTF or shifting offsets.
2. Restricted lexical reset/unset differs from global Turkish persistence and local r overrides.
3. Every comparison consumer and existing mode-specific optimizer proof receives the same policy.
4. Source/caller/global conflict priority precedes body parsing, including malformed/inert text.
5. Owned data, canonical work/cancellation and capture/depth/expansion ceilings stay shared.

## Task 1: regression contract

**Files:** Create tests/Pcre/PcreCallerCasePolicyReferenceTests.cs.
**Interfaces:** Optional reflection sets future CaselessRestricted/TurkishCasing
for executable RED. Native extras0x80/0x10000 pin independent results/errors.

- [x] Preflight matching/diagnostic profiles, full membership masks, selected
  offsets, ownership/work/capacity proofs; separate unsupported native JIT paths.
- [x] Add all four selections and five review boundaries. Publish tests-only
  draft PR242, inspect clean net8/9/10 RED with zero compiler warnings/errors,
  only missing-policy failures, and freeze the fixture before production.

## Task 2: settings integration

**Files:** Modify src/Pcre/PcreByteOptions.cs and PcreByteStartSettings.cs;
update seven repository documentation files.
**Interfaces:** bool CaselessRestricted/TurkishCasing init properties;
existing local caselessRestricted/turkishCasing initialize from options in Parse.

- [x] Add documented options and replace only the two false initializers; retain
  existing prefix OR, validation, parser initialization and comparison consumers.
- [ ] Include all inventories before final qualification. Verify all18 native
  results, configured supporting gates, prior257 hashes and frozen identities.

## Task 3: review and delivery

- [ ] Author review all five boundaries and whole branch inline; no agents.
- [ ] Verify clean tree and exact source/base/tree/proposed-merge identity.
- [ ] Record final GREEN/review on PR242 and only issue7's footer, mark ready,
  and remove only this plan's ledger. User owns merge/tag/publication.

## Inline progress before qualification

Tests-only [RED733](https://github.com/uniblab/Icod.Expressions/actions/runs/43432119944), source `c77092cbfb760f9c0e1452353d84bc68316773a5`, Linux ARM job131907925887: net8/9/10 each built without compiler warnings/errors and ran678 expected failures/76,852 passes/77,530total/zero skips. All failures were new missing-policy cases (338 spans,321 diagnostics,2 membership,12 offsets,2 work,2 ownership and1 quota); every earlier test and3,995 new controls passed. Fixture `94b6ffd9c834cbbc10ab03d97cf3c98b7e303c08` froze before production and remains unchanged.

Task1 and Task2 settings integration are complete. All seven repository documentation files precede qualification. The all18 GREEN gate and Task3 review/identities/footer/readiness are intentionally recorded on PR242/issue7 after observation, without another documentation-only source push.

## Task 2a: marked scalar-set search correction

**Files:** Create tests/Pcre/PcreMarkedScalarSetStartLineReferenceTests.cs;
modify src/Pcre/PcreByteProgram.cs only after its separate tests-only RED.
**Interfaces:** Existing HasLinearPathMarkStartBarrier handles ScalarSet like
Literal for consumed state, retaining all bounds/eligibility and AST caret proof.

- [ ] Pin native spans/captures/labels for caller/source/default marked sets and
  order/no-caret/byte/strict controls, boundaries, bounded capacity/work.
- [ ] Publish tests-only fixture on initial production; inspect net8/9/10 RED
  with the retained original one failure and only new supported-gap failures;
  freeze the new fixture before source correction.
- [ ] Extend only the linear compiler proof, include all final inventories,
  then qualify the complete suite/all18/supporting gates and final identities.
