# UTF star prefixes inside capturing ancestors

## Intent and native evidence

Continue #7/#102 after merged #142. Current first-star discovery descends only
noncapturing groups. Native bitmap study descends capturing groups too.
detect_early_fail accepts CBRA only when is_optimized_cbracket is true and
depth<4; check_opcode_types clears that bit for references and capture tests.
Initial60 bounded native probes at depths1/3/4/5/33 confirm:
(B*(S+))A, S=[\x09-\x0c\x{100}], ANYCRLF enters CRLF+A at LF1:2.
B+CRLF+A misses through four unreferenced ancestors, enters LF2:2 at depth5+.
Referencing/testing ancestor1 permits LF2:2 even at depth1; direct ancestor
reference{0} is erased and preserves the miss. Following S capture dependency
must not disable an earlier star cache. Native.so SHA256
7753b78ef5e7ed18a885392917fa789c9e8f801058ad005b73b9028fedac8395.
Reference-only PCRE.NET1.6.0/PCRE2 10.48 JIT.

## Approach (implementer decision under standing autonomy)

Extend FindUtfCrLfRootStarAtom's existing bounded iterative group descent to
PcreByteGroup. Collect only capture numbers on the star ancestor chain.
At the first supported unbounded star, call existing ClassifyUtfCaptureRepeat
with those numbers. Ineligible classification returns default; otherwise return
the identity with Skip=depth<=4 && classification.Skip. Noncapturing-only/root
paths keep their existing behavior and avoid this extra classification.
Use existing HashSet<int> compile-time storage and helper; no duplicate dependency
walker. Generalizing the classifier or adding runtime state is unnecessary for
this gap; refusing capturing ancestors leaves known native entry differences.

Both capturing and noncapturing groups charge group1/count toward ancestor depth.
Erased primitives preserve root1+2/operand+possessive-wrapper1 accounting;
cancellation/shared1000 proof ceiling remains. Reject explicit atomic ancestors,
alternatives, nullable prefix-only wrappers whose required set is outside,
retained/query/unsupported nodes and calls/pruning per existing classification.
Retained zero-count capture-reference group bodies remain dependencies, while
direct reference{0} remains erased. Support numeric/named/duplicate references
and tests via existing parser-resolved numbers; unrelated suffix captures do
not disable star Skip. Final mandatory-set proof still clears identity/Skip.
Existing source/caseset/r/UCP/CR/predicate/range/capture/BlocksSkip/ExplicitNewline/
fragment/work/parser/depth guards and leading-PLUS marker remain unchanged.

## Scope and constraints

Only private compiler discovery and six documentation files plus one reference
test class. No matcher/parser/membership/folding/Step/runtime state/layout/
instruction/program/frame/journal/snapshot/span/pointer/API/limit changes.
No workflow/package/version/dependency/consumer/CommandFramework changes.
Parser256/captures256/instructions4096/marks256/frames1024/work1m/proof1000/window12.
C#13/net8/9/10, UTF8 LF, root project/solution,src/tests/samples.
All11718 baseline expectations/frozen35 unchanged. No subagents/Python/SDK install.
Local Debug; PR Staging; native six OS/arch and net8/9/10 gate.

## Verification and boundaries

Pin full native capture spans for unreferenced/related/unrelated ancestors,
mixed capturing/noncapturing depth4/5, deep33/199, source/modifiers/casing/UCP,
erased prefixes, named/duplicate references/tests, retained zero-group barriers,
following deep/atomic/capture groups and unsupported controls. Six newlines,
every caller offset, strict/tolerant fragments, work exact/exhausted/observed,
cancellation/default budget/parser257/captures257/backtrack depth.
Tests-only draft then successful-build new-class RED/all11718baseline pass;
original tests/expectations immutable thereafter. Source then exact-head
six native Staging full/frozen214 on net8/9/10,0failed/skipped tests/buildwarnings/
errors, all configured gates and7equal workloads. Wholebranch author review,
strict logs/tree/blob/tests-afterRED/frozen/clean/whitespace audit. #102 open,
PR ready/unmerged for user review. Atomic ancestors/other placements/predicates/
group/alternative/negated/property/call/DEFINE/condition availability stay open;
byte99/unsafe100 separate. No complete PCRE/release/Grep readiness claim.
Publication/all known migrations precede CF removal; Grep last.
