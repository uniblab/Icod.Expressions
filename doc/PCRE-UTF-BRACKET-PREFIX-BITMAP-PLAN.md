# Optional bracket prefix bitmap implementation plan

Execute inline; no subagents. Spec: PCRE-UTF-BRACKET-PREFIX-BITMAP-DESIGN.md.
Goal: share bounded native-style skipped-prefix study across optional bracket
bodies, including explicit atomics. All spec constraints/limits apply; C#13/net8/
9/10, baseline28269/frozen214/35source blobs unchanged. Review focus: atomic tail
commitment; skipped assertion captures/rollback; mixed unsupported branches;
proof exhaustion/nullable fallback budgets; UTF/owner/resource boundaries.

## Task1: independent native corpus and RED

Files: new tests/Pcre/PcreUtfBracketPrefixBitmapReferenceTests.cs and this spec/
plan. Consume pinned PCRE.NET1.6.0/PCRE2 10.48 oracle; produce full literal capture
fixtures, protocol/work/cancellation/count/expansion controls and an immutable blob.

- [x] Qualify native JIT sizes, bounded/complete capture vectors and every caller
  offset; generate tests without modifying baseline expectations or production.
- [x] Publish tests/design/plan draft. Observe successful-build RED confined to
  the new class, all28269 baseline cases passing,0compilerwarnings/errors. Freeze
  the test blob after RED.

## Task2: selector and iterative branch-prefix scan

File: src/Pcre/PcreByteProgram.cs. Existing interfaces:
HasUtfCrLfOptionalGroupBitmap(PcreByteNode,ref int)->bool and
HasUtfCrLfMandatoryBodyBitmap(PcreByteAlternatives,ref int)->bool.

- [x] Select Atomic{NoncapturingGroup}.Body and charge its wrapper within the
  success-only checkpoint, preserving all prior paths/charges.
- [x] Scan only unquantified word-boundary/skipped-assertion prefix nodes in each
  body branch; charge1/2 with checked pre-addition/cancellation and require a
  remaining mandatory head. Preserve the iterative all-branch leaf proof.
- [ ] Verify immutable new tests and all baseline tests; append four inventories
  and one recent index row, preserving all earlier history.

## Task3: final qualification and review boundary

- [ ] Author whole-branch review; verify frozen/test blobs, baseline expectations,
  whitespace, historical prefixes and exact remote tree/allowed paths.
- [ ] Audit six native jobs/net8/9/10/full/frozen214, zero failures/skips/compiler
  warnings/errors, every configured gate and seven equal Linux workloads.
- [ ] Update only issue102 recent footer; put full RED/GREEN evidence in the PR,
  mark ready and leave unmerged for user review. No merge/tag/publication.

Observed RED: run522/Linux x64, all three TFMs;838new-class failures/28663passes/
0skips; all28269baseline+394newcontrols pass; compiler warnings/errors0. All2514
failure lines and2514xUnit FAIL lines identify only this new class. Its test blob
d0c25ed3fda55ad4c5a649c257dd728a3facce0f is frozen. Final qualification is in PR164.
