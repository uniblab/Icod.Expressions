# UTF star prefixes inside noncapturing wrappers

## Intent and evidence

Continue #7/#102 after merged141. Native PCRE.NET1.6.0/PCRE2 10.48 JIT
studies bitmap entry through deeper noncapturing wrappers than its early-fail
iterator recovery. Existing discovery stops at the first noncapturing group,
so it cannot recognize a first star inside an otherwise supported leading chain.
Initial96 native checks pin wrapper counts1/3/4/5/33/199 with literal/wide-set
stars and UCP. For S=[\x09-\x0c\x{100}], (?:B*(S+))A enters CRLF+A1:2
and VT+CRLF+VT+A2:3. With B+CRLF+A, wrappers1..4 miss because B* owns
consumed-run recovery; wrappers5+ enter LF2:2 because native no longer assigns
that cache. Wide-set stars consuming U0100 show the same boundary at LF3:2.
The suffix capture is not a wrapper ancestor of the star and must not affect
its cache depth. Native detect_early_fail recurses into BRA through depth4;
set_start_bits continues deeper. No parsing/membership change is required.

## Approach under standing authorization

Extend the bounded first-star discovery through single-branch noncapturing
ancestors (and the already supported erased primitives), returning separate
compile-time Atom identity and Skip eligibility. Recognition persists through
supported deep wrappers, but Skip is true only when star ancestor depth<=4.
This preserves bitmap entry without assigning invalid endpoint recovery.
An unconditional cache would introduce false misses at depth5; refusing all
wrappers would keep the existing entry gap.

FindUtfCrLfRootStarAtom returns (PcreByteNode? Atom,bool Skip). Iteratively
follow a current branch/index; charge root1, erased2+possessive wrapper1 and
noncapturing group1 under shared1000-visit/CT checks. Count group depth only
before reaching the star. Validate nonempty/single-branch group bodies. Stop
at capturing or explicit atomic wrappers, alternatives, retained nodes,
queries/required/unsupported nodes. Unwrap an atomic modifier only when its
child is a Repeat, preserving existing possessive-repeat/erased-atom behavior;
do not accidentally admit explicit Atomic(NoncapturingGroup) ancestors.

Compiler stores recognized identity and a compile-time skipUtfCrLfLeadingStar
boolean; clear both when following eligible mandatory set PLUS selection fails.
Existing star admission keeps using identity/local barriers. Literal/ScalarClass/
ScalarSet endpoint marker emission also requires Skip. Existing leading-PLUS
marker remains independent. No matcher/instruction/program/frame/journal runtime
layout/state, Step, scanner or capture changes. Update atomic-following-star
comment to reflect a proven barrier without assuming that deep stars own a cache.

Keep original omission/source/casing/UCP/CR/predicate/range/capture/depth/BlocksSkip/
ExplicitNewline/fragment/work/cancellation guards and parser256/captures256/
instructions4096/marks256/frames1024/work1m/proof1000/window12. All10797 baseline
expectations/frozen35 unchanged. Native remains reference-only; no public/API/
package/version/workflow/dependency/consumer/CF changes.

## Verification and boundaries

Native-first full captures for noncapturing wrapper counts1/3/4/5/6/33/199,
empty versus consumed literal/set/type runs, modifiers/aliases/source/casing/UCP,
erasures outside/inside wrapper chains, mixed stars/barriers, following captured/
related/deep/atomic mandatory sets, explicit newline/unsupported controls.
Capturing/explicit atomic ancestors, nullable prefix wrappers whose mandatory
PLUS is outside the body, and other unsupported placements have ordinary
consumption controls only; do not claim their bitmap parity. Six newline/UCP/
every offset/strict-tolerant fragments, exact/exhausted/observed work, cancellation/
default budget/parser257/depth rejection remain behavioral tests.

Tests-only draft -> successful-build new-class RED/all10797baseline pass ->
source -> six native Staging OS/arch/net8/9/10 full/frozen214/configured gates/
7equal workloads -> wholebranch author review/strict logs/tree/blob/tests-afterRED/
frozen/clean audit -> issue102/PR ready; user merge. No subagents/Python/SDK
installation. C#13/net8/9/10, UTF8 LF, root project/solution,src/tests/samples.
Keep102 open for capturing/atomic ancestors, other placements, bracket predicates/
group/alternative/negated/property/call/DEFINE/condition availability; byte99/
unsafe100 separate. No complete PCRE/release/Grep readiness claim; CF removal
requires publication/all known migrations; Grep last.
