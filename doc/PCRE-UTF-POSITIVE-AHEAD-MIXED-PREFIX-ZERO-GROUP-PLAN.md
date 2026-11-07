# Mixed-prefix zero group-repeat implementation plan

**Goal:** Retain LF after CR through two short zero-width nodes between
a positive literal lookahead and a nullable group repeat.

**Architecture:** Replace the exact middle-position checks in
`HasUtfCrLfLiteralAheadZeroGroupRepeatBitmap` with a bounded loop over
at most two intermediates. Accept only `PcreByteWordBoundary` or the
existing one-byte ASCII negative literal assertion predicate. Charge
the respective 1/2 visits under the existing 1,000-visit limit; keep
the iterative mandatory group proof and earlier paths unchanged.

**Spec:** `doc/PCRE-UTF-POSITIVE-AHEAD-MIXED-PREFIX-ZERO-GROUP-DESIGN.md`.

1. Add PCRE2 10.48 JIT full-capture fixtures for ten two-node prefixes,
   five group shapes, three subjects, strict/tolerant UTF. Commit
   test-only RED and freeze the test blob.
2. Extend the bounded proof and charge each accepted middle node;
   reject other nodes and longer chains.
3. Append exact evidence to four canonical inventories and recent index.
   Audit all six native Staging jobs/net8/9/10, full/frozen214, build
   scripts, frozen35 source and immutable tests.
4. Mark the PR ready for user review. Issue #102 remains open.
