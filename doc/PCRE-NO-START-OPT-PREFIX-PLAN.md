# NoStartOpt Source Prefix Implementation Plan

> Execute inline under standing user direction; no subagents or approval pauses.

**Goal:** Recognize the native pattern-start NoStartOpt directive through the existing caller policy.
**Architecture:** Resolve caller/source OR in the prefix scanner; route the same bool to parser metadata and program construction.
**Tech Stack:** C#13; library net8/net9/net10; tests net10; pinned PCRE2 10.48/JIT complete return0.
**Spec:** doc/PCRE-NO-START-OPT-PREFIX-DESIGN.md.

## Global Constraints

- Base50b53f0f8896235004d879546e660e9640830867/treea40723746f8f42d24d3cca0029ac0db40f7203ce;92,998 baseline.
- Preserve284 prior test files/279 C# sources/frozen35; freeze3 new C# blobs after clean RED.
- Three production files: PcreByteStartSettings.cs, PcreByteCompiler.cs, PcreByteParser.cs; no grammar/AST/matcher/program-emitter/proof/limit/API/dependency/workflow changes.
- Exact uppercase, uninterrupted start-only, repeated/interleaved/idempotent global OR; Literal bypasses scanning.
- Seven inventories before final qualification; final evidence on PR256/issue7 footer; user merges/tags/publishes; Grep last.

## Review Focus

1. Exact prefix placement/spelling/repetition versus inert Literal/quoted/class/comment contexts.
2. The same resolved bool reaches parser interior-range metadata and every existing program/matcher gate; no-prefix caller behavior retained.
3. Global OR and source resets preserve independent NoAutoPossess, explicit anchors, intrinsic newline/nullable facts and FIRSTLINE forced versus ordinary starts.
4. Original-byte length, whole-source UTF, caller lockouts and diagnostic offsets retain cancellation/work/Literal/no-read priority.
5. Sliced-source/settings ownership, signed captures/marks, observed/unobserved exact work/cancellation and complete capacities.

### Task1: native-first tests-only fixture

**Files:** create tests/Pcre/PcreNoStartOptPrefixReferenceTests.cs and PcreNoStartOptPrefixProtocols.Part1.cs/Part2.cs; current design/plan.
**Interfaces:** existing NoStartOptimize/NoAutoPossess/MaxPatternLength; native bit0x10000, start source prefix and match flags0/4/8/12.

- [x] Independently reproduce547 merged255 native groups/42,792 protocols; pin622 new groups/46,726 protocols and110 diagnostics, separating192 split offsets.
- [x] Generate36 supplementary cases and publish draft256 with production identical to merged255.
- [x] Observe636 expected failures/93,130 passes/93,766 total; match failure keys/post-native frames, zero skips/compiler warnings/errors.
- [x] Freeze3 new C# blobs with exact source/tree/job evidence before production.

### Task2: resolve source prefix

**Files:** src/Pcre/PcreByteStartSettings.cs, PcreByteCompiler.cs, PcreByteParser.cs; seven inventories.
**Interfaces:** bool NoStartOptimize on start settings, initialized from options, set true on exact prefix and consumed by both existing compile-policy snapshots.

- [x] Initialize/scan/return the global bool without changing original bytes/body offset or Literal bypass.
- [x] Route settings.NoStartOptimize to program construction and parser metadata; keep existing proof/matcher implementation fixed.
- [x] Update all seven inventories before final source push.
- [ ] Require six fresh93,766 net10 results/214 frozen/120 methods, all3 library outputs/supporting gates.

### Task3: review and delivery

**Files:** PR256 body/issue7 footer; own ignored ledger.
**Interfaces:** completed source-pinned logs, actual merged255 base, exact reviewed tree and proposed-merge ordered parents, immutable issue prefix.

- [ ] Whole-branch author review across five boundaries; clean diff/frozen/prior checks.
- [ ] Record final GREEN/review/identities externally; mark ready/readback without docs-only CI repeat.
- [ ] Retire only this plan's identity-checked ledger; preserve branch/worktree.

Corrected RED/frozen evidence is recorded in the design. Task2 source and all seven inventories are implemented; exact-source qualification and Task3 author review/delivery remain external gates.
