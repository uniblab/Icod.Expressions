# Positive ahead group-repeat implementation plan

**Goal:** Retain LF first-bitmap entry after CR for proven mandatory
repeated group heads following positive ahead.

**Architecture:** Extend `HasUtfCrLfMandatoryBodyBitmap` and its direct
group helper in `src/Pcre/PcreByteProgram.cs` with one shared guard for
positive-minimum group repeats. Reuse bounded iterative all-branch proof;
keep speculative visit counts separate from actual traversal.

**Spec:** `doc/PCRE-UTF-POSITIVE-AHEAD-REPEATED-GROUP-DESIGN.md`.

1. Add PCRE2 10.48 JIT full-capture fixtures for six repeated group
   shapes and three subjects in strict and tolerant UTF. Commit RED and
   freeze the test blob.
2. Add a guarded repeated-group proof and preserve cancellation and the
   visit budget. Keep zero-minimum and nullable group controls outside
   this tranche.
3. Append evidence to four canonical inventories and recent index.
   Audit six native net8/9/10 Staging jobs, full/frozen214, build scripts,
   immutable test blob, frozen source and branch diff.
4. Mark the PR ready for user review. Issue #102 remains open.
