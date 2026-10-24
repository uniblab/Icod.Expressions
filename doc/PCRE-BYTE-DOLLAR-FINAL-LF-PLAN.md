# PCRE Byte Dollar Before Final LF Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Correct default byte-mode `$` to match before one final LF as well as at absolute end.

**Architecture:** Reuse the `PcreByteEndBeforeFinalLf` syntax node and opcode already used by `\Z`, leaving strict `\z` on End.

**Tech Stack:** C# 13, .NET 8/9/10, xUnit.

**Spec:** `doc/PCRE-BYTE-DOLLAR-FINAL-LF-DESIGN.md`

## Global Constraints

- Root solution/project in `/`, implementation in `/src`, tests in `/tests`.
- Internal 8-bit non-UTF PCRE engine; no PCRE.NET or `System.Text.RegularExpressions` runtime fallback.
- Preserve frozen GNU production/tests, source byte offsets, provisional work/stack limits, cancellation, and single package.
- Do not change Grep or CommandFramework production code.

## Review Focus

- `$` accepts before exactly one final LF, never before an internal LF.
- `\z` remains strict at absolute end; `\Z` remains unchanged.
- `$` is zero width even when a later literal LF consumes that byte.
- Nonzero start offset cannot redefine the end; dotall is independent.
- Bracket `$` stays literal and direct `$+` is an invalid quantifier.

---

### Task 1: Test-first characterization

**Files:** `tests/Pcre/PcreByteDollarFinalLfTests.cs`.

**Interfaces:** `PcreByteCompiler.Compile` and `PcreCompiledBytePattern.Match` with byte-indexed captures.

- [ ] Add focused tests for every review case and raw NUL/high bytes.
- [ ] Push test-only draft PR; observe hosted failure specifically on final-LF dollar assertions.

### Task 2: Reuse the final-LF assertion

**Files:** `src/Pcre/PcreByteParser.cs`, `doc/PCRE-DESIGN.md`.

**Interfaces:** `$` returns `PcreByteEndBeforeFinalLf`, `\z` returns `PcreByteAnchor(false)`.

- [ ] Make the single parser dispatch change and document its default LF behavior.
- [ ] Verify complete Staging suite, frozen GNU corpus, parity and package-only smoke on six runners.
- [ ] Obtain independent review and leave PR ready for user merge.
