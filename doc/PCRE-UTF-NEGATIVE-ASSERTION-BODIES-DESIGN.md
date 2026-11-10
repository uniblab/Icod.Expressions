# Bounded regular negative assertion bodies before nullable repeats

Continue issue102 after merged PR204. PCRE2 10.48 `set_start_bits` skips negative
lookahead/lookbehind bodies before studying the following consuming head.
The present class-only admission still rejects ordinary sequences, alternatives,
groups and repeats inside those assertion bodies. Preserve the established
mandatory CR-excluding repeated-group head and supported leading/middle sites.

## Approach and boundaries

Retain the existing single-class path and its exact visit costs. Add a fallback
for ordinary regular negative assertion bodies, using an iterative compiler-only
walk over branch/node entries. Admit literals, scalar classes/sets, dot,
word boundaries, capturing/noncapturing/atomic groups, repeats and ordinary
nested lookahead/lookbehind or collapsed literal assertions. Charge each body,
branch, node, range, predicate and decoded literal-assertion scalar under
cancellation. Bound both visited and queued work at1,000; commit prefix charges
only after the full body qualifies. A skipped negative body needs no mandatory
or nonnullable inner head, and finite/unbounded repeats visit their child once.

Backreferences, calls, conditions, anchors, match-start reset and unknown nodes
remain excluded. Positive assertion admission and existing literal-head rules
are unchanged. This completes regular structural bodies within this placement;
it does not claim every assertion or full optimizer parity. A global matcher or
parser metadata change would affect unrelated paths. Unrestricted O(1) body
admission would discard the conservative structural/resource contract. Both
alternatives are rejected in favor of the bounded fallback.

## Evidence and verification

Pinned PCRE2 10.48 source7978954dbd2efc6f2196869290553cf1871b4ce6 native JIT
probe passes2,720 compilation/bitmap/JIT-size checks and5,440 literal complete
capture comparisons. LF is present and CR absent in every bitmap. Cases cover
18 body shapes, including200 nested groups and64 alternatives, UCP/caseless,
leading/middle ahead/behind, five nullable repeat shapes and CRLF/ordinary
subjects. Unbounded body repeats are ahead-only; negative captures stay unset.

Add the same2,720 fixtures before source changes, observe successful-build RED
in CI, and preserve their blob. Keep all43,059 baseline expectations, frozen214
and frozen35. C#13/net8/9/10, six native Staging OS/architecture runners, all
configured package/API/parity/workload checks. No subagents, Python, SDK or
PowerShell installation. No parser, matcher, API, limit, dependency, consumer or
release change. Issue102 remains open, Grep last, user owns review/merge.
