# Positive lookahead repeated range-class implementation plan

**Goal:** Preserve PCRE2 LF first-bitmap entry after CR for optional bracket
bodies beginning with positive lookahead and a mandatory range-class repeat.

**Architecture:** Extend only `src/Pcre/PcreByteProgram.cs`'s bounded proof,
reusing its class predicate. Probe against a copied visit count and charge
ranges once at the mandatory leaf. Runtime matching is unchanged.

**Spec:** `doc/PCRE-UTF-POSITIVE-AHEAD-CLASS-REPEAT-DESIGN.md`.

1. Add pinned native full-capture cases in
   `tests/Pcre/PcreUtfPositiveAheadClassRepeatReferenceTests.cs`; verify PCRE2
   10.48 JIT bitmap/captures, commit immutable tests and observe successful-
   build RED limited to this class while all 30,631 baseline cases pass.
2. Add a positive-minimum class-repeat predicate that unwraps a possessive
   atomic repeat and delegates to `HasUtfCrLfClassBitmap`. Use it at the
   positive-ahead direct-continuation pre-scan with a copy of `visits` and at
   the mandatory repeat leaf with the real `visits`. Keep the literal-repeat
   and unsupported paths intact.
3. Append scope/evidence to four canonical PCRE inventories and the recent
   progress index. Audit six-platform net8/9/10 full/frozen CI, configured
   gates, immutable test blob, frozen source check and whole-branch diff.
4. Update only issue #102's recent footer, mark the PR ready for user review
   and leave merging to the user.
