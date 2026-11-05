# Positive lookahead mandatory class implementation plan

**Goal:** Preserve PCRE2 LF start-bitmap entry after CR for optional bracket
bodies beginning with positive lookahead and a mandatory CR-excluding class.

**Architecture:** Extend only the pre-scan in the existing bounded iterative
proof of `src/Pcre/PcreByteProgram.cs`; reuse the established class predicate
and charge range visits once at the mandatory leaf. Runtime is unchanged.

**Spec:** `doc/PCRE-UTF-POSITIVE-AHEAD-CLASS-DESIGN.md`.

1. Add pinned native full-capture reference cases to
   `tests/Pcre/PcreUtfPositiveAheadClassReferenceTests.cs`. Commit test-only
   blob, verify native PCRE2 10.48 JIT first-bitmap and captures, then observe
   successful-build RED limited to these cases with baseline 30,595 green.
2. In `PcreByteProgram.cs`, allow a direct scalar set as the first significant
   positive-ahead continuation when `HasUtfCrLfClassBitmap` succeeds against
   a copy of the visit count. The mandatory leaf retains its existing range
   charge. Keep all unsupported branches and conservative fallbacks.
3. Append design, syntax, resource and consumer inventory evidence and a
   recent progress row. Audit six-platform net8/9/10 full/frozen CI, configured
   gates, immutable test blob, frozen source checks and whole-branch diff.
4. Update only issue #102's recent footer, mark PR ready for user review and
   leave merging to the user.
