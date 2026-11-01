# Internal UTF capture-wrapper repeat recovery

## Intent and evidence

Continue #7/#102 after merged #128. Native detect_early_fail descends ordinary
OP_CBRA as well as OP_BRA through depth4, provided the capture is optimized.
Positive predicate-free class PLUS reached through unreferenced capturing groups
needs existing repeat-endpoint recovery. Beyond depth4, bitmap recognition still
permits LF entry while the repeat-skip marker must remain false.

Pinned PCRE.NET1.6.0/PCRE2 10.48 JIT, UTF ANYCRLF, S=[\x09-\x0c\x{100}]:
(S+)A on CRLF+A matches1:2/group1=1:1; on U0100+CRLF+A it misses; on
VT+CRLF+VT+A it selects VT3:2/group1=3:1. Five mixed/capturing wrappers select
LF2:3/group spans2:2 on the last subject. Captures and noncaptures share depth.
check_opcode_types clears optimized_cbrackets for backreferences, capture tests,
possessive captured brackets and selected returned captures; these contexts need
a separate contract. Current selector follows only noncaptures, causing wide
positive-entry misses and ASCII false-positive LF winners with capture wrappers.

## Approaches and implementer choice

Under standing autonomy, extend the existing compiler selector to ordinary
capturing groups only when a bounded whole-root analysis proves absence of capture
dependencies. Reuse existing Set/Skip classification, AST identity and marker.
Native capture-dependency analysis would cover more patterns but needs source
optimization/pruning and subroutine-return provenance; defer it in102. A separate
matcher recovery path would duplicate existing endpoint logic.

## Compiler contract

In src/Pcre/PcreByteProgram.cs, FindUtfCrLfLeadingRepeat may follow either
PcreByteGroup.Body or PcreByteNoncapturingGroup.Body with the same single/nonempty
checks. Both increment groupDepth; terminal class PLUS guards and Skip<=4 remain.
If a capture is encountered, require CanUseUtfCaptureRepeat(root) before admission.

The private bool helper iteratively examines all root branches/nodes, charging a
conservative1000 combined branch/node visits and bounding queued nodes. Follow
group/noncapture/repeat/atomic/lookahead/lookbehind bodies. Accept known leaf
atoms/anchors/boundaries/assertions/failure. Return false for backreferences, calls,
all conditional families and unknown nodes, including dependencies in zero-count
children or DEFINE. Conservative exclusion retains baseline behavior and avoids
emulating native bytecode pruning. Check cancellation per visit and branch.
Run the helper only if the selected chain includes captures. Unknown/dependency/
over-bound forms cannot disable existing noncapture-only admission.

No AST/parser/source metadata/membership/UTF validation/matcher/program/instruction
layout/Step/capture/frame/journal or runtime saved-state change. Compiler worklist
lives on heap; no saved Span/pointer/snapshot, recursion or stackalloc. No raised
parser256/captures256/instructions4096/marks256/frames1024/defaultwork1,000,000 or
start-proof1000 ceiling. All6187 baseline expectations and frozen35 unchanged.

## Verification and remaining gates

Native-first full captures for ASCII/wide, named aliases, mixed depth1/4/5/34/200,
greedy/lazy/possessive/count, head/whole-expression/scoped-case/suffix captures.
Six newlines/UCP/every caller offset, strict/tolerant fragments, no-LF/CR/G/empty
and dependency controls, observed/exact/exhausted work/cancellation, default budget
and parser depth cap. Reference-only native; avoid unsafe malformed Xuc probes100.
Tests-only draft -> observed successful-build RED with6187 baseline passing ->
compiler change -> six native Staging OS/arch/net8/9/10 full/frozen214 and configured
gates green -> whole-branch author self-review/exact head-tree-log/frozen/clean/
whitespace/test-afterRED audit -> ready PR; user merge.

C#13/net8/net9/net10, UTF8 LF, root project/solution and src/tests/samples layout.
No subagents/Python/SDK installation, public/package/version/dependency/workflow/
release/consumer/fallback/engine-clone changes. Capture-dependency/alternative/
quantified/atomic/optional/negated/property/exact broader102 remains; byte99 and
unsafe100 separate. Grep last; CommandFramework removal only after publication
and all known consumer migrations. No full PCRE readiness claim.
