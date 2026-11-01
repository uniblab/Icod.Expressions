# Internal UTF leading-capture dependencies

## Intent and native evidence

Continue #7/#102 after merged #129. Preserve bitmap LF entry through referenced
leading captures while retaining repeat-endpoint acceleration when only later
captures are referenced. This extends compiler classification, not VM semantics.

PCRE.NET1.6.0/PCRE2 10.48 JIT, UTF ANYCRLF, S=[\x09-\x0c\x{100}]:
(S+)A\1? on U0100+CRLF+A matches LF3:2/group1=3:1. On VT+CRLF+VT+A it
selects LF2:3/group1=2:2. (S+)(A)\2? instead misses the first subject and selects
VT3:2 on the second. Capture conditions show the same leading-versus-later slot
distinction. check_opcode_types clears optimized_cbrackets for OP_REF/REFI/CREF
and each duplicate-name target. detect_early_fail stops at an unoptimized leading
OP_CBRA; start-bitmap study remains available.

Direct zero-count backreferences are omitted by pcre2_compile.c7423ff, whereas
zero-count groups retain OP_SKIPZERO/body at7574ff and can still clear capture
optimization. DEFINE/calls/returned captures require additional source provenance.

## Approaches and implementer choice

Under standing autonomy, classify prefix capture numbers against resolved
backreference and capture-condition slots in the existing bounded whole-root scan.
Return Eligible and Skip independently. A related slot clears Skip; unrelated slots
retain it. Reject direct zero-count backreference repeats, calls, DEFINE/all other
conditions and unknowns as conservative baseline behavior. Zero-count groups with
retained dependency bodies can be traversed normally, matching native evidence.
A complete native bytecode-pruning model would broaden scope; defer it in102.
A separate matcher/reference path would duplicate existing behavior.

## Compiler change and invariants

src/Pcre/PcreByteProgram.cs: FindUtfCrLfLeadingRepeat locally collects a
HashSet<int> of traversed capturing-group numbers (at most parser256). No set for
noncapture-only chains. Replace CanUseUtfCaptureRepeat with private
(bool Eligible, bool Skip) ClassifyUtfCaptureRepeat(PcreByteAlternatives root,
IReadOnlySet<int> captures). Preserve the1000 combined branch/node/queue bounds
and per-visit cancellation. Observe resolved Backreference.Number/DuplicateNumbers
and CaptureConditional.CaptureNumbers; any prefix-number intersection clears Skip.
Follow capture-condition bodies in addition to prior safe children. Direct
Repeat{Maximum:0,Child:Backreference} returns default before inspecting that child;
other calls/conditions/unknown nodes still return default. Scan all syntactic
branches, even unchosen condition bodies. Check cancellation per observed slot.

After class LF/CR checks, require Eligible; final Skip is depth<=4 AND classified
Skip. Noncapture-only admission is unchanged. Reuse selected leaf identity, existing
immutable scalar markers, start-class proof and per-attempt integer endpoint.
No parser/AST/membership/source metadata/UTF validation/matcher/program/instruction
layout/Step/capture/frame/journal/runtime-state change. Compiler heap worklist and
integer-number set only; no saved Span/interior pointer/snapshot/recursion/stackalloc.
No raised parser256/captures256/instructions4096/marks256/frames1024/defaultwork1m
or proof1000 ceiling. All6309 baseline expectations/frozen35 unchanged.

## Verification and remaining gates

Native-first full spans: related/unrelated numeric/named/duplicate references and
capture conditions, greedy/lazy/possessive, mixed depth4/5/34/200, zero-count groups,
lookahead/negative assertions, source-pruning/DEFINE/call controls. Six newlines/
UCP/every offset/strict-tolerant fragments, observed/exact/exhausted work/cancellation,
no-LF default budget and parser257 rejection. No unsafe malformed Xuc probes100.
Tests-only draft -> observed successful-build RED with6309 baseline passing ->
compiler classification -> six native Staging OS/arch/net8/9/10 full/frozen214 and
configured gates/7 equal workloads -> whole-branch author self-review and exact
head-tree-log/frozen/clean/whitespace/test-afterRED audit -> ready; user merge.

C#13/net8/net9/net10, UTF8 LF; root project/solution, src/tests/samples. No agents/
Python/SDK install, public/package/version/dependency/workflow/release/consumer/
fallback/engine clone changes. Broader102/byte99/unsafe100 remain. Grep last;
CommandFramework removal only after publication and all known migrations.
