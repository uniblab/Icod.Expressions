# Internal PCRE returned capture lists implementation plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox syntax for tracking.

**Goal:** Retain explicitly selected nested captures across nonrecursive calls.

**Architecture:** Globally resolve exact list headers into bounded sorted integer
selections carried by existing Call instructions. Omit selected capture-value
compensation during the existing shared-journal Return scan; preserve starts,
marks, K, guards and all retry checkpoints.

**Tech Stack:** C#13, net8/net9/net10, xUnit, pinned PCRE.NET1.6.0 / PCRE2 10.48 JIT.

**Spec:** `doc/PCRE-RETURNED-CAPTURES-DESIGN.md`.

## Global constraints

- Internal only; no public API, release, dependencies, workflows or consumer changes.
- Root project/solution, src, tests, samples; UTF-8 LF; no Python.
- Depth256/captures256/instructions4096/marks256/savedframes1024/default work1,000,000.
- Shared cancellation/journal/integer call contexts; no capture snapshots or saved spans.
- Local Debug; six native Staging PR jobs, full/frozen suites on net8/9/10.
- No local SDK: managed red/green is observed in CI; user owns merge.

## Review focus

- Retaining a selected result must still restore capture starts for open caller groups.
- Retrying a returned callee must remove its first retained result before trying another.
- Outer returns must independently filter values retained by an inner call.
- Duplicate names must retain every resolved slot, independent of declaration ordering.
- Even erased/unselected calls must validate every returned reference and body width.

## Task 1: Pin native contracts and publish expected-red fixtures

**Files:** Create tests/Pcre/PcreReturnedCaptureReferenceTests.cs and this design/plan;
modify tests/Pcre/PcreSubroutineReferenceTests.cs, current PCRE design/syntax/resource/
consumer inventories and historical subroutine/recursion-condition design notices.

**Interfaces:** Consume existing compile/match results, native PcreRegex8Bit and
diagnostic codes. Produce executable literal all-capture/diagnostic/resource fixtures.

- [ ] Research pinned source and native compiled/JIT outcomes; builder-check actual design.
- [ ] Write byte/UTF literal fixtures for exact five call forms, global/relative/duplicate
  selections, root/optional/unset/preexisting and empty captures, nested propagation,
  caller starts, retry/fallback/K, atomic/non-atomic/negative/conditional assertions,
  consumed-list behind rejection and ahead/DEFINE bypasses, zero-erased validation,
  UTF/G/CRLF/invalid offsets and resource gates.
- [ ] Remove only the two obsolete pending-list rows; retain recursion pending controls.
- [ ] Check every literal expectation with saved native probes; update current inventories.
- [ ] Commit tests/docs, verify exact published tree, open draft PR under issue #7.
- [ ] Observe completed CI test failures confined to expected missing list support;
  build and native assertions must pass before any production edit.

## Task 2: Implement bounded selections and prove all gates

**Files:** Modify src/Pcre/PcreByteParser.Subroutines.cs,
src/Pcre/PcreByteParser.NamedCaptures.cs, src/Pcre/PcreByteSyntax.cs,
src/Pcre/PcreByteParser.Lookbehind.cs, src/Pcre/PcreByteProgram.cs and
src/Pcre/PcreByteMatcher.cs. Create
src/Pcre/PcreByteParser.ReturnedCaptures.cs for exact list parsing/resolution if needed.

**Interfaces:** ParsedSubroutineCall carries optional parsed list references.
PcreByteSubroutineCall(Number,Offset,ReturnedCaptures) carries ReadOnlyMemory<int>.
PcreByteInstruction.CaptureNumbers carries sorted selection; Call operands and
CallScope(ReturnPc,Parent,ChangeCount,Number) remain unchanged.

- [ ] Parse exact nonempty comma lists on parenthesized calls, using shared name grammar
  and bounded cancellable ASCII-number conversion/relative source capture counts.
- [ ] Resolve every numeric/name reference globally, validating all entries before
  sorting/deduplication to at most256 targets; preserve whole-pattern/cycle rejection.
- [ ] Pass selection to Call payload and preserve it through existing entry patching.
- [ ] Reject nonempty selections when consumed width scheduling visits the call, at
  the owning behind; preserve existing ahead/DEFINE width bypasses and transitive checks.
- [ ] During Return, retrieve selection from caller.ReturnPc-1; skip compensation only
  for selected StateKind.Capture entries using bounded binary search after Step.
- [ ] Commit and publish exact local tree; run six full/frozen Staging jobs.
- [ ] Fresh independent whole-branch review against spec, plan and ledger; address
  material findings with observed RED→GREEN and green suites.
- [ ] Inspect every platform log, configured gates, full-range whitespace and all35
  frozen blobs; guard exact reviewed head/tree, update proof and mark ready.
