# End-Anchored Compilation Implementation Plan

> Execute inline with executing-plans under the standing no-agents/no-pauses workflow.

**Goal:** Require root matching to consume through physical subject EOF.
**Architecture:** Snapshot one program policy flag; normal short completion
backtracks, while short root ACCEPT abandons the candidate. Retain shared VM.
**Tech Stack:** C#13, net8/net9/net10, xUnit and pinned PCRE2 10.48 oracle only.
**Spec:** doc/PCRE-END-ANCHORED-COMPILE-DESIGN.md.

## Global constraints

Baseline246:85,685 tests/framework/263 prior files. Preserve all prior hashes,
GNU/BRE/ERE, frozen35/214 outcomes/120 methods, parser/IR/opcodes/journal/shared
limits/exported API/dependencies/workflows. UTF8+LF; no Python/agents/local SDK
installation. Reuse linked worktree; user owns merge/tag/publication; issue7/102
open and Grep last. All seven inventories precede source qualification; final
evidence only PR247/issue7 footer, without documentation-only CI repeats.

## Review focus

1. Ordinary acceptance retries longer branches; short root ACCEPT discards cuts/choices.
2. Recursive and assertion ACCEPT remain scoped, with captures/labels/reset state intact.
3. Physical EOF wins over malformed-fragment and reported backslash-K endpoints.
4. Both anchoring flags, literal/casing and word/line/newline/source policies compose.
5. Ownership, validation priorities, work/cancellation and full capacity remain shared.

## Task 1: native-first contract

**Files:** Create tests/Pcre/PcreEndAnchoredCompileReferenceTests.cs.
**Interfaces:** Optional reflection selects future bool EndAnchored;
native0x20000000 plus independent Anchored0x80000000. Group explicit protocols.

- [x] Preflight normal/early/scoped completion, recursion/cuts/labels, modes/UTF/offsets, diagnostics, work and capacity boundaries.
- [x] Write pinned protocol batches and ownership/default/validation/work/capacity controls; keep all263 earlier test files fixed.
- [ ] Publish tests-only draft PR247; inspect clean net8/net9/net10 RED with only missing-policy failures and freeze fixture before production.

## Task 2: immutable policy and root completion

**Files:** Modify src/Pcre/PcreByteOptions.cs, PcreByteCompiler.cs,
PcreByteProgram.cs, PcreByteMatcher.cs; update seven inventories.
**Interfaces:** bool PcreByteOptions.EndAnchored init, bool
PcreByteProgram.EndAnchored init; compiler snapshots after ordinary compilation.

- [ ] Add default-false option/program flag and snapshot without opcode or capacity cost.
- [ ] Reject short root normal completion through shared backtracking. After scoped/synthetic-close handling, discard short root EarlyAccept choices/cuts and fail candidate. Disable tolerant empty shortcut under EndAnchored.
- [ ] Include all seven inventories before source push.
- [ ] Verify all18 results/supporting gates, prior263 hashes, frozen fixture/source and shared resource contracts; record evidence externally.

## Task 3: review and delivery

- [ ] Author review whole branch against all five boundaries; no agents.
- [ ] Verify clean worktree and exact source/base/tree/proposed-merge identities.
- [ ] Record evidence on PR247 and only issue7 footer, mark ready and retire only this plan's ledger.
