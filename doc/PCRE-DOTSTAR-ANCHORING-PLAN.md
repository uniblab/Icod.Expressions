# Direct DotAll-star anchoring implementation plan

> Execute inline under standing direction; no subagents or approval pauses.

**Goal:** Own a bounded direct root-leading DotAll-star anchor independently of start study.
**Architecture:** AST proof and owned program bool; effective caller-or-inferred matcher anchor.
**Tech stack:** C#13; library net8/net9/net10; tests net10; pinned PCRE2 10.48/JIT0.
**Spec:** doc/PCRE-DOTSTAR-ANCHORING-DESIGN.md.

## Global constraints

- Base0532d55b8a80fe25d8a083183b67702d2bfa7f2f/tree18cb27b10cb85d981ddf24bb3a85264be40535ac;93,766 baseline tests.
- Preserve287 prior test files/282 C# sources and frozen35; freeze3 new C# blobs after exact RED.
- Production only PcreByteProgram.cs/PcreByteMatcher.cs; no grammar/AST/API/diagnostic/opcode/frame/workflow/dependency changes.
- Direct root star only; exclude calls/targets and all lexical PRUNE/SKIP including erased/DEFINE; proof bound silently falls back.
- Seven documents before qualification; final evidence PR257/issue7 footer only; Grep last; user merges/tags/publishes.

## Review focus

1. Exact direct greedy/lazy/possessive shape and erased lexical cuts, with inert quoted/mark controls.
2. NoStartOptimize/NoAutoPossess independence and effective-anchor propagation to all eight matcher gates.
3. Tolerant fragments/split starts, empty flags, FIRSTLINE/COMMIT and signed captures/marks.
4. Exact observed/unobserved budgets, cancellation and validation/no-read priorities.
5. Owned source/settings and complete instruction/capture/depth capacities, bounded iterative fallback.

### Task1: native-first tests-only proof

**Files:** three new PcreDotstarAnchoring C# fixtures; current design/plan.

- [x] Reproduce622 merged256 groups/46,726 protocols independently; pin479 new groups/42,953 protocols and34 diagnostics, separating1,433 split offsets.
- [x] Generate44 supplemental cases; publish tests-only draft with production identical to merged256.
- [ ] Observe101 expected failures/94,222 passes/94,323 total/zero skips/compiler warnings/errors; inspect every failure category after native preflight.
- [ ] Freeze three new C# blobs with exact source/tree/job evidence before production.

### Task2: own and apply inferred anchor

**Files:** src/Pcre/PcreByteProgram.cs, src/Pcre/PcreByteMatcher.cs.

- [ ] Add owned bool and conservative bounded iterative AST proof with cancellation and silent fallback.
- [ ] Combine caller/inferred anchor once in matcher; replace all eight reads consistently.
- [ ] Preserve every prior fixture/frozen blob; author-review all five boundaries; run applicable local shell gates and diff checks.

### Task3: source-pinned qualification and delivery

**Files:** current design/plan and five shared inventories before final source qualification; final external evidence only.

- [ ] Six native net10 results each94,323 passes,214 frozen outcomes/120 methods, zero failures/skips/compiler warnings/errors; all three library outputs.
- [ ] Every configured Unicode/corpus/API/glibc/package/consumer/workload/script/classifier gate; Intel aggregate same single result.
- [ ] Exact source/tree/base/proposed-merge parent identity and clean prior-file preservation; whole-branch author review.
- [ ] Update PR257/issue7 footer, preserve original prefix, mark ready and read back; no docs-only CI repeat; user retains merge/tag/publication.


## Fixture boundary correction before freeze

Initial tests-only source a3734dbc465fc09efd57e42e421061c4bb0a23a3/tree20117a65a64d6e6f066edce72cfa6b2c04e0cca5, [run43756385085](https://github.com/uniblab/Icod.Expressions/actions/runs/43756385085), Linux x64 job132941292410:103 failures/94,220 passes/94,323 total/zero skips/compiler warnings/errors. All native preflights pass. The79 protocol failures are the78 independently predicted counter-oracle differences plus lazy DotAll-star/COMMIT under tolerant UTF with study enabled: at split byte1 of C3A941 native rejects and old managed retries at2. This is the same intended anchoring gap; native NoDotstarAnchor counter-oracle also has COMMIT study, so it is not a perfect model of that old managed path. The other22 feature failures are18 work,2 ownership and2 native-independence cases.

Two extra failures came from my capacity-fixture arithmetic: the current star loop emits5 instructions plus final Accept, leaving4090 literal instructions at the existing4096 limit. Correct only capacity source length/native span4092 to4090 and one-over-limit source/span4093 to4091. All479 native protocol groups,34 diagnostics, other native expectations/assertion/helper bodies, production and prior tests stay unchanged. Corrected RED must show101 feature failures/94,222 passes/94,323 total before freezing. The erased-body proof-over-budget cases already pass, confirming they fit existing compilation capacity.
