# UTF Wide Leading-Class Implementation Plan

> REQUIRED SUB-SKILL: superpowers:executing-plans inline. No subagents.

**Goal:** Preserve native LF entry for positive property-free wide UTF classes.
**Architecture:** Widen the compiler range bound and model direct root-wide PLUS
recovery with immutable instruction metadata and a per-attempt integer endpoint.
Reuse membership, captures, Step/cancellation and existing frames/journal.
**Tech Stack:** C#13/net8/net9/net10, xUnit, PCRE.NET1.6.0/PCRE2 10.48 JIT.
**Spec:** PCRE-UTF-WIDE-START-CLASS-DESIGN.md.

## Global constraints and review focus

All design constraints apply; frozen35/214, all5881 existing tests unchanged.
1. Wide scalar leaders cannot introduce CR; every first class still excludes CR.
2. At least one class must contain LF; no-LF input keeps prior default-budget skip.
3. No widening of predicate/negation/optional/exact>1 restrictions; nested/alternative
   wide unbounded class PLUS remains excluded until native recovery is modeled.
4. UTF byte offsets, caller boundaries/fragments, case closure and captures shared.
5. Conservative1000 proof visits and depth256 remain; observer/cancellation paid.

## Task1: Native contracts and managed RED

Files: tests/Pcre/PcreUtfWideStartClassReferenceTests.cs; this design/plan.
Interfaces: existing PcreByteCompiler.Compile/PcreCompiledBytePattern.Match,
PCRE.NET PcreRegex8Bit with pinned Compiled/JIT settings.
- [ ] Add literal native-first spans and all-offset matrices for the admitted
  wide/CR-including/no-LF shapes; strict and tolerant UTF/UCP/newlines.
- [ ] Pin multibyte captures, work/exhaustion/cancellation, no-LF default budget,
  proof34/220 wrapper boundaries. No existing expectation changes.
- [ ] Publish tests-only draft, observe successful-build expected RED with all5881
  baseline tests passing; retain exact test blobs afterRED.

## Task2: Bounded compiler extension and handoff

Files: src/Pcre/PcreByteProgram.cs, src/Pcre/PcreByteMatcher.cs; four inventories.
Interfaces: existing bool HasUtfCrLfStartClass; new compile-only selected set and
immutable PcreByteInstruction.SkipCrLfAfterRepeat flag; per-attempt utfRepeatEnd.
- [ ] Extend bound127 to0x10FFFF. After observed five failures on the first source
  attempt, select a direct root class PLUS/possessive wrapper; mark its scalar
  instructions by AST identity. Reject other wide unbounded class PLUS contexts.
- [ ] Retain maximum successful marked scalar end outside calls/assertions; after
  whole-attempt failure, skip a CRLF at this end within the same fragment, only
  with valid proof/no explicit source newline/CRLF-family convention. Comparisons
  already paid; no extra scan, frame/journal/state restore or raised limit.
- [ ] Six native Staging platforms/net8/9/10 full/frozen214/configured gates green;
  failures/skips/compiler warnings/errors0 and seven Linux workload equalities.
- [ ] Whole-branch author self-review; exact head/tree/log/frozen/whitespace/clean
  and tests-afterRED audit; update issue102/PR, mark ready. User owns merge.

Pre-flight: tests consume unchanged internal compile/match results. No new
public interfaces. Wider bound/recovery do not alter membership or UTF decoding.
First source attempt RED5/5936/5941 proved recovery necessary; keep all tests fixed. Documented native optional/exact>1 positives remain outside admission.
