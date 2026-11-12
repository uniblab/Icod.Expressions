# PCRE roadmap status

Reviewed after merged PR227, with PR228 in progress. Issue #7 remains the delivery roadmap; the
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

PR227 is merged at `600d4f9e9f0d6c984ed101c163262cbcc615f3b1`;
main 43169151838 and scripts 43169151810 are green. Its 1,495 new controls
qualified in 18 native results, 51,948 tests per framework, with zero failures,
skips, build warnings or errors.

PR228 adds internal `AllowEmptyClass`, default false. Enabled `[]` never matches
and `[^]` matches any character including newlines; classes remain repeatable.
Ordinary and nested UTS classes use the existing empty bitmap/ALLANY nodes and
bounded scalar terms. Perl class algebra, original source bytes, default literal
closing-bracket behavior and all existing matcher paths remain unchanged.

The 1,005 controls include the primary 941-case fixture and 64 width controls,
researched across 1,472 native profiles (858 JIT-valid, 614 error profiles).
One compile-time class provenance field distinguishes direct empty source,
which native rejects in consumed lookbehind, from computed-empty sets and
unconsumed lookahead/DEFINE/FAIL content. No new node, opcode or VM state.

Valid tests-only RED 686 (run 43173736095, Linux ARM64 job 131032890324),
source `56e69c751148142a74f52d34e46d5829863a6684`, kept the first implementation
fixed and had 28 expected lookbehind-rejection failures and 52,925 passes per
framework, zero skips/build warnings/errors. Every valid/default/diagnostic/
offset/native-oracle/earlier-reference check passed. The corrected 941-case blob
`c917a24b7d6d7a9cdaacd8a43a27a01e9058562d` and 64-case blob
`08edf98ee26f4c3efcd44c2a591ae5b087756567` freeze before the width correction.
The initial freeze is superseded after CI 685 exposed two invalid positive-loop
entries, now replaced with valid lookaheads while all rejection controls remain.
Final qualification requires 52,953 tests in all 18 native results and author
review. Earlier references, 35 frozen production blobs and 214 frozen outcomes
in 120 methods stay fixed.

Other modes/directives/callouts, failed/partial labels, UTF code-unit escapes,
wider optimizer/resources (#102), UTF16/exported API and realistic consumer/
release qualification remain open. Grep stays last. See
[design](PCRE-EMPTY-CLASSES-DESIGN.md) and [plan](PCRE-EMPTY-CLASSES-PLAN.md).
