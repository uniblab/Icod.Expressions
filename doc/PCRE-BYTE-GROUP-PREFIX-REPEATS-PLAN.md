# Repeated Byte Group Prefix Implementation Plan

> REQUIRED SUB-SKILL: superpowers:executing-plans inline. No subagents.

**Goal:** Preserve native exact group-copy prefix scanning and last-capture spans.
**Architecture:** Bound repeated group child scheduling inside the existing proof;
existing VM/compiler lowering and #117 selector unchanged.
**Tech Stack:** C#13/net8/net9/net10, xUnit, PCRE.NET1.6.0/PCRE2 10.48 JIT.
**Spec:** PCRE-BYTE-GROUP-PREFIX-REPEATS-DESIGN.md.

## Constraints and review focus

No agents/Python/SDK install, public/package/workflow/release/consumer/fallback or
raised-limit changes. Frozen35 intact; original spans/captures/work preserved.
1. Group copies differ from direct XCLASS+CRRANGE; no bitmap/source-kind inference.
2. Multi-atom/nested repeats accumulate byte offsets; conditional misses still skip LF.
3. Last occurrence/atomic captures, local case/UCP, explicit source flags stay native.
4. Count1/2/11/12, pending work1000, no recursive/unbounded group expansion.
5. Work/observer/exhaustion/cancellation and UTF/resource gates remain shared.

## Task1: Pin native group-copy contracts and observe RED

Files: tests/Pcre/PcreByteGroupPrefixRepeatReferenceTests.cs; design/plan.
Interfaces: existing Compile/Match and pinned reference APIs only.
- [ ] Literal full-capture positive/miss pairs, nested/multi-atom offsets, group/
  atomic/possessive forms and direct-property/bitmap controls.
- [ ] All offsets over six newlines/two UCP choices; supported UTF, work/budget/
  cancellation/deep/compile-ceiling controls; native spans bounded.
- [ ] Publish tests-only draft, observe successful-build RED; old5416 baseline pass.

## Task2: Bounded proof scheduling and verified handoff

Files: src/Pcre/PcreByteProgram.cs; four canonical inventories.
Interfaces: existing FindByteCrLfPrefix bool/optional literal result; new private
local `bool AddCopies(PcreByteNode node,int count)` consumes the existing heap stack.
- [ ] Exact positive1..11 repeat whose direct child is ordinary/atomic group pushes
  its child count times after pending/visits bound check. No other repeat shape.
- [ ] Existing traversal/width/source-kind/sole-literal/cancellation checks apply to
  every copy. No parser/AST/matcher/VM/state/limit edits.
- [ ] Six native Staging jobs/net8/9/10 full/frozen214/configured gates green,
  zero failed/skipped/build warnings/errors and seven Linux workload equalities.
- [ ] Whole-branch author review, exact head/tree/log/frozen/whitespace/clean audit;
  update #99/PR, mark ready; user owns merge.

Pre-flight: tests use stable APIs; helper schedules existing immutable node refs,
compiler proof alone consumes them. Group records/AST rewriting already preserve
wrappers and capture numbers. No task interface conflict; standing scope applies.
