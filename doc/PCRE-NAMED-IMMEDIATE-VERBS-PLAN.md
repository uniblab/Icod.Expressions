# Named immediate verbs implementation plan

Execute inline with superpowers:executing-plans, tests first, from merged222.
Goal: named ACCEPT/FAIL/F with native implicit MARK and shared scope behavior.
Architecture: optional leaf names, shared lexer, PathMark then existing immediate
opcode. Tech: C#13/net8/9/10. Spec:
[design](PCRE-NAMED-IMMEDIATE-VERBS-DESIGN.md).

## Constraints and review focus

Keep GNU/BRE/ERE/frozen35/frozen214/120, exported API and dependencies fixed.
No fallback, subagents, Python or SDK installs; user owns merge/tag/publication.
Review five failure classes: quantifiers applying outside the implicit mark;
scope/capture/label rollback and authentic history reclamation; lexical erased/
NUL history isolation; raw-byte length/EOF/UTF diagnostics and memory ownership;
shared history/slot/expansion/work/cancellation limits. Each has new fixtures.
Native JIT-unsupported non-atomic ACCEPT references are explicitly scoped.

## Task1: qualify and freeze

Files: PcreNamedImmediateVerbReferenceTests; obsolete inventories in
PcreAcceptVerbReferenceTests/PcreFailureVerbReferenceTests, PcreGroupAliasReferenceTests
and PcreSyntaxAcceptanceTests. Interfaces: existing compile/match/MarkName.

- [x] Verify266 literal/profile fixtures in526 native executions; prepare312 cases.
- [x] Verify native source outside obsolete ACCEPT/FAIL inventories unchanged.
- [x] Publish draft; run clean-build RED with expected new-only failures,
  no native oracle/earlier fixture failures and zero warnings/errors.
- [x] Freeze new/revised references before production changes.

## Task2: implement and qualify

Files: Syntax, Parser.PathMarks/Groups, Program. Interfaces:
PcreByteAccept/Failure(ReadOnlyMemory<byte> Name = default),
TryParseNamedImmediateVerb, existing CompilePathMark and CompileAccept.

- [x] Parse exact names with shared lexer; retain bare F/FAIL EOF byte1 and
  direct repeat/termination/width behavior on the same typed leaves.
- [x] Compile nonempty names through PathMark before EarlyAccept/Fail, inside
  any ACCEPT quantifier. Empty/erased names allocate no runtime label slot.
- [x] Extend lexical atomic history metadata to both nonempty immediate names.
- [ ] Qualify full native CI:48104 cases per framework,18 results, zero
  failures/skips/build warnings/errors and frozen214/120 in every result.

## Task3: review and ready

Files: syntax/resource/consumer inventories, roadmap status and issue7 footer.

- [ ] Review five focus classes, native unsupported-mode limits and fixed blobs.
- [ ] Confirm all qualifications, exact head/base and proposed merge tree.
- [ ] Record RED/GREEN evidence, update roadmap/PR, ready PR; clean own ledger.

## Frozen RED checkpoint

RED667 run43128427985/LinuxARM64 job130886300032 built net8/9/10 with zero
warnings/errors. Each framework reported48104 cases:298 expected new failures,
47806 passes,0 skips. No earlier fixture or native literal oracle failed.
New312-case blob1d87fd94a6e71f5b92340a518c25ae3689cb0004 freezes before production.
Revised inventories also freeze: ACCEPT13ef58775336b310950db8d7fdcb7a183e385cb5;
FAIL070cf3e9dda7e66494d8a41f5acfaa6f9cda4101;
group-aliasdfd0b09613d7428529998f89ee20be711315e053;
syntaxb21c77d2509ad661cf2e6a0ca18509624a7dbf46.
Native source outside obsolete header inventories and bare FAIL EOF1 stay fixed.

## Review regression task

CI668/LinuxARM64 built cleanly and passed48103/48104 per framework; the only
failure was the frozen accepted-callee label fixture. Add separate
PcreAcceptedCallLabelReferenceTests (18 cases,36 native profiles) before the fix.

- [x] Reproduce native normal/accepted returns and lexical quit contrast; read
  JIT accepted-call exit and kept-shared label copy rules.
- [ ] Qualify separate RED and freeze18 controls; original312 refs unchanged.
- [ ] Add bounded per-target lexical-quit Call metadata and charged label-only
  journal compaction at accepted-callee exit; preserve older checkpoints.
- [ ] Qualify final48122 cases/framework across18 native results.
