# UTF continuation after positive finite nullable-group repeats

## Intent and evidence

Continue #7/#102 after merged #149. Its finite-repeat fallback requires Minimum0.
Positive-count groups can also be nullable when every body branch is nullable.
Initial274 bounded PCRE.NET1.6.0/PCRE2 10.48 JIT probes pin exact/ranged positive
counts, query/erased/empty/alternative/star bodies and group types. Recovery must
remain distinct: (?:B*){1,3}(S+)A can miss after consuming B before CRLF, whereas
(?:B*){1,3}+(S+)A and (?>B*){1,3}(S+)A enter LF2:2. An earlier B? or returned
empty group also prevents inner-star ownership; erased X{0} does not. Here
S=[\x09-\x0c\x{100}]. These differences rule out treating every positive repeat
as an optional-group recovery barrier.
Reference source commit09639d45d4c03a8269fa2cde6edc7a7383bbc18c:
pcre2_study.c group/copy continuation and pcre2_jit_compile.c detect_early_fail
(BRA/CBRA recursion, retained counts, ONCE/atomic stop). Native.so SHA256
7753b78ef5e7ed18a885392917fa789c9e8f801058ad005b73b9028fedac8395.

## Approach (implementer decision under standing autonomy)

Reuse #149's finite-repeat walker/copy budget/pending-repeat guard. Admit finite
Maximum>0 group repeats with nonnegative Minimum. Preserve incoming BlocksSkip
for ordinary positive-minimum bodies instead of forcing it true. The existing
nullable-atom classifier then refuses an unbounded branch star unless a prior
retained operand or independently proven owner already permits crossing it.
Alternative groups retain their existing barrier. Minimum0 repeats keep their
old barrier; possessive Atomic(Repeat) and explicit atomic group descent also
block native early-fail descent, permitting their nullable body stars without
inventing an owner. Collect repeated-child captures as before.

Considered assigning first-star identity through repeated groups. The same AST
child is lowered into several copies, so a shared identity could wrongly mark
every copy as an owner. That needs a separate placement proof/emission design.
Chosen extension preserves the existing owner finder and all lowering, and
keeps first unbounded stars without proven ownership outside its scope.

All body branches must still be fully nullable; pending alternatives/repeats
prevent early mandatory-set acceptance. RepeatMaximum/RepeatStartVisits charge
all potential finite copies using division-before-multiplication under1000
visits, including exact counts/empty bodies/nested repeats. Existing successful
selectors, root/first-star/depth/capture policy, set/capture validation, actual
return/completed-proof flags, cancellation and fallback PLUS Skip=false remain.
Required-body/unbounded groups, mixed mandatory/nullable alternatives and broader
bitmap availability remain separate. No general repeated-group cache proof.

## Constraints and verification

All19372 baseline expectations/frozen35 unchanged. C#13/net8/9/10, UTF8 LF,
root project/solution,src/tests/samples; no subagents/Python/SDK installation.
Parser256/captures256/instructions4096/marks256/frames1024/work1m/proof1000/window12.
No parser/emitter/matcher/Step/VM state/layout/instruction/frame/journal/snapshot/
span/pointer/API/limit/workflow/package/dependency/consumer/CommandFramework changes.
Publication/all known migrations precede CF removal; Grep last.

Native-pinned full captures for exact/ranged/modifier/source/group/depth/nesting/
sequential/alternative/dependency cases; unproven-star consumed-run misses and
root-owner/atomic/possessive/retained/erased controls. Six newline/UCP/every-offset
strict/tolerant fragments; work exact/exhausted/observed, cancellation/default
budget/parser/capture/backtrack/copy/expansion rejection. Large-copy controls use
ordinary input, not native bitmap parity beyond proof1000.
Tests-only draft -> successful-build new-class RED/all19372baseline pass ->
source/tests immutable -> six native Staging/net8/9/10 full/frozen214/configured
gates/7equal workloads,0failed/skipped tests/compiler warnings/errors -> author
wholebranch review/exacthead/tree/blob/frozen/clean audit -> ready PR unmerged.
Keep102/unsafe100 and public/release/Grep gates open.
