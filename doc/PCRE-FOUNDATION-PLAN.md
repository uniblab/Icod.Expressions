# In-house PCRE byte foundation implementation plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add an internal, tested ordered-backtracking PCRE byte-mode foundation without switching Grep or publishing a new package.

**Architecture:** A separate parser compiles byte patterns to immutable ordered bytecode; a matcher searches authoritative bytes from the requested offset using an explicit bounded backtracking stack, and rolls capture state back with failed paths. GNU BRE/ERE code is untouched. UTF-8/UCP, lookarounds and full Grep migration are later milestones under issue #7.

**Tech Stack:** .NET 8/9/10, C# 13, xUnit, existing single `Icod.Expressions` project and test project.

**Spec:** [`doc/PCRE-DESIGN.md`](PCRE-DESIGN.md), [issue #7](https://github.com/uniblab/Icod.Expressions/issues/7).

## Global constraints

- Root solution and project files stay in `/`; implementation is under `/src`; tests under `/tests`; samples under `/samples`.
- The first PCRE API stays internal with the existing `InternalsVisibleTo("Icod.Expressions.Tests")` grant; no release version or public API change in this milestone.
- No PCRE.NET or `System.Text.RegularExpressions` runtime call/fallback. No modification to existing GNU BRE/ERE result selection.
- The internal `PcreByteOptions.MatchWorkLimit` is a positive immutable per-compiled-pattern limit, default 1,000,000. Each match starts at zero; count each candidate start and attempted node/byte transition. A limit overrun is a result diagnostic, not no match. These provisional values are revisited before Grep migration.
- Cancellation is observed before compile/match and at least every 256 work steps. An internal observer option can cancel a token at a deterministic step in tests; it is not a planned public API.
- Initial grammar: raw pattern bytes other than `|` are literals, including NUL and 0xFF. `|` separates ordered alternatives, and empty pattern/branches are valid zero-length matches. All other PCRE metacharacters and backslash escapes are diagnosed until a later task implements them; never silently literalize unsupported constructs.
- The compiler copies pattern bytes or fully compiles immutable nodes, so later mutation of the caller's array cannot alter a compiled pattern.
- All offsets and capture lengths in this milestone are authoritative byte offsets. UTF-8/UCP behavior is explicitly outside this milestone, so no Grep consumer switch occurs.

## Review focus

1. `a|ab` on `ab` returns `a`, while `ab|a` returns `ab`; the GNU longest selection must not leak into PCRE.
2. `(a|ab)c` on `abc` restores capture state when the first branch fails, yielding capture `ab`.
3. Greedy and lazy repetition on the same input choose different first successful spans, and empty repeats terminate.
4. Embedded NUL and high bytes remain bytes and return exact offsets; no string transcoding.
5. A pathological nested repeat hits a deterministic work limit or cancellation without unbounded stack growth; it never becomes a false no-match.

---

### Task 1: Characterization and internal result contract

**Files:**
- Create: `tests/Pcre/PcreByteFoundationTests.cs`
- Create: `src/Pcre/PcreByteResult.cs`
- Create: `src/Pcre/PcreByteOptions.cs`
- Create: `src/Pcre/PcreByteCompiler.cs`
- Create: `src/Pcre/PcreCompiledBytePattern.cs`
- Create: `src/Pcre/PcreByteMatcher.cs`

**Interfaces:**
- Produce internal compile result with either `PcreCompiledBytePattern` or diagnostic `(code, byte offset, message)`; match result with `IsSuccess`, optional `(ByteIndex, ByteLength, captures)`, and resource diagnostic.
- Options include `MatchWorkLimit` with the global default; reject nonpositive limits. Match rejects start offsets below zero or beyond input length with a diagnostic.

- [ ] **Write failing tests** for literal `ab` in `zab`, ordered alternatives `a|ab` and `ab|a`, empty `ab|` branch, high-byte/NUL literal pattern and exact source offsets, caller mutation after compile, unsupported `(?<=x)`/backslash diagnostics, invalid start offset, pre-canceled token, and a small-budget long literal search.
- [ ] **Run** `dotnet test tests/Icod.Expressions.Tests.csproj -c Staging --filter FullyQualifiedName~PcreByteFoundationTests` and record the missing compiler failure.
- [ ] **Add** the internal result/options types, `PcreByteCompiler.Compile(ReadOnlyMemory<byte> pattern, PcreByteOptions? options = null, CancellationToken cancellationToken = default)`, and `PcreCompiledBytePattern.Match(ReadOnlyMemory<byte> input, int startOffset = 0, CancellationToken cancellationToken = default)` with literal/ordered alternation, byte copying, bounded search, diagnostics, and cancellation.
- [ ] **Run** the focused test filter, then full `dotnet test Icod.Expressions.sln -c Staging` on .NET 8/9/10; commit.

### Task 2: Groups, quantifiers and capture rollback

**Files:**
- Modify: `src/Pcre/PcreByteCompiler.cs`
- Modify: `src/Pcre/PcreByteMatcher.cs`
- Modify: `src/Pcre/PcreCompiledBytePattern.cs`
- Modify: `tests/Pcre/PcreByteFoundationTests.cs`
- Modify: `doc/PCRE-DESIGN.md` with actual supported/rejected syntax status.

**Interfaces:**
- `PcreCompiledBytePattern.Match(ReadOnlyMemory<byte> input, int startOffset = 0, CancellationToken cancellationToken = default)` returns the Task 1 byte result; capture 0 is the whole match and numbered captures retain byte spans.

- [ ] **Write failing tests** for `(a|ab)c` on `abc`, `(a)(b)` capture positions, `a+`/`a+?` on `aaab`, `a*` on empty input, anchors `^`/`$`, a malformed quantifier diagnostic, `(a+)+$` under a deliberately small work limit, and cancellation during search triggered by an internal work observer (never by a timer).
- [ ] **Run** the focused filter and observe behavioral failures.
- [ ] **Implement** ordered backtracking with copy-on-branch or an undo log for captures, explicit bounded work/depth accounting, and safe zero-length repeats. Cancellation throws `OperationCanceledException`.
- [ ] **Run** focused and full solution Staging tests; update the syntax status; commit.
- [ ] **Verify** full build/test on Linux, Windows and macOS CI for net8/net9/net10, pack the single library, and run its package-only sample; request independent review.

The milestone is internal and incomplete by design. Do not migrate Grep, remove PCRE.NET, change CommandFramework, or describe this package as PCRE capable until the remaining design gates are met.
