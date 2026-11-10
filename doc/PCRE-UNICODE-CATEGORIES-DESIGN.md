# Pinned Unicode general categories

## Intent and scope

Close the host-version category gap in roadmap issue7. PCRE.NET1.6.0 embeds
PCRE2 10.48, whose category data is Unicode17.0.0. Our scalar classes and UTF
capture-name parser currently consult .NET/OS Unicode data. Identical patterns
can therefore reject recently assigned letters or match their old Cn category.

Use a generated immutable table derived from Unicode17's
`extracted/DerivedGeneralCategory.txt`, matching the data bundled at PCRE2 tag
`pcre2-10.48` (commit7978954dbd2efc6f2196869290553cf1871b4ce6).
Record its SHA256 and distribute the Unicode permission notice with the package.
A standalone C# regeneration tool validates that input hash and emits packed
range starts/category values. Static span data and binary search require no
runtime file, native library, category service or per-query allocation. Keep an
ASCII fast path for existing workloads. The generated file owns only data;
lookup and classification remain ordinary reviewed C#.

Route scalar categories, UTF capture-name letters/digits and Unicode-digit
subroutine diagnostics through the table. Existing property complements,
caseless category widening, UCP/ASCII restrictions, byte projection, spans,
resource charging and cancellation remain shared. No new syntax or VM state.

## Alternatives and boundaries

Continuing with host categories leaves the documented cross-platform gap.
Bundling a full Unicode engine or querying native PCRE2 would add an unnecessary
runtime dependency. A compact generated category table addresses this specific
contract and establishes reproducible data ownership for later Unicode work.
Full case folding, scripts/binary properties, grapheme clusters, class algebra,
UTF16/API/release review and Grep migration remain separate gates. Grep stays
last; no change to GNU/BRE/ERE or frozen35.

## Evidence and validation

Before production changes, add an exhaustive fingerprint over all1,112,064
valid scalars via existing class membership. Derive the expected bytes directly
from PCRE2's native UCD table using the explicit .NET category-number mapping;
SHA256 is `2dbbcf96f6b264878e1862474e8a7c214eb6d1eca5e3eba4b0b99e30ca28ec75`.
Add literal recent assignments, complements/brackets/caseless modes, live JIT
category sampling and capture-name grammar cases. Tests must build and fail
because the old categories differ; preserve that test blob after RED. Native
research is a scratch C probe, not shipped code or a production fallback.

Use C#13/net8/9/10 and all six native OS/architectures (Intel frameworks now
separate jobs). No subagents, Python, SDK or PowerShell installation. CI supplies
managed execution. Preserve all45,779 baseline tests and frozen214 results.

Native research compared every valid scalar from the pinned Unicode source
against the native UCD table: zero mismatches. The table needs4,116 non-ASCII
ranges plus128 ASCII bytes (16,592 data bytes). Test-only CI620 confirmed the
expected successful-build RED; test blob remains immutable at
`18e46522c52a6750b83a147350e7b48ae64f65f3`.
