# UTF-8 POSIX Bracket Classes Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Support all fourteen ordinary POSIX bracket class names in the internal UTF-8 PCRE path.

**Architecture:** Compile each POSIX name to an existing or new scalar predicate kind selected by UCP. Evaluate new predicate kinds in the existing scalar-set matcher while preserving its charged checks and UTF barriers.

**Tech Stack:** C# 13, `System.Text.Rune`, xUnit, net8/net9/net10.

**Spec:** `doc/PCRE-UTF8-POSIX-CLASSES-DESIGN.md`

## Global Constraints

- Internal API only; no Grep migration or CommandFramework production edit.
- Solution/project in root, source in `/src`, tests in `/tests`, design/plan in `/doc`.
- Preserve byte spans, invalid UTF barriers, cancellation and bounded work.
- PR Staging on Linux/Windows/macOS x64/ARM64 and net8/net9/net10; frozen GNU and package smoke must pass.

## Review Focus

- `alnum` in UCP includes Unicode number categories beyond Nd, while `digit` remains Nd.
- UCP `blank` includes U+180E; UCP `graph` excludes it and U+061C/U+2066–U+2069.
- UCP `punct` includes Latin-1 symbols but excludes later Unicode symbols.
- Non-UCP names do not classify non-ASCII UTF scalars, even those below U+0100.
- `[:^name:]` and outer negation never consume malformed bytes or EOF.

---

### Task 1: Characterize missing names

**Files:** Create `tests/Pcre/PcreUtf8PosixClassTests.cs`; update pending `alpha` expectations in `tests/Pcre/PcreUtf8BracketUnionTests.cs`, `PcreUtf8DigitTests.cs`, `PcreUtf8SpaceTests.cs`.

**Interfaces:** `PcreByteCompiler.Compile(byte[], PcreByteOptions)`, `PcreCompiledBytePattern.Match`, `PcreByteOptions.Utf8/Ucp/MatchInvalidUtf`.

- [ ] Write theory fixtures for every new name, UCP toggles, union and negation, byte spans, malformed barriers and limits; retain unknown-name diagnostics.
- [ ] Commit tests and docs; push a draft PR and verify a Staging failure because `alpha` is unsupported (tests compile).

### Task 2: Compile and evaluate predicates

**Files:** Modify `src/Pcre/PcreByteSyntax.cs`, `PcreByteParser.cs`, `PcreByteMatcher.cs`.

**Interfaces:** `PcreByteScalarClassKind`, `ParseUtf8PosixItem`, `ScalarClassContains`.

- [ ] Map all fourteen names to ASCII or Unicode predicate kinds selected at compile time, leaving `ascii` ASCII in both modes.
- [ ] Implement category membership and PCRE2's fixed special cases; reuse existing scalar-set union/negation evaluation and work accounting.
- [ ] Verify targeted tests and all Staging jobs at the exact final commit.

### Task 3: Matrix, review and delivery

**Files:** Update `doc/PCRE-DESIGN.md` feature matrix.

- [ ] Record completed subset and remaining Unicode-version/scoped-options gates.
- [ ] Obtain independent read-only whole-branch review, address material findings, and verify six green jobs on the exact final head before marking PR ready.
