# Anchored Compilation Implementation Plan

> Execute inline with executing-plans under the standing no-agents/no-pauses workflow.

**Goal:** Support caller-anchored matching at the requested candidate offset.
**Architecture:** Snapshot one program policy flag and gate unanchored candidate
search/retry; reuse the ordinary parser, bytecode and ordered matcher.
**Tech Stack:** C#13, net8/net9/net10, xUnit and pinned PCRE2 10.48 oracle only.
**Spec:** doc/PCRE-ANCHORED-COMPILE-DESIGN.md.

## Global constraints

Baseline245:85,360 tests/framework/262 prior files. Preserve all prior test
hashes, GNU/BRE/ERE, frozen35/214 outcomes/120 methods, parser/IR/opcodes/journal/
shared limits/exported API/dependencies/workflows. UTF8+LF; no Python/agents/local
SDK installation. Reuse linked worktree; user owns merge/tag/publication;
issue7/102 open and Grep last. All seven inventories precede source qualification;
final evidence only PR246/issue7 footer, without documentation-only CI repeats.

## Review focus

1. Fixed candidate suppresses all later-byte, fragment, SKIP and newline retries.
2. Recursion, assertions, ACCEPT, cuts, labels and reported resets retain scope.
3. Literal, source/caller modes, casing and word/line/newline policies compose normally.
4. Strict validation, tolerant raw offsets and lookbehind context remain byte-authoritative.
5. Owned options/source, first-attempt work/cancellation and full grammar/instruction capacity remain shared.

## Task 1: native-first contract

**Files:** Create tests/Pcre/PcreAnchoredCompileReferenceTests.cs.
**Interfaces:** Optional reflection selects future bool Anchored; native flag
0x80000000. Explicit protocol batches pin all native outcomes before managed
matching; baseline controls select Anchored=false.

- [x] Preflight matching/error/offset protocols, canonical first-attempt work, long-byte budget and capacity boundaries.
- [x] Write fixed-candidate, recursion/reset/cut, mode/UTF/offset, ownership/work/cancellation/capacity checks; keep all262 prior files fixed.
- [x] Publish tests-only draft PR246; inspect clean net8/net9/net10 RED with only missing-policy failures and freeze fixture before production.

## Task 2: immutable policy and candidate entry

**Files:** Modify src/Pcre/PcreByteOptions.cs, PcreByteCompiler.cs,
PcreByteProgram.cs, PcreByteMatcher.cs; update seven inventories.
**Interfaces:** bool PcreByteOptions.Anchored init; bool PcreByteProgram.Anchored
init; compile snapshots via program record copy, no new opcode/constructor.

- [x] Add default-false option and program flag; copy the option after ordinary compilation without changing root0/callee IR or resource capacity.
- [x] Gate unobserved byte/suffix searches and COMMIT first-byte entry for anchored programs. Keep existing validation and fragment construction.
- [x] Reject candidate offsets different from requested; return after first failed attempt before unanchored retry/bumpalong. Keep empty/nullable malformed-offset support and all VM backtracking/scopes.
- [x] Include all seven inventories before source push.
- [ ] Verify all18/full supporting gates, prior262 hashes, frozen fixture/source identities and capacity/work checks; record final evidence externally on PR246/issue7.

## Task 3: review and delivery

- [ ] Author review the whole branch against all five boundaries; no agents.
- [ ] Verify clean worktree and exact source/base/tree/proposed-merge identities.
- [ ] Record evidence on PR246 and only issue7 footer, mark ready and retire only this plan's ledger.

## Tests-first checkpoint

Tests-only [RED746](https://github.com/uniblab/Icod.Expressions/actions/runs/43578255592), source `33ed1a4a6e5344fa53df7aeb03c6c462ab371706`, Linux ARM job132397293572: net8/net9/net10 each built with zero compiler warnings/errors and ran **131 expected failures / 85,554 passes / 85,685 total / zero skips**. Failures were128 protocol groups,2 bounded-byte-budget cases and1 ownership case/framework. Every pinned native assertion, all85,360 earlier tests and194 new controls passed. Freeze `95620f1478537b71880c07c64e20474c3de537ad` before production. Before clean RED, one new byte-range diagnostic control was corrected to preserve its existing hyphen offset2; native108/offset4 and all earlier tests remained unchanged.

Remaining qualification/closure boxes are tracked by final PR246/issue7 evidence after this source head; do not push a documentation-only CI repeat.
