# Internal PCRE recursion conditions implementation plan

> **For agentic workers:** Use superpowers:executing-plans for root integration,
> one focused fixture worker and one fresh read-only whole-branch review.

**Goal:** Test the innermost active nonrecursive call with R/Rdigits/R&name.

**Architecture:** Global header/name resolution, distinct recursion-condition
semantic nodes, shared conditional lowering and a group number in call scopes.

**Tech Stack:** C#13, net8/net9/net10, xUnit, PCRE.NET1.6.0/PCRE2 10.48 JIT.

**Spec:** `doc/PCRE-RECURSION-CONDITIONS-DESIGN.md`.

## Global constraints

- Internal only, no API/package/version/dependency/workflow/consumer changes.
- Root project/solution, src, tests, samples; UTF-8 LF; no Python/native fallback.
- Depth256/captures256/instructions4096/marks256/savedframes1024/work1,000,000.
- One new integer per call arena record (16KiB max payload); saved frames unchanged.
- Local Debug; no SDK, executable managed red/green through six-platform Staging CI.
- Actual recursion/whole-pattern calls/returned captures remain pending.

## Review focus

1. R/Rdigits name shadowing is global and precedes bounded numeric interpretation;
   R&name forces recursion, duplicates test ANY current target, wrapped names stay captures.
2. Only the innermost active call counts; return, retry and pruning cannot expose
   inactive guard records or mistake ordinary capture nesting for a call.
3. Conditional branch choice is committed while internal alternatives may retry;
   captures/K and current call IDs restore through shared assertion/journal paths.
4. All-body traversal, omitted else and dynamic explicit width bounds preserve
   forward declarations, hidden diagnostics, zero-count validation and metadata.
5. Call target numbers survive entry-address patching; scope memory, guard/work/
   cancellation and deep compile limits remain bounded without ancestor scans.

### Task 1: Native contract, design and failing fixtures

**Files:** Create design/plan and `tests/Pcre/PcreRecursionConditionReferenceTests.cs`;
remove obsolete pending R rows from `PcreCaptureConditionReferenceTests.cs`;
update design/syntax/resource/consumer inventories and historical scope notices.

**Interfaces:** Existing internal Compile/Match/options and compiled JIT oracle.

- [ ] Native exact header/current-context probes and actual design/plan builder check.
- [ ] Explicit native outcome/all-capture fixtures for all five review risks,
  plus exact byte diagnostics and shared compile/runtime/cancellation controls.
- [ ] Publish tests/docs-only draft, inspect completed job's expected missing-
  feature failures before production edits; record exact head/tree/run evidence.

### Task 2: Global resolution and bounded context test

**Files:** Modify Syntax, parser Conditions/TreeRewrite/Lookbehind, Program and
Matcher. Add parser RecursionConditions partial for numeric fallback if useful.

**Interfaces:** PcreByteRecursionConditional(ReadOnlyMemory<int> GroupNumbers,
PcreByteAlternatives Body); CallScope(ReturnPc,Parent,ChangeCount,Number);
RecursionTest opcode using First/Second branch PCs and CaptureNumbers targets.

- [ ] Parse forced R&name and resolve bare R/digits after global capture-name lookup;
  zero/empty suffix means any call, positive targets validate against group count.
- [ ] Traverse/rewrite recursion bodies; share dynamic width/nullability/newline
  policies and conditional lowering, parameterized by opcode/targets/body.
- [ ] Preserve target number in Call.Second during entry patching; save in current
  call scope. Test only activeCall/current Number with per-candidate work checks.
- [ ] Whitespace/frozen35/exact-tree checks; publish and inspect full six-job CI.
  Diagnose concrete failures and fix through observed focused red/green evidence.
- [ ] Fresh whole-branch review, resolve findings, inspect final exact head/tree,
  all six full/frozen/configured gates; ready PR for the user's merge.
