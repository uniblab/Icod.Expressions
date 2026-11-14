# Caller Line-Boundary Policies Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: superpowers:executing-plans. Execute inline; standing user direction prohibits subagents and approval pauses.

**Goal:** Add caller NotBol/NotEol without changing explicit absolute anchors.
**Architecture:** Snapshot two global bools in the parser; annotate existing typed anchors; lower disabled non-multiline anchors to Fail and multiline boundary suppression to the existing Second operand.
**Tech Stack:** C#13, library net8/net9/net10, tests net10, xUnit, pinned native PCRE2 10.48/JIT.
**Spec:** doc/PCRE-CALLER-LINE-BOUNDARIES-DESIGN.md.

## Global Constraints

- Base merged249 eb9751af94f3824113b7715af7cc008d21465841/tree6ecf612749f4a23d04da49d01145c31d2542341b,87,744 tests.
- All266 prior test files/261 C# sources and frozen35 source blobs stay fixed.
- Freeze the three new C# files after clean tests-only RED, before production.
- Six production files; existing AST kinds/opcodes/frames/journal/quotas/API/dependencies/workflows remain.
- Builds net8/net9/net10; tests only net10 on six native hosts;214 frozen outcomes/120 methods each.
- No subagents/Python/local SDK installs/approval pauses; user owns merge/tag/publish; Grep last.
- Seven inventories before final source push; final evidence PR250/issue7 footer only.

## Review Focus

1. Caret/dollar flags must leave backslash-A/Z/z and inert source text unchanged.
2. Keep multiline internal newlines/alternate-caret EOF and fixed-versus-variable JIT end limits distinct.
3. Original offset/physical boundaries, raw UTF fragments and anchored/first-line/cut restart behavior compose.
4. Global flags survive reset/calls/assertions; Literal and entry-scope MATCH_LINE/MATCH_WORD guards retain provenance.
5. Own source/options and preserve validation priority, stable exact/exhausted budgets, cancellation and structural capacities.

### Task 1: tests-only native conformance fixture

**Files:** tests/Pcre/PcreCallerLineBoundaryReferenceTests.cs; tests/Pcre/PcreCallerLineBoundaryProtocols.Part1.cs; tests/Pcre/PcreCallerLineBoundaryProtocols.Part2.cs; this design and plan.
**Interfaces:** native Match(subject, offset, PcreMatchOptions) versus managed Compile(source, options).Match(subject, offset); optional reflection setters permit the unchanged baseline build.

- [ ] Pin1,023 groups/45,411 protocols and69 diagnostics; native batches precede managed comparison.
- [ ] Add36 work/ownership/strict-validation/capture/depth/exact-instruction cases, with stable counters exercised on the tests-only baseline.
- [ ] Publish draft PR250 with production identical to merged249.
- [ ] Observe682 expected failures/88,190 passes/88,872 total/zero skips/compiler warnings/errors; all87,744 earlier tests pass.
- [ ] Freeze all three fixture blobs and record exact RED source/tree/job evidence before production.

### Task 2: typed anchor policy integration

**Files:** src/Pcre/PcreByteOptions.cs; PcreByteParser.cs; PcreByteParser.Anchors.cs; PcreByteSyntax.cs; PcreByteProgram.cs; PcreByteMatcher.cs; seven inventories.
**Interfaces:** readonly parser NotBol/NotEol; existing typed anchor metadata defaults false; existing instruction Second carries multiline boundary suppression.

- [ ] Add/snapshot default-false caller flags, retaining Literal match-policy compatibility.
- [ ] Annotate caret/dollar factories, including entry-scope line wrappers; retain original anchor types for parsing/width/diagnostic checks.
- [ ] Emit Fail for disabled non-multiline anchors; preserve explicit escape anchors and instruction count.
- [ ] Suppress only the applicable subject boundary for multiline anchors; preserve newline/fragment/scoped-end readers.
- [ ] Update design/syntax/resource/Grep/roadmap and this design/plan before source qualification.
- [ ] Require six net10 results at88,872 each, zero compiler warnings/errors/failures/skips,214 frozen/120 methods, all three library outputs and every supporting gate.

### Task 3: review and delivery

**Files:** PR250 body and issue7 footer; no documentation-only CI repeat.
**Interfaces:** frozen blobs, exact source/base/tree/proposed-merge ordered parents and completed current-head native logs.

- [ ] Review whole branch against all five boundaries; verify clean worktree, prior hashes and frozen35.
- [ ] Record final evidence externally, preserve original2600-character issue7 prefix and mark ready.
- [ ] Retire only this plan's identity-checked ledger; preserve branch/worktree. User merges/tags/publishes.
