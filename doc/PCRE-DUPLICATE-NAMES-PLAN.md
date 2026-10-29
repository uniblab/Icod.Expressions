# Internal PCRE duplicate names implementation plan

> **For agentic workers:** Use superpowers:executing-plans for integration, a
> focused fixture worker and fresh read-only whole-branch review.

**Goal:** Implement local J and declaration-ordered duplicate named references.

**Architecture:** Extend the existing backreference node/instruction with shared
immutable candidate numbers, selecting participation once before shared matching.
Compile-time name registration and option snapshots preserve source-local policy.

**Tech Stack:** C#13; net8/net9/net10; xUnit; PCRE.NET1.6.0 / PCRE2 10.48 JIT.

**Spec:** `doc/PCRE-DUPLICATE-NAMES-DESIGN.md`.

## Global constraints

- Root project/solution, src source, tests tests; UTF-8 LF; no Python.
- Local Debug, six PR Staging OS/architecture jobs; no local SDK, CI red/green.
- Preserve instruction4096, depth256, capture256, saved paths1024, work1,000,000.
- No API/version/dependency/consumer/resource policy or frame/journal change.
- Preserve local i/r, UTF/UCP gates and existing numeric reference fast path.

## Review focus

1. Declaration order can differ from numeric order with branch-reset slots.
2. First historical name controls validation when J is disabled, even for aliases.
3. Empty captures participate; a selected mismatch cannot try another capture.
4. Lookbehind rejects actual consumed duplicate names, not J or numeric controls.
5. Every participation lookup shares work and cancellation without match allocation.

### Task 1: Native contract and regression fixtures

**Files:** Create `tests/Pcre/PcreDuplicateNameReferenceTests.cs`; replace obsolete
J-pending rows in named/branch-reset/reset fixtures; update current inventories
and historical named/reset design notices.

**Interfaces:** Existing internal Compile/Match/options and native Compiled/JIT;
explicit selected outcomes/group counts, followed by all numbered byte captures.

- [ ] Cover design verification classes and all five review risks, with distinct
  first-empty/first-mismatch, source-order and first-history validation controls.
- [ ] Publish test-only draft PR from merged #85. Observe expected missing-J
  failures, inspect stacks for native expectation errors and record evidence.

### Task 2: Option scope and ordered capture selection

**Files:** `src/Pcre/PcreByteParser.cs`, `PcreByteParser.Groups.cs`,
`PcreByteParser.NamedCaptures.cs`, `PcreByteParser.Lookbehind.cs`,
`PcreByteSyntax.cs`, `PcreByteProgram.cs`, `PcreByteMatcher.cs`.

**Interfaces:** Private bool duplicateNames in parser/GroupOptions; name maps to
List<int> during parse, frozen shared arrays during reference resolution.
PcreByteBackreference(int Number, ReadOnlyMemory<int> DuplicateNumbers=default);
optional CaptureNumbers on instruction/Emit. No new opcode or runtime stack.

- [ ] Set/unset/snapshot J transactionally; reset preserves it. Register names in
  declaration order, validate first historical number before deduplicating.
- [ ] Resolve all named references globally, preserving i/r and sharing frozen
  multi-slot arrays; retain single-slot numbered form for unique names.
- [ ] Reject duplicate consumed references in ReferenceTarget at behind opener.
- [ ] Forward candidate memory through compiler; Step each candidate lookup and
  commit to first participating slot before existing byte/UTF comparison.
- [ ] Whitespace/frozen checks; publish implementation and inspect CI; resolve
  concrete failures with focused red/green evidence.
- [ ] Fresh whole-branch review; fix findings. Verify exact head/tree, six green
  Staging jobs with full/frozen per-framework counts and configured gates.
- [ ] Record evidence and mark PR ready; user retains merge control.
