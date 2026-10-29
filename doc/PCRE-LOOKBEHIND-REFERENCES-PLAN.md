# Consumed Lookbehind References Implementation Plan

> **For agentic workers:** Use superpowers:executing-plans. User retains merge control.

**Goal:** Resolve capture-body lengths for consumed lookbehind backreferences.

**Architecture:** Defer general behind metadata, resolve names, collect global capture/lexical maps, then perform heap width-dependency analysis and iterative reconstruction. Existing compiler and VM remain unchanged.

**Tech Stack:** C# 13, net8/net9/net10, xUnit, test-only PCRE.NET 1.6.0 / PCRE2 10.48 Compiled/JIT.

**Spec:** `doc/PCRE-LOOKBEHIND-REFERENCES-DESIGN.md`.

## Global Constraints

Internal managed implementation; no public API/version/dependency/consumer changes.
Root project/solution, `/src`, `/tests`, `/samples`, UTF-8 LF, no Python.
Local Debug and six-platform Staging CI. Existing256 nesting/capture ceilings,
4096 compiled instructions/node visits,1024 saved paths and work/cancellation
contracts remain; variable ranges255, fixed/intermediate65535.
Names resolve before width analysis; global branch-reset syntax bans consumed
references. Existing unlimited groups/aliases/J/subroutines remain pending.

## Review Focus

- Forward targets resolve without assuming capture participation or outer repeats.
- Lexical own-group references reject despite zero-width assertion boundaries.
- Consumed dependency cycles reject; references only in lookahead add no width edge.
- Every behind validates under erased bodies, with original opener diagnostics.
- Heap reconstruction and width resolution stay off the native stack at depth256.

## Task 1: Native reference/capture fixtures before implementation

**Files:** Create `tests/Pcre/PcreLookbehindReferenceLengthTests.cs`; docs above.
**Interfaces:** Existing compile/match APIs and `PcreRegex8Bit`.

- [ ] Pin explicit Compiled/JIT match/all-capture results for the design's supported spellings and capture-body rules; byte/UTF and scoped-end/offset sweeps.
- [ ] Pin invalid targets, lexical self/mutual cycles, unbounded/erased targets, global branch reset and width limits; deep named and dependency-chain cases.
- [ ] Publish test-first draft PR; inspect clean compilation and expected missing-feature failures before production edits.

## Task 2: Global compile-time resolution

**Files:** `src/Pcre/PcreByteParser.cs`, `PcreByteParser.Groups.cs`, `PcreByteParser.NamedCaptures.cs`, `PcreByteParser.Lookbehind.cs`, `PcreByteSyntax.cs`; create `PcreByteParser.TreeRewrite.cs`, `PcreByteParser.LookbehindResolution.cs`; obsolete rejection fixtures and PCRE inventories.
**Interfaces:** Raw lookbehind retains OpeningOffset. Shared iterative tree rewrite replaces recursive named reconstruction. Capture context maps group roots/lexical intervals and reference positions. Existing node widths and final compiler metadata remain.

- [ ] Defer general behind closure, track global branch-reset usage, and sequence numeric/name validation before behind finalization.
- [ ] Add shared iterative reconstruction and global capture/lexical collection; include all nested/erased AST children.
- [ ] Extend heap width traversal with reference edges, lexical self/global-reset checks and active-node cycle rejection; preserve zero-count/intermediate/final range policies.
- [ ] Finalize behind metadata in postorder; update old unsupported-reference fixtures and inventories. Preserve literal/ordinary programs and VM.
- [ ] Run CI; resolve confirmed defects with focused red-to-green evidence.

## Task 3: Review and ready

- [ ] Fresh read-only full-branch review against spec and Review Focus; address material findings.
- [ ] Verify exact final head on all six Staging jobs, full/frozen suites net8/net9/net10, workload/extraction/API/package gates, whitespace/frozen production source.
- [ ] Record red/green/review evidence in PR and mark ready; user owns merge.
