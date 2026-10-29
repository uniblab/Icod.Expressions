# Internal PCRE non-atomic positive assertions

## Intent and approach

Continue issue #7 after #87 with resumable positive lookahead and lookbehind.
PCRE.NET1.6.0 / PCRE2 10.48 Compiled/JIT remains the oracle. Retain body choices
on the existing bounded VM backtrack stack, sharing its capture journal. An
independent matcher or recursive callback would duplicate width/option/fragment
semantics and obscure resource limits; capture snapshots per retry would defeat
the journal. This approach follows the standing autonomous development remit.

## Syntax and semantics

| Forms | Kind |
|---|---|
| `(?*...)`, `(*napla:...)`, `(*non_atomic_positive_lookahead:...)` | Positive ahead |
| `(?<*...)`, `(*naplb:...)`, `(*non_atomic_positive_lookbehind:...)` | Positive behind |

Alpha headers are exact lowercase colon-terminated bytes, even under i/x/xx.
These assertions consume no caller text. On a body success, reset the current
position to the original assertion position and keep captures. A later failure
may re-enter saved body alternatives, repetitions or reverse starts; rollback
restores all intervening capture starts/values and repeat marks. The first body
success is therefore not a commit. When all body paths fail, restore the entry
state and propagate positive-assertion failure. Enclosing atomic groups/assertions
and possessive repetition still prune retries. Negative assertions stay atomic.

Direct assertion quantifiers use the existing finite normalization and U/lazy/
possessive policy. Options, J/name selection, K prohibition, original G offset,
invalid-UTF fragments and byte spans retain existing semantics. Lookbehind uses
existing fixed/variable widths, branch order and longest-first reverse retries;
variable-body JIT end caps must be hidden after success and restored on re-entry.
An ahead inside behind still contributes zero consumed width while nested behind
validation runs. Unlimited consumed widths, cycles, actual duplicate named refs
and global branch-reset reference constraints retain their current rejection.

## Parser and lowering

Add optional NonAtomic=false to the existing lookahead/behind nodes and a private
GroupFrame flag. Recognize the six forms using existing exact alias parsing and
symbolic group headers. All guards/source contexts/capture numbering remain
shared. Non-atomic bodies always use general assertion nodes, including literals
and empty bodies: no atomic literal shortcut. Preserve NonAtomic through deferred
behind reconstruction along with i/r. Existing tree rewrites retain record fields.
CompileAssertion adds optional nonAtomic=false and encodes bit4 beside existing
positive1 and variable-end-limit2 in AssertionEnter.Second; AssertionExit.Second
also carries bit4 so scope nodes need no atomicity field. No new opcode/engine.

## Bounded persistent assertion contexts

The existing mutable active-scope list cannot restore scopes removed at success.
Replace its depth interpretation with stable integer scope IDs into a LIFO arena:

- An AssertionScope stores five integers: Boundary, LimitEnd, Parent, ReturnPc,
  Position. Parent is the active scope ID at entry; -1 means no active assertion.
  Arena entries remain alive exactly while their guard frames are on the stack.
- A branch/reverse frame saves the current scope ID. A guard saves its caller's
  parent scope ID, plus existing pc/position/journal/atomic checkpoints. Reuse the
  old AssertionDepth integer field as AssertionScope; no extra frame fields.
- Entry checks the existing1024 saved-frame limit, appends one scope, pushes one
  guard, and activates the new ID. Non-atomic guards have a distinct compact kind.
- Non-atomic success uses the active scope's ReturnPc/Position, then activates
  Parent. Keep its guard/body retry frames and arena entries. Captures remain in
  the journal; there is no per-success snapshot or continuation allocation.
- Atomic assertion exit copies its active node before pruning and explicitly
  activates Parent after releasing its own guard. Positive success retains
  captures; negative success restores entry journal/atomic state and fails.
  All frame removals,
  including atomic-group pruning and assertion pruning, use one PopFrame helper:
  popping an assertion guard removes the last arena entry. Guard creation/removal
  and arena creation/removal share strict LIFO order, even for inactive sibling
  scopes. No append-only history or free-list graph is needed.
- On retry, restore the journal/atomic depth, pc/position and the frame's scope ID.
  Newer guards have already been popped before an older body frame is reachable,
  so its scope remains alive. Exhausted positive guards (atomic or non-atomic)
  propagate failure; negative exhaustion resumes as before. No scope RemoveRange
  occurs in RestoreFrameState. Candidate reset clears stack and arena together.

Live scopes are bounded by live guard frames, hence at most1024; scope payload is
at most20KiB of integer fields. Saved frames keep integers/enums, no spans/interior
pointers. Atomic-depth storage remains valid because completed atomic scopes
prune all retries that would need to regrow it. Existing Step transition/comparison/
guard/reverse accounting and cancellation apply to every retained path.

## Verification and boundaries

Native explicit outcomes/group counts followed by every numbered byte capture:
all six forms versus atomic controls; first-success failure and ordered retries;
empty/unset/named/duplicate refs; nested non-atomic, atomic, negative and sibling
contexts; options/reset/i/r/J and repeat preference; fixed/variable behind,
reverse-end cap restoration and malformed UTF/CRLF caller offsets; K and original
byte diagnostics; depth256/capture256, instructions4096, saved paths1024, work and
cancellation; sequential inactive guards and reclaimed scopes in repeated atomic
bodies. Retain directives, verbs, script runs, conditions and subroutines as
explicitly unsupported. Replace obsolete non-atomic-pending rows and update
current inventories, with historical notices where appropriate.

No public API/version/dependency/release/consumer change. Default work1,000,000 and
existing depth/capture/instruction/stack policies remain. Root project/solution,
src, tests, samples; UTF-8 LF; no Python. Local Debug; six-platform PR Staging.
No local SDK: observe CI red/green on net8/net9/net10 full plus214 frozen tests,
configured workloads/extraction/API/package consumers; local whitespace/frozen35.
User owns merge; Grep and CommandFramework migration remain gated.

## Pinned sources

pcre2pattern.3 2634–2647 and2831–2884: quantifiers, syntax and resumable semantics;
pcre2_compile.c9733–9779: non-atomic assertion width handling;
pcre2_match.c6404–6423: non-atomic pointer reset versus atomic pruning.
Native probes check pinned10.48 and nonzero compiled JIT code size.
