# Skipped prefixes in optional bracket bitmap study

Continue #7/#102 after merged163 (full28269/frozen214). Complete the selected
mandatory-body proof for explicit atomic outer groups and unquantified word
boundaries/bitmap-skipped assertions before a mandatory body head. Every branch
must still reach a supported literal/type/positive predicate-free range excluding
CR. Runtime capture, assertion, atomic commitment, tail and option semantics stay
with the existing matcher.

## Approach and native basis

Reuse HasUtfCrLfMandatoryBodyBitmap and IsUtfCrLfSkippedAssertion. Pinned native
set_start_bits treats ONCE as a bracket, ignores word boundaries, and skips
negative lookahead and all supported lookbehind headers before continuing. It
studies BRAZERO's bracket and then continues outside it. Capturing/noncapturing/
atomic groups and optional negative lookahead therefore share this proof.

Extend the optional-group body selector with Atomic{NoncapturingGroup}; charge
its wrapper in the local success-only visit checkpoint. Preserve prior capture/
noncapture paths and the exact noncapturing single-leaf fast path.

In the iterative body worklist, scan each branch's initial unquantified word
boundaries and IsUtfCrLfSkippedAssertion nodes, charging1/2 respectively with
cancellation and checked pre-addition. Push the first remaining node; reject a
branch ending after only skipped nodes. Keep wrapper/all-branch/mandatory-leaf
logic unchanged. Do not call FindUtfCrLfLeadingNode from this walker: that would
admit unrelated repeat/nullable prefixes and introduce recursive proof cycles.
A second bracket walker would duplicate the existing proof. These are agent
implementation choices under standing authorization to continue without pauses.

No optional/exact prefix-repeat, positive-ahead, erased-prefix, nullable group
return or empty-branch admission is added. The caller retains Minimum0/Maximum
1..499 before study/multiply,2*Maximum+1/retained=true, wrapper/pre-addition guards,
and existing root-star/PLUS/capture-ancestor barriers. Existing failed group
proofs retain the success-only checkpoint, preserving fallback visits.

## Constraints and verification

C#13/net8/9/10; root project/solution,src/tests/samples,doc; UTF8+LF. No subagents,
Python or SDK/PowerShell installation. No parser/emitter/owner/matcher/Step/VM/
frame/journal/API/workflow/contract/dependency/consumer/CommandFramework changes
or production reference fallback. No spans/interior pointers in VM frames.
Limits stay parser256/captures256/instructions4096/marks256/frames1024/work1000000/
proof1000/window12; preserve all28269 baseline tests and35 frozen source blobs.

Native first: literal full capture spans across all bracket kinds, modifiers,
negative/positive behind (including captures before match start), word-boundary/
UCP/ASCII modes, ignored predicate bodies, nested alternatives, rollback, atomic
tail commitment, captures/references/conditions and PLUS/star owners. Include
six-newline/UCP/every-offset malformed fragments, strict validation, cancellation,
work/default budget/count/expansion controls. Counted ordinary controls use native
interpretation in CI for159; bitmap-sensitive fixtures retain JIT. Unsupported
availability remains ordinary-consumption-only.

Publish tests/design/plan first, observe successful-build RED confined to the new
class with all28269 baseline cases passing, then freeze its blob and implement.
Final exact-head six native Staging OS/arch jobs/net8/9/10 must pass full/frozen214/
120method identities, zero failures/skips/compiler warnings/errors and every
configured gate, including seven equal Linux workloads. Author whole-branch
review, exact remote tree/blob checks and historical inventories/index preservation
precede ready/unmerged PR. Only issue102 recent footer changes after success.
Broader7/99/100/102/native159 and release/Grep gates stay open; publication and
all known migrations precede CommandFramework removal; Grep remains last.
