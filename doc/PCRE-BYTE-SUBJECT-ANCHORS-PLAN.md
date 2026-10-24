# PCRE Byte Subject Anchors Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add internal byte-mode `\A`, `\Z`, and `\z` subject assertions.

**Architecture:** Parse `\A`/`\z` to existing absolute anchor node; parse `\Z` to a distinct node and bytecode instruction that succeeds at end or before one final LF.

**Tech Stack:** C# 13, .NET 8/9/10, xUnit.

**Spec:** `doc/PCRE-BYTE-SUBJECT-ANCHORS-DESIGN.md`

## Global Constraints

- Root solution/project in `/`, implementation in `/src`, tests in `/tests`.
- Internal 8-bit non-UTF API; no PCRE.NET or System.Text.RegularExpressions runtime call.
- Preserve frozen GNU production/tests, byte positions, provisional work/stack limits, cancellation, and single package.
- Do not switch Grep or alter CommandFramework production code.

## Review Focus

- `\Z` succeeds before exactly one final LF; `\z` only at absolute end.
- Nonzero start offsets do not move subject edges.
- Bracket escapes and quantification fail with explicit diagnostics.
- Backtracking restores captures after an anchor succeeds but later atoms fail.
- Searching a long nonmatching input remains bounded and cancellable.

---

### Task 1: Test-first characterization

**Files:** `tests/Pcre/PcreByteSubjectAnchorTests.cs`.

**Interfaces:** internal compiler/matcher and exact byte result.

- [ ] Add focused test cases for review focus, NUL/high bytes, and empty subject.
- [ ] Push test-only commit; observe hosted CI semantic failures.

### Task 2: Anchor parser and execution

**Files:** `src/Pcre/PcreByteSyntax.cs`, `src/Pcre/PcreByteParser.cs`, `src/Pcre/PcreByteProgram.cs`, `src/Pcre/PcreByteMatcher.cs`, `doc/PCRE-DESIGN.md`.

**Interfaces:** `PcreByteEndBeforeFinalLf` and `PcreByteOpcode.EndBeforeFinalLf`.

- [ ] Recognize three escapes outside brackets, reject direct repetition.
- [ ] Reuse Start/End, add LF-before-end instruction, keep zero-width work accounting.
- [ ] Verify focused/full tests, six-runner CI, frozen corpus, package-only smoke; request independent review.
