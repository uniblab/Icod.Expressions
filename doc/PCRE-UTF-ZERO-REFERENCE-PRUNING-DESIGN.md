# Internal UTF direct zero-count reference pruning

## Intent, evidence and implementer choice

Continue #7/#102 after merged #130. In the existing whole-root capture-dependency
classifier, ignore a direct Backreference repeat whose maximum is zero. Native
pcre2_compile.c7417..7427 omits OP_REF/REFI/DNREF/DNREFI entirely for that count.
It cannot clear optimized_cbrackets during native JIT dependency analysis.
Native zero-count groups instead retain OP_SKIPZERO/body at7574..7601; references
inside those bodies can still disable leading-capture repeat skipping.

PCRE.NET1.6.0/PCRE2 10.48 JIT, UTF ANYCRLF, S=[\x09-\x0c\x{100}]:
(S+)A\1{0} on initial CRLF+A matches1:2/group1=1:1, on U0100+CRLF+A misses,
and on VT+CRLF+VT+A selects VT3:2/group1=3:1. (S+)A(?:\1){0} instead matches
LF3:2 on the second subject and LF2:3 on the third. A nested (?:\1{0}){0}
keeps the outer group body but removes the inner reference, restoring VT recovery.

Under standing autonomy, change the existing direct-zero handler from ineligible
to no dependency effect. Reuse Set/Skip classification and existing matcher paths.
A full native pruning/leading-prefix model would broaden this change; defer it.
A new AST rewrite or matcher scan would duplicate existing zero-repeat handling.

## Change and invariants

src/Pcre/PcreByteProgram.cs ClassifyUtfCaptureRepeat: the
PcreByteRepeat{Maximum:0,Child:PcreByteBackreference} case breaks without pushing
the child. Preserve generic Repeat traversal for zero-count group/assertion/
condition bodies; preserve later live references and capture-condition slot tests.
Named/duplicate targets are omitted only for that direct shape. Lazy/possessive
zero counts and nested direct zeros share existing AST wrappers.

No parser/AST/membership/source metadata/UTF validation/matcher/program/instruction
layout/Step/capture/frame/journal/runtime state change. Existing compiler1000
branch/node/queue bound, CT and prefix-slot set<=256 stay; all parser256/captures256/
instructions4096/marks256/frames1024/defaultwork1m ceilings stay. No recursion/
stackalloc/saved Span/interior pointer/capture snapshot. All6464 baseline
expectations/frozen35 unchanged. Noncapture-only classification remains unchanged.

## Verification and boundaries

Native-first full spans for ASCII/wide class PLUS, greedy/lazy/possessive and
zero-count forms, numeric/relative/named/duplicate references, mixed depth4/5/34/200,
direct versus retained grouped/assertion/condition dependencies, nested zero-count
pruning and later live references. Six newlines/UCP/all caller offsets/strict-
tolerant fragments; exact/exhausted observed work, cancellation, default no-LF
budget and parser257 rejection. Avoid unsafe malformed Xuc native probes100.
Tests-only draft -> successful-build expected RED with6464 baseline passing ->
minimal compiler change -> six native Staging OS/arch/net8/9/10 full/frozen214 and
configured gates/7 equal workloads -> whole-branch author review/exact head-tree-
log/frozen/clean/whitespace/tests-afterRED audit -> ready; user merge.

C#13/net8/net9/net10, UTF8 LF; root project/solution, src/tests/samples. No agents/
Python/SDK install, public/package/version/dependency/workflow/release/consumer/
fallback changes. Leading-prefix pruning before the selected PLUS, calls/returned
captures, DEFINE/other conditions and broader102/byte99/unsafe100 remain. Grep last;
CommandFramework removal requires publication and all known migrations.
