# Internal PCRE nonrecursive subroutines implementation plan

> **For agentic workers:** Use superpowers:executing-plans for root integration,
> one focused fixture worker and one fresh read-only whole-branch review.

**Goal:** Add nonrecursive calls with native capture transactions and retries.

**Architecture:** Global integer target resolution and heap cycle validation;
shared out-of-line body bytecode; integer call guards and compensating journal.

**Tech Stack:** C#13, net8/net9/net10, xUnit, PCRE.NET1.6.0/PCRE2 10.48 JIT.

**Spec:** `doc/PCRE-SUBROUTINES-DESIGN.md`.

## Global constraints

- Internal only, no API/package/version/dependency/consumer/workflow changes.
- Root project/solution, src, tests, samples; UTF-8 LF; no Python.
- Depth256/captures256/instructions4096/marks256/savedframes1024/work1,000,000.
- Local Debug; no SDK here, executable red/green through six-platform Staging CI.
- Recursion/whole-pattern/returned-capture/consumed-behind calls stay unsupported.

## Review focus

1. First definition selection, erased/forward definitions, nested acyclic graphs
   and cycle rejection must not substitute capture participation for call targets.
2. Interior callee retries after return must reconstruct temporary captures;
   returned state restores numbered captures/starts/marks but preserves K/G0.
3. Atomic/assertion pruning and inactive sibling call scopes release exactly the
   owned arena node and restore caller call/assertion contexts on retry.
4. Shared1024 saved-frame and work/cancellation ceilings cover call entry,
   return compensation, empty/choice repeats and repeated callable bodies.
5. Pending recursion/returned-list/consumed-behind and suppressed-definition
   boundaries retain original byte diagnostics and never execute unlowered AST.

### Task 1: Native contract and failing fixtures

**Files:** Create the design/plan and `tests/Pcre/PcreSubroutineReferenceTests.cs`;
update obsolete pending rows in Backreference, BranchReset, NamedCapture,
SyntaxAcceptance and StaticCondition fixtures; update current inventories.

**Interfaces:** Existing internal Compile/Match and native compiled JIT oracle.

- [ ] Native researcher/builder check against exact10.48 source and probes.
- [ ] Explicit native outcome/capture/diagnostic fixtures for all five review risks.
- [ ] Publish tests/docs-only draft; inspect completed job's expected missing-
  feature failures and native assertions before production changes.

### Task 2: Parser, shared body compiler and transactional VM

**Files:** Create `src/Pcre/PcreByteParser.Subroutines.cs`; modify parser Groups,
main parser, Syntax, Compiler, Program and Matcher. Resource/design/syntax/consumer
inventories record support and pending gates.

**Interfaces:** PcreByteSubroutineCall(int Number,int Offset); parser's final
IReadOnlyDictionary<int,PcreByteGroup> SubroutineTargets; compiler Compile(root,
targets); Call/Return opcodes; guard-owned CallScope(ReturnPc,Parent,ChangeCount).

- [ ] Parse exact numbered/named/relative headers with cancellation and global
  resolution; heap cycle detection and final target metadata validation.
- [ ] Compile main and one callable Body per target; patch entries, share
  instruction/visit/mark ceilings; nullable calls and source newline metadata.
- [ ] Save integer call IDs in frames; push guards and LIFO call arenas, retain
  callee retries on return, journal compensating restores with work accounting,
  preserve slot0, release guards through failure and atomic/assertion pruning.
- [ ] Whitespace/frozen35/exact-tree checks; publish and inspect full six-job CI.
  Diagnose actual failures and fix with observed focused red/green proof.
- [ ] Fresh whole-branch review; address findings, inspect final exact head/tree,
  all six full/frozen/configured gates; ready PR for the user's merge.
