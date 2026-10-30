# Internal UTF CRLF leading class implementation plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox syntax for tracking.

**Goal:** Restore bounded native JIT LF entry after mandatory positive ASCII class starts in #102.

**Architecture:** Analyze first consumption of every AST branch with a compile-time heap worklist. Store one UTF-only program bool; use it at the existing failed-CR candidate boundary without skipping VM work.

**Tech Stack:** C#13, net8/net9/net10, xUnit, PCRE.NET1.6.0 / PCRE2 10.48 JIT.

**Spec:** `doc/PCRE-UTF-CRLF-START-CLASS-DESIGN.md`.

## Global constraints

- Internal C#13 net8/net9/net10, UTF-8 LF; root project/solution, src, tests, samples; no Python.
- No API/package/version/dependency/workflow/release/consumer changes or production native/Regex fallback.
- Depth256/captures256/instructions4096/marks256/savedframes1024/default work1,000,000 unchanged.
- No saved spans/interior pointers, frame/journal/opcode changes. Frozen35 BRE/ERE source unchanged.
- Keep #98 UTF source metadata, #101 byte behavior, original captures/offsets/fragment barriers and shared work/cancellation.
- #102 remains open for broader optimizer shapes, coordinated with #99; #100 native Xuc malformed behavior excluded.
- No local SDK: executable managed red/green via six native Staging jobs/full/frozen/configured gates. User owns merge; Grep last and CommandFramework removal gated on publication/all consumer migrations.

## Review focus

- Every alternative must reject CR; existential leading-class proof would cause false matches.
- Optional/fixed repeats, predicates, negations and non-ASCII folded ranges must not accidentally qualify.
- Atomic/capture/group wrappers must preserve bytecode, captures and native prefix constraints.
- Invalid runs and continuation caller offsets retain valid-fragment barriers and original G offsets.
- Observers/cancellable/unobserved matches share results and bounded work without new scan exemptions.

## Task 1: Native contracts and executable red

**Files:** Create tests/Pcre/PcreUtfCrLfStartClassReferenceTests.cs and this design/plan;
update doc/PCRE-DESIGN.md, PCRE-SYNTAX-ACCEPTANCE-MATRIX.md,
PCRE-RESOURCE-ERROR-CONTRACT.md, PCRE-GREP-CONSUMER-FIXTURES.md.

**Interfaces:** Existing PcreByteCompiler/options/match results, PcreRegex8Bit reference;
full native captures compared through indexed Groups (not LINQ).

- [ ] Builder-check actual design; independently native-pin positive CR-excluding versus CR-including/predicate/negation controls.
- [ ] Native-first literal `0D0A41` `[9-c]A` ->1:2, `[9-e]A` ->miss; class-only,
  suffix/wrappers/all-versus-some alternative rejection, i/xx, variable/group repeats,
  fixed-repeat native no-match counterexample, offset1 and source-explicit controls.
- [ ] Compare all captures/every caller offset under LF/ANYCRLF, strict/tolerant UTF,
  multibyte/invalid boundaries, captures/backrefs/recursive suffixes where admitted,
  byte unchanged controls, G/anchor/excluded forms and shared work/cancellation.
- [ ] Validate native expectations; whitespace/frozen35; commit/publish exact tests/docs
  draft, observe expected managed failures after native assertions before source edit.

## Task 2: Compile the bounded first-class proof and retain LF candidate

**Files:** Modify src/Pcre/PcreByteProgram.cs and src/Pcre/PcreByteMatcher.cs.

**Interfaces:** PcreByteProgram adds `bool UtfCrLfStartClass`; compiler helper
`HasUtfCrLfStartClass(PcreByteAlternatives root)` returns bool and observes compiler
cancellation. No AST, instruction, frame or journal payload additions.

- [ ] Heap worklist checks FIRST node of EVERY nonempty branch. Follow capture/noncapture
  group bodies, atomic Child and required variable/exact1 repeat Child. Reject empty
  branches and all other initial nodes. Require positive predicate-free scalar sets,
  nonempty normalized ranges entirely0..127, no range containing CR.
- [ ] Compute bool only for UTF; stop native CRLF LF-skipping when true, while retaining
  all existing VM transitions/comparisons/work/observer/cancellation.
- [ ] Commit/publish exact source tree and run all six Staging full/frozen/configured gates.
- [ ] Fresh independent whole-branch review; material findings fixed in one pass with
  native-first observed RED→GREEN and green full suites, minor findings ledgered.
- [ ] Audit every platform log/overall completed-success, exact reviewed/fixed head/tree,
  whitespace/frozen35/clean state; update PR proof and mark ready; keep worktree.
