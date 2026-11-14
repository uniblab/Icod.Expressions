# First-Line Compilation Implementation Plan

> Execute inline with executing-plans under the standing no-agents/no-pauses workflow.

**Goal:** Limit unanchored candidate starts to the requested first-line window.
**Architecture:** Snapshot one program flag and gate candidate entry with a
bounded incremental newline frontier. Preserve full-input VM consumption.
**Tech Stack:** C#13, net8/net9/net10, xUnit and pinned PCRE2 10.48 oracle only.
**Spec:** doc/PCRE-FIRST-LINE-COMPILE-DESIGN.md.

## Global constraints

Baseline247:86,250 tests/framework/264 prior files. Preserve prior hashes,
GNU/BRE/ERE, frozen35/214 outcomes/120 methods, parser/IR/opcodes/journal/shared
limits/exported API/dependencies/workflows. UTF8+LF; no Python/agents/local SDK
installation. Reuse linked worktree; user owns merge/tag/publication; issue7/102
open and Grep last. All seven inventories precede qualification; final evidence
only PR248/issue7 footer, without documentation-only CI repeats.

## Review focus

1. Candidate at the newline and matches crossing it remain valid; later starts fail.
2. Offset-relative window follows all six newline conventions, raw UTF and fragments.
3. Ordinary retries retain the window; native forward SKIP honors anchored/filtered/unfiltered entry distinctions and normal scope/reset behavior.
4. Anchored ignores FirstLine; EndAnchored and literal/caller/source policies compose.
5. Owned/default state, validation priorities, bounded work/cancellation and capacity remain shared.

## Task 1: native-first contract

**Files:** Create tests/Pcre/PcreFirstLineCompileReferenceTests.cs.
**Interfaces:** Optional reflection selects future bool FirstLine; native0x100
plus independent Anchored0x80000000 and EndAnchored0x20000000.

- [x] Preflight window/offset/newline/fragment/cut/scoped/caller protocols, diagnostics, work and capacity boundaries.
- [x] Write explicit native batches and ownership/default/validation/work/capacity controls; keep all264 earlier test files fixed.
- [x] Publish tests-only draft PR248; inspect clean net8/net9/net10 RED with only missing-policy failures and freeze fixture before production.

## Task 2: immutable policy and bounded candidate frontier

**Files:** Modify src/Pcre/PcreByteOptions.cs, PcreByteCompiler.cs,
PcreByteProgram.cs, PcreByteMatcher.cs; update seven inventories.
**Interfaces:** bool PcreByteOptions.FirstLine init; bool
PcreByteProgram.FirstLine init; compiler snapshots after ordinary compilation.
Add bool PcreByteProgram.FirstLineSkipBypassesWindow from bounded
HasUnfilteredFirstLineSkipEntry(PcreByteAlternatives root), without IR changes.

- [x] Add default-false option/program flag and snapshot without syntax/opcode/capacity cost.
- [x] Disable unobserved byte/suffix shortcuts under FirstLine. Gate ordinary candidate entry after scalar selection and before COMMIT entry using a frontier from startOffset, existing newline/UTF readers and Step charges; ignore flag when Anchored.
- [x] Honor explicit forward SKIP before anchored termination, allowing only its requested next entry. Bypass FirstLine only for a bounded proven-unfiltered root prefix; preserve filtered entry and ordinary-failure termination.
- [x] Include all seven inventories before source push.
- [ ] Verify all18 results/supporting gates, prior264 hashes, frozen fixture/source and shared resources; record final evidence externally.

## Task 3: review and delivery

- [ ] Author review whole branch against five boundaries; no agents.
- [ ] Verify clean worktree and exact source/base/tree/proposed-merge identities.
- [ ] Record evidence on PR248/issue7 footer, mark ready and retire only this plan's ledger.

## Composition finding before production

RED750 exposed199 missing-window groups,2 bounded-work cases,1 ownership case
and4 pre-existing anchored-SKIP protocol groups/framework; all86,250 prior tests
passed. Expand tests before production with native filtered/unfiltered forward
SKIP and explicit FirstLine=false anchored regressions. The four-file scope is
unchanged; the additional immutable proof and forced-entry marker are now part
of Task2. Require clean expanded RED and fixture freeze before implementation.

## Tests-first checkpoint

