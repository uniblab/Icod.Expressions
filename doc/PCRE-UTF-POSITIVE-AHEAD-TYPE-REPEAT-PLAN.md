# Positive lookahead supported-type repeat implementation plan

**Goal:** Preserve PCRE2 LF first-bitmap entry after CR for optional bracket
bodies beginning with positive lookahead and a mandatory supported-type repeat.

**Architecture:** Extend only the bounded proof in `src/Pcre/PcreByteProgram.cs`.
Reuse `HasUtfCrLfTypeBitmap` for a direct `PcreByteScalarClass` repeat and its
possessive wrapper. Do not change runtime matching.

**Spec:** `doc/PCRE-UTF-POSITIVE-AHEAD-TYPE-REPEAT-DESIGN.md`.

1. Pin native PCRE2 10.48 JIT first-bitmap and full capture cases in
   `tests/Pcre/PcreUtfPositiveAheadTypeRepeatReferenceTests.cs`. Commit tests
   first, observe successful-build RED limited to the new class with 30,673
   baseline cases green and freeze the test blob.
2. Add a positive-minimum repeat predicate delegating to the existing type
   predicate. Use it in the positive-ahead pre-scan and mandatory leaf alongside
   existing literal and class repeat checks. Keep cancellation, bounds,
   all-branch proof and conservative fallback.
3. Append exact scope/evidence to the four canonical PCRE inventories and
   recent progress index. Audit six-platform net8/9/10 full/frozen CI,
   configured gates, immutable tests, frozen source and whole-branch diff.
4. Update only issue #102's recent footer, mark the PR ready for user review
   and leave merging to the user.
