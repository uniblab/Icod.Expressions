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
