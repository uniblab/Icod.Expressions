# PCRE roadmap status

Reviewed after merged PR226, with PR227 in progress. Issue #7 remains the delivery roadmap; the
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

PR226 is merged at `fa0d79c4368f7fb65784a7191c3bf02034a83aac`;
main 43159771095 and scripts 43159771272 are green. Its 709 new controls
qualified in 18 native results, 50,453 tests per framework, with zero failures,
skips, build warnings or errors.

PR227 adds internal `EscapedCrIsLf`, default false. Only textual `\r` is
mapped to canonical LF in the shared escape parser; other CR spellings, raw
labels, original source bytes, newline policies and existing matcher paths stay
unchanged. The 1,495 distinct controls include 1,474 JIT-valid native profiles
and six unique compile-error cases, researched across 1,500 native profiles.
Additional cases exercise all byte values, UTF/CRLF offsets, option composition,
byte diagnostics, ownership, work equality, cancellation and resource ceilings.

Valid tests-only RED 682 (run 43166167501, source
`f0384874b3a39adb2128db895a7709b5e53abfcc`) had 354 option-dependent new
failures and 51,594 passes per framework, with zero skips, build warnings/errors
or earlier-reference/native literal-oracle failures. Production matched merged
PR226 exactly. All default profiles and diagnostic checks passed. The 1,495-case
reference freezes at `02a1e6f7874e39f406b91ee7496b71fc51ade294` before restoring
the same three-file implementation. The earlier RED 680/freeze receipt is
invalidated: CI 681 exposed two incorrect new default-byte diagnostic offsets,
now corrected strictly to the existing hyphen contract, with every assertion
and native profile retained. Production diagnostic behavior stays fixed.
Final qualification requires 51,948 tests in all 18 native results and author
review. All earlier reference files, 35 frozen source blobs and 214 frozen
outcomes in 120 methods remain fixed.

Other directives/callouts, failed/partial labels, UTF code-unit escapes, wider
optimizer/resources (#102), UTF16/exported API and realistic consumer/release
qualification remain open. Grep stays last. See
[design](PCRE-ESCAPED-CR-DESIGN.md) and [plan](PCRE-ESCAPED-CR-PLAN.md).
