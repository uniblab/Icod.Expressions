# UTF positive finite nullable-group continuation implementation plan

> Execute inline with superpowers:executing-plans; no subagents.

**Goal:** Resume the bounded UTF proof after positive finite nullable groups
without assigning repeated AST copies new recovery ownership.
**Architecture:** Reuse existing walker/frames/copy budget; distinguish ordinary
positive bodies from optional/possessive/atomic barriers before shared admission.
**Tech:** C#13/net8/9/10, xUnit, PCRE.NET1.6.0/PCRE2 10.48 JIT.
**Spec:** PCRE-UTF-POSITIVE-GROUP-ENTRY-DESIGN.md.

## Global constraints and review focus

Spec constraints apply; all19372 baseline expectations/frozen35 unchanged.
1. An unproven first star in an ordinary positive repeat must not gain metadata.
2. Atomic/possessive barriers and prior retained operands differ from erasure.
3. Required-count captures and last copied empty captures retain native spans.
4. Pending body/alternative guards survive exact counts and nested returns.
5. Existing copy arithmetic/1000 visits and runtime resource rejection stay bounded.

## Task1: Native fixtures and observed RED

Files: tests/Pcre/PcreUtfPositiveGroupEntryReferenceTests.cs; design/plan.
Interfaces: existing Compile/Match/PcreRegex8Bit JIT reference.
- [ ] Native full-capture literals for positive exact/ranged/modifier/group/depth/
  alternative/source/capture/recovery/exclusion cases and retained/erased controls.
- [ ] Six newline/UCP/every-offset strict/tolerant fragments; work exact/exhausted/
  observed/cancellation/default budget/parser/capture/backtrack/copy/expansion limits.
- [ ] Tests-only draft; successful-build failures confined to new class with all
 19372baseline pass. Preserve all tests/expectations afterward.

## Task2: Preserve recovery admission for positive finite bodies

Files: src/Pcre/PcreByteProgram.cs; four canonical inventories.
Interface: FindUtfCrLfJoinedRepeat(root)->PcreByteScalarSet? unchanged.
- [ ] Track a local repeatBlocksSkip for possessive modifiers/Minimum0; admit
  nonnegative Minimum with finite Maximum>0. Preserve incoming state otherwise.
- [ ] Explicit atomic group descent sets retained BlocksSkip. Existing alternatives
  keep their barrier. Do not modify owner discovery or instruction lowering.
- [ ] Keep repeated-child captures, pending body/alternative guards, measured-copy
  arithmetic/CT1000/set/classifier/actual-return/completed-proof/PLUS-Skip=false.
- [ ] Verify unchanged tests/frozen35/whitespace; six native Staging full/frozen214
  ×net8/9/10/configured gates/7equal workloads,0failed/skipped tests/compiler
  warnings/errors. Author review/strict logs/exacthead/tree/blob/clean audit;
  #102 stays open; ready PR remains unmerged for user review.
