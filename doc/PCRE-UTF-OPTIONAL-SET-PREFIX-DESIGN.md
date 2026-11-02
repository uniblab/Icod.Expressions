# UTF bitmap entry through optional positive sets

## Intent and native evidence

Continue #7/#102 after merged #134. A direct optional positive predicate-free
character set contributes its possible first characters to native bitmap study,
then study continues to the mandatory leading class PLUS. Native JIT CRQUERY
raises early-fail count to3, disabling type_skip. This follows the distinction
already implemented for optional literals, with a range-backed proof that the
optional set cannot consume CR.

Pinned PCRE.NET1.6.0/PCRE2 10.48 JIT, UTF ANYCRLF, S=[\x09-\x0c\x{100}]:
[B-C]?(S+)A matches CRLF+A at1:2, U0100+CRLF+A atLF3:2 and VT+CRLF+VT+A
atLF2:3. On B+VT+A the prefix consumes B and captures VT at1:1. Optional sets
that admit LF can consume it; possessive [\x09-\x0c]?+ therefore misses CRLF+A
but matches LF+VT+A with only VT in the mandatory capture. Preserve those spans.

Native pcre2_study.c1731..1885 rejects property-bearing XCLASS, adds positive
UTF class/map start bits and continues after CRQUERY/CRMINQUERY/CRPOSQUERY.
JIT detect_early_fail handles those opcodes by advancing and raising count3.
CR-admitting classes cannot establish a CR-excluding bitmap proof. Source CR/LF
metadata is independent and must remain intact.

## Approaches and implementer choice

Under standing autonomy, extend the existing shared first-node helper with one
optional ScalarSet arm and a bounded class-range predicate. Reuse Node/BlocksSkip,
root/group traversal, selected-leaf proof and existing VM execution. Rewriting
AST nodes would lose source provenance; a separate nullable-prefix engine would
duplicate bounds and admit unpinned shapes. Keep this change within the helper.

## Compiler contract

src/Pcre/PcreByteProgram.cs: keep FindUtfCrLfLeadingNode's tuple signature.
For Repeat{Minimum:0,Maximum:1,Child:ScalarSet}, call private bool
IsUtfCrLfOptionalSet(PcreByteScalarSet set, ref int visits). Require a nonempty,
nonnegated set with no predicates. Iterate its ranges with cancellation; charge
each against MaximumStartClassVisits1000; reject out-of-scalar bounds or any
range containing CR. LF membership is not required in the optional prefix: the
mandatory selected class still establishes LF admission. Range visits plus the
existing repeat/child cost2 and possessive wrapper1 share each caller's budget.
On success return cost2/BlocksSkip=true. Empty/invalid/over-bound/unknown/other
optional nodes stay conservative. A later pruned node never clears the barrier.

No changes to scalar membership, case closure, source flags, AST or VM input
consumption. Selector/proof remain restricted to one mandatory PLUS chain;
capture dependencies and actual group depth retain existing behavior. Negated/
predicate-bearing optional sets, types/groups and larger finite bounds are not
newly admitted. All-optional branches supply no mandatory class proof.

Keep parser256/captures256/instructions4096/repeat marks256/frames1024/default
work1m/proof-classification1000/prefix window12. No matcher/runtime layout/storage/
Step/capture/frame/journal/saved span/interior pointer/snapshot/stackalloc change.
All7136 baseline expectations and frozen35 source blobs stay unchanged.

## Verification and boundaries

Native-first literal full captures for ASCII/wide/disjoint/caseless sets,
greedy/lazy/possessive/counted options, PLUS variants, consumed prefixes, LF
consumption, root/interior/deep groups, dependencies and retained/pruned mixtures.
Pin CR-admitting/negated/source-newline/trailing/all-optional/no-LF/G controls.
Six newline/UCP/every offset/strict-tolerant fragments, observed/unobserved exact
and exhausted work, cancellation/default budget/parser257. No unsafe Xuc probes.
Tests-only draft -> successful-build RED with7136 baseline passing -> helper ->
six native Staging OS/arch/net8/9/10 full/frozen214/configured gates and seven
equal workloads -> whole-branch author review/exact head-tree-log/frozen/clean/
whitespace/tests-after-RED audit -> ready; user merges.

C#13/net8/9/10, UTF8 LF, root project/solution and src/tests/samples. No subagents/
Python/SDK installation/public/package/version/dependency/workflow/release/
consumer/fallback changes. Broader102 optimizer/nullable/alternative/call/DEFINE/
conditions remain, byte99/unsafe100 separate. Grep last; CommandFramework removal
requires publication and all known consumer migrations.

## Case-folding source availability correction from author review

Native compilation can turn a one-source-character caseless class into a
PT_CLIST property query. (?i)[k]?, [s]? and sigma/Kelvin singleton forms then
lose start-bitmap availability, whereas multi-fold-key classes such as [k-l]?
retain it. Managed ScalarSet ranges contain canonical folded keys, so a
caseless one-key set cannot prove which native source opcode was emitted.
Reject that ambiguous shape conservatively after the bounded range checks:
require !set.Caseless, more than one range, or a nonsingleton sole range.
No source metadata or case-fold membership change is introduced. This leaves
ordinary caseless singleton classes and differently spelled one-fold-key classes
on their preexisting conservative path until source provenance is preserved.

Add a separate native-first case-fold regression file and observe successful-build
RED before the guard correction. Keep the original196-test file unchanged.
Multi-fold-key caseless controls must remain admitted. This is a correction to
the new class proof, not a full resolution of caseless/source optimizer provenance.
