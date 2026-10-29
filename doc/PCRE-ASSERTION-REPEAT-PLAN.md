# Direct Assertion Repeats Implementation Plan

> **For agentic workers:** Use superpowers:executing-plans. Steps track the bounded feature through compiled-oracle verification and a ready PR.

**Goal:** Implement direct lookaround quantifiers with PCRE2 finite assertion semantics.

**Architecture:** Normalize direct assertion bounds in the parser, then reuse finite counted-repeat lowering, assertion guards and the capture journal. Ordinary consuming and group-wrapped repeats retain their existing lowering.

**Tech Stack:** C# 13, net8/net9/net10, xUnit, test-only PCRE.NET 1.6.0 / PCRE2 10.48 Compiled/JIT.

**Spec:** `doc/PCRE-ASSERTION-REPEAT-DESIGN.md`.

## Global Constraints

Internal managed implementation; no public API/version/dependency/consumer change.
Source `/src`, tests `/tests`; local Debug, six-platform Staging CI. No Python.
Existing 256 parser depth, 65,535 source count, 4,096 compiled instructions and
1,024 saved paths, cancellation and work limits remain. Grep migration stays gated.

## Review Focus

- Zero-width captures grow across iterations instead of stopping on no progress.
- Optional-repeat retries restore earlier captures rather than leave later values.
- Finite maximum zero still registers captures but executes no assertion.
- Literal lookbehind and general lookahead share normalization without widening lookbehind syntax.
- Normalized open maximum cannot overflow or bypass expansion/cancellation limits.

## Task 1: Pin direct-repeat behavior before implementation

**Files:** Create `tests/Pcre/PcreAssertionRepeatReferenceTests.cs`; design and plan above.
**Interfaces:** Existing `PcreByteCompiler.Compile`, `PcreCompiledBytePattern.Match`, test-only `PcreRegex8Bit`.

- [ ] Add explicit Compiled/JIT outcomes and complete capture comparisons for the design's acceptance cases. Include `^(?=(\1a|a)){3,}\1$` on four ASCII a bytes, whose capture must grow to four bytes; optional rollback and U/lazy/possessive distinctions; direct fixed-literal lookbehind and zero counts.
- [ ] Add selected-offset byte/tolerant-UTF comparisons and resource/invalid syntax cases.
- [ ] Publish a test-only draft PR and inspect expected missing-feature failures, with successful native assertions and clean compilation.

## Task 2: Normalize and integrate

**Files:** Modify `src/Pcre/PcreByteParser.Groups.cs`; replace obsolete rejection fixtures in `tests/Pcre/PcreByteLookaroundTests.cs` and `tests/Pcre/PcreGeneralLookaheadReferenceTests.cs`; update PCRE inventory docs.
**Interfaces:** Existing `PcreByteRepeat(Child, Minimum, Maximum, Greedy, Offset)` and counted compiler.

- [ ] Recognize `PcreByteLiteralAssertion` and `PcreByteLookahead` before wrapping. Parse the existing bounds/suffix grammar, normalize only unlimited direct assertion maxima to minimum+1 and use original quantifier offset for finite lowering.
- [ ] Charge no progress marks for these finite repeats; retain all ordinary repeat behavior and validation.
- [ ] Replace obsolete rejection assertions with supported behavior coverage; update matrix/design/resource/consumer inventories.
- [ ] Run CI and inspect failures against the native oracle; fix confirmed defects with focused red-to-green tests.

## Task 3: Review and finish

- [ ] Fresh read-only whole-branch review against the design and review focus; resolve material findings with regression evidence.
- [ ] Verify `git diff --check`, `sh tools/verify-frozen-source.sh`, and all six Staging jobs, full and frozen tests on all three frameworks, workload/extraction/package gates.
- [ ] Update PR with exact head and evidence; mark ready. User retains merge control.
