# UTF-8 General Categories Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Accept PCRE2's abbreviated Unicode general-category properties in the internal UTF-8 PCRE path.

**Architecture:** Retain the scalar-class opcode and bracket predicate path. Add a general-category selector with one-letter group or a specific `UnicodeCategory`, normalized at parse time and evaluated once per valid scalar.

**Tech Stack:** C# 13, `System.Globalization.UnicodeCategory`, `System.Text.Rune`, xUnit, net8/net9/net10.

**Spec:** `doc/PCRE-UTF8-GENERAL-CATEGORIES-DESIGN.md`

## Global Constraints

- Internal API only; byte mode, public API, Grep and CommandFramework production code unchanged.
- Root solution/project; `/src` implementation, `/tests` cases, `/doc` design and plan.
- Preserve source-byte spans, invalid UTF barriers, cancellation and bounded work.
- Staging matrix Linux/Windows/macOS x64/ARM64, net8/net9/net10, frozen GNU corpus and package smoke.

## Review Focus

- `\p{P}` uses Unicode punctuation only; POSIX `[[:punct:]]` also includes Latin-1 symbols in UCP.
- `\p{Z}` excludes ASCII tab/newline; `\s` includes them.
- `\p{Cs}` accepts valid syntax yet matches no valid UTF-8 scalar; `\P{Cs}` does match one.
- Bracket `[^\P{N}A]` applies the outer negation to the union after inner complement.
- Loose name parsing must not accidentally accept unrelated scripts, `\p{Any}` or invalid property types.

---

### Task 1: Characterize category properties

**Files:** Create `tests/Pcre/PcreUtf8GeneralCategoryTests.cs`; adjust pending N-property expectations in `tests/Pcre/PcreUtf8LetterPropertyTests.cs`, `PcreUtf8DigitTests.cs`, `PcreUtf8BracketUnionTests.cs`.

**Interfaces:** `PcreByteCompiler.Compile`, `PcreCompiledBytePattern.Match`, `PcreByteOptions.Utf8/Ucp/MatchInvalidUtf`.

- [ ] Add table-driven group and subcategory cases, spelling, complements, bracket unions, invalid UTF, diagnostics and work limits.
- [ ] Push test-only draft PR; verify build succeeds and Staging fails for unsupported categories.

### Task 2: Parse and evaluate categories

**Files:** Modify `src/Pcre/PcreByteSyntax.cs`, `PcreByteParser.cs`, `PcreByteProgram.cs`, `PcreByteMatcher.cs`.

**Interfaces:** `PcreByteScalarClass` and `PcreByteScalarPredicate` convey the normalized selector; scalar bytecode carries it through unchanged.

- [ ] Normalize the property name, select one-letter group or two-letter category, preserve existing negation and diagnostics.
- [ ] Evaluate category against one valid Rune; retain each predicate's charged work step and EOF/malformed barrier behavior.
- [ ] Verify targeted tests and full Staging on exact implementation head.

### Task 3: Review and delivery

**Files:** Update `doc/PCRE-DESIGN.md`.

- [ ] Document supported category subset and separate casefold/version gates.
- [ ] Obtain independent read-only review; address material issues; require final exact-head six green jobs before marking ready.
