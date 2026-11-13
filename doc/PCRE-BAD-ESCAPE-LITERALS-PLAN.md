# Bad Escape Literal Recovery Implementation Plan

Execute inline under the standing continuous workflow.
**Goal:** Implement native opt-in literal recovery without changing successful escapes.
**Architecture:** A gated transactional escape wrapper reuses the existing reader;
a local native-number grammar check distinguishes recoverable syntax from limits.
**Tech stack:** C#13/net8/9/10, pinned PCRE2 10.48, managed CI RED/GREEN.
**Spec:** [design](PCRE-BAD-ESCAPE-LITERALS-DESIGN.md).

## Global constraints

Merged239 6e0d56f3962100f0f48657d9d7af61d203d46ad3/tree25c5d9d3c4095f12e45f233234cf862758d2b755;
69,855 tests/framework,255 earlier test files. Preserve GNU/BRE/ERE, frozen35/
214 outcomes/120 methods, canonical AST/instruction types/opcodes, matcher,
journals/shared limits/public API/dependencies/workflows. UTF8+LF; no Python,
subagents or SDK installs. Reuse linked worktree, execute inline. User owns
merge/tag/publication; Grep last. Existing unsupported boundaries stay explicit.

## Review focus

1. Recovery consumes only the escape letter; following braces, groups and scopes retain native ownership.
2. Native-invalid syntax must recover while valid unresolved or library-over-limit references stay errors.
3. Perl class and verb-name suppression cannot leak into following source; UTS recovery stays available.
4. Failed reads leave no pending reference/subroutine state or source lifetime dependency.
5. Successful escapes, source UTF/caller guards, cancellation and resource diagnostics are unchanged.

## Task 1: regression contract

**Files:** Create tests/Pcre/PcreBadEscapeLiteralReferenceTests.cs.
**Interfaces:** Optional reflection sets future BadEscapeIsLiteral for executable
tests-only RED; native extra-option2 selects the oracle policy. Explicit mode
bits cover current caller restrictions, numeric/alternate policies and contexts.

- [ ] Preflight native matching/error profiles and supplemental offset protocols;
  classify no-match(-1), unsupported native JIT paths and deliberate library limits.
- [ ] Write pinned results/errors and controls for all five review boundaries,
  plus all-octet comparisons, work/cancellation, source/options ownership and quotas.
- [ ] Publish tests-only draft PR240, inspect net8/9/10 RED with zero compiler
  warnings/errors and only missing-policy failures, then freeze the fixture.

## Task 2: parser integration

**Files:** Modify src/Pcre/PcreByteOptions.cs, PcreByteParser.cs,
PcreByteParser.AltVerbNames.cs and PcreByteParser.ClassAlgebra.cs;
create src/Pcre/PcreByteParser.BadEscapes.cs.
**Interfaces:** Add bool BadEscapeIsLiteral init property, readonly parser
badEscapeIsLiteral, scoped bool suppressBadEscapeLiteral, and
ParseEscape(int offset,bool inBracket=false,bool allowBadLiteral=true).
Rename the old method ParseEscapeCore(int offset,bool inBracket=false).
Private CanLiteralizeBadEscape(int start,bool inBracket,PcreByteParseException error)
and InvalidNativeReferenceEscape(int start) inspect only local cursors/state;
NativeDecimalReferenceOverflow(int start) handles bare reference overflow.

- [ ] Snapshot the new caller option, add the gated wrapper and pure eligibility
  probes; rollback index and pending reference/subroutine state on recovery only.
- [ ] Suppress recovery in ParsePerlClass with finally restoration; name decoding
  calls ParseEscape with allowBadLiteral:false. Preserve the old core false path.
- [ ] Update design/matrix/resource/consumer/status inventories before final
  qualification. Run all18 native results and every configured supporting gate;
  inspect outputs and preserve frozen fixtures/earlier hashes.

## Task 3: review and delivery

- [ ] Author review all five boundaries and whole branch; no independent agents.
- [ ] Verify clean tree, frozen new fixture/255 earlier test hashes and exact
  source/base/tree/proposed-merge identity.
- [ ] Record final GREEN/review in PR240 and only issue7's roadmap footer, mark
  ready and remove only this plan's temporary ledger. User owns merge.
