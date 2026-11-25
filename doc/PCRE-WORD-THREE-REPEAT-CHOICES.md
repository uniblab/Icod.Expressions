# Native 16-bit finite choices around three unbounded repeats

The word sequence compiler admits finite variable atom counts before,
between and after three unbounded atoms in a top-level path. The bounded
builder partitions choices at each repeat boundary and stores them behind
the corresponding unbounded atom. Prefix choices remain outer paths;
each later choice runs inside the greedy or lazy count that precedes it.
The same builder now handles zero through three unbounded repeats, keeping
the existing one- and two-repeat ordering. Exact counts, straight atoms,
absolute assertions and top-level alternatives remain available.

The product of all finite-choice levels, summed across top-level paths,
remains at most 64. Every expanded source path still has at most 4,096
nodes; exact counts and unbounded minima remain capped at 64. A fourth
unbounded repeat, nested groups, captures and possessive wrappers remain
unsupported. Raw 16-bit units and strict UTF-16 scalars match the original
subject view, with group zero in original code-unit coordinates. Ordered
admission, comparison, rewind and completion share the cancellation-aware
work quota.

## Evidence

Fifty source/subject/start fixtures across six compiled defaults and 64
call masks compare 19,200 outcomes with pinned PCRE2-16: 15,896 matches
and 3,304 misses. The matrix covers all four choice levels, overlapping
atoms, greedy and lazy counts, finite prefixes, top-level alternatives,
absolute assertions, empty and end-anchored policies, UCP, caseless classes,
raw surrogates and supplementary scalars. Three valid-Unicode fixtures
compare managed UTF-8 byte and UTF-16 word coordinates. Product-bound and
exact work-trace tests exercise the new builder and quota. Every native
preflight row JIT compiled without error; its LF-terminated output SHA-256
was:

`ce0082e9dd59ddfda2bbee5c7af47e0eda496c978efce83576bfa89a94b1d10d`

The independent native oracle and pinned PCRE2 10.48/SLJIT revisions are
recorded in the [word sequence evidence](PCRE-WORD-SEQUENCES.md). Four or
more unbounded repeats, general nested execution, durable captures and
broader P10–P12 gates remain open.
