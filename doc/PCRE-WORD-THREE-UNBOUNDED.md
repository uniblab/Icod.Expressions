# Native 16-bit ordered three-repeat continuations

The word sequence compiler admits three unbounded scalar atoms in one
top-level path when the continuations are straight. Supported atoms,
absolute assertions and exact counts may appear before, between and after
them. Finite variable counts before the first repeat retain their ordered
outer paths. Finite variable counts after the first repeat, a fourth
unbounded repeat, groups, captures and possessive wrappers remain outside
this bounded slice. Unbounded minima and exact counts remain capped at 64;
the 64-path and 4,096-expanded-source-node limits still apply.

At each first-repeat count, the executor tries the second-repeat counts;
at each second count, it tries the third-repeat counts. Each level follows
its own greedy or lazy ordering. Failed and policy-rejected completions
return to the next count at the appropriate level. Reverse scalar
navigation uses the original subject view without storing count positions.
Every comparison, retry, rewind and completion shares the cancellation-aware
work quota. Raw 16-bit units and strict UTF-16 scalars retain original
code-unit coordinates for group zero.

## Evidence

Fifty-one source/subject/start fixtures across six compiled defaults and
64 call masks compare 19,584 outcomes with pinned PCRE2-16: 17,488 matches
and 2,096 misses. The matrix includes all eight greedy/lazy combinations,
overlapping atoms, nonzero minima, finite prefix choices, exact counts,
top-level alternatives, assertions, empty and end-anchored policies, UCP,
caseless classes, raw surrogates and supplementary scalars. Three valid
Unicode cases compare managed UTF-8 byte and UTF-16 word coordinates. A
failed overlapping search with an exact work trace verifies quota and
cancellation behavior. All native preflight rows JIT compiled without
error; their LF-terminated output SHA-256 was:

`e17a78ebf172bebf7d5dbb5e5cf28cd050eb5465dd81ff0d66833ca1f129f5c0`

The independent native oracle and pinned PCRE2 10.48/SLJIT revisions are
recorded in the [word sequence evidence](PCRE-WORD-SEQUENCES.md). Finite
choices within three-repeat paths, further repeats, general nested execution,
durable captures and broader P10–P12 gates remain open.
