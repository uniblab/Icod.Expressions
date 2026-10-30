# Bracket reference escapes implementation plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans inline. Do not use subagents, per user instruction.

**Goal:** Interpret ordinary bracket g/k escapes as literal letters without consuming reference-looking suffixes.
**Architecture:** Two existing ParseEscape branches return the existing literal node in bracket context; all class/runtime paths are shared.
**Tech Stack:** C#13, net8/net9/net10, xUnit, PCRE.NET1.6.0 / PCRE2 10.48 Compiled/JIT.
**Spec:** `doc/PCRE-BRACKET-REFERENCE-ESCAPES-DESIGN.md`

## Global constraints

- Ordinary brackets only; consume just g or k; outside grammar/diagnostics unchanged.
- No AST/opcode/frame/journal/matcher changes, source rewriting, fallback or new engine policy.
- No Python/public API/package/version/dependency/workflow/release/consumer changes. Grep last; CommandFramework removal after publication/all-consumer migration. User owns merge.
- Local Debug; six native Staging jobs; no local SDK, managed RED/GREEN via CI.
- No subagents. Separate whole-branch self-review and preserve evidence/worktree.

## Review focus

1. Reference-looking suffixes and nonexistent names must remain ordinary class members.
2. Range endpoints, negation, i/r folding and x/xx/quoting must follow literal class rules.
3. Captures/calls/assertions/repetition and behind widths must not acquire reference dependencies.
4. Original byte diagnostics, outside reference grammar and malformed UTF precedence must stay intact.
5. Caller offsets, CRLF/fragments, work/budget/cancellation must use existing literal protocols.

## Task1: Pin literal behavior and correct #105 prose

**Files:** Create `tests/Pcre/PcreBracketReferenceEscapeTests.cs`, this design/plan; update PCRE design/matrix/resource/consumer docs and the start-directive design qualification.
**Interfaces:** Existing Compile/Match and native PcreRegex8Bit only, unchanged.

- [ ] Write native-first literal member tables over all256 bytes and strict UTF probes; literal capture rows for ranges, suffixes, quotes/trivia, calls/behind and folding. Literal expectations independent of production.
- [ ] Compare all caller offsets in byte/tolerant UTF with LF/ANYCRLF, plus malformed classes, original UTF byte offsets, outside controls, work/cancellation equivalence.
- [ ] Run the pinned native checker, whitespace/frozen35 checks. Self-check design as a builder; no unresolved configuration choices. Commit/publish tests-only draft and observe successful build plus expected feature failures before source edits.

## Task2: Return literal nodes before reference parsing

**Files:** Modify `src/Pcre/PcreByteParser.cs` only.
**Interfaces:** ParseEscape unchanged; inBracket g/k branches return `new PcreByteLiteral(value)`. Existing bracket consumers accept those nodes without alteration.

- [ ] After observed RED, replace the two bracket Unsupported throws with literal returns. Preserve every outside path and other escape gate.
- [ ] Run all six Staging jobs, 3 frameworks each, full suite plus frozen214; zero failed/skipped tests/build warnings/errors. All configured gates and seven Linux workload equalities.
- [ ] Separate read-only whole-branch self-review against all five focus classes; verify exact tree/PR head, whitespace/frozen35/clean. Record findings and resolve material issues with observed RED→GREEN. Update proof and mark PR ready; never merge.

Plan self-check: Task1 fixture boundary stays Compile/Match; Task2 changes two existing parser branches only. All review-focus classes have Task1 fixtures. No interface conflict.
