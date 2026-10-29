# Internal PCRE bounded variable-length lookbehind

Historical tranche boundary: non-atomic positive ahead/behind now extend the shared
engine with bounded resumable scopes; see `PCRE-NONATOMIC-ASSERTIONS-DESIGN.md`.
Earlier pending-variant or scope-depth descriptions below describe that tranche.

## Goal and boundaries

Extend #79's shared lookbehind machinery to bounded consuming repeats, unequal
nested group alternatives and `\R` (one or two characters). Compute minimum and
maximum character lengths. If any direct branch varies, every branch maximum
must fit the pinned default variable limit of 255. All-fixed assertions retain
independent fixed lengths and their existing 65,535-character analysis ceiling.
Consumed references, unlimited ordinary groups, aliases and conditions remain
controlled unsupported syntax. Consumed references need a separate global
capture-length analysis and are intentionally deferred.

The target oracle is PCRE.NET 1.6.0 / PCRE2 10.48 Compiled/JIT. Its variable
lookbehind behavior differs from the interpreter in important corners. Native
JIT evidence and explicit executable comparisons determine this internal contract.

## Static analysis

Extend the existing reference-keyed heap postorder traversal to minimum/maximum
ranges. Sequences add, group alternatives take minimum/maximum, bounded repeats
multiply their respective bounds, and `\R` contributes 1..2. Nested assertions
contribute zero. Continue validating erased children and rejecting source-unbounded
direct nested lookbehind, while permitting quantified nested lookahead. Sums and
products remain bounded before converting to integers. Store each direct branch
maximum and a group minimum sentinel: -1 for all-fixed, otherwise the minimum of
all direct branches. This matches the pinned compiler's single group minimum.
Validate at parser closure, including enclosing zero repeats.

## Shared compilation and matching

Keep all-fixed Reverse and compatible literal paths. A variable assertion uses
its global minimum and each branch maximum: emit ReverseRange when maximum is
larger than that minimum; otherwise keep fixed Reverse or an empty branch.
Emit a ReverseEnd check after ranged branch bodies. A ranged branch tries the
longest available start first, fully explores its ordered body paths, then retries
one character closer until the minimum. Body lazy/greedy preference applies
within each start. First complete positive success remains atomic.

Use one compact ReverseRetry frame at a time to resume the next start, sharing
journal and atomic/assertion depth checkpoints. A byte-sized remaining count fits
the 255 ceiling and existing frame padding. Every retry shares the 1,024 saved-path
ceiling, spends work and observes cancellation. No per-start body duplication,
capture copies, subject slices, spans in frames or recursive matcher.

The pinned JIT caps its visible subject end at the original assertion position
whenever any branch uses variable reverse. Apply that scope to every branch,
nested assertions, consuming atoms and end/word/line tests. Store the optional
end limit alongside each existing assertion boundary; ordinary frame scope-depth
checkpoints restore it on retries. All-fixed behind retains the enclosing subject visibility, including any outer
variable-assertion cap; it never widens that end to the original input.
Restore the prior end after positive/negative completion and outer backtracking.
Original caller `\G`, left-context bounds and capture coordinates remain unchanged.

UTF reversal counts scalars. At absolute subject start, reduce the longest
available distance if the minimum fits. In tolerant UTF mode the pinned JIT fails
a ranged branch when optional maximum rewind meets an invalid left barrier,
even if the minimum already fits; pin this independently in compiled-oracle tests.
Do not cross malformed fragments. Byte mode has no malformed barrier.

## Alternatives and decision

Unrolling a copy of the entire body for every possible start would multiply
compiled size and repeat logic. A second recursive matcher would duplicate
capture/resource semantics. The maintainer agent chooses one shared ranged
reverse opcode, compact retry frames and scoped ends under the user's standing
authorization. A separate capture-length resolver will add consumed references
later. Full variable lookbehind is bounded at the native default 255 for now.

## Constraints and acceptance

Internal managed C# 13, net8/net9/net10; no native or Regex production fallback.
Root project/solution, `/src`, `/tests`, `/samples`, UTF-8 LF, no Python. Local
Debug; six OS/architecture Staging CI jobs. Existing parser/capture/repeat and
instruction/node/work/cancellation ceilings remain. No public API, package
version/dependency, Grep migration or destructive CommandFramework changes.

Explicit compiled-native outcomes/all-capture spans must cover longest-start
ordering, body preference, atomic/negative and inter-start rollback, nested scoped
ends and restoration, mixed fixed/variable alternatives, zero lengths, names and
branch resets, options/comments, direct repeats, UTF casing/byte widths, malformed
barriers, caller/CRLF sweeps, 255/256 and erased-body checks, work/cancellation and
saved paths. Require independent review, frozen source, all six full/frozen suites
and existing workload/extraction/API/package gates before marking the PR ready.
