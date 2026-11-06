# Positive lookahead direct group implementation plan

**Goal:** Preserve native LF first-bitmap entry after CR for positive ahead
followed by a one-level mandatory direct-head group in an optional bracket.

**Architecture:** Add a bounded group-head pre-scan in
`src/Pcre/PcreByteProgram.cs`, using existing literal/class/type predicates
and an isolated visit copy. The iterative mandatory-body proof remains the
actual owner of visits. Runtime behavior is unchanged.

**Spec:** `doc/PCRE-UTF-POSITIVE-AHEAD-DIRECT-GROUP-DESIGN.md`.

1. Add pinned PCRE2 10.48 JIT full-capture fixtures to
   `tests/Pcre/PcreUtfPositiveAheadDirectGroupReferenceTests.cs`; commit tests
   first, observe a successful-build RED confined to the new class, freeze
   the test blob, and confirm all 30,745 baseline cases pass.
2. Add a helper that accepts unquantified capture/noncapture/atomic group
   bodies only when all nonempty branches begin with one direct supported
   literal, class or type. Charge each probed branch in a copied visit count;
   let the existing iterative leaf traversal validate the body again.
3. Append evidence to the four canonical PCRE inventories and recent index.
   Audit six-platform net8/9/10 full/frozen CI, configured gates, immutable
   tests, frozen source and whole-branch diff.
4. Update only issue #102's recent footer, mark the PR ready for user review
   and leave merge to the user.
