# Pattern Byte-Length Limit Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: superpowers:executing-plans. Execute inline under standing user direction; no subagents or approval pauses.

**Goal:** Add caller MaxPatternLength with native8-bit source-length admission and priority.
**Architecture:** Nullable uint context; compare original memory length at the compile boundary before source copying; append a dedicated diagnostic.
**Tech Stack:** C#13, library net8/net9/net10, tests net10, xUnit, pinned native PCRE2 10.48/JIT.
**Spec:** doc/PCRE-PATTERN-LENGTH-LIMIT-DESIGN.md.

## Global Constraints

- Base6da239a485720ccd7299a67c4c7e7e3c8901056d/tree1906a545b08ee5e05545c2a678006809354f4978;89,568 baseline.
- All272 prior test files/267 C# sources/frozen35 fixed; freeze3 new C# blobs after clean RED.
- MaxPatternLength=null means unlimited; uint values include0/4,294,967,295; exact length admitted.
- Three production files; append diagnostic identity, preserve all existing identities and shared ceilings.
- Builds8/9/10, tests only10 on six native hosts;90,703 passes and214 frozen/120 methods per host plus all3 library outputs/supporting gates.
- Seven inventories before final source push; final evidence on PR252/issue7footer only; user merges/tags/publishes; Grep last.

## Review Focus

1. Count original supplied bytes including directives/trivia/NUL/UTF, excluding unrelated backing memory and compiler guards.
2. Zero/exact/null/uint-max comparison is inclusive and safe; all other quotas remain independent.
3. Preserve cancellation/work/Literal conflict priority before length, then original syntax/UTF/restriction diagnostics at allowed lengths.
4. Reject oversized memory before reading it; accepted sliced source/options remain owned after caller mutation.
5. Preserve work traces/exact exhaustion/cancellation and full capture/depth/instruction capacities, UTF/offset/subject priority.

### Task1: tests-only fixture

**Files:** create tests/Pcre/PcrePatternLengthLimitReferenceTests.cs and PcrePatternLengthLimitProtocols.Part1.cs/Part2.cs; this design/plan.
**Interfaces:** native PcreRegexSettings.MaxPatternLength versus optional reflection setter on internal nullable options; expected diagnostic name strings compile before the enum member exists.

- [x] Pin576 matching groups/535 diagnostics and24 work/ownership/view/no-read/priority/capacity cases; native first.
- [x] Publish draft PR252 with production identical to merged251.
- [x] Observe336 expected failures/90,367 passes/90,703 total/zero skips/compiler warnings/errors; exact334 missing groups plus2 no-read cases, all89,568 prior and799 new controls pass.
- [x] Freeze3 C# blobs and exact RED source/tree/job evidence before production.

### Task2: compile admission

**Files:** modify src/Pcre/PcreByteOptions.cs, PcreByteCompiler.cs, PcreByteResult.cs; seven inventories.
**Interfaces:** public uint? MaxPatternLength init on internal options, defaultnull; PatternLengthLimitExceeded appended; original memory.Length compared before ToArray.

- [x] Add nullable option outside Literal conflicts and append the diagnostic without reordering.
- [x] Check exceeded ceiling after cancellation/work/Literal checks and before source access; report byte0.
- [x] Update design/syntax/resource/Grep/roadmap/current design/current plan before source qualification.
- [ ] Require six fresh90,703 native net10 results, all3 library outputs/frozen/supporting gates.

### Task3: review and delivery

**Files:** PR252 body/issue7 footer; own ignored ledger.
**Interfaces:** fresh completed source-pinned logs and exact source/base/tree/proposed-merge ordered parents; immutable issue prefix.

- [ ] Author whole-branch review against all five boundaries; clean tree/diff/frozen/prior checks.
- [ ] Record final GREEN/review/identities externally, mark ready and verify readback; no docs-only CI repeat.
- [ ] Retire only this plan's identity-checked ledger; preserve branch/worktree.

Corrected Task1/native-preflight evidence and refrozen blobs are recorded in the design, superseding the initial invalid qualification. The original minimal production implementation is reapplied only after corrected RED. Task2 fresh six-host/supporting qualification and Task3 final review/evidence remain external delivery gates.
