# Positive lookahead prefixed group implementation plan

**Goal:** Preserve LF first-bitmap entry after CR for one-level groups whose
mandatory head follows a supported word boundary or lookahead.

**Architecture:** Extend `HasUtfCrLfDirectGroupBitmap` in
`src/Pcre/PcreByteProgram.cs` to skip bounded leading boundary/lookahead
nodes before checking existing mandatory head/repeat predicates. The outer
pre-scan uses a visit copy and iterative body proof validates the actual
branch. No runtime matcher change.

**Spec:** `doc/PCRE-UTF-POSITIVE-AHEAD-GROUP-PREFIX-DESIGN.md`.

1. Add PCRE2 10.48 JIT full-capture fixtures to
   `tests/Pcre/PcreUtfPositiveAheadGroupPrefixReferenceTests.cs`; commit
   before production, observe successful-build RED solely in the new class,
   and freeze the test blob.
2. Scan only unquantified supported boundary/lookahead prefixes, charge
   bounded visits, and require a proven nonnullable direct or repeat head
   in every branch. Keep the existing iterative all-branch proof.
3. Append evidence to four canonical inventories and recent index. Audit
   six native net8/9/10 Staging jobs, frozen214, build scripts, unchanged
   tests, frozen source and whole-branch diff.
4. Update issue #102's recent footer and mark the PR ready for user review.
