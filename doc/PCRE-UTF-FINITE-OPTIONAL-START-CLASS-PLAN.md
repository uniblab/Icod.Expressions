# Finite optional UTF starter-class implementation plan

> Execute inline; no subagents.

**Goal:** Cross already-supported finite optional primitives in general starter-class proofs.
**Architecture:** Share the existing nullable-prefix walker with explicit unbounded-repeat admission.
**Tech:** C#13/net8/9/10,xUnit,PCRE.NET1.6.0/PCRE2 10.48 JIT.
**Spec:** PCRE-UTF-FINITE-OPTIONAL-START-CLASS-DESIGN.md.

## Constraints and review focus
Spec constraints apply. Optional bitmap CR exclusion is required even when the
mandatory class excludes CR; UCP and source-sensitive folding can remove bitmap
availability. Root stars require ownership, so the general path remains finite
only. Exhausted branches and nullable group returns stay rejected; every
alternative must be proven; budget/cancellation/selected PLUS state stay intact.

## Task1: Pin the native contract and observe RED
Files: tests/Pcre/PcreUtfFiniteOptionalStartClassReferenceTests.cs;design/plan.
Interfaces: existing Compile/Match/PcreRegex8Bit JIT.
- [ ] Pin native full captures for the admitted atom/count/modifier/source shapes,
  mixed prefixes/wrappers/alternatives and excluded/legacy PLUS owner controls.
  Add six-newline/UCP/offset/fragment,strict validation and resource cases.
- [ ] Publish tests-only draft. Observe successful-build failures confined to
  the new class,all23947 baseline tests passing. Freeze the test blob thereafter.

## Task2: Share finite optional traversal
Files: src/Pcre/PcreByteProgram.cs;four canonical inventories;recent progress index.
Interfaces: FindUtfCrLfLeadingNode gains optional bool allowUnboundedOptionalRepeats=true;
CanCrossUtfCrLfOptionalRepeat gains that bool; existing return tuple unchanged.
- [ ] Thread the flag through the existing four optional-atom arms. Finite
  maximum still passes; unbounded additionally requires the flag and unchanged
  BlocksSkip/identity rule. General proof calls the walker with the flag equal
  to selected-repeat presence,replacing its duplicate prefix scan.
- [ ] Preserve costs,retained state,guards,immutable tests/frozen35/whitespace.
  Commit,verify exact remote content and review entire branch. Audit all six
  native Staging/full/frozen214/net8/9/10/configured gates/seven equal workloads,
  zero failures/skips/compiler warnings/errors;ready PR remains unmerged.
- [ ] Append one progress row and update only issue102's footer after CI;
  retain earlier issue history and preserved index block.
