# UTF-8 caseless ordinary brackets

**Status:** internal PCRE syntax slice under issue #7, following #36.

**Historical checkpoint:** Local r restrictions and the corrected POSIX/property
distinction are recorded in `PCRE-CASELESS-RESTRICTIONS-DESIGN.md`.

## Intent and bounds

Accept the existing ordinary UTF-8 bracket members under one leading `(?i)` or the caller's `Caseless` option: literal scalars, ranges, supported shorthands and Unicode general categories, and all fourteen ordinary POSIX class names. A bracket remains one scalar wide, preserves subject-byte spans, and never consumes across an invalid-UTF fragment barrier. Outer and inner complements apply after each member's caseless behavior.

Use the same provisional fold equivalence as UTF caseless literals. At compile time, map literal scalar ranges to their folded scalar keys and compress those keys into sorted disjoint intervals. Runtime matching folds the subject scalar once before inspecting those intervals, charging each inspection to the existing work budget. A 136 KiB transient bitmap bounds temporary memory even for a wide Unicode range; iterate only valid Unicode scalars and observe compile cancellation. Range preprocessing may still scan up to the Unicode scalar space per pattern, so startup performance remains a Grep migration gate. A full-space range is accepted directly without scanning.

Unicode `Lu`/`Ll`/`Lt` bracket predicates share `Lc` under caseless mode, as standalone properties do. UCP POSIX `lower`/`upper` remain Ll/Lu even under i, unlike explicit properties (verified pinned PCRE2 compilation/JIT). Without UCP, POSIX `lower`/`upper` expand to ASCII letters only; the other POSIX and shorthand predicates retain their established independent UCP policy. Byte mode and case-sensitive UTF behavior remain unchanged. No Grep or CommandFramework production change and no public PCRE capability claim.

## Approaches

| Approach | Tradeoff | Decision |
|---|---|---|
| Iterate a pattern range at every match attempt | Unbounded work for a wide bracket, disrupting match limits. | Reject. |
| Materialize a large hash set for every scalar in each class | Fast lookup but potentially megabytes per compiled pattern. | Reject. |
| Compress canonical fold keys into intervals once at compile time | Compact compiled program; bounded temporary memory and match work, with a measurable compilation cost. | Choose. |

## Verification

Test ASCII ranges, non-contiguous ranges, accented letters, Kelvin sign, long s, sigma, Turkish I, titlecase letters, `Lu`/`Ll`/`Lt`, negated members and whole bracket, POSIX UCP/non-UCP lower/upper, shorthands, work limit, cancellation, invalid UTF barriers, scalar offsets and caseful/byte regression. Run all six Staging jobs across net8/net9/net10, frozen GNU and package smoke. Pinned PCRE2 10.48 Unicode-table conformance remains open.
