# Binary and Bidi properties implementation plan

**Goal:** Complete the pinned native binary/Bidi property catalogue.
**Spec:** `doc/PCRE-BINARY-BIDI-PROPERTIES-DESIGN.md`.
**Architecture:** Parameterized scalar kinds and generated interval/alias data
behind the existing class opcodes and PropertyValue metadata.
**Execution:** Inline implementation and author review; no subagents.

## Constraints and review focus

C#13/net8/9/10; no runtime fallback/dependencies, Python or SDK installation.
Preserve source-byte spans, Step/cancellation, journals, frozen35/frozen214 and
GNU behavior. Exercise byte projection, case-insensitive property independence,
Bidi @missing overrides, native InCB/Mirrored rules, alias namespaces and
checkout-independent fixture framing. Keep remaining syntax/API/release gates.

## Task 1: Native contract and RED

- Add native Boolean/Bidi boundary fixtures and reference tests. Update every
  obsolete pending binary/Bidi rejection fixture without dropping negative
  coverage; preserve wrong namespace failures.
- Publish a draft and require successful-build RED for missing properties.
  Freeze new test and data blobs after observing the expected failure.

## Task 2: Data/dispatch and GREEN

- Add hash/native-fingerprint-checked C# generator and immutable property data.
  Extend Linux regeneration and packaged Unicode notice.
- Add BinaryProperty/BidiClass parsing and membership through existing metadata;
  leave VM opcodes, saved state and resource rules intact.
- Update four inventories/current roadmap. Require18 complete native results,
  frozen214, package/API/parity/workload, script and full applicable compatibility
  gates. Review all files, unchanged new test blobs, frozen35 and exact merge
  tree; record evidence and mark ready. User owns merge.
