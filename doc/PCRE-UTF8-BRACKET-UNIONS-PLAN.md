# UTF-8 Bracket Predicate Unions Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Union supported UTF-8 scalar predicates with literal ranges inside one PCRE bracket set.

**Architecture:** Extend the existing scalar-set AST/instruction with predicate terms and teach the UTF bracket parser to return literal or predicate items. Match a single valid scalar against OR terms with outer negation and per-term work accounting.

**Tech Stack:** C# 13, `System.Text.Rune`, xUnit, net8/net9/net10.

**Spec:** `doc/PCRE-UTF8-BRACKET-UNIONS-DESIGN.md`

## Global Constraints

- Internal API only; no runtime regex fallback, Grep migration, or CommandFramework production edit.
- Project/solution in root, source in `/src`, tests in `/tests`, design/plan in `/doc`.
- Preserve authoritative source-byte captures, invalid UTF barriers, cancellation, and bounded work.
- PR Staging on Linux/Windows/macOS x64/ARM64 and net8/net9/net10; frozen GNU and package smoke must pass.

## Review Focus

- `[^\W_]` and `[^[:^digit:]_]` apply the outer negation to a union, not separately to each member.
- POSIX digit uses UCP independently of `AsciiBsD`; `\d` respects `AsciiBsD` even in brackets.
- A negative predicate cannot match an invalid UTF byte or EOF; match one valid scalar only.
- Incomplete `[[.` and `[[=` stay literal; complete unsupported tokens diagnose explicitly.
- Invalid predicate range endpoints cannot be accepted as a literal `-` union; diagnostic points to the opening bracket.

---

### Task 1: Characterize scalar predicate unions

**Files:** Create `tests/Pcre/PcreUtf8BracketUnionTests.cs`; adjust pending unsupported cases in `tests/Pcre/PcreUtf8LiteralBracketTests.cs`, `PcreUtf8DigitTests.cs`, `PcreUtf8SpaceTests.cs`, `PcreUtf8ScalarTests.cs`, `PcreUtf8LetterPropertyTests.cs`.

**Interfaces:** `PcreByteCompiler.Compile`, `PcreCompiledBytePattern.Match`, `PcreByteOptions` (`Utf8`, `Ucp`, `AsciiBsD`, `MatchInvalidUtf`).

- [ ] Write byte-span fixtures for range/predicate unions, all positive and negative variants, both UCP settings, ASCII `\d`, existing POSIX standalone forms, invalid UTF, captures, resource limits, and unsupported/malformed syntax.
- [ ] Push tests on a draft PR and observe a Staging failure for the missing union behavior, not a test compilation error. Commit tests and design/plan.

### Task 2: Implement union parser and matcher

**Files:** Modify `src/Pcre/PcreByteParser.cs`, `PcreByteSyntax.cs`, `PcreByteProgram.cs`, `PcreByteMatcher.cs`.

**Interfaces:** `PcreByteScalarPredicate(PcreByteScalarClassKind Kind, bool Negated)`, `PcreByteScalarSet(Ranges, Predicates, Negated)`; bytecode `ScalarSet` carries both read-only arrays. Existing `PcreByteScalarClass` remains for standalone escapes.

- [ ] Parse all UTF bracket items into literal scalar or predicate. Reuse `ParseEscape` for supported shorthand/property kinds and preserve standalone POSIX behavior through the union parser.
- [ ] Reject nonliteral range endpoints and unsupported special forms without changing byte-mode parsing.
- [ ] Evaluate one decoded scalar against intervals and predicates, charge every term, apply outer negation, and verify targeted and full Staging jobs.

### Task 3: Review and delivery

**Files:** Update `doc/PCRE-DESIGN.md` feature matrix.

- [ ] Confirm exact PR head has six green Staging jobs, net8/net9/net10, frozen GNU and package smoke; verify clean diff.
- [ ] Obtain independent read-only review and address material findings before marking ready for the user's merge.
