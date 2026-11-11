# Immediate successful termination with ACCEPT

PR215 follows merged PR214. Issue7's byte-authoritative managed PCRE contract
continues unchanged. This increment supports exact, no-argument `(*ACCEPT)`;
named ACCEPT/MARK and the remaining control verbs stay explicitly rejected.

## Scope and approach

ACCEPT terminates the current match, assertion, or subroutine immediately. It
skips the remaining body, including script-run validation and lookbehind end
checks. At root/assertion level, open captures at the same assertion depth
close at the current byte position; exterior captures remain open. Native JIT
ignores these synthetic closes inside called groups, including return-list
captures. Completed selected captures still return through the existing journal.

Use one AST leaf and one VM instruction. The compiler records lexical open
captures and patches assertion-local exits. The VM chooses a newer assertion
before its enclosing call; an ACCEPT in a call with no newer assertion returns
that call. Prune the accepted scope's internal choices and restore its atomic
nesting, then reuse ordinary AssertionExit/Return/Accept. NOTEMPTY rejection at
root remains retryable. Quantification uses existing greedy/lazy/possessive
lowering, including succeed-on-backtrack `(*ACCEPT)??`.

Alternatives considered: rewriting ACCEPT as an empty group cannot express
termination; a second matcher or exception unwinding would duplicate the
existing scope/journal protocol. The existing VM is the implementation choice
under the standing authorization to continue the roadmap inline.

Direct ACCEPT terminates consumed lookbehind width scanning like FAIL; nested
groups and quantified forms retain native structural width rules. Global
reference and source validation still inspect unreachable tails. Nullable
fragment metadata conservatively admits ACCEPT programs without allowing scalar
consumption through invalid UTF. No optimizer expansion belongs here.

## Evidence and bounds

Pinned PCRE2 10.48 source (`pcre2_compile.c`, `pcre2_jit_compile.c`) and
`pcre2pattern.3` define closes, scope success and quantifiers. Native probes pin
literal capture spans before managed implementation. PCRE2 declines JIT for
non-atomic assertions containing ACCEPT; those cases use its ordinary matcher
and remain explicit in the reference evidence.

Tests cover root/capture/branch reset/options/quotes, all repeat preferences,
K/NOTEMPTY rollback, nested positive/negative/conditional/non-atomic assertions,
fixed/variable lookbehind, calls/recursive calls/return lists, atomic/script-run
bypass, malformed input, every selected byte offset and work/cancellation.
All scanning/pruning/closing uses shared bounded work and cancellation. No new
saved VM state category or public API. Existing 4096 compile/instruction and
1024 live saved-frame limits remain. GNU/BRE/ERE and frozen35/frozen214 untouched.

The remaining roadmap includes named verbs, other control verbs/directives,
callouts, UTF C, broader optimizer/resource conformance, UTF16/API, consumer
performance and release. Grep stays last.
