# UTF-8 Caseless References Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Enable scalar-aware UTF-8 caseless backreferences and fixed literal lookarounds with exact byte spans.

**Architecture:** Preserve byte and case-sensitive UTF fast paths. Add a scalar comparison path in existing backreference and literal assertion opcodes; walk previous scalar boundaries for lookbehind and compare forward across actual subject widths. Remove parser guards only for these supported constructs.

**Tech Stack:** C# 13, .NET 8/9/10, xUnit, Staging CI.

**Spec:** `doc/PCRE-UTF8-CASELESS-REFERENCES-DESIGN.md`

## Global Constraints

- Project file at root, source under `/src`, tests under `/tests`, samples under `/samples`.
- No Grep or CommandFramework production changes and no public PCRE capability claim.
- Keep original source-byte spans, fragment barriers, deterministic work limits and cancellation.
- Staging PR matrix: Linux, Windows and macOS, x64 and ARM64.

## Review Focus

- A one-byte capture referenced as a three-byte Kelvin sign advances the subject by three bytes: Task 1.
- A three-byte capture referenced as a one-byte letter preserves the capture's original span: Task 1.
- Caseless lookbehind uses scalar count rather than asserted byte count: Task 2.
- Negative assertions at fragment boundaries never inspect malformed bytes: Task 2.
- Cancellation during long scalar comparisons interrupts before a false match result: Tasks 1 and 2.

---

### Task 1: Scalar-aware backreferences

**Files:** `src/Pcre/PcreByteParser.cs`, `src/Pcre/PcreByteMatcher.cs`, `tests/Pcre/PcreUtf8CaselessReferenceTests.cs`.

**Interfaces:** Existing `PcreUtf8.TryRead` and `PcreUtf8CaseFold.Equals`; `PcreByteOpcode.Backreference` reads captured source and consumes subject scalar widths.

- [ ] Write UTF caseless reference fixtures including variable widths, failure paths, barriers, capture spans, work limit and cancellation.
- [ ] Push test-first commit; observe Staging failures for absent support.
- [ ] Remove the UTF caseless reference guard and implement scalar comparisons; preserve byte/caseful behavior.
- [ ] Run full CI, then commit.

### Task 2: Scalar-aware fixed literal assertions

**Files:** `src/Pcre/PcreByteParser.cs`, `src/Pcre/PcreByteMatcher.cs`, `tests/Pcre/PcreUtf8CaselessReferenceTests.cs`, `tests/Pcre/PcreUtf8CaselessTests.cs`, `doc/PCRE-DESIGN.md`.

**Interfaces:** `PcreByteOpcode.AssertLiteral` compares decoded UTF scalars; lookbehind starts after walking one subject scalar for each literal scalar.

- [ ] Write positive/negative lookahead and lookbehind fixtures, UTF width changes, empty assertions, barriers and work/cancellation cases.
- [ ] Confirm Staging red; then remove the assertion parser guard and implement the scalar comparison.
- [ ] Replace prior unsupported diagnostics with semantic fixtures; keep unsupported UTF bracket diagnostics.
- [ ] Verify all six Staging jobs on the exact final head, plus frozen GNU cases and package smoke.
- [ ] Run independent read-only review, address important findings, and update PR for review.
