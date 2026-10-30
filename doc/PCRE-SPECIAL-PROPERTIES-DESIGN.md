# Internal PCRE special Unicode properties

**Status:** issue #7, following merged #97; UTF-8 internal profile only.

## Intent and scope

Add the six PCRE-specific properties Any, Xan, Xps, Xsp, Xwd and Xuc against
PCRE.NET1.6.0 / PCRE2 10.48 Compiled/JIT. Reuse the existing scalar atom and
bracket-predicate path. Script/binary properties, runtime Unicode table version
reconciliation, byte-mode properties, public API/release and consumer migration
remain separate gates. No production native/Regex fallback.

## Membership

| Property | Membership over valid decoded Unicode scalars |
|---|---|
| Any | Every scalar, including NUL, newline, private/unassigned/noncharacter and supplementary values |
| Xan | General category L or N; excludes underscore and combining marks |
| Xps, Xsp | Existing PCRE UnicodeSpace: H/V whitespace plus Z, including U+0085 and U+180E; both names are identical |
| Xwd | L, N, Mn or Pc; includes underscore/nonspacing marks, excludes Mc and Me |
| Xuc | U+0024, U+0040, U+0060, or U+00A0..U+10FFFF excluding surrogates |

Xuc matches the characters permitted in universal-character names, not the text
of an escape spelling. UTF decoding continues to reject/partition surrogate and
malformed encodings before predicate evaluation. Complements never consume an
invalid code unit. Any is independent of DotAll and the newline convention.

Explicit properties ignore UCP, aD/aS/aW/aP/aT/bare-a restrictions and case-fold
restrictions. Existing caseless/category handling remains; none of these six
predicates changes membership under i/r. Category-derived predicates retain the
existing target-runtime Unicode data policy, not a new full-version parity claim.
Do not replace PCRE space membership with Rune.IsWhiteSpace (U+180E matters).

## Names, complements and profile boundaries

Use existing ASCII loose case/separator normalization in braced property names.
Normalize the whole braced token before consuming one leading caret, so ignored
hyphens/underscores/ASCII whitespace before that caret work like native PCRE2.
P and caret independently invert membership; preserve ordinary unions and outer
bracket negation. Short single-character property parsing remains unchanged.

Special names resolve only as unprefixed names. Existing internal gc:/gc=/
General_Category category aliases remain restricted to general categories; they
must not accidentally admit Any/X names when the shared property resolver grows.
Those preexisting category-namespace aliases are a library profile extension;
the pinned native parser does not accept them. Do not broaden or remove that
existing profile in this tranche. Unknown/script/binary/property-namespace forms
keep existing UnsupportedSyntax policy at the original escape/bracket opener;
malformed braces/empty names and nonliteral range endpoints retain InvalidSyntax.
The current byte-mode property gate remains UnsupportedSyntax even where native
8-bit PCRE2 can evaluate such a property.

## Architecture and approaches

Generating a second range table would duplicate existing alnum/word/space rules
and complicate complements, casing and Unicode-version policy. Lowering Any to
dot would wrongly apply DotAll/newline behavior. The maintainer choice is to
retain scalar predicates: Xan uses UnicodeAlnum, Xps/Xsp use UnicodeSpace, Xwd
uses UnicodeWord; add AnyScalar and UniversalCharacterName kinds with direct
membership checks before category lookup. No new matcher opcode or saved state.

ParseUnicodeProperty must distinguish category-prefixed tokens before optional
special lookup. Keep GeneralCategory dedicated to its current namespace and add
a small SpecialProperty factory. Bracket parsing already preserves scalar-class
items as predicates, so the same kinds work in atoms/unions/complements without
range expansion, cloned engines or independent Unicode tables.

## Metadata, widths and resources

All six properties consume exactly one decoded scalar. Existing repeats,
capture/backreference state, recursive calls, assertions and fixed/variable
behind widths apply. Properties do not by themselves set explicit-CR/LF source
metadata, even when their membership includes CR/LF. Preserve predicate lowering
so unanchored ANYCRLF candidate continuation retains native pair-skipping.
Literal CR/LF in a mixed class still contributes its existing metadata.

Native research exposed two existing UTF class metadata defects that affect new
property combinations: deriving flags from range membership marks interior-only
CR/LF incorrectly, and suppressing every negated class misses literal CR/LF in
mixed/duplicate classes. Correct the UTF path here using source provenance.
Track original operand count, whether the sole operand is a literal or equal
endpoint range, and literal/range-endpoint CR/LF before folding or deduplication.
Store ExplicitNewline on PcreByteScalarSet. A negated single literal/equal range
is the native exception and suppresses the flag; duplicate literals and
predicates prevent that exception even when they add no distinct values.
Compiler metadata reads the stored flag. Add observable CRLF search regressions
for old general-category/range cases as well as new properties, quoting/xx,
erased/DEFINE bodies and caller offsets. Byte-mode class/search optimization
parity remains a separate preexisting gate; this correction is UTF only.

Existing instruction/predicate lookup work and cancellation remain. Atoms emit
one scalar-class instruction and class unions spend work per inspected term;
no 1.1-million-scalar expansion, extra frames, journal, capture copies or native
stack recursion. Depth256/captures256/instructions4096/marks256/savedframes1024/
default work1,000,000 remain shared.

## Delivery and evidence

C#13, net8/net9/net10; UTF-8 LF; root solution/project, src, tests, samples;
no Python. No API/package/version/dependency/workflow/release/consumer changes.
Grep stays last and CommandFramework removal remains gated on publication and
migration of every known consumer. User owns merge; preserve worktree.

Primary pinned sources: pcre2pattern.3 property descriptions around883 and1110,
pcre2_compile.c get_ucp property normalization/namespace parsing, pcre2_xclass.c
membership around110-178 and the generated property table. Native fixtures must
cover exact stable category representatives, complete small boundary sets,
NUL/noncharacters/max scalar, loose names/caret/P, namespace and byte-profile
gates, i/r/ASCII/UCP independence, union/complement/range diagnostics, captures/
retries/recursive calls/lookarounds, widths, CRLF metadata, malformed UTF/caller
offsets/G and shared source/compile/work/cancellation limits.

Tests/docs draft precedes observed expected-red CI and production changes.
Fresh independent whole-branch review plus six native Staging full/frozen
net8/9/10 suites and configured workload/extraction/API/package gates precede
ready status. No local SDK; managed executable verification comes from CI.
