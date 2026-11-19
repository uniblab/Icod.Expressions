# Interpreted word boundary oracle

The [match-policy probe](match-policy-oracle.c) uses blob
`6fbaee449941be0b462604d3d1c4101d79241a37` against pinned static native8 PCRE2
10.48 `7978954dbd2efc6f2196869290553cf1871b4ce6` and SLJIT
`de0259c7aaf36aa40cba8014f3fad3edde9307f9`.

The corpus covers 48 definitions by 64 policies, with 920 matches and 2,152
misses. All patterns compiled for JIT with positive code size. Match-time
anchor flags select interpreted execution. The raw results have SHA-256
`77b2da1083a97236ad9a472558c2010bd78de233f1cd1bf75af7ec8d5d9603b3`.

The interpreter classifies both edges of an empty initial fragment as
nonword. The corpus includes direct and nested boundaries, malformed runs
and continuation recovery, later alternatives, and captures and marks.
