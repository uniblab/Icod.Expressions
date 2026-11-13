# Extra Alternate Backslash Grammar Implementation Plan

Execute inline under the standing continuous workflow.
**Goal:** Honor default-false ExtraAltBsux through canonical literals.
**Architecture:** Extend one reader and flatten a temporary literal-brace marker
in AppendNode; no persistent syntax/IR/matcher changes.
**Tech stack:** C#13/net8/9/10; pinned local native research; managed CI RED/GREEN.
**Spec:** [design](PCRE-EXTRA-ALT-BSUX-DESIGN.md).

## Global constraints

Merged236 9d55f31241df69aa4262eecbd9a60dec3bbc2689;65,470 tests/framework; preserve
250 earlier test files, frozen35/214 outcomes/120 methods, GNU/BRE/ERE, public API,
dependencies/workflows/shared limits. UTF8+LF; no Python/subagents/SDK installs.
Reuse isolated worktree; user merges/tags/publishes; Grep last. Surrogate-escape
allowance and alternate verb-name decoding remain separate increments.

## Review focus

1. Malformed braces are literal and quantifiers bind only the brace, even with trivia.
2. Byte/UTF bounds require complete tokens; uint overflow preserves native early errors.
3. Leading zeros, scalar widths/casing and diagnostic ownership survive all class contexts.
4. Inert/produced text and caller resets never rewrite syntax or disable caller policy.
5. Source/options ownership, cancellation/work/quotas and earlier prohibitions remain shared.

## Task 1: native-first regression contract

Create tests/Pcre/PcreExtraAltBsuxReferenceTests.cs. Independently pin native
spans/captures/labels, compile errors/offsets and strict UTF subject errors; cover
all four caller selections, fallback quantifiers/trivia, scalar/overflow bounds,
class grammar and inert contexts. Add all256 values, offset/newline/fragment,
ownership/work/cancellation/quota/precedence controls. Use optional reflection
for the absent new property so tests-only RED compiles. Native compile arrays
and error assertions precede managed comparisons.

- [x] Preflight every native profile; classify negative match results explicitly,
  deduplicate diagnostic rows and compute expected case/failure counts.
- [ ] Publish tests-only draft, observe net8/9/10 RED with0compiler warnings/errors
  and no native/default/earlier-control failures; freeze before production.

## Task 2: parser integration

- [ ] Add bool ExtraAltBsux init property in src/Pcre/PcreByteOptions.cs.
- [ ] Snapshot extraAltBsux in src/Pcre/PcreByteParser.cs; altBsux is either flag.
  Pass inBracket to ParseAltBsuxEscape(byte value,int offset,bool inBracket).
- [ ] Extend src/Pcre/PcreByteParser.AltBsux.cs with
  ParseBracedAltBsuxEscape(int offset,bool inBracket) and private
  ParsedAltBsuxLiteralBrace marker. Use uint high-nibble overflow before shifting,
  cancellation per digit, complete-token bounds and separate malformed fallback.
- [ ] In src/Pcre/PcreByteParser.Groups.cs AppendNode, flatten the marker into
  canonical case-scoped u and brace before existing quantifier handling.
- [ ] Qualify all18 native results/full/frozen suites plus existing Unicode,
  workloads, extraction/POSIX/API/corpus/frozen35/package/three smokes/OS scripts.

## Task 3: review and delivery

- [ ] Author review all five boundaries; independent review excluded by user.
- [ ] Update all five inventories/status and issue7 footer, preserving its contract.
- [ ] Verify hashes, clean tree, base/head/tree/proposed-merge identities and GREEN;
  record final evidence on PR/issue7, mark ready and remove only this ledger.

Native preflight:940 primary profiles (730 spans/labels,204 compile diagnostics,
6 strict-UTF subject errors);2,304 supplemental compile profiles include2,297 JIT0
accepts and7 expected rejects. All17,040 selected offset protocols return match or
real no-match(-1), never other negative errors. All primary rows are unique.
Native extra-off comparison has718 differences;6 already share the same managed
Perl InvalidSyntax0, predicting712 primary failures plus37 supplemental failures.
Expected RED749fail/65,700pass/66,449total/0skip;979 new cases.
Merged236 main43382670308/scripts43382670284 are now green.
