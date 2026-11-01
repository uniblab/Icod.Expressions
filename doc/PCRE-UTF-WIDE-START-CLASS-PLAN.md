# UTF Wide Leading-Class Implementation Plan

> REQUIRED SUB-SKILL: superpowers:executing-plans inline. No subagents.

**Goal:** Preserve native LF entry for positive property-free wide UTF classes.
**Architecture:** Widen the existing compiler range bound; reuse its proof result,
matcher, captures and charged-work/cancellation machinery.
**Tech Stack:** C#13/net8/net9/net10, xUnit, PCRE.NET1.6.0/PCRE2 10.48 JIT.
**Spec:** PCRE-UTF-WIDE-START-CLASS-DESIGN.md.

## Global constraints and review focus

All design constraints apply; frozen35/214, all5881 existing tests unchanged.
1. Wide scalar leaders cannot introduce CR; every first class still excludes CR.
2. At least one class must contain LF; no-LF input keeps prior default-budget skip.
3. No widening of predicate/negation/optional/exact>1/first-node proof restrictions.
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

Files: src/Pcre/PcreByteProgram.cs; four canonical PCRE inventories.
Interface: existing bool HasUtfCrLfStartClass(PcreByteAlternatives root); unchanged.
- [ ] Change only range upper bound127 to0x10FFFF and clarify leading-byte comment.
  Preserve all other source shapes, CR/LF, proof limit and runtime interfaces.
- [ ] Six native Staging platforms/net8/9/10 full/frozen214/configured gates green;
  failures/skips/compiler warnings/errors0 and seven Linux workload equalities.
- [ ] Whole-branch author self-review; exact head/tree/log/frozen/whitespace/clean
  and tests-afterRED audit; update issue102/PR, mark ready. User owns merge.

Pre-flight: tests consume unchanged internal compile/match results. No new
interfaces. Wider end bound does not alter normalized membership or runtime UTF
decoding. Documented native optional/exact>1 positives remain outside admission.
