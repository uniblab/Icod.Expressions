# PCRE roadmap status

Reviewed after merged PR222, with PR223 in progress, 2026-11-12. Issue7 remains the delivery roadmap; the
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

PR222 is merged; main43125926626 and scripts43125926861 are green. PR223
adds named ACCEPT/FAIL/F through actual MARK then existing immediate opcodes.
ACCEPT quantifiers enclose the implicit mark, while lexical erased names still
affect atomic history isolation. Capture/label rollback and scope pruning remain
shared. Failed/partial-attempt result labels remain a separate gate.

All312 new cases freeze after clean RED667: net8/9/10 compile with zero warnings/
errors,298 expected new failures and47806 passes each,48104 total.266 literal/
profile fixtures were checked in526 native executions. Six non-atomic ACCEPT
fixtures use the normal native matcher after native JIT_UNSUPPORTED, an explicit
finite qualification matching the existing oracle. Obsolete managed inventories
advance while native matching/capture expectations remain fixed. The18 accepted-call regression controls freeze after clean RED670 with10
expected label failures and no other failures. The correction preserves native
accepted-call labels using bounded lexical local-quit metadata and the shared
journal. Final qualification requires48122 cases per framework in18 native
results (330 new cases), plus author review.

Failed/partial labels, remaining directives/callouts, UTF C, wider optimizer/
resources (issue102), UTF16/exported API and realistic consumer performance/
release stay open. Grep stays last; GNU/frozen35/frozen214/120 remain fixed.
See PCRE-NAMED-IMMEDIATE-VERBS-DESIGN.md and PCRE-NAMED-IMMEDIATE-VERBS-PLAN.md.
