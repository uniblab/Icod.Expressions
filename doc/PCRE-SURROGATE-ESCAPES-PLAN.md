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

- [x] Preflight primary native success/compile/strict-subject profiles, all2048
  label encodings, selected offset protocols, bounds and inert/caller controls.
- [x] Write pinned span/capture/success-label and native error/owning-offset tests,
  plus all-surrogate fingerprints, offsets, work/cancellation, source/options
  ownership, structural quotas and whole-pattern UTF priority.
- [x] Publish tests-only draft PR239. Observe net8/9/10 RED with zero compiler
  warnings/errors and only missing-policy managed failures, then freeze fixture.

## Task 2: canonical integration

**Files:** Modify src/Pcre/PcreByteOptions.cs, PcreByteParser.cs,
PcreByteParser.AltBsux.cs, PcreByteParser.Groups.cs,
PcreByteParser.AltVerbNames.cs, PcreUtf8.cs, PcreUtf8CaseFold.cs and PcreByteProgram.cs.
**Interfaces:** Add bool AllowSurrogateEscapes init property and readonly parser
allowSurrogateEscapes. Add internal static int PcreUtf8.EncodeCodePoint(int value,
Span<byte> destination), returning encoded width; valid values delegate to Rune,
surrogates write ED/continuation bytes. Source/subject TryRead remains strict.

- [x] Gate the three existing UTF surrogate checks with !allowSurrogateEscapes;
  preserve all preceding numeric syntax and range decisions.
- [x] Treat D800..DFFF as identity in FoldUnrestricted, preserving other bounds.
- [x] In FinishAssertion, fall back to FinishGeneralAssertion for invalid Rune
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

## PR239: optional surrogate escapes and tolerant literals

Default-false internal AllowSurrogateEscapes implements PCRE2 10.48 extra-option1.
The parser snapshots it and gates three existing UTF surrogate checks in ordinary
braced hex/octal/N and standard/extra u grammar. Byte limits, values above10FFFF,
malformed syntax, uint overflow, owning offsets and caller restrictions retain
their existing paths. Adjacent surrogates remain separate code points; caller
policy survives reset scopes. Canonical integer literals/ranges retain widths,
negation and algebra. Surrogate folds are identity; valid Unicode17 mappings and
other out-of-range guards are unchanged. Non-Rune literal assertions use existing
general assertion lowering. EncodeCodePoint writes native surrogate bytes for
decoded labels and existing COMMIT first-code-unit metadata only.

Source and strict-subject TryRead validation remain unchanged. Native JIT also
allows encoded surrogate bytes through explicit literal comparisons in tolerant
mode while general classes/types reject them. A derived HasSurrogateLiterals
program property records emitted Literal instructions. Positive ordinary classes
with original Source=Literal surrogate singleton/equal-end range use the existing
Literal opcode; duplicate/range/algebra classes retain their typed paths. Only
when MatchInvalidUtf and this property are true, TryReadLiteral admits complete
ED/A0..BF/80..BF units for fragment scanning, literal comparison, nullable entry
and assertion rewind/retry positions. General scalar/class/type readers remain
strict. FF, truncated encodings and other malformed bytes remain hard barriers.
Labels and erased literals do not activate this path. Work/cancellation and all
journals/limits remain shared; no new opcode, instruction type or matching engine.

**Explicit boundary:** compiled tolerant surrogate literals combined with
WordBoundary, Backreference or ScriptRunCheck return UnsupportedSyntax at byte0.
These native malformed-context rules need separate qualification. The bounded
post-compilation scan polls cancellation within the existing4,096-instruction
ceiling. Strict UTF, label-only names and erased literals retain those constructs.
This deliberate restriction applies only to the new tolerant literal path.
NO_UTF_CHECK execution and UTF16 remain separate gates.

Nine production files change: PcreByteOptions.cs, PcreByteParser.cs,
PcreByteParser.AltBsux.cs, PcreByteParser.Groups.cs,
PcreByteParser.AltVerbNames.cs, PcreUtf8.cs, PcreUtf8CaseFold.cs,
PcreByteProgram.cs and PcreByteMatcher.cs. The program gains one derived metadata
property; AST/instruction types, opcodes, journals, shared limits, public API,
dependencies and workflows are unchanged. All253 earlier test files and frozen35/
214 outcomes/120 methods remain fixed.

The1,830 new cases include1,799 unique primary native profiles:1,332 matching or
explicit-boundary/strict controls,437 compile diagnostics and30 strict-subject
errors. Supplemental preflight checks10,240 JIT0 profiles (all2,048 label encodings
in three modes and all2,048 literal byte matches in two modes), three restriction
errors and4,500 selected offset protocols. Coverage includes numeric spellings/
bounds/priority, singleton versus duplicate/range/algebra classes, erased and
repeated bodies, fixed/variable and non-atomic assertions, calls/graphemes,
nonpairing, casing/UCP/Turkish/reset scopes, raw/decoded/inert labels, ownership,
hard malformed barriers and shared work/cancellation/capture/depth/expansion.
All-surrogate label fingerprint remains
F268065A0C6B161BC958261B53D01F16174B8107640F79B8C5A262DAF1E0690E.

Tests-only [RED723](https://github.com/uniblab/Icod.Expressions/actions/runs/43402553616),
source846c85ee20f193dd02c8d857343c0666508e0c38, LinuxARM job131809003628,
built net8/9/10 without compiler warnings/errors: each572 expected failures,
68,703 passes,69,275total,0skips. Only new missing-policy cases failed; native,
default and earlier controls passed. The unfrozen fixture first retained existing
outer-expression ownership for Perl/UTS errors without changing native expectations.
Freeze751bd09a727ca5ea584b48527538f5d77ccddd07 before the initial implementation.

CI724 then exposed22 retained failures per framework (10 native literal spans and
12 offset theories): the initial design incorrectly treated surrogate encodings
as barriers even for direct native literal comparisons. Keep the fixture fixed.
Add separate tests-only [RED725](https://github.com/uniblab/Icod.Expressions/actions/runs/43406280480),
sourcef5b742ba0c4d40e496dd5f94dfb4bbfc3e277ec9, LinuxARM job131821296383.
All frameworks built without compiler warnings/errors: each201 expected failures,
69,654 passes,69,855total,0skips. The added164 span profiles,6 explicit guards,
2 all-value theories,6 offset theories and1 work fact failed with the original22.
Native/strict/erased/label/default/earlier controls passed. Freeze second fixture
444f833a7190b5a41a9f4cf229bae89f3f939b56 before the matcher correction. Both fixtures
remain unchanged.

Qualification requires all18 native results at69,855 tests/framework and every
configured supporting gate. All repository inventories precede the final source
head; final GREEN, author review and exact base/head/tree/proposed-merge identity
live on [PR239](https://github.com/uniblab/Icod.Expressions/pull/239) and issue7's
roadmap footer without another documentation-only CI run. Tolerant surrogate
word/reference/script combinations, remaining syntax/modes/directives/callouts,
failed/partial labels, wider optimizer/resources (#102), UTF16/exported API and
realistic consumer/release gates remain open. User owns merge/tag/publication;
Grep last. See [design](PCRE-SURROGATE-ESCAPES-DESIGN.md) and
[plan](PCRE-SURROGATE-ESCAPES-PLAN.md).
