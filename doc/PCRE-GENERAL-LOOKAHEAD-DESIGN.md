# Internal PCRE atomic lookahead bodies and captures

Historical tranche boundary: exact atomic-group and atomic-lookaround aliases now
share these existing semantic paths; see `PCRE-GROUP-ALIASES-DESIGN.md`. Earlier
alias-pending notes below describe the original tranche.

## Goal and reference

Advance issue #7 with general atomic (?=...) and (?!...) bodies using the
existing grammar and matcher. Pin PCRE.NET 1.6.0 / PCRE2 10.48, especially
[pcre2pattern assertions](https://github.com/ltrzesniewski/pcre-net/blob/v1.6.0/src/PCRE/doc/pcre2pattern.3)
and the pinned pcre2_match.c assertion handling. No public/release/consumer change.

A positive assertion starts at the current position, accepts its first complete
ordered body success, restores position, keeps captures and discards body retry
points. Outer failure cannot revisit a successful assertion. A negative
assertion succeeds only when all body alternatives fail and restores every
body mutation; a matching body makes it fail. Nested assertions obey the same
transaction rules. Existing captures may be restored to prior values, not just
cleared. Captures may extend beyond the reported whole-match span.

Assertions allocate no capture slot; their bodies use normal numbering, names,
branch resets, options, classes, repeats and nested supported assertions. Local
options restore at assertion exit. Backreferences and consumption use the same
original subject and UTF fragment. Backslash-G keeps the caller's start offset.
Backslash-K anywhere inside assertion nesting is invalid, including through
ordinary groups; literal quoted/escaped K text remains ordinary text.

Lookbehind stays fixed-literal. Non-atomic/long aliases, conditions, recursion,
verbs, broader options and directly quantified assertions remain pending.
PCRE2 10.48 has special direct-assertion repeat rules; do not silently lower
them as ordinary repeats. Ordinary groups around assertions may repeat using
the existing zero-progress guard. Keep source-byte diagnostics and 256-level
parser nesting, capture/expansion/mark, 1024 saved-path and match-work limits.

## Approaches

1. Recursive calls into a second matcher or new subject slices duplicate the VM,
   lose caller start/fragment boundaries and risk native stack exhaustion.
2. Re-run assertions on outer backtracking violates atomic assertion semantics.
3. **Choose inline bytecode and typed guard frames.** Compile a general body
   between lookahead-enter/exit instructions. Enter saves position, journal
   checkpoint, atomic/assertion depths and continuation in an assertion guard
   on the existing bounded backtracking stack. Body execution uses the same
   instruction loop and capture journal. Guard kind determines whether body
   exhaustion succeeds (negative) or propagates failure (positive).

On body success prune only frames above the guard, then pop it. Positive exit
keeps capture changes available to outer rollback; negative exit rolls back to
the guard and propagates failure. Every normal saved branch records assertion
depth alongside atomic depth. Unwinding restores both stacks and the journal.
Frames contain only integers, indexes and compact enums; no retained spans,
interior pointers, native recursion or per-backtrack capture-array copies.
Assertion boundaries can use a lazily allocated list of backtrack depths so
programs without general lookahead retain their storage path. Guard frames
count against the existing saved-path limit. Instruction transitions and guard
failure unwinds consume work and honor cancellation.

## Parsing, lowering and analysis

Extend existing heap parser frames with assertion kind/context; nested ordinary
groups inherit assertion context for K validation. Close lookahead into a new
semantic node. Resolve named references inside bodies. Preserve existing
fixed-literal lookahead as the same literal-assertion node/instruction whenever
a single branch consists only of literals with the entry casing, including
quoted/braced literals and ignored comments. This preserves existing fast paths
and work counts. Mixed options or nonliteral syntax uses general bytecode.

Compile bodies with the existing heap continuation stack. Extend explicit-
newline analysis through lookahead. Conservatively mark general lookahead as
potentially zero-width for the malformed-offset synthetic candidate, along
with optimized negative/empty literal lookahead. A real fragment ending at
the requested offset already supplies a zero-width candidate with valid left
context; never shadow it with an empty fragment. Body consumption still
cannot cross a fragment. After failure, nullable paths may also try later
malformed bytes in gaps between real fragments. Enumerate those zero-width
candidates lazily with indexes; do not add candidates inside valid scalars or
allocate a gap list. Between two malformed codepoints, both word-boundary
forms fail (the pinned JIT uses an invalid/invalid sentinel), rather than
treating both sides as ordinary nonword characters. Subject start and EOF
retain their existing handling. Pin this against the oracle at every
selected offset. Keep suffix/search shortcuts restricted to existing proven
shapes. No Regex/native fallback or cloned byte/UTF engines.

## Verification

Test first: all whole/capture spans; first-success atomicity; positive capture
visibility and negative/outer rollback; previous repeat values; named/numeric
references; nested positive/negative/atomic groups; byte/UTF/invalid fragments;
caller offsets/G, captures outside G0, options/comments, newline conventions,
empty paths, depth/expansion/saved-path/work and cancellation. Preserve old
literal assertion work tests and pending diagnostic controls. Require an
independent whole-branch review and six green Staging jobs on the exact head,
net8/net9/net10 plus frozen/workload/API/extraction/package-only gates.
