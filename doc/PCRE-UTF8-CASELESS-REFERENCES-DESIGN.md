# UTF-8 caseless backreferences and literal assertions

**Status:** internal PCRE syntax slice under issue #7, following #35.

## Intent and bounds

Allow the existing single-digit numbered backreferences and fixed literal lookahead/lookbehind assertions under UTF-8 caseless mode, whether selected by one leading `(?i)` or the caller's `Caseless` option. Compare decoded scalars using the existing provisional Unicode equivalence helper. A captured scalar and its reference may have different UTF-8 byte widths; consume subject bytes, keep the capture's original byte span, and never use captured byte length to determine the reference's subject length. Literal lookbehind measures its fixed width in scalars, walking back only within the current valid UTF fragment. Assertions remain zero-width. Keep byte-mode and case-sensitive UTF paths unchanged.

Do not cross malformed-subject fragment barriers. Charge attempted scalar comparisons and backwards boundary traversal to the match work limit; cancellation remains observable. Caseless UTF brackets remain explicitly unsupported. No Grep or CommandFramework production change and no public PCRE capability claim.

## Approaches

| Approach | Tradeoff | Decision |
|---|---|---|
| Fold captured or asserted UTF-8 bytes one at a time | Fails when case-equivalent scalars have distinct byte encodings or lengths. | Reject. |
| Decode and compare source scalars when the opcode executes | Preserves capture and subject offsets; a small extra decoding cost stays in the existing work budget. | Choose. |
| Precompute every Unicode fold equivalent and byte length at compile time | Adds representation and Unicode version policy before pinned PCRE2 conformance. | Defer. |

## Verification

Test equivalences of one, two and three UTF-8 bytes, both comparison directions, Greek sigma, Turkish I isolation, captures, ordered backtracking, positive/negative lookahead and lookbehind, empty assertions, invalid UTF barriers, start offsets, work limits and cancellation. Verify caseful UTF and byte caseless regressions. Run six Staging jobs with net8/net9/net10, frozen GNU corpus and package smoke.

Pinned Unicode 10.48 conformance and caseless UTF bracket matching remain gates before Grep migration.
