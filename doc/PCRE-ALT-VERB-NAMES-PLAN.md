# Alternate Control-Verb Names Implementation Plan

Execute inline under the standing continuous workflow.
**Goal:** Honor default-false AltVerbNames through owned byte labels.
**Architecture:** One gated name decoder reuses canonical literal parsing and
shared trivia/newlines; existing NUL identity preserves argument presence.
**Tech stack:** C#13/net8/9/10; pinned native research and managed CI RED/GREEN.
**Spec:** [design](PCRE-ALT-VERB-NAMES-DESIGN.md).

## Global constraints

Merged237 f9b28d2a9a31342a46b3e92679d82ce11f8f5028;66,451tests/framework; preserve
251 earlier test files, frozen35/214 outcomes/120 methods, GNU/BRE/ERE, canonical
AST/IR/matcher/journals/shared limits/API/dependencies/workflows. UTF8+LF; no Python,
subagents or SDK installs. Reuse isolated worktree; user merges/tags/publishes;
Grep last. Surrogate allowance, failed/partial labels and other syntax stay separate.

## Review focus

1. Decoded-empty names remain named; true empty arguments retain native unnamed behavior.
2. Raw255/256 byte quotas include ignored/escaped text and follow earlier escape errors.
3. Escaped closes, local quotes and UTF bytes never re-enter regex syntax or case folding.
4. Non-data escapes and numeric/caller policies retain native label-specific priority.
5. Extended comments/newlines stay local; ownership/work/cancellation/quotas stay shared.

## Task 1: native-first regression contract

Create tests/Pcre/PcreAltVerbNameReferenceTests.cs with fixed native success label/
span/capture and compile-error/offset profiles. Cover all verbs and policies,
empty/NUL identity, syntax/trivia/quotes/scopes, raw length and error precedence.
Add owned source/options, exact work/cancellation and existing structural/UTF/
caller controls. Native arrays/error checks precede managed comparison; optional
reflection keeps tests-only RED executable before the new property exists.

- [x] Preflight all native profiles/protocols, deduplicate error rows independently
  of subjects, classify negative results and compute expected unique case counts.
- [x] Publish tests-only draft and validate net8/9/10 RED with0compiler warnings/
  errors and no native/default/earlier-control failures; freeze before production.

## Task 2: decoder integration

- [x] Add bool AltVerbNames init property in src/Pcre/PcreByteOptions.cs, snapshot
  readonly altVerbNames in src/Pcre/PcreByteParser.cs.
- [x] Gate ReadControlVerbName in src/Pcre/PcreByteParser.PathMarks.cs to
  ReadAlternateControlVerbName(string verb,bool requireName).
