# Negative assertion body implementation plan

**Goal:** Extend bounded LF entry to regular negative assertion bodies.
**Spec:** `doc/PCRE-UTF-NEGATIVE-ASSERTION-BODIES-DESIGN.md`.
**Execution:** Inline implementation/author review, no subagents.

## Review focus and constraints

Preserve existing class proof charges, literal and positive assertion paths,
mandatory consuming-head CR exclusion, cancellation,1,000 visited/queued work,
frozen35 and all43,059 baseline expectations. Check empty/nullable bodies,
capture rollback, atomic/possessive repeats, nested/deep groups, alternatives
and unsupported dependency nodes. No parser/matcher/runtime-layout/API/limit
or consumer changes; no SDK installation. Validate managed code through CI.

## Task 1: Native evidence and RED

- Add `tests/Pcre/PcreUtfNegativeAssertionBodiesReferenceTests.cs`:2,720
  native-first whole/capture fixtures, strict/tolerant managed comparison;
  assert all negative-body and unentered repeated-group captures are unset.
- Retain the local pinned10.48 probe result:2,720 bitmaps/positive JIT sizes,
  LF present/CR absent,5,440 complete literal expected capture comparisons.
- Push a tests-first draft PR; inspect a clean-build RED with all prior tests
  and ordinary controls passing. Preserve the new blob unchanged thereafter.

## Task 2: Bounded fallback and GREEN

- Add `HasUtfCrLfNegativeAssertionBitmap(PcreByteNode, ref int, int)` in
  `src/Pcre/PcreByteProgram.cs`; reuse the existing single-class path before
  the structural fallback and call it at the two existing assertion sites.
- Validate regular body nodes iteratively, with speculative local visit
  charges and bounded queue admission; reject dependencies/unknown nodes.
- Update four canonical contracts and recent-progress index. Verify native
  six-platform full/frozen214 suites, zero failures/skips/warnings/errors,
  all configured gates and seven equal workload comparisons.
- Author-review complete diff, immutable tests/frozen35, remote exact tree;
  record final results and mark ready. Full Compatibility keeps its existing
  path gate; user owns merge. Broader PCRE/release/consumer gates stay open.
