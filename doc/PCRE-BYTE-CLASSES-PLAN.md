# PCRE Byte Classes Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add ordinary bracket classes and ASCII POSIX class items to the internal PCRE byte matcher.

**Architecture:** Parse bracket elements into an immutable 256-bit union, complement for leading `^`, and reuse the existing class opcode. Keep range and POSIX token rules confined to the parser; the VM and public package contract do not change.

**Tech Stack:** C# 13, .NET 8/9/10, xUnit, existing single `Icod.Expressions` library.

**Spec:** [`doc/PCRE-BYTE-CLASSES-DESIGN.md`](PCRE-BYTE-CLASSES-DESIGN.md).

## Global Constraints

- Root solution/project files in `/`, implementation under `/src`, tests under `/tests`, samples under `/samples`.
- Internal API and 8-bit non-UTF, non-UCP behavior only. No PCRE.NET or `System.Text.RegularExpressions` runtime dependency.
- Preserve the frozen GNU source and 214 exact tests, per-match 1,000,000-step default work budget, 1,024 backtrack-frame cap and cancellation.
- Grep remains last. Do not change Grep or remove CommandFramework production regex code.

## Review Focus

1. `[^a]` matches LF, NUL and 0xFF; an ordinary dot does not match LF. Task 1 tests both.
2. `[]]`, `[a-]`, `[-a]`, and `[\]]` distinguish first/last literal punctuation from a range. Task 1 tests each.
3. `[z-a]` and `[a-\d]` fail at compile time without silently literalizing a bad range. Task 1 tests both.
4. `[[:digit:]]` is ASCII in byte mode; POSIX negation and combination with literals/shorthands preserve the union before outer negation. Task 2 tests this.
5. A class followed by `+` uses existing greedy choice and work limits; Task 1 tests capture rollback and a small budget.

---

### Task 1: Literal, range, negated, escaped and shorthand class items

**Files:**
- Modify: `src/Pcre/PcreByteSet.cs`, `src/Pcre/PcreByteParser.cs`
- Create: `tests/Pcre/PcreByteBracketTests.cs`
- Modify: `doc/PCRE-DESIGN.md` (supported syntax status after implementation)

**Interfaces:**
- `PcreByteSet` gains immutable `Singleton(byte)`, `Range(byte, byte)` and `Union(PcreByteSet)` operations.
- `PcreByteParser.ParseAtom(int)` returns `PcreByteClass` for `[ ... ]`. Existing `ParseEscape(int)` supplies byte literals or sets, with a class-only `\b` exception.

- [ ] Write PCRE2-pinned failing tests for simple/negated sets, byte ranges, literal `]` and hyphen, escaped endpoints and high bytes, `\d` union, malformed classes, captures and bounded work.
- [ ] Push a test-only commit and observe the expected failures in PR CI (local `dotnet` is unavailable).
- [ ] Parse class items/ranges with cancellation, explicit diagnostics and one immutable set. Make `]` outside a class literal in default mode.
- [ ] Push the implementation; verify focused tests and the full CI/parity/package workflow.

### Task 2: ASCII POSIX named items

**Files:**
- Modify: `src/Pcre/PcreByteSet.cs`, `src/Pcre/PcreByteParser.cs`, `tests/Pcre/PcreByteBracketTests.cs`, `doc/PCRE-DESIGN.md`

**Interfaces:**
- The class parser recognizes `[:name:]` and `[:^name:]` as atomic set items when `[` occurs inside a class; it unions them with ordinary items.
- `PcreByteSet` provides the fourteen ASCII non-UCP POSIX sets listed in the design.

- [ ] Write failing fixtures for `[[:digit:]]`, `[[:^digit:]]`, `[[:word:]_]`, `[^[:digit:]]`, all supported names at boundary bytes, unknown names, malformed tokens and invalid ranges adjacent to POSIX items.
- [ ] Push the test-only commit and observe the expected failures.
- [ ] Implement the POSIX item recognizer and fixed ASCII set definitions without changing the VM or Unicode behavior.
- [ ] Run all cross-platform framework/package/parity gates, request independent review, and keep the PR stacked on byte-atoms until #9 merges.
