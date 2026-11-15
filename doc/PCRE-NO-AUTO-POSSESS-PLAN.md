# NoAutoPossess implementation plan

> Execute inline under standing user direction; no subagents or approval pauses.

**Goal:** Caller control of the existing bounded automatic possessification proof.
**Spec:** doc/PCRE-NO-AUTO-POSSESS-DESIGN.md.
**Architecture:** bool option, Literal compatibility check, one program-construction gate.
**Tech stack:** C#13; library net8/net9/net10; tests net10; pinned PCRE2 10.48/JIT0.

## Global constraints

- Base7e55bbf2044b788bb5be6ec27f5d4a75fcfd317c/treefb7b7dacee2c9c8e49992d1770beb85fcd5a70fa;90,703 baseline.
- Preserve275 prior test files/270 C# sources/frozen35 and freeze3 new C# blobs after clean RED.
- Only three production files; no parser/matcher/opcode/frame/API/workflow/dependency changes.
- Seven inventories before final source qualification; final evidence on PR253/issue7 footer only; user merges/tags/publishes; Grep last.

## Review focus

1. Suppress only the existing terminal optional literal proof, preserving explicit possessive/atomic commits and broader optimizer gaps.
2. Pass and own the global option once; source resets/caller mutation cannot clear it.
3. NoAutoPossess+Literal returns InvalidSyntax0 before length/UTF; cancellation/work retain earlier priority.
4. Preserve reversed signed spans, every capture and successful mark during fallback retries.
5. Preserve work budgets/cancellation, full capture/depth/instruction capacities, UTF and offset admission.

### Task1: tests-only oracle fixture

**Files:** tests/Pcre/PcreNoAutoPossessReferenceTests.cs and PcreNoAutoPossessProtocols.Part1.cs/Part2.cs; current design/plan.
**Interfaces:** native compile bit0x4000; optional reflection for baseline options; empty match flags0/4/8/12.

- [x] Pin796 matching groups/57 diagnostics/23 supplementary cases; native preflight before managed comparisons.
- [ ] Publish tests-only draft253 with production identical to merged252.
- [ ] Observe110 expected managed failures/91,469 passes/91,579 total/zero skips/compiler warnings/errors; check exact failure keys and post-native stack boundaries.
- [ ] Freeze3 new C# blobs with source/tree/job identities before production.

### Task2: compile-proof gate

**Files:** src/Pcre/PcreByteOptions.cs, PcreByteCompiler.cs, PcreByteProgram.cs; seven inventories.
**Interfaces:** bool NoAutoPossess defaultfalse; constructor copy; !noAutoPossess gate; HasLiteralOptionConflict.

- [ ] Add option/Literal conflict and pass to program compiler; suppress bounded proof only.
- [ ] Update all seven inventories before source qualification.
- [ ] Require six fresh91,579 native net10 results/214 frozen/120 methods, all3 library outputs and supporting gates.

### Task3: review and delivery

**Files:** PR253 body/issue7 footer; own ignored ledger.
**Interfaces:** source-pinned completed logs; exact source/base/tree/proposed-merge ordered parents; immutable issue prefix.

- [ ] Author whole-branch review against five boundaries; clean diff/frozen/prior checks.
- [ ] Record final GREEN/review/identities externally; mark ready/readback without docs-only CI repeat.
- [ ] Retire only this plan's identity-checked ledger, preserving branch/worktree.
