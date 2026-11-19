# Optional lookbehind rewind preflight

Recompile unchanged original [C probe](match-policy-oracle.c), blob
6fbaee449941be0b462604d3d1c4101d79241a37, against the existing independently
built pinned static native8/JIT library, PCRE2 10.48
7978954dbd2efc6f2196869290553cf1871b4ce6 /SLJIT
de0259c7aaf36aa40cba8014f3fad3edde9307f9. Use the documented protocol and
cc -O2 -std=c11 -Wall -Wextra -Werror. Match-time anchors disable JIT dispatch;
successful JIT compilation/size is provenance, not execution evidence for them.

48 definitions x64 policies give3,072 complete outcomes:1,848 matches/1,224
misses, every JIT compile0/positive size. No compile/match errors or discarded
final-corpus rows. Raw SHA256
`f46d73b9a28bf1e3b46da589341496c9c2b8fc848f29cdb092ab108945059477`.
PcreOptionalRewindCases.cs retains every signed capture/mark literally. All64
native assertions precede managed comparison for every definition in CI.

Pinned pcre2_match.c OP_VREVERSE stops at check_subject; below minimum it fails,
otherwise it shortens the maximum and tries forward starts. At a malformed
fragment barrier, default/JIT `(?<=a?)b` onFF62 misses while call anchored gives
1:1; negative polarity reverses the result. Retain fixed/minimum/raw/strict/
physical-start/source/default and original-offset controls. This does not close
all native interpreter/JIT assertion differences.
