# Anchored-fragment native preflight

The original [C probe](anchored-fragment-oracle.c) builds against pinned native
PCRE2 10.48/SLJIT as in [README](README.md), with native8/JIT only required:

```sh
../pcre2/configure --enable-jit --disable-shared --enable-static --disable-pcre2-16 --disable-pcre2-32
make -j4 libpcre2-8.la
cc -O2 -std=c11 -Wall -Wextra -Werror -I src -I ../pcre2/src \
  /path/to/Icod.Expressions/tests/Pcre/Native/anchored-fragment-oracle.c \
  .libs/libpcre2-8.a -o anchored-fragment-oracle
./anchored-fragment-oracle
```

PCRE2 commit7978954dbd2efc6f2196869290553cf1871b4ce6 and SLJIT
commitde0259c7aaf36aa40cba8014f3fad3edde9307f9 produce1,568 short protocols,
grouped as364 all-offset fixture rows, and12 large-profile outcomes. Every
compile has JIT return0/positive size. UTF/MATCH_INVALID_UTF is explicit;
caller anchoring is independent of dot-star source anchoring. Native output
includes status, complete numbered capture spans and JIT facts. Default LF/Unicode
BSR are retained. All declared inputs/results are included; no mismatch is dropped.

Large subjects alternate A/FF, at1,024/65,536 bytes. Caller-anchor match/miss,
late caller offset, nullable invalid requested byte and dot-star match/miss
profiles preserve original byte offsets. Their native captures are literal in
PcreFragmentAllocationCases.cs. Short literals remain in PcreAnchoredFragmentCases.cs.
Native output SHA256:
`307808a233ae5d02f3e14afe7f77a02d6d5043cb933eb8a7a00aa192beefe02f`.
JIT sizes are development-host facts, not portable thresholds. No native memory/
allocation/timing claim follows; managed allocation assertions are separate.
