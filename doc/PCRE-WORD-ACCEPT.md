# Native 16-bit scoped ACCEPT

This increment adds bare and named `(*ACCEPT)` to the internal bounded raw16
and strict UTF16 sequence profile. It follows the failure/label work in #366.
It does not expose a new public entry point or change frozen provider sources.

## Execution contract

Root ACCEPT closes active captures and succeeds immediately. Assertion ACCEPT
commits the innermost assertion and resumes its caller; a matching negative body
fails normally. Call ACCEPT commits the innermost invocation, returns its ordinary
caller capture state, and resumes the caller. Atomic groups and repetition do not
create an ACCEPT exit scope. All exits bypass the remaining body, including
lookbehind endpoint checks. Direct greedy, lazy and possessive quantifiers retain
their normal entry choices; a selected ACCEPT exits its scope immediately.

Named ACCEPT performs MARK before ACCEPT, inside its direct quantifier. Labels
use owned original word strings, preserve raw surrogate units, and expose only
the prefix before NUL. An empty argument does not assign a label; a nonempty
argument beginning with NUL assigns the empty visible label. Native word-name
capacity remains subject to the smaller 4,096-unit source limit.

Synthetic closes apply outside active subroutine calls, as in the existing byte
executor. Root completion applies NotEmpty/NotEmptyAtStart and EndAnchored. A
short root ACCEPT abandons the candidate, except when the native interpreter
first rejects an empty match and can retry saved choices. Compiled anchoring
and call-time anchoring remain distinct dispatch inputs.

The parser's lexical non-atomic-ACCEPT flag selects interpreter behavior even
when the ACCEPT appears in a zero-count, DEFINE or statically unselected body.
That flag also controls UTF reverse-window and variable-lookbehind visibility.
Interpreted assertion-condition ACCEPT restores the condition's entry label.
Native JIT accepted calls instead commit the callee's label across checkpoints
that share the entry write. Write identities and match-local committed values
preserve that distinction without extra retry frames. Earlier distinct writes
still roll back, and committed values reset at the next candidate.

## Bounds and qualification limits

Continuation traversal, capture closes, scope pruning and ordinary rollback use
the existing work/cancellation checks. Calls retain the 1,024-frame ceiling.
Compilation still validates the complete source, including unreachable tails.
Unresolved nested lookbehind widths remain outside the existing bounded profile.

ACCEPT combined with selected-capture subroutine returns is explicitly rejected
with UnsupportedSyntax. The pinned native interpreter returns INTERNAL (-44)
for these cases while native JIT can produce a capture result. Admission rejects
the lexical combination, including unused bodies, until a cross-dispatch contract
is established. Ordinary selected returns without ACCEPT remain supported.

## Evidence

The differential corpus contains 621 raw16/UTF16 fixtures, each run with eight
compiled defaults and all 64 call masks: 317,952 native PCRE2-16 comparisons.
The local native preflight produced 197,200 matches and 120,752 nonmatches, with
no compile/match errors in this corpus. It compares full capture vectors and
successful labels. The selected-return native error is pinned separately.

The native reference is PCRE2 10.48 at
`7978954dbd2efc6f2196869290553cf1871b4ce6`, with SLJIT at
`de0259c7aaf36aa40cba8014f3fad3edde9307f9`. Build the development oracle with
`ORACLE_LITERAL=0`, `ORACLE_MARKS=1` and `ORACLE_INTERPRETER_FALLBACK=1`.
The last option admits only native JIT_UNSUPPORTED (-68) with zero JIT size;
all other unexpected JIT outcomes remain errors. Default oracle output and
strict JIT checks remain unchanged. The LF protocol SHA-256 is
`d0851c44830cce9dcfefa08a7f9ed3ecbfd36a1db10f8eaeb64e852609b5a773`.

Additional tests pin explicit captures/labels, lexical interpreter dispatch,
exact work costs, cancellation at every observed transition, frame exhaustion
and reuse, original-unit syntax diagnostics, source bounds, strict UTF validation,
cross-width captured values and concurrent ownership. Managed validation runs
through the six-host CI matrix; this development workspace has no .NET SDK.

## Remaining work

PRUNE, COMMIT, THEN and named/unnamed SKIP remain the next control-verb work.
Selected-return ACCEPT needs the explicit qualification above. Backslash-K,
grapheme matching, pair-aware newline search, broader syntax and public
package qualification remain open in the roadmap.
