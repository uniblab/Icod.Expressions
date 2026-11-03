# UTF Atomic Mandatory Repeat Entry Implementation Plan

> REQUIRED SUB-SKILL: superpowers:executing-plans inline. No subagents.

**Goal:** Preserve native bitmap entry for atomic mandatory classes without a star.
**Architecture:** Reuse existing atomic group/set proof and its disabled PLUS Skip;
remove only the unrelated star-identity prerequisite.
**Tech Stack:** C#13/net8/9/10, xUnit, PCRE.NET1.6.0/PCRE2 10.48 JIT.
**Spec:** PCRE-UTF-ATOMIC-REPEAT-ENTRY-DESIGN.md.

## Constraints and review focus

Spec constraints apply; all13928 baseline expectations/frozen35 unchanged.
1. Bare atomic wide PLUS preserves LF entry and never gains consumed-run recovery.
2. Non-atomic PLUS and outside-root first-star caches remain unchanged.
3. Possessive modifier and explicit group shapes remain distinct.
4. Optional/erased/capture prefixes keep source/eligibility/CT/1000 guards.
5. Nullable-group rejoin and broader bitmap shapes remain separate.

## Task1: Native-first fixtures and observed RED

Files: tests/Pcre/PcreUtfAtomicRepeatEntryReferenceTests.cs; design/plan.
Interfaces: existing Compile/Match and PcreRegex8Bit JIT reference.
- [ ] Full literal captures/depth1..199/modifiers/aliases/captures/source/casing/
  UCP/prefixes/unsupported and cache controls.
- [ ] Six newline/every-offset/strict-tolerant boundaries/work/cancellation/
  default budget/parser/capture/backtrack tests.
- [ ] Tests-only draft; successful-build new-class RED/all13928baseline pass.
  Preserve original tests/expectations afterward.

## Task2: General atomic mandatory-set proof

Files: src/Pcre/PcreByteProgram.cs; four canonical inventories.
Interface: FindUtfCrLfLeadingRepeat(root)->(Set,Skip), existing tuple/markers.
- [ ] Remove star-identity condition from Atomic(NoncapturingGroup) body switch;
  update comment; retain unconditional atomic Skip=false and final set guards.
- [ ] Exact-head six native Staging/net8/9/10 full/frozen214/configured gates/
  7equal workloads pass,0failed/skipped tests/buildwarnings/errors.
- [ ] Wholebranch author review/strict logs/head/tree/blob/tests-afterRED/frozen/
  clean audit, issue102 open and PR ready/unmerged; user review/merge.
