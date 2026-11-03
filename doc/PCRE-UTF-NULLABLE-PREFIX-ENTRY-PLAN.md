# UTF finite and empty nullable-prefix continuation implementation plan

**Goal:** Extend #146's bounded group continuation to finite/omitted/empty bodies.
**Architecture:** Reuse the existing compile-time continuation walker and proof;
preserve first-star ownership and all runtime code.
**Tech:** C#13/net8/9/10, xUnit, pinned PCRE.NET1.6.0/PCRE2 10.48 JIT.
**Spec:** PCRE-UTF-NULLABLE-PREFIX-ENTRY-DESIGN.md; inline execution/no subagents.

## Task1: Native fixtures and observed RED

Files: tests/Pcre/PcreUtfNullablePrefixEntryReferenceTests.cs; design/plan.
- [ ] Native full capture literals and bounded protocol checks for the scope and
  controls in the design, all reference compilations JITpositive/spansbounded.
- [ ] Work/cancellation/default budget/parser/capture/backtrack cases.
- [ ] Tests-only draft, successful-build new-class RED/all16306baseline pass.

## Task2: Reuse bounded fallback without requiring a first star

Files: src/Pcre/PcreByteProgram.cs; four canonical inventories.
- [ ] Explicit UTF guard, run fallback after old-proof miss without star guard.
- [ ] Fallback-only empty single-branch acceptance; returned groups set retained
  BlocksSkip. Update ownership comment; no other proof/runtime changes.
- [ ] Keep original tests/expectations unchanged after RED; six native Staging
  full/frozen214 ×net8/9/10/configured gates/7equal workloads,0failed/skipped
  tests/compiler warnings/errors. Author self-review and strict logs/exacthead/
  tree/blob/testimmutability/frozen/clean audit. Issue102 stays open; ready PR
  remains unmerged for user review.
