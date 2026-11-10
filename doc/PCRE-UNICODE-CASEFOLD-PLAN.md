# Pinned Unicode case-fold implementation plan

**Goal:** Make supported PCRE single-scalar case equivalence independent of host casing.
**Spec:** `doc/PCRE-UNICODE-CASEFOLD-DESIGN.md`.
**Architecture:** ASCII fast path and generated Unicode17 C/S pairs behind the
existing `PcreUtf8CaseFold` interfaces; no VM/guard/journal changes.
**Execution:** Inline implementation and author review; no subagents.

## Constraints and review focus

C#13/net8/9/10; no runtime dependencies/fallback, Python or SDK installation.
Preserve frozen35/frozen214, all45,811 baseline tests, source-byte captures,
local i/r, work/cancellation and GNU behavior. Exercise new and multi-member
case sets, non-growing folds, restricted cross-ASCII behavior, invalid UTF and
both directions where byte widths differ. Keep generated data reproducible.

## Task 1: Reference and immutable RED

- Add `tests/Pcre/PcreUnicodeCaseFoldVersionReferenceTests.cs` using existing
  Fold/compiler APIs. Hash semantic equivalence classes over all1,112,064 valid
  scalars in default/restricted modes; compare live JIT literal outcomes and
  full capture spans across existing syntax paths and malformed-UTF offsets.
- Open a draft PR with tests/design/plan. Require a successful build and the
  expected new case-equivalence failures before changing production code.

## Task 2: Generated lookup and GREEN

- Add `src/Pcre/PcreUtf8CaseFold.Data.cs` and replace only FoldUnrestricted's
  host-casing query with bounded immutable lookup; leave CloseRanges intact.
- Add hash-checked `tools/GeneratePcreUnicodeCaseFold` C# tool. Add Linux PR/main
  byte-for-byte regeneration and extend the existing packaged Unicode notice.
- Update current roadmap status and four canonical inventories. Verify native
  equivalence fingerprints, unchanged test blob, frozen source and exact remote
  merge tree. Require all18 native platform/framework results, script checks,
  package/API/parity/workload checks and applicable full compatibility jobs.
  Record evidence in PR metadata and mark ready. User owns merge.
