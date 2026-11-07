# Positive ahead zero-minimum group-repeat implementation plan

**Goal:** Preserve LF first-bitmap entry after CR for a literal positive
ahead immediately followed by a supported zero-minimum group repeat.

**Architecture:** Add a narrow two-node branch proof to
`HasUtfCrLfMandatoryBodyBitmap` in `src/Pcre/PcreByteProgram.cs`, reusing
the iterative `HasUtfCrLfDirectGroupBitmap` and visit budget. The branch
must match the known native assertion/repeat form; runtime execution is
untouched.

**Spec:** `doc/PCRE-UTF-POSITIVE-AHEAD-ZERO-GROUP-REPEAT-DESIGN.md`.

1. Add PCRE2 10.48 JIT full-capture fixtures for five repeated group
   shapes and three subjects in strict and tolerant UTF. Commit RED and
   freeze the test blob.
2. Implement the bounded literal-ahead and zero-minimum group-repeat
   proof. Preserve conservative fallback for unsupported branches.
3. Append evidence to four canonical inventories and recent index.
   Audit six native net8/9/10 Staging jobs, full/frozen214, build scripts,
   immutable tests, frozen source and branch diff.
4. Mark the PR ready for user review. Issue #102 remains open.
