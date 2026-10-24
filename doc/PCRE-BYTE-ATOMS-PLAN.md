# PCRE Byte Atoms Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Extend the internal PCRE byte matcher with dot, literal escapes and ASCII shorthand character types without switching any consumer.

**Architecture:** Parse each supported atom into an existing literal node or a new immutable 256-bit set node. Emit a class instruction to the explicit backtracking VM; it consumes one authoritative byte and uses the same resource and cancellation accounting as a literal. Unknown PCRE syntax stays a compile diagnostic.

**Tech Stack:** C# 13, .NET 8/9/10, xUnit, existing single-library project.

**Spec:** [`doc/PCRE-BYTE-ATOMS-DESIGN.md`](PCRE-BYTE-ATOMS-DESIGN.md).

## Global Constraints

- Root solution/project files remain in `/`, source in `/src`, tests in `/tests`, samples in `/samples`.
- Internal API only; no `PCRE.NET` or `System.Text.RegularExpressions` runtime path.
- Byte-mode default LF newline and non-UCP ASCII types only; no UTF, options or bracket classes in this PR.
- GNU BRE/ERE frozen source/tests and its public behavior remain untouched; Grep stays last and CommandFramework production code is not removed.
- Preserve the compiled pattern snapshot, per-match 1,000,000-step default work budget, 1,024 saved-path cap and cancellation checks.

## Review Focus

1. A dot on LF fails but on CR or NUL matches one byte; add cases in Task 1.
2. `\xFG` consumes only `F` as a hex digit, leaving `G` literal; add a case in Task 1.
3. `\D` and `\W` include 0xFF in byte mode while `\d` and `\w` do not; add cases in Task 2.
4. `\q`, backreferences and braced hex must fail explicitly, while an escaped punctuation byte is literal; add cases in Task 1 and Task 2.
5. Quantified sets use the existing ordered backtracking and work limit; add a capture and a small-budget case in Task 2.

---

### Task 1: Byte set opcode, dot and literal escapes

**Files:**
- Create: `src/Pcre/PcreByteSet.cs`
- Modify: `src/Pcre/PcreByteSyntax.cs`, `src/Pcre/PcreByteParser.cs`, `src/Pcre/PcreByteProgram.cs`, `src/Pcre/PcreByteMatcher.cs`
- Create: `tests/Pcre/PcreByteAtomTests.cs`
- Modify: `tests/Pcre/PcreByteFoundationTests.cs` (replace the old `\xFF` unsupported assertion)

**Interfaces:**
- Internal `PcreByteSet.Contains(byte)` over four immutable `ulong` words; `PcreByteClass(PcreByteSet Set)` AST node.
- The existing `PcreByteCompiler.Compile(ReadOnlyMemory<byte>, PcreByteOptions?, CancellationToken)` and compiled `Match(ReadOnlyMemory<byte>, int, CancellationToken)` signatures stay unchanged.

- [ ] Write failing PCRE2-pinned tests for `.`, `\n`, `\t`, `\\`, `\.` and `\|`, `\x`, `\xF`, `\xFF`, `\xFG`, trailing slash, braced hex, `\q`, quantified dot and capture byte spans.
- [ ] Push the test-only commit and verify expected failure in the PR CI (local `dotnet` is unavailable in this workspace).
- [ ] Add the immutable byte set and class instruction; parse dot, non-alphanumeric literal escapes, control escapes and zero-to-two-digit hex. Preserve explicit diagnostics for pending escapes.
- [ ] Push and verify the focused tests, full suite, frozen 214-case parity, and single-library package checks on all target frameworks/platforms; commit the implementation.

### Task 2: ASCII shorthand classes

**Files:**
- Modify: `src/Pcre/PcreByteSet.cs`, `src/Pcre/PcreByteParser.cs`, `tests/Pcre/PcreByteAtomTests.cs`, `doc/PCRE-DESIGN.md`

**Interfaces:**
- Reuse `PcreByteClass(PcreByteSet)` and the class opcode; factory sets for ASCII `\d`, `\s`, `\w` and their byte complements.

- [ ] Write failing PCRE2-pinned tests for digit, word and space boundaries, 0xFF complements, capture plus quantifier ordering, and the match-work diagnostic with a small limit.
- [ ] Push the test-only commit and verify expected CI failures.
- [ ] Implement the factory sets and parser mapping without adding a runtime reference engine; keep `\p`, `\h`, `\v` and options unsupported.
- [ ] Run the full cross-platform CI/package/parity gates and request independent review. Leave the PR stacked on the foundation until that PR merges.
