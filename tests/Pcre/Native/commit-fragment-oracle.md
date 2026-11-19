# Committed-fragment and physical-end preflight

Reuse the unchanged original [match-policy C probe](match-policy-oracle.c),
Git blob6fbaee449941be0b462604d3d1c4101d79241a37, with the exact input/output
format and mode/policy bits documented in [its contract](match-policy-oracle.md).
The existing PcreMatchPolicyCase model and typed package consumer are reused;
neither old source changes for this increment.

Pins: PCRE2 10.48 `7978954dbd2efc6f2196869290553cf1871b4ce6` and SLJIT
`de0259c7aaf36aa40cba8014f3fad3edde9307f9`. Out-of-tree static native8/JIT build,
then cc -O2 -std=c11 -Wall -Wextra -Werror as in the earlier contract. The probe
calls pcre2_match; match-time ANCHORED/ENDANCHORED select native interpreter
dispatch, while the recorded JIT compile result remains provenance.

52 definitions x64 policies produce3,328 unfiltered complete outcomes:
1,744 matches/1,584 misses, every JIT compile0/positive size. Unexpected native
compile/match errors fail the run; none is converted to no-match or discarded.
Raw output SHA256:
`f7c4a5b23dabe17e98d2fee8b9b5cc2038d41d49a29be5a561aadd238f8b4a5b`.

Fixtures cover root/named COMMIT, discarded cuts under PRUNE/SKIP/successful
atomic scopes, unrelated cut controls, repeated valid fragments and malformed
runs, raw/strict/tolerant/UCP, supplementary literals, original offsets, captures/
marks/K/empty policies, physical A/z/Z/dollar boundaries and compiled defaults.
The immutable literal catalog is PcreCommitFragmentCases.cs. All64 native
outcomes precede managed comparisons per definition in normal six-host CI.

Pinned pcre2_match.c converts MATCH_COMMIT to MATCH_NOMATCH before ENDLOOP;
nonterminal tolerant fragments then resume at FRAGMENT_RESTART. A true root cut
abandons the current fragment, not all later valid fragments in that dispatch
profile. OP_EOD/OP_EODN independently compare true_end_subject, distinguishing
physical z/Z from a call's fragment-local end-anchoring check. Neither rule
requires a different managed VM, source wrapper or subject transcode.
