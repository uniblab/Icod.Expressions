# UTF-8 Caseless Literals Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add UTF-8 caseless scalar literals and cased-letter properties without changing unsupported caseless syntax silently.

**Architecture:** Preserve the existing byte-mode folding path. Add a UTF scalar fold helper used only for UTF literal instructions, and evaluate `Lu`/`Ll`/`Lt` as `Lc` under UTF caseless. Reject UTF caseless brackets, backreferences and assertions at their pattern byte offsets.

**Tech Stack:** C# 13, .NET 8/9/10, xUnit, GitHub Staging CI.

**Spec:** `doc/PCRE-UTF8-CASELESS-LITERALS-DESIGN.md`

## Global Constraints

- Project file at root, source under `/src`, tests under `/tests`, samples under `/samples`.
- No Grep or CommandFramework production changes; no public PCRE capability claim.
- PR builds Staging on Linux, Windows and macOS, x64 and ARM64; local Debug when available.
- Keep authoritative byte offsets and bounded work/cancellation behavior.

## Review Focus

- Different UTF widths for equivalent literal scalars preserve subject byte offsets and lengths: Task 1.
- Dotted and dotless I remain isolated in the default PCRE2 casing mode: Task 1.
- `Lu`/`Ll`/`Lt` complements apply the shared cased-letter set, even without UCP: Task 2.
- UTF caseless brackets and references fail with diagnostics rather than silently giving wrong matches: Task 2.
- Malformed subject fragments do not join across invalid bytes: Task 2.

---

### Task 1: Literal case equivalence

**Files:** `src/Pcre/PcreByteCompiler.cs`, `src/Pcre/PcreByteParser.cs`, `src/Pcre/PcreByteMatcher.cs`, `src/Pcre/PcreUtf8CaseFold.cs`, `tests/Pcre/PcreUtf8CaselessTests.cs`.

**Interfaces:** `PcreUtf8CaseFold.Equals(int left, int right)`; existing `PcreByteProgram.Caseless` and `Utf8` flags.

- [ ] Add literal fixtures for leading/caller mode, Unicode equivalences, Turkish I, exact byte spans and caseful contrast.
- [ ] Push test-only commit and verify expected Staging test failures for absent UTF caseless support.
- [ ] Enable leading and caller mode, implement scalar fold for UTF literals, keep byte-mode path intact.
- [ ] Verify suite on exact implementation head.
- [ ] Commit.

### Task 2: Properties, guardrails and budget

**Files:** `src/Pcre/PcreByteParser.cs`, `src/Pcre/PcreByteMatcher.cs`, `tests/Pcre/PcreUtf8CaselessTests.cs`, `tests/Pcre/PcreUtf8ScalarTests.cs`, `doc/PCRE-DESIGN.md`.

**Interfaces:** `ScalarClassContains(..., bool caseless = false)` applies `Lc` to specific cased categories; unsupported syntax carries byte offset.

- [ ] Add tests for three cased properties and negations, independent UCP, rejected brackets/references/assertions, invalid fragments, work budget and cancellation.
- [ ] Verify the tests fail for missing behavior.
- [ ] Implement property handling and explicit diagnostics; adjust the prior unsupported expectations.
- [ ] Run all Staging jobs, full net8/net9/net10 suite, frozen GNU and package smoke.
- [ ] Review diff for scope, clean state and documentation; commit and prepare PR against the dependency branch until #34 merges.
