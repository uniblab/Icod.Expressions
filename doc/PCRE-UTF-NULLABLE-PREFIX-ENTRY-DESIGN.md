# UTF finite and empty nullable-prefix group continuation

## Intent and evidence

Continue #7/#102 after merged #146. Its bounded continuation proof requires
an independently proven first star and nonempty group bodies. Native bitmap
study also resumes after finite optional, omitted-only and empty groups.
Initial124 plus24 additional PCRE.NET1.6.0/PCRE2 10.48 JIT probes pin group
types/depths1/4/5, B?, B{0,3}, X{0}, reference0 and empty aliases. With
S=[\x09-\x0c\x{100}], (?:B?)(S+)A enters CRLF+A at LF1:2 and wide+CRLF+A
at LF3:2, including after the mandatory PLUS consumed a wide prefix. Empty
participating captures remain successful zero-length captures. A later star
after an empty/omitted group also has no owned recovery cache.
Native.so SHA2567753b78ef5e7ed18a885392917fa789c9e8f801058ad005b73b9028fedac8395.
Source: pcre-net1.6.0 commit09639d45d4c03a8269fa2cde6edc7a7383bbc18c,
pcre2_study.c set_start_bits group SSB_CONTINUE/KET continuation and
pcre2_jit_compile.c detect_early_fail group return/count and depth4 rules.

## Approach (implementer decision under standing autonomy)

Keep the old successful selector and existing first-star finder unchanged.
After an old-proof miss, run the existing continuation fallback in UTF mode
without requiring a first-star identity. Permit empty single-branch bodies in
that fallback. Mark a completed group return as retained BlocksSkip=true:
native group traversal raises the early-fail count even if its body contains
no retained iterator. This allows later supported stars to participate in the
bitmap proof, while assigning no first-star/PLUS recovery ownership.
Existing nullable-atom source/casing/UCP/CR/predicate/range checks, shared final
mandatory positive unbounded LF/no-CR set validation, capture eligibility,
single-branch guard, actual group-return requirement, cancellation/1000-visit
budget and completed-proof start-class flag remain unchanged. Empty-body
acceptance applies only to the fallback, preserving old successful paths.

No parser/emitter/matcher/Step/VM state or lifetime changes. The compile-time
continuation stack stores branch references/indexes only; runtime frames remain
integers/enums. No new first-star identity, recovery cache, repeated scans,
unbounded recursion or execution of unsupported group shapes.
Required operands, alternatives, CR-admitting/predicate optional atoms, calls,
unsupported pruning and exhausted proofs continue to stop this proof. Actual
quantified groups remain unsupported except previously supported zero forms.

## Constraints and verification

All16306 baseline expectations/frozen35 unchanged. C#13/net8/9/10, UTF8 LF,
root project/solution,src/tests/samples; no subagents/Python/SDK installation.
Parser256/captures256/instructions4096/marks256/frames1024/work1m/proof1000/window12.
No public API/limit/workflow/package/version/dependency/consumer/CommandFramework
changes. Publication/all known migrations precede CF removal; Grep last.

Native-pinned full capture fixtures for finite bounds/modifiers/source/types,
empty aliases, erased-only bodies, group types/depths/nesting/sequential returns,
later stars and capture dependencies. Controls retain first-star ownership and
required/unsupported/final-set exclusions. Six newline modes/UCP/every caller
offset, strict/tolerant fragments, exact/exhausted/observed work, cancellation,
default budget and parser/capture/backtrack rejection are required.
Tests-only draft -> successful-build new-class RED/all16306baseline pass ->
minimal production change/tests immutable -> six native Staging/net8/9/10
full/frozen214/configured gates/7equal workloads,0failed/skipped tests/compiler
warnings/errors -> wholebranch author review/exacthead/tree/blob/frozen/clean
audit -> issue102 open/PR ready/unmerged. Broader bitmap availability and unsafe
native UTF boundary issue100 remain separate; no PCRE/release/Grep closure claim.
