# Internal PCRE nonrecursive subroutines

**Status:** issue #7, following merged #92; internal implementation only.

**Historical scope:** The lookbehind exclusion below records #93. Bounded
consumed calls are added by `PCRE-SUBROUTINE-LOOKBEHIND-DESIGN.md`. Recursion
conditions and the fourth call-arena integer are added by
`PCRE-RECURSION-CONDITIONS-DESIGN.md`. Parenthesized returned-capture lists are
added by `PCRE-RETURNED-CAPTURES-DESIGN.md`; exclusions below are historical.

## Intent and scope

Add nonrecursive calls to existing numbered/named capture definitions, including
DEFINE declarations and forward definitions. Preserve PCRE.NET1.6.0 / PCRE2
10.48 compiled JIT source-byte results, definition options, capture restoration
and non-atomic retries. This is no public PCRE compatibility or release claim.

Support `(?N)`, `(?+N)`, `(?-N)`, `(?&name)`, `(?P>name)`, `\g<N>`,
`\g<name>` and quoted-g equivalents, including signed relative g targets.
ASCII magnitudes1..256 permit leading zeros; relative zero is invalid. Forward
relative adds to the current capture count; backward relative uses count+1-N.
Names use the existing case-sensitive128-byte grammar and global declarations.
No header trivia is skipped, even under x. Undefined/invalid targets report
InvalidSyntax at the original opening parenthesis/backslash byte.

Exact whole-pattern calls R/absolute0, call graph cycles, recursion conditions,
10.47 returned-capture lists and consumed calls in lookbehind remain
UnsupportedSyntax. Cycles are conservatively checked in all declared bodies,
including unreachable syntax. Ordinary lookaheads inside behinds may contain
calls, because those calls add no consumed width. A targeted definition that
contains a general behind whose metadata was suppressed by DEFINE-in-behind
also rejects at the call opener; it must never execute unresolved metadata.
Other finalized lookbehinds in callees keep existing behavior.

## Approaches and decision

Inlining target ASTs duplicates bodies, couples runtime capture transactions to
compiler expansion and obstructs future recursive calls. Extending the matcher
with integer call guards and shared out-of-line bodies keeps one semantic engine
and gives future recursion a clear extension point. Choose the latter. Full
recursion, returned-capture lists and consumed-call width dependencies require
separate native contracts and fixtures; keep these boundaries explicit now.
These are implementation decisions under the standing autonomous remit.

## Parsing, resolution and compilation

A temporary call holds numeric/name target and source offset. Resolve globally
before lookbehind analysis; select the first declared name's number and the
first syntactic capture definition for a numbered slot, including branch reset.
A bounded heap graph walk rejects cycles through any nested groups, assertions,
repeats or conditional bodies. Recollect final rewritten targets after width
resolution. Share existing parser traversal; no recursive AST expansion.

Append one callable **body** per referenced target after main Accept; do not
execute the target root's CaptureStart/CaptureEnd opcodes. Patch Call operands to
entry addresses; Return resumes the dynamic caller. Definition AST nodes retain
all lexical options. Both ordinary source bodies and callable bodies charge the
existing4096 instruction/visit and256 repeat-mark limits. Captures in {0} and
unreachable bodies still define callable targets. Newline metadata scans source
bodies as before; conservatively consider calls nullable for invalid-UTF search
candidate eligibility. The VM determines actual participation/consumption.

## Runtime transactions

Each Call saves a guard on the existing1024-frame stack and allocates a stable
heap arena record (return PC, parent call ID, journal checkpoint). Saved frames
retain call IDs alongside assertion IDs, never pointers, spans or ref structs.
Call guards and arena nodes have the same creation/LIFO release order, including
inactive successful siblings. Atomic/assertion pruning and failure release them.

Return keeps the guard and internal alternatives so later failure may retry the
callee. Restore numbered captures and their start slots and private repeat
marks to entry values by walking the original fixed journal interval forward
and appending the first compensating mutation per field. Reuse integer generation tags per field (no capture values) to avoid doubling
nested-call history. Preserve original entries for interior
retries. Do not restore slot0: a callee's `\K` updates the reported match start.
The root target capture is never set by a call; nested captures are temporarily
visible to references/conditions and restore on return. Outer capture values
remain visible on entry. No capture snapshots, native fallback or new engine.
Every scanned return mutation spends work and polls cancellation; all existing
instruction/guard/work/compile/cancellation ceilings stay shared.

## Evidence and review risks

Primary contract: pinned `pcre2pattern.3`, Groups as subroutines and Recursion and
subroutines with returned capture groups; native `pcre2_match.c` OP_RECURSE and
recursion return restoration; `pcre2_compile.c` call/name/number fixups.
Native probes pin non-atomic alternative retry, definition options, first target,
root/nested capture restoration, forward/relative numbers and persistent K.

Fixtures must cover interior retries after return, caller capture rollback,
atomic/assertion pruning with inactive siblings, shared1024 guard boundaries,
empty repetitions, acyclic nested calls, erased declarations, invalid UTF/start
positions, global CRLF metadata and exact diagnostic boundaries. Fresh review
must trace guard/arena lifetime and journal compensation independently.

## Release constraints

C#13; net8/net9/net10; root solution/project, src, tests, samples; UTF-8 LF;
no Python. Local Debug, six native Staging PR jobs. No local SDK: executable
managed red/green comes from CI. No public API/package version/dependency,
workflow or consumer changes. Grep remains last; CommandFramework removal stays
gated on publication and every known consumer migration. User owns merge.
