# PCRE byte-mode noncapturing groups implementation plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add internal `(?:...)` grouping without consuming a capture number.

**Architecture:** Parser node for noncapturing bodies; compiler emits the body's alternatives directly into the existing ordered bytecode.

**Tech Stack:** C# 13, .NET 8/9/10, xUnit, GitHub Actions.

**Spec:** `doc/PCRE-BYTE-NONCAPTURING-DESIGN.md`

## Global Constraints

- Keep the API internal, preserving the GNU/POSIX package and Grep production behavior.
- Reject other `(?...)` constructs explicitly; preserve capture numbering and resource limits.

## Review Focus

- `(?:...)` must not allocate a capture slot even when nested or repeated.
- Nested captures retain source-order indices and backtrack correctly.
- Empty branches and optional groups must not loop indefinitely.
- An unclosed noncapturing group points at its `(`.
- Other special group prefixes remain unsupported.

---

### Task 1: Tests and implementation

**Files:** Create `tests/Pcre/PcreByteNoncapturingTests.cs`; modify `src/Pcre/PcreByteParser.cs`, `src/Pcre/PcreByteSyntax.cs`, `src/Pcre/PcreByteProgram.cs`, `doc/PCRE-DESIGN.md`.

- [ ] Write tests for grouped alternatives, repetition, capture numbering/backreferences, empty/optional groups, raw bytes, invalid/unclosed prefixes, and bounded work/cancellation.
- [ ] Run tests on a CI branch and observe the expected failure on currently unsupported `(?:` syntax.
- [ ] Add parser, AST and compiler support with no matcher changes.
- [ ] Run all six CI jobs, including the frozen 214-case GNU corpus and package/consumer gates.
- [ ] Review and commit the ready implementation.
