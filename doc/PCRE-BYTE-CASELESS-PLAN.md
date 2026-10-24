# PCRE Byte ASCII Caseless Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add one leading `(?i)` for ASCII byte caseless matching throughout the internal PCRE matcher.

**Architecture:** Parse one whole-pattern flag; carry the flag on the compiled byte program for byte comparisons; close bracket sets before negation. Preserve source bytes in captures.

**Tech Stack:** C# 13, .NET 8/9/10, xUnit.

**Spec:** `doc/PCRE-BYTE-CASELESS-DESIGN.md`

## Global Constraints

- Root project and solution in `/`, source in `/src`, tests in `/tests`.
- Internal 8-bit non-UTF API; no PCRE.NET or `System.Text.RegularExpressions` runtime fallback.
- Preserve the frozen GNU production and tests, byte offsets, provisional work/depth limits, cancellation, and the single library package.
- Keep unsupported option spellings explicit. Do not migrate Grep or alter CommandFramework production code.

## Review Focus

- Negated classes exclude both cases, while shorthand complements remain complements.
- POSIX upper/lower class components include both ASCII cases under `(?i)`.
- Backreferences compare source bytes caselessly while captures report original bytes and positions.
- Fixed literal assertions apply caselessness without consuming bytes.
- High bytes, NUL, and nonletters are never folded; limits and cancellation still count comparisons.

---

### Task 1: Test-first characterization

**Files:** `tests/Pcre/PcreByteLeadingCaselessTests.cs`, `tests/Pcre/PcreByteLeadingDotAllTests.cs`.

**Interfaces:** `PcreByteCompiler.Compile(ReadOnlyMemory<byte>, PcreByteOptions?, CancellationToken)` and `PcreCompiledBytePattern.Match`.

- [ ] Write focused tests for each review case, default case-sensitive behavior, and unsupported option placement; replace the prior `(?i)` unsupported assertion.
- [ ] Run hosted CI on a test-only PR and confirm semantic failures from the missing feature.

### Task 2: Compile and execute ASCII caseless comparisons

**Files:** `src/Pcre/PcreByteParser.cs`, `src/Pcre/PcreByteSet.cs`, `src/Pcre/PcreByteCompiler.cs`, `src/Pcre/PcreByteProgram.cs`, `src/Pcre/PcreByteMatcher.cs`, `doc/PCRE-DESIGN.md`.

**Interfaces:** Parser `Caseless`, program `Caseless`, set `AsciiCaseClosure`, matcher ASCII byte equality.

- [ ] Parse one leading `(?i)`; retain unsupported syntax for combinations, scoped, and mid-pattern flags.
- [ ] Close bracket components before outer or POSIX negation; compare literal, backreference, and lookaround bytes with ASCII case equality.
- [ ] Run all six Staging CI lanes and read full test, parity, frozen corpus, and package smoke results.
- [ ] Review the whole PR diff and leave it ready for the user's merge.
