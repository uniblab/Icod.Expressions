# Native 16-bit ordered top-level alternatives

The word sequence compiler admits up to 64 top-level branches made of the
already supported straight literal, dot, scalar class and absolute assertion
nodes. Each subject candidate tries branches in original source order. A
failed or policy-rejected branch leaves the same candidate available to later
branches. `NOTEMPTY`, `NOTEMPTY_ATSTART` and `ENDANCHORED` therefore apply
to each complete path without changing leftmost-first selection. Empty
branches are first-class paths. The [fixed-repeat increment](PCRE-WORD-FIXED-REPEATS.md)
now admits exact atom counts; captures, nested groups and variable repeats
remain explicitly unsupported.

The immutable compiled pattern owns branch arrays. Candidate, branch, node,
completion and scalar-advance steps consume the existing work quota with
cancellation checks. The single-branch path keeps its previous work trace.
Strict UTF-16 validates the requested suffix and reads original scalars;
raw mode reads individual 16-bit units. Group zero retains original code-unit
coordinates. The compiler bounds admitted branch count at 64, even if every
branch is empty.

## Evidence

Fifty-four source/subject/start fixtures across six compiled defaults and
64 call masks compare 20,736 outcomes with pinned PCRE2-16. They include
overlapping ordered prefixes, empty alternatives, policy-rejected paths,
absolute assertions, nonzero starts, caseless and UCP classes, raw surrogate
units and supplementary strict scalars. Four valid-Unicode cases compare
managed byte/word coordinates. Exact work traces and 64/65 branch admission
controls verify bounded retries and the preserved straight-path budget.

The independent preflight used the ordinary-regex oracle and pinned PCRE2
10.48/SLJIT revisions recorded in the [word sequence
record](PCRE-WORD-SEQUENCES.md). All rows JIT compiled with positive code
size. It produced 15,184 matches and 5,552 misses, with no native errors.
The LF-terminated output SHA-256 was:

`b05f8d41a5d4a563f37f0460253dc2270f1d1de4b52af0f8297543f47ffd312e`

Ordered repetition and nested frames, durable captures, expanded UTF policy
and the broader P10–P12 release gates remain open.
