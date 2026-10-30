# PCRE failure verbs implementation plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans inline. No subagents, per user instruction.

**Goal:** Support no-argument FAIL/F through the shared backtracking and native width-tail rules.
**Architecture:** A failure AST leaf emits one Fail opcode, which sets existing failed state. Ordered width traversal stops only the immediate branch at failure; global source visitors remain complete.
**Tech Stack:** C#13, net8/net9/net10, xUnit, PCRE.NET1.6.0 / PCRE2 10.48 Compiled/JIT.
**Spec:** `doc/PCRE-FAILURE-VERBS-DESIGN.md`

## Global constraints

- Exact no-argument FAIL/F; zero consumption/unconditional failure, direct repetition invalid, grouped repeats valid. Arguments/other verbs remain pending.
- Shared backtracking/journal/budgets; no new frames/state, saved spans/pointers, snapshots, clones, fallback, API/package/dependency/workflow/release/consumer changes or Python.
- Preserve full parse/global reference validation/newline metadata. Only consumed-width branch tails and nested-behind width suppression terminate at failure.
- Local Debug; six native Staging jobs; managed RED/GREEN via CI (no local SDK). No subagents; separate self-review. User owns merge. Grep last/CommandFramework removal after publication/all-consumer migration.

## Review focus

1. Failure must roll back captures/starts/marks and retry through existing scopes, without consuming subject or accepting at EOF/malformed offsets.
2. Direct quantifiers must reject while grouped repetition/quoting/brackets and exact alias boundaries retain ordinary grammar.
3. Width must stop at direct failure only within the current branch; preceding unbounded/cyclic widths and later alternatives still validate.
4. Nested behinds in dead width tails must be suppressed, including lookahead descendants, while global tails and global unresolved references remain validated.
5. Full-source newline metadata, caller offsets/UTF fragments, cancellation/budgets/instruction limits must retain existing protocols.

## Task1: Pin native behavior

**Files:** Create `tests/Pcre/PcreFailureVerbReferenceTests.cs`, design/plan; update PCRE design/matrix/resource/consumer docs and pending verb representatives.
**Interfaces:** Existing Compile/Match/native APIs only.

- [ ] Literal native-first all-capture rows and all-offset matrices for byte/UTF modes, valid/malformed subjects, LF/ANYCRLF, captures/assertions/calls/conditionals and width-tail rules. Pin nonrepeatable aliases, malformed/argument boundaries, global errors and resource/cancellation behavior.
- [ ] Run pinned native checker, self-check design/plan and frozen35/whitespace; publish tests-only draft and observe successful build plus expected feature failures before source edits.

## Task2: One failure transition and ordered width handling

**Files:** Modify `src/Pcre/PcreByteSyntax.cs`, `PcreByteParser.Groups.cs`, `PcreByteParser.Lookbehind.cs`, `PcreByteParser.LookbehindResolution.cs`, `PcreByteProgram.cs`, `PcreByteMatcher.cs`.
**Interfaces:** New leaf `PcreByteFailure : PcreByteNode` and opcode `Fail`; existing Compile/Match unchanged.

- [ ] Recognize exact complete FAIL/F headers at group opening; return failure leaf. Reject direct repeats via existing quantifier parsing/diagnostic path.
- [ ] Emit one Fail opcode; matcher sets existing failed flag after the shared instruction step. No separate rollback/frame logic. Default semantic analyses conservatively reject impossible empty success; full lexical metadata/tree visitors retain tail source.
- [ ] Width ScheduleBody and SequenceWidth stop after direct failure per branch, with failure width0. Extend existing width-mode suppressed-behind traversal to mark whole tail subtrees per branch after failure; global mode still walks later source. Retain DEFINE/ahead suppression and existing call-target validation gates.
- [ ] All six Staging jobs: full/frozen214 on net8/net9/net10, zero failed/skipped tests/build warnings/errors; configured gates and seven Linux equalities. Separate whole-branch self-review; fix material issues RED→GREEN; exact source/tree/head/frozen35/whitespace/clean checks. Update PR proof/mark ready; never merge.

Self-check: Task1 consumes stable Compile/Match; Task2 adds leaf/one transition and bounded heap width traversal only. All five focus classes have Task1 coverage. No unresolved design question or interface conflict.
