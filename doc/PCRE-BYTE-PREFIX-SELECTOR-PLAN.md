# Byte Prefix Selector Implementation Plan

> REQUIRED SUB-SKILL: superpowers:executing-plans inline. No subagents.

**Goal:** Preserve conditional native byte CRLF entry for longer restrictive fixed prefixes.
**Architecture:** One bounded AST proof returns old bool plus optional literal metadata;
existing matcher bumpalong charges a shared Step comparison only for that metadata.
**Tech Stack:** C#13/net8/net9/net10, xUnit, PCRE.NET1.6.0/PCRE2 10.48 JIT.
**Spec:** PCRE-BYTE-PREFIX-SELECTOR-DESIGN.md.

## Global constraints and review focus

No agents/Python/SDK install; no public/package/workflow/release/consumer/fallback
changes or raised limits; frozen35 intact. Original offsets/captures/work retained.
1. Literal failure admits LF; literal success plus VM class failure still skips LF.
2. One final literal within12 positions, no repeated class provenance inference.
3. UCP/local i/r folding and explicit-source metadata, byte-only guard and bounded reads.
4. Old unconditional cases keep work; new comparison always charged/observed/cancellable.
5. Atomic/capture order, deep wrappers, fixed11/12 cutoff and UTF controls remain native.

## Task1: Pin the missing selector and observe RED

Files: tests/Pcre/PcreBytePrefixSelectorReferenceTests.cs; design/plan.
Interfaces: existing Compile/Match/reference APIs only.
- [ ] Pin literal full-capture positive/miss fixtures and all-offset protocols,
  six newlines/two UCP choices, 11/12 boundaries, CRLF Any(false), UTF controls.
- [ ] Add shared work/budget/cancellation/resource controls and native bounded-span checks.
- [ ] Publish tests-only draft; observe successful-build RED, old5272 baseline pass.

## Task2: One proof, descriptor and conditional bumpalong

Files: src/Pcre/PcreByteProgram.cs, src/Pcre/PcreByteMatcher.cs; four canonical inventories.
Interfaces: `PcreBytePrefixLiteral(int ByteOffset,int Value,bool Caseless,bool CaselessRestricted)`;
program nullable `ByteCrLfPrefixLiteral`; private proof returns `(bool Unconditional,
PcreBytePrefixLiteral? Literal)`. Existing bool/UTF callers and runtime ScalarEquals/Step.
- [ ] Reuse bounded proof, accumulate restrictive positions; direct positive exact Any
  repeats count their width. Sole final literal returns bool or optional descriptor.
- [ ] In existing failed-CR pair guard, charge Step and bounded shared ScalarEquals
  only for descriptor; successful literal skips LF, failed literal retains candidate.
- [ ] Six native Staging jobs/net8/9/10 full/frozen214, extraction and configured
  workload/API/package gates green, zero failed/skipped/build warnings/errors.
- [ ] Separate whole-branch author review, frozen35/whitespace/clean/head/tree/log
  audit; update #99 and PR, mark ready; user owns merge.

Pre-flight: task1 consumes stable existing APIs; task2 adds only immutable program
metadata and uses existing comparison/accounting. No task interface conflict.
Standing scope choice: retain broader optimizer gates, preserve prior work counts.
