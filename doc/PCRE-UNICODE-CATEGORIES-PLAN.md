# Pinned Unicode category implementation plan

**Goal:** Make PCRE general categories independent of the .NET/OS Unicode version.
**Architecture:** Unicode17 packed range table, ASCII fast path and shared scalar
lookup. Existing class and name consumers retain their surrounding semantics.
**Spec:** `doc/PCRE-UNICODE-CATEGORIES-DESIGN.md`.
**Execution:** Inline implementation and author review; no subagents.

## Constraints and review focus

C#13/net8/9/10; no runtime dependencies, native fallback, Python or SDK installs.
Preserve frozen35/frozen214, byte spans, work/cancellation and GNU behavior.
Exercise newly assigned and still-unassigned scalars, supplementary-plane range
boundaries, complements/caseless category widening, UTF names/digit diagnostics,
and reproducibility/license packaging. Full Unicode folding remains separate.

## Task 1: Reference and immutable RED

- Add `tests/Pcre/PcreUnicodeCategoryVersionReferenceTests.cs` using existing
  membership/compiler APIs: exhaustive native fingerprint, pinned assignment
  literals, bounded live PCRE.NET JIT sampling and name grammar/capture spans.
- Open a draft PR with tests/design/plan. Require a successful managed build and
  the expected category/name failures before modifying production code.

## Task 2: Data ownership and GREEN

- Add `src/Pcre/PcreUnicodeCategories.cs` and generated companion data with
  `internal static UnicodeCategory GetCategory(int scalar)` for valid scalars.
- Add standalone C# generator in `tools/GeneratePcreUnicodeCategories`, validate
  input SHA256 `d62e5bab70ca74f099343f71224fa051cb1fdd61a1ab45c0488c44cfc0b6102e`
  and compare regenerated output byte-for-byte before accepting it.
- Replace runtime category/letter/digit queries in scalar membership, capture
  names and non-ASCII subroutine-number diagnostics. Ship Unicode notice in
  the NuGet package and update the four canonical PCRE inventories plus status.
- Require the full suite/frozen214 on all18 native platform/framework pairs,
  existing package/API/parity/workload gates, script checks and applicable full
  source-pinned compatibility checks. Review immutable test blob, generated
  data hash, complete diff and exact remote merge tree. User owns merge.
