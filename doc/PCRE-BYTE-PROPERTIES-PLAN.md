# Byte Unicode Properties Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans inline. No subagents, per user instruction.

**Goal:** Explicit Unicode property sets in byte mode, sharing UTF membership.
**Architecture:** Shared pure membership evaluator; compile-time256-value projection into existing byte classes.
**Tech Stack:** C#13, net8/net9/net10, xUnit, pinned PCRE.NET1.6.0 / PCRE2 10.48 JIT.
**Spec:** doc/PCRE-BYTE-PROPERTIES-DESIGN.md

## Global Constraints

No subagents/Python/local SDK. UTF-8 LF; root project/solution, src/tests/samples.
No public/package/version/dependency/workflow/release/consumer/fallback changes.
Existing depth256/captures256/instructions4096/marks256/frames1024/work1,000,000.
Byte UCP remains rejected; Grep last; user owns merge; preserve evidence.

## Review Focus

1. Every byte, including00/85/A0/FF and UTF-looking sequences, remains one consumed byte.
2. Lu/Ll/Lt caseless widening happens before P/caret and outer negation, independently of ASCII/r.
3. Property terms retain source provenance: CRLF search and nonliteral range errors must not change through set projection.
4. Scoped/reset/call/assertion/retry paths preserve original capture spans and one-byte behind widths.
5. Malformed property/profile/source/start errors and resource/cancellation behavior remain bounded; existing UTF execution stays identical.

## Task1: Native fixtures/design and observed RED

**Files:** New tests/Pcre/PcreBytePropertyReferenceTests.cs, design/plan;
existing SpecialProperty byte-gate representative; design/matrix/resource/consumer inventories.
**Interfaces:** Existing Compile/Match and pinned reference only.

- [ ] Pin literal256-bit native masks for all45 category/special names, normal/i; test atoms/P/caret/class complements over every byte. These catch missing projection, wrong category, complement order and ASCII-only folding.
- [ ] Literal all-capture fixtures and every-offset byte matrices cover flags, byte/UTF contrast, all newline conventions, CRLF provenance, repeats/behind/assertions/calls/recursion/retries and malformed-UTF-looking bytes.
- [ ] Grammar/unknown/namespace/UCP/UTF-source/start errors plus equivalent-class work, tiny budget, compile limits and cancellation controls cover focus5.
- [ ] Replace old byte-property rejection loop with pending byte script/binary controls, retaining UTF script/binary assertions.
- [ ] Verify native masks/spans, design-builder/plan interfaces/frozen35/whitespace; commit and publish tests-only draft. Observe new-fixture UnsupportedSyntax RED with successful build before production.

## Task2: Shared membership and projection

**Files:** New src/Pcre/PcreScalarClassMembership.cs; Matcher and Parser; canonical inventories.
**Interfaces:** PcreScalarClassMembership.Contains(PcreByteScalarClassKind,int,UnicodeCategory?=null,bool=false);
Parser.CreatePropertyClass(PcreByteScalarClass) -> PcreByteNode. Existing VM/AST/public interfaces unchanged.

- [ ] Move ScalarClassContains and its category helpers unchanged; update all four UTF matcher call sites, preserving work accounting/caseless arguments.
- [ ] Remove byte-only ParseUnicodeProperty rejection, route both property success exits through CreatePropertyClass. UTF returns scalar node; byte projects Contains != Negated into PcreByteSet with cancellation per value, returns class with default provenance.
- [ ] Run full/frozen suites and configured gates across six native Staging platforms/frameworks. Fix material findings with observed RED before changes.
- [ ] Separate whole-branch author self-review, exact head/tree/log audit, frozen35/whitespace/clean check; record proof, mark PR ready and stop for user merge.

Pre-flight: Task1 uses existing compiler/matcher API; Task2 extraction keeps membership
signature and semantics, and byte bracket Class path already unions projected sets.
No shared-interface conflicts. Maintainer choice within standing authorization.
