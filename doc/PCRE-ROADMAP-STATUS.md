# PCRE roadmap status

Reviewed after merged PR232, with PR233 in progress. Issue #7 remains the delivery roadmap; the
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

PR232 merged at49037aef15cce24e7b6c79df7e6f04a928eced1f;
main43236233339 and scripts43236233142 are green. Its2,052 new controls
qualified in18 native results,59,682 tests/framework, with zero failures,
skips, build warnings or errors.

## PR233: optional unset backreferences

Internal default-false MatchUnsetBackreference implements PCRE2_MATCH_UNSET_BACKREF
(0x200). Existing nonparticipating captures match empty without becoming set;
capture conditions, declaration-ordered duplicate selection and participating
empty/nonempty comparisons keep their existing semantics. Missing references
remain errors with the established global validation order and byte diagnostics.

One selected heap RewriteTree pass marks canonical Backreference nodes after
name resolution and before condition/call/width/final-target resolution. The
default path skips the pass. UnsetMatchesEmpty lowers into unused Backreference
Second; null lookup advances pc only for Second=1, without source reads or
capture/journal mutation. The existing instruction still charges work. Selected
consumed references reject at their owning lookbehind opener, even under zero
repeats or through calls; existing lookahead/DEFINE/FAIL skips remain shared.

A bounded pure-instruction empty-path proof extends existing STARTLINE metadata
for selected nullable references under ANY/MatchInvalidUtf. It uses at most the
existing 1,000 start-analysis instruction/visit bound, heap storage and cancellation
polls; the existing root line-anchor proof and matcher exclusion/physical-EOF
rules remain authoritative. Calls/assertions/conditions/verbs are outside this
proof. A mandatory leading-B control retains first-character search. Nonnullable
optional-capture/reference prefixes, including ANY/multiline ^(?:(A)?backref1)B
after a split Unicode newline, remain a known wider optimizer gap in #102.

The 1,740 new cases include 1,652 independently pinned PCRE2 10.48/JIT profiles,
32 selected width errors and 40 legacy diagnostics. Supplement all octets, UTF
and malformed-fragment offsets, prior caller policies, owned source/options/raw
labels, all 256 user captures, exact zero-width work, cancellation and quotas.
Native loop arrays compile before managed comparisons. All 246 earlier test
files remain unchanged; GNU/frozen35/214 outcomes/120 methods, API and dependencies
are preserved. This is internal conformance work; Grep migration remains last.

Expanded tests-only RED699 (run43247650585, Linux x64 job131284532504), source
d05a98f991131d65045e4d4925d696850608766c, retained merged PR232 production.
Each net8/9/10 built with zero warnings/errors and ran 61,422 tests: 495 expected
failures, 60,927 passes, zero skips. The 454 primary differences/framework match
independent native flag-off research. Default/earlier fixtures, diagnostics, UTF
validation and native literal/oracle checks passed. Initial RED698 was provisional;
the expanded fixture was frozen at23ebe126b9e8096f4527e4479fdd89e7a9cb5223
before production edits. Require all 18 native results at61,422 passes and the
existing frozen, Unicode, parity/API/corpus, resource, packaging and script gates.

Other syntax/modes/directives/callouts, labels on failed/partial results, wider
optimizer/resources (#102), UTF16/exported API and realistic consumer/release
qualification remain open. See [design](PCRE-UNSET-BACKREFERENCES-DESIGN.md) and
[plan](PCRE-UNSET-BACKREFERENCES-PLAN.md).
