# Escaped CR compile option implementation plan

Execute inline with superpowers:executing-plans. Spec:
[design](PCRE-ESCAPED-CR-DESIGN.md).
Goal: compile-time r-to-LF selection through the existing simple-escape path.
Architecture: one default-false caller bool, readonly parser copy and simple
literal selection; no source rewrite, typed node, opcode or runtime change.
Tech: C#13/net8/9/10; GitHub CI for managed RED/GREEN.

## Constraints and review focus

Preserve all earlier references, default behavior, GNU/BRE/ERE, 35 frozen and
214 frozen/120, exported API/dependencies. No Python, subagents, SDK installs or
runtime fallback. User owns merge/tag/publication. Review only-r mapping vs
other CR spellings; canonical range/class/newline metadata; quotes/comments/raw
names; byte/UTF diagnostics; owned options/source and work/state/cancellation.

## Task 1: references and freeze

Create tests/Pcre/PcreEscapedCrReferenceTests.cs. Consume existing Compile/Match/
MarkName, PcreByteOptions and pinned PCRE.NET oracle. Reflection activates the
new option after implementation while allowing the tests-first commit to build;
reference extras cast 0x10 and existing Python/NO_BS0/UTS flags where applicable.

- [x] Independently qualify 1500 native profiles (1474 JIT-valid, 26 range errors).
- [x] Write 1495 distinct cases (20 duplicate diagnostic rows removed) covering each review class and all earlier-context controls.
- [x] Publish draft and observe clean net8/9/10 RED with expected new-only
  failures, no earlier/native literal oracle failure and zero build warnings/errors.
- [x] Freeze the new reference blob before production changes.

## Task 2: implement and qualify

Files: PcreByteOptions.cs bool EscapedCrIsLf; PcreByteCompiler.cs forwards it;
PcreByteParser.cs appends optional bool escapedCrIsLf=false to its constructor,
initializes readonly field and selects 10 instead of 13 for simple escape r.
Keep original pattern bytes, quotes, raw names and every other escape unchanged.
Existing typed nodes, class/range metadata, widths, bytecode and matcher own the
result exactly as for n. No new runtime state.

- [x] Implement those option/plumbing/selection changes only.
- [ ] Qualify 51948 tests/framework in 18 native platform/framework results,
  zero failures/skips/build warnings/errors and 214 frozen/120 in each.
- [ ] Verify 35 frozen, Unicode generators, workload/parity/API/corpus/pack and
  package-only smoke; all three OS script checks; note existing consumer classifier.

## Task 3: author review and ready

- [ ] Review all focus classes, unchanged earlier test files and exact source diff.
- [ ] Verify head/base/proposed merge tree and final qualification receipts.
- [ ] Update syntax/resource/consumer docs, roadmap/issue #7 and PR evidence;
  ready PR; remove only this plan's ledger. Grep stays last.

## Baseline

PR #226 merged fa0d79c4368f7fb65784a7191c3bf02034a83aac; main 43159771095 and
scripts 43159771272 are green. Source 101a99ab8c0d4f4d8f630ee84b449a2fcf06c8c4,
tree 05241a257a48e3764422d7f432e97cc42c8bffb3, 50453 tests/framework before 1495.

## Frozen reference receipt

Corrected tests-only source f3f5b8476c82edf4139b4525140cd0de8396cadb;
RED 680/run 43164711369, Linux ARM64 job 131003756401. net8/9/10 each:
51,948 tests, 356 expected new-only failures, 51,592 passes, zero skips and
build warnings/errors. No earlier-reference or native literal-oracle failure.
Freeze all 1,495 distinct cases at d08e6c308f4754152166c97c88a67f64fec8f674
before the three production files change. Initial CI 679's 20 duplicate
diagnostic rows were removed before this receipt.
