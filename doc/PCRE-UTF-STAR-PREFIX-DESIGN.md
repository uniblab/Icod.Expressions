# UTF star-prefix bitmap entry and consumed-run recovery

## Intent and evidence

Continue #7/#102 after merged139. Native PCRE.NET1.6.0/PCRE2 10.48 JIT
retains bitmap study across supported STAR/MINSTAR/POSSTAR atom prefixes.
The finite-only compiler guard currently stops there, losing LF-after-CR entry.
Native UTF ANYCRLF, S=[\x09-\x0c\x{100}], B*(S+)A on VT+CRLF+VT+A
selects LF2:3. All432 initial native checks confirm source/UCP/casing/modifier
availability and exclusions. Merely allowing all stars is insufficient: a root
star can own native type_skip and cache its consumed-run endpoint. For example
[\x{100}-\x{101}]*(S+)A on U0100+CRLF+A misses, while B*(S+)A selects LF3:2.
The wide prefix consumed U0100 and resumes bumpalong at CR; the literal prefix
consumed nothing. This differs from the finite query's later-repeat skip barrier.

Native pcre2_study.c1600..1910 permits supported star bitmap study;
pcre2_jit_compile.c1397..1775 assigns the first count0 accelerated iterator
as type_skip, then count4 prevents a later PLUS from receiving type_skip.
A preceding finite query already raises count3; a retained zero bracket stops
that enhancement. Such locally proven barriers permit a following star without
assigning it its own consumed-run recovery.

## Approach under standing authorization

Reuse existing atom/source classifiers, SkipCrLfAfterRepeat instruction bit and
per-attempt utfRepeatEnd integer. No new runtime layout, journal/frame data or
span/pointer storage. Extend the selector only when a star is either:
1. the root branch's first atom (possibly possessive), with a recorded identity; or
2. preceded in the same branch by an already proven retained optional prefix.
First stars inside a wrapper or after erased prefixes remain conservative;
root stars followed by deeper mandatory groups remain eligible. Broader source
placement needs a separate native early-fail proof.

Compiler: retain a compile-time PcreByteNode reference for a root-leading
min0/maxint.MaxValue child. Existing four optional cases keep min0/max>0 and
call CanCrossUtfCrLfOptionalRepeat(repeat,blocksSkip): finite bounds pass;
unbounded ones require a local retained barrier or that root child identity.
Keep literal caseset/r, original set source/CR/range/predicate/CT/1000-visit and
four type Kind/Negated/UCP/ASCII checks. Cost2/wrapper1/retained/BlocksSkip and
following capture/depth eligibility remain unchanged. Clear the provisional
root identity if no eligible following mandatory set PLUS is found.

Emit the existing SkipCrLfAfterRepeat bit for that root star's Literal,
ScalarClass or ScalarSet atom. Preserve existing leading-PLUS marker selection.
Matcher: after an already charged successful Literal/ScalarClass comparison,
record max(utfRepeatEnd,position) under the same flag/no-call/no-assertion guard
already used for ScalarSet. Existing per-attempt reset and failure bumpalong
recovery apply; consumed runs ending at CR resume past CRLF, empty runs allow
bitmap LF entry. No extra scan/Step or new matcher state. Keep ExplicitNewline,
fragment/caller bounds, captures, backtracking, work/depth/cancellation intact.

No parser/membership/folding/program/instruction/frame/journal layout/limit/
public/package/version/workflow/dependency/consumer changes. Keep parser256/
captures256/instructions4096/marks256/frames1024/work1m/proof1000/window12.
All8993 baseline expectations/frozen35 unchanged. Required and rejected atom/
property/CR/other source shapes remain conservative. Native is reference-only.

## Verification and boundaries

Native-first full captures for literal/set/type stars, greedy/lazy/possessive/
counted aliases, source/UCP/casing/restrictions, empty versus consumed runs ending
at CR, overlapping classes, single/multiple stars and finite/retained barriers.
Root star before deeper/captured/dependent mandatory sets, mixed unsupported
prefixes, newline flags/CR/property/required/trailing/no-LF/G controls. Wrappers
around a first star and erased predecessors have consumption-only controls.
Six newline modes/UCP/every offset/strict-tolerant fragments; exact/exhausted/
observed work, cancellation/default budget/parser257 and depth rejection remain
behavioral tests. No unsafe Xuc probes or source-text assertions.

Tests-only draft -> observed successful-build new-class RED/all8993baseline
pass -> source -> six native Staging OS/arch/net8/9/10 full/frozen214/configured
gates/7equal workloads -> wholebranch author review/strict audit -> issue102/
PR ready; user review/merge. No subagents/Python/SDK installation. C#13/net8/9/10,
UTF8 LF, root project/solution,src/tests/samples. Keep102 open for further star
placement, bracket predicates/groups/alternatives/negated/property/call/DEFINE/
condition availability; byte99/unsafe100 separate. No complete PCRE/release/Grep
readiness claim. Grep last; CF untouched until publication/all known migrations.
