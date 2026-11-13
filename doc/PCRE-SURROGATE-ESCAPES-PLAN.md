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

## PR239: optional surrogate escape code points

Default-false internal AllowSurrogateEscapes implements PCRE2 10.48 extra-option1
for escaped D800..DFFF in UTF8. The parser snapshots the flag and gates the three
existing surrogate checks in ordinary braced hex/octal/N and standard/extra u
grammar. Byte limits, values above10FFFF, malformed token syntax, uint overflow,
source offsets and prior caller restrictions retain their existing paths. Adjacent
surrogates remain separate code points; reset scopes do not clear caller policy.

Canonical integer literals/ranges retain widths, complements and class algebra.
Surrogate code points fold to themselves; valid Unicode17 mappings and other
out-of-range checks are unchanged. Existing class closure excludes the surrogate
gap from valid scalar membership. Literal-only assertion optimization falls back
to the existing general assertion path for non-Rune literals, avoiding invalid
byte assertions while retaining ordinary ahead/behind width and capture rules.

PcreUtf8.EncodeCodePoint writes native three-byte surrogate data only for decoded
labels and existing COMMIT first-code-unit metadata. All other values delegate
to Rune encoding. Strict source/subject TryRead is unchanged; accepting an escape
never makes raw surrogate bytes valid UTF input. Strict subjects retain errors,
and tolerant subjects retain malformed fragment barriers. NO_UTF_CHECK execution,
UTF16 and failed/partial labels remain outside this increment.

Eight production files change: PcreByteOptions.cs, PcreByteParser.cs,
PcreByteParser.AltBsux.cs, PcreByteParser.Groups.cs,
PcreByteParser.AltVerbNames.cs, PcreUtf8.cs, PcreUtf8CaseFold.cs and
PcreByteProgram.cs. No canonical AST/IR shape/opcode/matcher/journal/shared-limit/
public API/dependency/workflow changes. All253 earlier test files and frozen35/
214 outcomes/120 methods remain fixed.

The1,250 new cases include1,229 unique pinned native profiles:762 byte-span/
capture/success-label profiles,437 compile diagnostics and30 strict-subject errors.
Supplemental preflight verifies6,144 JIT0 label encodings (all2,048 surrogates in
three modes), three caller-restriction errors and2,664 selected offset protocols.
Coverage includes escape spellings and numeric bounds, classes/ranges/algebra/
negation, erased/unreachable/repeated bodies, assertions and widths, nonpairing,
casing/UCP/Turkish/reset scope, raw/decoded/inert labels, caller/UTF priority,
source/options ownership and shared work/cancellation/capture/depth/expansion.
Native error and span/label assertions precede managed comparison; only -1 is
classified as no-match. All-surrogate label fingerprint:
F268065A0C6B161BC958261B53D01F16174B8107640F79B8C5A262DAF1E0690E.

Refined tests-only [RED723](https://github.com/uniblab/Icod.Expressions/actions/runs/43402553616),
source846c85ee20f193dd02c8d857343c0666508e0c38, LinuxARM job131809003628,
built net8/9/10 without compiler warnings/errors. Each ran69,275 tests:572 expected
managed failures,68,703 passes,0skips. Failures were520 span/label profiles,
30 strict-subject profiles,3 reverse-range diagnostic owners,3 fingerprint
theories,12 offset theories and4 resource/ownership/restriction facts. Native,
default and earlier controls passed. Before valid RED, the unfrozen fixture
preserved existing outer-expression ownership for Perl/UTS algebra diagnostics;
all cases and native expectations were retained. Freeze fixture
751bd09a727ca5ea584b48527538f5d77ccddd07 before production; it is unchanged.

Qualification requires all18 native results at69,275 tests/framework and every
configured supporting gate. Repo evidence precedes the qualification commit;
final GREEN, author review and exact base/head/tree/proposed-merge identity live
on [PR239](https://github.com/uniblab/Icod.Expressions/pull/239) and issue7's roadmap
footer without another documentation-only CI run. Remaining syntax/modes/
directives/callouts, failed/partial labels, wider optimizer/resources (#102),
UTF16/exported API and realistic consumer/release gates remain open. User owns
merge/tag/publication; Grep last. See [design](PCRE-SURROGATE-ESCAPES-DESIGN.md)
and [plan](PCRE-SURROGATE-ESCAPES-PLAN.md).
