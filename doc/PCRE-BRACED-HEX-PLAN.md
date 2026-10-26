# Braced Hexadecimal Escape Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Compile `\x{...}` to one literal scalar or byte in supported PCRE contexts.

**Architecture:** Extend `PcreByteParser.ParseEscape(int)` with a bounded hexadecimal accumulator, reuse the existing literal nodes and each caller's encoding path. Avoid runtime regex dependencies or matcher changes.

**Tech Stack:** C# 13, .NET 8/9/10, xUnit, Staging CI.

**Spec:** `doc/PCRE-BRACED-HEX-DESIGN.md`

## Global Constraints

- Root solution/project, `/src`, `/tests`, `/samples` layout.
- No public PCRE API claim, Grep or CommandFramework production change.
- Preserve source-byte spans, cancellation and work limits, existing GNU corpus and package smoke.

## Review Focus

- A value above 0xff must fail in byte mode before a bracket casts it: Task 1.
- A UTF surrogate or out-of-range value must fail before `Rune` encoding: Task 1.
- Leading zeros must not overflow or make parsing take superlinear time: Task 1.
- Bracket ranges and fixed assertions must see the same scalar as ordinary atoms: Task 1.
- Malformed UTF barriers must remain effective after an escaped scalar: Task 1.

---

### Task 1: Parse and integrate the escape

**Files:** `src/Pcre/PcreByteParser.cs`, `tests/Pcre/PcreBracedHexTests.cs`, `tests/Pcre/PcreByteAtomTests.cs`, `doc/PCRE-DESIGN.md`.

**Interfaces:** `ParseEscape(int offset)` returns `PcreByteLiteral` with valid byte/scalar value for `\x{...}`; existing atom/bracket/assertion consumers remain unchanged.

- [ ] Add test-first syntax, byte, UTF, bracket, assertion, repetition and barrier cases; move the old pending-escape fixture to the new tests.
- [ ] Push test-only commit and confirm expected unsupported-syntax failures in Staging.
- [ ] Parse braced hexadecimal values with checked upper bound and cancellation in the digit loop.
- [ ] Run full Staging matrix and package/frozen-corpus gates; self-review the diff and keep the PR draft until all six are green.
