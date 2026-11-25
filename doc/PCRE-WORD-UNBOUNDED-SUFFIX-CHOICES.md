# Native 16-bit unbounded repeat with finite suffix choices

The word sequence compiler admits one unbounded atom followed by finite
variable atom counts. Suffix choices are compiled in their ordinary greedy
or lazy order, but stored behind the unbounded repeat. For each count of the
unbounded atom, the executor tries the suffix choices in order before
advancing to the next greedy or lazy count. Finite choices before the repeat
remain outer branches, preserving their precedence over count retries.
Ordered top-level alternatives retain source order.

The product of prefix and suffix choices, summed across top-level paths,
remains at most 64. Each expanded path still has at most 4,096 source nodes;
fixed repeats and minimum unbounded counts remain capped at 64. Suffix
choices use the existing bounded atom and absolute-assertion grammar. Two
unbounded repeats, nested groups, captures and possessive repeats remain
unsupported. Raw words read original code units; strict UTF-16 reads scalars.
All group-zero coordinates remain code-unit offsets. Each suffix attempt
charges ordered admission and its nodes against the cancellation-aware work
quota, including failed or policy-rejected completions.

## Evidence

Forty-nine source/subject/start fixtures across six compiled defaults and
64 call masks compare 18,816 outcomes with pinned PCRE2-16: 18,072 matches
and 744 misses. The matrix includes both greedy and lazy unbounded and
finite counts, finite choices on either side of the unbounded atom,
overlapping repeats, top-level alternatives, absolute assertions, empty
and end-anchored policies, UCP, caseless classes, raw surrogate units and
supplementary scalars. Four Unicode fixtures compare managed UTF-8 byte and
UTF-16 word coordinates. Work traces and product limits cover bounded
execution and explicit unsupported diagnostics. All native preflight rows
JIT compiled without error; their LF-terminated output SHA-256 was:

`b3d74ad513e0f2bc14ca31fc1a5aa6851d409625f7c0d104bbac2f7c8d4ebdc7`

The independent native oracle and pinned PCRE2 10.48/SLJIT revisions are
recorded in the [word sequence evidence](PCRE-WORD-SEQUENCES.md). Multiple
unbounded repeats, nested execution, durable captures and broader P10–P12
gates remain open.
