# NoDotStarAnchor source prefix implementation plan

> Execute inline under standing user direction; no subagents or approval pauses.

**Goal:** Recognize the pattern-start dot-star control through the existing owned caller policy.
**Architecture:** Resolve caller/source OR in start settings; pass the bool to existing program construction.
**Tech Stack:** C#13; library net8/net9/net10; tests net10; pinned PCRE2 10.48/JIT0.
**Spec:** doc/PCRE-NO-DOTSTAR-ANCHOR-PREFIX-DESIGN.md.

## Global Constraints

- Based9c8f1b278fab1c04baef2b4cf741f652df2a779/tree614006e0d82fc21619297b6873a673f7b7ef392a;95,033 baseline tests.
- Preserve293 prior test files/288 C# sources except exactly3 pending-syntax InlineData directive tokens; preserve every assertion/helper body and frozen35.
- Freeze3 new C# blobs after exact native-first RED; no subsequent fixture changes.
- Production only PcreByteStartSettings.cs/PcreByteCompiler.cs; parser/AST/program/matcher/opcodes/frames/diagnostic/API/dependencies/workflows fixed.
- Exact initial uninterrupted uppercase prefix, repeated/interleaved/idempotent source/caller global OR; Literal bypasses scanning.
- Seven inventories before final qualification; final evidence PR259/issue7 footer only; user merges/tags/publishes; Grep last.

## Review Focus

1. Prefix spelling/placement versus quoted/class/comment/mark/Literal text and ordinary resets.
2. Owned source/caller OR delivered to existing compiler policy, independent explicit anchors/start study/possessification and zero-width cuts.
3. Original-byte length/whole-source UTF/caller lockouts and diagnostic/no-read/cancellation/work-limit priority.
4. Owned sliced source/settings, signed captures/marks, observed/unobserved exact work and deterministic cancellation.
5. Full instruction/capture/depth capacities and exactly3 refreshed pending-syntax tokens with unchanged helpers/counts/frozen fixtures.

### Task1: native-first tests-only proof

**Files:** new PcreNoDotStarAnchorPrefixReferenceTests.cs and PcreNoDotStarAnchorPrefixProtocols.Part1.cs/Part2.cs; current design/plan.
**Interfaces:** existing caller NoDotStarAnchor; source20-byte prefix; native0x8000; independent mode65536 NoStartOptimize.

- [x] Reproduce509 merged258 groups/45,361 protocols; pin578 new groups/53,205 protocols,108 diagnostics and1,736 separate strict split offsets.
- [x] Generate38 supplementary cases covering all five review boundaries; publish tests-only draft with production/prior tests identical to merged258.
- [ ] Observe612 expected managed failures/95,145 passes/95,757 total/zero skips/compiler warnings/errors; compare exact profile/diagnostic IDs and post-native frames.
- [ ] Freeze3 new C# blobs with exact source/tree/job evidence before production.

### Task2: source resolution and pending-syntax refresh

**Files:** src/Pcre/PcreByteStartSettings.cs, src/Pcre/PcreByteCompiler.cs; three specified pending-syntax files; seven inventories.
**Interfaces:** add NoDotStarAnchor to settings, initialize caller bool, set on exact source token, consume resolved bool in compiler.

- [ ] Add/initialize/scan/return global bool while retaining original source/body offset and Literal bypass.
- [ ] Pass settings.NoDotStarAnchor to existing compiler policy; keep parser/program/matcher implementation fixed.
- [ ] Refresh only3 obsolete InlineData tokens to LIMIT_MATCH=1000000, retaining counts/helpers/assertions/new frozen fixtures/other290 files.
- [ ] Whole-branch author review, local frozen source/corpus/A05/classifier/main-job/diff gates; all seven inventories before qualification.

### Task3: source qualification and delivery

**Files:** external PR259/issue7 footer for final evidence only.

- [ ] Six fresh native net10 results at95,757 passes/214 frozen outcomes/120 methods, zero failures/skips/compiler warnings/errors, all three library outputs.
- [ ] All configured Unicode/extraction/glibc/corpus/API/workload/package/consumer/script/classifier gates; Intel aggregate same single net10 result.
- [ ] Exact source/tree/base/proposed-merge parent identities, clean preservation checks and completed whole-branch author review.
- [ ] Update PR259/issue7 footer, preserve original prefix, mark ready and read back; no docs-only CI repeat; user retains merge/tag/publication.
