# PCRE start directives implementation plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan inline. Steps use checkbox syntax for tracking.

**Goal:** Accept LF/ANYCRLF and Unicode/AnyCRLF BSR start directives with native precedence and original offsets.
**Architecture:** A prefix-only parser scan selects the two existing global settings before ordinary grammar. The compiler uses the parser's effective settings for existing bytecode lowering; no runtime state changes.
**Tech Stack:** C#13, net8/net9/net10, xUnit, pinned PCRE.NET1.6.0/PCRE2 10.48 Compiled/JIT.
**Spec:** `doc/PCRE-START-DIRECTIVES-DESIGN.md`

## Global constraints

- Exact case-sensitive four directives, uninterrupted original-byte0 prefix; last wins independently; caller defaults otherwise retained.
- Original source retained; no AST/opcode/frame/journal/matcher change, retained spans, snapshots or production fallback.
- No Python/public API/package/version/dependency/workflow/release/consumer change; Grep last and CommandFramework removal after publication/all-consumer migration.
- Local Debug; six native Staging CI jobs; no local SDK, executable managed verification in CI. User owns merge.
- C#13, net8/net9/net10; source src, tests tests, docs doc; preserve ignored evidence/worktree.

## Review focus

1. A newline override must govern x/xx comments before ordinary grammar and all runtime newline users.
2. BSR must remain independent of newline, survive inline resets/groups/calls, and retain atomic CRLF/behind widths.
3. Trivia, options or comments interrupt the prefix; brackets/quotes must never mutate settings.
4. Original UTF byte offsets and invalid-pattern validation precedence must survive prefix consumption.
5. Long/repeated prefixes must not consume VM instructions/work or change cancellation and limits.

## Task1: Pin start-only selection contracts

**Files:** Create this design/plan and `tests/Pcre/PcreStartDirectiveReferenceTests.cs`; update `tests/Pcre/PcreSyntaxAcceptanceTests.cs` pending representatives and four PCRE inventory/contract docs.
**Interfaces:** Consume existing `PcreByteCompiler.Compile(ReadOnlyMemory<byte>, PcreByteOptions?, CancellationToken)` / `PcreCompiledBytePattern.Match(ReadOnlyMemory<byte>, int, CancellationToken)` and native regex settings. Produce externally pinned fixtures for Task2; no production changes.

- [ ] Write literal native-first span cases: independent duplicate/mixed precedence, opposite caller defaults, dot/N/anchors/final newline, x/xx comments, group/reset/call/behind/capture behavior; each has fixed subject bytes and full expected capture spans.
- [ ] Add byte membership and UTF scalar membership checks for newline versus BSR; every caller offset over byte/tolerant-UTF CRLF/invalid subjects. Add quotes/brackets, misplaced/truncated directives, body original-byte diagnostics, invalid UTF precedence, long prefix/work/cancellation equivalence with direct options.
- [ ] Verify fixture expectations with the pinned native wrapper. Meaningful mutations: first-wins, coupled families, scan-after-trivia, strip-prefix, compiler uses caller instead of effective settings, runtime directive node.
- [ ] Run whitespace/frozen checks, commit tests/docs and publish a draft PR from merged#104. Observe CI build success and expected UnsupportedSyntax failures in the new fixture before any production change. Existing pending representatives move to other unimplemented newline settings.

## Task2: Select effective settings before grammar

**Files:** Create `src/Pcre/PcreByteParser.StartDirectives.cs`; modify `src/Pcre/PcreByteParser.cs`, `src/Pcre/PcreByteCompiler.cs` only.
**Interfaces:** Task1 fixtures consume existing Compile/Match, unchanged. Extend internal parser constructor with `bool bsrAnyCrLf`. Expose `internal bool AnyCrLfNewline` and `internal bool BsrAnyCrLf`; existing newline readers use the effective value.

- [ ] At Parse entry call private `ParseStartDirectives()`: cancellation per token; compare exact four byte prefixes from index0; update only selected family and advance index, break at first unrecognized token. No whitespace/comment/option processing in this scan.
- [ ] Pass caller BSR into parser and send parser effective newline/BSR to existing program compiler after Parse. Preserve source bytes, compile validation, VM/width/capture/resource behavior and all other syntax gates.
- [ ] Run all six Staging jobs: each framework full suite and frozen214, zero failures/skips/warnings/errors, configured extraction/glibc/corpus/API/package-consumer gates and seven Linux workload equalities. Local whitespace/frozen35/clean and exact published tree checks.
- [ ] Fresh whole-branch review on the most capable model; fix material findings once with observed RED→GREEN and full green; ledger deferred minors and explicit rulings. Update PR proof and mark ready only after exact-head checks. Never merge.

Plan self-check: Task1 produces behavior fixtures; Task2 retains their public/internal Compile/Match boundary, changing only parser configuration plumbing. All five review-focus classes have explicit Task1 coverage. No shared interface conflict.
