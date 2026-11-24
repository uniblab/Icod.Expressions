# Native 16-bit fixed-count atom repetition

The word sequence compiler now admits `{n}` and `{n,n}` for a supported
literal, dot, scalar class or bounded scalar set. Counts from zero through 64
expand to an immutable straight sequence at compile time. The compiled
sequence can be placed before or after other atoms and inside ordered
top-level alternatives. The zero count contributes an empty path, so
`NOTEMPTY`, `NOTEMPTY_ATSTART` and `ENDANCHORED` still apply at completion;
another branch at the same start remains available after rejection.

Each branch has at most 4,096 expanded nodes. The limits also bound matching
work in proportion to the caller's existing work quota. The expansion owns
no additional subject storage; comparisons read original 16-bit units in raw
mode and Unicode scalars in strict UTF-16 mode. Group-zero indices and lengths
remain code-unit coordinates. An exact count needs no retry frame and its
greedy or lazy spelling has the same matching behavior. Captures, nested
groups, variable or unbounded repetition and possessive wrappers remain
explicitly unsupported in the word compiler.

## Evidence

Fifty source/subject/start fixtures across six compiled defaults and 64 call
masks compare 19,200 outcomes with pinned PCRE2-16: 13,332 matches and 5,868
misses. Raw and strict modes cover zero counts, ordered empty retries,
nonzero starts, assertions, UCP, caseless classes, raw surrogate units and
supplementary scalars. Four valid-Unicode examples compare the managed byte
and word coordinates. Work traces check that two expanded literal
comparisons consume two steps. Admission controls cover counts 64/65 and
branch expansions 4,096/4,097. The native preflight JIT compiled every row
without an error; its LF-terminated output SHA-256 was:

`28a9875a4c5fc8f4ae0bba1e0fbb39bba8a9093c2479fa80212bb37b4a2ac432`

The independent native oracle and pinned PCRE2 10.48/SLJIT revisions are
recorded in the [word sequence evidence](PCRE-WORD-SEQUENCES.md). Ordered
variable repetition, nested execution, durable captures and the broader
P10–P12 gates remain open. The following [variable-repeat increment](PCRE-WORD-VARIABLE-REPEATS.md)
admits one finite variable count per top-level path.
