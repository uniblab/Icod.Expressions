# UTF nullable-prefix group continuation

## Intent and evidence

Continue #7/#102 after merged #145. Native set_start_bits returns SSB_CONTINUE
when a group has no mandatory operand and resumes after KET in the enclosing
sequence. Current mandatory-repeat discovery descends but cannot resume; current
start-class traversal similarly ends in that nullable body. Initial164 bounded,
JITpositive/spansbounded native probes pin noncapturing/capturing/atomic prefix
groups at depths1/4/5/33, literal/wide stars, capture references and joined
queries/groups. (?:B*)(S+)A, S=[\x09-\x0c\x{100}], enters CRLF+A at LF1:2;
B+CRLF+A misses through four optimized star ancestors, enters LF2:2 at depth5+.
Related prefix capture refs and atomic ancestors disable star recovery. Suffix
capture refs do not. Empty prefix captures must remain successful zero-length
captures. No VM group instruction is discarded.
Pinned PCRE.NET1.6.0/PCRE2 10.48 JIT; native.so SHA256
7753b78ef5e7ed18a885392917fa789c9e8f801058ad005b73b9028fedac8395.

## Approach (implementer decision under standing autonomy)

Keep existing successful mandatory-repeat discovery unchanged. Add a fallback
only when that proof fails and existing first-star identity has been proven.
The fallback uses a bounded iterative stack of AST branch references and next
indexes (compile-time only, no VM frames/spans/pointers). It descends only
nonempty single-branch capturing/noncapturing/explicit atomic groups, preserving
their continuations. Pop/resume only when every operand in a branch was accepted
as nullable by the existing optional-atom classifier. Require at least one
actual nullable-group return before accepting a mandatory set.
Carry BlocksSkip across resumed branches so later optional atoms share the
existing source/casing/UCP/CR/predicate/range/retained rules. First-star discovery
and its depth/ancestor-capture/atomic recovery eligibility remain unchanged;
the fallback never assigns a later PLUS cache.
This avoids repeated speculative nullability scans and avoids broad changes to
already passing proof paths. Dropping group contents would accept required
operands; a recursive/general nullable optimizer would expand unrelated shapes.

Extend FindUtfCrLfLeadingNode with optional startIndex/blocksSkip inputs and
NextIndex/Nullable outputs. Existing callers keep defaults and Node/BlocksSkip.
Full consumption returns Nullable=true with retained state; budget failure
returns Nullable=false/Node=null. Stop on every unsupported/required operand.
Share final mandatory-set validation between old and fallback selectors:
mandatory unbounded positive ScalarSet, nonempty ranges, valid scalar bounds/
ordered ranges, LF present, CR absent, no predicates, cancellation checks.
Collect traversed captures and reuse existing classification; always require
Eligible, including an empty capture set, to reject calls/unsupported pruning.
Root/group/optional existing costs and shared1000 visit ceiling remain.
No quadratic re-probing: each fallback continuation advances through its branch.

A compile-only joined-proof flag records successful fallback selection. Only
then may HasUtfCrLfStartClass directly consume that complete validated LF/no-CR
proof. The generic existing start-class traversal remains for all other paths.
Failed final proof still clears first-star identity/Skip. No runtime metadata,
VM state/layout/instruction markers or Step changes; existing start-class bool
and recovery markers are reused.

## Constraints and verification

All14898 baseline expectations/frozen35 unchanged. C#13/net8/9/10, UTF8 LF;
root project/solution,src/tests/samples. No subagents/Python/SDK installation.
Parser256/captures256/instructions4096/marks256/frames1024/work1m/proof1000/window12.
No matcher/parser/emitter/frame/journal/snapshot/span/pointer/public API/limit/
workflow/package/version/dependency/consumer/CommandFramework changes.

Pin full native captures for prefix group types/depths1/3/4/5/6/33/199, star
source/modifiers/UCP/casing, erased operands, related/unrelated/named/duplicate
refs/tests, nested/sequential returns, later optional groups/queries, inside/
outside required sets, and unsupported controls. Required CR, CR-admitting
optional atoms, alternatives, required literals/assertions, predicates/negated
classes and no-LF final sets must not be accepted as nullable CR-excluding paths.
Unproven first-star placements remain ordinary-consumption controls only.
Six newline/UCP/every-offset/strict-tolerant fragments; work exact/exhausted/
observed/cancellation/default budget/parser/capture/backtrack rejection.
Tests-only draft -> successful-build new-class RED/all14898baseline pass -> source
with tests immutable -> six native Staging/net8/9/10 full/frozen214/configured
gates/7equal workloads,0failed/skipped tests/buildwarnings/errors -> wholebranch
author review/strict logs/tree/blob/testsafterRED/frozen/clean audit -> issue102
open/PR ready/unmerged; user review/merge. Broader bitmap availability and byte99/
unsafe100 remain separate; no complete PCRE/release/Grep-readiness claim.
Publication/all known migrations precede CF removal; Grep last.
