# PCRE roadmap status

Reviewed after merged PR229, with PR230 in progress. Issue #7 remains the delivery roadmap; the
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

PR229 is merged at `a590294fa81439096ba910a953db4be28331b967`;
main 43181932890 and scripts 43181932592 are green. Its 1,046 new controls
qualified in 18 native results, 53,999 tests/framework, with zero failures,
skips, build warnings or errors.

Internal default-false MatchWord and MatchLine implement native extra compile
bits 0x4/0x8. Line wins when both are set. Existing word boundaries or line
anchors surround one existing noncapturing source group. Guards use entry
multiline/UCP/ASCII-word policy, before local source options, and line mode keeps
the selected newline convention and final-newline behavior. Word mode uses word
boundaries, including punctuation between word characters. All alternatives,
cuts, ACCEPT, K, raw labels and capture numbering retain native scope.

The parser copies both flags and snapshots entry options before source headers.
It wraps the typed root after original source diagnostics, before name/call/width
resolution and final subroutine targets, so whole-pattern recursion includes the
guards. With both options false it returns the identical root. No source rewrite,
new AST node, opcode, runtime mode, exported API or dependency is introduced.
Existing guards, work, cancellation, journals and resource limits own execution.

The 1,543 new cases include 1,480 independently pinned PCRE2 10.48/JIT profiles,
all four selections, byte/UTF/UCP, initial/local multiline, six newline conventions,
recursion and scoped exits. Supplemental controls pin 40 exact diagnostics, every
octet, malformed-UTF/CRLF offsets, old caller policies, UTF validation, owned
source/options/raw labels, all 256 user captures, canonical work and quotas.
Supplemental native patterns compile before managed matching; earlier tests stay fixed.

Valid tests-only RED 690 (run 43189811376, Linux ARM64 job 131085966252), source
`7cabd7ffe56049052bd1c9fac416ce2a39a91cf3`, exactly matched merged PR229
production. net8/9/10 built with zero warnings/errors; each had 55,542 tests,
350 expected mode-dependent failures, 55,192 passes and zero skips. Defaults,
exact diagnostics, UTF validation, full capture capacity, earlier fixtures and
native literal/oracle checks passed. Freeze the reference at
`ac93862e45ff345c70dd742e0f9b52128c227cbb` before production changes.
Final qualification requires all 55,542 tests in 18 native platform/framework
results and the existing frozen, Unicode, parity, resource and package gates.

Other modes/directives/callouts, failed/partial labels, UTF code-unit escapes,
wider optimizer/resources (#102), UTF16/exported API and realistic consumer/
release qualification remain open. Grep stays last. See
[design](PCRE-WORD-LINE-MODES-DESIGN.md) and [plan](PCRE-WORD-LINE-MODES-PLAN.md).
