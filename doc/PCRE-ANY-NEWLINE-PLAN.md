# ANY newline implementation plan

> **For agentic workers:** Use superpowers:executing-plans inline; no subagents, per user instruction.

**Goal:** Complete ANY newline selection and correct pinned consumed-CR ANYCRLF anchor regressions.
**Architecture:** Appended enum, shared fixed vertical membership, bounded byte-length/previous-scalar helpers; cached byte dot set and existing Any/anchor/comment paths, no new VM state.
**Tech Stack:** C#13, net8/net9/net10, xUnit, PCRE.NET1.6.0 / PCRE2 10.48 Compiled/JIT.
**Spec:** `doc/PCRE-ANY-NEWLINE-DESIGN.md`

## Global constraints

- Exact original-byte0 last-wins family; UTF/UCP/BSR/NOTEMPTY independent. LF/VT/FF/CR/NEL/UTF LS/PS; NUL ordinary.
- Correct consumed-CR midpoint caret/dollar/Z under ANYCRLF and ANY; CRLF-only unchanged. Existing candidate/behind caps/resources shared.
- No new AST/opcode/frame/journal/limits, clone/fallback/API/package/version/dependency/workflow/release/consumer/Python/subagent change. No SDK; local Debug/CI six native Staging; user merges. Grep last/CommandFramework removal after publication/all consumers.

## Review focus

1. UTF newline length/reverse anchors must stay inside active ends/fragments and never mistake continuation85 for NEL.
2. Comments decode complete UTF scalars, consume CRLF once and ignore NUL as a terminator; byte mode85 remains a newline.
3. Consumed-CR midpoint anchors are distinct from unanchored pair bumpalong; preserve CRLF-only and byte/UTF prefix differences.
4. DotAll/N, R/BSR, classes/properties/shorthands and old conventions remain independent, including scoped/reset/call/behind paths.
5. Original-byte prefix/errors/strict validation, long patterns, work/cancellation and malformed subject offsets remain bounded.

## Task1: Native fixtures and expected RED

**Files:** New `tests/Pcre/PcreAnyNewlineReferenceTests.cs`, design/plan; PCRE design/matrix/resource/consumer inventories; pending ANY controls in SyntaxAcceptance/StartDirective tests.
**Interfaces:** Existing Compile/Match/pinned reference only.

- [ ] Literal native spans in byte/UTF, raw-comment/continuation controls, all-byte/Unicode membership, every-offset byte/tolerant matrix, grammar/strict/resource cases and three old-mode anchor regressions cover five focus classes.
- [ ] Native checker/design-plan-interface self-check/frozen35/whitespace, publish tests-only draft and observe successful compilation plus expected new-fixture RED before source edits.

## Task2: Scalar-aware shared newline handling

**Files:** Newline helper, StartSettings, Parser/Parser.Extended, Set and Matcher under `src/Pcre/`.
**Interfaces:** Append Any; helpers `AnyLengthAt(ReadOnlySpan<byte>, int position, int end, bool utf8)` and `IsAnyBefore(ReadOnlySpan<byte>, int position, int start, bool utf8)`, fixed membership via existing IsVertical. Existing Compile/Match/VM instruction interfaces unchanged.

- [ ] Scan ANY; cached DotAny set, byte ANY DotAll uses Any leaf (existing byte prefix proof), ordinary byte dot/N set. UTF Any rejects fixed vertical membership when not DotAll.
- [ ] Helpers decode bounded scalar widths and at most3 preceding continuation bytes; comments use full scalar/pair length, anchors use current end/fragment. Final-newline checks allow one pair/scalar or final LF after consumed CR. Remove two contrary ANYCRLF midpoint restrictions. Add ANY to pair bumpalong; retain all other mode branches and metadata/limits.
- [ ] Six full/frozen214 suites per framework/platform, zero failed/skipped/build warnings/errors, configured gates/seven Linux equalities; separate author whole-branch self-review/material fixes RED→GREEN, exact tree/head/frozen35/whitespace/clean proof. Update PR/mark ready; never merge.

Pre-flight: fixed membership already shared, Any width1 and byte execution supported by #111; Task1 uses stable APIs, Task2 helper bounds and old-anchor corrections explicit. No interface conflict. Preserve evidence.

### Discovered search/offset corrections

Extend Program metadata with a zero-width instruction whitelist plus native
first-significant-AST-node proof (leading StartLine/capture/noncapture/atomic,
every branch nonempty/qualified); cancellation-aware heap walk. Derive and
exclude the end of a partially preceding ANY newline only under this proof.
K/empty groups before ^, literals and unanchored alternatives disable it.
Nineteen controls observed RED before the leading-proof correction.

Extend the requested malformed/continuation nullable virtual-fragment path with
all remaining continuation-byte empty candidates inside a validated scalar,
at most3 candidates total. The offset matrix already observed RED under
NOTEMPTY_ATSTART; no consumption is allowed in these points. Preserve real
fragment context, valid scalar search and existing gap logic. Shared bounded
scalar-start/end helper, no saved/backtracked state or raised limits. Final
whole-suite/six-platform verification is still required.
