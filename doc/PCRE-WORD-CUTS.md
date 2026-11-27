# Native 16-bit PRUNE and COMMIT

This increment adds bare and named `(*PRUNE)` and `(*COMMIT)` to the internal
bounded raw16 and strict UTF16 sequence profile, following #367's scoped ACCEPT.

## Execution contract

A cut saves a checkpoint and continues forward normally. Only backtracking into
that checkpoint discards older choices. At the root, PRUNE abandons the current
candidate and COMMIT ends the search. A later PRUNE or a successful atomic scope
can discard an earlier COMMIT before it takes effect. Negative assertions,
assertion conditions and subroutine invocations catch cuts; ordinary positive
assertions and consuming groups do not catch a failing cut. Successful atomic
assertions and groups discard their internal cut checkpoints.

Named cuts share their checkpoint with the label write. Labels own their original
16-bit units, preserve raw surrogate units, and expose only the prefix before NUL.
An empty argument does not assign a label; a nonempty argument beginning with NUL
assigns the empty label. Cuts are continuation boundaries, so finite suffix retries
do not repeat a cut or its label write. Direct cut quantifiers remain invalid.

Native JIT preserves selected captures completed before a call-local cut; the
interpreter restores them. Cut-time rollback first removes right-side captures,
then prunes only the selected call's journal interval. Accepted JIT calls with
lexical cut verbs restore their entry label on caller retry, including cuts in
erased, DEFINE and statically unselected source. Called definitions do not add
lexical cuts to their caller. Ordinary accepted calls retain the earlier committed
label behavior.

The native first-code-unit proof skips eligible candidates before entering a
leading COMMIT. It uses original raw/UTF16 units, including the first surrogate,
and simple Unicode case pairs; larger case equivalence sets do not supply that
proof. Source alternatives must agree on the first unit; singleton/case-pair
classes and nested groups preserve the proof. Equivalent lexical r settings do
not split the native first-unit identity across alternatives. A bounded leading literal-plus/PRUNE/fallback shape also retains native
JIT scan reuse across candidates; interpreter matching does not reuse it. These are semantic candidate proofs, not a general
new optimizer.

## Bounds and evidence

Scope traversal, checkpoint pruning, selected-capture journal edits and candidate
proofs share work quotas and cancellation. The 1,024-frame ceiling, whole-source
admission, strict UTF validation and original-unit capture coordinates remain in
force. THEN, SKIP, match-start reset and other unqualified word features remain
outside this profile. ACCEPT with selected returns remains explicitly rejected. COMMIT patterns whose
first-unit proof is supplied only by a positive assertion are also explicitly
rejected: qualifying their required-unit and start-bitmap interactions is a
separate follow-up. Ordinary consuming first-unit proofs remain supported.

The corpus has 1,070 raw16/UTF16 fixtures, eight compiled defaults and all 64
call masks: 547,840 native PCRE2-16 comparisons of full capture vectors and
successful labels. Local native preflight reports no compile/match errors.
Additional tests cover exact work budgets, cancellation at every observed
transition, frame exhaustion, admission and concurrent reuse of owned labels.

Native reference: PCRE2 10.48, commit
`7978954dbd2efc6f2196869290553cf1871b4ce6`, with SLJIT
`de0259c7aaf36aa40cba8014f3fad3edde9307f9`.
Protocol SHA-256:
`03c0cde3f7773b007af33682b58383149367fa969201300afcae7beb156c20cf`.

The word policy matrices reuse one native compiled pattern for each defaults row,
then execute all 64 match masks. Captures and labels are materialized before the
next call. Native resources remain local to that row; there is no shared cache.
Fresh-compile comparisons separately qualify capture/label reset, JIT/interpreter
dispatch, compile/match errors and literal mode. Coverage and test rows remain intact.

Managed build, test and package qualification is tracked by this increment's PR.
The next word-control slice is THEN and SKIP; public width-16 API integration and
broader profile qualification remain separate roadmap work.
