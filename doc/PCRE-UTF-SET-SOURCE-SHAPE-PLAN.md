# UTF Set Source Shape Implementation Plan

> REQUIRED SUB-SKILL: superpowers:executing-plans inline. No subagents.

**Goal:** Admit original multi-operand optional classes after canonical folding.
**Architecture:** Preserve Unknown/Literal/Class provenance on the existing
ScalarSet AST, then use it only in the bounded optional-set proof.
**Tech Stack:** C#13/net8/9/10, xUnit, PCRE.NET1.6.0/PCRE2 10.48 JIT.
**Spec:** PCRE-UTF-SET-SOURCE-SHAPE-DESIGN.md.

## Global constraints and review focus

Spec constraints apply. All7359 baseline expectations/frozen35 stay unchanged.
1. Literal source shapes (including equal-end ranges) keep conservative behavior.
2. Duplicate/quoted/escaped/range-plus-literal and Unicode aliases retain Class shape.
3. Node with-copies/tree rewrites preserve source shape and case/newline flags.
4. Source shape must not bypass CR/negation/predicate/range/budget guards or Skip.
5. Unknown provenance defaults conservative; no runtime/public metadata emitted.

## Task1: Native-first executable regressions and observed RED

Files: tests/Pcre/PcreUtfSetSourceShapeReferenceTests.cs; design/plan.
Interfaces: existing Compile/Match and native reference PcreRegex8Bit Compiled/JIT.
- [ ] Literal full spans for source variants/modifiers/consumption/depth/rewrites/
  dependencies/mixed chains and genuine singleton/conservative controls.
- [ ] Six newline/UCP/every offset/strict-tolerant/work/exhaustion/cancellation/
  default budget/parser257 tests. Publish tests-only draft; inspect successful-
  build RED with7359 baseline passing; freeze test blobs afterward.

## Task2: Source provenance and proof admission

Files: src/Pcre/PcreByteSyntax.cs; src/Pcre/PcreByteParser.cs;
src/Pcre/PcreByteProgram.cs; four canonical PCRE inventories.
Interfaces: PcreByteScalarSetSource Unknown/Literal/Class; internal init-only
PcreByteScalarSet.Source (default Unknown); existing constructor/helper unchanged.
- [ ] Add enum/property, set Source from original sourceTermCount/soleLiteral
  before folding; preserve ExplicitNewline and existing case/node with-copies.
- [ ] Admit Source Class after existing optional-set range validation; keep
  genuine Literal/Unknown one-key cases conservative and all Skip guards intact.
- [ ] Six native Staging OS/arch/net8/9/10 full/frozen214/configured gates green,
  failed/skipped tests/build warnings/errors0; seven equal workloads.
- [ ] Whole-branch author review and exact head/tree/log/tests-afterRED/frozen/
  clean/whitespace audit; update102/PR, mark ready, preserve artifacts; user merges.

Pre-flight: parser produces the enum/property that the compiler consumes; the
constructor and runtime instruction interfaces remain unchanged.
