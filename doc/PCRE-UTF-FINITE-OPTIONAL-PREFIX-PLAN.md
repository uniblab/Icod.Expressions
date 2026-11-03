# UTF Finite Optional Prefix Implementation Plan

> REQUIRED SUB-SKILL: superpowers:executing-plans inline. No subagents.

**Goal:** Preserve native bitmap entry across finite optional atom prefixes.
**Architecture:** Broaden four existing shared compiler cases from max1 to
positive finite maxima, reusing every atom/source classifier and Skip barrier.
**Tech Stack:** C#13/net8/9/10, xUnit, PCRE.NET1.6.0/PCRE2 10.48 JIT.
**Spec:** PCRE-UTF-FINITE-OPTIONAL-PREFIX-DESIGN.md.

## Constraints and review focus

Spec constraints apply; all8376 baseline expectations/frozen35 unchanged.
1. All four existing literal/set/type cases admit positive finite maxima only.
2. Literal multicase/source/UCP/CR/property classifiers remain intact.
3. Retained/cost2/wrapper1/CT/shared1000/depth/capture guards stay shared.
4. Existing4096 expansion and65535 numeric count policies remain unchanged.
5. Consumed/deep/capture/UTF boundaries/work/cancellation remain behavioral.

## Task1: Native-first fixtures and observed RED

Files: tests/Pcre/PcreUtfFiniteOptionalPrefixReferenceTests.cs; design/plan.
Interfaces: existing Compile/Match and PcreRegex8Bit native JIT reference.
- [ ] Full capture fixtures for supported families/finite bounds/modifiers/
  source forms/UCP/restrictions, consumption/deep/dependency/mixed/negative controls.
- [ ] Six newline/every offset/strict-tolerant UTF validation, exact/exhausted/
  observed work/cancellation/default budget/parser257, expansion/count rejection.
- [ ] Publish tests-only draft; successful-build RED confined to new class,
  all8376 baseline pass; preserve tests afterRED.

## Task2: Broaden finite optional-prefix bound classification

Files: src/Pcre/PcreByteProgram.cs; four canonical PCRE design/fixtures/
resource-contract/syntax inventories.
Interfaces: FindUtfCrLfLeadingNode existing switch; no new runtime/public API.
- [ ] Four existing optional cases require Minimum0, Maximum>0 and<int.MaxValue;
  keep all atom classifiers/costs/retained/guards unchanged.
- [ ] Exact-head six native Staging platforms/net8/9/10 full/frozen214/configured
  gates/7equal workloads pass;0failed/skipped tests/build warnings/errors.
- [ ] Wholebranch author review; strict logs/head/tree/tests-afterRED/frozen/
  clean/whitespace audit; issue102/PR ready/open/unmerged; user review/merge next.
