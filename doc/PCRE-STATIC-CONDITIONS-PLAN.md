# Internal PCRE static conditions implementation plan

> **For agentic workers:** Use superpowers:executing-plans for integration,
> a focused fixture worker and one fresh read-only whole-branch review.

**Goal:** Add DEFINE declaration groups and VERSION compatibility conditions.

**Architecture:** Static conditional semantic nodes, heap parsing/global traversal,
shared width analysis and Jump-selected bytecode; both bodies still compile.

**Tech Stack:** C#13; net8/net9/net10; xUnit; PCRE.NET1.6.0 / PCRE2 10.48 JIT.

**Spec:** `doc/PCRE-STATIC-CONDITIONS-DESIGN.md`.

## Global constraints

- Root solution/project, src, tests, samples; UTF-8 LF; no Python.
- Internal compatibility version10.48; separate package version, no public claim.
- Local Debug, six-platform Staging PR; no SDK, executable CI red/green.
- Depth256/capture256/instructions4096/saved paths1024/default work1,000,000.
- No new VM opcode/state or public API/version/dependency/release/consumer change.

## Review focus

1. Unreachable DEFINE/VERSION bodies retain declarations, diagnostics, nested
   assertion validation according to native scan contexts and compile budgets;
   runtime never sets their captures.
2. DEFINE0 width must not analyze its ordinary unbounded body as consumed; global versus consumed-width scan context determines nested behind validation;
   consumed-reference definition capture dependencies still validate.
3. VERSION numeric tuple semantics and reserved bare/wrapped-name precedence,
   header exactness, bounds1000 and compatibility level10.48 stay distinct.
4. False single-branch repeats ignore expansion/marks/possessive wrappers without
   dropping original AST width counts or body validation; explicit else retains
   ordinary repetition/resource rules.
5. Runtime nullability uses the chosen branch, while global CR/LF metadata visits
   all bodies; original offsets, assertion K contexts and journal rollback remain.

### Task 1: Native contract, fixtures and inventory

**Files:** Create `tests/Pcre/PcreStaticConditionReferenceTests.cs`; update pending
DEFINE/VERSION cases in `PcreCaptureConditionReferenceTests.cs` and current/
historical design, syntax, resource and consumer inventories.

**Interfaces:** Existing Compile/Match/options and native compiled JIT oracle.

- [ ] Native builder check and explicit outcome/rejection probes for all review risks.
- [ ] Fixture with explicit outcomes/groups and every numbered capture, width/
  metadata/repetition distinctions, original byte diagnostics and resource gates.
- [ ] Publish test/docs-only draft after merged #91; inspect expected red CI and
  native assertions before production edits; record exact evidence.

### Task 2: Static parsing, traversal, width and lowering

**Files:** Create `src/Pcre/PcreByteParser.StaticConditions.cs`; modify
`PcreByteSyntax.cs`, `PcreByteParser.Groups.cs`, `PcreByteParser.Conditions.cs`,
`PcreByteParser.TreeRewrite.cs`, `PcreByteParser.Lookbehind.cs`,
`PcreByteParser.LookbehindResolution.cs`, `PcreByteProgram.cs`.

**Interfaces:** PcreByteStaticConditional(bool Value,PcreByteAlternatives Body,
bool IsDefinition=false); private StaticCondition token and TryParseStaticCondition.

- [ ] Parse exact headers with bounded cancellation-polled version tuples; preserve
  capture-name controls; enforce DEFINE1/VERSION2 branches and construct AST.
- [ ] Preserve Repeat AST min/max but ignore execution expansion/marks/possessive
  state only for false static nodes with one branch after validating bounds,
  suffixes and duplicates; preserve all bodies and global resolution. Charge their
  logical finite/minimum count to the4096 compile-visit budget without expansion.
- [ ] Traverse/rewrite all bodies; heap prepass tracks GlobalScan/WidthScan and
  suppressed DEFINE descendants by source offset. Implement DEFINE0 skip body,
  VERSION explicit widths/original repeat widths, chosen-branch nullable/all-body
  newline metadata and unconditional capture/reference dependency maps.
- [ ] Heap-compile choice Jump and both body sequences, no Split or matcher changes.
- [ ] Whitespace/frozen35, exact-tree publish, full CI; diagnose concrete failures
  and verify fixes with focused red/green evidence.
- [ ] Fresh whole-branch review; resolve findings, inspect final exact head/tree,
  all six green full/frozen/configured gates; ready PR for user merge.
