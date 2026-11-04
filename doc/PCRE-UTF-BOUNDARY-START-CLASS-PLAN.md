# UTF boundary-prefixed starter-class implementation plan

> Execute inline; no subagents.

**Goal:** Admit leading word boundaries in the existing general UTF class proof.
**Architecture:** One bounded boundary-only prefix scan in local AddBranches.
**Tech:** C#13/net8/9/10,xUnit,PCRE.NET1.6.0/PCRE2 10.48 JIT.
**Spec:** PCRE-UTF-BOUNDARY-START-CLASS-DESIGN.md.

## Constraints and review focus
Spec constraints apply. Retain selected-repeat ownership, reject boundary-only
branches, require CR exclusion in every alternative and at least one LF, preserve
unselected wide PLUS/exact-large/predicate exclusions, charge/cancel every visit.

## Task1: Native contract and observed RED
Files: tests/Pcre/PcreUtfBoundaryStartClassReferenceTests.cs;design/plan.
Interfaces: existing Compile/Match/PcreRegex8Bit JIT.
- [ ] Pin full native captures for boundary/class/group/repeat/alternative/source/
  capture-dependency/exclusion forms; six newline/UCP/every-offset fragments and
  strict validation. Work/cancellation/default budget/long ordinary controls.
- [ ] Publish tests-only draft; observe successful-build failures confined to new
  class with all23346baseline pass. Record test blob; keep tests immutable.

## Task2: General boundary-only proof admission
Files: src/Pcre/PcreByteProgram.cs;four canonical inventory appendices.
Interface: HasUtfCrLfStartClass(root)->bool and local AddBranches unchanged.
- [ ] When no selected repeat set exists, skip only leading PcreByteWordBoundary
  nodes with one visit/cancellation per node, then enqueue the next node or
  reject an exhausted branch. Keep selected-repeat path and all worklist guards.
- [ ] Preserve tests/frozen35/whitespace; six native Staging full/frozen214/
  net8/9/10 and configured gates/seven equal workloads,0failed/skipped tests/
  compiler warnings/errors. Author review/exacttree/eightblobs/clean audit;
  #102 stays open; ready PR left unmerged for user review.
