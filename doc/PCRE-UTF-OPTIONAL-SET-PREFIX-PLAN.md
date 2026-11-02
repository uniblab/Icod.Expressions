# UTF Optional Set Prefix Implementation Plan

> REQUIRED SUB-SKILL: superpowers:executing-plans inline. No subagents.

**Goal:** Preserve native bitmap entry without repeat skipping after optional sets.
**Architecture:** Add a bounded range-backed optional-set guard to the shared
Node/BlocksSkip helper. Preserve existing mandatory-repeat proof and VM behavior.
**Tech Stack:** C#13/net8/9/10, xUnit, PCRE.NET1.6.0/PCRE2 10.48 JIT.
**Spec:** PCRE-UTF-OPTIONAL-SET-PREFIX-DESIGN.md.

## Global constraints and review focus

Spec constraints apply. All7136 baseline expectations/frozen35 stay unchanged.
1. Optional ASCII/wide/disjoint sets must reject CR before bitmap admission.
2. Greedy/lazy/possessive sets retain consumed-prefix and full capture spans.
3. Range proof shares CT/1000 budget with source-node traversal; never expands scalars.
4. Mixed optional/pruned/retained chains preserve the accumulated skip barrier.
5. All-optional/negated/predicate/CR-admitting/unknown nodes stay conservative.

## Task1: Native-first tests and observed RED

Files: tests/Pcre/PcreUtfOptionalSetPrefixReferenceTests.cs; design/plan.
Interfaces: existing Compile/Match and native reference PcreRegex8Bit Compiled/JIT.
- [ ] Native literal full spans for set forms/modifiers/consumed LF and prefixes,
  depth4/5/34/200, dependencies, mixed chains and conservative controls.
- [ ] Six newline/UCP/all offsets/strict-tolerant, exact/exhausted work,
  cancellation/default budget/parser257. Publish tests-only draft; inspect
  successful-build expected RED with7136 baseline passing; freeze test blobs.

## Task2: Compiler class proof and handoff

Files: src/Pcre/PcreByteProgram.cs; four canonical PCRE inventories.
Interfaces: private bool IsUtfCrLfOptionalSet(PcreByteScalarSet set, ref int visits);
existing (PcreByteNode? Node, bool BlocksSkip) FindUtfCrLfLeadingNode.
- [ ] Guard positive/nonempty/predicate-free scalar sets; range bounds/CR checks
  with CT and one visit per range. Only min0/max1 sets cross with cost2/true,
  plus existing wrapper1. Preserve all other guards and source/VM contracts.
- [ ] Six native Staging OS/arch/net8/9/10 full/frozen214/configured gates pass,
  failed/skipped tests/build warnings/errors0; seven equal workloads.
- [ ] Whole-branch author review, exact head/tree/log/tests-after-RED/frozen/clean/
  whitespace audit. Update102/PR, mark ready, preserve evidence; user merges.

Self-check: range guard and callers share ref visits; optional-set LF membership
is irrelevant to prefix eligibility. The mandatory selected set still admits LF.

## Review correction: ambiguous caseless one-key sets

Files: tests/Pcre/PcreUtfOptionalSetCaseFoldReferenceTests.cs; compiler; design/
plan and four canonical inventories. Original196-test blob remains unchanged.
- [ ] Native-first singleton K/S/sigma/Kelvin misses and multi-key positives,
  full captures/four UTF-UCP modes/six newline/every-offset fragment protocols.
  Publish the separate tests; inspect successful-build expected RED.
- [ ] After range validation, reject caseless sets with exactly one singleton
  folded-key interval; preserve multi-key admission and all existing semantics.
- [ ] Final complete suite/six-native GREEN and whole-branch review/exact audit.
