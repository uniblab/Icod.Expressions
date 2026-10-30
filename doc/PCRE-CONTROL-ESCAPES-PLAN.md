# Internal control escapes implementation plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox syntax for tracking.

**Goal:** Implement PCRE2 printable-ASCII `\cX` escapes in the existing literal path.

**Architecture:** Validate/consume exactly one source operand in ParseEscape;
uppercase ASCII lowercase and XOR40; return existing PcreByteLiteral. Reuse every
class, width, instruction, matching, capture and resource path.

**Tech Stack:** C#13, net8/net9/net10, xUnit, PCRE.NET1.6.0 / PCRE2 10.48 JIT.

**Spec:** `doc/PCRE-CONTROL-ESCAPES-DESIGN.md`.

## Global constraints

- Internal C#13 net8/net9/net10, UTF-8 LF; root project/solution, src, tests, samples; no Python.
- Depth256/captures256/instructions4096/marks256/savedframes1024/default work1,000,000 unchanged.
- No new AST/opcode/frame/payload, matcher change, snapshots, saved spans or interior pointers.
- No API/package/version/dependency/workflow/release/consumer change, production native/Regex fallback or raised limits.
- Frozen35 BRE/ERE source unchanged. #99/#102 optimizer work and #100 native unsafe-Xuc deviation remain separate gates.
- User owns merge; Grep last and CommandFramework removal requires publication/all known consumer migrations.
- No local SDK: executable red/green through six native Staging jobs/full/frozen214/configured gates and fresh review.

## Review focus

- Printable punctuation operands consume exactly one character despite class/group/quote/comment/quantifier roles.
- x/xx trivia must not skip the operand, including space and hash.
- Produced ASCII letters obey existing local casing/r/UCP policies; operand conversion itself is ASCII invariant.
- cJ/cM source metadata must flow through existing literal/class negation rules and original UTF fragments/G offsets.
- Invalid operand/missing syntax retains original escape byte indexes; outer structural and invalid UTF diagnostics keep their precedence.

## Task 1: Native-first fixtures and expected red

**Files:** Create tests/Pcre/PcreControlEscapeReferenceTests.cs and this design/plan;
update doc/PCRE-DESIGN.md, PCRE-SYNTAX-ACCEPTANCE-MATRIX.md,
PCRE-RESOURCE-ERROR-CONTRACT.md and PCRE-GREP-CONSUMER-FIXTURES.md.

**Interfaces:** Existing PcreByteCompiler/options/results and native PcreRegex8Bit;
use indexed native Groups and explicit expected capture spans.

- [ ] Builder-check design; pin95-byte literal operand table using native10.48 JIT.
- [ ] Cover every printable operand in byte/UTF atoms/classes/negation/ahead/behind/repetition;
  literal NUL/DEL/ranges/delimiters/quotes/x/xx/captures/retries/calls/width cases and i/r folding.
- [ ] All-offset byte/tolerant UTF LF/ANYCRLF matrices retain cJ/cM source metadata,
  invalid/multibyte barriers, G and capture protocols; do not include known #99 false-positive profiles.
- [ ] Native-reject161 nonprintable/high-byte operands in byte atoms/classes; UTF
  non-ASCII/missing/structural errors and invalid UTF precedence with original offsets.
- [ ] Verify literal-equivalent observer/work budgets and compile/pre/mid cancellation.
- [ ] Save native evidence; whitespace/frozen35; commit/publish exact tests/docs draft;
  observe expected managed missing-feature failures after native expectations before source edit.

## Task 2: Return the literal from ParseEscape

**Files:** Modify src/Pcre/PcreByteParser.cs only.

**Interfaces:** Existing ParseEscape(int offset,bool inBracket=false) -> PcreByteNode;
add c branch which returns PcreByteLiteral. Existing Invalid(offset,message).

- [ ] Validate next source byte20..7E without trivia skipping; missing/invalid ->
  InvalidSyntax at escape offset. Consume exactly one printable operand.
- [ ] Uppercase ASCII a..z arithmetically and XOR40; return existing literal.
- [ ] Commit/publish exact source tree; run all six native Staging full/frozen/configured gates.
- [ ] Fresh whole-branch review; fix material findings in one pass with native-first
  observed RED→GREEN plus green full suites; ledger minor findings without extra review loops.
- [ ] Audit all platform logs/overall completed-success, whitespace/frozen35/clean/exact
  local-remote tree/final PR head; update proof and mark ready; preserve worktree.
