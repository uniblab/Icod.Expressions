# Interpreted fragment caret preflight

Reuse original [per-call C probe](match-policy-oracle.c), blob
`6fbaee449941be0b462604d3d1c4101d79241a37`, with its documented protocol.
Pin PCRE2 10.48 `7978954dbd2efc6f2196869290553cf1871b4ce6` /SLJIT
`de0259c7aaf36aa40cba8014f3fad3edde9307f9`. Out-of-tree static native8/JIT
configure/build, then cc -O2 -std=c11 -Wall -Wextra -Werror as before.
Match-time ANCHORED/ENDANCHORED disable JIT dispatch; successful JIT compilation
is provenance, not a claim these calls execute JIT.

48 definitions x64 policies give3,072 unfiltered complete outcomes:1,680 matches/
1,392 misses, every JIT compile0/positive size. Unexpected compile/match errors
fail; none becomes no-match or is removed from this final corpus. Raw SHA256
`9f6d32a45c6e0f0731e41d6302b1bcfffcdd253c872a127342a67a8843426ad5`.
PcreFragmentCaretCases.cs stores every signed capture/mark literally. All64 native
outcomes precede the managed comparison per definition in CI.

Pinned pcre2_match.c OP_CIRCM rejects Feptr==mb->end_subject without
PCRE2_ALT_CIRCUMFLEX. Tolerant interpreter fragments use their local end,
independent of true_end_subject used by z/Z. Default JIT can accept after a
newline at a nonterminal fragment end. `(?m)a\n^` with610AFF62/start0 therefore
gives default0:2 and per-call anchored miss; alternate-caret gives0:2 in both.
Positive/negative assertions and alternatives expose false acceptance/rejection.
Retain raw/strict/physical-end/source/default/alternate-caret controls.
