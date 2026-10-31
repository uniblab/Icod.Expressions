# Byte Code-Unit Escape Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: superpowers:executing-plans inline. No subagents, per user instruction.

**Goal:** Byte C consumes any one byte using shared Any lowering.
**Architecture:** Parser-only alias, bracket diagnostic and explicit UTF gate; existing bytecode/state/widths/prefix proof.
**Tech Stack:** C#13, net8/net9/net10, xUnit, pinned PCRE.NET1.6.0 / PCRE2 10.48 JIT.
**Spec:** doc/PCRE-BYTE-CODE-UNIT-DESIGN.md

## Global Constraints

No agents/Python/SDK install. UTF-8 LF; root project/solution, src/tests/samples.
No public/package/version/dependency/workflow/release/consumer/fallback changes.
Existing depth256/captures256/instructions4096/marks256/frames1024/work1,000,000.
Grep last; CommandFramework unchanged. User owns merge; preserve evidence.

## Review Focus

1. NUL/newline/high bytes always consume exactly1; UCP/case/dot/newline/BSR do not widen or restrict C.
2. Quotes/backslashes/lowercase-c stay distinct; brackets reject C at original slash.
3. Native byte ALLANY prefix handling survives ordinary wrappers and CRLF retry offsets.
4. Captures/retries/calls/assertions/behind use byte widths and shared rollback, with no new state.
5. Strict/tolerant UTF reject explicitly; source/start/work/cancellation and compile ceilings remain.

## Task1: Native fixtures and observed RED

**Files:** New tests/Pcre/PcreByteCodeUnitReferenceTests.cs; design/plan; canonical inventories; SyntaxAcceptance/ByteAtom/ControlEscape/FixedLookbehind historical C gates.
**Interfaces:** Existing Compile/Match/reference only.

- [ ] Native all-byte modes and literal all-capture spans, every-offset six-convention protocols, quote/control/grammar/UTF/profile and work/resource controls cover focus1..5.
- [ ] Replace old byte C gates with UTF-C/X representatives; preserve counts/owner controls.
- [ ] Native checker/design-builder/plan-interface self-check, frozen35/whitespace; publish tests-only draft and observe expected RED/build success before source.

## Task2: Parser alias and full verification

**Files:** src/Pcre/PcreByteParser.cs only; canonical status records.
**Interfaces:** Existing ParseEscape(int,bool)->PcreByteNode; existing PcreByteAny(bool DotAll).

- [ ] After reading escape C: bracket -> Invalid(offset), UTF -> Unsupported(offset), byte -> new PcreByteAny(true). No new AST/opcode/width/frame path.
- [ ] Full/frozen214 on six native Staging platforms/net8/9/10, failed/skipped/build warnings/errors0, configured gates and7Linux workload equalities.
- [ ] Separate whole-branch author self-review, exact reviewed head/tree/log/frozen35/whitespace/clean proof, update PR/mark ready; never merge.

Pre-flight: Any already reads bytes when Utf8=false, DotAll bypasses newline and
its byte width/prefix metadata are shared. The UTF gate preserves decoder invariants.
No interface conflict. Maintainer scope choice recorded; no extra approval pause.
