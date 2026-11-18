# Independent native dual-width oracle

This development-only C harness records the same explicit protocol against
PCRE2's native 8-bit and 16-bit libraries. It is not part of the production
library or its package. Normal CI uses the existing PCRE.NET 1.6.0 test-only
dependency on all six native hosts; no new dependency or workflow is needed.

Pins:

- PCRE2 10.48: `7978954dbd2efc6f2196869290553cf1871b4ce6`.
- SLJIT: `de0259c7aaf36aa40cba8014f3fad3edde9307f9`.
- PCRE.NET 1.6.0 source inspected: `09639d45d4c03a8269fa2cde6edc7a7383bbc18c`.

From a disposable out-of-tree build directory, with the pinned PCRE2 checkout
(including its SLJIT submodule) at `./pcre2`:

```sh
./pcre2/configure --enable-jit --enable-pcre2-16 --disable-shared --enable-static
make -j4 libpcre2-8.la libpcre2-16.la
cc -O2 -std=c11 -Wall -Wextra -Werror -I . -I pcre2/src \
  /path/to/Icod.Expressions/tests/Pcre/Native/dual-width-oracle.c \
  .libs/libpcre2-8.a .libs/libpcre2-16.a -o oracle
printf '8|1|C3A9|C3A9|0\n16|1|00E9|00E9|0\n' | ./oracle
```

Each LF-terminated input has five fields:

`width|mode|pattern-code-unit-hex|subject-code-unit-hex|native-start-offset`

Width is 8 or 16. Hex has two digits per byte or four per 16-bit code-unit
**value**, independent of host endianness. `-` means empty. Mode bits are
1=UTF, 2=UCP, 4=allow assertion `\K`. Newline is LF; BSR is Unicode; nesting
limit is 512. Lines are bounded to 65,535 characters including their newline.

Successful compilation reports `M` (match) or `N` (no match), JIT_COMPLETE
return, JIT size, numbered captures, successful mark hex and name table:

`M|0|positive-size|start:length,-,...|mark-hex|name-hex=group;...`

All declared captures are retained, including trailing unset slots. Lengths
are signed differences of endpoints, preserving reversed reported ranges.
No-match has empty capture/mark fields but retains compile-time names.
Compile failure is `E|native-code|native-error-offset`. Match failure other
than no-match is `X|native-code|jit-return|jit-size`. Never convert an error
into no-match. Native code frees contexts, programs and match data per row.

The checked-in fixture fields are literal results of this unmodified pinned
oracle. Fixtures carry separate byte/UTF-16 capture offsets plus independent
scalar expectations. Scalar conversion is test infrastructure, not a matcher
implementation and not a production transcoding adapter.

PCRE.NET's pinned `PcreRegex` string API unconditionally enables UTF. Therefore
the shared managed fixture contract deliberately starts with valid Unicode
under UTF at both widths. Malformed subjects have width-specific error rows
and are never treated as equivalent scalar sequences. This harness also
supports raw native mode (`mode=0`); e.g. raw 16-bit `0100` and isolated
`D800` match themselves at `0:1`. These raw native witnesses do **not** imply
that PCRE.NET's string API or Icod.Expressions has a raw 16-bit backend.
Raw 16-bit CI integration, Perl/reducer evidence and full P01 closure remain
separate work. Native managed 16-bit execution remains P10/P11 work.
