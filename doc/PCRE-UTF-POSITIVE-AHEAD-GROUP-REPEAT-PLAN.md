# Positive lookahead group repeat head implementation plan

**Goal:** Retain native LF first-bitmap entry after CR when positive ahead
precedes a one-level mandatory group with proven repeat heads.

**Architecture:** Extend `HasUtfCrLfDirectGroupBitmap` in
`src/Pcre/PcreByteProgram.cs` to recognize existing mandatory literal,
class and type repeat predicates, using the caller's copied visit budget.
The iterative body proof remains the final authority. No runtime change.

**Spec:** `doc/PCRE-UTF-POSITIVE-AHEAD-GROUP-REPEAT-DESIGN.md`.

1. Pin PCRE2 10.48 JIT full-capture fixtures in
   `tests/Pcre/PcreUtfPositiveAheadGroupRepeatReferenceTests.cs` for strict
   and tolerant UTF. Commit tests first, observe a successful-build RED
   confined to this class, and freeze the test blob.
2. Accept each nonempty branch's first repeat only if a positive-minimum
   literal, bounded range class or supported type passes an existing proof.
   Reuse cancellation and bounded visits; keep the pre-scan speculative.
3. Append evidence to four canonical inventories and recent index. Audit
   six native net8/9/10 Staging jobs, frozen 214, build scripts, immutable
   tests, frozen source and entire branch diff.
4. Update issue #102's recent footer and mark the PR ready for review.
   The user merges it.
