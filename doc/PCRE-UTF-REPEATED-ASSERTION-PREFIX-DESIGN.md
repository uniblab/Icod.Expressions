# Repeated skipped assertions in UTF prefix proofs

## Intent and approach
Continue #7/#102 after merged #157. Native bitmap study skips the bodies of
negative ahead/all behind assertions,including their required exact copies.
Their opcodes stop early-fail descent,so later PLUS/star recovery still
cannot own a type_skip marker. Existing parser normalization gives unbounded
assertion repeats a finite Maximum=Minimum+1; emission/capture effects remain.

Extract #157's exact assertion kind list to IsUtfCrLfSkippedAssertion(node)->bool.
Keep direct assertions cost2/retained=true. Accept direct repeats of that list
only when Minimum=Maximum>0 and Maximum<=(MaximumStartClassVisits-1)/2; charge
2*Maximum+1 visits and retained=true. Test the bound before multiplying,then use
the existing pre-addition budget check and atomic-wrapper charge. This budgets
every required copy conservatively without traversal/expansion or overflow.
Zero-count behavior remains in its original classifier. Positive ahead and
arbitrary nullable-group repeat bodies are not added. Root-star identity,
selected PLUS state,source/capture/alternative guards and runtime stay intact.

Native investigation narrowed the initial optional/ranged proposal: BRAZERO can
enter an assertion body directly,exposing properties/reverse opcodes and CR bits.
Native optional property-ahead and ranged property-behind controls miss at LF.
These are different from skipped required copies and remain excluded.
Considered cloning lists or admitting any repeated assertion. Shared exact
classification prevents drift; exact counts avoid the optional-body rule;
positive ahead has a different bitmap rule. Chosen proof is an implementer decision under the standing
instruction to continue autonomously. Existing C#13/compiler architecture remains.

## Constraints and verification
Root project/solution,src/tests/samples,UTF8 LF;net8/9/10. No subagents/Python/
SDK installation. Parser256/captures256/instructions4096/marks256/frames1024/
work1m/proof1000/window12 unchanged. Preserve all24621baseline expectations and
frozen35. No parser/emitter helper/owner finder/matcher/Step/VM/frame/journal/
API/limit/workflow/package/dependency/consumer/CommandFramework changes.

Native-first full captures for literal/general negative ahead,positive/negative/
non-atomic/variable behind,required exact counts and all
modifiers,capture side effects/dependencies,opaque predicates,mixed prefixes,
wrappers/alternatives and optional/ranged/unbounded/positive-ahead exclusions.
Later PLUS modifiers and earlier-vs-later
stars pin ownership. Six-newline/UCP/every-offset tolerant fragments,strict
validation/caller boundaries,exact/exhausted/observed work,cancellation/default
budget,long ordinary multi-prefix/counted controls and an in-budget boundary
fixture. No bitmap parity claim beyond the conservative1000 total budget.
Successful-build new-class RED/all24621baseline pass before source; tests then
immutable. Six native Staging/full/frozen214/net8/9/10,zero failures/skips/compiler
warnings/errors,configured gates/seven equal workloads. Author review/exacttree/
nineblobs/frozen/clean before ready unmerged PR. Append one recent row and update
only issue102 footer after CI; earlier history/index block preserved. Keep102/
unsafe100/release/Grep gates open. Publication/all known migrations before CF
removal; Grep last.
