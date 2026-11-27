# Native 16-bit conditional branches

Raw 16-bit and strict UTF-16 sequences now admit capture-participation,
innermost-call/recursion, VERSION and atomic assertion conditions. Numeric,
relative, named and duplicate-name tests reuse the shared parser's resolution,
including reserved-name precedence. Conditions can be nested, repeated and
possessive; quantified DEFINE blocks retain their declarations without executing
them. VERSION compares against the existing internal PCRE2 10.48 syntax target.

## Selected arms and state

The compiler records original source-arm boundaries before finite choices expand
into ordered alternatives. A condition selects one arm. Failure may retry choices
inside that arm or outside the condition, but never offers the sibling arm as an
alternative. Selection itself adds no saved retry frame. Both source arms remain
validated and charged to the existing path, node, repetition and nesting budgets.

Capture conditions treat empty captures as participating. Duplicate-name scans
visit declaration-ordered slots and poll the shared work/cancellation controls.
Recursion conditions test only the innermost active call, including nonrecursive
subroutine calls. Calls save the caller identity in their return continuation;
retry frames preserve the active identity alongside captures and subject
visibility. Returned callees therefore recover their call identity on retry,
while successful return and assertion/atomic pruning restore the caller context.

Assertion conditions use the existing guard and capture journal. A successful
test body commits its choices and preserves its captures for either sign:
positive tests select the yes arm and negative tests select the no arm. An
exhausted body rolls back before selecting the opposite arm. Neither outcome
consumes subject units. The shared parser continues to reject non-atomic
assertions used directly as condition tests; nested non-atomic assertions remain
available inside an atomic test body.

## Repeat and lookbehind details

Counted unbounded conditional groups check empty progress on their final required
copy, as native PCRE2 does. The byte emitter receives the same correction and
reuses the existing journaled progress marker, preserving its repeat-state
capacity. Explicit capture-growth witnesses distinguish this from a separate
empty optional iteration. Finite and possessive conditional counts retain the
existing group and atomic execution paths.

Lookbehind endpoint checks now belong to the expanded branches that actually use
a ranged rewind. Fixed branches trust the native declared width. This matters
when a false condition omits its else arm: an omitted arm and an explicit empty
arm have different native width calculations and can produce different matches.
Mixed fixed/ranged branches, positive/negative tests and non-atomic lookbehind
retain their original endpoint visibility. Inactive arms and assertion-test
bodies still contribute UTF validation metadata.

## Evidence and limits

356 fixtures across eight compiled defaults and all 64 call masks add 182,272
full-vector PCRE2-16 comparisons. They cover all four condition families, native
header aliases and name priority, committed arm selection, finite alternatives,
capture rollback, call return/retry, recursive calls, assertion captures, repeats,
lookbehind widths, original start offsets, Unicode names/casing/classes, raw
surrogate units and LF/CR/NUL line modes.

The pinned native preflight produced 59,924 matches and 122,348 misses with no
native errors. LF output SHA-256:
`03207e4b8793bd7a2b22e37e88527f5bf8694a617aa24a5fb8dc3332d90eada3`.
Oracle provenance is in [word-sequence evidence](PCRE-WORD-SEQUENCES.md).

Additional regressions pin explicit capture vectors, byte/word coordinates and
values, exact work limits, cancellation during long duplicate-name scans,
the 1,024-frame boundary, atomic frame reclamation, unchanged byte repeat-state
capacity, source/result ownership, inactive-source budgets, syntax offsets and
UTF validation windows. Six-host CI results are recorded on the pull request.

Word execution remains an internal bounded profile: 64 source levels, 64 source
paths, finite counts through 64, 64 retained unbounded repeats and 4,096 weighted
nodes. This includes ignored DEFINE repeat counts; it does not claim every native
compile-time simplification. Control verbs, broader newline search, graphemes,
full native diagnostic/resource qualification and public/package delivery remain
open. Managed execution uses no native fallback or subject transcoding.
