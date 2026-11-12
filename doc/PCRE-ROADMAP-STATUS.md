# PCRE roadmap status

Reviewed after merged PR230, with PR231 in progress. Issue #7 remains the delivery roadmap; the
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

PR230 is merged at f0d240b9536a722bc370cc8a859f097f148689b6;
main43212521201 and scripts43212521645 are green. Its1,543 new controls
qualified in18 native results,55,542 tests/framework, with zero failures,
skips, build warnings or errors.

Internal default-false AsciiBsS, AsciiBsW, AsciiPosix and AsciiDigit initialize
the existing five-bit ASCII parser mask beside AsciiBsD. Native extra bits are
0x200/0x400/0x800/0x1000. Caller Posix seeds only Posix and Digit only PosixDigit;
source aP still sets both, aT selects Digit and reset preserves the mask.
Existing local unsets, alternative propagation, group snapshots and restoration
remain authoritative. No source rewrite, new node, opcode, runtime matching flag,
exported API, dependency or GNU/BRE/ERE change is introduced.

Backslash s/S and w/W use canonical ASCII predicates when selected; w also owns
b/B and MatchWord's entry guards. Posix restricts ordinary POSIX classes, while
Digit restricts digit/xdigit independently of backslash d. Properties, h/v/R and
literal/range folding remain independent. Byte/UTF/UCP domains, class algebra,
lookbehind widths, captures/calls/cuts and all shared resource paths stay fixed.

The 2,088 cases include 1,979 independently pinned PCRE2 10.48/JIT profiles.
Every one of 32 flag combinations runs in all four UTF/UCP modes. Scoped unsets,
overlap/reset, captures/references/calls, assertions, algebra/folding, raw labels
and word/line entry policies are pinned. Supplemental controls cover 60 exact
diagnostics, every octet, malformed-UTF/CRLF start offsets, all14 POSIX names
and UTS algebra, prior policies, UTF validation, owned settings/source/labels,
canonical work, cancellation and quotas. All native loop arrays compile before
managed comparisons. All244 earlier test files remain hash-identical.

Valid tests-only RED692 (run43215345702, Linux x64 job131173057468), source
07e4228007c7b870b6d9942e09db09d7dc6b46de, retained merged PR230 production.
net8/9/10 each built with zero warnings/errors and ran57,630 tests:239 expected
option-dependent failures,57,391 passes and zero skips. The219 primary failures
exactly match independently predicted native flag differences. Defaults, original
diagnostics, UTF validation, canonical work/cancel/limits, earlier fixtures and
native literal/oracle checks passed. Freeze the2,088-case reference at
b8fdd8af07f4bb998e56603953ead7dc18d4f111 before production changes.
Final qualification requires57,630 tests in18 native results and the existing
frozen, Unicode, parity, resource and packaging gates.

Other modes/directives/callouts, failed/partial labels, UTF code-unit escapes,
wider optimizer/resources (#102), UTF16/exported API and realistic consumer/
release qualification remain open. Grep stays last. See
[design](PCRE-CALLER-ASCII-POLICIES-DESIGN.md) and
[plan](PCRE-CALLER-ASCII-POLICIES-PLAN.md).
