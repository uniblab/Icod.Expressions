# Native 16-bit bounded variable repetition

The word sequence compiler admits one finite variable-count repeat per
top-level path when its child is a supported literal, dot, scalar class or
bounded scalar set. The shared grammar supplies `{m,n}`, `?` and their lazy
forms. Counts must lie within zero through 64 and produce at most 64 total
ordered paths across the pattern. Every expanded path still holds at most
4,096 nodes. Unbounded `*`, `+` and `{m,}` remain unsupported.

Greedy repeats try counts from maximum down to minimum; lazy repeats try them
from minimum up to maximum. Each count includes the original prefix and
continuation, so a failed suffix or rejected complete match can retry the
next count at the same subject start. A later top-level alternative follows
all of the prior branch's counts. The matcher charges branch admission and
each comparison to its existing cancellation-aware work quota. It reads
original raw 16-bit units or strict UTF-16 scalars and returns code-unit
coordinates for group zero. Nested groups, more than one variable repeat in
a path, captures and possessive wrappers remain explicitly unsupported.

## Evidence

Forty-seven source/subject/start fixtures across six compiled defaults and
64 call masks compare 18,048 outcomes with pinned PCRE2-16: 15,176 matches
and 2,872 misses. They cover greedy and lazy preference, suffix failure,
zero-count policy retries, ordered alternatives, assertions, UCP, caseless
classes, raw surrogate units and supplementary scalars. Four valid-Unicode
examples compare managed byte and word coordinates. Work traces check exact
retry charges. Admission tests cover 64/65 expanded paths, count 64/65, a
second variable repeat and unsupported unbounded or nested forms. Every
native preflight row JIT compiled without error; the LF-terminated output
SHA-256 was:

`b4432095d4d3376864080f8f4d281e0d6c4a74d04ef788543ee315449d5942ff`

The independent native oracle and pinned PCRE2 10.48/SLJIT revisions are
recorded in the [word sequence evidence](PCRE-WORD-SEQUENCES.md). Dynamic
unbounded repetition, multiple variable repeats, nested execution, durable
captures and the broader P10–P12 gates remain open. The following
[multiple-repeat increment](PCRE-WORD-MULTIPLE-REPEATS.md) admits several
finite variable counts within the same global path bound.
