# UTF-8 caseless literals and Unicode letter categories

**Status:** internal PCRE syntax slice under issue #7; stacked after general categories (#34).

## Intent and bounds

Enable the internal UTF-8 matcher to recognize one leading `(?i)` and the caller's `Caseless` option. Literals compare whole Unicode scalars using simple case equivalence, preserving source byte spans even when equivalent characters have different UTF-8 widths. In this first slice, standalone `\p{Lu}`, `\p{Ll}` and `\p{Lt}` (and their complements) use the shared `Lc` set under caseless mode, as PCRE2 10.45+ does. Other category predicates and ASCII/UCP shorthand behavior stay independent of caseless mode. Invalid UTF fragments, work limits and cancellation keep their existing contracts.

Caseless UTF bracket expressions, backreferences and literal assertions require additional handling (case-equivalent ranges, variable-width captured text and variable-width lookbehind). Diagnose those constructs as `UnsupportedSyntax` when UTF caseless is selected until separate slices implement them. Existing case-sensitive UTF and byte-mode behavior remain unchanged. No Grep or CommandFramework production changes and no public PCRE capability claim.

The provisional equivalence canonicalizes through `Rune.ToUpperInvariant` then `Rune.ToLowerInvariant`, with explicit Unicode simple-fold treatment for Greek final sigma, Kelvin sign and long s, and isolation of dotted/dotless Turkish I in the default mode. Unicode-version parity and exhaustive comparison to the pinned PCRE2 10.48 Unicode tables remain a migration gate. Special PCRE2 flags for Turkish casing and caseless restriction remain unsupported.

## Approaches

| Approach | Tradeoff | Decision |
|---|---|---|
| Enable the existing ASCII fold for all UTF constructs | Loses Unicode equivalences and silently mishandles variable byte widths. | Reject. |
| Generate and pin a complete Unicode fold table now | Gives deterministic PCRE2 version parity but adds significant package data and version maintenance before conformance fixtures are ready. | Defer to conformance work. |
| Add a UTF scalar fold path and explicitly reject unimplemented caseless constructs | Delivers useful literal/property behavior with no silent false results in the remaining constructs. | Choose for this slice. |

## Verification

Test leading and caller caseless modes; ASCII, accented letters, Kelvin, long s, Greek sigma and Turkish I; matched byte spans; caseful contrast; independent UCP behavior; `Lu`/`Ll`/`Lt` complements and bracket diagnostics; invalid UTF fragments; work limits and cancellation. Run all six Staging jobs on the exact head, including net8/net9/net10, frozen GNU corpus and package smoke.
