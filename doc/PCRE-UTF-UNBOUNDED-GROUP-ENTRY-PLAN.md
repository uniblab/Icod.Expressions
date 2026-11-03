# UTF unbounded nullable-group continuation implementation plan

> Execute inline with superpowers:executing-plans; no subagents.

**Goal:** Resume bounded UTF bitmap proofs after nullable unbounded groups,
preserving earlier required-copy and primitive recovery ownership.
**Architecture:** Reuse existing walker/pending guards; finite study-copy counts
represent unbounded loops under the same1000-visit budget. No runtime changes.
**Tech:** C#13/net8/9/10, xUnit, PCRE.NET1.6.0/PCRE2 10.48 JIT.
**Spec:** PCRE-UTF-UNBOUNDED-GROUP-ENTRY-DESIGN.md.

## Global constraints and review focus

Spec constraints apply; all20489 baseline expectations/frozen35 unchanged.
1. Group star/plus loops cannot acquire a scalar-owner flag.
2. Earlier required copies at Minimum>=2 can still own recovery; do not force a barrier.
3. Lazy zero-count captures differ from required/greedy participating empties.
4. Minimum+1 and nested copy charges cannot overflow or bypass1000 visits.
5. Pending bodies/alternatives and existing progress/resource guards remain active.

## Task1: Native fixtures and observed RED

Files: tests/Pcre/PcreUtfUnboundedGroupEntryReferenceTests.cs; design/plan.
Interfaces: existing Compile/Match/PcreRegex8Bit JIT reference.
- [ ] Native full captures for loops/count aliases/modifiers/group/depth/nesting/
  alternative/source/dependency/owner/guard/exclusion cases and copied captures.
- [ ] Six newline/UCP/every-offset strict/tolerant fragments; work/cancellation/
  default budget/parser/capture/backtrack/copy/expansion boundaries.
- [ ] Tests-only draft; successful-build failures confined to new class with all
  20489baseline pass. Preserve all tests/expectations afterward.

## Task2: Bound study copies and retain ownership guards

Files: src/Pcre/PcreByteProgram.cs; four canonical inventories.
Interfaces: FindUtfCrLfJoinedRepeat(root)->PcreByteScalarSet? unchanged;
FindUtfCrLfRootStarAtom(root)->(Atom,Skip) preserves identity but disallows scalar
Skip for group children.
- [ ] Recognize unbounded group bodies, normalize study copies to Minimum+1 only
  after guarding Minimum<1000, and block inner ownership at Minimum0/1.
- [ ] Rename local/frame RepeatMaximum to RepeatCopies; preserve all finite paths,
  repeated-child captures and sibling bookkeeping. Keep pending/set/capture/
  actual-return/completed-proof/CT1000/division-before-multiplication/PLUS-Skip=false.
- [ ] Group first-star children have Skip=false; primitive owner finder/markers/
  depth/capture policy and all lowering/runtime code remain.
- [ ] Verify immutable tests/frozen35/whitespace; six native Staging full/frozen214
  ×net8/9/10/configured gates/7equal workloads,0failed/skipped tests/compiler
  warnings/errors. Author review/strict logs/exacthead/tree/blob/clean audit;
  #102 stays open; ready PR remains unmerged for user review.