Expanded tests-only [RED751](https://github.com/uniblab/Icod.Expressions/actions/runs/43590798617), source `77ed49eca15e2f9c2a77ca9feda7917f485b21fa`, Linux ARM132434872721: net8/net9/net10 each built with zero compiler warnings/errors and ran **368 expected failures /86,487 passes /86,855 total /zero skips**. Failures were341 missing-window groups,24 anchored-SKIP groups,2 bounded-work cases and1 ownership case/framework, all after successful native assertions. All86,250 earlier tests and237 new controls passed. Freeze `68d9c25772810758954234e4df4b45c76677e3dd` before production. Initial RED750 exposed four anchored-SKIP groups beyond the original FirstLine predictions; the fixture was expanded from413 to605 before production, preserving every original expectation and all prior test files.

Remaining qualification/closure boxes are tracked by final PR248/issue7 evidence after this source head; do not push a documentation-only CI repeat.

## UTF prefix qualification correction

Initial source4e24403e0e4c8f552dbaaf1e09b39ba672231b65/CI75243591726169 built cleanly; Linux ARM132437783721 ran4 failures/86,851 passes/86,855 total/zero skips on all three frameworks. Only plain/named SKIP into dot-plus-B in tolerant UTF failed, with both EndAnchored policies. Native10.48 scan_prefix stops at UTF OP_ANY/OP_ALLANY before considering the suffix; the bounded proof now treats UTF dot as opaque regardless of suffix, retaining fixed-width byte suffix filtering. The four frozen regression groups provide RED; all264 earlier test files and fixture68d9c25772810758954234e4df4b45c76677e3dd remain unchanged. Broader prefix proofs stay in issue102. Final all18 qualification/review/identities remain external on PR248/issue7.

## Task 4: requested temporary CI policy

User commit0ab40ea6048c43d2c908d8eef1402a5acfde2bcc changes the test project
to net10.0 only. The user then explicitly requested builds for8/9/10 with test
execution only in10. This supersedes the earlier eighteen-result requirement
for the final head. Preserve all engine/test-case source blobs and that user
project change; allow the existing main-job verifier fixture to follow the new
job set. Pre-policy source1ed0262 already passed all18 results on CI753.

**Files:** PR/main/release workflows, macOS framework action, result contract,
main-job prerequisite/fixture, README and seven inventories. No engine change.
**Interfaces:** expectedFrameworks=[net10.0]; six native net10 results; seven
required main jobs (five combined native jobs, one Intel test job, aggregate).

- [x] Resync user commit without altering the frozen605-case fixture.
- [x] Observe the updated main-job positive fixture fail against the old guard.
- [x] Restrict test/parity/smoke execution to10; keep solution/package builds for8/9/10 on all six native hosts. Remove Intel8/9 test jobs and global TargetFrameworks overrides.
- [x] Update the result contract and main-job guard, and verify the existing acceptance/rejection checks.
- [x] Include the CI policy in README/inventories before source qualification.
- [ ] Qualify six complete net10 results,214 frozen/120 methods each, zero failures/skips/compiler warnings/errors; verify all three library outputs on every native host and all configured compatibility/supporting gates.
- [ ] Review final scope/identities, record current policy and evidence on PR248/issue7, mark ready and retire only this plan's ledger.

## Requested temporary .NET 10 test policy

The user changed tests/Icod.Expressions.Tests.csproj in commit0ab40ea6048c43d2c908d8eef1402a5acfde2bcc to target net10.0 only, then explicitly requested builds for net8.0/net9.0/net10.0 with test execution only on10. PR/main/tag workflows now retain all three library builds on all six native OS/architecture hosts, select net10.0 for full/frozen tests and parity/package smoke execution, and remove the Intel8/9 test jobs. Intel restores/builds the complete solution without TargetFrameworks overrides. The result contract expects net10.0; release prerequisites require all seven current main jobs, including Intel test and aggregation. Existing main-job acceptance/rejection checks were updated (RED against the old guard, then all nine checks GREEN); generic result-validation capabilities remain unchanged.

Pre-policy engine source1ed02620bd83355a351570fe8af655c848309b84/CI75343592621873 passed all18 results at86,855 each. Final qualification after this user-directed configuration change requires six net10 results,214 frozen outcomes/120 methods each, zero failures/skips/compiler warnings/errors, all three library outputs per native host, supporting gates and all configured source-pinned compatibility jobs. The frozen605-case fixture68d9c25772810758954234e4df4b45c76677e3dd, all prior C# test sources and engine sources from1ed0262 remain unchanged. The user project edit and existing main-job gate fixture are intentional configuration changes. Final current-head evidence/review/identities remain external on PR248 and only issue7 footer; this policy supersedes the previous eighteen-current-result requirement. No merge/tag/publish action is authorized here.
