# UTF Capture-Wrapper Repeat Recovery Implementation Plan

> REQUIRED SUB-SKILL: superpowers:executing-plans inline. No subagents.

**Goal:** Preserve native UTF class PLUS recovery through unreferenced captures.
**Architecture:** Extend existing Set/Skip selector with shared group-depth count
and bounded whole-root capture-dependency exclusion; no new matcher path.
**Tech Stack:** C#13/net8/net9/net10, xUnit, PCRE.NET1.6.0/PCRE2 10.48 JIT.
**Spec:** PCRE-UTF-CAPTURE-REPEAT-RECOVERY-DESIGN.md.

## Global constraints and review focus

Design constraints apply; all6187 baseline expectations/frozen35 unchanged.
1. Mixed capture/noncapture depth4 enables marker; depth5+ retains LF without it.
2. Global backreference/call/condition/unknown/over-bound contexts reject only
   new capture-chain classification; old noncapture classification remains intact.
3. Named/whole-expression/suffix/scoped-case capture spans remain native byte spans.
4. No-LF/CR-including/empty/G/explicit-LF/UTF fragment and offset guards stay.
5. Iterative1000 combined branch/node/queue bound and cancellation; all caps stay.

## Task1: Pin native-first regressions and observe RED

Files: tests/Pcre/PcreUtfCaptureRepeatRecoveryReferenceTests.cs; design/plan.
Interfaces: existing compile/match and reference PcreRegex8Bit Compiled/JIT.
- [ ] Literal spans for ASCII/wide wrappers/depth/quantifiers/named/local-case/
  captures and dependency controls; every offset/six newlines/UCP/strict-tolerant.
- [ ] Pin observed/exact/exhausted work/cancellation/default no-LF budget/parser cap.
- [ ] Publish tests-only draft; inspect successful-build expected RED with all6187
  baseline passing; fix test blobs for the rest of this tranche.

## Task2: Extend compiler selection and handoff

Files: src/Pcre/PcreByteProgram.cs and four canonical inventories.
Interfaces: existing (PcreByteScalarSet? Set, bool Skip) FindUtfCrLfLeadingRepeat;
new private bool CanUseUtfCaptureRepeat(PcreByteAlternatives root).
- [ ] Follow Group/Noncapture bodies with shared depth and existing terminal guards;
  track capture presence, require bounded whole-root eligibility only for captures.
- [ ] Add heap worklist eligibility with1000 combined branch/node/queue bound and
  per-visit cancellation, conservative dependencies/unknown rejection.
- [ ] Verify six native Staging jobs/net8/9/10 full/frozen214/configured gates, zero
  failed/skipped tests/compiler warnings/errors and seven equal workloads.
- [ ] Whole-branch author review/exact head-tree/log/tests-afterRED/frozen/whitespace/
  clean audit; update102/PR, mark ready, preserve artifacts; user owns merge.

Self-check: classifier signature unchanged; both group kinds feed one depth; the
new helper gates capture chains only. No runtime or public interface changes.
