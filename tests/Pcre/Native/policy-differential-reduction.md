# Bounded policy differential sweep and reduction

The test-only [probe](../PcrePolicyDifferentialProbe.cs) connects live
PCRE.NET 1.6.0 / PCRE2 10.48 comparisons to the existing
[byte fixture reducer](../PcreFixtureReducer.cs). It distinguishes candidate
compilation failures, resource diagnostics, match presence, numbered capture
spans, and successful marks. Captures retain original byte offsets, signed
lengths, and unset slots. No-match is separate from errors.

The [sweep](../PcrePolicyDifferentialTests.cs) enumerates twelve small grammar
templates in source order, mode bits 8, 9, 11, and 12, and JIT/default versus
NO_JIT source prefixes. Each of these 96 profiles checks eight explicit
subject/start pairs and all 64 call-policy masks: 49,152 comparisons.
The subjects are bounded ASCII strings; malformed UTF, wider grammars,
native 16-bit execution, and randomized fuzz closure remain separate work.

On disagreement, the sweep reduces pattern and subject while retaining mode,
policy mask, start offset, and mismatch class. Native-invalid patterns cannot
replace a reproducing witness. Native runtime errors, candidate exceptions,
and cancellation propagate. The evaluation limit covers every attempted
candidate; the final witness reuses its already-confirmed comparison.
Failure messages contain original and reduced results, reduction status,
evaluation count, and a seven-field hex replay row accepted by
[match-policy-oracle.c](match-policy-oracle.c).

Fault controls deliberately drop NO_JIT dispatch or change a successful mark
in the candidate compiler. Live native comparisons detect and reduce both
faults, preserving their respective capture and mark classes. Additional
controls check presence/compile/resource classifications, the evaluation
limit, non-reproduction, cancellation, exception propagation, and owned byte
views with nonzero start offsets.

An independent static 8-bit C preflight used PCRE2 10.48 at
`7978954dbd2efc6f2196869290553cf1871b4ce6` and SLJIT
`de0259c7aaf36aa40cba8014f3fad3edde9307f9`. In the sweep's profile/subject/mask
order, assigning profile IDs 1–96, it returned 21,760 matches and 27,392
misses, with zero native errors. All JIT compilation returns were zero;
24,576 rows had positive JIT size and 24,576 had zero size. The LF-terminated
raw output has SHA-256
`ba6380b0315775b5015f52f5357f094807a56b748a664befe19a2bd5ca980209`.
