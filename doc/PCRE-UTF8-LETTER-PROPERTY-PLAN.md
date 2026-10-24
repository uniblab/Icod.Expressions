# UTF-8 Letter Property Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Internal UTF-8 `\p{L}` and negated forms with byte-accurate spans and bounded execution.

**Architecture:** Parse only the `L` property spellings into a scalar-property node and instruction. Evaluate the category at each decoded UTF-8 scalar, preserving existing match state and byte offsets.

**Tech Stack:** C# 13, `System.Text.Rune`, `System.Globalization.UnicodeCategory`, xUnit, net8/net9/net10.

**Spec:** `doc/PCRE-UTF8-LETTER-PROPERTY-DESIGN.md`

## Global Constraints

- Internal API only. No runtime regex fallback, Grep or CommandFramework edits, and no change to default byte mode.
- Keep unsupported property names explicit; account for cancellation and per-match work.
- Use `/src`, `/tests`, `/doc` and all six Staging CI jobs.

## Review Focus

- Supplementary letter and nonletter scalars preserve byte spans and do not split UTF-8 sequences.
- Negation and double negation include numbers, marks, emoji and empty-subject behavior.
- Captures and backtracking restore state after a failed letter path.
- Malformed braces and pending valid property names fail with distinct diagnostics at the slash offset.
- Budget and cancellation interrupt a long property match.

---

### Task 1: Characterize the property contract

**Files:** Add `tests/Pcre/PcreUtf8LetterPropertyTests.cs`; adjust the earlier pending-property assertion in `tests/Pcre/PcreUtf8ScalarTests.cs`.

**Interfaces:** `PcreByteCompiler.Compile(pattern, new PcreByteOptions { Utf8 = true })`, `Pattern.Match(subject)`.

- [ ] Write PCRE2-derived span fixtures for `\p{L}`, `\P{L}`, `\p{^L}`, `\P{^L}`, `\pL` and `\PL`, including Unicode category edges, quantification and capture rollback.
- [ ] Write malformed/unsupported syntax and budget/cancellation fixtures.
- [ ] Run hosted Staging and verify the new behavior fails before implementation, then commit.

### Task 2: Implement one Unicode property atom

**Files:** Modify `src/Pcre/PcreByteSyntax.cs`, `PcreByteParser.cs`, `PcreByteProgram.cs`, `PcreByteMatcher.cs`.

**Interfaces:** `PcreByteLetterProperty(bool Negated)` emits a scalar property instruction; matcher decodes and tests `UnicodeCategory`.

- [ ] Parse only supported L spellings and reject invalid/pending names deliberately.
- [ ] Match full UTF-8 scalars, account work and cancellation, and preserve byte spans.
- [ ] Run the full Staging matrix, package smoke and frozen GNU checks, then commit.

### Task 3: Review and deliver

**Files:** Update `doc/PCRE-DESIGN.md` status.

- [ ] Review the diff against the design and run fresh six-job verification.
- [ ] Request independent read-only review and address material findings.
- [ ] Ready the PR for the user's review and merge.
