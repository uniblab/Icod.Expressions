# UTF entry through pruned atom prefixes

## Intent and native evidence

Continue #7/#102 after merged #133. Direct zero-count literals, character types,
classes and newline sequences disappear from native bytecode. They do not block
bitmap recognition or repeat acceleration. A retained zero-count bracket still
blocks acceleration, even when its contents are themselves pruned atoms.

Pinned PCRE.NET 1.6.0 / PCRE2 10.48 JIT, UTF ANYCRLF, S=[\x09-\x0c\x{100}]:
B{0}(S+)A matches initial CRLF+A at 1:2, misses U0100+CRLF+A, and selects
VT3:2 on VT+CRLF+VT+A. Related trailing references or depth beyond four retain
bitmap entry without acceleration, as existing rules require. By contrast,
\n{0}(S+)A enters LF3:2/LF2:3 on the latter subjects: source newline metadata
survives opcode pruning. Do not erase that metadata or rewrite the AST.

Native pcre2_compile.c 7371..7410 routes CHAR/CHARI/NOT/NOTI to single repeats;
7413..7427 removes CLASS/NCLASS/XCLASS/ECLASS and direct references at max zero;
7857..7900 routes character types/properties to OUTPUT_SINGLE_REPEAT, resets
code to previous, then omits max-zero items. Retained brackets use OP_SKIPZERO.

## Approaches and implementer choice

Under standing autonomy, extend the existing first-node helper's omitted-child
pattern to supported UTF primitive nodes. The existing Node/BlocksSkip result,
root/group traversal, selected-leaf proof and marker logic already express the
native distinction. An AST rewrite risks losing source metadata and declaration
information. A second prefix walker would duplicate bounds and source decisions.
Use the shared helper; preserve all other admission guards.

## Compiler contract

In src/Pcre/PcreByteProgram.cs, FindUtfCrLfLeadingNode keeps its existing tuple
signature. Direct Repeat{Maximum:0} with child Literal/Any/ScalarClass/ScalarSet/
NewlineSequence joins Backreference in the cost-two, retained-false arm. An outer
possessive Atomic adds one visit as before. Known retained bracket arms remain
unchanged and continue setting BlocksSkip. A pruned atom following a retained
bracket must never clear that bit. All-skipped/over-budget sequences yield no
proof; nonzero/unknown nodes remain significant.

FindUtfCrLfLeadingRepeat and HasUtfCrLfStartClass retain their shared traversal,
selected-leaf identity, capture dependencies and depth/proof rules. This does not
admit general optional starts or finite/exact/alternative prefixes. Source flags,
AST nodes and runtime compilation remain intact; only first-node selection skips
the omitted primitives. No byte-mode or UTF validation change.

No matcher/program/instruction layout/Step/capture/frame/journal/runtime-state
change, saved spans/interior pointers/snapshots, recursion or stackalloc. Keep
parser depth256, captures256, instructions4096, repeat marks256, frames1024,
default work1m, classification/proof1000 and prefix window12. All6789 baseline
test expectations and frozen35 source blobs remain unchanged.

## Verification and boundaries

Native-first literal full captures cover primitive kinds, zero-count syntax and
modifiers, ASCII/wide classes and PLUS variants, source newline preservation,
root/interior/deep groups, references and omitted/retained combinations. Include
all-skipped/trailing/nonzero/no-LF/CR/G/call/DEFINE controls. Six newline modes,
UCP, every caller offset, strict/tolerant fragments, observed/unobserved exact and
exhausted work, cancellation, default no-LF budget and parser257 are covered.
No unsafe positive Xuc malformed-UTF probes (#100).

Tests-only draft -> successful-build expected RED with6789 baseline passing ->
helper extension -> six native Staging OS/arch/net8/9/10 full/frozen214/configured
gates and seven equal workloads -> whole-branch author self-review and exact
head/tree/log/frozen/tests-after-RED/clean/whitespace audit -> ready; user merges.

C#13, net8/net9/net10, UTF8 LF, root project/solution and src/tests/samples. No
subagents/Python/SDK installation/public/package/version/dependency/workflow/
release/consumer/fallback changes. General nullable/alternative/other pruned
starts, calls/returned captures, DEFINE/other conditions and broader optimizer
provenance remain in102; byte99 and unsafe100 remain separate. Grep last;
CommandFramework removal requires publication and all known consumer migrations.
