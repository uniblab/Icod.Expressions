# Fixed Byte Prefix Implementation Plan

> REQUIRED SUB-SKILL: superpowers:executing-plans inline. No subagents.

**Goal:** Preserve byte CRLF LF entry for fixed unrestricted prefixes and atomic wrappers.
**Architecture:** Bounded iterative AST proof; existing flag/matcher/VM state.
**Spec:** PCRE-BYTE-FIXED-PREFIX-DESIGN.md. C#13/net8/net9/net10.

## Constraints and review focus

No agents/Python/SDK install, public/package/workflow/consumer changes, fallback,
raised limits or frozen35 edits. Original offsets, captures, width and work remain.
1. Native12-byte window and direct exact Any repeats; no repeated PrefixAny inference.
2. Atomic/group wrappers preserve order/captures; no recursive traversal.
3. A restrictive prefix stays exactly one position; sole final literal, no suffix.
4. UTF, variable repeats, alternatives/assertions/calls and legacy range flags stay separate.
5. Work/cancellation/resource checks and native span bounds; six-platform proof.

## Task1: Pin native contracts and observe RED

Files: tests/Pcre/PcreByteFixedPrefixReferenceTests.cs; design/plan.
Interfaces: existing Compile/Match and pinned reference only.
- [ ] Pin literal captures, 1/2/11/12/13 boundaries, atomic/property/class controls.
- [ ] Add every-offset six-newline/UCP protocols, UTF and observer/budget/cancellation controls.
- [ ] Validate native fixtures and span bounds; publish tests-only draft, observe expected RED and zero build warnings/errors.

## Task2: Compiler proof and verified handoff

Files: src/Pcre/PcreByteProgram.cs; four canonical PCRE status inventories.
Interface: existing HasByteCrLfPrefix(root), now instance for cancellation; immutable flag unchanged.
- [ ] Heap stack flattens only nonempty single-branch group/atomic bodies in source order, bounded1000 visits.
- [ ] Admit sole final literal after legacy one-position prefix or1..11 Any(true) positions; direct positive exact Any repeats only. Reject other nodes/widths/suffixes.
- [ ] Run final six-platform full/frozen214/configured gates, seven Linux workload equalities, zero warnings/errors.
- [ ] Separate author self-review; frozen35/whitespace/clean/exact-tree/head/log audit; update PR, mark ready, never merge.

Pre-flight: parser already lowers C to Any(true), possessive to Atomic, repeats
carry exact bounds. No new parser/AST/runtime interface required. Native optimizer
provenance remains explicit. Standing scope approval applies; no extra pause.
