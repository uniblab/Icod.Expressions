# CR and NUL newlines implementation plan

> **For agentic workers:** Use superpowers:executing-plans inline; no subagents, per user instruction.

**Goal:** Add CR/NUL global newline conventions without changing existing LF/ANYCRLF or BSR behavior.
**Architecture:** Four-value immutable newline enum and primary-character helper feed parser dot/comments and shared VM dot/anchors; ANYCRLF-specific pair logic stays conditional.
**Tech Stack:** C#13, net8/net9/net10, xUnit, PCRE.NET1.6.0 / PCRE2 10.48 Compiled/JIT.
**Spec:** `doc/PCRE-CR-NUL-NEWLINES-DESIGN.md`

## Global constraints

- Exact byte0 prefix, last-wins newline family independent of UTF/UCP/BSR/NOTEMPTY; caller bool maps only initial LF/ANYCRLF.
- CR/NUL are single scalar conventions; raw NUL is not a terminator, R remains BSR-owned. No AST/opcode/frame/journal/width/optimizer/limit policy change.
- No API/package/version/dependency/workflow/release/consumer changes, fallback, Python or subagents. Local Debug, PR six native Staging jobs, no SDK; CI RED/GREEN. User merges; Grep last/CommandFramework removal after publication/all consumers.

## Review focus

1. CRLF under CR is two characters; under ANYCRLF retain all existing pair/caller-start/fragment behavior.
2. Raw NUL in pattern/subject is real data: comments resume after it, anchors use it, strict/tolerant UTF validation still sees the whole source.
3. Dot/N and s differ; R/BSR and bracket/property/shorthand membership remain independent.
4. All anchor forms must use the same global convention across assertions/calls/reset and limited lookbehind end contexts.
5. Prefix order/repetition/errors and work/cancellation/invalid offsets retain existing bounds and byte positions.

## Task1: Native fixtures and expected RED

**Files:** Create `tests/Pcre/PcreCrNulNewlineReferenceTests.cs`, design/plan; update PCRE design/matrix/resource/consumer inventories and pending CR/NUL representatives.
**Interfaces:** Existing Compile/Match/pinned reference APIs only.

- [ ] Literal all-capture, all256 byte and selected Unicode membership, extended comments/rawNUL, prefix ordering and all-offset CRLF/NUL/malformed UTF matrices; grammar/strict diagnostic and resource controls pin all five focus classes.
- [ ] Native checker and design/plan self-check, frozen35/whitespace; publish tests-only draft, observe successful build and expected new-fixture failures before production.

## Task2: One convention through shared paths

**Files:** Create `src/Pcre/PcreByteNewline.cs`; modify StartSettings, Compiler, Parser, Parser.Extended, Set, Program, Matcher under `src/Pcre/`.
**Interfaces:** `PcreByteNewline` enum Lf/AnyCrLf/Cr/Nul plus shared `Character()` extension; replace settings/program newline bool with enum, retain program derived AnyCrLfNewline and primary-character properties.

- [ ] Map caller bool to initial enum; scan CR/NUL and all existing newline tokens last-wins. Propagate enum through parser/program compiler. Cache DotCr/DotNul byte sets; scalar dot and N use the primary character.
- [ ] Comments terminate at primary character; only ANYCRLF consumes a CRLF pair. Replace unconditional LF tests in StartLine/EndLine/EndBeforeFinalLf with primary-character tests; retain every existing ANYCRLF-specific branch and bumpalong condition.
- [ ] All six native Staging full/frozen214 on all three frameworks, zero failed/skipped/build warnings/errors, configured gates/seven Linux equalities. Separate author whole-branch self-review; material fixes RED→GREEN, exact head/tree/frozen35/whitespace/clean proof; update PR/mark ready. Never merge.

Pre-flight: Task1 uses stable APIs, Task2 enum/configuration stays internal; all downstream consumers mapped. Self-check found no interface conflict; all review-focus classes have Task1 coverage.
