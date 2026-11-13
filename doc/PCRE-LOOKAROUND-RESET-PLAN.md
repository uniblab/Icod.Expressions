# Lookaround Match-Start Reset Implementation Plan

> Execute inline with executing-plans under the standing no-agents/no-pauses workflow.

**Goal:** Add caller opt-in for native K semantics inside supported assertions.
**Architecture:** Snapshot a parser policy, retain canonical reset lowering and
the shared assertion/call journal, and preserve signed reported endpoints.
**Tech Stack:** C#13, net8/net9/net10, xUnit and pinned PCRE2 10.48 oracle only.
**Spec:** doc/PCRE-LOOKAROUND-RESET-DESIGN.md.

## Global constraints

Preserve baseline242,259 earlier test files, GNU/BRE/ERE, frozen35/214outcomes/
120methods, IR/opcodes/matcher/journals/shared limits/exported API/dependencies/
workflows. UTF8+LF; no Python/agents/SDK installs. Reuse linked worktree. User
owns merge/tag/publication; Grep last. Repository docs precede qualification;
final evidence only PR243/issue7 footer, without a docs-only source push.

## Review focus

1. Default prohibition versus immutable opt-in, including reset scopes and erased bodies.
2. Positive propagation versus negative/failure rollback through nesting and calls.
3. Signed group0 endpoints, lookbehind reports before requested start and NOTEMPTY.
4. Inert text, prior/source errors and other unsupported/resource boundaries.
5. Owned source/options, work/cancellation, full capture and parser/expansion limits.

## Task 1: native-first regression contract

**Files:** Create tests/Pcre/PcreLookaroundResetReferenceTests.cs.
**Interfaces:** Optional reflection selects future bool AllowLookaroundBackslashK;
native extra0x40 pins endpoints. Compare oracle EndIndex-Index, never Length.

- [x] Preflight matching/error/offset/capacity/work profiles independently.
- [x] Write literal native spans, including (?=abK) start2/end0 and behind
  before-offset controls; pin all five review boundaries.
- [x] Publish tests-only draft PR243. Inspect clean net8/9/10 RED with only
  missing-option failures, then freeze fixture before source changes.

## Task 2: parser integration and inventories

**Files:** Modify src/Pcre/PcreByteOptions.cs, src/Pcre/PcreByteParser.cs;
update seven repository documentation files including this design/plan.
**Interfaces:** bool AllowLookaroundBackslashK init property, readonly parser
snapshot and existing ParseAtom K prohibition conditional on that snapshot.

- [x] Add the documented default-false option and parser snapshot/guard.
- [x] Update final inventories before source push; retain existing journal,
  canonical work and unsupported boundaries, source and prior test hashes.
- [ ] Verify all18 full-suite results and every configured supporting gate.

## Task 3: review and delivery

- [ ] Whole-branch author review all five boundaries inline; no agents.
- [ ] Verify clean worktree and exact source/base/tree/proposed-merge identities.
- [ ] Record qualification on PR243/issue7 footer, mark ready and retire only
  this plan's ledger. User owns merge/tag/publication; issue7 stays open.

## PR243: regression and integration evidence

Tests-only [RED737](https://github.com/uniblab/Icod.Expressions/actions/runs/43444152881), source `552f6672bac9d57214d556d73fb76478429cd340`, Linux ARM job131949099519: net8/9/10 each built with zero compiler warnings/errors, then ran **1,406 expected failures / 79,328 passes / 80,734 total / zero skips**. Only missing-policy cases failed:1,366 primary spans,3 error boundaries,24 offset theories,4 work/ownership cases,2 capacity/depth facts,1 quota fact and6 implicit-guard profiles per framework. All79,118 earlier tests and210 new controls passed. All native literal endpoint assertions passed. Freeze `a6cd825ba357ca8be6f11fd3ff788032f0c24217` before production; no fixture refinements were needed.

The1,616 new cases include1,574 primary native profiles (1,510 matching/query/capture/success-label and64 compile profiles),1,032 selected offset protocols and16 supplemental work/capacity/implicit-guard profiles. All accepted native matching uses JIT0 and negative matching requires no-match(-1). Fifty-eight primary profiles report reversed group0 endpoints. Two UTF-C profiles outside native JIT support remain separate compile/unsupported boundaries. Another54 native review probes cover selected reset/i/r scopes, UTF/casing prefixes and lexical callee resets.

Task1 and Task2 integration/inventories are complete. Final all18 qualification, author review, identities, footer and readiness are recorded on PR243/issue7 after observation; pending boxes represent those external evidence gates, without a documentation-only CI run.
