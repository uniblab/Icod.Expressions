# Internal UTF entry through pruned reference prefixes

## Intent and native evidence

Continue #7/#102 after merged #131. Direct zero-count backreferences preceding a
leading class PLUS disappear from native bytecode. The compiler currently selects
branch[0] for repeat recognition and the start-class proof, so that obsolete AST
node prevents recognizing the real leading repeat. No matcher change is required.

PCRE.NET1.6.0/PCRE2 10.48 JIT, UTF ANYCRLF, S=[\x09-\x0c\x{100}]:
\1{0}(S+)A matches initial CRLF+A at1:2/group1=1:1, misses U0100+CRLF+A and
selects VT3:2 on VT+CRLF+VT+A. A live trailing \1? changes recovery to LF3:2
and LF2:3 respectively. A possessive zero reference also vanishes; zero-count
groups remain significant, including (?:\1{0}){0} before the repeat.
Native pcre2_compile.c7417..7427 omits direct REF/REFI/DNREF/DNREFI at max0;
7574..7601 retains grouped zero-count bodies under OP_SKIPZERO.

## Approaches and implementer choice

Under standing autonomy, add one shared first-node helper for the existing
leading-repeat selector and its start-class proof. Skip exactly a direct max0
Backreference repeat or its possessive Atomic wrapper; charge omitted source
nodes and check cancellation. An AST rewrite would affect other consumers of
source shape; a complete prefix flattener would admit unpinned forms. Defer both.

## Compiler contract

src/Pcre/PcreByteProgram.cs: private PcreByteNode? FindUtfCrLfLeadingNode(
IReadOnlyList<PcreByteNode> branch, ref int visits) iteratively skips direct
Repeat{Maximum:0,Child:Backreference} (cost2) and
Atomic{Child:Repeat{Maximum:0,Child:Backreference}} (cost3). Check CT per node;
return null if the1000 visit budget is exceeded or the branch is entirely omitted.
Return the first other node without charging it; caller traversal charges that
semantic node as before. No group/optional/other-node elision.

FindUtfCrLfLeadingRepeat initializes its own visit budget at1, uses the helper
for the root/each single-branch group body and charges each descended group.
Preserve total group depth for native Skip<=4, prefix capture slots and existing
terminal guards/classification. Pruned references consume no group depth.
HasUtfCrLfStartClass.AddBranches uses the helper only when a repeat leaf was
recognized (utfCrLfLeadingRepeatSet nonnull); otherwise retain branch[0]. This
limits new proof admission to the selector's single-branch mandatory PLUS chain.
The proof independently charges skipped nodes and existing proof nodes within1000.
All-omitted bodies cannot supply a class proof. Nonpruned node charging is unchanged.

No parser/AST/source metadata/membership/UTF validation/matcher/program/instruction
layout/Step/capture/frame/journal/runtime-state change. No new saved Span/pointer/
snapshot/recursion/stackalloc. All6567 baseline expectations/frozen35 unchanged.
Parser256/captures256/instructions4096/marks256/frames1024/defaultwork1m and
existing classification/proof1000 ceilings remain; no new per-match scan/state.

## Verification and boundaries

Native-first full spans for root/interior prefix pruning, numeric/forward/named/
duplicate refs, greedy/lazy/possessive and zero forms, mixed depth4/5/34/200, live
trailing dependencies and all-omitted/retained/DEFINE/call/no-LF/CR/G controls.
Six newlines/UCP/every caller offset/strict-tolerant fragments; exact/exhausted
observed work/cancellation/default no-LF budget/parser257. No unsafe Xuc probes100.
Tests-only draft -> successful-build expected RED with6567 baseline passing ->
compiler helper -> six native Staging OS/arch/net8/9/10 full/frozen214/configured
gates/7 equal workloads -> author review and exact head-tree-log/frozen/clean/
whitespace/tests-afterRED audit -> ready; user merge.

C#13/net8/net9/net10, UTF8 LF, root project/solution and src/tests/samples. No agents/
Python/SDK install/public/package/version/dependency/workflow/release/consumer/
fallback change. Retained/nullable/alternative/other pruned-prefix forms, calls/
returned captures, DEFINE/other conditions and broader102/byte99/unsafe100 remain.
Grep last; CommandFramework removal requires publication and all known migrations.
