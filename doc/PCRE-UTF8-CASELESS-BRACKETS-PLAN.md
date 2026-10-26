# UTF-8 Caseless Brackets Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Match supported UTF-8 ordinary bracket members under caseless mode while preserving byte spans and bounded match work.

**Architecture:** Preprocess literal ranges into compressed folded-key intervals with a transient Unicode bitmap and compile cancellation. Reuse the scalar-set opcode, folding the subject for its ranges and applying caseless category semantics to its predicates. Keep byte and caseful paths unchanged.

**Tech Stack:** C# 13, .NET 8/9/10, xUnit, Staging CI.

**Spec:** `doc/PCRE-UTF8-CASELESS-BRACKETS-DESIGN.md`

## Global Constraints

- Project file at root; source in `/src`, tests in `/tests`, samples in `/samples`.
- No Grep or CommandFramework production changes or public PCRE capability claim.
- Preserve authoritative byte spans, invalid-subject fragment barriers, work limits and cancellation.
- Staging PR matrix Linux/Windows/macOS x64/ARM64; all three .NET targets, GNU corpus and package smoke.

## Review Focus

- Canonicalizing non-contiguous source ranges must never fill a gap that was not in the original bracket: Task 1.
- `K/k/K`, `S/s/ſ`, Greek sigma and Turkish I are classified by the existing provisional equivalence: Task 1.
- A range crossing surrogate code points processes only valid scalars and stays cancellable: Task 1.
- `Lu`/`Ll`/`Lt` complements and POSIX lower/upper respect independent UCP policy: Task 2.
- Long unions and work-budget/cancellation cases cannot evade per-interval/per-predicate accounting: Task 2.

---

### Task 1: Caseless scalar literal ranges

**Files:** `src/Pcre/PcreUtf8CaseFold.cs`, `src/Pcre/PcreByteParser.cs`, `src/Pcre/PcreByteMatcher.cs`, `tests/Pcre/PcreUtf8CaselessBracketTests.cs`.

**Interfaces:** `PcreUtf8CaseFold.CloseRanges(IReadOnlyList<PcreByteScalarRange>, CancellationToken)` returns disjoint folded intervals; `PcreUtf8CaseFold.Fold(int)` returns canonical fold key.

- [ ] Write failing cases for literal/range folding, byte spans, negation, Unicode equivalence, surrogate-spanning range, budget and cancellation.
- [ ] Push test-only commit; observe Staging failures from the current explicit unsupported diagnostic.
- [ ] Implement compile-time folded ranges and runtime membership, remove only the UTF caseless bracket guard.
- [ ] Run all CI jobs, then commit.

### Task 2: Caseless bracket predicates and integration

**Files:** `src/Pcre/PcreByteMatcher.cs`, `tests/Pcre/PcreUtf8CaselessBracketTests.cs`, `tests/Pcre/PcreUtf8CaselessTests.cs`, `doc/PCRE-DESIGN.md`.

**Interfaces:** `ScalarClassContains(..., caseless: true)` applies cased category closure to bracket properties and UCP POSIX lower/upper; ASCII POSIX lower/upper remain ASCII only.

- [ ] Add red tests for Unicode properties, POSIX lower/upper with UCP on/off, other shorthands and predicate complements.
- [ ] Implement predicate semantics, adjust old unsupported fixture, update design matrix.
- [ ] Verify exact final head on all six CI jobs and review for scope and performance hazards.
- [ ] Independent read-only review, address important issues, and mark PR ready.
