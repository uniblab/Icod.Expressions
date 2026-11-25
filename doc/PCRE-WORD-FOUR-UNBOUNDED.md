# Native 16-bit four unbounded repeats

The bounded word-sequence compiler now admits four unbounded scalar atoms
in a top-level path. Finite variable counts may occur before, between and
after those atoms. The common builder partitions each level at its repeat
boundary, so later alternatives are retried inside the preceding greedy or
lazy count. Exact counts, straight atoms, absolute assertions and ordered
top-level alternatives remain available.

The product of finite-choice levels, summed across top-level paths, is at
most 64. Each expanded source path has at most 4,096 nodes, and exact
counts and unbounded minima remain capped at 64. A fifth unbounded repeat,
nested groups, captures and possessive wrappers remain unsupported. Raw
16-bit units and strict UTF-16 scalars match the original subject view;
group zero uses original code-unit coordinates. Admission, comparison,
rewind and completion charge the cancellation-aware work quota.

## Differential coverage

Forty-three fixtures across six compiled defaults and 64 call masks compare
16,512 outcomes with pinned PCRE2-16. A standalone native preflight returned
14,720 matches and 1,792 misses, with no compile or match errors. Its
LF-terminated output SHA-256 was
`1d192d0894329436809dd642debbb9273a51b074bc4066c996ea366ca6834ef6`.
The fixtures exercise five finite-choice
levels, overlapping greedy and lazy repeats, minima, alternatives, absolute
assertions, empty and end-anchored policies, UCP, caseless classes, raw
surrogates and supplementary scalars. Three valid-Unicode fixtures compare
managed UTF-8 byte and UTF-16 word coordinates. Admission bounds and exact
work traces cover the new depth and quota. The native oracle and pinned
PCRE2 10.48 revision are recorded in the [word-sequence evidence](PCRE-WORD-SEQUENCES.md).

Five or more unbounded repeats, general nested execution, durable captures
and broader P10–P12 gates remain open.
