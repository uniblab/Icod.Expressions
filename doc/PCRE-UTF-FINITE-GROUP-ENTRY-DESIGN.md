# UTF continuation after finite nullable group repeats

## Intent and evidence

Continue #7/#102 after merged #148. Its fallback accepts nullable groups and
alternatives but stops at nonzero group quantifiers. Native bitmap study crosses
BRAZERO/BRAMINZERO and finite copied groups; JIT early-fail study does not give
their inner stars a root recovery owner. Initial270 bounded JIT probes cover
capturing/noncapturing/atomic, empty/erased/query/star/alternative bodies and
greedy/lazy/possessive optional/finite forms. (?:B*)?(S+)A, with
S=[\x09-\x0c\x{100}], enters B+CRLF+A at LF2:2. Captured greedy empties
participate; lazy omitted captures can remain unset. Separate root B* before
the group still owns recovery and can miss after consuming B. Boundary probes
also pin copied nullable queries at33/199/333/500 and unsupported inner properties.
Reference PCRE.NET1.6.0/PCRE2 10.48 JIT; pcre-net source commit
09639d45d4c03a8269fa2cde6edc7a7383bbc18c, pcre2_study.c BRAZERO traversal and
pcre2_jit_compile.c detect_early_fail. Native.so SHA256
7753b78ef5e7ed18a885392917fa789c9e8f801058ad005b73b9028fedac8395.

## Approach (implementer decision under standing autonomy)

Extend only the existing bounded continuation fallback. Recognize Minimum0,
finite Maximum>0 group repeats, including a possessive Atomic(Repeat) modifier;
descend capturing/noncapturing/explicit atomic bodies with the same classifiers.
Set retained BlocksSkip before the body. Require every body branch to be fully
nullable; a pending-repeat count prevents mandatory-set acceptance after nested
returns before the quantified body has finished. Existing alternative guards
continue checking every branch. Capture eligibility remains mandatory.

Continuation tuples gain RepeatMaximum/RepeatStartVisits, both zero for existing
plain groups. Charge one repeat visit plus any modifier, in addition to the
existing group visit. At body completion, perCopy=visits-RepeatStartVisits includes
one group visit and all checked body/nested-copy costs. Charge (Maximum-1)*perCopy
before returning; compare using division before multiplication so overflow and
proof-budget bypass are impossible. No AST copies or repeated speculative scans.
The single traversal plus arithmetic conservatively budgets every potential
native copy, including empty bodies and nested finite repeats. The1000 ceiling
and cancellation remain. Failed/over-budget proofs return no metadata.

Considered query-only support and a general bitmap/nullability analyzer. Finite
bounds share the existing traversal once copy cost is charged; a new analyzer
would duplicate source/casing/UCP/CR/predicate checks. Chosen scope is finite
repeats with fully nullable bodies. Required-body, positive-minimum, unbounded
group repeats and mixed mandatory/nullable alternatives remain separate. Their
possible native availability is not silently assumed here.

Old successful selectors, root/first-star identity/depth/capture/atomic policy,
shared final positive unbounded LF/no-CR set validator, actual return/completed
proof flags, fallback PLUS Skip=false and runtime instructions remain unchanged.
No matcher/parser/emitter/Step/VM/frame/journal/span/pointer/snapshot changes.

## Constraints and verification

All18169 baseline expectations/frozen35 unchanged. C#13/net8/9/10, UTF8 LF,
root project/solution,src/tests/samples; no subagents/Python/SDK installation.
Parser256/captures256/instructions4096/marks256/frames1024/work1m/proof1000/window12.
No public API/limit/workflow/package/dependency/consumer/CommandFramework changes.
Publication/all known migrations precede CF removal; Grep last.

Pin full native captures for group/quantifier/source/modifier/depth/alternative/
nesting/sequential/recovery/dependency cases. A nested early mandatory-set trap
followed by an unsupported property must stop this restricted proof. Include
six newline/UCP/every-offset strict/tolerant fragments, work exact/exhausted/
observed, cancellation/default budget/parser/capture/backtrack/copy ceilings.
Large-copy controls use ordinary consumption, not parity beyond proof1000.
Tests-only draft -> successful-build new-class RED/all18169baseline pass ->
source with immutable tests -> six native Staging/net8/9/10 full/frozen214/
configured gates/7equal workloads,0failed/skipped tests/compiler warnings/errors
-> author wholebranch review/exacthead/tree/blob/frozen/clean audit -> ready PR
unmerged for user review. Broader102/unsafe100/release/Grep gates remain open.
