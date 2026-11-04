# UTF boundary-prefixed starter classes

## Intent and approach
Continue #7/#102 after merged #152. Native set_start_bits skips ASCII/UCP word
boundaries before direct classes as well as PLUS. The current general class
proof uses branch[0] whenever the single-chain PLUS selector found no set, so
a leading boundary prevents otherwise supported direct/group/alternative paths.

Extend only HasUtfCrLfStartClass's local AddBranches: with no selected repeat
set, skip leading PcreByteWordBoundary nodes, charging one bounded visit and
checking cancellation per node, then submit the first significant node to the
existing worklist. A branch containing only boundaries still rejects. Keep
the selected-repeat path's existing shared nullable-prefix walker.

Considered always using the shared nullable walker and a generalized zero-width
analyzer. Both admit additional optional/assertion source shapes without a
native ownership proof. Chosen approach leaves those gates unchanged and reuses
the current class, required-repeat, wrapper, all-alternative CR-exclusion and
at-least-one-LF checks. Wide unselected PLUS, exact larger repeats, predicates/
negated classes and arbitrary prefixes retain their existing exclusions.
Decision made by implementer under standing autonomous-development authorization.

## Constraints and verification
No recovery-owner selection/marker placement, parser/emitter/matcher/Step/VM
state/journal/API/limit/workflow/package/dependency/consumer/CommandFramework
changes. C#13/net8/9/10, UTF8 LF; root project/solution,src/tests/samples.
Parser256/captures256/instructions4096/marks256/frames1024/work1m/proof1000/window12.
No subagents/Python/SDK installation. All23346 baseline expectations/frozen35 stay.

Native-first literal capture fixtures cover ASCII/wide classes, boundaries/
ASCII-UCP restrictions, wrappers/depths, qualified alternatives/branch resets,
required variable/exact1 repeats, related captures/references/conditions, casing
and exclusion controls. Six newline/UCP/every-offset tolerant fragments; strict
validation, exact/exhausted/observed work, cancellation/default budget and long
boundary prefixes on ordinary input. Long-prefix controls claim no bitmap parity
beyond1000. Observe tests-only successful-build new-class RED with all23346baseline
pass; tests immutable thereafter. Six native Staging jobs/full/frozen214/net8/9/10,
all configured gates/seven equal workloads,0failures/skips/compiler warnings/errors,
author self-review/exacthead/tree/blob/frozen/clean audit before ready unmerged PR.
#102/unsafe100 stay open; no full PCRE/release/Grep readiness claim.
Publication/all known migrations precede CF removal; Grep last.
