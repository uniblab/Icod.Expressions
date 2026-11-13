# Escaped UTF Literal Implementation Plan

> Execute inline with executing-plans under the standing no-agents/no-pauses workflow.

**Goal:** Parse native ordinary escaped UTF-8 scalars through canonical literals.
**Architecture:** Reuse the validated scalar literal reader at shared escape
entry; retain all existing parser/IR/runtime consumers and diagnostic ownership.
**Tech Stack:** C#13, net8/net9/net10, xUnit, pinned PCRE2 10.48 oracle only.
**Spec:** doc/PCRE-ESCAPED-UTF-LITERALS-DESIGN.md.

## Global constraints

Baseline243:80,734tests/framework,260 prior files. New fixture freezes after RED;
only two old pending assertions are intentionally converted to native acceptance
for their same inputs, with258 other prior hashes fixed. Preserve GNU/BRE/ERE,
frozen35/214outcomes/120methods, IR/opcodes/matcher/journal/shared limits/API/
dependencies/workflows. UTF8+LF; no Python/agents/SDK installs. Reuse worktree.
User owns merge/tag/publish; Grep last. Final repository docs precede source
qualification; final evidence only PR244/issue7 footer, without docs-only CI.

## Review focus

1. Decode exactly one whole UTF scalar; byte mode and following quantifiers retain ownership.
2. Canonical case/ASCII-restriction/Turkish and ordinary/Perl/UTS range semantics stay shared.
3. Whole-source UTF/caller/prior errors and later structural errors retain original byte offsets.
4. Quotes/comments/labels, alternate grammars and bad-escape recovery remain distinct.
5. Owned source, canonical work/cancellation and capture/depth/expansion ceilings stay shared.

## Task 1: native-first regression contract

**Files:** Create tests/Pcre/PcreEscapedUtfLiteralReferenceTests.cs.
**Interfaces:** Existing compiler/options only; native literal captures/labels
and errors are independent expected values before managed comparison.

- [x] Preflight scalar, grammar, class/case, diagnostic, offset and resource profiles.
- [x] Write the literal native fixture with all five review boundaries; keep all260 old files fixed.
- [x] Publish tests-only draft PR244, inspect clean net8/9/10 RED with only missing-grammar failures and freeze fixture.

## Task 2: shared reader and pending-boundary conversion

**Files:** Modify src/Pcre/PcreByteParser.cs and PcreByteParser.BadEscapes.cs;
convert only pending assertions in tests/Pcre/PcreBadEscapeLiteralReferenceTests.cs
and PcreAltVerbNameReferenceTests.cs; update seven repository inventories.
**Interfaces:** Existing ParseEscapeCore(int,bool) delegates UTF non-ASCII entry
to existing ParseQuotedLiteral(); no new signatures or options.

- [x] Add the validated scalar-reader branch and remove the obsolete tail; update recovery comment.
- [x] Replace only the two temporary unsupported assertions with native acceptance checks for their same inputs and retain all unrelated checks.
- [ ] Include final inventories before source push, verify258 prior hashes/new frozen fixture/frozen35 and all18/full supporting gates.

## Task 3: review and delivery

- [ ] Whole-branch author review all five boundaries and the exact pending-test diff; no agents.
- [ ] Verify clean worktree and exact source/base/tree/proposed-merge identities.
- [ ] Record final evidence on PR244/issue7 footer, mark ready and retire only this plan's ledger; issue7 stays open.

## PR244: regression and integration evidence

Tests-only [RED739](https://github.com/uniblab/Icod.Expressions/actions/runs/43461757644), source `7198004337030e2d39008bc20843809b0fe5dc4d`, Linux ARM job132010183721: net8/9/10 each built with zero compiler warnings/errors and ran **687 expected failures / 81,040 passes / 81,727 total / zero skips**. Failures were619 primary profiles,35 later-error boundaries,24 offset theories,5 work/ownership cases,2 capacity/depth facts,1 quota and1 strict-subject fact per framework. All80,734 earlier tests and306 new controls passed; every native literal assertion passed. Freeze `1d6cf18667352f51c8d3a245560803af8fe1ab31` before production; no fixture refinements.

The993 new cases include956 primary native profiles (854 matching/query/capture/success-label and102 compile profiles),1,320 selected offset protocols and15 paired work/capacity profiles. Accepted native matching requires JIT0 and negative matching requires no-match(-1). Four UTF-C profiles outside native JIT support remain separate compile/unsupported controls.

Two earlier facts intentionally retire temporary escaped-e-acute UnsupportedSyntax assertions. Their same inputs and caller selections now require pinned native0:2 acceptance and managed parity, with every unrelated quote/C/directive/surrogate guard retained. Both facts remain; the other258 earlier test files stay hash-identical. The frozen new fixture also covers those exact source/caller selections, guarding against a masked regression. No invented caller flag or escape-recovery interpretation is introduced.

Task1 and Task2 integration are implemented. All seven inventories precede final source qualification. Final all18/supporting gates and Task3 author review/identities/footer/readiness are recorded on PR244/issue7 after observation, without a documentation-only CI run.
