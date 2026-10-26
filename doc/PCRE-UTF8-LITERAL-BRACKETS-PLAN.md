# UTF-8 Literal Brackets Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add internal, case-sensitive UTF-8 literal bracket sets and inclusive scalar ranges with authoritative byte spans.

**Architecture:** Parse literal scalar intervals into a dedicated immutable AST/bytecode operand; match one decoded scalar within the current valid fragment. Preserve existing standalone POSIX brackets and byte-mode classes.

**Tech Stack:** C# 13, `System.Text.Rune`, xUnit, net8/net9/net10, six Staging jobs.

**Spec:** `doc/PCRE-UTF8-LITERAL-BRACKETS-DESIGN.md`

## Global Constraints

- Internal API only; no Grep or CommandFramework production changes and no runtime regex fallback.
- Root project/solution, source in `/src`, tests in `/tests`, plans in `/doc`.
- The original byte spans, fragment barriers, distinct diagnostics, work limits, and cancellation must remain intact.
- PR Staging must pass Linux/Windows/macOS x64/ARM64, net8/net9/net10, frozen GNU corpus, and package smoke.

## Review Focus

- Negated scalar classes must not match invalid UTF bytes or the end of input; test invalid barriers and an empty subject.
- A range spanning supplementary code points must advance one scalar while returning four bytes; test both ends.
- A literal closing bracket in first position and hyphens after a range must retain PCRE2 meanings; test each.
- Mixed POSIX, shorthand/property escapes, and descending/missing ranges must diagnose at the opening bracket; test each.
- A failed class inside a captured alternative must roll back captures; test an ordered fallback.

---

### Task 1: Characterize UTF literal brackets

**Files:** Create `tests/Pcre/PcreUtf8LiteralBracketTests.cs`; modify `tests/Pcre/PcreUtf8ScalarTests.cs` pending unsupported assertion if needed.

**Interfaces:** `PcreByteCompiler.Compile(ReadOnlyMemory<byte>, PcreByteOptions?)`, `PcreCompiledBytePattern.Match(ReadOnlyMemory<byte>, startOffset: ...)`, `PcreByteCapture`.

- [ ] Write xUnit fixtures for positive/negative sets, raw scalar and escaped scalar values, ranges, punctuation, bracket/hyphen placements, quantifiers, captures, malformed subject barriers, diagnostics, budget, and cancellation. Keep existing standalone POSIX fixtures.
- [ ] Run targeted tests on the hosted Staging matrix before implementation; observe a behavioral red `UnsupportedSyntax`, not a test compile error. Commit tests and design/plan.

### Task 2: Implement scalar range instruction

**Files:** Modify `src/Pcre/PcreByteParser.cs`, `src/Pcre/PcreByteSyntax.cs`, `src/Pcre/PcreByteProgram.cs`, `src/Pcre/PcreByteMatcher.cs`.

**Interfaces:** AST `PcreByteScalarSet` carries `ReadOnlyMemory<PcreByteScalarRange>` and `Negated`; instruction `ScalarSet` carries the same immutable operand; matcher decodes one scalar inside `fragment` and checks inclusive ranges.

- [ ] Dispatch existing standalone UTF POSIX brackets as before and parse ordinary literal scalar sets/ranges with the explicitly supported syntax. Reject unsupported unions and escapes, malformed and descending ranges.
- [ ] Compile one scalar-set opcode; match one decoded scalar, consume its byte width, and count an attempted comparison against the work budget.
- [ ] Run the targeted suite and the full hosted Staging jobs; correct any failures on the PR.

### Task 3: Review and deliver

**Files:** Update `doc/PCRE-DESIGN.md` feature matrix.

- [ ] Confirm diff and the final six Staging jobs on the exact PR head; frozen GNU, package-only consumer, and parity checks must pass.
- [ ] Request independent read-only review, resolve material findings, then mark the PR ready for the user's merge.
