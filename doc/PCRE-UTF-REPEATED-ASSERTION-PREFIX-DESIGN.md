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

## Correction for four original optional capture cases
Run504's exact-count fix resolved349 of353 original failures. Four immutable
cases remained: (?!([B-C])){0,3} before the mandatory class/capture condition.
They were outside the narrowed exact-only plan but remain required tests. All
24621baseline+438new controls passed on Linux x64/net8;0compiler warnings/errors.

Add a separate focused test class before the corrective source change. Extend
only non-exact repeats of negative general lookahead whose body has a proven
class bitmap: every branch's first node must lead through ordinary/capturing/
atomic groups to a mandatory positive,predicate-free scalar set admitted by the
existing IsUtfCrLfOptionalSet range/source/CR guards. Reject empty branches,
other nodes,properties,negated sets and CR-containing heads. Since every body
branch reaches a mandatory set,native study stops there; no group return or
body-tail continuation is inferred. Literals/types/nullable bodies and optional
lookbehind/positive ahead remain excluded from this new path.

Use one compile-time worklist of (Node?,Body?) entries. Charge each pop and
branch before enqueue,check cancellation throughout,and share ref visits with
the outer proof. No recursion or ref capture in local functions. Check normalized
Maximum<=(1000-1)/2 before body study; successful proof uses2*Maximum+1/
retained=true plus existing wrapper/pre-addition charging. The exact path still
skips bodies. No parser/emitter/owner/matcher/VM state changes. General optional
assertions do more work per candidate; their separate default-budget fixture
uses10000 CRLF pairs under the unchanged1m ceiling. Original442 tests unchanged.
