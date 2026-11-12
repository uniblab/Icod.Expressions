# Empty Character Classes Implementation Plan

Execute inline with the established continuous workflow.
**Goal:** Add native caller-selected empty class parsing with default behavior fixed.
**Architecture:** Readonly parser bool and class-start recognition; existing
empty bitmap/ALLANY nodes and bounded UTS scalar operands own all downstream work.
**Tech Stack:** C# 13, net8/9/10; managed RED/GREEN through GitHub CI.
**Spec:** [design](PCRE-EMPTY-CLASSES-DESIGN.md).

## Global constraints

Preserve all earlier test files, GNU/BRE/ERE, 35 frozen production blobs, 214
frozen outcomes in 120 methods, exported API and dependencies. UTF-8+LF. No
Python, subagents, local SDK installs or runtime fallback. User owns merge/tag/
publication. Continue in the existing isolated worktree. Grep stays last.

## Review focus

- Empty classes consume no successful character; zero repeats remain meaningful.
- Negated emptiness keeps ALLANY width, newline, UTF-fragment and start behavior.
- Nested UTS empty operands work; Perl algebra grammar remains unchanged.
- Class trivia/quotes and UTF/source diagnostic bytes retain their exact meanings.
- Caller isolation and existing work/cancellation/depth/capture/expansion gates hold.

## Task 1: reference fixture and valid freeze

Create `tests/Pcre/PcreEmptyClassReferenceTests.cs`. Consume existing Compile,
Match, MarkName and PCRE.NET oracle interfaces. Reflection permits tests-first
builds; the native compile flag is 0x1 alongside UTS/UTF and existing extra flags.
Use 941 distinct cases: 818 literal native profile rows, 94 distinct native compile-error cases,
exact independently read legacy diagnostic controls, all octets, selected UTF/
CRLF offsets, source/options/labels, option composition and resource controls.

- [x] Independently qualify 1,408 native profiles and deduplicate theory inputs.
- [x] Publish the tests-only draft and observe clean net8/9/10 RED.
  Expected: only option-dependent new failures; every default/native-literal/
  earlier-reference/legacy-diagnostic check passes; zero warnings/errors/skips.
- [x] Record and freeze the new blob before production changes. Inspect failure
  identities and assertion sites, not just counts, before declaring valid RED.

## Task 2: implement and qualify

Modify `src/Pcre/PcreByteOptions.cs` with default-false bool `AllowEmptyClass`;
`PcreByteCompiler.cs` forwards it to a final optional parser constructor bool;
`PcreByteParser.cs` copies it readonly and checks class starts before dispatch.
Create `PcreByteParser.EmptyClasses.cs`: `TryReadEmptyClass(out bool negated)`
uses ParseClassNegation, consumes only an immediate closing bracket, restores
index otherwise. Ordinary empty classes use existing empty bitmap or ALLANY.
Modify `PcreByteParser.UtsClass.cs` nested openings to recognize empty operands,
using existing scalar operands/AddUtsTerm/JoinUtsSequence. Preserve depth checks.
Perl class-algebra readers, existing AST/width/VM/runtime remain unchanged.

- [x] Implement only those interfaces and paths after the valid reference freeze.
- [ ] Qualify all 52,889 tests/framework in all 18 native platform/framework results, zero
  failures/skips/build warnings/errors and frozen 214/120 in each.
- [ ] Verify frozen 35, five Unicode generators, workload/parity/API/corpus,
  package-only smoke and three-OS scripts; state existing consumer classifier.

## Task 3: review and readiness

- [ ] Author-review all five input classes and unchanged earlier test files.
- [ ] Verify final head/base/proposed merge tree and qualification receipts.
- [ ] Update syntax/resource/consumer docs, roadmap status/issue #7 and PR evidence.
- [ ] Mark PR ready and remove only this tranche ledger. User merges/publishes.

## Baseline

PR227 merged at 600d4f9e9f0d6c984ed101c163262cbcc615f3b1.
Main 43169151838 and scripts 43169151810 are green; 51,948 tests/framework.
Source ae2ba88e6869b7b13192c95b6ae1bbbb12dfbaf2,
tree f6660fdcd403e7d2a23d776663376d16e4c30d58.

## Frozen reference receipt

Tests-only source 399c334c6caaf166421b5969cfd90d2f16e388b0,
tree 7c3c638e9ca7b45882a6f9376311a5b4aecdaed3.
RED 684/run 43171972153, Linux x64 job 131027191251: net8/9/10 each
52,889 tests, 640 option-dependent new failures, 52,249 passes and zero skips,
build warnings or errors. Default profiles, grammar errors, exact diagnostics,
UTF validation, earlier fixtures and native literal checks passed. Freeze all
941 cases at 8565080638577ecf293f0d4f24685cead1e1de64 before production edits.
