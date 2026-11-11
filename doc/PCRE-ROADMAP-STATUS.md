# PCRE roadmap status

Reviewed after PR218, with PR219 in progress, 2026-11-11. Issue7 remains the delivery roadmap; the
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
   lexical alternation retry and assertion/callee catches. UTF code-unit escape,
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

PR218 is merged and its main checks are green. PR219 adds exact no-argument
THEN: retry the nearest lexical sibling, or on the last branch permit earlier
outer retries. One-branch wrappers and conditional arms inherit ownership;
assertions/callees bound it, with the pinned JIT positive-assertion/local-call
exception. Owned-trap rollback stays distinct from forced selected-value call
quit. Journaled stack-depth checkpoints reuse mark/capture
transactions, with shared256 marks/1024 frames/4096 compilation/work limits.
COMMIT's bounded first-unit entry also crosses leading THEN; wider optimizer
parity remains issue102.

The original167 cases freeze after clean RED656: net8/9/10 compile with zero
warnings/errors,159 missing-feature failures and46655 passes each. Eight added
regression cases freeze after RED658:46822 total,6 capture failures,46816 passes,
0 skips and zero warnings/errors. All175 new cases remain fixed for the
production-only capture correction. Full native CI and author review are readiness gates. Named verbs/
MARK, remaining directives/callouts, UTF code-unit escape, wider optimizer/
resource conformance, UTF16/API and realistic consumer performance/release stay
open. Grep stays last; GNU/BRE/ERE and frozen35/frozen214 stay intact.
