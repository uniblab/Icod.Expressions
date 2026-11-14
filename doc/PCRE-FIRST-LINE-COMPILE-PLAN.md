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
3. SKIP/CRLF/inferred-line/empty-EOF retries cannot escape; scope/recursion/resets remain normal.
4. Anchored ignores FirstLine; EndAnchored and literal/caller/source policies compose.
5. Owned/default state, validation priorities, bounded work/cancellation and capacity remain shared.

## Task 1: native-first contract

**Files:** Create tests/Pcre/PcreFirstLineCompileReferenceTests.cs.
**Interfaces:** Optional reflection selects future bool FirstLine; native0x100
plus independent Anchored0x80000000 and EndAnchored0x20000000.

- [x] Preflight window/offset/newline/fragment/cut/scoped/caller protocols, diagnostics, work and capacity boundaries.
- [x] Write explicit native batches and ownership/default/validation/work/capacity controls; keep all264 earlier test files fixed.
- [ ] Publish tests-only draft PR248; inspect clean net8/net9/net10 RED with only missing-policy failures and freeze fixture before production.

## Task 2: immutable policy and bounded candidate frontier

**Files:** Modify src/Pcre/PcreByteOptions.cs, PcreByteCompiler.cs,
PcreByteProgram.cs, PcreByteMatcher.cs; update seven inventories.
**Interfaces:** bool PcreByteOptions.FirstLine init; bool
PcreByteProgram.FirstLine init; compiler snapshots after ordinary compilation.

- [ ] Add default-false option/program flag and snapshot without syntax/opcode/capacity cost.
- [ ] Disable unobserved byte/suffix shortcuts under FirstLine. Gate every candidate after scalar selection and before COMMIT entry using a frontier from startOffset, existing newline/UTF readers and Step charges; ignore flag when Anchored.
- [ ] Include all seven inventories before source push.
- [ ] Verify all18 results/supporting gates, prior264 hashes, frozen fixture/source and shared resources; record final evidence externally.

## Task 3: review and delivery

- [ ] Author review whole branch against five boundaries; no agents.
- [ ] Verify clean worktree and exact source/base/tree/proposed-merge identities.
- [ ] Record evidence on PR248/issue7 footer, mark ready and retire only this plan's ledger.
