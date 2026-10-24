# PCRE Byte Leading DotAll Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Recognize a leading internal byte-mode `(?s)` and make dot match LF throughout the pattern.

**Architecture:** Consume the exact prefix before parsing, select an all-byte set for dot atoms, and reuse the existing class bytecode/matcher.

**Tech Stack:** C# 13, .NET 8/9/10, xUnit.

**Spec:** `doc/PCRE-BYTE-LEADING-DOTALL-DESIGN.md`

## Global Constraints

- Root solution/project in `/`, code in `/src`, tests in `/tests`.
- Internal 8-bit non-UTF API only; no runtime PCRE.NET or System.Text.RegularExpressions.
- Preserve frozen GNU production/tests, exact byte offsets, 1,000,000-step default work budget, cancellation, and single library package.
- No Grep switch or CommandFramework production change.

## Review Focus

- Ordinary dot excludes LF; prefixed dot consumes it, including in a second alternative.
- Prefix does not allocate or shift capture slots.
- Escaped dot and bracket dot remain literal under DotAll.
- Unsupported scoped or mid-pattern forms do not silently enable DotAll.
- Matching high bytes/NUL remains byte based, and small work limits still interrupt searching.

---

### Task 1: Characterization

**Files:** `tests/Pcre/PcreByteLeadingDotAllTests.cs`.

**Interfaces:** existing internal `PcreByteCompiler.Compile` and `PcreCompiledBytePattern.Match`.

- [ ] Write tests for review cases, Grep fixtures, and diagnostics.
- [ ] Push test-only commit and observe expected semantic failures on hosted CI.

### Task 2: Parser and byte set

**Files:** `src/Pcre/PcreByteParser.cs`, `src/Pcre/PcreByteSet.cs`, `doc/PCRE-DESIGN.md`.

**Interfaces:** one exact prefix at offset zero sets parser DotAll; `PcreByteSet.AllBytes` contains every byte.

- [ ] Consume prefix, select all-byte dot set, and update the syntax table.
- [ ] Verify focused/full tests, six-runner CI, frozen corpus, and package smoke; request independent review.
