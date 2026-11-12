# Escaped CR compile option implementation plan

Execute inline with superpowers:executing-plans. Spec:
[design](PCRE-ESCAPED-CR-DESIGN.md).
Goal: compile-time r-to-LF selection through the existing simple-escape path.
Architecture: one defaultfalse caller bool, readonly parser copy and simple
literal selection; no source rewrite, typed node, opcode or runtime change.
Tech: C#13/net8/9/10; GitHub CI for managed RED/GREEN.

## Constraints and review focus

Preserve all earlier references, default behavior, GNU/BRE/ERE, frozen35 and
frozen214/120, exported API/dependencies. No Python, subagents, SDK installs or
runtime fallback. User owns merge/tag/publication. Review only-r mapping vs
other CR spellings; canonical range/class/newline metadata; quotes/comments/raw
names; byte/UTF diagnostics; owned options/source and work/state/cancellation.

## Task1: references and freeze

Create tests/Pcre/PcreEscapedCrReferenceTests.cs. Consume existing Compile/Match/
MarkName, PcreByteOptions and pinned PCRE.NET oracle. Reflection activates the
new option after implementation while allowing the tests-first commit to build;
reference extras cast0x10 and existing Python/NO_BS0/UTS flags where applicable.

- [x] Independently qualify1500 native profiles (1474 JIT-valid,26 range errors).
- [x] Write1515 cases covering each review class and all earlier-context controls.
- [ ] Publish draft and observe clean net8/9/10 RED with expected new-only
  failures, no earlier/native literal oracle failure and zero build warnings/errors.
- [ ] Freeze the new reference blob before production changes.

## Task2: implement and qualify

Files: PcreByteOptions.cs bool EscapedCrIsLf; PcreByteCompiler.cs forwards it;
PcreByteParser.cs appends optional bool escapedCrIsLf=false to its constructor,
initializes readonly field and selects10 instead of13 for simple escape r.
Keep original pattern bytes, quotes, raw names and every other escape unchanged.
Existing typed nodes, class/range metadata, widths, bytecode and matcher own the
result exactly as for n. No new runtime state.

- [ ] Implement those option/plumbing/selection changes only.
- [ ] Qualify51968 tests/framework in18 native platform/framework results,
  zero failures/skips/build warnings/errors and frozen214/120 in each.
- [ ] Verify frozen35, Unicode generators, workload/parity/API/corpus/pack and
  package-only smoke; all3OS script checks; note existing consumer classifier.

## Task3: author review and ready

- [ ] Review all focus classes, unchanged earlier test files and exact source diff.
- [ ] Verify head/base/proposed merge tree and final qualification receipts.
- [ ] Update syntax/resource/consumer docs, roadmap/issue7 and PR evidence;
  ready PR; remove only this plan's ledger. Grep stays last.

## Baseline

PR226 mergedfa0d79c4368f7fb65784a7191c3bf02034a83aac; main43159771095 and
scripts43159771272 are green. Source101a99ab8c0d4f4d8f630ee84b449a2fcf06c8c4,
tree05241a257a48e3764422d7f432e97cc42c8bffb3,50453 tests/framework before1515.
