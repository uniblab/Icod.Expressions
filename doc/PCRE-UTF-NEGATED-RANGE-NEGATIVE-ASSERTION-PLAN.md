# Negated-range negative assertion implementation plan

**Goal:** Complete PR203's bounded LF-entry correction.
**Spec:** `doc/PCRE-UTF-NEGATED-RANGE-NEGATIVE-ASSERTION-DESIGN.md`.
**Execution:** Implement inline and author-review; no subagents.

1. Preserve original test-only run610 and its immutable840-fixture blob.
   It compiled cleanly on all six native runners; each framework failed
   exactly420 CRLF cases and passed40031 others, with zero skips.
2. Add48 native guard fixtures for excluded ranges covering onlyCR, onlyLF,
   or neither, paired caseful/caseless, ahead/behind, leading/middle and
   CRLF/ordinary. Observe test-only RED before the source fix.
   Run611 revealed24 more CRLF failures:444 failed/40055 passed per inspected
   framework. Preserve the guard corpus too; native study skips these bodies
   independently of their newline membership.
3. In `HasUtfCrLfNegativeClassAssertionBitmap` in
   `src/Pcre/PcreByteProgram.cs`, admit predicate-free negated sets with
   valid nonempty excluded ranges. Charge each range visit and check
   cancellation; retain prefix cost, mandatory CR-excluding repeated-group
   head and all surrounding guards. Positive assertion admission is unchanged.
4. Preserve test blobs and frozen35 source; require full40499 and frozen214
   on each framework/platform, zero failed/skipped tests/warnings/errors,
   configured extraction/API/package/workload gates and build-script workflow.
   Full source-pinned Compatibility retains its existing path gate. CI is the
   executable validation environment because the local workspace has no SDK.
5. Update four canonical contracts and recent-progress index, author-review
   the complete PR diff, and verify the exact remote source tree and final CI
   before marking ready for user review. Record run IDs and final outcomes in
   the PR body; keep issue102 open. No merge/tag/publication/consumer changes.
