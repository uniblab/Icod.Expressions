# Double-boundary zero group-repeat implementation plan

**Goal:** Retain LF after CR through two word boundaries between a literal
positive lookahead and a nullable group repeat in the UTF first bitmap.

**Architecture:** Extend the exact shape in
`HasUtfCrLfLiteralAheadZeroGroupRepeatBitmap` to recognize two consecutive
`PcreByteWordBoundary` nodes. Reuse the bounded iterative group proof and
retain existing adjacent and single-boundary behavior.

**Spec:** `doc/PCRE-UTF-POSITIVE-AHEAD-DOUBLE-BOUNDARY-ZERO-GROUP-DESIGN.md`.

1. Add PCRE2 10.48 JIT full-capture fixtures for two boundary forms, five
   group shapes and three subjects under strict and tolerant UTF. Commit
   test-only RED and freeze the test blob.
2. Extend the bounded shape proof, charging both boundaries without
   permitting a longer or different intermediate chain.
3. Append exact evidence to four canonical inventories and recent index.
   Audit six native Staging OS/architecture jobs on net8/9/10, full suite,
   frozen214, build scripts, frozen35 sources and immutable tests.
4. Mark the PR ready for user review. Issue #102 remains open.
