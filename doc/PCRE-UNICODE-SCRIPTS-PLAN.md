# Unicode script properties implementation plan

**Goal:** Implement pinned PCRE2 script/scx semantics across existing byte/UTF paths.
**Spec:** `doc/PCRE-UNICODE-SCRIPTS-DESIGN.md`.
**Architecture:** Parameterized immutable scalar-class metadata and generated
per-script interval tables; existing matcher opcodes and resource paths.
**Execution:** Inline implementation and author review; no subagents.

## Constraints and review focus

C#13/net8/9/10; no runtime fallback/dependencies, Python or SDK installation.
Preserve frozen35/frozen214, byte spans, limits/cancellation and GNU behavior.
Cover primary versus scx, Common/Inherited/Unknown, all alias/range boundaries,
byte projection and predicate metadata through nested grammar. Keep binary
properties, script runs, general optimizer and public/release gates separate.

## Task 1: Native reference and immutable RED

- Add native boundary/alias fixtures and `PcreScriptPropertyReferenceTests.cs`.
  Retain unsupported-family diagnostics by replacing obsolete pending-Greek
  rows with distinct binary/Bidi properties in existing unfrozen tests.
- Publish test-only draft; require successful-build RED caused by unsupported
  script properties. Freeze all new/modified test blobs after that observation.

## Task 2: Data, metadata and GREEN

- Add `PcreUnicodeScripts` interval lookup and generated data, with a standalone
  source-hash/native-fingerprint-checked C# generator and regeneration gate.
- Add Script/ScriptExtensions and PropertyValue metadata through parser,
  predicates, instruction compiler and membership calls. Retain existing
  opcodes, width analysis, Step/journal and byte projection.
- Extend Unicode notice and update four canonical inventories/current roadmap.
  Require all18 full native results, frozen214, package/API/parity/workload,
  script and applicable full compatibility checks. Verify immutable tests,
  frozen35 and exact remote merge tree before marking ready. User owns merge.
