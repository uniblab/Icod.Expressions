# UTF finite nullable-group continuation implementation plan

> Execute inline with superpowers:executing-plans; no subagents.

**Goal:** Resume the bounded UTF bitmap proof after finite repeats of nullable groups.
**Architecture:** Extend existing compile-time continuation tuples with copy-cost
metadata and a pending-repeat guard; preserve shared proofs and all runtime code.
**Tech:** C#13/net8/9/10, xUnit, PCRE.NET1.6.0/PCRE2 10.48 JIT.
**Spec:** PCRE-UTF-FINITE-GROUP-ENTRY-DESIGN.md.

## Global constraints and review focus

Spec constraints apply; all18169 baseline expectations/frozen35 unchanged.
1. Greedy empty capture participation differs from lazy omission and must survive.
2. Inner returns cannot accept a mandatory set while a repeat body is pending.
3. Later copied/alternative predicates or CR operands invalidate the entire proof.
4. Finite-copy/nested-repeat costs cannot overflow or bypass1000 visits.
5. Inner stars gain no recovery owner; a separately proven earlier star keeps it.

## Task1: Native fixtures and observed RED

Files: tests/Pcre/PcreUtfFiniteGroupEntryReferenceTests.cs; design/plan.
Interfaces: existing Compile/Match/PcreRegex8Bit JIT reference.
- [ ] Native literal full captures for finite bounds/aliases/modifiers, group
  types/depths/alternatives/nesting/sequential/capture/recovery/exclusion cases.
- [ ] Six newline/UCP/every-offset strict/tolerant fragments; exact/exhausted/
  observed work, cancellation/default budget/parser/capture/backtrack/copied bounds.
- [ ] Tests-only draft; successful-build failures confined to new class, all18169
  baseline pass. Keep tests/expectations unchanged after RED.

## Task2: Reuse bounded continuation for finite nullable repeats

Files: src/Pcre/PcreByteProgram.cs; four canonical inventories.
Interface: FindUtfCrLfJoinedRepeat(root)->PcreByteScalarSet? unchanged.
- [ ] Unwrap possessive modifiers and finite Minimum0 group repeats; charge
  repeat/modifier visits, collect child captures and set retained BlocksSkip.
- [ ] Tuple gains int RepeatMaximum/RepeatStartVisits (zero for plain groups).
  Preserve them across sibling branches; pending-repeat count blocks final-set
  selection until the whole body closes. Charge remaining copies by checked
  division-before-multiplication of measured per-copy visits.
- [ ] Preserve existing root/first-star/alternatives/nullable-atom/set/capture/
  actual-return/CT1000/PLUS-Skip=false guards and all runtime code.
- [ ] Verify unchanged tests/frozen35/whitespace; six native Staging full/frozen214
  ×net8/9/10/configured gates/7equal workloads,0failed/skipped tests/compiler
  warnings/errors. Author wholebranch review/strict logs/exacthead/tree/blob/
  clean audit; #102 stays open; ready PR remains unmerged for user review.
