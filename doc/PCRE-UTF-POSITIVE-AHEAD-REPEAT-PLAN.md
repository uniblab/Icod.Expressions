# Positive lookahead repeated literal implementation plan

**Goal:** Preserve PCRE2 LF start-bitmap entry after CR for optional bracket
bodies beginning with positive lookahead and a mandatory literal repeat.

**Architecture:** Extend only the bounded iterative proof in
`PcreByteProgram.cs`, retaining the established literal bitmap predicate and
falling back on any unsupported branch. Runtime execution is unchanged.

**Spec:** `doc/PCRE-UTF-POSITIVE-AHEAD-REPEAT-DESIGN.md`.

1. Add literal native capture fixtures in
   `tests/Pcre/PcreUtfPositiveAheadRepeatReferenceTests.cs`; verify the pinned
   PCRE2 10.48 JIT bitmap and captures, commit immutable tests and observe a
   successful-build RED confined to this class with baseline 30,560 green.
2. In `src/Pcre/PcreByteProgram.cs`, admit a positive-minimum direct literal
   repeat (or its possessive atomic wrapper) in the positive-ahead continuation
   check and mandatory-body leaf proof. Reject minimum zero and other operands.
   Keep cancellation, finite proof budget, all-branch and fallback behavior.
3. Append the evidence to the four canonical PCRE inventories and one recent
   progress row. Audit six-platform net8/9/10 full/frozen CI, configured gates,
   immutable test blob, clean tree and whole-branch self-review.
4. Update only issue #102's recent footer, mark the PR ready, leave merge and
   publication to the user.
