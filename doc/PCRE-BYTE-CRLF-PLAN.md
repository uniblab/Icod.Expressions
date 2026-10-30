# Internal byte CRLF implementation plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox syntax for tracking.

**Goal:** Correct byte class negation metadata and the bounded two-atom byte prefix cases in #99.

**Architecture:** Keep source operand shape on the byte class AST, reduce the admitted two-atom shape to one program bool, and use it at the existing failed-CR candidate boundary. Keep all VM work and storage paths.

**Tech Stack:** C#13, net8/net9/net10, xUnit, PCRE.NET1.6.0 / PCRE2 10.48 JIT.

**Spec:** `doc/PCRE-BYTE-CRLF-DESIGN.md`.

## Global constraints

- Internal C#13 net8/net9/net10, UTF-8 LF; root project/solution, src, tests, samples; no Python.
- No public API/package/dependency/workflow/release/consumer changes or production native/Regex fallback.
- Depth256/captures256/instructions4096/marks256/savedframes1024/default work1,000,000 unchanged.
- Frozen35 BRE/ERE source untouched. Existing journal, widths, original offsets, invalid UTF barriers and work/cancellation paths.
- No local SDK; managed executable red/green uses six native Staging jobs and full/frozen/configured gates. User owns merge.
- #99 stays open for broader byte start-optimization parity. Grep last; CommandFramework removal requires publication and all consumer migrations.

## Review focus

- Semantically identical duplicate versus singleton source classes retain distinct native metadata.
- Interior newline ranges and mixed negated predicates cannot infer metadata from final membership.
- Prefix flag must never spread to UTF, repeat/alternative/assertion/call/anchor/atomic shapes.
- Capturing wrappers retain original capture spans while qualifying only single-branch admitted shapes.
- Observed/cancellable results and shared work limits stay equivalent to ordinary VM execution.

## Task 1: Native contracts and executable red

**Files:** Create tests/Pcre/PcreByteCrLfSearchReferenceTests.cs and this design/plan;
update doc/PCRE-DESIGN.md, doc/PCRE-SYNTAX-ACCEPTANCE-MATRIX.md,
doc/PCRE-RESOURCE-ERROR-CONTRACT.md, doc/PCRE-GREP-CONSUMER-FIXTURES.md.

**Interfaces:** Existing PcreByteCompiler.Compile/PcreByteOptions/Match result and
PcreRegex8Bit reference; no new test-only production interfaces.

- [ ] Complete builder check and independently native-pin literals/metadata/shape controls.
- [ ] Add native-first literal spans: CRLF+A `[0D0A41]` at0 gives1:2 for negated
  singleton/equal-range/all-byte two-atom forms and mixed/duplicate CR classes;
  ordinary CR-including bitmap controls give miss, explicit offset1 gives1:2.
- [ ] Compare all captures/offsets, LF/ANYCRLF, quoted/xx/i wrappers, excluded optimizer
  shapes, UTF strict/tolerant and invalid barriers. Assert native literals before managed comparisons.
- [ ] Pin observed/unobserved/cancellable equality, work exhaustion and deterministic cancellation.
- [ ] Verify native fixture expectations, whitespace/frozen35; commit and publish exact
  tests/docs tree as draft PR; inspect expected managed failures before any source edit.

## Task 2: Preserve byte class provenance and bounded prefix semantics

**Files:** Modify src/Pcre/PcreByteParser.cs, PcreByteSyntax.cs,
PcreByteProgram.cs and PcreByteMatcher.cs.

**Interfaces:** PcreByteClass adds `bool PrefixAny=false`; PcreByteProgram adds
`bool ByteCrLfPrefix`; compiler helper `HasByteCrLfPrefix(PcreByteAlternatives root)`.
Existing ExplicitNewline bool and VM instructions unchanged.

- [ ] In ParseBracket track original term count/sole literal, endpoint CR/LF and equal
  ranges before folding; apply same negated-singleton metadata exception as UTF.
  Preserve historical positive interior-range flags until broader start-optimization
  coverage can replace them without lost matches (review-fix scope correction).
- [ ] Set PrefixAny for negated singleton/equal range or completed AllBytes set only.
- [ ] Detect exactly one root branch with two admitted unwrapped nodes, PrefixAny class
  then literal, optionally single-branch capture/noncapture wrappers around sequence/atoms.
  Return false for UTF and every other shape. No unbounded traversal beyond existing AST depth.
- [ ] Use ByteCrLfPrefix to bypass ordinary failed-CR LF skip, with all comparisons,
  transitions and work/cancellation on existing VM path.
- [ ] Commit/publish exact tree; run six native Staging jobs and configured gates.
- [ ] Fresh independent whole-branch review; fix material findings using red/green.
  Add native-first9-c class/class+A, exact-repeat2, all-byte+AA and atomic-wrapper
  regressions; observe failure before preserving positive interior flags.
- [ ] Inspect all six logs, full-range whitespace/frozen35/clean checkout/exact head/tree;
  update PR proof and mark ready; keep #99 follow-up open and worktree for feedback.
