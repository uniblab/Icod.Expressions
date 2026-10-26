# PCRE2 Reference Conformance Fixture Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add a pinned, test-only PCRE2 reference comparison for supported whole-match byte spans.

**Architecture:** Add `PCRE.NET` 1.6.0 to the test project only. Use `PcreRegex8Bit` with Grep-equivalent options for table cases, compare success and authoritative byte span against the internal compiler/matcher, and retain the existing package smoke as a dependency-closure gate.

**Tech Stack:** C# 13, .NET 8/9/10, xUnit, PCRE.NET 1.6.0 (tests only), Staging CI.

**Spec:** `doc/PCRE-REFERENCE-CONFORMANCE-DESIGN.md`

## Global Constraints

- Solution/project root, `/src`, `/tests`, `/samples` layout; single library package.
- No production PCRE.NET or `System.Text.RegularExpressions` path, no Grep or CommandFramework production change.
- Six Staging OS/architecture jobs, three frameworks, frozen 214-case corpus and package-only smoke.
- User requested no subagents; self-review the full diff and results.

## Review Focus

- Compare raw bytes rather than strings after decoding: Task 1.
- Match Grep's UTF/UCP/ASCII-`\d` option combination: Task 1.
- Start offsets after invalid-UTF barriers must remain source-byte offsets: Task 1.
- Native reference assets must run on all six RIDs: Task 1.
- Packed library must remain a single managed DLL without a PCRE dependency: Task 1.

---

### Task 1: Whole-match reference matrix

**Files:** `tests/Icod.Expressions.Tests.csproj`, `tests/Pcre/PcreReferenceConformanceTests.cs`, `doc/PCRE-DESIGN.md`.

**Interfaces:** `PcreByteCompiler.Compile(ReadOnlyMemory<byte>, PcreByteOptions?, CancellationToken)` and `PcreRegex8Bit(ReadOnlyMemory<byte>, Encoding, PcreRegexSettings)`; compare `Match(..., startOffset)` success/byte index/length.

- [ ] Add a pinned test-only package reference and focused byte/UTF table, including braced escape whitespace, caseless, counted and invalid-subject cases.
- [ ] Push a draft PR and run Staging. Verify tests actually exercise the native reference rather than only the internal matcher.
- [ ] Investigate and resolve any mismatch at the option/semantic source; keep unresolved cases visible and the PR draft.
- [ ] Verify exact final head on all six jobs, three frameworks, frozen corpus and package smoke; self-review and mark ready.
