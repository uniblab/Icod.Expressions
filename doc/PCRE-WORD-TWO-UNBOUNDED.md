# Native 16-bit ordered two-repeat continuations

The word sequence compiler admits up to two unbounded scalar atoms in one
top-level path. Supported straight atoms, exact counts and absolute
assertions can appear before, between and after them. Finite variable
counts before the first repeat retain their ordered outer paths. Variable
counts between or after the unbounded repeats remain unsupported; they
need a nested suffix-choice representation in a later increment. Three
unbounded repeats, groups, captures and possessive wrappers remain
unsupported. Unbounded minima and exact counts stay capped at 64; the
64-path product and 4,096 expanded-source-node bounds remain in force.

At each first-repeat count, the executor advances through the intervening
straight nodes and tries every eligible second-repeat count in greedy or
lazy order. If all second-repeat counts fail or are rejected by match
policy, the first repeat retries its next count. Reverse scalar navigation
uses the original subject view; no subject copy or count-position array is
stored. Every comparison, retry, rewind and completion uses the existing
cancellation-aware work quota. Raw 16-bit units and strict UTF-16 scalars
retain original code-unit coordinates for group zero.

## Evidence

Forty-nine source/subject/start fixtures across six compiled defaults and
64 call masks compare 18,816 outcomes with pinned PCRE2-16: 16,544 matches
and 2,272 misses. The matrix includes adjacent and separated repeats,
overlap, all four greedy/lazy combinations, nonzero minima, finite prefix
choices, top-level alternatives, assertions, empty and end-anchored
policies, UCP, caseless classes, raw surrogates and supplementary scalars.
Three valid-Unicode cases compare managed UTF-8 byte and UTF-16 word
coordinates. A long overlapping failure under a small work quota and an
already-cancelled request verify bounded execution. All native preflight
rows JIT compiled without error; their LF-terminated output SHA-256 was:

`8bdf429a38c4a2922eea9dc137d6c7223f523532dae5382ca04b02f61d354d6a`

The independent native oracle and pinned PCRE2 10.48/SLJIT revisions are
recorded in the [word sequence evidence](PCRE-WORD-SEQUENCES.md). Nested
suffix choices, general repeat frames, durable captures and broader
P10–P12 gates remain open.
