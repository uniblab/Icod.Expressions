# Native 16-bit THEN

This increment adds bare and named `(*THEN)` to the internal bounded raw16 and
strict UTF16 sequence profile, following #368's PRUNE and COMMIT.

## Execution contract

THEN continues normally until backtracking reaches its checkpoint. It then
abandons the current source alternative and tries that owner's next alternative.
On the last source alternative, it discards the owner's internal choices and
allows earlier outer retries. Finite repeat expansions and conditional yes/no
arms do not create lexical owners. Single-branch groups retain the outer owner.

Original source-branch boundaries are retained through lowering. The root enters
an ordinary group when the source contains THEN, so root and nested alternatives
use the same saved-frame mechanism. A retained next-branch frame keeps its entry
capture and label checkpoint while skipping lowered paths in the failed source
branch. Successful atomic scopes discard their internal cut checkpoints.

Assertions and calls reset lexical ownership. Unowned THEN uses the native local
failure scope: assertions catch it, except that native JIT uses a call-local quit
for an unowned THEN inside a called atomic positive assertion. The interpreter
catches that assertion. Only the call-local JIT quit preserves selected captures
completed before the cut; failure of an owned alternative restores them normally.
Accepted calls with lexical THEN also restore their entry label on older retries,
including verbs in erased, DEFINE and statically unselected source.

Named THEN shares the cut checkpoint with its label write. Original 16-bit units,
raw surrogate labels, empty arguments and NUL visibility follow the existing
named-cut contract. Direct THEN quantifiers remain invalid. Whole-source admission,
strict UTF validation, original-unit capture coordinates, cancellation, work quotas
and the 1,024-frame ceiling remain in force.

A following PRUNE can discard THEN before it fires. The bounded leading literal
scan cache recognizes this pair, including up to four sole ordinary root group
wrappers, matching native early-failure study. Both word and byte matching apply
this cache only under JIT dispatch; call-time anchors and NO_JIT retain interpreter
behavior. Separate byte comparisons cover these wrappers, labels and dispatch
options without changing the native reference.

## Evidence and remaining scope

The corpus has 1,114 raw16/UTF16 fixtures across eight compiled defaults and all
64 call masks: 570,368 native PCRE2-16 comparisons of full capture vectors and
successful labels. Native preflight reports no compile/match errors. Supplemental
tests cover exact budgets, cancellation at every observed transition, frame
exhaustion, unsupported erased source, ownership and concurrent reuse.

Native reference: PCRE2 10.48, commit
`7978954dbd2efc6f2196869290553cf1871b4ce6`, with SLJIT
`de0259c7aaf36aa40cba8014f3fad3edde9307f9`.
Protocol SHA-256:
`037a09a197b7d8e64368a5ed50a7f14bd9db12ee110e0ec60efb0a638a01c1ea`.
Managed build, test and package qualification is tracked by this increment's PR.

Bare SKIP follows in [the SKIP record](PCRE-WORD-SKIP.md); named-SKIP history/replay
is next. ACCEPT with selected returns and
asserted-only COMMIT start proofs retain their existing explicit qualification
gates. Public width-16 API integration remains separate roadmap work.
