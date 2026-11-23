# Malformed UTF-8 policy differential sweep

The [live sweep](../PcreMalformedPolicyDifferentialTests.cs) extends the
[bounded differential probe](../PcrePolicyDifferentialProbe.cs) to original
malformed byte subjects. Sixteen grammar templates cover nullable matches,
scalar and grapheme consumption, properties, backreferences, lookarounds,
word boundaries, physical anchors, calls, SKIP/FAIL, ACCEPT, and marks.

Each template runs with mode bits 12 and 15, both with and without the
`(*NO_JIT)` prefix. Mode 12 enables tolerant UTF implicitly; mode 15 enables
UTF and UCP explicitly alongside tolerant matching. All profiles allow
lookaround `\K`. The 24 byte subjects include isolated continuation bytes,
illegal leaders, truncations, overlong encodings, surrogate encodings,
out-of-range scalars, repeated invalid bytes, CRLF, NUL, and valid BMP and
supplementary controls. Every byte offset through the subject end is tested,
including continuation positions inside otherwise valid multi-byte scalars.

The 96 subject/start pairs and all 64 call-policy masks produce 6,144
comparisons per profile, or **393,216 comparisons across 64 profiles**.
On disagreement, the existing reducer retains mode, call mask, start offset,
and mismatch class and prints a hex replay row. Subjects stay as byte values;
replacement decoding cannot conceal a discrepancy. Native errors continue
to propagate rather than becoming no-match.

Two additional fault controls deliberately compile the candidate in strict
UTF mode. Reduction must preserve the resource/diagnostic mismatch, shrink
the subject to its original `FF` byte, and reproduce the same result on a
fresh live comparison. The replay retains that byte and the selected mode.

## Independent native preflight

The unmodified [C policy oracle](match-policy-oracle.c) was compiled against
static PCRE2 10.48 at `7978954dbd2efc6f2196869290553cf1871b4ce6`, with SLJIT
at `de0259c7aaf36aa40cba8014f3fad3edde9307f9`, using
`cc -O2 -std=c11 -Wall -Wextra -Werror`.

Input follows template order, modes 12 then 15, default/JIT then NO_JIT,
subject order, ascending start offset, and ascending call mask. Profile IDs
are 1 through 64; compiled defaults are zero. The preflight produced
112,560 matches and 280,656 misses with zero native errors. All JIT compile
returns were zero; 196,608 rows had positive JIT size and 196,608 had zero
size. LF-terminated raw output SHA-256:

`52e7fa5717b9e247b829de4c371202bd5e7fe208eb19cf6a8efcf6a134db81f0`

This is a bounded deterministic malformed-subject increment. Randomized
grammar fuzzing, strict UTF error equivalence, and native managed 16-bit
execution/cross-width fuzzing remain separate roadmap work. Final managed
qualification and exact source identities are recorded in the PR.
