# Positive lookahead group lookbehind prefix implementation plan

**Goal:** Preserve LF first-bitmap entry after CR for mandatory group heads
following proven unquantified lookbehind prefixes.

**Architecture:** Extend the bounded prefix switch in
`HasUtfCrLfDirectGroupBitmap` (`src/Pcre/PcreByteProgram.cs`) to recognize
general and lowered literal lookbehind. Existing visit copy and iterative
body proof remain authoritative. No runtime matcher change.

**Spec:** `doc/PCRE-UTF-POSITIVE-AHEAD-GROUP-BEHIND-DESIGN.md`.

1. Pin PCRE2 10.48 JIT full-capture fixtures in
   `tests/Pcre/PcreUtfPositiveAheadGroupBehindReferenceTests.cs`, strict and
   tolerant; commit tests first, observe RED only in the new class, freeze
   test blob and baseline count.
2. Charge bounded lookbehind prefixes and require existing proven direct
   or repeat heads in every branch. Leave nullable and unsupported heads
   conservative.
3. Append evidence to four canonical inventories and recent index. Audit
   six native Staging jobs, net8/9/10 full/frozen214, build scripts,
   immutable test blob, frozen source and whole branch diff.
4. Update issue #102's recent footer and mark PR ready for user review.
