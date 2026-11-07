# Negative assertion zero group-repeat implementation plan

**Goal:** Retain LF after CR when one negative literal assertion separates
a positive literal lookahead and a nullable group repeat.

**Architecture:** Extend the exact branch shape in
`HasUtfCrLfLiteralAheadZeroGroupRepeatBitmap` to recognize one negative
single-byte ASCII `PcreByteLiteralAssertion`, ahead or behind, in the
middle position. Reuse the bounded group proof and retain earlier adjacent
and boundary behavior.

**Spec:** `doc/PCRE-UTF-POSITIVE-AHEAD-NEGATIVE-ASSERTION-ZERO-GROUP-DESIGN.md`.

1. Add PCRE2 10.48 JIT full-capture fixtures for two assertion forms,
   five group shapes and three subjects in strict and tolerant UTF.
   Commit test-only RED and freeze its test blob.
2. Extend the bounded proof to accept exactly one negative literal
   assertion and charge its visit without admitting other middle nodes.
3. Append evidence to four canonical inventories and recent index.
   Audit six native Staging OS/architecture jobs/net8/9/10, full suite,
   frozen214, build scripts, frozen35 sources and immutable tests.
4. Mark the PR ready for user review. Keep issue #102 open.
