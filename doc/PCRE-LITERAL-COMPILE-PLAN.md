# Literal Compilation Implementation Plan

> Execute inline with executing-plans under the standing no-agents/no-pauses workflow.

**Goal:** Add native caller literal compilation using canonical literal nodes.
**Architecture:** Check caller compatibility, suppress pattern prefix/grammar
interpretation and reuse the existing scalar reader, wrappers and matcher.
**Tech Stack:** C#13, net8/net9/net10, xUnit and pinned PCRE2 10.48 oracle only.
**Spec:** doc/PCRE-LITERAL-COMPILE-DESIGN.md.

## Global constraints

Baseline244:81,727tests/framework/261 prior files. Preserve every prior hash,
GNU/BRE/ERE, frozen35/214outcomes/120methods, IR/opcodes/matcher/journal/shared
limits/API/dependencies/workflows. UTF8+LF; no Python/agents/SDK installs. Reuse
linked worktree; user owns merge/tag/publish; issue7 open/Grep last. All repository
docs precede qualification; final evidence only PR245/issue7 footer, no docs-only CI.

## Review focus

1. Every source token/prefix is data and cannot alter caller modes or capture/group state.
2. Byte/scalar boundaries, case policies, word/line and newline/BSR contexts stay shared.
3. Literal incompatibility precedes source/other policy errors; UTF and Turkish priorities remain.
4. Empty/large punctuation and embedded quote/NUL/control data do not enter regex grammar.
5. Owned source/options, canonical work/cancellation and compiled resource ceilings stay shared.

## Task 1: native-first contract

**Files:** Create tests/Pcre/PcreLiteralCompileReferenceTests.cs.
**Interfaces:** Optional reflection selects future Literal; native flag0x02000000,
four supported extras and rejected exposed options pin independent results.

- [ ] Preflight matching, all-octet, error, offset, canonical work and structural profiles.
- [ ] Write literal native spans/errors and all five review boundaries; keep prior261 files fixed.
- [ ] Publish tests-only draft PR245, inspect clean net8/9/10 RED with only missing-policy failures, freeze fixture.

## Task 2: options, settings and literal parser

**Files:** Modify src/Pcre/PcreByteOptions.cs, PcreByteCompiler.cs,
PcreByteStartSettings.cs, PcreByteParser.cs; create PcreByteParser.Literal.cs;
update seven repository inventories.
**Interfaces:** bool Literal init; internal bool HasLiteralOptionConflict;
readonly parser literal snapshot; private PcreByteAlternatives ParseLiteralPattern().

- [ ] Add Literal and explicit native compatibility predicate; guard after existing cancellation/work-limit checks and before source settings/validation.
- [ ] Skip uninterrupted prefix loop for literal mode; snapshot parser flag and dispatch before leading options/trivia.
- [ ] Parse all original bytes/scalars into canonical literals with current case flags and existing wrappers; preserve shared resources.
- [ ] Include inventories before source push; verify all18/full supporting gates, prior261 hashes and frozen fixture/source identities.

## Task 3: review and delivery

- [ ] Author review all five boundaries and the complete allowed/rejected-option list; no agents.
- [ ] Verify clean worktree and exact source/base/tree/proposed-merge identities.
- [ ] Record final evidence on PR245/issue7 footer, mark ready and retire only this plan's ledger.
