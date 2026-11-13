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
