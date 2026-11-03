# UTF mandatory-repeat entry through atomic groups

## Intent and evidence

Continue #7/#102 after merged #144. Native set_start_bits recurses through OP_ONCE
regardless of a preceding star; detect_early_fail does not descend that ancestor.
The private mandatory-repeat selector still requires proven-star identity before
atomic descent, so bare atomic wide-class PLUS cannot supply its start-class proof.
Initial88 bounded native probes (positive JIT/bounded captures) confirm
(?>[\x09-\x0c\x{100}]+)A enters CRLF+A at LF1:2 and U0100+CRLF+A at LF3:2,
without consumed-repeat cache at any atomic depth. Captures, aliases, possessive
PLUS, optional B? inside/outside and mixed ancestors retain this behavior.
Pinned PCRE.NET1.6.0/PCRE2 10.48 JIT; native.so SHA256
7753b78ef5e7ed18a885392917fa789c9e8f801058ad005b73b9028fedac8395.

The same probes confirm nullable prefix-only group rejoin is still a gap:
(?:B*)(S+)A and capturing/atomic variants require continuing in the enclosing
sequence after the nullable group ends. That larger traversal refactor stays
separate; it is not approximated by discarding group contents.

## Approach (implementer decision under standing autonomy)

Remove only the proven-star-identity condition on the existing
Atomic(NoncapturingGroup) mandatory-set descent. Existing atomic traversal
already sets its local Skip=false; keep that behavior. The final proof still
requires single nonempty branches and a mandatory unbounded positive ScalarSet
with LF/no CR/no predicates, plus existing capture/depth/cancellation/1000 and
optional-source/range/casing/UCP/BlocksSkip guards. Possessive Atomic(Repeat)
is still unwrapped only at the leaf; non-atomic/root-plus cache eligibility and
first-star discovery/identity/Skip remain unchanged.
This reuses the same mandatory-set proof and matcher, rather than a separate
atomic optimizer. Retaining the old guard leaves demonstrated bare atomic
entry differences. A generic nullable-group/continuation walker changes a
larger surface and is a separate tranche after this one.

## Scope and constraints

One private selector condition/comment plus four canonical inventories,
design/plan and one reference test class. No matcher/parser/emitter/Step/runtime
state/layout/instruction/frame/journal/span/pointer/snapshot/API/limit change.
No workflow/package/version/dependency/consumer/CommandFramework change.
Parser256/captures256/instructions4096/marks256/frames1024/work1m/proof1000/window12.
C#13/net8/9/10, UTF8 LF, root project/solution,src/tests/samples.
Local Debug/PR Staging; six native OS/arch/net8/9/10. No subagents/Python/SDK install.
All13928 baseline expectations/frozen35 unchanged; native is reference-only.

## Verification and boundaries

Pin full strict/tolerant native captures for bare/captured PLUS, greedy/lazy/
possessive/alias modifiers, depths1/3/4/5/6/33/199, atomic aliases/mixed ancestors,
optional/erased/retained-zero prefixes, related/named/duplicate capture references/
tests, source/casing/UCP/CR/predicate controls and outside-root cache controls.
Six newline/UCP/every-offset and strict/tolerant fragment/start validation;
exact/exhausted/observed work/cancellation/default budget/parser/capture/depth.
Nullable prefix-only wrapper controls pin ordinary consumption only.
Tests-only draft -> successful-build new-class RED/all13928baseline pass ->
minimal source, tests/expectations unchanged -> six native Staging/net8/9/10
full/frozen214/configured gates/7equal workloads,0failed/skipped tests/build
warnings/errors -> wholebranch author review/strict logs/tree/blob/tests-afterRED/
frozen/clean/whitespace audit -> issue102 open/PR ready/unmerged, user review/merge.
Other placement and bitmap availability (predicates/groups/alternatives/negated/
property/call/DEFINE/conditions), byte99 and unsafe100 remain separate.
No complete PCRE/release/Grep readiness claim; Grep last; publication and all
known consumer migrations precede any CommandFramework code removal.
