# UTF Literal Case Query Implementation Plan

> REQUIRED SUB-SKILL: superpowers:executing-plans inline. No subagents.

**Goal:** Match native bitmap availability for genuine optional singleton literals.
**Architecture:** Retain original SourceLiteral on ScalarSet and share a private
pinned source-opcode classifier between plain and bracket optional literals.
**Tech Stack:** C#13/net8/9/10, xUnit, PCRE.NET1.6.0/PCRE2 10.48 JIT.
**Spec:** PCRE-UTF-LITERAL-CASE-QUERY-DESIGN.md.

## Constraints and review focus

Spec constraints apply. All7565 baseline expectations/frozen35 stay unchanged.
1. Every default multicase member is rejected as a property query; Turkish pairs excluded.
2. r restores only ASCII-crossing casesets, not sigma/titlecase/other non-ASCII sets.
3. Raw original scalar survives folding/copies; Unknown remains conservative.
4. Source Class behavior and all CR/predicate/range/CT/budget/Skip guards stay intact.
5. No matcher/membership/runtime table or raised limit enters production.

## Task1: Native-first tests and observed RED

Files: tests/Pcre/PcreUtfLiteralCaseQueryReferenceTests.cs; design/plan.
Interfaces: existing Compile/Match and PcreRegex8Bit Compiled/JIT reference.
- [ ] Full captures for all84 members/plain and bracket, ordinary/restricted/
  alias/modifier/consumed/deep/dependency/mixed forms and conservative controls.
- [ ] Six newline/UCP/every offset/strict-tolerant/work/exhaustion/cancellation/
  default budget/parser257. Publish tests-only draft; inspect successful-build
  RED with7565 baseline passing; freeze test blobs afterward.

## Task2: Original scalar and shared opcode classification

Files: PcreByteSyntax.cs, PcreByteParser.cs, PcreByteProgram.cs under src/Pcre;
four canonical PCRE inventories.
Interfaces: ScalarSet.SourceLiteral internal init int default-1;
private static bool HasUtfCrLfLiteralBitmap(int scalar,bool caseless,bool restricted).
- [ ] Capture original literal scalar before folding without changing constructor,
  Source enum, newline flags, membership or copy/rewrite behavior.
- [ ] Implement pinned84-member classifier with restricted K/S suppression; use
  it in plain optional Literal arm and proven Source Literal optional-set guard.
- [ ] Six native Staging platforms/net8/9/10 full/frozen214/configured gates green,
  zero failed/skipped tests/build warnings/errors; seven equal workloads.
- [ ] Whole-branch author review/exact head-tree-log/tests-afterRED/frozen/clean/
  whitespace audit; update102/PR, mark ready, preserve artifacts; user merges.

Pre-flight: parser produces SourceLiteral consumed by compiler; constructors and
runtime interfaces unchanged. Native caseset membership facts remain compile-time.
