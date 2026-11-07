# Boundary-prefixed zero group-repeat implementation plan

**Goal:** Preserve LF first-bitmap entry after CR when one word boundary
separates a literal positive lookahead and a zero-minimum group repeat.

**Architecture:** Extend the exact branch shape in
`HasUtfCrLfLiteralAheadZeroGroupRepeatBitmap` (`src/Pcre/PcreByteProgram.cs`)
to accept one intermediate `PcreByteWordBoundary` and charge its visit.
Reuse the bounded iterative group proof and retain adjacent behavior.

**Spec:** `doc/PCRE-UTF-POSITIVE-AHEAD-BOUNDARY-ZERO-GROUP-DESIGN.md`.

1. Add PCRE2 10.48 JIT full-capture fixtures for five group shapes and
   three subjects in strict and tolerant UTF. Commit test-only RED and
   freeze its blob.
2. Extend the bounded proof by one boundary visit. Preserve conservative
   fallback for other intermediate nodes.
3. Append evidence to four canonical inventories and recent index.
   Audit six native net8/9/10 Staging jobs, full/frozen214, build scripts,
   immutable tests, frozen source and branch diff.
4. Mark the PR ready for user review. Issue #102 remains open.
