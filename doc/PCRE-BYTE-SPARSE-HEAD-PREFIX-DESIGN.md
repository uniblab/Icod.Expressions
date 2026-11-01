# Internal PCRE homogeneous sparse byte head prefixes

## Intent and native cause

Continue #7/#99 after #123. Admit a sparse CR/LF-containing Bitmap only when one
direct exact repeat is the entire consuming prefix before a sole final literal.
Native can select a literal, a SIMD pair or a bitmap table for these shapes; the
existing conditional literal selector still preserves match results in this
homogeneous head case without cloning those native optimizers.

Authority: pinned PCRE.NET1.6.0 / PCRE2 10.48 JIT; scan_prefix bitmap exact CRRANGE
and finite sets, priority6547ff, pair6390ff and minimum-four table6585ff in pinned
pcre2_jit_compile.c. Native-first probes cover2/3/4/5-member CR/LF sets, fixed
counts2/3/4/11/12 and ordinary/capture/atomic wrappers. Source metadata unchanged.

## Proof and approach

If a candidate at LF could match S{n}L, S contains CR and LF, and LF follows CR,
then the preceding CR candidate's n bitmap positions also fit S: CR fits, and
the remaining positions overlap n-1 successful LF-candidate positions. Thus when
the literal passes at CR the CR candidate succeeds first; when it rejects CR,
native prefiltering can reach LF. A CR bitmap failure also precludes a match at
LF. This argument requires the repeated bitmap to be the complete homogeneous
head; it does not justify mixed prefixes or arbitrary zero-width operations.

Alternatives: reproduce all sparse SIMD/table optimizers, globally admit sparse
bitmap atoms, or use this bounded overlap proof in existing compilation. Choose
the third under standing implementation authority. Preserve existing broader
prefix paths from #122/#123 without sealing them.

Add a compile-local bool sparseBitmapPrefix. A direct exact Bitmap repeat min>=2
may qualify at prefixLength0 if both CR/LF are members. Existing broad tests still
qualify at any previously admitted position. For a newly admitted non-broad repeat,
add its width then set sparseBitmapPrefix; AddWidth rejects any later consuming
prefix atom. Groups/atomic wrappers may surround the repeat or final literal;
ordinary bitmap atoms/count1 and copied groups containing those atoms still fail.
Existing sole-final-literal handling, width1..11/window12, proof1000, source kinds,
bool/conditional descriptor, local case/UCP/captures and charged runtime selector
remain. No parser/AST/set/source-flag/matcher/VM/Step/frame/journal/limit change.

## Work expectations and scope

Two previous sparse no-match budget controls become eligible. Update only those
expected budgets16->22 before tests-only RED: one selector comparison plus a
failed LF attempt add six charged steps. Rename their theory methods to describe
current eligibility/work. Match outcomes remain no-match. Existing ordinary atom
and count1 budgets stay unchanged; no work ceiling or frozen GNU test changes.

Mixed sparse prefixes, CR-excluding/non-LF sets, legacy Atom mixtures, coupled
positive-interior flags, variable/zero/multi-literal/alternative/assertion/anchor/
call/reset shapes and general optimizer availability remain #99. Known broader
misses/false positives remain. UTF #102 and unsafe native #100 separate.

Internal C#13/net8/net9/net10; UTF8 LF; root project/solution, src/tests/samples.
No subagents/Python/SDK install, public/package/version/dependency/workflow/release/
consumer changes, production native/Regex fallback, engine clones, saved spans/
pointers/snapshots or raised limits. Depth256/captures256/instructions4096/marks256/
frames1024/default work1,000,000; proof1000/window12. Frozen35 intact. Grep last;
CommandFramework removal requires publication and all known migrations. User merge.

## Verification / builder check

Native-first full captures for2..5 members, counts2/3/4/11/12, local options,
wrappers/group copies, literal overlap and rejected mixed shapes. Six newline/
two UCP/every-offset protocols, supported UTF controls, work/budget/cancellation
and compile ceilings. Tests-only successful-build RED before source; all5799
unchanged baseline expectations pass, two updated work expectations fail as
expected. Tests unchanged afterRED. Six native Staging/net8/9/10 full/frozen214/
configured gates green, zero failed/skipped/build warnings/errors. Author review
and exact head/tree/log/frozen/whitespace/clean audit. Interface check: one bool
belongs only to compiler proof; existing AddWidth enforces complete-prefix shape.
