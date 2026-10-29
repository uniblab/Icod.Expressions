# Internal PCRE capture conditions implementation plan

> **For agentic workers:** Use superpowers:executing-plans for integration,
> a focused fixture worker and fresh read-only whole-branch review.

**Goal:** Implement numeric/relative/named capture-participation conditionals.

**Architecture:** Post-parse immutable target resolution plus one capture-test
branch instruction; existing body nodes, journal, scopes and retries remain shared.
Native explicit-branch width analysis remains distinct from runtime implicitelse.

**Tech Stack:** C#13; net8/net9/net10; xUnit; PCRE.NET1.6.0 / PCRE2 10.48 JIT.

**Spec:** `doc/PCRE-CAPTURE-CONDITIONS-DESIGN.md`.

## Global constraints

- Root project/solution, src source, tests tests; UTF-8 LF; no Python.
- Local Debug, six PR Staging platforms; no local SDK, CI red/green.
- Preserve depth256, capture256, instructions4096, saved paths1024, work1,000,000.
- No matcher recursion, capture snapshots, frame/scope/journal changes, public
  API/version/dependency/release/consumer/resource-policy change.
- Preserve J name order/ANY participation, native reserved-name priority and
  explicit-branch behind widths while retaining nullable runtime omitted else.

## Review focus

1. Chosen branch failure cannot fall into the other branch; outer retry retests.
2. Duplicate names test ANY participation; empty and previous-iteration captures
   count, while numeric targets remain slot-specific.
3. Relative/forward target resolution respects n, branch reset and original bytes.
4. Native omitted-else width differs from explicit empty else, independently of
   synthetic invalid-UTF candidate nullability.
5. Reserved ASCII Rdigits/DEFINE/VERSION-prefix names and unsupported condition kinds never silently
   acquire capture semantics; all branch bodies still validate before erasure.

### Task 1: Native contract, fixtures and inventories

**Files:** Create `tests/Pcre/PcreCaptureConditionReferenceTests.cs`; replace obsolete
capture-condition pending row; update current/historical inventories.

**Interfaces:** Existing internal Compile/Match/options and pinned native JIT;
explicit outcomes/group counts and all numbered source-byte captures.

- [ ] Cover all grammar/semantic/resource classes and five review risks, including
  no-fallback, reserved-name controls and omitted/explicit-else behind differences.
- [ ] Publish test-first draft from merged #88. Observe missing-condition red CI,
  inspect native assertions and record exact evidence before production edits.

### Task 2: Heap parsing, resolution, lowering and capture test

**Files:** Create `src/Pcre/PcreByteParser.Conditions.cs`; modify
`PcreByteSyntax.cs`, `PcreByteParser.cs`, `PcreByteParser.Groups.cs`,
`PcreByteParser.TreeRewrite.cs`, `PcreByteParser.Lookbehind.cs`,
`PcreByteProgram.cs`, `PcreByteMatcher.cs` under `src/Pcre/`.

**Interfaces:** private CaptureCondition token/ParsedCaptureConditional body;
optional GroupFrame.Condition; ParseCaptureCondition(int offset) and
ResolveCaptureConditions(PcreByteAlternatives). Semantic
PcreByteCaptureConditional(ReadOnlyMemory<int> CaptureNumbers,Body).
New CaptureTest opcode uses First=yes pc, Second=no pc and existing CaptureNumbers.

- [ ] Parse numeric/relative/name headers exactly; distinguish reserved/pending
  forms, preserve source contexts and enforce two direct branches/no capture slot.
- [ ] Traverse/rewrite temporary and resolved bodies; globally resolve targets and
  cache immutable lists after named fixups, before lookbehind analysis.
- [ ] Add conditional body width traversal over explicit branches only; nullable
  program analysis includes runtime implicitelse; explicit-newline scan visits body.
- [ ] Heap-compile yes/no sequences with test/end jump and no conditional Split.
  Step-poll each participation lookup; branch without consuming or snapshotting.
- [ ] Whitespace/frozen checks; publish implementation and inspect CI. Resolve
  concrete failures with focused red/green evidence.
- [ ] Fresh whole-branch review; fix findings, inspect exact final head/tree and
  six green Staging jobs/full/frozen counts/configured gates. Finalize PR ready;
  user retains merge control.
