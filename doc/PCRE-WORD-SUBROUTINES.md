# Native 16-bit subroutine calls and recursion

Raw 16-bit and strict UTF-16 sequences now admit numbered, relative and named
subroutine calls, including forward references, `(?R)`/`(?0)` whole-pattern
recursion, recursive definitions and explicit returned-capture lists.
`(?(DEFINE)...)` registers and validates definitions without executing them.
Finite, greedy/lazy unbounded and possessive calls reuse the existing repeat
and atomic guards. Definition-local options and first-definition selection for
duplicate names or branch-reset slots come from the shared parser.

## Calls, captures and retries

Call targets are integer edges into separately retained source bodies. They do
not inline definitions, recursively compile call graphs or create cyclic object
graphs. Each call executes the definition's body without closing its enclosing
capture slot. Ordinary lexical group execution still closes that slot normally.
This distinction also applies when a returned-capture list names the called
group itself.

A call guard saves the caller's capture checkpoint and continuation. Success
keeps the consumed position, restores caller captures except explicitly selected
slots, and retains callee choices for later suffix failure. Return restoration
appends inverse writes to the shared capture journal so a retry inside the
callee can recover its previous state. Generation tags restore each slot's
oldest value once per return; nested calls do not duplicate journal history
exponentially. Repeats already keep counts and starts in immutable continuations.

Failed calls restore their entry checkpoint before failing into the enclosing
retry stack. Atomic groups and assertions prune returned guards with the rest
of their internal choices. Non-atomic assertions can resume called bodies.
Fixed and bounded-variable lookbehind retain their existing endpoint visibility
and whole-unit/scalar rewind rules, including through a call.

The final required copy of an unbounded call repeat checks empty progress,
matching native PCRE2. The byte emitter now applies the same check to counted
calls, reusing the existing journaled progress marker and repeat-state capacity.
Directly quantified assertions retain their separate finite-bound rule.

## Resource and ownership behavior

Source nesting, path, repeat-count, retained-unbounded-repeat and weighted-node
admission bounds remain. Definitions, including unused or zero-count bodies,
still consume source complexity and contribute lookbehind validation metadata.
Calls count as retained edges; the referenced bodies are charged in their
lexical source positions rather than expanded at each call or recursion level.

All call guards and callee retry choices share the 1,024-saved-frame ceiling.
Entry, return, journal scans, undo and guard exhaustion charge the match work
quota and observe cancellation. Productive recursion uses the same iterative
runner. Unproductive recursion stops at the configured work or shared frame
limit; matching the native interpreter's separate recursion-loop diagnostic is
not claimed. No failed or cancelled call publishes partial captures.

Compiled source, targets and name metadata are owned by the pattern. Results
own stable capture slots and retain no subject memory. There is no native
fallback, subject copy or transcoding in managed execution.

## Evidence and remaining work

237 fixtures across eight compiled defaults and all 64 call masks add 121,344
full-vector PCRE2-16 comparisons. They cover call spellings, first definitions,
forward and mutual references, recursive and whole-pattern calls, capture
restoration and selection, repeats, nested assertions, atomic pruning,
lookbehind, Unicode names/casing/classes, raw surrogate units and caller offsets.
The pinned native preflight produced 64,184 matches and 57,160 misses with no
native errors. LF output SHA-256:
`20cf4c53536095176b9f68bb285359ffb8297651636ed99874360f5f62a830f8`.
Oracle provenance is in [word-sequence evidence](PCRE-WORD-SEQUENCES.md).

Additional regressions pin explicit capture results, native and managed
cross-width coordinates/values, exact work boundaries, cancellation during
return/undo/pruning, saved-frame exhaustion and atomic reclamation, bounded
nested return history, source/result ownership, admission bounds, original
syntax offsets and UTF validation windows. Six-host CI results are recorded
on the pull request.

Runtime capture/assertion/recursion conditions, general static conditions,
quantified DEFINE blocks and control verbs remain unsupported. They are next,
along with broader newline search, graphemes, package consumers and public
API/resource qualification.
