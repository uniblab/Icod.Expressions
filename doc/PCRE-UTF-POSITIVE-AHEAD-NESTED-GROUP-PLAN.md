# Positive lookahead one-nested-group implementation plan

**Goal:** Preserve LF first-bitmap entry after CR through one nested
mandatory group head following positive lookahead.

**Architecture:** Give `HasUtfCrLfDirectGroupBitmap` in
`src/Pcre/PcreByteProgram.cs` a bounded nesting depth. It may check one
inner unquantified group after supported prefixes, applying the existing
all-branch direct/repeat proof. Outer speculative visits remain a copy;
the existing iterative body proof owns actual visits.

**Spec:** `doc/PCRE-UTF-POSITIVE-AHEAD-NESTED-GROUP-DESIGN.md`.

1. Pin PCRE2 10.48 JIT full-capture fixtures in
   `tests/Pcre/PcreUtfPositiveAheadNestedGroupReferenceTests.cs`; strict
   and tolerant, test-only commit and new-class-only RED before production.
2. Accept one inner group under bounded visits and cancellation, reject
   additional nesting, nullable/repeated groups and unsupported heads.
3. Append evidence to four canonical inventories and recent index. Audit
   six native Staging jobs on net8/9/10 full/frozen214, build scripts,
   immutable test blob, frozen source and branch diff.
4. Update issue #102's recent footer and mark PR ready for user review.
