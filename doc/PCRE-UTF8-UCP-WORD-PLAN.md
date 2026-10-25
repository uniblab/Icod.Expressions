# Internal PCRE UTF-8 Word Classes and Boundaries Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Match PCRE2 UTF-8 UCP word classes and zero-width boundaries over Unicode scalars with byte-accurate captures.

**Architecture:** Extend the existing scalar-class opcode with word predicates, and encode the boundary's UCP classification in the boundary instruction. The matcher decodes the preceding scalar from its leading byte only at assertion time. Existing byte-mode instructions remain unchanged.

**Tech Stack:** C# 13, .NET 8/9/10, xUnit, GitHub Actions Staging matrix.

**Spec:** `doc/PCRE-UTF8-UCP-WORD-DESIGN.md`

## Global Constraints

- Project/solution live in `/`, source in `/src`, tests in `/tests`, samples in `/samples`.
- Match and capture offsets are bytes; both work limit and cancellation remain effective.
- No production change to Grep or CommandFramework; no public PCRE API claim.
- No local .NET SDK in this workspace: publish a test-first red commit to a draft PR and use Staging CI to observe failure, then green commits and full matrix verification.

## Review Focus

- Nonspacing mark and letter adjacency must be nonboundary under UCP.
- Number-letter adjacency uses N, including nondecimal numeric categories.
- A supplementary scalar immediately before a boundary is decoded from its lead byte.
- A nonzero start offset still sees the preceding scalar.
- Empty subject, negated class at end and abandoned capture branches retain their existing semantics.

---

### Task 1: Word predicates and boundary semantics

**Files:**
- Create: `tests/Pcre/PcreUtf8WordTests.cs`
- Modify: `src/Pcre/PcreByteParser.cs`, `src/Pcre/PcreByteSyntax.cs`, `src/Pcre/PcreByteProgram.cs`, `src/Pcre/PcreByteMatcher.cs`
- Modify: `tests/Pcre/PcreUtf8DigitTests.cs`, `doc/PCRE-DESIGN.md`

**Interfaces:**
- Consumes: `PcreByteCompiler.Compile(ReadOnlyMemory<byte>, PcreByteOptions?, CancellationToken)` and `PcreCompiledBytePattern.Match(ReadOnlyMemory<byte>, int, CancellationToken)`.
- Produces: internal `PcreByteScalarClassKind.AsciiWord`/`UnicodeWord` and UCP-aware `PcreByteWordBoundary`, with no new public API.

- [ ] **Step 1: Write failing tests** for UCP `\w`/`\W` matching UTF letters, combining marks, numeric categories, connectors, excluding symbols; non-UCP ASCII `\w` and scalar `\W`; UCP `\b`/`\B` start, end, mark adjacency, emoji transitions, supplementary predecessor and nonzero offset; rollback/capture, work/cancellation, and 8-bit regression. Retire the prior UCP rejection fixture only with replacement positive assertions.
- [ ] **Step 2: Run focused tests on Staging** with a test-only commit in a draft PR. Expected: current UCP `\b` rejection or UTF `\w` rejection fails at least one new test; report the actual failure.
- [ ] **Step 3: Implement scalar word classification and UCP-aware boundary opcode**. Use `Rune.GetUnicodeCategory`, category L/N/Mn/Pc; walk backward over UTF continuation bytes to the previous lead, and decode that scalar. Keep one `Step()` for each inspected side, including cancellation and budget.
- [ ] **Step 4: Run full Staging tests, frozen GNU corpus and package smoke** across ubuntu/windows/macos x64/ARM64 and net8/net9/net10. Expected: all six jobs green, with no unrelated regressions.
- [ ] **Step 5: Review the full branch, fix material findings with a failing test first, rerun the full matrix, then mark the PR ready.**
