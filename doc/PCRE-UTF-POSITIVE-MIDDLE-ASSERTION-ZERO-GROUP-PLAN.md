# Positive middle assertion zero group-repeat implementation plan

**Goal:** Retain LF after CR through a positive literal assertion before
a nullable group repeat inside an optional bracket.

**Architecture:** Generalize the one-byte ASCII literal assertion
predicate used by `HasUtfCrLfLiteralAheadZeroGroupRepeatBitmap` so the
at-most-two-middle-node scan admits positive ahead/behind as well as
negative ahead/behind. Charge two visits for either polarity, retain
the boundary cost, the 1,000-visit limit and the iterative repeated
group head proof.

**Spec:** `doc/PCRE-UTF-POSITIVE-MIDDLE-ASSERTION-ZERO-GROUP-DESIGN.md`.

1. Add PCRE2 10.48 JIT full-capture fixtures for twelve prefixes,
   five group shapes, three subjects and strict/tolerant UTF. Commit
   test-only RED and freeze its blob.
2. Generalize only the intermediate literal assertion predicate,
   retaining the bounded scan and budget accounting.
3. Append evidence to four canonical inventories and recent index.
   Audit six native Staging jobs/net8/9/10, full/frozen214, build
   scripts, frozen35 sources and immutable tests.
4. Mark the PR ready for review. Issue #102 remains open.
