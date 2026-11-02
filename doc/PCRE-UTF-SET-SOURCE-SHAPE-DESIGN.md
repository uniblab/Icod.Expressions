# UTF optional set source shape before case folding

## Intent and native evidence

Continue #7/#102 after merged #135. A caseless class with multiple source
operands may fold to one canonical key while retaining native class bitmap
availability. Preserve source shape before folding instead of guessing it from
normalized membership. Keep genuinely singleton sources conservative.

PCRE.NET1.6.0/PCRE2 10.48 JIT, UTF ANYCRLF, S=[\x09-\x0c\x{100}]:
(?i)[kk]?(S+)A enters CRLF+A at1:2, U0100+CRLF+A atLF3:2, and VT+CRLF+VT+A
atLF2:3. [kK], [k-kk], quoted/escaped duplicates and Kelvin/sigma aliases show
that behavior. In contrast, [k], [k-k] and quoted-single-k keep the native
PT_CLIST property-query path, missing the first two subjects and selectingVT3:2
on the third. Full capture spans and actual optional input consumption remain.

Native pcre2_compile.c6337..6453 distinguishes original one-character classes
before case expansion; multi-character case partners can be optimized only with
additional native caseset guards. Native class study can process the remaining
positive predicate-free class, while singleton caseless property-query study
has different availability. Managed canonical fold keys cannot recover this
original shape; the parser already tracks sourceTermCount/soleLiteral for
newline metadata, so record the distinction there.

## Approaches and implementer choice

Under standing autonomy, add a small immutable source enum to the existing
internal ScalarSet AST, initialized by the UTF bracket parser. Inferring source
shape from folded ranges loses duplicates and aliases; adding a full native
Unicode caseset classifier would broaden the version/source gate. Preserve
Unknown/Literal/Class shape and admit only proven Class sources through the
existing optional-set exception. No AST rewrite or second engine.

## Contract

src/Pcre/PcreByteSyntax.cs: internal enum PcreByteScalarSetSource with
Unknown/Literal/Class; internal init-only Source property on PcreByteScalarSet,
default Unknown. Keep its existing four constructor parameters unchanged.
src/Pcre/PcreByteParser.cs ParseUtf8Bracket: at close, derive Literal when
sourceTermCount==1 && soleLiteral, otherwise Class, using the existing original
operand accounting before folding. Set Source on the returned record. Preserve
ExplicitNewline and every membership/case-closure/diagnostic decision.
Record with-copies retain Source; existing tree rewrites keep scalar leaves by
reference, so capture/call/assertion processing does not erase it.

src/Pcre/PcreByteProgram.cs IsUtfCrLfOptionalSet: preserve the nonempty/positive/
predicate-free/range-order/scalar-bound/CR checks, CT/range visits/shared1000,
repeat/child cost2 and possessive wrapper1. After validation, allow Source==Class
alongside existing noncaseless or multikey cases. Unknown and Literal caseless
one-key forms keep the conservative path. The optional prefix still blocks Skip;
selected mandatory PLUS proof, root/group depth and dependencies remain shared.

The enum is compile-time source provenance only. It is not emitted into the
program or runtime frames. No matcher/program/instruction layout/Step/capture/
frame/journal/storage/saved span/interior pointer/snapshot/stackalloc changes.
Parser256/captures256/instructions4096/marks256/frames1024/work1m/proof1000/window12
remain. All7359 baseline expectations and frozen35 blobs stay unchanged.

## Verification and boundaries

Native-first full captures for duplicate/case-partner/quoted/escaped/equal-range
plus literal/Unicode alias/range forms, modifiers/consumed prefixes, depth4/5/34/
200, dependencies and mixed chains. Genuine singleton, negated/predicate/CR/
source-newline/trailing/all-optional/no-LF/G controls. Six newlines/UCP/all offsets/
strict-tolerant fragments, exact/exhausted observed work, cancellation/default
budget/parser257. No unsafe Xuc probes. Tests-only expected RED then parser/AST/
compiler metadata -> six native Staging OS/arch/net8/9/10 full/frozen214/configured
gates/seven equal workloads -> whole-branch author review/exact audit -> ready;
user merge.

C#13/net8/9/10, UTF8 LF, root project/solution and src/tests/samples. No subagents/
Python/SDK installation/public/package/version/dependency/workflow/release/
consumer/fallback changes. Genuine singleton caseset classification, negated/
predicate starts, types/groups/larger bounds and broader102 source/nullable/call/
DEFINE/condition forms remain; byte99/unsafe100 separate. Grep last; no destructive
CommandFramework work before publication and all known consumer migrations.
