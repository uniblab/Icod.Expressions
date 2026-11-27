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

An unbounded call repeat with minimum one checks empty progress immediately
after that required call. Higher minima run their required copies before the
separate optional loop; an empty optional call can still update returned
captures. The byte emitter now applies the minimum-one check to counted calls,
reusing its journaled progress marker and repeat-state capacity. Directly
quantified assertions retain their separate finite-bound rule.

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

243 fixtures across eight compiled defaults and all 64 call masks add 124,416
full-vector PCRE2-16 comparisons. They cover call spellings, first definitions,
forward and mutual references, recursive and whole-pattern calls, capture
restoration and selection, repeats, nested assertions, atomic pruning,
lookbehind, Unicode names/casing/classes, raw surrogate units and caller offsets.
The pinned native preflight produced 66,424 matches and 57,992 misses with no
native errors. LF output SHA-256:
`176a4371dbc6f5c37c279b3d3d92c19f5736c713289e6ce547c3d5a2313fa7b7`.
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
