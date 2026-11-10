# Negated-range negative assertion implementation plan

**Goal:** Complete PR203's bounded LF-entry correction.
**Spec:** `doc/PCRE-UTF-NEGATED-RANGE-NEGATIVE-ASSERTION-DESIGN.md`.
**Execution:** Implement inline and author-review; no subagents.

1. [x] Inspect original test-only run610: six native runners, net8/9/10,
   exactly420 CRLF failures and40031 passes per framework; zero build warnings/errors.
2. [ ] Add48 separate native guard fixtures for excluded-range unions covering
   onlyCR, onlyLF, or neither, paired caseful/caseless, ahead/behind,
   leading/middle, CRLF/ordinary. Observe RED remains limited to the original420.
3. [ ] In `HasUtfCrLfNegativeClassAssertionBitmap` in
   `src/Pcre/PcreByteProgram.cs`, admit only predicate-free negated sets with
   valid nonempty excluded ranges covering bothCR/LF. Charge range visits and
   check cancellation; retain prefix cost and all surrounding guards.
4. [ ] Preserve original test blob and frozen35 source; require full40499 and
   frozen214 on each framework/platform, zero failed/skipped tests/warnings/errors,
   configured extraction/API/package/workload gates and build-script checks.
   Full source-pinned Compatibility remains subject to its existing path gate.
5. [ ] Update four canonical contracts and recent-progress index, review the
   complete PR diff and exact remote tree, then mark ready for user review.
   Keep issue102 open; no merge/tag/publication/consumer changes.
