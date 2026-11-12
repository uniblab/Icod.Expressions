# PCRE roadmap status

Reviewed after merged PR228, with PR229 in progress. Issue #7 remains the delivery roadmap; the
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

PR228 is merged at `209dd96c3b30eeadec8dfc431f2ca04ca6192354`;
main 43176216559 and scripts 43176216531 are green. Its 1,005 new controls
qualified in 18 native results, 52,953 tests/framework, with zero failures,
skips, build warnings or errors.

PR229 initializes existing parser scopes from seven default-false internal
caller options: DotAll, Multiline, NoAutoCapture, Ungreedy, DuplicateNames,
Extended and ExtendedMore. Local unsets, restoration, named numbering, duplicate
resolution and reset behavior remain shared; reset clears m/s/n/x and retains
U/J. ExtendedMore implies extended parsing. One options record replaces the
positional boolean constructor list; every old/new primitive is copied and no
options reference is retained. Prefix/UTF validation and all matching paths stay fixed.

The 1,046 controls include 988 pinned JIT profiles and 10 distinct compile errors,
researched across 1,018 native profiles and all 128 flag combinations in byte/UTF.
Additional controls pin exact diagnostics, all octets, malformed-UTF/CRLF offsets,
old caller policies, UTF validation, source/options/labels, canonical work,
cancellation and quotas. Valid tests-only RED 688 (run 43179233033, Linux ARM64
job 131050872473) had 318 expected caller-option failures, 53,681 passes and zero
skips/build warnings/errors per framework. Defaults, exact diagnostics, duplicate
grammar, whole-UTF validation, earlier tests and native literal checks passed.
Reference `0d04add04dc65e5272e9b7052c5b7727990a6f5d` freezes before production.
Final qualification requires 53,999 tests in all 18 native results and author
review. Earlier references, frozen 35 production blobs and 214 frozen outcomes
in 120 methods remain fixed.

Other modes/directives/callouts, failed/partial labels, UTF code-unit escapes,
wider optimizer/resources (#102), UTF16/exported API and realistic consumer/
release qualification remain open. Grep stays last. See
[design](PCRE-CALLER-PATTERN-OPTIONS-DESIGN.md) and
[plan](PCRE-CALLER-PATTERN-OPTIONS-PLAN.md).
