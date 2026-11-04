# Repeated skipped assertion UTF prefix implementation plan

> Execute inline; no subagents.

**Goal:** Prove required exact repeats of skipped assertions before UTF starter classes.
**Architecture:** One shared kind helper;bounded conservative copy cost in the leading walker.
**Tech:** C#13/net8/9/10,xUnit,PCRE.NET1.6.0/PCRE2 10.48 JIT.
**Spec:** PCRE-UTF-REPEATED-ASSERTION-PREFIX-DESIGN.md.

## Constraints and review focus
Spec constraints apply. Admit only Minimum==Maximum>0. Native investigation shows
optional copies can expose assertion bodies,so normalized unbounded/ranged/query/
star forms remain excluded. Guard before multiplying;
include outer possessive charge; keep zero-count behavior. Capture side effects
of repeated positive/non-atomic behind remain live. Skipped body opacity is
bitmap-only; later recovery remains blocked and prior root stars retain ownership.

## Task1: Native contract and observed RED
Files: tests/Pcre/PcreUtfRepeatedAssertionPrefixReferenceTests.cs;design/plan.
Interfaces: existing Compile/Match/PcreRegex8Bit JIT.
- [x] Pin native full captures for assertion kinds/counts/modifiers/captures,
  wrappers/mixed prefixes/alternatives,opaque predicates,PLUS/star endpoints and
  exclusions. Add six-newline/UCP/offset fragments,strict validation,resources,
  cancellation,long ordinary controls and an in-budget counted boundary fixture.
- [x] Publish tests-only draft; observe successful-build failures only in new
  class,all24621baseline pass. Freeze test blob before production changes.

## Task2: Share classification and budget every possible copy
Files: src/Pcre/PcreByteProgram.cs;four canonical inventories;recent progress index.
Interfaces: new private static bool IsUtfCrLfSkippedAssertion(PcreByteNode node);
FindUtfCrLfLeadingNode tuple/signature unchanged.
- [x] Extract the exact direct assertion list; direct cost2/retained=true stays.
  Positive exact repeats of those kinds require Maximum<=(1000-1)/2 before
  multiplication,cost2*Maximum+1/retained=true. Existing pre-addition/wrapper
  charging and all owner/source/alternative guards stay; zero-count path unchanged.
- [ ] Preserve tests/frozen35/whitespace;commit/exact remote verification/author
  whole-branch review. Audit six native Staging/full/frozen214/net8/9/10,
  configured gates/seven equal workloads,zero failures/skips/compiler warnings/
  errors;ready PR remains unmerged for user review.
- [ ] Append one recent row and change only issue102 footer after CI,retaining
  all earlier issue and preserved index history.

## Task3: Resolve four preserved optional class-body failures
Files: tests/Pcre/PcreUtfOptionalAssertionClassReferenceTests.cs;
src/Pcre/PcreByteProgram.cs;the same inventories/index above.
Interfaces: private bool HasUtfCrLfOptionalAssertionClass(PcreByteAlternatives body,
ref int visits);existing IsUtfCrLfOptionalSet reused unchanged.
- [x] Pin focused native class-body captures,all alternatives/groups/modifiers,
  CR/property/nullable/positive-ahead/lookbehind exclusions,PLUS/star endpoints,
  six-newline/UCP/offset fragments,strict validation and resource controls. Keep
  the original442 inputs/assertions;publish follow-up tests-only and observe RED.
- [x] Prove only non-exact negative general ahead with every body branch leading
  through groups to a mandatory supported scalar set. Worklist (Node?,Body?)
  charges pops/branches before enqueue,cancels,shares visits and rejects empty/
  unknown/CR/property heads. Bound Maximum before study/multiplication;successful
  proof cost2*Maximum+1/retained=true. Exact path unchanged;no runtime changes.
- [ ] Update counts/native evidence and disclose run504/expanded scope. Preserve
  test inputs/assertions and disclose the task4 oracle-mode correction;run every
  original final gate and leave ready PR unmerged.

## Task4: Correct the native-only Windows ARM64 boundary failure

- [x] Isolate the count500 failure with sequential tracing, then separate native
  and managed probes. Managed passes; native PCRE.NET JIT aborts (run511).
  Track the reproduction and unconfirmed SLJIT instruction lead in issue159.
- [x] Preserve all original test inputs/assertions. Only the counted499/500/1001
  ordinary controls use the native interpreter on all platforms; all bitmap
  cases and long sequential controls keep JIT. Disclose the original blob change.
- [x] Remove temporary diagnostic tests and restore the workflow byte-for-byte,
  including ordinary parallel scheduling. No dependency or engine limit change.
- [ ] Requalify full25373/frozen214 on net8/9/10 across all six native jobs,
  all configured gates and seven equal workloads; update review, PR and only
  issue102 footer after success. Leave issue159 and broader gates open.
