# PCRE Byte Word Boundaries Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add internal ASCII byte-mode `\b` and `\B` zero-width assertions.

**Architecture:** Parse to a boundary node, emit one opcode, and classify the adjacent subject bytes using the existing `Word` set under the shared work/cancellation budget.

**Tech Stack:** C# 13, .NET 8/9/10, xUnit.

**Spec:** `doc/PCRE-BYTE-WORD-BOUNDARIES-DESIGN.md`

## Global Constraints

- Root solution/project in `/`, implementation in `/src`, tests in `/tests`.
- Internal 8-bit non-UTF API only; no PCRE.NET or System.Text.RegularExpressions runtime path.
- Preserve frozen GNU source/tests, authoritative byte positions, provisional work/stack limits and single library package.
- Do not switch Grep or modify CommandFramework production code.

## Review Focus

- Empty input and non-word edges: `\b` fails, `\B` succeeds.
- A nonzero search offset does not hide the preceding subject byte.
- High bytes/NUL are non-word and `_` is word in this mode.
- Bracket `\b` remains backspace; bracket `\B` fails compilation.
- Boundary checks cannot bypass work limits or cancellation.

---

### Task 1: Characterization

**Files:** `tests/Pcre/PcreByteWordBoundaryTests.cs`.

**Interfaces:** existing internal compiler and matcher.

- [ ] Write focused tests for review cases, ordered alternatives/captures, and diagnostics.
- [ ] Push test-only commit; observe expected behavior failures in PR CI.

### Task 2: Parser, bytecode and matcher

**Files:** `src/Pcre/PcreByteSyntax.cs`, `src/Pcre/PcreByteParser.cs`, `src/Pcre/PcreByteProgram.cs`, `src/Pcre/PcreByteMatcher.cs`, `doc/PCRE-DESIGN.md`.

**Interfaces:** `PcreByteWordBoundary(bool IsBoundary)` and `PcreByteOpcode.WordBoundary` polarity in `First`.

- [ ] Recognize `\b`/`\B` outside brackets and reject their repetition.
- [ ] Execute boundary at current byte position, charging adjacent bytes to Step().
- [ ] Verify focused/full tests and six-runner CI, frozen GNU corpus, and package-only smoke; request independent review.
