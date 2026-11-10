# Negative class bitmap implementation plan

**Goal:** Preserve native LF entry for scalar-class negative assertion bodies.
**Spec:** `doc/PCRE-UTF-NEGATIVE-CLASS-BITMAP-DESIGN.md`.
**Execution:** Inline implementation and author review; no subagents.

## Constraints and review focus

Preserve parser source metadata, positive assertion/head admission, mandatory
CR exclusion in the repeated group, cancellation, 1,000 proof visits, API,
runtime state, frozen35 and all40499 baseline test expectations. Cover scalar
predicates with no ranges, mixed ranges/predicates, complemented classes,
UCP/caseless variants and captured/atomic/possessive repeated groups.

## Task 1: Native reference and RED

- Add `tests/Pcre/PcreUtfNegativeClassBitmapReferenceTests.cs` using the pinned
  PCRE.NET1.6.0 reference. Assert literal expected native whole/capture spans
  before comparing managed strict/tolerant results.
- Pin native10.48 start bitmap LF/CR bits and JIT sizes for the same patterns.
- Push tests and docs to a draft PR; inspect successful-build RED on CI.
  Keep the new test blob immutable after this observation.

## Task 2: Bounded proof and GREEN

- In `src/Pcre/PcreByteProgram.cs`, extend only
  `HasUtfCrLfNegativeClassAssertionBitmap(PcreByteNode, ref int, int)` to parsed
  scalar classes and valid nonempty range/predicate scalar sets. Charge each
  range and predicate; preserve assertion and middle-opcode costs.
- Update four canonical PCRE inventories and recent-progress index.
- Require six native Staging jobs: full suite/frozen214 on net8/9/10, zero
  failures/skips/warnings/errors and all configured extraction/API/package/
  workload checks. Full Compatibility keeps its configured path gate.
- Review the complete diff, frozen35, immutable test blob, and exact remote
  merge tree. Record final CI in the PR and mark ready; user owns merge.
