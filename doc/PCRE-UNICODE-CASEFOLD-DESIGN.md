# Pinned Unicode simple-case equivalence

## Intent and approach

Continue issue7 after PR207. Categories now use pinned Unicode 17.0.0 data,
but `PcreUtf8CaseFold` still uses host Rune casing and a few special cases.
Replace that provisional equivalence with the complete Unicode 17 C/S mapping
used by PCRE2 10.48. This is single-scalar simple folding: multi-scalar F and
optional Turkic T mappings are outside the currently supported contract.

Use a generated immutable mapping table, an ASCII fast path and bounded binary
search. Keep standard C/S fold targets so most existing canonical keys remain
unchanged. The 1,486 non-ASCII mappings fit in 11,888 data bytes: each ulong
packs a 21-bit source and target. Retain `Fold`, `Equals`, `CloseRanges`, local
r policy, byte-UCP projection, compiler cancellation and all VM work/limits.
No runtime file/native dependency, public API or new matcher state.

A separate C# maintenance tool validates the exact Unicode17 CaseFolding.txt
SHA256 `ff8d8fefbf123574205085d6714c36149eb946d717a0c585c27f0f4ef58c4183`.
Linux CI verifies byte-for-byte regeneration alongside category data; extend the
existing Unicode notice to include this source. Keeping runtime casing leaves
host-version and round-trip gaps. A native fallback violates the engine contract.
Changing the VM or unrelated optimizer proofs is unnecessary for this increment.

## Independent reference and scope

Native research uses PCRE2 tag pcre2-10.48, commit
`7978954dbd2efc6f2196869290553cf1871b4ce6`. Export the minimum scalar in each
UCD_CASESET / UCD_OTHERCASE equivalence class, excluding ASCII/non-ASCII mixing
when restricted. Serialize one little-endian int32 per valid scalar in ascending
order, skipping only surrogates. The default SHA256 is
`bc64140b4140e63f4f5b29612a7605eaa1e11c7daa1745f0d06a0f9478ec161e`;
restricted is `b4f0a081aa1123671d63192f3625681fcc3989d28d881c120f7908ad6081c3d0`.
Independent C/S parsing agrees with both native exports at every valid scalar.

Tests normalize equivalence classes to minimum representatives before hashing,
so canonical key choice is not the oracle. Add literal native outcomes for new
pairs, multi-member sets, direct/range/bracket/reference/assertion paths, scoped
r behavior and invalid-UTF offsets. Observe successful-build RED before source
changes; preserve the test blob. Existing baseline: 45,811 tests per framework,
frozen214 (120 methods), frozen35. No subagents, Python, SDK or PowerShell
installation. Managed verification runs in six-platform/net8/9/10 CI.

This closes default simple-case table ownership. Extra Turkic modes, full-string
folding, scripts/binary properties, graphemes/class algebra, optimizer gaps and
API/release/consumer gates remain distinct. Grep stays last.
