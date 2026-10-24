# PCRE Byte Leading Multiline Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add one leading `(?m)` for LF-aware line anchors in the internal byte-mode PCRE matcher.

**Architecture:** The parser selects multiline-only line-anchor nodes for `^` and `$`, which compile to dedicated zero-width opcodes. Existing subject assertions stay absolute.

**Tech Stack:** C# 13, .NET 8/9/10, xUnit.

**Spec:** `doc/PCRE-BYTE-LEADING-MULTILINE-DESIGN.md`

## Global Constraints

- Root solution/project in `/`, implementation in `/src`, tests in `/tests`.
- Internal 8-bit non-UTF PCRE API; no PCRE.NET or `System.Text.RegularExpressions` runtime fallback.
- Preserve frozen GNU production/tests, exact byte captures, work/depth limits, cancellation, and single package.
- Grep and CommandFramework production code remain untouched.

## Review Focus

- `^` succeeds after internal LF, but not after a final LF at subject end.
- `$` succeeds before internal LF as well as at absolute end, without consuming LF.
- `\A`, `\Z`, and `\z` retain absolute-subject behavior under multiline mode.
- One leading `(?m)` has no capture and preserves caller `Caseless`; other flag placements still fail.
- Raw NUL/high bytes, capture rollback, direct quantifier diagnostics, and bounded/cancellable searches stay correct.

---

### Task 1: Test-first characterization

**Files:** `tests/Pcre/PcreByteLeadingMultilineTests.cs`, `tests/Pcre/PcreByteLeadingDotAllTests.cs`.

**Interfaces:** internal byte compiler, options, and byte-indexed match/captures.

- [ ] Add tests for every review case, including LF edge positions checked against PCRE2.
- [ ] Replace the prior `(?m)` unsupported assertion with another unsupported flag.
- [ ] Push test-only draft PR and observe semantic failures before implementation.

### Task 2: Compile and execute line anchors

**Files:** `src/Pcre/PcreByteParser.cs`, `src/Pcre/PcreByteSyntax.cs`, `src/Pcre/PcreByteProgram.cs`, `src/Pcre/PcreByteMatcher.cs`, `doc/PCRE-DESIGN.md`.

**Interfaces:** `PcreByteLineAnchor`, `PcreByteOpcode.StartLine`, `PcreByteOpcode.EndLine`.

- [ ] Parse one leading `(?m)`; direct repetition remains invalid; compile distinct line opcodes.
- [ ] Implement LF boundary predicates without consuming source bytes or changing work accounting.
- [ ] Verify six Staging platforms, three target frameworks, frozen corpus, parity, and package-only smoke.
- [ ] Request independent source review and leave the PR ready for user merge.
