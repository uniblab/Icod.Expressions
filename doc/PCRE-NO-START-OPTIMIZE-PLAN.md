# NoStartOptimize Implementation Plan

> Execute inline under standing user direction; no subagents or approval pauses.

**Goal:** Add caller NoStartOptimize with native candidate-entry/cache behavior.
**Architecture:** Compile-time proof gates and owned program bool; two private accelerator guards at match setup.
**Tech Stack:** C#13; library net8/net9/net10; tests net10; pinned PCRE2 10.48/JIT0.
**Spec:** doc/PCRE-NO-START-OPTIMIZE-DESIGN.md.

## Global Constraints

- Base463ae290d4281445bedb0cfcc724dd4942004047/tree6e26348ba1af4c35ec7b547a1dba9d1d7daf224f;92,355 baseline.
- Preserve281 prior test files/276 C# sources/frozen35; freeze3 new C# blobs after clean RED.
- Four production files only; no AST/parser/opcode/frame/diagnostic/API/dependency/workflow changes.
- Defaultfalse; Literal-compatible; independent NoAutoPossess/explicit anchors/newline provenance/nullable fragments.
- Seven inventories before qualification; final evidence on PR255/issue7 footer; user merges/tags/publishes; Grep last.

## Review Focus

1. Gate every start-sensitive proof, including PRUNE detect_early_fail and both private setup accelerators.
2. Preserve independent possessification/explicit anchors, intrinsic newline provenance and nullable UTF fragments.
3. FIRSTLINE forced SKIP bypasses filters only when appropriate; ordinary candidate windows stay intact.
4. Literal compatibility/admission priority and immutable compiled source/flag ownership.
5. Signed captures/marks, observed/unobserved exact budgets/cancellation and full capture/depth/instruction capacities.

### Task1: native-first tests-only fixture

**Files:** create tests/Pcre/PcreNoStartOptimizeReferenceTests.cs and PcreNoStartOptimizeProtocols.Part1.cs/Part2.cs; current design/plan.
**Interfaces:** native compile bit0x10000; optional reflection setter on baseline internal options; existing NoAutoPossess/MaxPatternLength.

- [x] Pin562 groups/68 diagnostics/28 supplementary cases; native batches before managed comparisons.
- [ ] Publish draft255 with production identical to merged254.
- [ ] Observe130 managed failures/92,883 passes/93,013 total, exact failure keys and post-native frames, zero skips/compiler warnings/errors.
- [ ] Freeze3 new C# blobs with source/tree/job evidence before production.

### Task2: caller option and proof/accelerator gates

**Files:** src/Pcre/PcreByteOptions.cs, PcreByteCompiler.cs, PcreByteProgram.cs, PcreByteMatcher.cs; seven inventories.
**Interfaces:** bool NoStartOptimize defaultfalse; program/compiler copy; existing proof discovery and setup fast paths.

- [ ] Add Literal-compatible caller bool and pass it into program construction.
- [ ] Gate UTF start/repeat/byte-prefix/suffix/COMMIT/PRUNE-cache/line-start metadata; resolve FIRSTLINE forced-SKIP bypass; keep NoAuto proof and intrinsic newline/empty facts independent.
- [ ] Disable private unobserved byte/required-suffix accelerators once at setup.
- [ ] Update all seven inventories before final source push.
- [ ] Require six fresh93,013 net10 results/214 frozen/120 methods, all3 library outputs/supporting gates.

### Task3: review and delivery

**Files:** PR255 body/issue7 footer; own ignored ledger.
**Interfaces:** completed source-pinned logs; exact source/base/tree/proposed-merge ordered parents; immutable issue prefix.

- [ ] Whole-branch author review against five boundaries; clean diff/frozen/prior checks.
- [ ] Record final GREEN/review/identities externally; mark ready/readback without docs-only CI repeat.
- [ ] Retire only this plan's identity-checked ledger; preserve branch/worktree.
