# Caller Empty-Match Policy Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: superpowers:executing-plans. Execute inline; standing user direction prohibits subagents and approval pauses.

**Goal:** Allow caller-selected NotEmpty/NotEmptyAtStart through existing root acceptance.
**Architecture:** Initialize existing global start settings from two owned booleans; OR source prefixes into the same settings and reuse unchanged bytecode/matcher logic.
**Tech Stack:** C#13; library net8.0/net9.0/net10.0; tests net10.0; xUnit and pinned PCRE2 10.48/JIT reference.
**Spec:** doc/PCRE-CALLER-EMPTY-MATCH-DESIGN.md.

## Global Constraints

- Work from merged248 74af49bb62fcad01f79fdaff2fb26b29abf6611d/tree7e48200e810e252d9d45f035e7740ac73c36b85f; baseline86,855 tests.
- Preserve all265 prior test files/260 C# files and all35 frozen production blobs.
- Freeze new841-case fixture after clean tests-only RED, before production.
- Two production files only; no matcher/IR/opcode/journal/limit/API/dependency/workflow changes.
- Library builds8/9/10; only net10 test execution across six native hosts;214 frozen outcomes/120 methods each.
- User owns merge/tag/publish; inline/no agents/Python/local SDK installation; Grep last.
- Seven inventories before final source push; final evidence on PR249/issue7 footer only.

## Review Focus

1. Reported empty endpoints after K, including resets in assertions, must not be confused with consumed length or original candidate.
2. Retry rejected root acceptance without changing scoped assertion/callee success, captures, cuts or labels.
3. Requested offset remains the ATSTART boundary across candidate restarts, malformed fragments, SKIP, FirstLine and anchoring.
4. Source prefixes OR into caller flags; Literal and local reset never reinterpret or unset caller policy.
5. Preserve validation priority, owned options/source, exact/exhausted work, cancellation and full capture/depth/instruction capacity.

### Task 1: independently pinned tests-only fixture

**Files:** tests/Pcre/PcreCallerEmptyMatchReferenceTests.cs; this design and plan.
**Interfaces:** native Match(subject, offset, PcreMatchOptions) versus managed Compile(source, PcreByteOptions).Match(subject, offset); optional reflection setter allows a genuine tests-only baseline.

- [x] Pin 754 native groups/26,212 protocols and60 diagnostics, validating native batches before managed comparison.
- [x] Add27 work/ownership/validation/capacity cases; reported endpoints, scope/restart and caller/source/Literal composition cover all five review boundaries.
- [x] Publish tests-only draft PR249 against merged248; no production changes.
- [x] Observe clean net10 RED:547 failures/87,149 passes/87,696 total/zero skips/zero compiler warnings/errors; all86,855 previous tests pass.
- [x] Record exact source, fixture hash and RED result; freeze fixture.

### Task 2: caller policy initialization

**Files:** src/Pcre/PcreByteOptions.cs, src/Pcre/PcreByteStartSettings.cs; seven inventories.
**Interfaces:** add public init-only internal-record bools NotEmpty/NotEmptyAtStart; existing settings constructor and program compiler consume owned bool values.

- [x] Add the two default-false properties and initialize notEmpty/options.NotEmpty and notEmptyAtStart/options.NotEmptyAtStart before prefix scanning.
- [x] Preserve Literal compatibility, OR source directives and all existing program/matcher/resource paths.
- [x] Update PCRE-DESIGN, syntax matrix, resource contract, Grep inventory, roadmap status and this design/plan before source qualification.
- [ ] Publish source; require six net10 results/87,696 tests each, zero warnings/errors/failures/skips,214 frozen/120 methods and all three library outputs per host.
- [ ] Verify every configured supporting gate, frozen35 and all prior/frozen test hashes.

### Task 3: author review and delivery

**Files:** PR249 body, issue7 footer; no documentation-only CI repeat.
**Interfaces:** exact reviewed source/base/tree/proposed merge and completed native job logs.

- [ ] Review whole branch and all five review boundaries; inspect final diff separately after implementation.
- [ ] Verify clean worktree, immutable fixture/prior sources and exact source/base/tree/proposed-merge ordered parents.
- [ ] Record final GREEN/review/identities on PR249 and only issue7 footer, preserving its original2600-character prefix.
- [ ] Mark ready; retire only this plan's identity-checked ledger, preserving branch/worktree. Leave user merge/tag/publish.

## Tests-first checkpoint

Tests-only [RED757](https://github.com/uniblab/Icod.Expressions/actions/runs/43606687443), source `77824ef8a9f0e3c8b45fd2234ac729f1aedee165`, macOS ARM64 job132482835413: net10 built with zero compiler warnings/errors and ran **547 expected failures / 87,149 passes / 87,696 total / zero skips**. Exactly 538 missing-policy protocol groups, six retry-work and three ownership cases failed after successful native batches; all 86,855 earlier tests and 294 new controls passed. Freeze `e99bbdac4a0c2c33c22d1ffe1234bad7dc25de22` before production. Before clean RED, coverage review added 36 groups/1,300 protocols for assertion resets and caller-only local reset, retaining all original 805 expectations.

Remaining qualification/review/delivery checkboxes are fulfilled by final PR249/issue7 evidence; do not push a documentation-only CI repeat.
