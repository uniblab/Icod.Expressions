# Relative `\g` Backreference Implementation Plan

> **For agentic workers:** Use superpowers:executing-plans to implement this plan yourself. No subagents.

**Goal:** Resolve signed relative numeric `\g` references against capture numbering in the internal PCRE parser.

**Architecture:** Parse an optional sign and bounded decimal magnitude in the existing `ParseAbsoluteBackreference` path, resolve it to a positive absolute group number, and reuse the current post-parse validation and VM opcode.

**Tech Stack:** C# 13, .NET 8/9/10, xUnit, pinned PCRE.NET test reference, six Staging jobs.

**Spec:** `doc/PCRE-RELATIVE-G-REFERENCE-DESIGN.md`

## Global Constraints

- Internal PCRE API only; no Grep or CommandFramework production edits.
- Source-byte spans, capture journal and work/cancellation accounting remain in the existing VM.
- The parser's capture ceiling is 256; no signed integer overflow on long operands.

## Review Focus

- `-1` after group 2 resolves to group 2, while `-2` inside group 2 resolves to group 1.
- `+1` before the next group can compile but initially fails if that group is unset.
- A later repetition may use a capture from an earlier iteration through a forward relative reference.
- Signed zero, missing digits and an out-of-range target fail at the escape byte.
- Named and subroutine forms retain `UnsupportedSyntax`.

---

### Task 1: Relative numeric escape

**Files:**
- Modify: `tests/Pcre/PcreReferenceConformanceTests.cs`, `tests/Pcre/PcreByteBackreferenceTests.cs`, `tests/Pcre/PcreSyntaxAcceptanceTests.cs`
- Modify: `src/Pcre/PcreByteParser.cs`
- Modify: `doc/PCRE-DESIGN.md`, `doc/PCRE-SYNTAX-ACCEPTANCE-MATRIX.md`

**Interface:** `PcreByteCompiler.Compile(byte[], PcreByteOptions?)` continues to lower references to the existing `PcreByteBackreference` node; public API remains unchanged.

- [x] Add pinned reference spans, unset/iteration cases and diagnostics for signed forms.
- [x] Verify the test-first CI run fails for the intended unsupported signed syntax.
- [x] Resolve signed operands with bounded arithmetic and retain other `\g` rejections.
- [ ] Update the syntax inventory and verify all six Staging jobs on the final head.
