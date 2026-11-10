# PCRE roadmap status

Reviewed after PR209, with PR210 in progress, 2026-11-10. Issue7 remains the delivery roadmap; the
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
   implemented in PR209; PR210 adds the remaining native binary/Bidi catalogue.
   Grapheme clusters, extended class algebra, UTF code-unit escape,
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

PR210 adds the remaining57 native binary properties/106 aliases and23 Bidi
values after scripts/scx in PR209 and category/simple-case data in PR207/208.
It preserves native aliases, unassigned Bidi defaults, InCB/Mirrored rules,
complements, byte projection and existing grammar. Source hashes, complete
native fingerprints and live JIT boundary fixtures qualify data ownership.

The native property catalogue is covered. Graphemes and extended class algebra
are the next deliberate syntax work; script-run groups remain a separate gate.
General optimizer/resource, UTF16/API, realistic consumer performance and
release gates remain open. Grep stays last; GNU/BRE/ERE and frozen35 stay intact.
