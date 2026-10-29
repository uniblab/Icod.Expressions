# General Fixed-Length Lookbehind Implementation Plan

> **For agentic workers:** Use superpowers:executing-plans. Execute through a reviewed ready PR; user owns merge.

**Goal:** Extend internal atomic lookbehind to general fixed-length branches.

**Architecture:** Validate character widths through heap traversal at parser closure, then emit per-branch Reverse instructions inside shared assertion guards. Preserve literal optimization and shared capture transactions.

**Tech Stack:** C# 13; net8/net9/net10; xUnit; PCRE.NET 1.6.0 / PCRE2 10.48 Compiled/JIT oracle.

**Spec:** `doc/PCRE-FIXED-LOOKBEHIND-DESIGN.md`.

## Global Constraints

Internal managed implementation; no public API/version/dependency/consumer change.
No Python. Root project/solution, `/src`, `/tests`, `/samples`, UTF-8 LF.
Local Debug and six-platform Staging CI. Existing depth 256/captures 256,
4,096 compiled instructions/node visits, 1,024 saved paths/guards, work and
cancellation contracts remain; new general branch lengths at most 65,535.
Variable consumed lengths/backreferences remain controlled unsupported syntax.

## Review Focus

- Alternatives restore the original endpoint before branch-specific rewind.
- Nested ahead can inspect beyond the endpoint while captures keep source indexes.
- Negative and empty behind remain eligible at malformed caller offsets.
- Length validation is bounded and does not consume native stack at depth 256.
- Zero-count enclosing repeats cannot skip unsupported lookbehind validation.

## Task 1: Establish compiled-oracle behavior

**Files:** Create `tests/Pcre/PcreFixedLookbehindReferenceTests.cs`; design and plan above.
**Interfaces:** Existing compile/match result APIs and `PcreRegex8Bit`.

- [ ] Add explicit outcomes/all-capture spans for the design's supported body families, including `(?<=a(?=(bc)))b` on abc => G0 1:1, G1 1:2; ordered unequal direct branch lengths; nested equal-length alternatives; direct repeats and capture rollback.
- [ ] Add UTF, selected malformed/CRLF caller-offset sweeps, controlled unsupported lengths, depth/work/cancellation/resource cases.
- [ ] Publish a test-first draft PR; inspect clean compilation and expected feature failures before production edits.

## Task 2: Shared parser, width analysis and VM lowering

**Files:** Modify `src/Pcre/PcreByteSyntax.cs`, `PcreByteParser.Groups.cs`, `PcreByteParser.cs`, `PcreByteParser.NamedCaptures.cs`, `PcreByteProgram.cs`, `PcreByteMatcher.cs`; create `src/Pcre/PcreByteParser.Lookbehind.cs`.
**Interfaces:** New `PcreByteLookbehind(Body, IsPositive, BranchLengths)` node; shared compiler alternatives accept optional reverse lengths; existing guard frames/journal remain unchanged.

- [ ] Parse both assertion directions with heap group frames and inherited assertion context. Preserve compatible literal lowering. Validate general behind lengths at closure, including enclosing zero-repeat cases.
- [ ] Compute lengths with an explicit postorder heap traversal and reference-keyed widths. Reject nonfixed/unknown consumed lengths at the assertion opening; bound sums/products to 65,535.
- [ ] Resolve names through behind bodies and extend explicit-newline/nullable metadata. Normalize direct behind repeats as assertions.
- [ ] Emit Reverse per direct alternative under shared guards, with fragment-bounded byte/scalar rewind, work/cancellation checks and no subject-end change.
- [ ] Replace obsolete rejection fixtures with supported cases or still-pending controls; update syntax/design/resource/consumer inventories.
- [ ] Inspect compiled-oracle CI; fix confirmed defects with focused red-to-green evidence.

## Task 3: Review and ready PR

- [ ] Fresh read-only whole-branch review against spec and Review Focus; resolve material findings.
- [ ] Verify whitespace/frozen source, full/frozen tests net8/net9/net10 on all six Staging platforms, workload/extraction/API/package gates.
- [ ] Update PR with exact head and red/green/review evidence and mark ready; retain merge control with user.
