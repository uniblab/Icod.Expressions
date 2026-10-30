# Internal PCRE recursive and whole-pattern calls implementation plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox syntax for tracking.

**Goal:** Enable bounded actual recursive and whole-pattern calls in the shared VM.

**Architecture:** Permit cyclic integer call edges and resolve number0 to a
synthetic group over the semantic root. Reuse out-of-line target compilation,
Call/Return, stable integer scopes, shared journals and existing resource limits.

**Tech Stack:** C#13, net8/net9/net10, xUnit, PCRE.NET1.6.0 / PCRE2 10.48 JIT.

**Spec:** `doc/PCRE-RECURSIVE-CALLS-DESIGN.md`.

## Global constraints

- Internal only; no API/package/release/dependency/workflow/consumer changes; no Python.
- Root project/solution, src, tests, samples; UTF-8 LF; C#13, net8/net9/net10.
- Depth256/captures256/instructions4096/marks256/savedframes1024/default work1,000,000.
- Shared cancellation/journal/Call/Return/contexts; no snapshots, saved spans or CLR recursion.
- No local SDK; managed red/green uses six native Staging jobs; user owns merge.

## Review focus

- Capture-changing same-position recursion must succeed without an interpreter loop heuristic.
- Whole-root target0 must preserve options/anchors, K and each call's returned selection.
- A returned recursive call must remain retryable with restored caller starts and context.
- Unproductive first branches must report bounded resource errors, not fall through as misses.
- Consumed recursive behind dependencies must reject before zero erasure; ahead/DEFINE skips remain.

## Task 1: Pin native contracts and expected-red fixtures

**Files:** Create tests/Pcre/PcreRecursiveCallReferenceTests.cs and this design/plan;
modify obsolete pending-recursion tests in PcreSyntaxAcceptanceTests,
PcreGrepConsumerFixtureTests, PcreStaticConditionReferenceTests,
PcreCaselessRestrictionReferenceTests, PcreSubroutineReferenceTests and
PcreReturnedCaptureReferenceTests; update current
PCRE design/syntax/resource/consumer inventories and historical call-design notices.

**Interfaces:** Consume existing compiler/match results, diagnostics, native
PcreRegex8Bit. Produce literal all-capture, compile-diagnostic and resource fixtures.

- [ ] Research pinned JIT/source contracts and builder-check the actual design/plan.
- [ ] Write native-first byte/UTF fixtures for numbered/named/relative/g/mutual/duplicate
  recursion, R/zero whole-root forms and lists, zero-progress capture-state changes,
  palindrome/internal retry, nested return filtering/caller starts/K, atomic/assertion/
  current-call conditions, source/compile/frame/work/cancel and malformed UTF/G/CRLF offsets.
- [ ] Pin owning-behind recursive/{0}/transitive rejection, ahead/DEFINE bypasses and
  existing suppressed-metadata exclusion; preserve nonrecursive width controls.
- [ ] Remove obsolete pending-recursion expectations; retain remaining pending families.
- [ ] Rerun saved native checkers, whitespace and frozen35; commit/publish exact tree,
  open tests/docs draft and observe managed expected missing-feature failures before code.

## Task 2: Enable cyclic edges and whole-root target0, then prove gates

**Files:** Modify src/Pcre/PcreByteParser.Groups.cs,
src/Pcre/PcreByteParser.Subroutines.cs and src/Pcre/PcreByteParser.Lookbehind.cs.
Compiler/matcher should require no behavior changes; inspect and adjust only if
native fixture evidence proves a missing shared invariant.

**Interfaces:** ParsedSubroutineCall/PcreByteSubroutineCall number0 identifies
whole pattern. SubroutineTargets may include key0 mapped to PcreByteGroup(0,root).
Existing Compile(root,targets), Call.First(entry)/Second(number)/CaptureNumbers,
CallScope(ReturnPc,Parent,ChangeCount,Number) remain unchanged.

- [ ] Recognize ?R at group entry and exact R header in call parser; permit absolute
  zero/g-zero; preserve name-R g forms, signed-zero errors and original byte diagnostics.
- [ ] Keep global call/list target validation but remove the global call-graph DFS rejection.
- [ ] Collect whole-root target0 over the final resolved root once; retain first-definition
  groups and unresolved-suppressed-behind exclusion for every referenced target.
- [ ] Reject consumed call0 at owning behind before target lookup; existing active width
  dependency chain rejects numbered recursion, with ahead/DEFINE skip policies unchanged.
- [ ] Commit/publish exact tree; observe all six full/frozen Staging jobs and configured gates.
- [ ] Fresh independent whole-branch review against spec/plan/review focuses/ledger;
  address material findings with observed RED→GREEN and green suites.
- [ ] Inspect six platform logs, completed-success run, exact reviewed head/tree, full-range
  whitespace and frozen35; update proof and mark ready, preserving worktree for feedback.
