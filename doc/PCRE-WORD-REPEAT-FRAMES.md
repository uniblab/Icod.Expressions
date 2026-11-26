# Native 16-bit explicit repeat frames

The bounded word-sequence executor uses one iterative loop for root paths,
repeat continuations and ordered finite suffix choices. A match-local stack
stores the node sequence, instruction index, subject position, repeat start
and retry count. Failed suffixes restore the most recent retry point, so
greedy counts decrease and lazy counts increase in the same order as before.
Completion policies can reject a result and resume the remaining choices.

Compilation now admits up to 64 sequential unbounded atoms per top-level
path, replacing the previous four-repeat bound. The continuation builder
works from the last repeat toward the first without recursive calls. It
flattens deterministic sections once and retains ordered choice nodes only
where finite counts require them. Each path still has at most 4,096 expanded
nodes, each exact count or unbounded minimum is at most 64, and the combined
finite-choice product across all top-level alternatives is at most 64.
The shared parser's existing 256-repeat-state-mark ceiling also applies
across the entire pattern; these per-path bounds do not supersede it.
These bounds permit at most 64 pending repeat frames and six suffix-choice
frames. A straight path without retries does not allocate a frame stack.

Raw-word and strict UTF-16 modes retain original code-unit coordinates.
Retries rewind whole supplementary scalars and whole CRLF sequences consumed
by `\R`. The existing work counter still charges comparisons, assertions,
repeat admission/growth/rewinds, ordered choices, completion and candidate
advancement. Stack bookkeeping adds no charges. The stack is local to one
match and clears between branches and candidates, including after failed
paths; immutable compiled patterns retain no execution state.

## Evidence and remaining scope

Sixty fixtures at repeat depths 5, 8, 16, 32 and 64 cover greedy and lazy
counts, distributed finite choices, empty completions, failed deep paths,
later alternatives, supplementary scalars and atomic CRLF. Eight compiled
defaults and 64 call masks produce 30,720 comparisons with pinned PCRE2-16.
The native preflight returned 23,840 matches and 6,880 misses with no errors;
its LF-terminated output SHA-256 is
`ae9e01d9a9f5de9a0b9776e8f964dda39b28ed1d03762ed92beff631295afc7f`.
The oracle revisions are pinned in [word-sequence evidence](PCRE-WORD-SEQUENCES.md).

Additional tests cover managed cross-width spans, raw surrogate units,
independent repeat/path bounds, exact work exhaustion, cancellation during
unwinding and reuse across branches, candidates and calls. An ambiguous deep
failure must terminate at its 10,000-step work limit. Existing word suites
continue to check the earlier shallow repeat behavior and exact quotas;
obsolete five-repeat rejection cases are replaced by the new admission
boundary checks.

This extends sequential atom repetition. General nested groups and repeats,
capture journals and durable capture results still require separate lowering
and execution work. Effective CRLF/ANYCRLF/ANY line conventions still need
native search-advancement and prefix-proof parity. Graphemes, broader
cross-width qualification and the public PCRE API also remain open.
