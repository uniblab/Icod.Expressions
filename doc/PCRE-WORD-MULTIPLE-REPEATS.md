# Native 16-bit multiple bounded variable repeats

The word sequence compiler now admits several finite variable-count atom
repeats in a top-level path. Each child remains a supported literal, dot,
scalar class or bounded scalar set. The shared grammar supplies `{m,n}`, `?`
and their lazy forms. Every count lies within zero through 64. The product
of count choices, plus other top-level paths, must fit the existing 64-path
bound; each expanded path holds at most 4,096 nodes. The compiler checks the
product before expansion and rejects the full pattern if it exceeds the
limit. Unbounded `*`, `+` and `{m,}` remain unsupported.

Count choices are enumerated in source order: the leftmost repeat is the
outer choice, and each later repeat cycles through its greedy or lazy counts
inside it. The original continuation follows every count combination.
Consequently, a failed suffix or rejected complete match retries the later
repeat before revisiting an earlier one, then later top-level alternatives.
The existing matcher charges branch and comparison steps to one work quota
with cancellation. It reads original raw 16-bit units or strict UTF-16
scalars and returns code-unit group-zero coordinates. Nested groups,
unbounded repeats, captures and possessive wrappers remain explicitly
unsupported.

## Evidence

Forty-three source/subject/start fixtures across six compiled defaults and
64 call masks compare 16,512 outcomes with pinned PCRE2-16: 15,432 matches
and 1,080 misses. Adjacent repeats compete for the same text under mixed
greedy and lazy choices; cases also cover policy-rejected empties, suffix
failure, ordered alternatives, assertions, UCP, caseless classes, raw
surrogate units and supplementary scalars. Four valid-Unicode examples
compare managed byte and word coordinates. Work traces check exact retry
charges. Admission tests cover products of 64 and 65 paths, many optional
atoms, other top-level branches and unsupported unbounded or nested forms.
Every native preflight row JIT compiled without error; the LF-terminated
output SHA-256 was:

`d1febc1a25b422a1cc3ddc2b4773783e08e014c8d4c6832e96346316673e9510`

The independent native oracle and pinned PCRE2 10.48/SLJIT revisions are
recorded in the [word sequence evidence](PCRE-WORD-SEQUENCES.md). Dynamic
unbounded repetition, nested execution, durable captures and the broader
P10–P12 gates remain open.
