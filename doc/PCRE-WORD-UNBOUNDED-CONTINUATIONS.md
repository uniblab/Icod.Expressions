# Native 16-bit unbounded repeat continuations

The word sequence compiler admits one unbounded repeat of a supported
literal, dot, scalar class or bounded scalar set before a straight suffix.
The suffix can contain supported atoms, absolute assertions and exact
counts. Finite variable repeats can precede the unbounded atom, where their
ordered choices remain outer paths; finite variable repeats after it remain
explicitly unsupported because their choices must be nested inside each
unbounded count. The minimum of `*`, `+` or `{m,}` is bounded at 64, with
the existing 64 compiled paths and 4,096 nodes per path retained. Nested
groups, another unbounded repeat, captures and possessive wrappers remain
unsupported.

The executor compares an unbounded atom on the original raw 16-bit units or
strict UTF-16 scalars. Greedy matching consumes to failure and tries the
continuation at each count from longest to minimum; rewinds use reverse
scalar navigation on the original subject. Lazy matching tries the
continuation before consuming the next atom. A complete suffix still checks
empty-match and end-anchored policies, and a failed or rejected count leaves
later choices and top-level paths available. Repeat admission, comparisons,
rewinds, suffix nodes and completions all share the cancellation-aware work
quota. No subject copy or count-position array is allocated. Group zero
remains in original code-unit coordinates.

## Evidence

Forty-six source/subject/start fixtures across six compiled defaults and
64 call masks compare 17,664 outcomes with pinned PCRE2-16: 15,488 matches
and 2,176 misses. The matrix includes greedy/lazy suffix failure, scalar
rewinds, exact suffix counts, finite prefixes, ordered alternatives, empty
and end-anchored policies, UCP, caseless classes, raw surrogate units and
supplementary scalars. Four valid-Unicode examples compare managed byte and
word coordinates. Exact work traces and a 10,001-unit subject under a small
quota verify bounded retries. Every native preflight row JIT compiled
without error; its LF-terminated output SHA-256 was:

`0440440c8d5b2d471a3331d7f50195fea79660c29939a0b419080b2c4a8784c6`

The independent native oracle and pinned PCRE2 10.48/SLJIT revisions are
recorded in the [word sequence evidence](PCRE-WORD-SEQUENCES.md). Multiple
unbounded repeats, finite variable suffix choices, nested execution,
durable captures and the broader P10–P12 gates remain open.
