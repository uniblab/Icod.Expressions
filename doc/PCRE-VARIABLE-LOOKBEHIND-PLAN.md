# Bounded Variable-Length Lookbehind Implementation Plan

> **For agentic workers:** Use superpowers:executing-plans. User retains merge control.

**Goal:** Add bounded variable lookbehind with pinned Compiled/JIT semantics.

**Architecture:** Extend heap width analysis to ranges; compile shared ReverseRange/ReverseEnd instructions under assertion guards. Compact retry frames and scope end limits share the existing capture journal and depth restoration.

**Tech Stack:** C# 13; net8/net9/net10; xUnit; test-only PCRE.NET 1.6.0 / PCRE2 10.48 Compiled/JIT.

**Spec:** `doc/PCRE-VARIABLE-LOOKBEHIND-DESIGN.md`.

## Global Constraints

Internal managed implementation; no public API/version/dependency/consumer changes.
Root project/solution; `/src`, `/tests`, `/samples`; UTF-8 LF; no Python.
Local Debug and six-platform Staging CI. Existing 256 depth/capture, 4,096 compiled
instructions/node visits and 1,024 saved paths/guards remain. Variable maxima255;
all-fixed analysis65535. Consumed references and unlimited ordinary groups pending.

## Review Focus

- Longest-start ordering is independent of lazy preference inside each body.
- Retried starts restore captures/marks and do not reopen successful assertions.
- End scopes restore on every guard/fallback and also affect mixed fixed branches.
- Tolerant UTF maximum excursion follows the pinned JIT invalid-barrier behavior.
- Erased bodies, mixed branch limits and range arithmetic cannot bypass analysis.

## Task 1: Pin bounded-variable behavior before implementation

**Files:** Create `tests/Pcre/PcreVariableLookbehindReferenceTests.cs`; docs above.
**Interfaces:** Existing compile/match APIs and `PcreRegex8Bit`.

- [ ] Explicit native outcomes/all captures for bounded repeats, unequal nested alternatives, R, body preferences/longest-start ordering, positives/negatives, zero lengths, options/names/branch-reset/direct assertion repeats.
- [ ] Native probes pin scoped-end and malformed-left-barrier JIT behavior. Include mixed branches, nested guards and restore after completion/fallback.
- [ ] Add selected every-byte offset byte/UTF/CRLF sweeps, 255/256/erased-body diagnostics, work/cancellation and saved-path coverage.
- [ ] Publish test-first draft PR; inspect clean compilation and expected feature failures before production edits.

## Task 2: Extend analysis/compiler/shared VM

**Files:** `src/Pcre/PcreByteParser.Lookbehind.cs`, `PcreByteParser.Groups.cs`, `PcreByteSyntax.cs`, `PcreByteProgram.cs`, `PcreByteMatcher.cs`; obsolete pending fixtures and PCRE inventory docs.
**Interfaces:** Lookbehind node retains branch maxima and optional group minimum. Assertion-enter flags carry optional visible-end scope; scope records retain boundary/limit. Reverse retry frames retain byte-sized remaining count with existing integer checkpoints.

- [ ] Extend heap analysis to min/max, retain global minimum, enforce255 for mixed/variable groups and existing arithmetic/erased/unbounded policies. R=1..2; consumed references stay unsupported.
- [ ] Emit per-branch ranged/fixed reverse with range endpoint checks; preserve all-fixed lowering and original diagnostic source bytes.
- [ ] Rewind within UTF/byte bounds and lazily save next-start retries under shared resource/journal policy; no unrolled bodies or allocations per retry.
- [ ] Apply and restore scoped end through assertion-depth records and existing frame checkpoints; update consuming/anchor/boundary checks consistently.
- [ ] Replace obsolete bounded-variable/R pending fixtures, update inventories, inspect CI and fix confirmed defects with focused red-to-green tests.

## Task 3: Review and ready

- [ ] Fresh read-only whole-branch review and resolve material findings.
- [ ] Verify exact final head: full/frozen suites on net8/net9/net10 across six Staging platforms, workload/extraction/API/package gates, whitespace and frozen production source.
- [ ] Record red/green/review evidence in PR and mark ready. Preserve user merge control.
