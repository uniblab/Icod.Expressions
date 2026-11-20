# Compiled-anchor reference routing

Build static native8 PCRE2 10.48 at
7978954dbd2efc6f2196869290553cf1871b4ce6 with SLJIT at
de0259c7aaf36aa40cba8014f3fad3edde9307f9 and JIT enabled. Compile the
[policy probe](match-policy-oracle.c) with gcc -O2 -std=c11 -Wall -Wextra -Werror.

The corrected probe passes policy((defaults & ~3u)|call) to pcre2_match.
Compile-time anchor defaults enter pcre2_compile. The caller's anchor flags
and all other effective restrictions enter pcre2_match.

The independent Linux x64 preflight records 48 definitions by 64 call masks,
with 2,732 matches and 340 misses. Every JIT compilation returned zero with
positive size. The raw results have SHA-256
`c77e7a9a083826f41759581d7dfbef82cce00547e5d38bac099fe360f96168ce`.
The corrected source is Git blob 9e4a61ee74b61111b8153deef7d0e2aa90c3e9ee.

The original probe, blob 6fbaee449941be0b462604d3d1c4101d79241a37, routes
compiled anchors back through the match flags. Its output for this corpus has
SHA-256
`1986bbaf0dcf01cd78e404e5dffbe9183857551ecf1c240aa438510c8e1eea80`.
It differs on 448 outcomes across definitions 1–30. All rows in both runs
completed, with no discarded rows or compile/match errors.

The corrected probe also reproduces every literal protocol in the original
56-definition, 3,584-outcome catalog. The remaining existing policy catalogs
have no compiled anchor defaults, for which the option mappings coincide.
The reproduced raw output has the original SHA-256
`75168c42dd34448d1c4119dccf2193e8a6a27d084789e5ca427d795217cf57d3`.

Match-time anchor flags disable native JIT dispatch. Positive code size
records successful JIT compilation for those calls.
