# Native 16-bit bare SKIP

This increment adds `(*SKIP)` and its empty-argument alias `(*SKIP:)` to the
internal bounded raw16 and strict UTF16 sequence profile, following #369's THEN.
Named SKIP follows in [the named-SKIP record](PCRE-WORD-NAMED-SKIP.md).

## Execution contract

SKIP continues normally until backtracking reaches its checkpoint. An uncaught
SKIP abandons the current candidate and its alternatives, restarting at the saved
position when that position is strictly ahead of the candidate. A target at or
before the candidate follows ordinary bumpalong; it cannot loop or move backward.
Targets, captures and start offsets remain original 16-bit code-unit coordinates.
Strict UTF validation and scalar boundaries apply before matching begins.

Calls and negative/conditional assertions catch SKIP using the existing cut
boundaries. A failed positive assertion can propagate its forward target, while
successful atomic scopes discard their internal cut checkpoints. Native JIT
call-local failure retains selected captures completed before the cut; interpreter
failure restores them. Accepted-call labels also recognize lexical SKIP in erased,
DEFINE and statically unselected source.

Native JIT permits a forward SKIP restart with compiled anchoring. Interpreter
anchoring stops the search before that restart. Call-time Anchored or EndAnchored
selects the interpreter, while an unanchored interpreter can still restart forward.
The matrix checks every combination of these policies, including nonzero starts,
empty end targets, repeated forward cuts and supplementary characters.

A later PRUNE can discard an earlier SKIP. The bounded leading-literal PRUNE scan
proof recognizes this pair in both the byte and word runners, including the existing
limit of four sole ordinary root group wrappers. The scan remains JIT-only.
Separate byte regressions cover bare/empty SKIP, labels, NO_JIT and NO_START_OPT.

The saved-frame ceiling remains 1,024. Candidate entry, forced restart, scope walk,
cut removal and capture rollback share the existing work budget and cancellation
checks. Matching retains no caller subject view or per-call state in the compiled
pattern; returned captures and labels remain stable across concurrent reuse.
Direct SKIP quantifiers remain invalid, and whole-source validation still rejects
unsupported named SKIP even in erased or unreachable bodies. A nonempty argument
starting with NUL is still named SKIP, not the empty-argument alias.

## Evidence and remaining scope

The corpus contains 852 fixtures across eight compiled defaults and all 64 call
masks: 436,224 native PCRE2-16 outcomes comparing full capture vectors and successful
labels. Native preflight reports no compile or match errors. Supplemental tests
cover exact work budgets, cancellation at every observed transition, frame
exhaustion and recovery, source ownership, subject slices and concurrent reuse.
The byte PRUNE scan matrix now covers 38 fixtures in raw8 and UTF8 modes, adding
4,096 comparisons for SKIP (38,912 total).

Native reference: PCRE2 10.48, commit
`7978954dbd2efc6f2196869290553cf1871b4ce6`, with SLJIT
`de0259c7aaf36aa40cba8014f3fad3edde9307f9`.
Protocol SHA-256:
`6643866d9f410e05813a3a03b7298193335482e27d18545980d3ad0d01e8be26`.
Managed build, test and package qualification is tracked by this increment's PR.

Named-SKIP MARK history, assertion/call history isolation and unmatched-name
interpreter replay are covered by the named-SKIP record. ACCEPT with selected returns and
asserted-only COMMIT start proofs retain their existing explicit qualification
gates. Public width-16 API integration remains separate roadmap work.
