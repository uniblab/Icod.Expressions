# Named cuts implementation plan

Execute inline with superpowers:executing-plans, tests first, from merged221.
Goal: named PRUNE/COMMIT/THEN labels with shared bounded cut transactions.
Architecture: optional raw name on existing nodes; one opcode/frame per cut;
shared journaled label slot, immutable bytes and lexical assertion metadata.
Tech: C#13/net8/9/10. Spec: [named cuts design](PCRE-NAMED-CUTS-DESIGN.md).

## Constraints and review focus

Keep GNU/BRE/ERE/frozen35/frozen214/120, exported API and dependencies fixed.
No fallback, subagents, Python or SDK installs. User owns integration/publication.
Review five failure classes: names accidentally becoming SKIP targets; raw empty
versus NUL names/UTF byte limits; atomic/nested/erased assertion history; returned
capture/label rollback and mixed-cut ownership; shared frame/mark/work/compile
bounds and mutation/cancellation. Each has fixtures in the new reference class.

## Task1: qualify and freeze

Files: PcreNamedCutReferenceTests; obsolete header inventories in
PcrePruneVerbReferenceTests/PcreCommitVerbReferenceTests/PcreThenVerbReferenceTests;
unfrozen pending entries in PcreSyntaxAcceptanceTests/PcreGroupAliasReferenceTests.
Interfaces: existing compile/match/MarkName and PCRE.NET Mark byte spans.

- [x] Verify502 literal/profile fixtures in988 native executions; prepare543 cases.
- [x] Verify prior native source outside the six obsolete header rows unchanged.
- [ ] Publish draft and run clean-build RED; expect missing named-cut failures
  with no earlier fixture or native oracle failures and zero warnings/errors.
- [ ] Freeze new references/revised inventories before production changes.

## Task2: implement and qualify

Files: Syntax; Parser.PathMarks/Groups; Program; Matcher.
Interfaces: PcreBytePrune/Commit/Then(ReadOnlyMemory<byte> Name = default),
TryParseNamedCut, CompileNamedCut(Opcode,Name,First,Second); existing instruction
PropertyValue carries label-slot-plus-one only for these cut opcodes.

- [ ] Parse exact named/empty headers with shared lexer and EOF diagnostics;
  activate HasThen for named THEN and preserve direct-repeat/width behavior.
- [ ] Emit one cut opcode, immutable visible bytes and one shared label slot;
  extend atomic history-isolation metadata to nonempty named PRUNE/COMMIT.
- [ ] Save cut checkpoint then journal/write its label; never add MARK history.
- [ ] Run all applicable native CI:47792 cases/framework,18 results, zero
  failures/skips/build warnings/errors and frozen214/120 in every result.

## Task3: author review and ready

Files: syntax/resource/consumer inventories, roadmap status and issue7 footer.

- [ ] Review all five focus classes and fixed references/frozen source.
- [ ] Confirm all qualifications, exact head/base and proposed merge tree.
- [ ] Record RED/GREEN evidence, update roadmap/PR, ready PR; clean own ledger.
