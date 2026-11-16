# Caller NoDotStarAnchor implementation plan

> Execute inline under standing user direction; no subagents or approval pauses.

**Goal:** Own caller suppression of inferred dot-star anchoring while retaining explicit anchors and zero-width cuts.
**Architecture:** Options/Literal admission plus a snapshotted program-compiler bool; existing anchor and candidate metadata.
**Tech Stack:** C#13; library net8/net9/net10; tests net10; pinned PCRE2 10.48/JIT0.
**Spec:** doc/PCRE-NO-DOTSTAR-ANCHOR-DESIGN.md.

## Global Constraints

- Base7e2ea411a2990570689052255a101f277d40e8cb/tree8148bb8f5d01882e1f98b96a5c5309c40fdb5c8d;94,323 baseline.
- Preserve290 prior test files/285 C# sources/frozen35; freeze three new C# fixtures after exact RED.
- Production only PcreByteOptions.cs/PcreByteCompiler.cs/PcreByteProgram.cs; matcher/parser/AST/opcodes/frames/diagnostic/API/dependencies/workflows fixed.
- NoDotStarAnchor is init-only bool default false, incompatible Literal192/0; independent NoStartOptimize/NoAutoPossess/explicit anchors.
- Retain the existing bounded root proof only for a disabled anchor's emitted-COMMIT synthetic candidate; wider proofs/source directive stay pending.
- Seven inventories before final qualification; final evidence PR258/issue7 footer only; Grep last; user merges/tags/publishes.

## Review Focus

1. False/default policy unchanged, explicit anchor retained, start-study and possessification controls independent.
2. Disabled anchor still admits the requested zero-width COMMIT cut at split/malformed UTF before consumption.
3. Literal/cancellation/invalid-work/length/UTF/lockout and no-read priority at original byte offsets.
4. Owned settings/sliced source, signed resets/captures and successful marks after caller mutation.
5. Observed/unobserved exact budgets, cancellation and complete instruction/capture/depth capacities plus silent proof fallback.

### Task1: native-first tests-only proof

**Files:** new PcreNoDotStarAnchorReferenceTests.cs and PcreNoDotStarAnchorProtocols.Part1.cs/Part2.cs; current design/plan.
**Interfaces:** test-only optional reflection setter for missing caller property; native0x8000; mode65536 maps independent NoStartOptimize.

- [x] Reproduce479 merged257 groups/42,953 protocols; pin509 new groups/45,361 protocols,132 diagnostics and1,485 separate strict split offsets.
- [x] Generate69 supplemental cases for all five review boundaries; publish tests-only draft with production identical to merged257.
- [ ] Observe149 expected managed failures/94,884 passes/95,033 total/zero skips/compiler warnings/errors; inspect exact failure keys and post-native frames.
- [ ] Freeze three new C# blobs with exact source/tree/job identities before production.

### Task2: caller option and owned metadata

**Files:** src/Pcre/PcreByteOptions.cs, src/Pcre/PcreByteCompiler.cs, src/Pcre/PcreByteProgram.cs.
**Interfaces:** add PcreByteOptions.NoDotStarAnchor; append optional bool noDotStarAnchor=false to program compiler and snapshot caller value.

- [ ] Add default-false init bool and HasLiteralOptionConflict membership; route caller bool to compiler.
- [ ] Gate DotStarAnchored while retaining proven emitted-COMMIT zero-width candidate metadata; skip proof when disabled without COMMIT.
- [ ] Review all five boundaries; preserve all prior/frozen fixtures; run frozen source/corpus/A05/classifier/main-job guards and diff checks.

### Task3: source qualification and delivery

**Files:** current design/plan plus five shared inventories before final source qualification; external final evidence only.

- [ ] Six fresh native net10 results at95,033 passes,214 frozen outcomes/120 methods, zero failures/skips/compiler warnings/errors and all three library targets.
- [ ] All configured Unicode/extraction/glibc/corpus/API/workload/package/consumer/script/classifier gates; Intel aggregate same single result.
- [ ] Exact source/tree/base/proposed-merge parent identity, whole-branch author review and clean preservation checks.
- [ ] Update PR258/issue7 footer, preserve original prefix, mark ready and read back; user retains merge/tag/publication; no docs-only CI repeat.


## Reference-wrapper boundary correction before freeze

Initial tests-only source22923786a927795d5b463309613b044247ec2e14/treeee042f4231953fbae59985896b0fa462d3e987b6, [run43848299352](https://github.com/uniblab/Icod.Expressions/actions/runs/43848299352), Linux x64 job133234016101:157 failures/94,876 passes/95,033 total/zero skips/compiler warnings/errors. All149 predicted feature failures occur:78 protocol keys exactly match native counter-oracle differences,48 Literal conflicts and23 supplemental cases. All94,323 prior tests pass. Eight extra failures occur in native preflight for disabled-flag Literal length controls: the direct C API enforces limit0, but the pinned PCRE.NET wrapper does not enforce it in these cases, so no exception is thrown.

Correct only those eight new fixture inputs from one-byte41/limit0 to two-byte4141/limit1. Eight independent C preflights retain native188/offset0; managed expectations, all509 protocol groups, all helper/assertion bodies, other diagnostics, prior files and production stay fixed. Positive limits match the existing working reference fixtures. Corrected RED must show149 feature failures/94,884 passes/95,033 total before freezing.
