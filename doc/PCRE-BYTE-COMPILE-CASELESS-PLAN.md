# PCRE Byte Compile-Time Caseless Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add an internal compile-time ASCII caseless option for the byte-mode PCRE matcher.

**Architecture:** Seed the parser's case mode from `PcreByteOptions.Caseless`; the existing leading `(?i)` can enable it and leading `(?s)` leaves it alone. Existing program and matcher behavior then applies unchanged.

**Tech Stack:** C# 13, .NET 8/9/10, xUnit.

**Spec:** `doc/PCRE-BYTE-COMPILE-CASELESS-DESIGN.md`

## Global Constraints

- Root solution/project in `/`, implementation in `/src`, tests in `/tests`.
- Internal 8-bit non-UTF PCRE API, no PCRE.NET or `System.Text.RegularExpressions` runtime fallback.
- Preserve frozen GNU production/tests, byte offsets, limits/cancellation, and the single package.
- Grep and CommandFramework production code remain untouched; do not expose a public API yet.

## Review Focus

- Caller `Caseless=true` works with and without a leading `(?s)` and across alternatives.
- Leading `(?i)` still enables case folding when the caller option is false.
- Bracket complements, lookarounds, backreferences, NUL and high bytes retain #19 semantics.
- Default mode remains case sensitive and captures keep original byte positions.
- Match limits/cancellation still count byte comparisons under the external option.

---

### Task 1: Test-first option contract

**Files:** `tests/Pcre/PcreByteCompileCaselessTests.cs`.

**Interfaces:** internal `PcreByteOptions.Caseless` and `PcreByteCompiler.Compile`.

- [ ] Write tests for the review focus, including the leading `(?s)` interaction.
- [ ] Push a test-only draft PR and observe the missing option fail in hosted CI.

### Task 2: Initialize the parser from options

**Files:** `src/Pcre/PcreByteOptions.cs`, `src/Pcre/PcreByteParser.cs`, `src/Pcre/PcreByteCompiler.cs`, `doc/PCRE-DESIGN.md`.

**Interfaces:** bool `PcreByteOptions.Caseless`; parser constructor accepts the initial case mode.

- [ ] Add the option, seed parser state, and make leading flags affect only their own mode.
- [ ] Verify six Staging jobs, three target frameworks, frozen corpus, parity and package smoke.
- [ ] Request independent review and leave the PR ready for merge.
