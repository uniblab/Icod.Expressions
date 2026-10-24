# PCRE Byte Backreferences Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add internal single-digit numbered backreferences to the managed PCRE byte matcher.

**Architecture:** Parse `\1`–`\9` into a capture-number node, validate references after the entire parse, emit one bytecode instruction and compare the saved capture span under the existing work and cancellation budget. Keep capture rollback in the existing backtracking snapshots.

**Tech Stack:** C# 13, .NET 8/9/10, xUnit, single `Icod.Expressions` library.

**Spec:** [`doc/PCRE-BYTE-BACKREFERENCES-DESIGN.md`](PCRE-BYTE-BACKREFERENCES-DESIGN.md).

## Global Constraints

- Root solution/project in `/`, implementation in `/src`, tests in `/tests`, samples in `/samples`.
- Internal 8-bit, non-UTF, non-UCP API only. No runtime PCRE.NET or `System.Text.RegularExpressions` dependency.
- Preserve the frozen GNU source and 214 exact tests, 1,000,000-step default work budget, 1,024 backtrack-frame cap, cancellation, and caller-pattern snapshot.
- Grep remains last; do not migrate it or remove CommandFramework regex production code.

## Review Focus

1. `(a)?b\1` fails on `b` because the group is unset; focused tests pin this.
2. `()\1` succeeds with an empty but set capture; focused tests pin this.
3. `(a|ab)\1` backtracks to the `ab` capture on `abab`; focused tests pin this.
4. A long captured span cannot bypass the work budget or cancellation; focused tests pin this.
5. `\10` is rejected rather than parsed as `\1` and a literal `0`; focused tests pin this.

---

### Task 1: Parse and execute one-digit byte backreferences

**Files:**
- Modify: `src/Pcre/PcreByteSyntax.cs`, `src/Pcre/PcreByteParser.cs`, `src/Pcre/PcreByteProgram.cs`, `src/Pcre/PcreByteMatcher.cs`
- Create: `tests/Pcre/PcreByteBackreferenceTests.cs`
- Modify: `doc/PCRE-DESIGN.md`

**Interfaces:**
- `PcreByteBackreference(int Number)` is a syntax node; `PcreByteOpcode.Backreference` carries its group number in `First`.
- `PcreByteParser.Parse()` validates recorded (number, byteOffset, priorGroupCount) references after the whole pattern is parsed. Forward single-digit references are allowed if the final capture count contains that number.
- `PcreByteMatcher.Match(...)` checks `captures[number]` and compares every referenced byte through `Step()`; unset captures fail, empty captures succeed.

- [ ] Write failing PCRE2-pinned tests for the Grep fixture, alternation rollback, unset/empty captures, last repeated capture, forward validation and numeric ambiguity.
- [ ] Push a test-only commit and observe expected failures in PR CI, since local `dotnet` is unavailable.
- [ ] Implement AST, parse validation, bytecode and bounded comparison without changing the public API or other escape semantics.
- [ ] Push implementation; verify focused tests and all platform/framework/package/consumer checks.
- [ ] Request independent review, resolve findings, and keep the PR stacked on byte classes until #10 merges.
