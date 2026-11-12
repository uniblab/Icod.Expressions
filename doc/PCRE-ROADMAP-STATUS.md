# PCRE roadmap status

Reviewed after merged PR233, with PR234 in progress. Issue #7 remains the delivery roadmap; the
[syntax matrix](PCRE-SYNTAX-ACCEPTANCE-MATRIX.md),
[resource contract](PCRE-RESOURCE-ERROR-CONTRACT.md) and
[Grep fixture inventory](PCRE-GREP-CONSUMER-FIXTURES.md) define the detailed gates.

## Present position

The internal byte/UTF8 matcher implements ordered alternatives, captures and
backreferences, recursive subroutines, atomic/non-atomic assertions, bounded
variable lookbehind, conditions, repetition, newline policies, malformed-UTF
fragments and guarded resource/cancellation behavior. The observed Grep fixture
floor is mapped, and component correctness/performance probes run in CI. These
are substantial implementation milestones, not an unrestricted PCRE2 claim.
PRs147–205 principally expanded native start-optimization/CRLF entry parity;
issue102 still tracks the wider optimizer gap. PR206 accelerated Intel CI while
retaining each framework's complete suite and frozen-result qualification.

## Remaining order

1. **Complete conformance and Unicode data ownership.** General categories
   are pinned to PCRE2 10.48's Unicode17 in PR207. PR208 pins the complete
   default simple-case mapping, closing host-version dependence in that path.
   Continue offsets, resources and malformed-UTF evidence;
   keep native optimizer differences and unsafe native cases explicitly tracked.
2. **Expand deliberate syntax coverage.** Script/script-extension properties are
   implemented in PR209; PR210 completes the native binary/Bidi catalogue and
   PR211 adds atomic grapheme clusters. PR212 adds Perl class algebra; PR213 adds
   UTS#18 activation, PR214 adds script-run groups, PR215 adds scoped ACCEPT and
   PR216 adds no-argument PRUNE; PR217 adds SKIP candidate restart and one bounded
   PRUNE repeat-cache parity proof. PR218 adds scoped COMMIT root termination
   with bounded native first-code-unit entry. PR219 adds no-argument THEN with
   lexical alternation retry and assertion/callee catches. PR220 adds MARK and
   its shorthand with successful-path byte labels on the internal result. PR221
   adds named SKIP using bounded visible MARK history and shared scoped cuts.
   PR222 adds successful labels for named PRUNE/COMMIT/THEN. PR223 adds
   named ACCEPT/FAIL/F using authentic implicit MARK and existing scope exits.
   PR224 adds the CASELESS_RESTRICT prefix through the existing (?r) policy.
   PR225 adds UTF Turkish casing through the shared scalar folding policy.
   PR226 adds caller Python-octal and bare-zero policies to shared numeric parsing.
   PR227 adds caller escaped-CR remapping through canonical literal parsing.
   PR228 adds caller-selected empty classes through canonical nodes and bounded UTS terms.
   PR229 adds caller s/m/n/U/J/x/xx initialization through the existing scoped state.
   PR230 adds native word/line compilation through existing typed guards and scopes.
   PR231 completes caller ASCII type restriction initialization through the same mask.
   PR232 adds dollar/circumflex compile options through canonical anchor lowering.
   PR233 adds optional unset backreferences through the existing capture instruction.
   PR234 extends bounded STARTLINE search to opaque reference prefixes in both policies.
   UTF code-unit escape,
   remaining directives/verbs/callouts and other matrix gaps require their own
   pinned reference fixtures. Decide any intentional exclusions before release;
   successful compilation must never imply an unsupported construct works.
3. **Finish architecture/API and consumer performance review.** Preserve the
   byte-authoritative contract; complete the planned UTF16 peer/public API
   decisions and realistic workload measurements. Component smoke comparisons
   do not establish command-level equivalence or release readiness.
4. **Publish and migrate consumers, with Grep last.** Qualify the package and
   public compatibility contract, then migrate consumers in the approved order.
   Grep additionally needs command diagnostics/cancellation, benchmark/stress and
   all six-RID packaging/archive/behavior gates. Remove its PCRE.NET runtime only
   after qualification. Keep CommandFramework production ownership until every
   known consumer migration is complete.

