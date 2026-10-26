# Counted Repetition Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Implement internal counted repetition for already supported PCRE atoms while preserving ordered matches and bounded compilation.

**Architecture:** Parse numeric bounds into `PcreByteRepeat`; emit mandatory and optional copies and reuse the existing progress guarded unlimited tail. Cap emitted instructions at 4096 and report a compile diagnostic at the quantifier.

**Tech Stack:** C# 13, .NET 8/9/10, xUnit, Staging CI.

**Spec:** `doc/PCRE-COUNTED-REPEATS-DESIGN.md`

## Global Constraints

- Project and solution files in `/`, source `/src`, tests `/tests`, samples `/samples`.
- Internal API only; no PCRE.NET or .NET regex runtime path.
- No Grep or CommandFramework production change; preserve GNU corpus and public API.
- Six Staging RID jobs, three frameworks, frozen 214-case GNU corpus and package smoke.

## Review Focus

- A zero minimum with an empty body must terminate; Task 2.
- A bounded group must restore captures when a greedy path backtracks; Task 2.
- Huge nested counts must fail before large allocation; Task 2.
- UTF counted repeats must preserve byte offsets and barriers; Task 2.
- Ambiguous brace text must produce a diagnostic rather than silently matching as a literal; Task 1.

---

### Task 1: Numeric syntax and diagnostics

**Files:** `src/Pcre/PcreByteParser.cs`, `src/Pcre/PcreByteSyntax.cs`, `tests/Pcre/PcreCountedRepeatTests.cs`.

**Interfaces:** `PcreByteRepeat(PcreByteNode Child, int Minimum, int Maximum, bool Greedy, int Offset = 0)`; parser accepts `{n}`, `{n,m}`, `{n,}`, `{,m}`, horizontal spaces, optional lazy suffix and rejects invalid bounds.

- [ ] Write matching and diagnostic tests for exact, finite, unbounded, zero, lazy, spaced forms, malformed bounds and suffixes.
- [ ] Push a test-only commit and verify Staging failures are the expected unsupported syntax assertions.
- [ ] Implement parsing and offset diagnostics without changing bytecode yet.
- [ ] Commit parser changes; Task 2's matching tests remain red until emission is implemented.

### Task 2: Bounded emission and integration

**Files:** `src/Pcre/PcreByteProgram.cs`, `tests/Pcre/PcreCountedRepeatTests.cs`, `doc/PCRE-DESIGN.md`.

**Interfaces:** `CompileRepeat(PcreByteRepeat)` expands mandatory/optional copies; emitted instruction cap 4096, diagnostic `PatternRepeatLimitExceeded` at `repeat.Offset`.

- [ ] Write red cases for group capture rollback, empty body, nested count resource cap, UTF spans/barriers, work limit and cancellation.
- [ ] Implement emission and cap, preserving existing `*`, `+`, `?` compilation.
- [ ] Run the complete six-job Staging matrix, request independent review, fix important findings, and leave a ready PR.
