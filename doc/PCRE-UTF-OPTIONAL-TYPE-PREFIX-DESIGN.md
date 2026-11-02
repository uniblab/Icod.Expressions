# UTF optional type-query prefixes

## Intent and evidence

Continue #7/#102 after merged137. Native PCRE.NET1.6.0/PCRE2 10.48 JIT
retains start-bitmap study across optional ASCII digit/word/nonspace and
horizontal-space queries. The current shared UTF leading-node helper stops at
all ScalarClass nodes, losing LF-after-CR entry for these supported types.
With S=[\x09-\x0c\x{100}], UTF ANYCRLF, \d?(S+)A, \w?(S+)A,
\h?(S+)A and ASCII \S?(S+)A enter CRLF+A at1:2, U0100+CRLF+A at3:2,
and VT+CRLF+VT+A at2:3. UCP changes digit/word/nonspace into property
queries (no bitmap), unless their corresponding aD/aW/aS restriction applies;
\h retains type-query availability regardless of UCP.

Native pcre2_study.c1600..1728 handles TYPEQUERY HSPACE/DIGIT/WORDCHAR/
NOT_WHITESPACE but rejects unsupported property/ANY/NOT_VSPACE types.
The accepted four types exclude CR. Native detect_early_fail in
pcre2_jit_compile.c1397..1550 raises optional type-query count to3, so
bitmap proof may continue but repeat skipping must remain blocked.

## Approach and constraints

Implementation choice under standing authorization: extend the existing compiler
switch using parsed ScalarClass Kind/Negated values. Those already encode UCP
and local ASCII restrictions. Add no parser metadata or runtime state.
Inferring from global UCP would mishandle scoped restrictions; admitting every
CR-excluding predicate would incorrectly include property/NOT_VSPACE queries.

Admit only direct min0/max1 ScalarClass positive AsciiDigit/AsciiWord/
HorizontalSpace, or negated AsciiSpace. Return cost2/retained true through
the existing shared1000-visit/wrapper1/BlocksSkip path. Keep CR-consuming,
Unicode property, dot, newline sequence, bracket predicate, larger optional
bounds, groups/alternatives/calls/DEFINE/conditions outside this extension.
No type membership, case-fold, parser, matcher, Step, instruction/frame/journal,
layout/limit/public API/package/version/dependency/workflow/consumer changes.
Keep parser256/captures256/instructions4096/marks256/frames1024/work1m/
proof1000/window12, all7947 baseline expectations and35frozen blobs.

## Verification

Tests pin full native capture spans for each admitted kind/negation under UCP
and scoped ASCII switches, optional greedy/lazy/possessive/counted modifiers,
consumed prefixes, root/capturing/noncapturing/depth4/5/34/200,
related/unrelated captures, mixed ordinary/property/pruned/retained prefixes,
CR-consuming/property/NOT_VSPACE/ANY/required/repeated/trailing/no-LF/G controls.
Six newline modes/UCP/every offset/strict-tolerant fragments preserve boundaries.
Exact/exhausted observed/unobserved work, cancellation/default budget/parser257
remain behavioral tests. No unsafe Xuc probe or source-text assertions.

Native-first checks -> tests-only draft -> observed successful-build RED,
all7947 baseline pass -> minimal compiler change -> exact-head six native
Staging platforms/net8/9/10 full/frozen214/configured gates/7equal workloads ->
whole-branch author self-review -> strict audit -> issue102/PR ready; user merge.
C#13/net8/9/10, UTF8 LF, root project/solution,src/tests/samples. No subagents/
Python/SDK installation. Byte99/unsafe100 separate; keep102 open for broader
availability. No full PCRE/release/Grep-readiness claim; Grep last and no
CommandFramework removal before publication/all known consumer migrations.