## Current increment and next syntax work

PR233 merged at5a286d566e8f3714cd4fd6fe185b927c7d0e2812;
main43251995145 and scripts43251995258 are green. Its1,740 new cases
qualified in18 native results,61,422 tests/framework, with zero failures,
skips, build warnings or errors.

## PR234: backreferences blocking first-character search

Extend PR233 STARTLINE metadata from selected nullable bodies to pure programs
with an opaque reference reachable before consumption. Native compile_branch
disables inferred first characters at references; JIT scan_prefix cannot cross
them before any character, and STARTLINE precedes FIRSTMAP. Default references
are equally opaque. ANY/multiline ^()backref1B on LS+B misses at split offset1
but matches3:1/capture1=3:0 at boundary3 under either reference policy.

Only PcreByteProgram.cs changes: rename the existing private helper to
HasBackreferenceStartBarrier, require any canonical reference and accept a
nonconsuming path reaching Backreference or Accept. Mandatory literals and
one/two-member classes keep native prefix-search priority. A supplementary
linear proof admits exactly one positive, predicate-free ASCII class with more
than two members before a reference: it cannot supply JIT first-character-pair
search. Additional consumers, branches/loops, non-ASCII ranges, predicates and
Turkish casing stay outside that proof. Every branch must still pass the existing
AST first-caret proof. Keep pure-instruction eligibility,1,000 instructions/visits,
heap storage and cancellation. No matcher/parser/IR-shape/source/capture/journal/work/quota/
API/dependency changes. Calls/assertions/conditions/verbs and over-budget
programs remain outside this conservative proof; broader #102 stays open.

The949 new cases include936 independently pinned PCRE2 10.48/JIT profiles
and284 preflighted supplemental native profiles. Cover both reference/casing
policies, aliases, participation/duplicates, repeats, wrappers/alternatives,
NEL/LS/PS split/boundary offsets, malformed fragments, strict boundaries, UCP,
owned source/options, deep/full-capacity/large-prefix proofs and shared work/
cancellation/quotas. Native arrays compile before managed comparison. All247
earlier test files and GNU/frozen35/214 outcomes/120 methods remain fixed.

Clean tests-only RED702 (run43255390418, Linux x64 job131312086289), source
1107f679f0a8da3b3ae069d0934e5c825a53c36c, retained merged PR233 production.
Each net8/9/10 built0warnings/errors and ran62,371 tests:295 expected failures,
62,076 passes,0skips. All285 primary differences/framework match independent
native split/boundary research, including87 default and198 selected failures.
Earlier fixtures, strict/fallback controls and native literal/oracle checks
passed. Freeze fixture f78f3d24ff62419bfefd9672a902259138fbbb7e before
production; require all18 native results and every configured supporting gate.

Qualification after the frozen six-case correction: source
`a00c32c0f9531808415991f99dd6cb4a2bfc5f38`, [CI704](https://github.com/uniblab/Icod.Expressions/actions/runs/43260813850)
and [scripts266](https://github.com/uniblab/Icod.Expressions/actions/runs/43260813804)
are green. All18 native platform/framework results pass62,371 tests each, with
zero failures/skips/build warnings/errors and214 frozen outcomes in120 methods.
Frozen35, five Unicode generators, seven equal component workloads, extraction/
POSIX/glibc/API/corpus, packaging and three package-smokes pass. Three-OS script
gates pass; full consumer compatibility is skipped by the PCRE-only classifier.
The949-case fixture and247 earlier test files are unchanged. An additional1,728
native JIT checks confirm class-pair priority across casing/UCP/reference policies,
root wrappers andNEL/LS/PS offsets. Whole-branch author review found no remaining
issues in this scope; independent review was not performed. Broader optimizer
shapes remain in #102. Final documentation changes preserve the qualified
source, test, packaging and workflow trees.

Other syntax/modes/directives/callouts, failed/partial labels, wider optimizer/
resources (#102), UTF16/exported API and realistic consumer/release gates remain
open. Grep stays last. See [design](PCRE-BACKREFERENCE-STARTLINE-DESIGN.md) and
[plan](PCRE-BACKREFERENCE-STARTLINE-PLAN.md).
