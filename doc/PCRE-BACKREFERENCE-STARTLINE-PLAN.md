# Backreference Startline Implementation Plan

Execute inline under the standing continuous workflow.
**Goal:** Preserve native STARTLINE when a backreference blocks first-character search.
**Architecture:** Extend the existing pure-IR compiler proof to default and selected
references reachable before consumption. Reuse all candidate scanning and bounds.
**Tech Stack:** C#13/net8/9/10; managed RED/GREEN through GitHub CI.
**Spec:** [design](PCRE-BACKREFERENCE-STARTLINE-DESIGN.md).

## Global constraints

All247 earlier test files,35 frozen blobs/214 outcomes/120 methods, GNU/BRE/ERE,
API/dependencies remain fixed. UTF8+LF; no Python/subagents/SDK installs. Reuse
isolated worktree; user owns merge/tag/publication; Grep stays last. Keep existing
1,000-instruction/visit proof ceiling, heap/cancellation and first-caret root proof.

## Review focus

1. Default participating empty captures and selected unset slots share opacity.
2. Optional reference/literal prefixes and alternatives cannot infer a later first unit.
3. Mandatory leading literals/classes and unsupported operations retain search priority.
4. Split versus scalar-boundary offsets preserve fragment, caret and EOF rules.
5. Deep/large/capture-full programs retain heap, work, cancellation and quota behavior.

## Task 1: native reference, expected RED and freeze

Create tests/Pcre/PcreBackreferenceStartlineReferenceTests.cs. Primary literal
profiles cover39 patterns, both reference/casing policies and NEL/LS/PS split
versus boundary offsets. Assert native literals before real compiled matching;
compare every capture. Supplemental native arrays compile first. Add offset,
strict, source/options ownership, proof-size/depth/capture capacity and shared
work/limit controls. Tests catch overrestrictive Accept-only/selected-only proof,
overbroad admission across mandatory consumption, lost root anchors and bad bounds.

- [x] Preflight all native profiles and supplemental patterns with JIT0.
- [x] Publish tests-only draft; observe net8/9/10 expected RED without native,
  diagnostic or earlier-fixture failures. Freeze the final fixture hash.

## Task 2: compiler barrier proof and full qualification

Modify src/Pcre/PcreByteProgram.cs only: rename private helper/call to
HasBackreferenceStartBarrier(); require any Backreference instead of Second=1;
return true at Backreference as well as Accept along existing nonconsuming CFG.
Update the explanatory comment. Preserve eligibility/size/visits/cancellation,
consuming leaves, control-flow edges and existing AST root-line-anchor proof.

- [x] Implement only after valid RED/freeze; no matcher or earlier tests change.
- [ ] All18 native results pass full/frozen tests with0failures/skips/build warnings/errors.
- [ ] Verify frozen35, five Unicode generators, seven equal workloads, extraction/
  POSIX/API/corpus, packaging/three package-smokes and three-OS script gates.

## Task 3: review and delivery

- [ ] Author review of all five boundaries; verify hashes/clean tree and exact
  source/base/proposed-merge identity; independent review excluded by user preference.
- [ ] Update design/matrix/resource/consumer/status inventories and issue7 footer only.
- [ ] Mark ready after all gates, remove only this tranche ledger; user owns merge.

Baseline233 merged5a286d566e8f3714cd4fd6fe185b927c7d0e2812,
tree8fa3cf1e8921ce44d91aeb5dd14a3a7d65203872. Main43251995145 and
scripts43251995258 green;61,422 tests/framework. Primary936 native literal
profiles preflighted; add supplemental controls before final expected count/freeze.

Final test inventory:949 new cases (936 primary literal profiles,4 offset,2strict,
2large-prefix and5facts), expected62,371/framework. All936 primary and284
supplemental native profiles compile/JIT0. Native split-versus-boundary research
predicts285 primary false-positive differences; both default and selected modes
are affected. No earlier test edits or production changes before valid RED/freeze.

Valid RED702/run43255390418, Linuxx64 job131312086289 at1107f679f0a8da3b3ae069d0934e5c825a53c36c:
each net8/9/10 has295 expected failures/62,076 passes/62,371 total/0skips,
0warnings/errors. Primary285 =87default+198selected per framework exactly
matches native research. Earlier/strict/fallback/native checks pass. Freeze
f78f3d24ff62419bfefd9672a902259138fbbb7e before compiler changes.
