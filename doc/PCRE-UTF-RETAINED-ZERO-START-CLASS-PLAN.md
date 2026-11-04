# UTF retained-zero starter-class implementation plan

> Execute inline; no subagents.

**Goal:** Cross the already-proven retained zero-prefix list in the general class proof.
**Architecture:** Share the existing classification with the PLUS nullable walker.
**Tech:** C#13/net8/9/10,xUnit,PCRE.NET1.6.0/PCRE2 10.48 JIT.
**Spec:** PCRE-UTF-RETAINED-ZERO-START-CLASS-DESIGN.md.

## Constraints and review focus
Spec constraints apply. Retained zero groups still block PLUS recovery; atomic
repeat wrappers differ from atomic group children; skipped bodies are opaque;
unsupported direct calls/other conditions stay excluded; budget/cancellation/
empty-branch and every-alternative CR/LF guards stay intact.

## Task1: Native contract and observed RED
Files: tests/Pcre/PcreUtfRetainedZeroStartClassReferenceTests.cs;design/plan.
Interfaces: existing Compile/Match/PcreRegex8Bit JIT.
- [ ] Pin full native captures for zero-prefix kinds/modifiers/dependencies/
  mixed-prefix/wrapper/alternative/exclusion cases and existing PLUS owners;
  six newline/UCP/offset fragments, strict validation and shared resource controls.
- [ ] Publish tests-only draft; observe successful-build failures only in new
  class, all23724baseline pass. Record test blob and preserve tests thereafter.

## Task2: Share retained-zero classification
Files: src/Pcre/PcreByteProgram.cs;four canonical inventories/recent progress index.
Interfaces: new IsUtfCrLfRetainedZeroRepeat(PcreByteNode)->bool; existing
FindUtfCrLfLeadingNode tuple and HasUtfCrLfStartClass bool unchanged.
- [ ] Extract exactly the old retained-zero kind list. PLUS walker retains=true,
  cost2/cost3 for atomic group child plus outer wrapper as before. General scan
  accepts shared retained classification, charging the same atomic-child extra.
- [ ] Keep unsupported kinds, owner/marker selection, all existing guards and
  immutable tests/frozen35/whitespace. Audit six native Staging/full/frozen214/
  net8/9/10/configured gates/seven equal workloads,0failures/skips/warnings/errors.
  Author review/exacttree/nineblobs/clean; ready PR unmerged for user review.
- [ ] Append one recent-index row and update only issue102's footer after CI;
  retain all earlier body history and the preserved old footer in the index.
