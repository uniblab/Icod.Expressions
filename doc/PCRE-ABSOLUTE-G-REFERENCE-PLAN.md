# Absolute `\g` Backreference Implementation Plan

> **For agentic workers:** Use superpowers:executing-plans to implement this plan task by task. No subagents.

**Goal:** Accept the unambiguous absolute numeric `\g` backreference forms within the internal 256-capture ceiling.

**Architecture:** Add one operand parser to the existing escape path and reuse `PcreByteBackreference` and the existing matcher. Pin byte spans against the test-only PCRE2 wrapper and keep other `\g` families rejected.

**Tech Stack:** C# 13, .NET 8/9/10, xUnit, pinned PCRE.NET test dependency, six Staging jobs.

**Spec:** `doc/PCRE-ABSOLUTE-G-REFERENCE-DESIGN.md`

## Global Constraints

- PCRE stays internal; no Grep or CommandFramework production edits.
- The parser and VM remain source-byte based, with no runtime PCRE dependency.
- Group references must remain bounded by the current 256-capture limit.

## Review Focus

- `\g{1}2` refers to group 1 then matches the literal `2`; `\g12` reads 12 as one operand.
- `\g{10}` reads the tenth group without octal ambiguity.
- Unset and forward references retain the current journal and post-parse validation semantics.
- A nonpositive, nonexistent or malformed absolute operand reports a diagnostic at its backslash.
- Relative, named and subroutine syntax still reports `UnsupportedSyntax`.

---

### Task 1: Absolute numeric escape

**Files:**
- Modify: `tests/Pcre/PcreReferenceConformanceTests.cs`, `tests/Pcre/PcreByteBackreferenceTests.cs`, `tests/Pcre/PcreSyntaxAcceptanceTests.cs`
- Modify: `src/Pcre/PcreByteParser.cs`
- Modify: `doc/PCRE-DESIGN.md`, `doc/PCRE-SYNTAX-ACCEPTANCE-MATRIX.md`

**Interfaces:** `PcreByteCompiler.Compile(byte[], PcreByteOptions?)` continues to produce the existing internal `PcreByteBackreference` node and `PcreByteDiagnostic`.

- [x] Add reference and diagnostic fixtures for `\gN`, `\g{N}`, a tenth capture, digit termination, unset/forward capture and rejected families.
- [x] Run the Staging tests against the unchanged parser and confirm the accepted-form cases fail as `UnsupportedSyntax`.
- [x] Parse only unsigned decimal absolute `\g` operands, enforce the capture ceiling and register the numeric reference for post-parse validation.
- [x] Update the acceptance inventory and design status without widening the public API.
- [ ] Verify all six Staging jobs and review the final diff before user merge.
