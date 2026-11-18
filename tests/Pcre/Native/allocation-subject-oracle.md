# Allocation-profile semantic preflight

The original [C probe](allocation-subject-oracle.c) verifies matching semantics,
not managed allocations. Build it against the generated pinned PCRE2 10.48
header/library using the source/SLJIT setup in [README](README.md):

```sh
cc -O2 -std=c11 -Wall -Wextra -Werror -I src -I pcre2/src \
  /path/to/Icod.Expressions/tests/Pcre/Native/allocation-subject-oracle.c \
  .libs/libpcre2-8.a -o allocation-oracle
./allocation-oracle
```

PCRE2 commit7978954dbd2efc6f2196869290553cf1871b4ce6 and SLJIT
commitde0259c7aaf36aa40cba8014f3fad3edde9307f9 produce eight outcomes: six
matches and two no-matches across four profiles and 1,024/65,536-byte inputs.
All compiles have JIT return0 /positive size. Anchoring is explicit; raw FF/NUL
code units and UTF-8 Greek lambda remain original bytes. Native captures are
0:1,0:1 for raw,0:2,0:2 for UTF dot/UCP letter, and empty for UTF no-match.
Development output SHA256:
`3ec6e5d2e40227b11951239711fbf4470d64065a5651f72cc7ede4bc80f00c1d`.
JIT byte sizes are recorded development-host facts, not portable thresholds.
No native allocation/memory/timing claim follows from this probe.
