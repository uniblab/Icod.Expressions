# PCRE roadmap status

Reviewed after merged PR231, with PR232 in progress. Issue #7 remains the delivery roadmap; the
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

PR231 is merged at a897e8a5663850f0c3b9fd2da2968321ed0f2989;
main43219511682 and scripts43219511702 are green. Its 2,088 new controls
qualified in 18 native results, 57,630 tests/framework, with zero failures,
skips, build warnings or errors.

Internal default-false DollarEndOnly and AltCircumflex implement native flags
0x10 and 0x200000. DollarEndOnly makes non-multiline dollar require the selected
subject end, while multiline dollar ignores it and backslash-Z/z stay unchanged.
Alternate multiline caret can match after a final newline. Local m, unsets and
reset still choose each source anchor; MatchLine guards use their entry snapshot.

Shared parser helpers lower ordinary and synthetic anchors. DollarEndOnly selects
the existing absolute-end node. One default-false AllowSubjectEnd property on
existing LineAnchor lowers into the unused StartLine First operand. First=1 only
extends the endpoint bound; fragment/context, excluded candidate, newline and
Step checks stay shared. Default operands are unchanged. No new AST node, opcode,
program-global runtime mode, source rewrite, API, dependency or width rule.

The 2,052 new cases include 1,984 independently pinned PCRE2 10.48/JIT profiles.
All four selections, initial/local/reset multiline, six newlines, empty lines,
absolute anchors, fixed/variable lookbehind endpoints, assertions/calls, cuts/
ACCEPT/K, labels and line wrappers are pinned. Supplemental controls cover 40
exact diagnostics, every octet, selected malformed-UTF/CRLF/Unicode-newline
offsets, prior caller policies, UTF validation, owned options/source/raw labels,
canonical work, cancellation and quotas. All native loop arrays compile before
managed matching. All 245 earlier test files remain hash-identical.

Before freeze, add 72 variable-lookbehind profiles and the same patterns to the
offset loop; the initial run694 was cancelled. Valid expanded tests-only RED695
(run43228870315, Linux ARM64 job131219356070), source
2d166f0ecd5c6f4b8b0047b2007fbd99ccf11d61, retained merged PR231 production.
net8/9/10 each built with zero warnings/errors and ran 59,682 tests: 245 expected
option-dependent failures, 59,437 passes and zero skips. The 230 primary failures
exactly match independent native flag-off research. Defaults, original diagnostics,
UTF validation, canonical work/cancellation/limits, earlier fixtures and native
literal/oracle checks passed. Freeze the 2,052-case reference at
a3e651d589d160f68c574911733e70ad73e4baca before production changes.
Final qualification requires 59,682 tests in 18 native results and the existing
frozen, Unicode, parity, resource and packaging gates.

Other modes/directives/callouts, failed/partial labels, UTF code-unit escapes,
wider optimizer/resources (#102), UTF16/exported API and realistic consumer/
release qualification remain open. Grep stays last. See
[design](PCRE-ANCHOR-COMPILE-OPTIONS-DESIGN.md) and
[plan](PCRE-ANCHOR-COMPILE-OPTIONS-PLAN.md).
