# UTF continuation after unbounded nullable-group repeats

## Intent and evidence

Continue #7/#102 after merged #150. The existing fallback accepts finite nullable
group repeats but rejects unbounded group loops. Initial409 bounded PCRE.NET1.6.0/
PCRE2 10.48 JIT probes cover star/plus/count aliases/modifiers, group types and
query/erased/empty/alternative/star bodies. (?:B*)*(S+)A and (?:B*)+(S+)A enter
B+CRLF+A at LF2:2; (?:B*){2,}(S+)A can miss after consuming B. Atomic/possessive,
prior retained query/empty return and earlier root-star controls remain distinct.
Here S=[\x09-\x0c\x{100}]. Lazy zero-count captures may remain unset; required
or greedy participating empties retain zero-length original byte spans.
Reference source commit09639d45d4c03a8269fa2cde6edc7a7383bbc18c:
pcre2_study.c group/KETR continuation and pcre2_jit_compile.c detect_early_fail
(BRAZERO stops, recursion requires ordinary KET, earlier required copies can own).
Native.so SHA2567753b78ef5e7ed18a885392917fa789c9e8f801058ad005b73b9028fedac8395.

## Approach (implementer decision under standing autonomy)

Reuse the existing all-nullable continuation walker. Admit group repeats with
unbounded Maximum. Minimum0/1 loops block inner scalar early-fail ownership;
higher minima preserve incoming admission unless an existing atomic/possessive/
alternative/retained barrier applies. Thus unproven first stars in earlier
required copies remain rejected. Group-star atoms do not qualify for the scalar
recovery Skip flag; keep their compile-time identity but clear that eligibility.
Existing primitive identity/depth/capture rules and lowering remain unchanged.

Rename compile-time RepeatMaximum to RepeatCopies: finite repeats still use
Maximum; unbounded repeats conservatively use Minimum+1 to account for required
copies plus one loop body. Reject Minimum>=1000 before addition. The group body
is checked once, every branch must be fully nullable, and existing division-
before-multiplication charges the bounded study copies including empty/nested
bodies under1000 visits. Pending-repeat/alternative guards continue preventing
early final-set acceptance. No infinite copying or rescan of the same body.

Considered a general mandatory/nullable bitmap analyzer and assigning ownership
through every required copy. Both expand beyond this proof; shared AST identity
cannot designate only the first lowered copy safely. Chosen scope is nullable
unbounded groups with explicit scalar-owner barriers and conservative copy cost.
Required-body groups, mixed mandatory/nullable alternatives and unproven earlier
copy owners remain separate. No new VM cache, frame, opcode or owner placement.

Old successful selectors, shared nullable-atom/final LF-noCR set/capture checks,
actual-return/completed-proof flags, cancellation, failed-proof reset and fallback
PLUS Skip=false remain. No parser/emitter/matcher/Step/journal/snapshot/span/
pointer/runtime layout changes; existing loop progress and resource guards stay.

## Constraints and verification

All20489 baseline expectations/frozen35 unchanged. C#13/net8/9/10, UTF8 LF,
root project/solution,src/tests/samples; no subagents/Python/SDK installation.
Parser256/captures256/instructions4096/marks256/frames1024/work1m/proof1000/window12.
No public API/limit/workflow/package/dependency/consumer/CommandFramework changes.
Publication/all known migrations precede CF removal; Grep last.

Native full captures for star/plus/count/modifier/source/group/depth/nesting/
sequential/alternative/dependency cases; primitive-owner versus group-loop and
guarded versus unproven required-copy controls. Six newline/UCP/every-offset
strict/tolerant fragments; work exact/exhausted/observer/cancellation/default
budget/parser/capture/backtrack/copy/expansion rejection. Large-copy controls use
ordinary input rather than native bitmap parity beyond proof1000.
Tests-only draft -> successful-build new-class RED/all20489baseline pass ->
source/tests immutable -> six native Staging/net8/9/10 full/frozen214/configured
gates/7equal workloads,0failed/skipped tests/compiler warnings/errors -> author
review/exacthead/tree/blob/frozen/clean audit -> ready PR unmerged for user review.
Keep102/unsafe100 and public/release/Grep gates open.
