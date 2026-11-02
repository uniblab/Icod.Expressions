# Internal UTF entry after retained zero-count prefixes

## Intent and native evidence

Continue #7/#102 after merged #132. Native start-bitmap study skips OP_SKIPZERO
brackets, while JIT detect_early_fail stops at that opcode before a later PLUS.
Recognize the first mandatory class after a retained zero-count prefix without
enabling its repeat-skip marker. Keep this distinct from truly omitted reference0.

Pinned PCRE.NET1.6.0/PCRE2 10.48 JIT, UTF ANYCRLF, S=[\x09-\x0c\x{100}]:
(?:B){0}(S+)A matches initial CRLF+A1:2, U0100+CRLF+A3:2 and
VT+CRLF+VT+A2:3. Unlike an omitted direct reference prefix, even an empty retained
group stops repeat acceleration. Capturing prefixes leave their capture slots
unset; possessive, assertion and atomic zero brackets show the same behavior.
Native pcre2_study.c1418..1422 skips the whole bracket; detect_early_fail has no
SKIPZERO traversal; pcre2_compile.c7574..7601 retains the bracket/body.

## Approaches and implementer choice

Under standing autonomy, extend the shared first-node helper to return an AST node
and a BlocksSkip bit. Zero-count retained brackets can be crossed for the existing
class proof, but set that bit permanently along the leading chain. Truly omitted
refs do not. An AST rewrite would lose source dependency/declaration information;
a matcher special case would duplicate existing candidate logic. Defer both.

## Compiler contract

src/Pcre/PcreByteProgram.cs FindUtfCrLfLeadingNode returns
(PcreByteNode? Node, bool BlocksSkip). Inspect an optional possessive Atomic wrapper
around Repeat0. Direct Backreference0 costs2, does not block. Repeat0 children
Group/Noncapture/Lookahead/Lookbehind/LiteralAssertion/CaptureConditional cost2
and block. Explicit Atomic{Noncapture} child costs3 and blocks. Add1 for the outer
possessive wrapper. Other shapes return the original node; max>0 and static/
recursion/assertion conditions are not skipped. CT and1000 visits retained.
Skip bracket bodies only in this first-node selection, as native bitmap study
does; the existing global capture-dependency scan still observes their contents.

FindUtfCrLfLeadingRepeat accumulates !BlocksSkip from root and every descended
body; final Skip also requires depth<=4 and capture classification.Skip. Do not
replace the accumulated value at finalization. Prefix retained brackets do not
consume byte input or the depth of the actual leading groups. The start-class
proof consumes only helper.Node, still gated by a recognized repeat leaf.
All-skipped/over-bound sequences give no class proof. Trailing retained brackets
do not affect prefix acceleration; no global barrier flag or runtime state added.

No parser/AST/source metadata/membership/UTF validation/matcher/program/instruction
layout/Step/capture/frame/journal/runtime-state change. Tuple/compiler integers
only; no saved spans/pointers/snapshots/recursion/stackalloc. All6668 baseline
expectations/frozen35 unchanged. Parser256/captures256/instructions4096/marks256/
frames1024/defaultwork1m and existing classification/proof1000 ceilings unchanged.

## Verification and boundaries

Native-first full spans for ASCII/wide, greedy/lazy/possessive, zero capture/noncap/
assertion/behind/atomic/alternative/empty/condition brackets, unset captures, mixed
depth4/5/34/200, omitted+retained combinations, live dependencies, trailing-bracket
controls, all-skipped/call/DEFINE/no-LF/CR/G/optional controls. Six newlines/UCP/all
offsets/strict-tolerant fragments, exact/exhausted observed work/cancellation,
default budget/parser257. No unsafe malformed Xuc probes100.
Tests-only draft -> observed successful-build RED with6668 baseline passing ->
compiler tuple/barrier -> six native Staging OS/arch/net8/9/10 full/frozen214 and
configured gates/7 equal workloads -> author review/exact head-tree-log/frozen/
clean/whitespace/tests-afterRED audit -> ready; user merge.

C#13/net8/net9/net10, UTF8 LF, root project/solution and src/tests/samples. No agents/
Python/SDK install/public/package/version/dependency/workflow/release/consumer/
fallback change. Other nullable/alternative/pruned starts, calls/returned captures,
DEFINE/other conditions and broader102/byte99/unsafe100 remain. Grep last;
CommandFramework removal requires publication and all known migrations.
