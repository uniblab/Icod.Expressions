# Positive lookahead skipped-prefix implementation plan

1. Generate immutable native full-capture fixtures from pinned PCRE2 10.48;
   check JIT sizes, capture completeness and byte bounds. Commit tests and
   this plan, then audit a successful-build RED on the unchanged production.
2. In `HasUtfCrLfMandatoryBodyBitmap`, recognize a positive lookahead only if
   the first significant node after a finite chain of skippable assertions
   and word boundaries is a valid CR-excluding literal. Charge each existing
   prefix visit once, with cancellation and pre-addition guards. Preserve
   failed-proof checkpoints and the existing head/owner behavior.
3. Add controls for unsupported continuations, proof-budget fallback,
   exact work/cancellation and ordinary matching if native evidence requires
   them. Keep the new test blob frozen after RED.
4. Append canonical histories and one recent index row without altering prior
   content. Audit six platform jobs and the build-script checks at the exact
   source head. Self-review, update only issue #102's recent footer, and mark
   the PR ready for user merge. No merge, tag or publication here.