- [x] Add bool allowInlineComments=true to SkipIgnoredPatternText in
  src/Pcre/PcreByteParser.Extended.cs; only gate (?#...) skipping by that bool.
- [x] Create src/Pcre/PcreByteParser.AltVerbNames.cs: local quoting, shared trivia
  with inline comments disabled, raw-byte closure quotas, literal-only escape
  decode/UTF encoding and existing NUL representation for decoded-empty names.
  AppendControlVerbEscape(List<byte> name,int offset) accepts only literal kinds;
  AppendControlVerbScalar(List<byte> name,int scalar) encodes owned label bytes.
- [ ] Full18 native results and existing frozen/Unicode/workloads/extraction/
  POSIX/API/corpus/package/three smokes/three-OS script gates pass.

## Task 3: review and delivery

- [ ] Author review all five boundaries; no independent agents under user policy.
- [ ] Update design/matrix/resource/consumer/status inventories before qualification;
  update only issue7 roadmap footer after GREEN.
- [ ] Verify fixture/earlier hashes, clean tree and exact base/head/tree/proposed-
  merge identity; record final GREEN/review on PR/issue7, mark ready, remove only
  this ledger. User owns merge.

Native preflight:833 unique primary profiles (671 spans/captures/success labels,
162 compile diagnostics), plus1,137 supplemental compile profiles (1,130 JIT0
accepts/7 expected rejects) and6,240 selected offset protocols. Native no-match
labels are deliberately excluded from the implemented result contract. Predicted
RED650fail/66,657pass/67,307total/0skip:490 primary label/span differences,139
new diagnostics and21 supplemental failures. There are856 new managed cases.
Merged237 main43388625282/scripts43388625167 are now green.

Clean tests-only [RED718](https://github.com/uniblab/Icod.Expressions/actions/runs/43392128698),
source2d0953fd995c0783cfc06e5a3251630587077ae7, LinuxARM job131774325691,
built net8/9/10 with0warnings/errors. Each ran67,307 tests:650 expected managed
failures,66,657 passes,0skips. Failures comprise490 success-label/span profiles,
139 compile diagnostics and21 supplemental cases, all for missing name decoding;
native/default/earlier controls pass. Freeze fixture
44ef5f4a1ca903df65c2f48c3c02a1db0d7f55e5 before production.

Prequalification author source review checked all five boundaries: source-byte
quota/escape priority, local UTF/quotes/comments/casing, literal-only decode,
existing NUL-presence identity and shared ownership/resources. No canonical
syntax/IR/matcher/journal/API/limit changes. All repo docs precede qualification;
final whole-branch review, GREEN, identities/footer/readiness live on PR238/issue7
without a documentation-only CI repeat. Independent review excluded by user.

## Integration correction: leading MARK and STARTLINE

CI719 resolves all name decoder cases but exposes one frozen ANY/tolerant-UTF
caret/MARK split-offset theory. The raw canonical form has the same native rule:
JIT scan_prefix stops at OP_MARK, returning no first-character position, so
STARTLINE precedes FIRSTMAP. The existing pure instruction proof excludes MARK.
Keep the frozen name fixture unchanged and add PcreMarkStartLineReferenceTests.
Its704 pinned JIT profiles cover raw/decoded/empty labels, NEL/LS/PS split and
boundary offsets, capture/noncapture/atomic wrappers, empty bodies, casing/UCP,
byte/strict boundaries and leading-literal/non-multiline priority controls.
Fourteen more native profiles cover literal suffixes at the1,000-instruction
proof limit, over-budget valid-boundary fallback and full256-capture capacity.

Extend only HasLineStartSearch eligibility in PcreByteProgram.cs through private
HasLinearPathMarkStartBarrier(). Require at most MaximumStartClassVisits(1,000)
instructions and only Literal, PathMark, captures, StartLine/end anchors, atomic
boundaries and the final Accept. Require a PathMark before any Literal. Reject
all branches/loops/calls/assertions/classes/references/other verbs, and keep the
existing AST first-caret proof. Preserve literal-before-MARK priority. Do not
change opcodes, IR shape, matcher/journal/work/quotas. Broader #102 remains open.

- [x] Observe added MARK regression RED before compiler-metadata correction.
- [x] Add HasLinearPathMarkStartBarrier and requalify complete18-result matrix.


CI719 then cleared the decoder cases, exposing one frozen ANY/tolerant-UTF caret/
MARK split-offset theory. Native JIT scan_prefix stops at OP_MARK; no collected
character means STARTLINE precedes FIRSTMAP. The raw canonical name has the same
rule. PcreByteProgram.cs now adds HasLinearPathMarkStartBarrier beside the existing
reference proof. Require at most1,000 instructions, only literals/PathMark/
captures/line-end anchors/atomic boundaries/final Accept, a PathMark before any
consumption, and the existing AST first-caret proof. Reject branches/loops/calls/
assertions/classes/references/other verbs; retain literal-before-MARK priority.
No opcode, instruction shape, matcher/journal/work/quota changes. Wider #102 stays
open. Six production files change, including this bounded compile-time metadata.

PcreMarkStartLineReferenceTests adds718 cases:704 pinned JIT profiles for raw/
decoded/empty labels, NEL/LS/PS split/boundary offsets, wrappers/casing/UCP, empty
bodies and priority/byte/strict controls;14 supplemental profiles cover1,000-
instruction boundaries, over-budget valid-boundary fallback and256 captures.
Combined increment:1,574 new cases,1,537 primary native profiles,1,151 supplemental
compile profiles (1,144 JIT0/7 expected rejects), and6,240 offset protocols.

Integration tests-only [RED720](https://github.com/uniblab/Icod.Expressions/actions/runs/43396124768),
sourcef6424db14a3f18e655baf928c213ffd8f4e559f9, LinuxARM job131787334583,
built net8/9/10 with0warnings/errors. Each ran68,025 tests:367 expected failures,
67,658passes,0skips. All360 added split-offset profiles,4 literal-boundary cases,
2 capture-capacity cases and the original1 theory failed; all name/earlier/native/
priority/boundary controls passed. Freeze MARK fixture
1e238d6f3cdbd5d4a1b70420dd9a1d9debc341a8 before compiler correction. Both frozen
fixtures remain unchanged.

Source review confirms the name decoder is unchanged since CI719; the additional
compiler proof is linear, cancellation-aware and bounded by the existing ceiling.
All repo evidence precedes final qualification. Final all18 results, supporting
gates, whole-branch review and exact identities are recorded on PR238/issue7.
