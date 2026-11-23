# Seeded bounded grammar differential

The test-only [generator](../PcrePolicyGrammarGenerator.cs) uses xorshift32
with shifts 13, 17 and 5, seeds `1`, `12345678`, `9E3779B9` and `DEADBEEF`
(hex), and 128 samples per seed/mode. Recursive grammar depth is three.
Choices include atoms, sequence, alternation, greedy/lazy/possessive bounded
repeats, positive/negative lookahead, atomic groups, named capture/reference
pairs, capture conditions and scoped casing. Names are allocated before
child generation. A root MARK makes successful mark projection observable.

Atoms include empty, literals, dot, digit/word/boundary escapes, NUL,
positive/negative classes and letter properties. Width-dependent atoms are
byte `80`/`FF` in mode 8 and U+00E9/U+1F600 in mode 15. Each subject has zero
through four byte chunks, selected from empty, ASCII, NUL, LF, CRLF, valid
BMP/supplementary UTF, and malformed encodings. One start offset from zero
through the subject end is generated after its chunks. Bytes are never
decoded with a replacement fallback.

The [live sweep](../PcreGeneratedPolicyDifferentialTests.cs) uses modes 8
(raw) and 15 (UTF/UCP/tolerant), default/JIT and NO_JIT prefixes, and every
call mask. Its 16 profiles cover 2,048 generated sample/prefix combinations
and **131,072 comparisons**. Failures include seed, sample index, original
and reduced mismatch, reduction status/count, and a native hex replay row.
Native exceptions propagate. The existing reducer retains mode, policy,
offset and mismatch class. A generated mark fault must reduce and reproduce
with its original nonzero offset and capture projection intact.

Generation rejects zero seeds, other modes, and counts outside 1 through
256. Maximum subject length is 16 bytes. Generation does not depend on
System.Random or current culture. Eight literal input digests pin all
seed/mode corpora across runtimes; serialization is uppercase pattern hex,
`|`, uppercase subject hex, `|`, decimal start and LF for each sample,
without the NO_JIT prefix. Those digests were also computed by an independent
JavaScript generator used for the C preflight.

## Independent native evidence

The unmodified [C policy oracle](match-policy-oracle.c), compiled with
`cc -O2 -std=c11 -Wall -Wextra -Werror`, used static PCRE2 10.48 at
`7978954dbd2efc6f2196869290553cf1871b4ce6` and SLJIT at
`de0259c7aaf36aa40cba8014f3fad3edde9307f9`.

Input ordering is seed, mode 8 then 15, default then NO_JIT, sample index,
and call mask. Sample/prefix IDs are 1 through 2,048; compiled defaults are
zero. All native compilations and matches completed without errors. Results:
25,524 matches, 105,548 misses, 65,536 positive-JIT-size rows and 65,536
zero-size rows. All JIT compile returns were zero. LF-terminated output
SHA-256:

`314155710999c9080a9a7d89ec91a77307823dc19deb8f5948d36db71a9139e3`

This is a reproducible bounded grammar fuzz increment. It does not close
unrestricted grammar fuzzing, strict UTF diagnostic equivalence, resource
fuzzing, native managed 16-bit execution or cross-width release gates.
Final managed qualification and exact source identities belong in the PR.
