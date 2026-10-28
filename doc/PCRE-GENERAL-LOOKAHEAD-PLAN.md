# Internal PCRE general lookahead implementation plan

**Goal:** Execute general atomic lookahead bodies with shared capture rollback.

**Architecture:** Heap-framed parser, one semantic lookahead node, inline
enter/body/exit bytecode, typed guards on the existing bounded VM stack and
shared journal. Preserve fixed-literal lowering and ordinary program behavior.

**Tech stack:** C#13, net8/net9/net10, pinned test-only PCRE.NET1.6.0/PCRE2 10.48.

**Spec:** [PCRE-GENERAL-LOOKAHEAD-DESIGN.md](PCRE-GENERAL-LOOKAHEAD-DESIGN.md).

## Constraints and review focus

- Root project/solution, src/tests/samples; UTF-8 LF; Debug local/Staging PR.
- No public API/version/dependency, consumer, native/Regex fallback or VM clone.
- Keep original caller start/subject/UTF fragment for every assertion.
- Positive captures persist but must roll back through outer failed paths.
- Negative success/failure both discard all temporary body mutations.
- Nested guards, atomic scopes and normal retry frames must restore together.
- Preserve old fixed-literal instruction shape/work and controlled pending forms.
- Use only integer/checkpoint/enum runtime frames, bounded by existing 1024 cap.

## Task 1: Reference and red fixtures

Create tests/Pcre/PcreGeneralLookaheadReferenceTests.cs with pinned whole/all
capture spans and independent key outcomes, covering the design's cases and
resource boundaries. Commit these notes/tests, open a draft and inspect missing-
feature CI failures before implementation. No local dotnet SDK is available.

## Task 2: Parser, semantic node and compiler

Modify PcreByteParser.Groups.cs and PcreByteParser.cs: heap lookahead frames,
inherited assertion context/K rejection, option restoration, general body
closure and unchanged literal lowering when safe. Add PcreByteLookahead in
PcreByteSyntax.cs; resolve names in its body in NamedCaptures.cs. Keep direct
quantifiers unsupported at the quantifier offset. Add enter/exit compilation in
PcreByteProgram.cs using compileWork, explicit-newline/zero-width analysis and
existing resource checks. Update obsolete pending fixtures/current inventories.

## Task 3: Shared VM guards

Modify PcreByteMatcher.cs: typed guard frames and assertion-depth checkpoints,
lazy active-boundary storage, bounded enter/exit, atomic pruning, shared journal
restoration and failure unwinding. Preserve existing Step behavior for ordinary
programs, charge new assertion work/cancellation and retain the saved-path cap.
No recursive matching, subject slicing or capture-array snapshots.

## Task 4: Review and full verification

Review the complete diff with a fresh read-only subagent; fix concrete findings
through observed red/green tests. Run whitespace/frozen checks and all six final
Staging jobs; inspect per-framework counts and existing compatibility/package/
workload gates. Record evidence, mark ready and leave merge to the user.
