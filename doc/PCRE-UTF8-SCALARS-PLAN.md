# UTF-8 Scalar Foundation Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Opt-in strict UTF-8 scalar literal/dot matching with byte-accurate spans.

**Architecture:** Validate UTF-8 at the boundary of compilation/matching. Decode scalars on demand in parser and bytecode executor; preserve input bytes for captures/backreferences.

**Tech Stack:** C# 13, `System.Text.Rune`, xUnit, net8/net9/net10.

**Spec:** `doc/PCRE-UTF8-SCALARS-DESIGN.md`

## Global Constraints

- Existing byte mode must retain all behavior. Internal API only; no Grep or CommandFramework edits.
- Invalid UTF and unsupported Unicode constructs receive explicit diagnostics, with byte offsets.
- Use the repository's `/src`, `/tests`, `/doc` layout and six hosted Staging jobs for verification.

## Review Focus

- Overlong, surrogate, truncated and continuation-only input: test first invalid byte.
- A start offset in a continuation byte: test rejection and preserve ordinary byte-mode offsets.
- Alternation/capture rollback across multibyte characters: test first successful path and byte spans.
- `\xFF` in UTF mode: test U+00FF, while byte mode still means a single FF byte.
- Long input and cancellation: test validation under work limit and token cancellation.

---

### Task 1: Characterize the opt-in contract

**Files:** Test `tests/Pcre/PcreUtf8ScalarTests.cs`; create design and plan under `doc/`.

**Interfaces:** `PcreByteCompiler.Compile(pattern, new PcreByteOptions { Utf8 = true })`; `Pattern.Match(subject, startOffset)`.

- [ ] Add tests for raw BMP/astral literals, scalar dot/repetition, byte spans, `\xFF`, strict invalid pattern/subject, continuation offset, unsupported constructs and bounded validation.
- [ ] Run hosted Staging PR tests and confirm new tests fail for missing UTF mode.
- [ ] Commit the failing tests.

### Task 2: Implement strict scalar mode

**Files:** Modify `src/Pcre/PcreByteOptions.cs`, `PcreByteResult.cs`, `PcreByteSyntax.cs`, `PcreByteParser.cs`, `PcreByteCompiler.cs`, `PcreByteProgram.cs`, `PcreByteMatcher.cs`; create `src/Pcre/PcreUtf8.cs`.

**Interfaces:** `PcreUtf8.TryRead(ReadOnlySpan<byte>, int, out int scalar, out int width)` returns false on malformed sequence; `Utf8` propagates from options to program.

- [ ] Validate pattern and subject with `TryRead`, honoring cancellation/work budget, and validate start boundary.
- [ ] Decode raw literals, escapes and dot at scalar boundaries. Preserve bytes in captures/backreferences and ordered backtracking.
- [ ] Reject UTF classes/lookarounds/caseless until their Unicode behavior is implemented.
- [ ] Run six Staging jobs plus package/GNU smoke, fix failures, and commit.

### Task 3: Review and deliver

**Files:** Update `doc/PCRE-DESIGN.md` status.

- [ ] Inspect diff against spec, including existing byte mode tests and limits.
- [ ] Verify fresh full CI results and request independent code review.
- [ ] Open/ready a reviewable PR for user merge.
