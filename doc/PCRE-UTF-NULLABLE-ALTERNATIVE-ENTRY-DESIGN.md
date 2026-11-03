# UTF continuation after nullable alternatives

## Intent and evidence

Continue #7/#102 after merged #147. Its bounded continuation walker requires
single-branch prefix groups. Native set_start_bits checks every alternative;
when all branches are nullable it resumes after the group's KET. Native
detect_early_fail raises the start count for alternatives, so their first
unbounded star has no independent type_skip ownership. Initial204 bounded JIT
probes cover capturing/noncapturing/atomic groups, empty/erased/finite/star
branches, depths1/4/5 and later stars. (?:B*|C?)(S+)A, with
S=[\x09-\x0c\x{100}], enters CRLF+A at LF1:2 and B+CRLF+A at LF2:2.
A separately proven root B* before that group still owns recovery and can miss
after consuming B. Five additional guard probes show why accepting an early
mandatory set inside one branch would be wrong: a later unsupported property
branch can disable native bitmap study for the entire group.
Reference: PCRE.NET1.6.0/PCRE2 10.48 JIT, source commit
09639d45d4c03a8269fa2cde6edc7a7383bbc18c, pcre2_study.c set_start_bits and
pcre2_jit_compile.c detect_early_fail. Native.so SHA256
7753b78ef5e7ed18a885392917fa789c9e8f801058ad005b73b9028fedac8395.

## Approach (implementer decision under standing autonomy)

Extend the existing fallback's compile-time continuation frames with group Body
and NextBranch. Fully validate each branch with the same nullable-atom classifier
before returning to the parent sequence. An alternative group sets retained
BlocksSkip before its first branch; this permits supported branch stars without
inventing a first-star owner. Additional branches consume one proof visit each.
A local count of pending alternative groups prevents accepting a mandatory set
inside a partly validated alternative, including after a nested empty group has
returned. Only after all branches are proven nullable does the outer continuation
resume. Empty branches are valid; zero branches fail. Captures across all checked
branches feed the existing eligibility classifier. Cancellation/1000-visit limits
remain, and work is linear in visited operands/branches.

Considered a separate general nullability/bitmap analyzer; it would duplicate
existing source/casing/UCP/CR/predicate checks and need a second traversal. The
existing walker already provides the required bounded state and continuation.
Required alternatives, mixed mandatory/nullable alternatives and quantified
groups remain outside this extension. They stop the proof even if some native
shapes could be proven separately. No general alternative optimizer is added.

Keep the old successful selector, first-star finder/ownership/depth/capture/
atomic rules, mandatory positive unbounded LF/no-CR set validator, actual group
return requirement, completed-proof flag and fallback PLUS Skip=false unchanged.
Root alternatives and unsupported/predicate/CR-admitting/required/call/pruning
paths still stop. Runtime instructions, captures and matching are untouched.

## Constraints and verification

All17256 baseline expectations/frozen35 unchanged. C#13/net8/9/10, UTF8 LF,
root project/solution,src/tests/samples; no subagents/Python/SDK installation.
Parser256/captures256/instructions4096/marks256/frames1024/work1m/proof1000/window12.
No parser/emitter/matcher/Step/VM state/layout/instruction/frame/journal/snapshot/
span/pointer/API/limit/workflow/package/dependency/consumer/CommandFramework changes.
Publication/all known migrations precede CF removal; Grep last.

Native-pinned literal full captures and six-newline/UCP/every-offset strict/
tolerant fragments cover branch order, empty branches, types/source/modifiers,
nested/sequential alternatives, capture numbering/branch reset/dependencies,
later stars and prior first-star controls. Late unsupported/CR-admitting branches
and nested early mandatory-set traps must stop; required/quantified alternatives
have ordinary-consumption controls. Pin work/exact/exhausted/observer parity,
cancellation/default budget/parser/capture/backtrack boundaries.
Tests-only draft -> successful-build new-class RED/all17256baseline pass ->
source change/tests immutable -> six native Staging/net8/9/10 full/frozen214/
configured gates/7equal workloads,0failed/skipped tests/compiler warnings/errors
-> author wholebranch review/exacthead/tree/blob/frozen/clean audit -> ready PR
unmerged for user review. Broader102/unsafe100 and release/Grep gates remain open.
