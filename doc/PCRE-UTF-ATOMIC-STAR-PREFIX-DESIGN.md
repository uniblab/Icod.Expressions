# UTF star prefixes inside explicit atomic ancestors

## Intent and native evidence

Continue #7/#102 after merged #143. First-star discovery currently rejects
Atomic(NoncapturingGroup), and mandatory-set discovery traverses that shape only
after !skip/proven-star identity. Native set_start_bits recurses through OP_ONCE;
detect_early_fail recurses only BRA/optimized CBRA, not an OP_ONCE ancestor.
Initial70 bounded native probes at depths1/4/5/33/199 and mixed atomic/capturing/
noncapturing ancestors confirm LF entry with no consumed-star cache at all depths.
For S=[\x09-\x0c\x{100}], (?>B*(S+))A enters CRLF+A1:2, B+CRLF+A2:2,
U0100+CRLF+A3:2; a wide-set star consuming U0100 still enters LF3:2.
Atomic groups following an outside root star must preserve that earlier cache.
Reference PCRE.NET1.6.0/PCRE2 10.48 JIT/native.so SHA256
7753b78ef5e7ed18a885392917fa789c9e8f801058ad005b73b9028fedac8395.

## Approach (implementer decision under standing autonomy)

Extend FindUtfCrLfRootStarAtom's bounded iterative discovery through explicit
Atomic(NoncapturingGroup), retaining the supported star identity but disabling
Skip after any atomic ancestor. Root1/group1/erased2+possessive-wrapper1 costs,
cancellation/shared1000 ceiling remain. Charge an additional1 for each explicit
atomic wrapper before its group body. Require nonempty single-branch groups;
keep capturing ancestor numbers and existing classification/eligibility.
Possessive Atomic(Repeat) remains an iterator modifier and does not disable Skip.
Use a local eligibility boolean; return Skip=boolean && groupDepth<=4 &&
captureClassification.Skip. No new compiler fields/runtime state are needed.

Mandatory-set discovery may traverse Atomic(NoncapturingGroup) when a first-star
identity has already been proven. Remove only the earlier !skip guard, and make
any such atomic traversal disable the later PLUS Skip marker. Existing traversal
charges/1000/cancellation/capture guards remain, including existing post-star
atomic handling. Without proven-star identity, atomic mandatory sets remain
excluded. Final mandatory-set failure still clears star identity/Skip.
This changes only the supported star-owned subset; a general atomic-group
optimizer would expand unrelated behavior, while retaining the rejection leaves
known native LF-entry differences. Reuse the existing matcher and markers.

## Constraints and verification

No matcher/parser/membership/folding/Step/runtime state/layout/instruction/frame/
journal/span/pointer/snapshot/API/limit changes. No workflow/package/version/
dependency/consumer/CommandFramework changes. Parser256/captures256/instructions4096/
marks256/frames1024/work1m/proof1000/window12. C#13/net8/9/10, UTF8 LF, project/
solution at root,src/tests/samples. Local Debug/PR Staging; six native OS/arch.
All12798 baseline expectations/frozen35 unchanged. No subagents/Python/SDK install.

Pin strict/tolerant full captures for atomic depths1/3/4/5/6/33/199, mixed ancestors,
short/long atomic aliases, star source/modifiers/casing/UCP, empty/consumed runs,
erased operands across levels, named/duplicate/related captures/tests, retained/
query/multiple-star barriers, later deep/atomic/capture groups and outside-root
star cache controls. Unsupported bare atomic PLUS, nullable-prefix-only wrappers,
alternatives/other placements have ordinary-consumption controls only.
Six newline/UCP/every-offset and invalid fragment/start protocols; exact/exhausted/
observed work, cancellation/default budget/parser257/captures257/depth limits.

Tests-only draft -> successful-build new-class RED/all12798baseline pass -> source
with original tests/expectations unchanged -> exact-head six native Staging full/
frozen214/net8/9/10/configured gates/7equal workloads,0failed/skipped tests/build
warnings/errors -> wholebranch author review/strict logs/tree/blob/tests-afterRED/
frozen/clean/whitespace audit -> issue102 open/PR ready/unmerged, user review/merge.
#102 stays open for other placements and bitmap availability (predicates/groups/
alternatives/negated/property/call/DEFINE/conditions); byte99/unsafe100 separate.
No complete PCRE/release/Grep-readiness claim. Grep last; publication and all known
consumer migrations precede any CommandFramework code removal.
