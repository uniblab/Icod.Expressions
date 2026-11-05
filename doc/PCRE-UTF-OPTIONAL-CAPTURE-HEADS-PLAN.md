# Optional capture heads implementation plan

Execute inline; no subagents. Spec: PCRE-UTF-OPTIONAL-CAPTURE-HEADS-DESIGN.md.
Goal: reuse bounded mandatory-body proof for finite optional capturing groups.
C#13/net8/9/10, baseline27282/frozen214/35source blobs unchanged; all spec limits
and constraints apply. Review focus: absent/rollback/last-iteration captures;
numbered/named/branch-reset dependencies; failed-proof fallback budgets; PLUS/
star/capture-ancestor owners; UTF caller boundaries and resource rejection.

## Task1: native corpus and tests-first draft

Files: new tests/Pcre/PcreUtfOptionalCaptureHeadsReferenceTests.cs and this spec/
plan. Consume pinned PCRE.NET1.6.0/PCRE2 10.48 oracle; produce independent literal
capture expectations and protocol/work/cancellation/count/expansion controls.

- [ ] Qualify native JIT sizes/bounded spans and literal captures; generate the
  new test class without changing any baseline test or production source.
- [ ] Publish tests/design/plan draft and observe successful-build RED confined
  to the new class, with all27282 baseline cases passing and no compiler warnings/
  errors. Freeze the test blob after this observation.

## Task2: capturing-group body selection

File: src/Pcre/PcreByteProgram.cs. Existing interface:
HasUtfCrLfOptionalGroupBitmap(PcreByteNode,ref int)->bool. Consume the existing
HasUtfCrLfMandatoryBodyBitmap(PcreByteAlternatives,ref int) worklist.

- [ ] Select a body for capture or noncapture nodes; keep the original noncapture
  single-leaf path/charges and use the bounded success-only checkpoint otherwise.
- [ ] Verify immutable new tests and complete baseline suite; append four PCRE
  inventories and one recent-progress row, retaining all earlier history.

## Task3: qualification and review boundary

- [ ] Author whole-branch review; verify source/test blobs, baseline expectations,
  whitespace, historical prefixes and exact remote tree/allowed paths.
- [ ] Audit six native jobs/net8/9/10/full/frozen214, zero failures/skips/compiler
  warnings/errors, all configured gates and seven equal Linux workloads.
- [ ] Update only issue102 recent footer; record exact RED/GREEN evidence in PR
  metadata, mark ready and leave unmerged for user review. No merge/tag/publish.
