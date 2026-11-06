# Positive lookahead deep group implementation plan

**Goal:** Preserve LF first-bitmap entry after CR through proven deep
mandatory groups within the existing visit budget.

**Architecture:** Replace depth-one recursion in
`HasUtfCrLfDirectGroupBitmap` (`src/Pcre/PcreByteProgram.cs`) with an
iterative pending stack of group heads. Reuse existing branch prefix,
direct-head and repeat predicates. Actual body traversal remains separate.

**Spec:** `doc/PCRE-UTF-POSITIVE-AHEAD-DEEP-GROUP-DESIGN.md`.

1. Add native PCRE2 10.48 JIT full-capture fixtures to
   `tests/Pcre/PcreUtfPositiveAheadDeepGroupReferenceTests.cs` for strict
   and tolerant UTF. Commit test-only RED and freeze its blob.
2. Replace bounded depth-one helper call with an iterative pending group
   stack. Retain visit charges, cancellation, all-branch requirements and
   conservative rejection of empty/nullable/repeated/unsupported heads.
3. Append evidence to four canonical inventories and recent index. Audit
   six native net8/9/10 Staging jobs, full/frozen214, build scripts,
   immutable test blob, frozen source and branch diff.
4. Update issue #102's recent footer and mark PR ready for user review.
