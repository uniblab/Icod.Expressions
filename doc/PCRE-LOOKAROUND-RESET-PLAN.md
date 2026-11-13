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

- [ ] Preflight matching/error/offset/capacity/work profiles independently.
- [ ] Write literal native spans, including (?=abK) start2/end0 and behind
  before-offset controls; pin all five review boundaries.
- [ ] Publish tests-only draft PR243. Inspect clean net8/9/10 RED with only
  missing-option failures, then freeze fixture before source changes.

## Task 2: parser integration and inventories

**Files:** Modify src/Pcre/PcreByteOptions.cs, src/Pcre/PcreByteParser.cs;
update seven repository documentation files including this design/plan.
**Interfaces:** bool AllowLookaroundBackslashK init property, readonly parser
snapshot and existing ParseAtom K prohibition conditional on that snapshot.

- [ ] Add the documented default-false option and parser snapshot/guard.
- [ ] Update final inventories before source push; retain existing journal,
  canonical work and unsupported boundaries, source and prior test hashes.
- [ ] Verify all18 full-suite results and every configured supporting gate.

## Task 3: review and delivery

- [ ] Whole-branch author review all five boundaries inline; no agents.
- [ ] Verify clean worktree and exact source/base/tree/proposed-merge identities.
- [ ] Record qualification on PR243/issue7 footer, mark ready and retire only
  this plan's ledger. User owns merge/tag/publication; issue7 stays open.
