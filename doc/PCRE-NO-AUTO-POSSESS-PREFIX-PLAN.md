# NoAutoPossess Source Prefix Implementation Plan

> Execute inline under standing user direction; no subagents or approval pauses.

**Goal:** Recognize the native pattern-start NoAutoPossess directive.
**Architecture:** Resolve caller/source OR in the existing prefix scanner and pass that bool to the existing compiler gate.
**Tech Stack:** C#13; library net8/net9/net10; tests net10; pinned PCRE2 10.48/JIT0.
**Spec:** doc/PCRE-NO-AUTO-POSSESS-PREFIX-DESIGN.md.

## Global Constraints

- Base243f83edf3026961ec7d2353ee2527dcdc5a89e9/tree7f34417770270df97b167e846c315682dbf4d306;91,579 baseline.
- All278 prior test files/273 C# sources/frozen35 fixed; freeze3 new C# blobs after clean RED.
- Only PcreByteStartSettings.cs and PcreByteCompiler.cs change in production; reuse the existing program gate.
- Source prefix is exact uppercase, uninterrupted start-only, idempotent and global; Literal bypasses prefix scanning.
- Seven inventories before final qualification; final evidence on PR254/issue7 footer; user merges/tags/publishes; Grep last.

## Review Focus

1. Exact start-only placement, repeated/interleaved prefixes, and inert Literal/quoted/class/comment text.
2. Caller/source OR, immutable compiled policy and ordinary source option resets.
3. Original-byte length, whole-source UTF, caller lockouts and parser offsets, preserving admission priority/no-read behavior.
4. Sliced source ownership, every signed capture and successful mark across retries.
5. Stable work traces/exact budgets/cancellation, full capture/depth/instruction capacities and UTF/offset validation.

### Task1: native-first tests-only fixture

**Files:** create tests/Pcre/PcreNoAutoPossessPrefixReferenceTests.cs and PcreNoAutoPossessPrefixProtocols.Part1.cs/Part2.cs; current design/plan.
**Interfaces:** existing caller NoAutoPossess/MaxPatternLength, native source prefix, compile bit0x4000 and match flags0/4/8/12.

- [x] Pin576 matching groups/164 diagnostics/36 supplementary cases, native assertions before managed comparisons.
- [ ] Publish draft254 with production identical to merged253.
- [ ] Observe584 managed failures/91,771 passes/92,355 total; verify exact failure keys and post-native assertion frames, zero skips/compiler warnings/errors.
- [ ] Freeze3 new C# blobs with exact source/tree/job evidence before production.

### Task2: resolve global source prefix

**Files:** src/Pcre/PcreByteStartSettings.cs and PcreByteCompiler.cs; seven inventories.
**Interfaces:** bool NoAutoPossess on start settings, caller initialization/exact-prefix consumption, resolved bool to existing PcreByteProgramCompiler.

- [ ] Add setting initialized from options.NoAutoPossess and set true on exact prefix; preserve original source/body offset and Literal bypass.
- [ ] Pass settings.NoAutoPossess to existing program constructor without modifying matcher/emission.
- [ ] Update all seven inventories before final source push.
- [ ] Require six fresh92,355 net10 results/214 frozen/120 methods, all3 library outputs/supporting gates.

### Task3: review and delivery

**Files:** PR254 body/issue7 footer; own ignored ledger.
**Interfaces:** completed source-pinned logs; exact source/base/tree/proposed-merge ordered parents; immutable issue prefix.

- [ ] Whole-branch author review across five boundaries; clean diff/frozen/prior checks.
- [ ] Record final GREEN/review/identities externally, mark ready/readback without docs-only CI repeat.
- [ ] Retire only this plan's identity-checked ledger; preserve branch/worktree.
