# UTF retained zero-count prefixes before starter classes

## Intent and approach
Continue #7/#102 after merged #154. Native set_start_bits skips OP_SKIPZERO's
bracket body without inspecting its starters. The existing PLUS/nullable walker
recognizes a bounded list of retained zero-count groups/assertions/conditions;
the general starter-class proof does not yet cross them.

Extract that exact list into IsUtfCrLfRetainedZeroRepeat(PcreByteNode)->bool.
The PLUS walker keeps retained=true and its existing cost2/cost3 atomic-child
distinction plus any outer wrapper charge. The general prefix scan accepts
that same classification alongside erased primitives and boundaries. Charge
an inner atomic group once in addition to the repeat and any outer modifier.
Do not inspect the skipped body or broaden zero-call/other-condition kinds.
Budget-check before addition under1000 and cancel every accepted prefix.

Considered cloning the list or using all nullable nodes in the general proof.
Cloning risks divergent admission; general nullability is not native bitmap
availability. Chosen shared classifier preserves ownership state in the PLUS
walker and leaves owner selection/marker placement unchanged. Implementer
decision under the standing instruction to continue autonomously.

## Constraints and verification
C#13/net8/9/10,UTF8 LF; root project/solution,src/tests/samples. No subagents/Python/
SDK installation. Parser256/captures256/instructions4096/marks256/frames1024/
work1m/proof1000/window12 unchanged. All23724baseline expectations/frozen35 stay.
No parser/emitter/matcher/Step/VM state/journal/API/limit/workflow/package/
dependency/consumer/CommandFramework changes.

Native-first full captures for group/atomic/literal-general lookahead/lookbehind/
capture-condition zero prefixes, modifiers, opaque property/reference/call bodies,
capture dependencies, mixed boundary/erased prefixes, wrappers/repeats/alternatives
and exclusions. Existing PLUS/primitive-owner controls must retain their recovery
semantics. Six newline/UCP/every-offset tolerant fragments, strict validation,
shared work/cancellation/default budget and long ordinary prefix controls;
no bitmap parity claim beyond1000.
Observe successful-build new-class RED/all23724baseline pass before source;
tests immutable afterward. Six native Staging/full/frozen214/net8/9/10,
configured gates/seven equal workloads,0failures/skips/compiler warnings/errors;
author review/exacttree/nineblobs/frozen/clean audit before ready unmerged PR.
Update the versioned recent index and only issue102's recent footer; earlier
body and preserved index block stay intact. Keep102/unsafe100 and release/Grep
gates open. Publication/all known migrations before CF removal; Grep last.
