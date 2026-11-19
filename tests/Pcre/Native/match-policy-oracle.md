# Per-call match-policy preflight

The original [C probe](match-policy-oracle.c) uses PCRE2 10.48
`7978954dbd2efc6f2196869290553cf1871b4ce6` and SLJIT
`de0259c7aaf36aa40cba8014f3fad3edde9307f9`. Build out of tree as in
[README](README.md), enabling static native8 and JIT, then:

```sh
cc -O2 -std=c11 -Wall -Wextra -Werror -I src -I pcre2/src \
  /path/to/tests/Pcre/Native/match-policy-oracle.c .libs/libpcre2-8.a -o oracle
```

Input: id|mode|compiled-default-mask|call-mask|pattern-hex|subject-hex|start.
Use '-' for empty hex. Masks0..63 use1=anchored,2=end-anchored,4=not-empty,
8=not-empty-at-start,16=not-BOL,32=not-EOL. Default anchor bits also enter
compilation; all effective bits are ORed into native match flags. Source
directives retain native precedence.

Mode bits:1=UTF,2=UCP,4=match-invalid-UTF,8=allow assertion-K,16=caseless,
32=literal,64=ANYCRLF newline,128=dollar-end-only,256=alternate-caret,
512=first-line. Otherwise newline=LF, BSR=Unicode; nesting512 is explicit.
Input lines are bounded to65,535 bytes including LF.

Output: id|call-mask|JIT-return|JIT-size|numbered-captures|successful-mark-hex.
All declared slots are retained; unset='-', lengths are signed end-start.
No-match leaves captures/mark empty. Unexpected compile or match errors produce
E/X records and a nonzero exit, never a no-match conversion. Every allocation,
context, program and match data is released per row. JIT compile results/sizes
are recorded; dispatch remains pcre2_match with its native option handling.

Pinned pcre2api.3, match-options section, explicitly states that match-time
ANCHORED/ENDANCHORED disable JIT and use the interpreter. Do not describe those
calls as JIT execution merely because the program has compiled JIT code.
PCRE.NET's pinned pcrenet_match.c also calls pcre2_match with additional_options.

All56 definitions and all64 policies produce3,584 complete literal outcomes:
1,874 matches,1,710 misses, including80 reversed reported ranges. Every compile
has JIT-return0/positive size. No row was discarded. Raw output SHA256:
`75168c42dd34448d1c4119dccf2193e8a6a27d084789e5ca427d795217cf57d3`.
The sample catalog stores these outcomes without running the implementation to
invent expectations. CI first checks all64 outcomes through PCRE.NET1.6.0 for
each definition, then exercises one reusable managed pattern with every policy.
No production native dependency or fallback is introduced.
