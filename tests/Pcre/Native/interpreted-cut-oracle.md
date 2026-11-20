# Interpreted callee-cut capture oracle

Use the unchanged [match-policy probe](match-policy-oracle.c), Git blob
6fbaee449941be0b462604d3d1c4101d79241a37, with static native8 PCRE2 10.48 at
7978954dbd2efc6f2196869290553cf1871b4ce6 and SLJIT at
de0259c7aaf36aa40cba8014f3fad3edde9307f9. The independent Linux x64 build
used configure with JIT enabled and shared libraries disabled, make's
libpcre2-8.la target, and gcc -O2 -std=c11 -Wall -Wextra -Werror for the probe.

Each catalog definition supplies all 64 policy masks. All definitions use
zero compiled defaults. The 3,072 outcomes contain 2,816 matches and 256
misses. Every JIT compilation returned zero with positive code size. Raw
result SHA-256 is
`554a8261d064297d18b1829450ca820e8daff0495939ee41991fd168e0c270f6`.
The catalog preserves the native capture and successful-mark protocols.

Match-time anchor flags disable native JIT dispatch. Positive JIT size
records compilation provenance for those calls.

A separate twelve-outcome control compiles each combination of ANCHORED
and ENDANCHORED and passes only the requested call flags to pcre2_match.
For this control alone, replace policy(defaults|call) with policy(call) in
an out-of-tree copy of the probe. The copy has SHA-256
`e797256cf89b352526e818d43068c5df02e79c5c286e5a602a137642820a3690`.
Its results have SHA-256
`d1b567bdbd5935f6008f759301c8ef0816a7107a98b37f5255615716c4ffe873`.
The group v is set on default calls and unset with either match-time anchor.
The assertion source repeats those native checks before managed comparisons.
