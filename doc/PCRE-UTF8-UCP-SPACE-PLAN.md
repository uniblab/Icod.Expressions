# Internal PCRE UTF-8 Whitespace and POSIX Classes Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Match UTF-8 `\s`/`\S` and standalone POSIX `space`/`word` under UCP while preserving the Grep backreference fixture’s byte captures.

**Architecture:** Add ASCII and UCP whitespace kinds to the scalar-class opcode; generalize the existing standalone UTF POSIX digit parser to dispatch `digit`, `space`, and `word` to existing scalar predicates. No general UTF bracket set or public API is introduced.

**Tech Stack:** C# 13, .NET 8/9/10, xUnit, six-job GitHub Actions Staging.

**Spec:** `doc/PCRE-UTF8-UCP-SPACE-DESIGN.md`

## Global Constraints

- Project/solution in `/`, source in `/src`, tests in `/tests`, samples in `/samples`.
- Preserve source-byte spans, cancellation, work limits, and strict UTF subject handling.
- No production changes to Grep or CommandFramework; no public PCRE claim.
- No local .NET SDK in this workspace: use a test-first draft PR to observe RED in Staging, then full six-job GREEN and package smoke.

## Review Focus

- UCP `\s` includes U+0085 and U+180E as well as all separator categories; non-UCP excludes them.
- POSIX `word` matches the same L/N/Mn/Pc set as `\w` under UCP.
- `AsciiBsD` never narrows POSIX `space` or `word`.
- Doubled outer/POSIX negation and EOF do not create a zero-width consuming match.
- Grep’s `^(\w+)\s+\1$` UTF fixture preserves capture bytes and backtracking.

---

### Task 1: Whitespace and standalone named brackets

**Files:**
- Create: `tests/Pcre/PcreUtf8SpaceTests.cs`
- Modify: `src/Pcre/PcreByteSyntax.cs`, `src/Pcre/PcreByteParser.cs`, `src/Pcre/PcreByteMatcher.cs`, `doc/PCRE-DESIGN.md`

**Interfaces:**
- Consumes: `PcreByteCompiler.Compile(ReadOnlyMemory<byte>, PcreByteOptions?, CancellationToken)` and `PcreCompiledBytePattern.Match(ReadOnlyMemory<byte>, int, CancellationToken)`.
- Produces: internal `PcreByteScalarClassKind.AsciiSpace`/`UnicodeSpace`; standalone UTF POSIX dispatch for `digit`, `space`, `word`.

- [ ] **Step 1: Write failing tests** for UCP/non-UCP whitespace and complements; standalone positive/negative POSIX word/space, outer negation and digit independence; Grep `^(\w+)\s+\1$` byte captures; EOF, rollback, and work/cancellation.
- [ ] **Step 2: Run test-only draft PR in Staging.** Expected: new `\s` or `[[:word:]]` tests fail with `UnsupportedSyntax`, while the existing suite builds.
- [ ] **Step 3: Implement scalar space predicate and POSIX dispatch.** UnicodeSpace is Zs/Zl/Zp plus HT/LF/VT/FF/CR, U+0085, U+180E. AsciiSpace is the six C-locale whitespace scalars.
- [ ] **Step 4: Run full Staging matrix and package/parity/frozen corpus checks.** Expected: all six jobs green, with net8/net9/net10 full and GNU corpus passing.
- [ ] **Step 5: Run independent whole-branch review; fix material findings test-first, repeat verification, and mark PR ready.**
