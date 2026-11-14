# Variable-Lookbehind Ceiling Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: superpowers:executing-plans. Execute inline under standing user direction; no subagents or approval pauses.

**Goal:** Add caller MaxVarLookbehind with default255 and native fixed/variable distinctions.
**Architecture:** Snapshot uint compile context in the parser; consume it only in the existing variable-branch width comparison.
**Tech Stack:** C#13, library net8/net9/net10, tests net10, xUnit, pinned PCRE2 10.48/JIT.
**Spec:** doc/PCRE-VARIABLE-LOOKBEHIND-LIMIT-DESIGN.md.

## Global Constraints

- Base505c3acf2c729bb5be531cc4399bad2b7f52c44a/tree7488f706f55c0f3ae3c898068fd7819f2f0daee3;88,872 baseline.
- 269 prior test files/264 C# sources and frozen35 blobs fixed; freeze3 new C# blobs after clean RED.
- Default255; uint domain includes0 and4,294,967,295; fixed65,535 and all shared capacities remain.
- Three production files; no new AST/opcode/frame/instruction/API/dependency/workflow.
- Six native net10 results at89,568 each;214 frozen/120 methods and all3 library outputs each.
- Seven inventories before final source push; final evidence externally on PR251/issue7 footer only.
- No agents/Python/SDK installs/approval pauses. User merges/tags/publishes; Grep last.

## Review Focus

1. Fixed branches and different fixed alternative lengths survive zero; variable alternatives compare their combined maximum.
2. Raised/uint-max values retain fixed65,535 and shared capture/depth/instruction/work ceilings.
3. Nested/erased/called/referenced behinds validate dependencies and keep their own diagnostic opening.
4. Byte/UTF scalar widths, signed captures, labels, non-atomic assertions and inert source retain native behavior.
5. Source/options ownership, UTF/offset/limit priority, stable budget snapshots and cancellation survive.

### Task1: tests-only fixture

**Files:** create tests/Pcre/PcreVariableLookbehindLimitReferenceTests.cs and PcreVariableLookbehindLimitProtocols.Part1.cs/Part2.cs; this design/plan.
**Interfaces:** native PcreRegexSettings.MaxVarLookbehind versus optional reflection setter on internal options, allowing unchanged baseline compilation.

- [x] Pin410 matching groups/267 diagnostics from native research, plus19 work/ownership/validation/capacity cases; native batches first.
- [x] Publish draft PR251 with production identical to merged250.
- [x] Observe224 expected failures/89,344 passes/89,568 total/zero skips/compiler warnings/errors; verify exact changed-policy groups plus3 ownership, all88,872 prior and472 new controls pass.
- [x] Freeze3 C# blobs with exact RED source/tree/job evidence before production.

### Task2: parser context integration

**Files:** modify src/Pcre/PcreByteOptions.cs, PcreByteParser.cs, PcreByteParser.Lookbehind.cs; seven inventories.
**Interfaces:** public uint MaxVarLookbehind init=255 on internal options; readonly uint maxVarLookbehind in parser; compare (uint)maximum only when variable.

- [x] Add and snapshot the context field; keep it outside Literal option conflicts.
- [x] Replace the variable maximum255 comparison; preserve existing UnsupportedSyntax and opening offsets.
- [x] Update design/syntax/resource/Grep/roadmap/current design/current plan before source qualification.
- [ ] Require six fresh89,568 net10 results,214 frozen/120 methods each, all3 library outputs and supporting gates.

### Task3: review and delivery

**Files:** PR251 body and issue7 footer; owned ignored ledger.
**Interfaces:** exact source/base/tree/proposed-merge ordered parents, frozen3/prior269/frozen35 and fresh completed native logs.

- [ ] Author whole-branch review against all five boundaries; clean worktree and diff checks.
- [ ] Record final evidence externally, preserve issue7prefix, mark ready and verify readback; no docs-only CI repeat.
- [ ] Retire only this plan's identity-checked ledger, preserving branch/worktree.

Exact observed Task1 proof/frozen blobs live in the design. Task2 source is implemented; current-head six-host qualification and Task3 final evidence remain delivery gates recorded externally.
