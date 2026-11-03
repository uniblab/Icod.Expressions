# UTF word-boundary prefixes in bounded bitmap proofs

## Intent and evidence
Continue #7/#102 after merged #151. The native study skips all four ASCII/UCP
word-boundary opcodes; detect_early_fail also skips them without changing its
retained count. Current FindUtfCrLfLeadingNode and FindUtfCrLfRootStarAtom stop
on PcreByteWordBoundary, hiding an otherwise supported mandatory PLUS or first
scalar star. Initial80 bounded native JIT probes pin these distinctions.

## Approach
Extend the existing shared leading-node walker and first-star identity finder
to cross PcreByteWordBoundary at a cost of one proof visit, without changing
BlocksSkip, group depth or capture policy. Existing direct repeat selection and
nullable-group continuation then reuse that behavior. Boundaries remain runtime
assertions with original ASCII/UCP semantics and work/cancellation accounting.
Do not treat a boundary as a retained query or as a new recovery owner.

Considered a general zero-width-assertion analyzer and adding boundary handling
to all starter-class paths. Both broaden the admission surface unnecessarily.
Chosen scope: word boundaries along already-supported single-chain mandatory
positive predicate-free LF/no-CR PLUS proofs and nullable-group continuation.
Unselected direct classes, arbitrary assertions, root alternatives and other
broader bitmap availability remain #102. This choice is the implementer's
decision under the standing instruction to continue autonomously.

## Invariants and verification
No parser/emitter/matcher/Step/VM state/journal/API/limit/workflow/dependency/
package/consumer/CommandFramework changes. C#13/net8/9/10, UTF8 LF; project and
solution in root, source/src, tests/tests, samples/samples. No subagents/Python/
SDK installation. Parser256/captures256/instructions4096/marks256/frames1024/
work1m/proof1000/window12 unchanged. All21932 baseline expectations/frozen35 stay.

Pin literal native captures before implementation for ASCII/UCP/restrictions,
positive/negative/repeated boundaries, group/depth/capture dependencies, primitive
star versus group-loop ownership, queries/erased/finite/unbounded nullable bodies,
LF/ANYCRLF and exclusion cases. Every caller offset in six newline modes and
malformed tolerant fragments; strict validation, work exact/exhausted/observed,
pre/mid cancellation, default budget and bounded-analysis controls.
Tests-only draft must establish successful-build new-class RED with all21932
baseline pass. Keep tests immutable thereafter. Require six native Staging jobs,
full/frozen214 ×net8/9/10, all configured gates/seven equal component workloads,
zero failed/skipped tests/compiler warnings/errors, exact head/tree/blob audit
and author self-review. Leave ready PR unmerged for user review; #102 stays open.
Publication/all known consumer migrations precede CF removal; Grep last.
