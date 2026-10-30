# Internal PCRE special Unicode properties implementation plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox syntax for tracking.

**Goal:** Support Any/Xan/Xps/Xsp/Xwd/Xuc in UTF atoms and ordinary classes.

**Architecture:** Reuse existing UnicodeAlnum/Space/Word predicates and add two
constant-membership kinds. Keep special lookup separate from category-prefixed
names and preserve scalar instructions, predicates, metadata and resource paths.

**Tech Stack:** C#13, net8/net9/net10, xUnit, PCRE.NET1.6.0 / PCRE2 10.48 JIT.

**Spec:** `doc/PCRE-SPECIAL-PROPERTIES-DESIGN.md`.

## Global constraints

- Internal UTF profile; no API/package/dependency/workflow/release/consumer change.
- C#13, net8/net9/net10, UTF-8 LF; root project/solution, src, tests, samples; no Python.
- Depth256/captures256/instructions4096/marks256/savedframes1024/default work1,000,000.
- Existing work/cancellation/scalar predicate/journal paths; no range expansion/saved spans.
- Existing category-prefix profile extension and byte-mode gate remain; user owns merge.
- No local SDK; managed red/green uses six native Staging jobs and full/frozen suites.

## Review focus

- Any includes NUL/noncharacters/newlines without DotAll or explicit-newline metadata.
- Xuc excludes ASCII letters/DEL/C1 while including only dollar/at/grave below A0.
- Xwd includes Mn/Pc while excluding Mc/Me; both space aliases include180E/85.
- Explicit properties must remain Unicode under local ASCII/UCP/caseless options.
- New bare properties must not become valid through the existing category-prefix extension.

## Task 1: Native contracts, fixtures and expected red

**Files:** Create tests/Pcre/PcreSpecialPropertyReferenceTests.cs and this design/plan;
create tests/Pcre/PcreClassNewlineMetadataReferenceTests.cs for UTF source metadata;
modify tests/Pcre/PcreUtf8GeneralCategoryTests.cs obsolete Any/Xan pending rows;
update current PCRE design/syntax/resource/consumer inventories.

**Interfaces:** Existing compiler/options/match results, native PcreRegex8Bit and
scalar predicate behavior; produce native-first literal captures and diagnostic/matrix tests.

- [ ] Research exact pinned memberships, names/options/metadata and builder-check actual docs.
- [ ] Write stable scalar/boundary/NUL/noncharacter/complement/union literal outcomes,
  loose names/caret/P, namespace/byte/range diagnostics, i/r/ASCII/UCP independence,
  captures/retry/calls/assertions/widths/CRLF/invalid UTF/G/offset and resource tests.
- [ ] Pin UTF range-interior, negated duplicate/mixed predicate, equal-range singleton,
  quoting/xx and erased/DEFINE metadata regressions with original byte spans.
- [ ] Remove only obsolete pending Any/Xan rows; retain unknown/script/binary gates.
- [ ] Verify all fixture expectations against saved native probes; whitespace/frozen35.
- [ ] Commit/publish exact tests/docs tree, open draft PR, observe expected managed
  missing-feature failures after native assertions before any production edit.

## Task 2: Resolve special names and reuse scalar predicates

**Files:** Modify src/Pcre/PcreByteParser.cs, src/Pcre/PcreByteSyntax.cs and
src/Pcre/PcreByteMatcher.cs plus src/Pcre/PcreByteProgram.cs metadata lookup.
Scalar instructions/width/frame paths remain shared.

**Interfaces:** Add AnyScalar and UniversalCharacterName scalar-class enum kinds;
SpecialProperty(string name,bool negated) returns PcreByteScalarClass?. Existing
GeneralCategory/string normalization and scalar instruction payload remain.
PcreByteScalarSet gains bool ExplicitNewline, supplied from original UTF class
operands before folding; byte class path stays unchanged.

- [ ] Normalize the complete braced token before one leading caret; retain malformed
  name handling and distinguish category-prefixed tokens before special lookup.
- [ ] Map Any/Xuc to new kinds and Xan/Xps/Xsp/Xwd to existing Unicode predicates.
- [ ] Add direct AnyScalar=true and UniversalCharacterName=$/@/grave/or>=A0 checks
  at the shared scalar predicate boundary (decoder already excludes surrogates).
- [ ] Track UTF source term count, sole-literal/equal-range shape and endpoint CR/LF;
  store class metadata with the native negated-singleton exception. Read that flag
  instead of inferring it from normalized range membership or all-negated suppression.
- [ ] Commit/publish exact tree; run six full/frozen Staging jobs and configured gates.
- [ ] Fresh independent whole-branch review against spec/plan/review focuses/ledger;
  address material findings with observed RED→GREEN and green suites.
- [ ] Inspect every platform log and completed-success run, exact reviewed head/tree,
  full-range whitespace/frozen35/clean state; update proof and mark ready.
