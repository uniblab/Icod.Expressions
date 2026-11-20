# Interpreted THEN assertion-scope oracle

Use the unchanged [match-policy probe](match-policy-oracle.c), Git blob
6fbaee449941be0b462604d3d1c4101d79241a37, with static native8 PCRE2 10.48 at
7978954dbd2efc6f2196869290553cf1871b4ce6 and SLJIT at
de0259c7aaf36aa40cba8014f3fad3edde9307f9. The independent Linux x64 build
used JIT, static libraries, and gcc -O2 -std=c11 -Wall -Wextra -Werror for
the probe.

All 48 catalog definitions use zero compiled defaults and supply all 64
policy masks. The 3,072 outcomes contain 2,304 matches and 768 misses.
Every JIT compilation returned zero with positive code size. Raw result
SHA-256 is
`e9b0c9a4c5902802ed1945b96e3c579410058f8cc8348231a38aae6343590f83`.
The catalog preserves every capture and successful-mark protocol.

Match-time anchor flags select interpreted native execution. Positive JIT
size records compilation provenance for those calls.

Twelve separate controls compile each combination of ANCHORED and
ENDANCHORED and pass only the requested call flags to pcre2_match.
Use the out-of-tree probe copy documented in the
[callee-cut oracle record](interpreted-cut-oracle.md), with SHA-256
e797256cf89b352526e818d43068c5df02e79c5c286e5a602a137642820a3690.
For the assertion-retry pattern in PcreInterpretedThenTests, the default
calls miss and either match-time anchor permits the longer capture.
The twelve results have SHA-256
`dc33c1e8cf8d060da8e7fc48172f68bd2e3ce1bc025675a6c262340247065615`.
The test source repeats the native checks before managed comparisons.
