# UTF Atomic Star Prefix Implementation Plan

> REQUIRED SUB-SKILL: superpowers:executing-plans inline. No subagents.

**Goal:** Preserve bitmap entry through explicit atomic star ancestors.
**Architecture:** Extend bounded identity discovery, disable recovery after atomic
ancestors, and guard mandatory-set atomic descent by proven-star identity.
**Tech Stack:** C#13/net8/9/10, xUnit, PCRE.NET1.6.0/PCRE2 10.48 JIT.
**Spec:** PCRE-UTF-ATOMIC-STAR-PREFIX-DESIGN.md.

## Constraints and review focus

Spec constraints apply; all12798 baseline expectations/frozen35 unchanged.
1. Atomic ancestors preserve identity and disable Skip even at depth1.
2. Possessive iterator wrappers retain ordinary first-star Skip rules.
3. Atomic groups following an outside root star cannot disable its cache.
4. Mixed/captured/alias/erased chains retain all eligibility/proof/capture guards.
5. Bare atomic PLUS/unsupported placements remain excluded; no broader optimizer.

## Task1: Native-first fixtures and observed RED

Files: tests/Pcre/PcreUtfAtomicStarPrefixReferenceTests.cs; design/plan.
Interfaces: existing Compile/Match/PcreRegex8Bit JIT reference.
- [ ] Pin full capture spans/depth1..199/mixed ancestors/source/modifiers/casing/
  UCP/aliases/erasure/capture dependencies/later groups/unsupported controls.
- [ ] Six newline/every offset/strict-tolerant fragment protocols and work/
  cancellation/default budget/parser/capture/backtrack limits.
- [ ] Tests-only draft; successful-build new-class RED/all12798baseline pass.
  Preserve original tests/expectations unchanged after RED.

## Task2: Guarded atomic discovery

Files: src/Pcre/PcreByteProgram.cs; four canonical PCRE inventories.
Interfaces: existing FindUtfCrLfRootStarAtom(root)->(Atom,Skip) and
FindUtfCrLfLeadingRepeat(root)->(Set,Skip); existing markers unchanged.
- [ ] Admit Atomic(NoncapturingGroup) ancestry with extra1 charge and disable
  local Skip; preserve Atomic(Repeat), CT/1000/group/capture/retained guards.
- [ ] Atomic mandatory-set descent requires proven-star identity; disable PLUS
  Skip during descent, retaining existing post-star handling/final proof reset.
- [ ] Exact-head six native Staging/net8/9/10 full/frozen214/configured gates/
  7equal workloads pass;0failed/skipped tests/buildwarnings/errors.
- [ ] Wholebranch author review and strict logs/head/tree/blob/tests-afterRED/
  frozen/clean/whitespace audit; update102/open and PR/ready/unmerged. User merge.
