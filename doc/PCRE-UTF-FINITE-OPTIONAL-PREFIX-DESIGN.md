# UTF finite optional atom prefixes

## Intent and native evidence

Continue #7/#102 after merged138. Native PCRE.NET1.6.0/PCRE2 10.48 JIT
preserves start-bitmap study across finite zero-minimum UPTO/TYPEUPTO/CRRANGE
prefixes whose atom/source shape already qualifies for QUERY/TYPEQUERY/CRQUERY.
The compiler currently restricts all four supported optional cases to Maximum1,
losing LF-after-CR entry at bounds2 and above.
With S=[\x09-\x0c\x{100}], UTF ANYCRLF, B{0,2}(S+)A,
[\x{100}-\x{101}]{0,3}?(S+)A, \h{0,5}+(S+)A and ASCII \S{0,2}(S+)A
select CRLF+A1:2, U0100+CRLF+A3:2 and VT+CRLF+VT+A2:3.
Native source pcre2_study.c1600..1910 continues across finite zero-minimum
literal/type/class repeats. pcre2_jit_compile.c1520..1675 treats UPTO/TYPEUPTO/
nonexact CRRANGE like queries: early-fail count3 blocks later repeat skipping.
Initial360 native probes confirm bound2/3/65535, greedy/lazy/possessive,
ordinary/Unicode/source/UCP/restricted types and excluded property/ANY controls.

## Approach under standing authorization

Extend existing shared optional-prefix cases from max1 to any positive finite
maximum; reuse literal caseset/source-shape and optional-set/type classifiers.
This is one bound-classification change across four existing arms, with no new
runtime/semantic engine. Limiting only the type arm would leave the identical
literal/set bug; admitting unbounded stars would mix different native early-fail
rules. Preserve that separate work.

Each of the current optional Literal, ScalarSet and two ScalarClass arms keeps
Minimum0, uses Maximum >0 and <int.MaxValue, and still returns cost2/retained true.
Keep native literal caseset/r suppression, set provenance/range/CR/predicate/
CT/1000-visit checks, four type Kind/Negated cases, possessive wrapper1 and
BlocksSkip/depth/capture rules intact. Max0 omission remains handled separately;
required/exact-minpositive and unbounded repeats do not enter these arms.

Native counts allow65535 but this managed compiler expands bounded repeats
under its existing4096 instruction/visit limits. Do not raise limits or claim
all65535-sized patterns compile. Pin large-count expansion rejection and65536
syntax rejection separately; use accepted bounds2/3/5/17/128/512 in match tests.
No parser/membership/case-fold/matcher/Step/instruction/frame/journal/layout/
storage/limit/public/package/dependency/workflow/release/consumer changes.
Keep parser256/captures256/instructions4096/marks256/frames1024/work1m/
proof1000/window12. All8376 baseline expectations/frozen35 unchanged.

## Verification and boundaries

Native-first full captures across supported literal/set/type source forms,
UCP/local ASCII/restricted casing and property exclusions, bounds/modifiers/
alias spacing, consumed/deep/capture/dependency/mixed prefixes, CR/negated/
predicate/ANY/property/required/trailing/no-LF/G controls. Larger accepted
bounds and expansion/syntax diagnostics preserve resource policy. Unbounded
controls pin ordinary consumption only, with no bitmap availability claim.
Six newline modes/UCP/every offset/strict-tolerant fragments, exact/exhausted/
observed work, cancellation/default budget/parser257 remain behavioral tests.
No unsafe Xuc probe/source-text assertion.

Tests-only draft -> observed successful-build new-class RED with all8376baseline
passing -> source -> exact-head six native Staging OS/arch/net8/9/10 full/
frozen214/configured gates/7equal workloads -> wholebranch author review/
strict audit -> issue102/PR ready; user review/merge. No subagents/Python/SDK
installation. C#13/net8/9/10, UTF8 LF, root project/solution,src/tests/samples.
Keep102 open for bracket predicates, unbounded, groups/alternatives/negated/
property/call/DEFINE/condition availability; byte99/unsafe100 separate. No full
PCRE/release/Grep readiness claim. Grep last; CommandFramework untouched until
publication and all known migrations.
