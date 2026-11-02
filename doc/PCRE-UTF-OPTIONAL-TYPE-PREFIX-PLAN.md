# UTF Optional Type Prefix Implementation Plan

> REQUIRED SUB-SKILL: superpowers:executing-plans inline. No subagents.

**Goal:** Preserve native bitmap entry across four supported optional type queries.
**Architecture:** Use existing ScalarClass Kind/Negated in the shared compiler
leading-node switch; retain cost2 and the optional-prefix Skip barrier.
**Tech Stack:** C#13/net8/9/10, xUnit, PCRE.NET1.6.0/PCRE2 10.48 JIT.
**Spec:** PCRE-UTF-OPTIONAL-TYPE-PREFIX-DESIGN.md.

## Constraints and review focus

Spec constraints apply; all7947 baseline expectations/frozen35 unchanged.
1. UCP digit/word/nonspace properties stay excluded; scoped aD/aW/aS restore types.
2. Negated AsciiSpace qualifies; CR-consuming types/NOT_VSPACE/ANY/properties do not.
3. Greedy/lazy/possessive/count forms share cost/BlocksSkip and consuming semantics.
4. Deep/capture/dependency/omitted-retained prefix guards remain bounded and shared.
5. Exact/exhausted work, UTF fragment/caller boundaries and cancellation remain intact.

## Task1: Native-first tests and observed RED

Files: tests/Pcre/PcreUtfOptionalTypePrefixReferenceTests.cs; design/plan.
Interfaces: existing public Compile/Match, PcreRegex8Bit JIT reference.
- [ ] Pin full captures for the admitted four types plus UCP/restriction,
  modifiers/consumption/deep/dependency/mixed/negative controls.
- [ ] Cover six newlines/every offset/strict-tolerant validation, work/exhaustion/
  cancellation/default budget/parser257. Publish tests-only draft and observe
  successful-build RED in new class only; preserve all baseline tests/test blobs.

## Task2: Bounded shared type-query classification

Files: src/Pcre/PcreByteProgram.cs; doc/PCRE-DESIGN.md,
PCRE-GREP-CONSUMER-FIXTURES.md, PCRE-RESOURCE-ERROR-CONTRACT.md,
PCRE-SYNTAX-ACCEPTANCE-MATRIX.md.
Interfaces: existing FindUtfCrLfLeadingNode branch switch, no new public/runtime API.
- [ ] Admit min0/max1 direct positive AsciiDigit/AsciiWord/HorizontalSpace and
  negated AsciiSpace as cost2/retained true; preserve every existing guard.
- [ ] Exact-head six native Staging OS/arch/net8/9/10 full/frozen214 gates green,
  zero failed/skipped tests/build warnings/errors,7equal workloads.
- [ ] Whole-branch author review/exact head-tree-log/tests-afterRED/frozen/clean/
  whitespace audit; issue102/PR updated, ready/unmerged. User review/merge next.
