# CRLF-only newline implementation plan

> **For agentic workers:** Use superpowers:executing-plans inline; no subagents, per user instruction.

**Goal:** Add CRLF-only newline semantics through the shared parser/VM and native byte/UTF candidate policies.
**Architecture:** Append CrLf enum value and bounded pair predicate. Byte CRLF dot/N use the existing Any leaf/opcode with byte-or-UTF reading; anchors/comments/bumpalong test complete pairs, existing behind end caps retained.
**Tech Stack:** C#13, net8/net9/net10, xUnit, PCRE.NET1.6.0 / PCRE2 10.48 Compiled/JIT.
**Spec:** `doc/PCRE-CRLF-NEWLINE-DESIGN.md`

## Global constraints

- Exact byte0 prefix, last-wins newline enum independent of UTF/UCP/BSR/NOTEMPTY; original bytes and validation precedence retained.
- Full pair only; lone CR/LF ordinary; no cloned width engine or new AST/opcode/frame/journal/limit policy. Byte-only two-atom Any+literal prefix proof; UTF unchanged.
- No API/package/version/dependency/workflow/release/consumer/fallback/Python/subagent change. Local Debug; six native Staging jobs, no SDK/CI RED→GREEN. User merges; Grep last/CommandFramework removal after publication/all consumers.

## Review focus

1. A CR is a newline only with an LF inside the active end; fixed and variable behind caps differ natively.
2. Byte two-atom dot/N prefix may enter at LF, UTF must retain failure-driven pair skip; explicit caller LF offset works in both.
3. All caret/dollar/Z/end forms and comment scanners must recognize complete pairs, not lone CR/LF or NUL, across fragments/scopes/calls.
4. DotAll versus N, R/BSR and literal/class/shorthand membership remain independent; old four modes and nullable/invalid UTF protocols unchanged.
5. Prefix overrides/malformed offsets, long inputs, exact shared work, cancellation and saved-state bounds remain controlled.

## Task1: Native fixtures and expected RED

**Files:** New `tests/Pcre/PcreCrLfNewlineReferenceTests.cs`, design/plan; PCRE design/matrix/resource/consumer inventories; pending CRLF representatives in SyntaxAcceptance/StartDirective tests.
**Interfaces:** Existing Compile/Match/pinned reference only.

- [ ] Native-first literal and width-specific prefix rows, byte256/Unicode and raw comments; every-offset byte/tolerant UTF matrix plus grammar/strict-error/work/cancellation cases cover all five focus classes.
- [ ] Native checker, design/plan/interface self-check, frozen35/whitespace; publish tests-only draft and observe successful build plus expected feature RED before source edits.

## Task2: Pair-aware shared semantics

**Files:** Newline enum/helper, StartSettings, Parser/Parser.Extended, Program and Matcher under `src/Pcre/`.
**Interfaces:** Append CrLf; `IsCrLf(ReadOnlySpan<byte>, int position, int end)` checks bounded pair; retain existing Any node/opcode and width1. Compile/Match unchanged.

- [ ] Scan CRLF last-wins; emit Any for byte CRLF dot/N. Any matcher reads byte or scalar using existing transition/comparison charges, rejecting pair CR only when not DotAll.
- [ ] Pair-aware caret/dollar/Z and x/xx comment termination; use current subjectEnd/absoluteEnd/fragment bounds. Extend pair bumpalong condition to CRLF. Extend existing HasByteCrLfPrefix first-atom proof to Any only, preserving byte-only call guard, mandatory literal suffix and wrapper restrictions; retain all old-mode branches/metadata.
- [ ] Six full/frozen214 suites on all frameworks/platforms, zero failed/skipped/build warnings/errors and configured gates/seven Linux equalities. Separate author whole-branch self-review; material fixes RED→GREEN, exact tree/head/frozen35/whitespace/clean proof; update PR/mark ready, never merge.

Pre-flight: existing Any already has consumed width1 and nonnullable semantics; Task1 uses stable APIs, Task2 configuration/lowering remains internal. Pair predicate end bounds and byte-only prefix gate explicit; no interface conflict. Preserve evidence.
