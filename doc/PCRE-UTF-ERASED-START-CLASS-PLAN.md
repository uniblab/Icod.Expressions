# UTF erased-prefix starter-class implementation plan

> Execute inline; no subagents.

**Goal:** Cross already-proven erased primitive prefixes in the general UTF class proof.
**Architecture:** Reuse IsUtfCrLfErasedRepeat in local AddBranches's bounded scan.
**Tech:** C#13/net8/9/10,xUnit,PCRE.NET1.6.0/PCRE2 10.48 JIT.
**Spec:** PCRE-UTF-ERASED-START-CLASS-DESIGN.md.

## Constraints and review focus
Spec constraints apply. Atomic repeat wrappers differ from retained atomic
groups; direct zero references differ from calls/groups; exhausted branches
reject; every prefix is charged/cancellable; existing CR/LF and owner gates stay.

## Task1: Native contract and observed RED
Files: tests/Pcre/PcreUtfErasedStartClassReferenceTests.cs;design/plan.
Interfaces: existing Compile/Match/PcreRegex8Bit JIT.
- [ ] Pin full native capture literals for erased types/modifiers/dependencies/
  mixed-boundary/wrapper/alternative/exclusion cases, six newline/UCP/offset
  fragments, strict validation and shared work/cancellation/default-budget/
  long ordinary controls.
- [ ] Publish tests-only draft; observe successful-build new-class RED with all
  23525baseline passing. Record test blob and preserve tests thereafter.

## Task2: Shared erasure classification in the general proof
Files: src/Pcre/PcreByteProgram.cs;four canonical inventory appendices.
Interfaces: HasUtfCrLfStartClass(root)->bool/local AddBranches and existing
IsUtfCrLfErasedRepeat(PcreByteNode)->bool unchanged.
- [ ] Skip boundaries(cost1) and direct erased repeats(cost2), with one extra
  charge for an atomic repeat wrapper; budget-check before addition and cancel
  per prefix. Retain all unsupported nodes for existing worklist rejection.
- [ ] Preserve owner/selected-repeat/range/alternative/exact-large guards and
  immutable tests/frozen35/whitespace; audit six native Staging full/frozen214/
  net8/9/10, configured gates/seven equal workloads,0failures/skips/warnings/
  errors. Author review/exacttree/nineblobs/clean; ready PR remains unmerged.
- [ ] Preserve issue102's recent index in doc/PCRE-UTF-RECENT-PROGRESS.md before
  linking only that footer to the exact source commit; earlier body history
  and open state remain intact.
