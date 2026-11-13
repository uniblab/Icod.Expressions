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

- [ ] Preflight scalar, grammar, class/case, diagnostic, offset and resource profiles.
- [ ] Write the literal native fixture with all five review boundaries; keep all260 old files fixed.
- [ ] Publish tests-only draft PR244, inspect clean net8/9/10 RED with only missing-grammar failures and freeze fixture.

## Task 2: shared reader and pending-boundary conversion

**Files:** Modify src/Pcre/PcreByteParser.cs and PcreByteParser.BadEscapes.cs;
convert only pending assertions in tests/Pcre/PcreBadEscapeLiteralReferenceTests.cs
and PcreAltVerbNameReferenceTests.cs; update seven repository inventories.
**Interfaces:** Existing ParseEscapeCore(int,bool) delegates UTF non-ASCII entry
to existing ParseQuotedLiteral(); no new signatures or options.

- [ ] Add the validated scalar-reader branch and remove the obsolete tail; update recovery comment.
- [ ] Replace only the two temporary unsupported assertions with native acceptance checks for their same inputs and retain all unrelated checks.
- [ ] Include final inventories before source push, verify258 prior hashes/new frozen fixture/frozen35 and all18/full supporting gates.

## Task 3: review and delivery

- [ ] Whole-branch author review all five boundaries and the exact pending-test diff; no agents.
- [ ] Verify clean worktree and exact source/base/tree/proposed-merge identities.
- [ ] Record final evidence on PR244/issue7 footer, mark ready and retire only this plan's ledger; issue7 stays open.
