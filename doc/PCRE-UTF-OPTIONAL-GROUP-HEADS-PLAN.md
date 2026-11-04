# Optional group body heads implementation plan

Execute inline; no subagents. Spec: PCRE-UTF-OPTIONAL-GROUP-HEADS-DESIGN.md.
Goal: preserve native UTF CRLF bitmap entry after finite optional mandatory-head
group bodies, using the existing bounded compile-time proof.

## Global constraints and review focus

All spec constraints apply. C#13/net8/9/10; no installations or dependencies;
baseline26465/frozen214/35source blobs unchanged. Keep limits256/256/4096/256/
1024/1000000/1000/12. Check: mixed unsupported branches cannot inherit a good
branch's bitmap; consumed-prefix captures/tail failure and scoped options restore;
failed speculative proofs preserve nullable fallback budgets; wrapper/PLUS/star
owners remain exact; malformed UTF, caller offsets and resource limits persist.

## Task1: native corpus and tests-first draft

Files: new tests/Pcre/PcreUtfOptionalGroupHeadsReferenceTests.cs and this spec/plan.
Consumes pinned PCRE.NET1.6.0/PCRE2 10.48 source/native oracle; produces literal
capture fixtures plus protocol/work/cancellation/count/expansion tests.

- [x] Qualify native spans/JIT sizes and bounded captures; generate immutable
  literal expectations independent of managed production code.
- [x] Publish tests/design/plan draft. Run merged six-platform CI; successful
  build RED must be confined to the new class, with all26465 baseline cases
  passing and zero compiler warnings/errors. Freeze the new test blob.

## Task2: shared mandatory-body proof

File: src/Pcre/PcreByteProgram.cs. Consume existing leaf guards and body worklist;
produce HasUtfCrLfMandatoryBodyBitmap(PcreByteAlternatives,ref int)->bool.

- [x] Extract the existing assertion body loop unchanged into the shared helper.
- [x] Keep the old single-leaf group path exact; try the shared helper for other
  noncapturing bodies with a local visit checkpoint, committing only success.
- [ ] Verify the immutable tests pass and all baseline tests stay green. Commit
  source and append four inventories plus one recent-progress row.

## Task3: qualification and review boundary

Files: four PCRE inventories and PCRE-UTF-RECENT-PROGRESS.md; issue102 footer.

- [ ] Author whole-branch review; whitespace, baseline/source/test blob and
  history checks; publish and verify exact remote tree and allowed path set.
- [ ] Audit all six native jobs, three TFMs/full/frozen214, zero failures/skips/
  compiler warnings/errors, all configured gates and seven equal Linux workloads.
- [ ] Update only issue102 recent footer; record complete RED/GREEN evidence in
  PR metadata; mark ready and leave unmerged for user review. No merge/tag/publish.

Observed RED: run518/Linux x64, all three TFMs;545new-class failures/26737passes/
0skips; all26465baseline+272newcontrols pass; compiler warnings/errors0. Test blob
a35d52ccb3df261d4b58d3b22ac3711c4c86da2d is frozen. Final qualification is in PR162.
