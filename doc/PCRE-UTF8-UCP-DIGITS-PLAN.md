# UTF-8 UCP Decimal Digits Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Match Grep's ASCII `\d`, Unicode POSIX digit, and leading `(?-aD)` override while adding UTF `\p{Nd}`.

**Architecture:** Represent ASCII and Unicode decimal digit predicates over decoded scalars. Parse only the standalone POSIX digit bracket and leading `aD` option settings needed by the consumer fixture. Generalize the existing L property node to L/Nd without widening unrelated syntax.

**Tech Stack:** C# 13, `System.Text.Rune`, `System.Globalization.UnicodeCategory`, xUnit, net8/net9/net10.

**Spec:** `doc/PCRE-UTF8-UCP-DIGITS-DESIGN.md`

## Global Constraints

- Internal API only, no runtime regex fallback, no Grep/CommandFramework edits, default byte mode unchanged.
- Exact byte spans, distinct diagnostics for unsupported syntax, cancellation and match-work limits.
- Layout `/src`, `/tests`, `/doc`; six hosted Staging jobs plus frozen GNU and package smoke.

## Review Focus

- Arabic-Indic/fullwidth Nd and numeric non-Nd characters distinguish Unicode decimal from ASCII digits.
- `\D`, `\P{Nd}` and nested POSIX/outer negation consume whole scalars and do not match EOF.
- The caller ASCII setting and leading `(?-aD)` override compose with ordered alternatives and capture rollback.
- `AsciiBsD` never changes POSIX or explicit `\p{Nd}`; UCP without UTF and mixed UTF brackets fail explicitly.
- Long property-class input respects match budget and cancellation; byte-mode classes retain their previous behavior.

---

### Task 1: Freeze the consumer digit contract

**Files:** Add `tests/Pcre/PcreUtf8DigitTests.cs`; adjust pending `\p{Nd}` assertions in `tests/Pcre/PcreUtf8ScalarTests.cs` and `tests/Pcre/PcreUtf8LetterPropertyTests.cs`.

**Interfaces:** `new PcreByteOptions { Utf8 = true, Ucp = true, AsciiBsD = true }`, `PcreByteCompiler.Compile`, `Pattern.Match`.

- [ ] Write PCRE2-derived byte-span fixtures for `\d`, `\D`, `\p{Nd}`, POSIX digit and both aD settings, including Grep's Arabic-Indic input.
- [ ] Write negation, capture/backtracking, malformed/unsupported and work/cancellation fixtures.
- [ ] Run hosted tests and confirm the new behavior is red before implementation; commit tests.

### Task 2: Implement scalar digit options and syntax

**Files:** Modify `src/Pcre/PcreByteOptions.cs`, `PcreByteCompiler.cs`, `PcreByteParser.cs`, `PcreByteSyntax.cs`, `PcreByteProgram.cs`, `PcreByteMatcher.cs`.

**Interfaces:** Internal scalar predicate node carries kind (L, Nd or ASCII digit) and negation; program compiles it to a scalar predicate instruction.

- [ ] Parse the compile options and one leading aD setting; preserve existing flag behavior.
- [ ] Parse UTF digit escapes, Nd property, standalone POSIX digit brackets; reject other UTF bracket syntax explicitly.
- [ ] Evaluate one scalar per predicate with byte spans, cancellation and work budget; preserve byte-mode code path.
- [ ] Run hosted Staging, frozen GNU and package smoke; commit.

### Task 3: Review and deliver

**Files:** Update `doc/PCRE-DESIGN.md`.

- [ ] Review diff against design and obtain six green Staging jobs on final head.
- [ ] Request independent read-only review, fix material findings, and ready PR for user merge.
