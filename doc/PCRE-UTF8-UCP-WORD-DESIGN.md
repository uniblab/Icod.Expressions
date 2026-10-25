# Internal PCRE UTF-8 word classes and boundaries

## Intent

Make the internal UTF-8 matcher recognize `\w`, `\W`, `\b`, and `\B` under Grep's UCP compile settings without using byte boundaries within a Unicode scalar. This prepares another portion of Grep's PCRE contract; it does not switch Grep away from PCRE2.

## Approaches

| Approach | Tradeoff | Decision |
|---|---|---|
| Reuse the existing byte word set at UTF positions | Simple, but treats a UTF continuation byte as the preceding character and misses Unicode letters, marks and numbers. | Reject. |
| Decode UTF scalars at each word-class atom and at each side of an asserted boundary | Preserves byte spans and the existing backtracking engine while implementing PCRE2 UCP word categories. | Choose. |
| Precompute a scalar map for every subject | Makes repeated boundary probes cheaper, but adds per-match storage and preprocessing before a performance baseline exists. | Defer. |

## Scope and contracts

- In UTF-8 UCP mode, `\w` matches categories L, N, Mn, Pc; `\W` matches the other valid scalars. This matches PCRE2 10.43+; in particular, Nd is only part of N, while combining marks and connector punctuation count as word characters. `AsciiBsD` affects `\d` only.
- In UTF-8 without UCP, `\w` recognizes ASCII letters, digits and underscore, and `\W` consumes one other scalar. Existing 8-bit non-UTF byte semantics are unchanged.
- `\b` and `\B` compare the classifications of the full scalar immediately before and after their byte position. Beginning/end of subject are nonword; no match may start inside a scalar. Nonzero start offsets do not hide the preceding scalar. Assertions consume no bytes and can participate in ordered alternatives and captures.
- Every instruction and each neighboring scalar classification uses the match work budget and cancellation checks. The existing strict UTF subject policy remains in force. Unicode properties come from the target .NET runtime; Unicode-version parity with the pinned PCRE2 remains a migration gate.
- Scoped options, `AsciiBsW`, UTF bracket unions/POSIX word classes, invalid UTF matching, Unicode caseless and public PCRE API remain outside this slice and continue to be explicitly unsupported where applicable.

## Follow-on

Do not migrate Grep or remove CommandFramework regex code until the remaining conformance, invalid-UTF, option, performance and consumer migration gates pass.
