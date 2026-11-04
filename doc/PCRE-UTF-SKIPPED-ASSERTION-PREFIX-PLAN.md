# Skipped assertion UTF prefix implementation plan

> Execute inline; no subagents.

**Goal:** Recognize the native skipped assertion prefix kinds in shared UTF proofs.
**Architecture:** One switch addition,cost2/retained=true;all existing guards reused.
**Tech:** C#13/net8/9/10,xUnit,PCRE.NET1.6.0/PCRE2 10.48 JIT.
**Spec:** PCRE-UTF-SKIPPED-ASSERTION-PREFIX-DESIGN.md.

## Constraints and review focus
Spec constraints apply. Negative assertions still execute and keep their
captures unset; positive behind can capture before the match. Non-atomic reverse
retries remain native-compatible. Body predicates must not veto skipped bitmap
study,while positive ahead/optional assertion repeats remain excluded. Later
PLUS/star ownership stays blocked; earlier root-star identity stays intact.

## Task1: Native contract and observed RED
Files: tests/Pcre/PcreUtfSkippedAssertionPrefixReferenceTests.cs;design/plan.
Interfaces: existing Compile/Match/PcreRegex8Bit JIT.
- [ ] Pin native full captures for assertion forms,opaque bodies,dependencies,
  wrappers/counts/alternatives,mixed prefixes,PLUS modifiers/star placement and
  exclusions. Add six-newline/UCP/every-offset fragments,strict validation and
  shared resource/cancellation/long ordinary controls.
- [ ] Publish tests-only draft; observe successful-build failures only in the
  new class,all24324baseline pass. Record immutable test blob before source.

## Task2: Admit exact skipped assertion kinds
Files: src/Pcre/PcreByteProgram.cs;four canonical inventories;recent progress index.
Interfaces: FindUtfCrLfLeadingNode tuple/signature unchanged.
- [ ] Add negative-or-behind PcreByteLiteralAssertion,negative PcreByteLookahead
  and every PcreByteLookbehind to the shared switch,returning cost2/retained=true.
  Keep source/atom/range/alternative/required-repeat/owner guards and body opacity.
- [ ] Preserve tests/frozen35/whitespace;commit and verify exact remote content;
  author review entire branch. Audit six native Staging/full/frozen214/net8/9/10,
  configured gates/seven equal workloads,zero failures/skips/compiler warnings/
  errors; ready PR remains unmerged.
- [ ] Append one recent row and update only issue102 footer after CI; earlier
  issue history and preserved historical index block remain untouched.
