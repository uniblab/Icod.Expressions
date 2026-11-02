# UTF singleton literal query availability

## Intent and evidence

Continue #7/#102 after merged #136. Distinguish native ordinary optional literal
queries from PT_CLIST property queries. Admit genuine ordinary bracket singletons
while correcting plain caseless multicase literal prefixes that currently imply
bitmap entry even though native property-query study fails.

PCRE.NET1.6.0/PCRE2 10.48 JIT, UTF ANYCRLF, S=[\x09-\x0c\x{100}]:
(?i)[B]?(S+)A enters CRLF+A1:2, U0100+CRLF+A atLF3:2 and VT+CRLF+VT+A atLF2:3.
(?i)k?(S+)A and [k]? instead miss the first two and selectVT3:2 on the third.
(?ir)k?, [k]? and Kelvin-singleton queries regain bitmap entry because native
restricted casing suppresses the ASCII-crossing K/S casesets. Sigma/titlecase
casesets remain property queries even with r.

Native pcre2_compile.c8414..8460 emits PT_CLIST when UCD_CASESET is nonzero,
except restricted sets whose first member is ASCII. Pinned pcre2_ucd.c117..154
has27 default groups/84 members; its two additional Turkish pairs begin at index112
and are not default UCD_CASESET entries. Current unsupported Turkish-extra modes
remain out of scope. This table classifies source opcode availability only and
must not replace membership/case folding or introduce a Unicode engine version.

## Approach under standing autonomy

Retain each genuine bracket singleton's original scalar before canonical folding
and share a private compiler predicate between bracket and plain literal query
selection. Preserve #136's proven Class source behavior. Guessing from canonical
keys would couple availability to host Unicode folding; broad rejection would
continue missing ordinary literals. No new runtime metadata or second engine.

## Contract

PcreByteSyntax.cs: add internal init-only int SourceLiteral=-1 to ScalarSet;
keep Source enum/default Unknown and four constructor parameters unchanged.
PcreByteParser.cs: at class close, before folding, record ranges[0].Start only
when Source is Literal, else -1. Preserve Source/SourceLiteral in record copies.

PcreByteProgram.cs: private static bool HasUtfCrLfLiteralBitmap(int scalar,
bool caseless, bool restricted). Reject unknown/out-of-scalar/CR. Caseful valid
literals qualify. For caseless, restricted ASCII-crossing K/k/Kelvin and S/s/long-s
groups qualify; otherwise exclude exactly the84 default pinned UCD_CASESET
members. All other scalars qualify. Use a bounded constant pattern, no per-match
lookup/folding/native dependency. Document the pinned source/version.

The optional Literal min0/max1 arm calls this predicate with its original Value
and flags. IsUtfCrLfOptionalSet retains every existing range/predicate/CR/CT/1000
check, then admits Source Literal through this predicate using SourceLiteral.
Source Class/noncaseless/multikey behavior remains unchanged. Unknown one-key
sources stay conservative. Optional prefixes still set BlocksSkip; properties
stop selection. Cost2 plus possessive wrapper1/depth/capture rules unchanged.

No membership/case closure/newline flags/UTF validation/diagnostic/Step/matcher/
program/instruction/frame/journal/runtime storage/limit changes. Retain parser256/
captures256/instructions4096/marks256/frames1024/work1m/proof1000/window12.
All7565 baseline expectations/frozen35 unchanged. Metadata is compile-time only.

## Verification and boundaries

Native-first full spans for every one of84 default members as plain/bracket
singletons, ordinary ASCII/wide/astral/special I/two-case characters, restricted
ASCII-crossing versus non-ASCII casesets, syntax/modifiers/consumed prefixes,
depth4/5/34/200, dependencies/mixed/trailing/CR/negated/predicate/newline/G/no-LF
controls. Six newlines/UCP/all offsets/strict-tolerant fragments, work/exhaustion/
cancellation/default budget/parser ceiling. No unsafe Xuc probes. Tests-only RED
then source -> six native Staging OS/arch/net8/9/10 full/frozen214/configured gates/
seven equal workloads -> author review/exact audit -> ready; user merge.

C#13/net8/9/10, UTF8 LF, root project/solution and src/tests/samples. No subagents/
Python/SDK installation/public/package/version/dependency/workflow/release/
consumer/fallback change. Native table is pinned optimizer classification, not
full Unicode/membership parity. Broader102 source/nullable/type/group/negated/
predicate/call/DEFINE/condition forms remain; byte99/unsafe100 separate. Grep last;
no destructive CommandFramework work before publication/all known migrations.
