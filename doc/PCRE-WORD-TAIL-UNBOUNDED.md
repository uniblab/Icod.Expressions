# Native 16-bit unbounded tail repetition

The word sequence compiler admits one unbounded repeat at the end of a
top-level path when its child is a supported literal, dot, scalar class or
bounded scalar set. The shared grammar supplies `*`, `+`, `{m,}` and their
lazy forms, with a minimum from zero through 64. Exact and finite variable
repeats may precede the tail. Other top-level paths remain ordered; the
existing 64-path and 4,096-node bounds apply to the compiled prefix. An
unbounded repeat with a continuation, another unbounded repeat, a nested
group or a possessive wrapper remains explicitly unsupported.

The matcher consumes the original subject as raw 16-bit units or strict
UTF-16 scalars. A greedy tail compares until the atom fails, then completes
the longest count. With no continuation, shorter counts cannot satisfy an
empty-match or end-anchored policy rejected at the longest count. A lazy
tail tries completion at each eligible count before comparing the next atom;
it may accept an empty path without scanning the subject. A policy rejection
lets it consume and retry, and later top-level paths remain available.
The existing work quota charges tail admission, each comparison and each
completion, with cancellation checks. Group zero stays in code-unit
coordinates. No repeat-count array or subject copy is allocated.

## Evidence

Forty-four source/subject/start fixtures across six compiled defaults and
64 call masks compare 16,896 outcomes with pinned PCRE2-16: 14,184 matches
and 2,712 misses. The matrix includes greedy/lazy choices, empty and
end-anchored policies, finite prefixes, ordered paths, assertions, UCP,
caseless classes, raw surrogate units and supplementary scalars. Four
valid-Unicode cases compare managed byte and word coordinates. Exact work
traces, a 10,000-unit subject under a small quota, and admission boundaries
check that tail matching is subject-driven and bounded by the work counter.
Every native preflight row JIT compiled without error; its LF-terminated
output SHA-256 was:

`f8a8f0fc36316cdeac20bfb1d2a4b92dccaf6f3ee4f908d8a77ff5ae40fefb78`

The independent native oracle and pinned PCRE2 10.48/SLJIT revisions are
recorded in the [word sequence evidence](PCRE-WORD-SEQUENCES.md). Unbounded
repeats with continuations, nested execution, durable captures and the
broader P10–P12 gates remain open.
