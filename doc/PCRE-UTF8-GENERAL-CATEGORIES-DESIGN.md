# UTF-8 Unicode general categories

**Status:** internal PCRE syntax slice under issue #7.

## Intent and bounds

Extend `\p`/`\P` from the existing `L` and `Nd` cases to PCRE2's abbreviated Unicode general categories: `C`, `L`, `M`, `N`, `P`, `S`, `Z` and their listed two-character subcategories, including `Lc`/`L&`. Use the categories both as standalone scalar atoms and as members of ordinary UTF-8 brackets. Preserve the existing byte spans, predicate work accounting, invalid-UTF fragment barriers, and independent `Ucp`/`AsciiBsD` options. No Grep or CommandFramework production change and no public PCRE capability claim.

The braces may be omitted for a one-letter category (`\pN`). A leading `^` inside braces and upper-case `\P` each complement once. Property names use PCRE2 loose matching of ASCII case, hyphen, underscore and ASCII space; `gc:`/`gc=` and `General_Category:`/`General_Category=` specify the same short category value. Other property types (scripts, binary properties, `Xan` etc.) remain explicit unsupported diagnostics. Missing names/braces remain invalid syntax. PCRE2's case-independent `Lu`/`Ll`/`Lt` equivalence is deferred together with UTF caseless support. `Cs` is recognized but has no matching UTF-8 scalar; `Cn` follows the target runtime's Unicode data and may differ from a pinned PCRE2 release. See [PCRE2 Unicode property details](https://www.pcre.org/current/doc/html/pcre2pattern.html#SEC9).

## Approaches

| Approach | Tradeoff | Decision |
|---|---|---|
| Generate a separate property table | Ties package size and behavior to a specific Unicode release. | Defer until version-wide conformance. |
| Encode each two-letter category as an AST and opcode variant | Large enum and redundant cases. | Reject. |
| Carry a specific `UnicodeCategory` value alongside a general-category group kind | Fits the existing scalar predicate path and checks one scalar once. | Choose. |

## Verification

Cover each group and representative subcategories, `Lc`/`L&`, `Cs`/`Cn`, UCP on/off, loose spelling and `gc` forms, both negations, bracket union and range diagnostics, invalid subject barriers, captures, work limits, cancellation, and unsupported property types. Staging must pass on six OS/architecture jobs with net8/net9/net10, the frozen GNU corpus and package smoke. The Unicode data/version conformance gate remains open.
