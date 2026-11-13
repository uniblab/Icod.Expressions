# Surrogate Escapes Implementation Plan

Execute inline under the standing continuous workflow.
**Goal:** Accept opt-in escaped surrogate code points without relaxing UTF input.
**Architecture:** Retain canonical integer literals and ranges, gate existing
escape checks, and use existing general assertions for non-scalar literals.
**Tech stack:** C#13/net8/9/10, pinned PCRE2 10.48 oracle, managed CI RED/GREEN.
**Spec:** [design](PCRE-SURROGATE-ESCAPES-DESIGN.md).

## Global constraints

Merged238 c17ea37489f5a550f7952117db11fa1141125f2f/tree337ea771449a3ee4f2c761958f6ef4a7f3919536;
68,025 tests/framework,253 earlier test files. Preserve frozen35/214 outcomes/
120 methods, GNU/BRE/ERE, AST/IR shape/opcodes/matcher/journals/shared limits/API/
dependencies/workflows. UTF8+LF; no Python/subagents/SDK installs. Reuse linked
worktree. User merges/tags/publishes; Grep last. NO_UTF_CHECK, UTF16, failed labels
and broader optimizer #102 remain separate gates.

## Review focus

1. Surrogate acceptance must never admit malformed source or subject UTF bytes.
2. Class negation/ranges/algebra and caseless literals retain correct valid-scalar membership.
3. Literal assertions, including behind widths and erased bodies, must not pass invalid bytes to scalar decoding.
4. Labels retain native three-byte identity without combining adjacent surrogate escapes.
5. Numeric bounds/overflow, policy precedence, reset scopes and shared ownership/resources stay intact.

## Task 1: native-first regression contract

**Files:** Create tests/Pcre/PcreSurrogateEscapeReferenceTests.cs.
**Interfaces:** Optional reflection sets future AllowSurrogateEscapes for tests-only
RED. Native PcreRegex8Bit uses extra-option1. Options and Reference helpers map
the same explicit mode bits; all expectations come from pinned native records.

- [ ] Preflight primary native success/compile/strict-subject profiles, all2048
  label encodings, selected offset protocols, bounds and inert/caller controls.
- [ ] Write pinned span/capture/success-label and native error/owning-offset tests,
  plus all-surrogate fingerprints, offsets, work/cancellation, source/options
  ownership, structural quotas and whole-pattern UTF priority.
- [ ] Publish tests-only draft PR239. Observe net8/9/10 RED with zero compiler
  warnings/errors and only missing-policy managed failures, then freeze fixture.

## Task 2: canonical integration

**Files:** Modify src/Pcre/PcreByteOptions.cs, PcreByteParser.cs,
PcreByteParser.AltBsux.cs, PcreByteParser.Groups.cs,
PcreByteParser.AltVerbNames.cs, PcreUtf8.cs, PcreUtf8CaseFold.cs and PcreByteProgram.cs.
**Interfaces:** Add bool AllowSurrogateEscapes init property and readonly parser
allowSurrogateEscapes. Add internal static int PcreUtf8.EncodeCodePoint(int value,
Span<byte> destination), returning encoded width; valid values delegate to Rune,
surrogates write ED/continuation bytes. Source/subject TryRead remains strict.

- [ ] Gate the three existing UTF surrogate checks with !allowSurrogateEscapes;
  preserve all preceding numeric syntax and range decisions.
- [ ] Treat D800..DFFF as identity in FoldUnrestricted, preserving other bounds.
- [ ] In FinishAssertion, fall back to FinishGeneralAssertion for invalid Rune
  literals. Reuse EncodeCodePoint only in label scalar writing and existing
  COMMIT first-byte proof. No new AST/IR/runtime instructions or matcher flags.
- [ ] Include design/matrix/resource/consumer/status inventory updates before
  the production qualification commit. Run all18 native results at the new
  total and every configured supporting gate; inspect all outputs.

## Task 3: review and delivery

- [ ] Author review all five boundaries and whole branch; no independent agents.
- [ ] Verify frozen new fixture,253 earlier test hashes, frozen35, clean tree and
  exact source/base/tree/proposed-merge identity.
- [ ] Record final GREEN and review on PR239 and only issue7's roadmap footer;
  mark ready and remove this plan's temporary ledger. User owns merge.
