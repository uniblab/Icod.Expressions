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
- [ ] Publish draft; run clean-build RED with expected new-only failures,
  no native oracle/earlier fixture failures and zero warnings/errors.
- [ ] Freeze new/revised references before production changes.

## Task2: implement and qualify

Files: Syntax, Parser.PathMarks/Groups, Program. Interfaces:
PcreByteAccept/Failure(ReadOnlyMemory<byte> Name = default),
TryParseNamedImmediateVerb, existing CompilePathMark and CompileAccept.

- [ ] Parse exact names with shared lexer; retain bare F/FAIL EOF byte1 and
  direct repeat/termination/width behavior on the same typed leaves.
- [ ] Compile nonempty names through PathMark before EarlyAccept/Fail, inside
  any ACCEPT quantifier. Empty/erased names allocate no runtime label slot.
- [ ] Extend lexical atomic history metadata to both nonempty immediate names.
- [ ] Qualify full native CI:48104 cases per framework,18 results, zero
  failures/skips/build warnings/errors and frozen214/120 in every result.

## Task3: review and ready

Files: syntax/resource/consumer inventories, roadmap status and issue7 footer.

- [ ] Review five focus classes, native unsupported-mode limits and fixed blobs.
- [ ] Confirm all qualifications, exact head/base and proposed merge tree.
- [ ] Record RED/GREEN evidence, update roadmap/PR, ready PR; clean own ledger.
