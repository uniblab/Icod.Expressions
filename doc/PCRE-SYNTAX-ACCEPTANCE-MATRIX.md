# PCRE syntax acceptance matrix

**Status:** issue #7 checkpoint against Grep's pinned PCRE.NET 1.6.0 /
PCRE2 10.48 behavior. This is an internal engine inventory, not a public PCRE
compatibility promise or authorization to migrate Grep.

## Contract and classifications

Grep's frozen `PcreTests.cs` consumer floor is mapped in
[`PCRE-GREP-CONSUMER-FIXTURES.md`](PCRE-GREP-CONSUMER-FIXTURES.md). The seven
matcher-component workloads are mapped in
[`PCRE-GREP-WORKLOAD-PROBE-DESIGN.md`](PCRE-GREP-WORKLOAD-PROBE-DESIGN.md).
This table groups the PCRE2 pattern syntax from the official
[syntax summary](https://www.pcre.org/current/doc/html/pcre2syntax.html) and
[pattern details](https://www.pcre.org/current/doc/html/pcre2pattern.html).
The live documentation may describe features newer than the pinned 10.48
reference; each pending implementation needs a version-pinned fixture.

**Supported subset** means the internal compiler accepts only the stated
shape, with the linked tests; it does not mean general PCRE2 parity.
**Pending, rejected** means a representative valid PCRE2 construct must
produce `UnsupportedSyntax` at its opening byte in the internal compiler.
Malformed syntax may instead produce `InvalidSyntax`. **Consumer-owned** means
the matcher returns spans, leaving framing, status and output to Grep.

| PCRE2 family | Internal status and boundary | Current evidence / next gate |
|---|---|---|
| Literal bytes, NUL, UTF-8 scalar literals; `|`, ordinary capture and `(?:...)` | Supported subset; source-byte spans and ordered alternatives | `PcreByteFoundationTests`, `PcreUtf8ScalarTests`, `PcreReferenceCaptureTests` |
| `^`, `$`, `\A`, `\Z`, `\z`, `\b`, `\B`, `\G` | Supported subset under LF, ANYCRLF, CR, NUL, CRLF or ANY convention; `\G` tests the original requested match offset | Anchor and boundary tests; `PcreMatchPositionReferenceTests` pins nonzero offsets and alternatives |
| `\K` reported match start | Resets group zero and whole-match spans; numbered captures retained, reset state journaled on fallback; invalid in brackets/lookarounds or with direct quantifiers | `PcreMatchPositionReferenceTests`; branch/atomic/repetition rollback, byte spans and empty paths |
| Dot, shorthand `\d\D\s\S\w\W`, simple/control/hex escapes | Supported subset with explicit UTF/UCP and ASCII-`\d` policy | Atom, scalar, space/word/digit tests and Grep consumer fixtures |
| `\h\H\v\V` horizontal/vertical whitespace | Fixed PCRE2 lists, independent of UCP/caseless and newline convention; byte and UTF-8 atoms and ordinary bracket members | `PcreSpaceEscapeReferenceTests`; all 256 bytes, Unicode members/boundaries, captures, CRLF offsets and invalid-UTF barriers |
| `\R` newline sequence | Atomic CRLF-first matching; Unicode line endings by default, internal `BsrAnyCrLf` restriction independent of newline convention; invalid in brackets | `PcreNewlineSequenceReferenceTests`; membership, captures/repetition, every CRLF/invalid-UTF offset, work and expansion limits |
| `\N` non-newline, UTF `\N{U+...}` literal | Bare form obeys LF/AnyCRLF and ignores DotAll; counted repetitions supported; Unicode codepoint form shares braced-hex literal/range/fixed-assertion rules | `PcreNonNewlineReferenceTests`; byte/scalar membership, braced-form disambiguation, captures, offsets and diagnostics |
| Bare and braced octal literals | Default up-to-three-digit octals and `\o{...}` in byte/UTF atoms, brackets/ranges and fixed literal assertions; byte/scalar range checks, brace-edge space/tab | `PcreNumericEscapeReferenceTests`; all 256 byte values, UTF bounds, quoting, offsets and diagnostics; extra Python/NO_BS0 modes pending |
| Ordinary `[ ]` ranges, negation, shorthand unions, 14 POSIX names | Supported subset in byte and UTF modes; ordinary bracket grammar unchanged, Perl algebra listed separately | Bracket and POSIX tests; Unicode table/version reconciliation |
| `\p`/`\P` general categories and special properties | Byte/UTF categories plus Any/Xan/Xps/Xsp/Xwd/Xuc; loose names, P/caret complements and property predicates independent of UCP/ASCII/i/r. Category-prefixed aliases remain a library extension for categories only. UTF class source operands/endpoints preserve CR/LF flags before folding, including the native negated-singleton exception. Scripts/scx now have pinned Unicode17 data; binary/Bidi properties now have pinned native data | `PcreBytePropertyReferenceTests`, `PcreSpecialPropertyReferenceTests`, `PcreClassNewlineMetadataReferenceTests`, `PcreUtf8GeneralCategoryTests`; see `PCRE-SPECIAL-PROPERTIES-DESIGN.md`; Unicode17 categories and default simple-case mappings are pinned; byte search-optimization parity remains a gate |
| Unicode script/scx properties | All175 scripts/344 normalized long, short and legacy aliases; bare names are scx, explicit sc/Script and scx/Script_Extensions, native primary-or-extension membership, byte projection and mixed predicates | `PcreScriptPropertyReferenceTests`, 3508 native boundaries and source-hash/native-fingerprint-checked regeneration; script-run groups remain separate |
| Unicode binary/Bidi properties | All57 binary predicates/106 aliases and23 Bidi short values, native bare bidi names, byte/UTF projection, complements and mixed predicates; UCP/i/r/ASCII independent | `PcreBinaryBidiPropertyReferenceTests`, 7706 native boundaries, source hashes/native fingerprint and exact C# regeneration; long Bidi values, Is/block names and boolean assignments rejected |
| Perl `(?[...])` extended character classes | Complement, intersection, union, subtraction, symmetric difference, grouping; escaped/property/POSIX/bracket operands, local space/tab grammar, operand case folding, native byte projection | `PcrePerlClassAlgebraReferenceTests`, fixed native JIT masks, bounded nesting/shared compilation and match work; opt-in UTS#18 activation is listed separately |
| UTS#18 extended character classes | Internal `AltExtendedClass` opt-in: nested brackets, implicit union, `|| && -- ~~`, whole-class complement, x/xx and quotes; mixed explicit operators require nesting | `PcreUtsClassAlgebraReferenceTests`, fixed native masks, shared algebra/resource contracts; ordinary default and Perl leaves preserved |
| Script-run groups | Four SR/ASR aliases, byte/UTF8, native extension/Han/digit rules and consumed-span validation; ASR body commits before validation | `PcreScriptRunReferenceTests`, native pair/boundary/digit/triple masks, recursion/journal/behind/invalid-UTF/work/state gates; native directional Han cases pinned |
| Atomic \X grapheme clusters | Byte/UTF8, pinned Unicode17/native rules, atomic clusters and whole-cluster repeats, RI prefix context and fragment/scope bounds | `PcreGraphemeReferenceTests`, `PcreGraphemeLookaheadScopeTests`; native table exceptions pinned; brackets invalid, consuming lookbehind unsupported |
| `* + ? {n,m}` greedy/lazy and possessive forms | Supported subset, bounded expansion and work; direct lookaround repeats use finite PCRE2 bounds (unlimited maximum becomes minimum plus one) | Counted-repeat, atomic and `PcreAssertionRepeatReferenceTests`; capture growth, optional rollback, zero counts, UTF offsets and resource limits; stress limits against consumer workloads |
| `(?>...)` atomic group | Supported subset, including exact lowercase `(*atomic:...)` | `PcreAtomicTests`, `PcreGroupAliasReferenceTests` |
| Bare decimal, absolute and signed relative numeric `\gN`/`\g{N}` backreferences | Default reference/octal disambiguation within 256 captures; braced g accepts edge space/tab. Nonrecursive subroutine forms are distinct calls, described below | Backreference/capture tests, `PcreNumericEscapeReferenceTests`; current-branch numbering, leading-8/9 forward references and entire decimal runs |
| Named captures and backreferences | Three declaration forms, five reference forms; unique names alias numbered slots, J permits duplicate names whose references select the first participating slot in declaration order without comparison fallback, forward references, case-sensitive names and a 128-byte name limit. UTF letters/digits use pinned Unicode17 categories. Nonrecursive subroutines are described below; bracket-literal `\g`/`\k` consume only the escaped letter and use ordinary class rules | `PcreNamedCaptureReferenceTests`, `PcreDuplicateNameReferenceTests`; all captures, rollback, UTF/invalid-UTF offsets, name grammar and boundaries; see `PCRE-NAMED-CAPTURES-DESIGN.md` and `PCRE-DUPLICATE-NAMES-DESIGN.md` |
| `(?|...)` branch-reset captures | Parser-scoped numbering, highest-branch continuation, same-number name aliases and ordinary numbered capture/journal semantics | `PcreBranchResetReferenceTests`; unequal widths, nesting, relative references, aliases, repeated captures, invalid-UTF offsets and resource boundaries |
| `(?=...)`, `(?!...)` atomic lookahead | General bodies over existing grammar, options, numbered/named captures, alternatives/repeats and nested assertions. Inline bytecode shares guards/journal; first success commits positive captures, negatives discard body state; original caller/fragment retained. Direct quantifiers use finite bounds with ordinary greedy/lazy/U and possessive choices; short/long atomic lookahead aliases use the same group path; non-atomic positive variants are listed below | `PcreGroupAliasReferenceTests`, `PcreGeneralLookaheadReferenceTests`; capture transactions, spans beyond G0, UTF offsets, 256 depth and guard-inclusive saved-path/work limits |
| `(?<=...)`, `(?<!...)` lookbehind | General fixed and bounded-variable branches over existing grammar, including classes/captures/groups/bounded repeats, R, nested assertions and local options. Heap min/max analysis, longest-first reverse retries, scoped JIT end limits and shared guards/journal preserve captures. Variable assertions cap all branch maxima at255; all-fixed branches retain65535. Bounded capture-body lengths for consumed numbered/named/relative/forward references resolve globally; lexical self/cycles, actual consumed duplicate names and global branch reset reject. Short/long atomic lookbehind aliases share the same width rules. Unlimited consumed groups remain pending | `PcreGroupAliasReferenceTests`, `PcreFixedLookbehindReferenceTests`, `PcreVariableLookbehindReferenceTests`, `PcreLookbehindReferenceLengthTests`, literal lookaround and explicit diagnostic controls; byte/UTF widths, capture transactions, nested visibility, malformed offsets, heap depth and resource limits |
| Non-atomic positive lookahead/lookbehind | Six symbolic/short/long spellings reuse general assertion lowering and capture journal; body/reverse choices remain retryable after success. Stable parent-linked scope IDs restore context/end caps; guard-linked LIFO storage stays bounded by1024 live saved frames. Existing atomic/negative pruning, direct repeat rules and consumed-width restrictions remain | `PcreNonAtomicAssertionReferenceTests`; native atomic controls, nested/sibling contexts, rollback, cap/reverse ordering, malformed UTF/CRLF offsets, arena reclamation, resource/cancellation boundaries |
| Capture-participation conditionals | Absolute/relative numbers and bare/angle/quoted names; global targets, J duplicate ANY-set tests and empty capture participation. One chosen branch with no cross-branch fallback; omitted else is runtime-empty but excluded from native behind width analysis. Exact headers, reserved-name precedence and existing group/repeat/scope limits | `PcreCaptureConditionReferenceTests`; forward/self/relative targets, rollback, atomic/non-atomic interactions, explicit/omitted widths, UTF offsets and resource/cancellation gates |
| Assertion conditionals | Atomic ahead/behind, positive/negative, symbolic and short/long aliases. Body-match captures survive both polarities; failure rolls back. One branch commitment, shared guard/journal/arena and explicit behind widths; non-atomic/repeated condition assertions invalid | `PcreAssertionConditionReferenceTests`; capture polarity, nested scope pruning/retries, options/numbering, K contexts, widths, UTF offsets and resource limits |
| Static DEFINE/VERSION conditions | DEFINE false/declarations with one branch; VERSION equality/at-least tuples against compatibility10.48, bounds1000, optional minor0. All-body validation/newline flags, selected branch execution, explicit VERSION widths and context-dependent DEFINE width suppression; false-one-branch repeats ignore execution but retain width/budget analysis | `PcreStaticConditionReferenceTests`; names/options/refs, unreachable captures, numeric/header errors, width contexts, CRLF/UTF offsets and resource gates |
| Leading/mid-pattern/scoped `i`, `m`, `n`, `s`, `U`, `J`, `r`, `x`/`xx`, `aD`/`aS`/`aW`/`aP`/`aT`/bare `a` | Compile-time branch propagation, local casing and group restoration. n suppresses unnamed slots; named groups still capture. U reverses ordinary/question-suffix repetition preference; possessive repeats stay greedy. x/xx ignore source trivia at token boundaries, with bracket-only SP/HT in xx; explicit x downgrades xx. Local ASCII restrictions select d/s/w predicates, aW word boundaries, aP all POSIX names and aT only POSIX digit/xdigit. aP/aT overlap uses native five-bit mask algebra; bare a sets all bits. Unicode properties and h/v remain independent. Local r blocks ASCII/non-ASCII fold crossings while preserving ordinary Unicode case pairs and explicit property semantics; UCP POSIX lower/upper retain Ll/Lu. Empty and reset headers, including empty hyphen sections, preserve U/J/ASCII restrictions while reset clears i/m/n/r/s/x/xx. J permits later duplicate declarations, restores at group boundaries and survives reset; references commit to the first participating declaration | `PcreOptionScopeReferenceTests`, `PcreCaptureRepeatOptionReferenceTests`, `PcreExtendedOptionReferenceTests`, `PcreResetOptionReferenceTests`, `PcreAsciiDigitOptionReferenceTests`, `PcreAsciiRestrictionReferenceTests`, `PcreCaselessRestrictionReferenceTests`, `PcreDuplicateNameReferenceTests`, `PcreLeadingOptionCombinationReferenceTests`; numbering, references, all quantifier families, UTF, limits/work and invalid-UTF/CRLF offsets |
| Ordinary braces and counted quantifier recognition | Complete count grammar before numeric validation; non-counted opening braces and closing braces are literal atoms. Malformed forms retain ordinary character-by-character lexing; all repeat boundaries agree; explicit braced tokens remain strict | `PcreBraceGrammarReferenceTests`; overflow-looking literals, real numeric errors, options/anchors/conditions, comments/quotes/classes, UTF widths/offsets and resource gates |
| `\Q...\E` literal quoting, isolated `\E` | Supported subset in ordinary patterns, ordinary brackets and fixed literal assertions; UTF-8 scalars and source-byte spans retained | `PcreQuotedLiteralTests`, pinned reference spans; see `PCRE-QUOTED-LITERALS-DESIGN.md` |
| Start directives `(*LF)`, `(*ANYCRLF)`, `(*BSR_UNICODE)`, `(*BSR_ANYCRLF)` | Exact consecutive byte0 prefix; independent last-wins override of caller newline/BSR settings, no source rewriting or runtime state | `PcreStartDirectiveReferenceTests`; see `PCRE-START-DIRECTIVES-DESIGN.md`; comments, global scopes, captures, byte offsets and work/fragment protocols |
| CR/NUL newline directives | Exact byte0 CR/NUL, last-wins with LF/ANYCRLF; dot/N, anchors and x/xx comments share single-character semantics; R/BSR remains independent | `PcreCrNulNewlineReferenceTests`; see `PCRE-CR-NUL-NEWLINES-DESIGN.md`; CRLF offsets, raw NUL source/subjects, scopes, validation and resource controls |
| CRLF-only newline directive | Exact byte0 CRLF, last-wins with other conventions; complete-pair dot/N/anchors/comments, native fixed/variable behind ends, byte-only bounded Any+literal prefix proof | `PcreCrLfNewlineReferenceTests`; see `PCRE-CRLF-NEWLINE-DESIGN.md`; lone/pair/caller offsets, all captures/widths, fragments and resource gates |
| ANY newline directive | Exact byte0 last-wins ANY; fixed vertical scalar list, UTF-aware anchors/comments, NUL ordinary and BSR independent; native consumed-CR midpoint anchors under ANY/ANYCRLF | `PcreAnyNewlineReferenceTests`; see `PCRE-ANY-NEWLINE-DESIGN.md`; continuation-byte controls, byte/UTF membership, spans/fragments/limits |
| Start directives `(*UTF)`, `(*UTF8)`, `(*UCP)` | Enable existing UTF/UCP modes in the shared uninterrupted prefix before whole-source UTF validation; caller defaults retained; byte UCP now uses the byte domain | `PcreUtfStartDirectiveReferenceTests`; see `PCRE-UTF-START-DIRECTIVES-DESIGN.md`; original-byte errors, strict/tolerant subjects, global modes and work/offset protocols |
| Start directives `(*NOTEMPTY)`, `(*NOTEMPTY_ATSTART)` | Global accumulating flags in the shared byte0 prefix; final acceptance rejects empty reported spans everywhere or only at the original caller offset, including K semantics; shared rollback/nullable fragment candidates | `PcreNotEmptyDirectiveReferenceTests`; see `PCRE-NOTEMPTY-DIRECTIVES-DESIGN.md`; same-candidate fallback, atomic/inner-success controls, UTF/CRLF offsets and resource gates |
| No-argument `(*THEN)` | Delayed cut retries the nearest lexical alternative; last branch permits outer retries; all assertions/callees bound ownership; conditionals and one-branch wrappers inherit their exterior owner | `PcreThenVerbReferenceTests`;149 native literal cases, selected UTF/CRLF offsets, mixed cuts/returned captures, shared1024 frames/256 marks and work/cancellation; named forms pending |
| No-argument `(*COMMIT)` | Delayed saved cut terminates the root search; shared call/negative/conditional assertion catches and selected callee values; bounded first-code-unit entry before leading COMMIT | `PcreCommitVerbReferenceTests`; native literal spans, rightmost mixed cuts, UTF malformed lead bytes/all selected offsets, shared frame/work/cancellation limits; named arguments pending |
| No-argument `(*SKIP)` | Shared PRUNE scope catches; strictly forward root restart at the verb byte position, equal/backward normal bumpalong; native direct LF/EOF candidate entry | `PcreSkipVerbReferenceTests`; literal JIT spans, UTF fragments, K/assertions/behind/calls, mixed cuts and shared1024 frames/work/cancellation; named arguments pending |
| No-argument `(*PRUNE)` | Saves a bounded backtracking cut; root resumes candidate search, calls/negative/conditional assertions catch, standalone positive assertions propagate; atomic success discards interior cuts | `PcrePruneVerbReferenceTests`; native selected pre-verb returned-capture edge, recursion/repeats/behind, UTF/CRLF offsets, 1024 saved-frame boundary and shared work/cancellation; named arguments pending |
| No-argument `(*ACCEPT)` | Immediate successful root/assertion/call termination, same-level capture closes, committed scope choices, ordinary greedy/lazy/possessive repeats; callee synthetic closes follow pinned native JIT behavior | `PcreAcceptVerbReferenceTests`; K/NOTEMPTY rollback, nested/conditional/non-atomic assertions, width tails, recursive/returned calls, script/atomic bypass, UTF fragments and shared limits; named arguments remain pending |
| No-argument `(*FAIL)` / `(*F)` | Unconditional zero-consumption failure through shared rollback; nonrepeatable directly, grouped repeats ordinary; direct failure ends consumed-width branch and suppresses nested behinds in its width tail, while global validation/full lexical metadata remain | `PcreFailureVerbReferenceTests`; see `PCRE-FAILURE-VERBS-DESIGN.md`; captures/scopes/widths, malformed offsets, fragments and resource bounds; arguments/other verbs pending |
| Ordinary bracket `\g`/`\k` | Literal g/k only; following names, delimiters and digits remain class source; outside reference/call grammar unchanged | `PcreBracketReferenceEscapeTests`; see `PCRE-BRACKET-REFERENCE-ESCAPES-DESIGN.md`; ranges/folding/quotes/widths/captures and original-byte/resource protocols |
| `\cX` control escapes | Printable ASCII operand20..7E, a..z uppercase then XOR40; byte/UTF literals, classes/ranges, assertions and widths share existing paths; source delimiters and x/xx trivia are consumed as operands | `PcreControlEscapeReferenceTests`; see `PCRE-CONTROL-ESCAPES-DESIGN.md`; original byte diagnostics, CR/LF metadata, quotes, folding, offsets and work/cancellation |
| Byte `\C` | Any one byte including NUL/newlines/high bytes; existing ALLANY lowering, width1 and byte prefix proof. Invalid in brackets; UTF C remains explicitly pending | `PcreByteCodeUnitReferenceTests`; see `PCRE-BYTE-CODE-UNIT-DESIGN.md`; offsets, captures, behind, modes and resource gates |
| Other escapes, including UTF code-unit escape | Pending, rejected; no literal reinterpretation | Representative compile-diagnostic assertions; prioritize by actual Grep patterns |
| `(?#...)` and x/xx line comments | Supported at token boundaries, including quantifier/suffix joins and fixed assertion bodies. Quotes/escapes and ordinary classes retain literal text; LF/ANYCRLF/CR/NUL/CRLF/ANY govern line termination | `PcreExtendedOptionReferenceTests`; original byte diagnostics and pinned whitespace/range rules |
| Recursive and nonrecursive subroutine calls | Numbered/named, forward/relative and g angle/quoted forms; first syntactic target, definition options, non-atomic retries and numbered capture restoration through bounded integer call guards/shared journal. Bounded consumed calls in lookbehind share the heap dependency width evaluator, including first duplicate-name/branch-reset definitions. Parenthesized calls accept nonempty absolute/relative/angle/quoted return lists, globally resolved and duplicate-expanded; selected numbered values survive return while starts/marks restore. Consumed calls with lists reject in behind width scans; ahead/DEFINE bypasses retain native behavior. Direct/mutual cycles and R/absolute-zero whole-root calls now share Call/Return and existing budgets; consumed recursive/whole-root widths and targets with suppressed unresolved behinds remain rejected | `PcreRecursiveCallReferenceTests`, `PcreReturnedCaptureReferenceTests`, `PcreSubroutineReferenceTests`, `PcreSubroutineLookbehindReferenceTests`; see `PCRE-RECURSIVE-CALLS-DESIGN.md`, `PCRE-RETURNED-CAPTURES-DESIGN.md`, `PCRE-SUBROUTINES-DESIGN.md` and `PCRE-SUBROUTINE-LOOKBEHIND-DESIGN.md` |
| Recursion conditions | R/R0 any active call, Rdigits current group, R&name duplicate ANY current target; innermost only, global bare-name capture precedence, shared conditional lowering and bounded integer call contexts | `PcreRecursionConditionReferenceTests`; see `PCRE-RECURSION-CONDITIONS-DESIGN.md` |
| Callouts | Pending, rejected | Representative compile-diagnostic assertions; evaluate consumer need |
| Other `(*...)` directives, newline settings, control verbs and other group forms | Pending, rejected at opening `(`; implemented atomic/assertion aliases are listed above; implemented start directives are listed above; other directives/verbs remain pending | Representative compile-diagnostic assertions; decide which directives Grep needs |
| Pattern-side malformed input, invalid UTF subjects and start offsets | Controlled diagnostics and finite conformance matrices, not exhaustive parity | Invalid UTF one/two/three-byte and capture/offset suites; expand error and resource fixtures |
| `-P -w/-x/-o/-z/-a`, color, locale, record framing, binary policy, exit status | Consumer-owned | Existing Grep tests; command-level migration and six-RID smoke after engine release |
| Replacement-string syntax | Outside matcher scope | Grep's `-P` selection does not substitute text |

## What this checkpoint does and does not close

The documented Grep cases have a supported matcher fixture or a named
consumer-owned behavior. That is a *finite observed consumer floor*. The
unknown pattern space accepted by the current PCRE2 runtime remains broader
than this internal engine. A release must say which unsupported syntax is a
deliberate product restriction and test the corresponding Grep diagnostic;
it cannot silently substitute a different engine or claim unrestricted
PCRE2 compatibility.

The [error/resource contract](PCRE-RESOURCE-ERROR-CONTRACT.md) now pins the
internal saved-path boundary and identifies Grep's remaining command-level
error and cancellation fixtures. Expand start-offset and resource parity
cases, then implement the highest-priority pending families against
the pinned reference. General atomic lookahead now has capture transactions;
the remaining gaps include additional Unicode modes, named/control verbs and other unsupported syntax.
Full workload measurements and command-level Grep comparisons follow before
public API/release review. No CommandFramework ownership change belongs to
this checkpoint.

## Byte class CRLF search follow-up

Byte class CRLF continuation now preserves negated source operand count/shape and literal/range endpoints, including the native negated-singleton exception. Historical positive interior-range flags remain until broader start-optimization coverage prevents lost matches; known false positives stay in #99. The bounded byte two-atom negated-singleton/all-byte class plus literal prefix preserves JIT LF candidate entry; optional single-branch capture/noncapture wrappers qualify. Broader prefix optimizer shapes remain tracked in #99. UTF behavior and all VM work/cancellation/resource paths remain shared. See `PCRE-BYTE-CRLF-DESIGN.md` and `PcreByteCrLfSearchReferenceTests`.

## UTF leading-class CRLF entry

Mandatory positive predicate-free ASCII bracket starts which exclude CR now retain native JIT LF entry, including all-qualified alternatives, capture/noncapture/atomic wrappers and required variable/exact1 repeats. At least one qualifying start class must admit LF, and root/worklist analysis is conservatively capped at1,000 visits to respect native start-bitmap exhaustion. Source newline metadata remains independent; all candidates keep existing VM work/cancellation and original fragment/capture protocols. General fixed/optional/assertion/call/Unicode starts remain in #102; byte #99/#101 behavior is unchanged. See `PCRE-UTF-CRLF-START-CLASS-DESIGN.md` and `PcreUtfCrLfStartClassReferenceTests`.

## Explicit properties in byte mode

`PcreBytePropertyReferenceTests` extends existing general categories and
Any/Xan/Xps/Xsp/Xwd/Xuc to U+0000..U+00FF byte values, including NUL and
non-UTF byte sequences. Compile-time projection of shared scalar membership
produces ordinary immutable byte classes; one-byte widths, existing work,
limits and rollback remain. Caseless Lu/Ll/Lt widen before complements.
No new VM state/opcode or UTF behavior. Byte UCP now also supports
literal/range/reference folding and shorthand/boundary/POSIX semantics.
See `PCRE-BYTE-PROPERTIES-DESIGN.md` and `PCRE-BYTE-PROPERTIES-PLAN.md`.

## Byte UCP

`PcreByteUcpReferenceTests` enables caller/prefix UCP without UTF: Unicode
shorthands/POSIX/boundaries over all256 bytes, local ASCII restrictions and shared
Unicode case folding for literals, ranges, references and fixed assertions.
Classification projects existing shared predicates; byte widths/source validation
and existing non-UCP/UTF paths remain distinct. Native source provenance retains
property/type prefix versus ASCII bitmap behavior. One immutable program setting;
no new saved state/opcode or limits. See `PCRE-BYTE-UCP-DESIGN.md` and
`PCRE-BYTE-UCP-PLAN.md`. Unicode-version/general optimization/public/release/
consumer gates remain.

## Byte code-unit escape

`PcreByteCodeUnitReferenceTests` pins byte `\C` as any one byte, independent of
DotAll/newline/BSR/UCP/ASCII/case modes. Existing Any lowering shares widths,
prefix selection, captures and work/resource paths. Bracket use is invalid;
UTF C remains an explicit separate gate with scalar/fragment invariants intact.
No new VM state/opcode or limit. See `PCRE-BYTE-CODE-UNIT-DESIGN.md` and
`PCRE-BYTE-CODE-UNIT-PLAN.md`. Public/release/consumer gates remain separate.

## Fixed byte prefix continuation

`PcreByteFixedPrefixReferenceTests` extends byte CRLF continuation through single-branch
ordinary/atomic wrappers and direct exact unrestricted Any repeats or separate Any
atoms: one through eleven byte positions, then exactly one literal inside native
JIT's twelve-position window. Existing restrictive PrefixAny classes remain a
one-position proof. Iterative cancellation-aware compiler analysis; no matcher,
capture state, limits or UTF changes. Repeated classes, variable/longer restrictive
prefixes and broader optimizer/provenance coverage remain #99; UTF search remains
#102. See `PCRE-BYTE-FIXED-PREFIX-DESIGN.md` and `PCRE-BYTE-FIXED-PREFIX-PLAN.md`.
No public/release/consumer change; frozen35 intact.

## Byte Any-property bracket prefixes

`PcreByteAnyPropertyPrefixReferenceTests` pins full-byte bitmap repeats built
with Any properties, including caret/class complements, mixed bitmap operands,
capture/group copies, local options, window boundaries and every caller offset.
AnyProperty source provenance separates old atom admission from actual XCLASS
requirements: only full byte bitmaps without real property data gain exact-repeat
admission. Empty classes retain prior atom/work behavior; direct properties keep
their existing repeat policy. Existing proof1000/window12, conditional selector,
byte membership, source flags, matcher, journals and limits remain shared.
Broader optimizer/metadata and mixed positive-Any/real-property native membership
discrepancies stay #99; UTF #102 and unsafe native #100 remain separate. See
`PCRE-BYTE-ANY-PROPERTY-PREFIX-DESIGN.md` and `PCRE-BYTE-ANY-PROPERTY-PREFIX-PLAN.md`.
No public/release/consumer change; frozen35 intact.


## Conditional byte prefix selection

`PcreBytePrefixSelectorReferenceTests` pins longer restrictive fixed byte prefixes:
native literal rejection at CR retains LF, while a passing prefix literal followed
by VM failure skips LF. Reuse the twelve-position/single-final-literal proof, with
optional immutable offset/value/local-case metadata and one shared Step/ScalarEquals
comparison only on eligible failed CR pairs. Old unconditional proofs retain work;
UTF remains separate. Direct exact Any repeats qualify; repeated class provenance,
ordinary bitmaps, variable/zero repeats and broader optimizer/source metadata stay
#99. See `PCRE-BYTE-PREFIX-SELECTOR-DESIGN.md` and `PCRE-BYTE-PREFIX-SELECTOR-PLAN.md`.
No new saved state/limits/public/release/consumer change; frozen35 intact.

## Repeated byte class prefix provenance

`PcreByteClassPrefixRepeatReferenceTests` distinguishes None/Atom/Exact source
encoding on existing byte classes. Single-source negation, property-free all-byte
bitmaps and direct property/type lowerings admit positive exact class repeats;
property brackets remain atom-only except native count1 elision. Equal byte sets
do not imply equal prefix scans. The existing bounded compiler proof and #117
conditional literal selector supply matching, captures and charged work; matcher
and VM state unchanged. Broader class/group/optimizer/source metadata stays #99;
UTF #102 and unsafe native #100 remain separate. See
`PCRE-BYTE-CLASS-PREFIX-REPEATS-DESIGN.md` and `PCRE-BYTE-CLASS-PREFIX-REPEATS-PLAN.md`.
No public/release/consumer or resource-limit changes; frozen35 intact.

## Repeated byte group prefix copies

`PcreByteGroupPrefixRepeatReferenceTests` pins fixed capture/noncapture/atomic
group copies in byte prefix scanning, including nested/multi-atom byte offsets
and last captures. The existing proof schedules at most eleven child copies,
charges cumulative proof visits and retains the twelve-position/sole-literal
boundary. Grouped property atoms stay distinct from directly repeated XCLASS.
Existing source kinds, bool/conditional selector, VM captures and charged work
remain shared; no parser/AST/matcher/state/limit change. Broader optimizer/source
metadata stays #99; UTF #102 and unsafe native #100 remain separate. See
`PCRE-BYTE-GROUP-PREFIX-REPEATS-DESIGN.md` and `PCRE-BYTE-GROUP-PREFIX-REPEATS-PLAN.md`.
No public/release/consumer change; frozen35 intact.

## Byte mixed-Any property membership

`PcreByteAnyPropertyMembershipReferenceTests` pins PCRE2 10.48's byte-only
bitmap inclusion when Any and real properties share a bracket. Actual property
data without an ordinary byte source selects the property-only union before
outer negation; a literal/range/nonempty quote/bitmap operand retains the full
ordinary union. Source order, UCP/ASCII/case/xx policies, all256 byte memberships,
full captures and caller offsets are covered. UTF scalar union, source flags,
class provenance, compiler proof, matcher/journals and limits remain unchanged.
This deliberately follows the pinned native behavior; revisit on reference
version changes. The named mixed-property discrepancy from #120 is addressed;
broader optimizer/positive-interior metadata remains #99, UTF #102 and unsafe
native #100 remain separate. See `PCRE-BYTE-ANY-PROPERTY-MEMBERSHIP-DESIGN.md`
and `PCRE-BYTE-ANY-PROPERTY-MEMBERSHIP-PLAN.md`. No public/release/consumer change.

## Broad byte bitmap repeat prefixes

`PcreByteBitmapPrefixRepeatReferenceTests` pins ordinary byte bitmap exact repeats
that expose a sole final literal. Bitmap source provenance stays excluded from
ordinary atom admission; direct exact min>=2 repeats qualify only with CR and LF
membership and at least six distinct bytes, matching native's broad scan count.
Count1/unquantified/group-copy bitmaps, sparse/CR-excluding sets and direct H/V
lowerings retain prior boundaries. Existing proof1000/window12, conditional
selector, captures, work/cancellation and limits remain shared. Byte membership,
source flags, matcher and journals are unchanged. Broader bitmap/source-metadata
and optimizer work stays #99; UTF #102 and unsafe native #100 remain separate.
See `PCRE-BYTE-BITMAP-PREFIX-REPEATS-DESIGN.md` and
`PCRE-BYTE-BITMAP-PREFIX-REPEATS-PLAN.md`. No public/release/consumer change.

## Byte-FF bitmap repeat prefixes

`PcreByteFfBitmapPrefixReferenceTests` pins native bitmap scan broad255 when the
effective byteFF bit is set, even for three/four/five-member sets. Existing Bitmap
provenance, direct exact min>=2, CR/LF, width1..11/window12, proof1000 and sole-final-
literal selector remain. The helper now accepts effective FF OR old popcount>=6;
ordinary/count1/group-copy and excluded-FF sparse controls retain prior boundaries.
Parser/AST/membership/source flags, matcher, journals and limits are unchanged.
Sparse withoutFF/metadata/general optimizer work remains #99; UTF #102 and unsafe
native #100 separate. See `PCRE-BYTE-FF-BITMAP-PREFIX-DESIGN.md` and
`PCRE-BYTE-FF-BITMAP-PREFIX-PLAN.md`. No public/release/consumer change.

## Homogeneous sparse byte head prefixes

`PcreByteSparseHeadPrefixReferenceTests` pins one direct exact sparse Bitmap repeat
as the complete consuming head before a sole final literal. Both CR/LF are required;
the CR prefix overlaps any successful LF prefix, so the shared selector preserves
results across native literal/pair/table choices. A compile-local bool rejects any
additional consuming prefix width after the admitted sparse repeat. Broad/FF mixed
paths retain existing behavior; count1/ordinary/copied bitmap atoms stay excluded.
No parser/AST/membership/source-flag/matcher/VM/limit changes. Two earlier eligible
sparse no-match budgets change16->22 for a paid comparison and failed LF attempt;
other expectations/frozen GNU tests remain unchanged. Mixed sparse and broader
optimizer/metadata remains #99; UTF #102 and unsafe native #100 separate. See
`PCRE-BYTE-SPARSE-HEAD-PREFIX-DESIGN.md` and `PCRE-BYTE-SPARSE-HEAD-PREFIX-PLAN.md`.
No public/release/consumer change.


## Wide UTF leading-class candidate entry

PcreUtfWideStartClassReferenceTests extends the existing positive predicate-free
UTF start-class proof to scalar ranges in0..0x10FFFF. Every leading class excludes
CR and at least one admits LF; bounded wrappers/branches/repeats and proof1000 stay.
No-LF classes retain prior skip/work. A direct root-leading wide class PLUS (also
lazy/possessive) needs native type_skip recovery: marked scalar instructions retain
one per-attempt integer high-water endpoint from already charged comparisons.
After a failed attempt, a CRLF at that endpoint is skipped as native bumpalong
requires. Recursive/assertion execution cannot update it; frames/journal unchanged.
Nested/alternative wide unbounded class PLUS contexts remain outside admission.
Membership/source flags/UTF validation/captures/Step and all resource ceilings stay
shared. All existing expectations/frozen35 unchanged. Broader optimizer #102, byte
#99 and unsafe native #100 remain separate. See PCRE-UTF-WIDE-START-CLASS-DESIGN.md
and PCRE-UTF-WIDE-START-CLASS-PLAN.md. No public/release/consumer change.


## ASCII UTF root-repeat recovery

PcreUtfAsciiRepeatRecoveryReferenceTests pins native CLASS as well as XCLASS
type_skip recovery for one direct first root positive predicate-free class PLUS,
including lazy/possessive and counted{1,}. Remove only the width filter from the
existing selector; retain root/first-node/min1/unbounded/CR-excluding/LF-admitting
and source-shape guards. Existing immutable markers and per-attempt integer
endpoints handle failed-run CRLF bumpalong without a matcher/Step/frame change.
Correct native misses or later winners replace earlier false-positive LF matches;
finite/min2/atomic/alternative/grouped-sequence and explicit-newline controls keep
their behavior. All5941 prior expectations and frozen35 remain unchanged; no
raised limits/new state. Broader102/byte99/unsafe100 remain separate. See
PCRE-UTF-ASCII-REPEAT-RECOVERY-DESIGN.md and PCRE-UTF-ASCII-REPEAT-RECOVERY-PLAN.md.
No public/release/consumer change.


## UTF noncapture group repeat recovery

PcreUtfGroupRepeatRecoveryReferenceTests extends the existing class PLUS selector
through up to four unquantified single-branch noncapturing groups. Native
detect_early_fail stops at the fifth group; ASCII depth5/6/200 controls retain LF
entry. Positive predicate-free ASCII/wide sets keep existing min1/unbounded/
CR-excluding/LF-admitting and possessive-versus-explicit-atomic guards.
Iterative descent checks cancellation and reuses leaf AST identity, immutable
instruction markers and the existing integer endpoint recovery; no matcher/Step/
capture/frame/journal/state/limit change. All6022 prior expectations/frozen35
unchanged. Wide beyond-depth4, capture/alternative/other iterator provenance and
broader102 remain pending; byte99/unsafe100 separate. See
PCRE-UTF-GROUP-REPEAT-RECOVERY-DESIGN.md and PCRE-UTF-GROUP-REPEAT-RECOVERY-PLAN.md.
No public/release/consumer change.


## UTF deep noncapture repeat entry

PcreUtfDeepRepeatEntryReferenceTests separates leading-class recognition from
native type_skip acceleration. A positive predicate-free class PLUS reached through
a single-branch nonempty chain of unquantified noncapturing groups can retain UTF
bitmap LF entry beyond four groups; only depths0..4 emit the existing repeat-skip
marker. Greedy/lazy/possessive and counted{1,} forms share terminal guards. Explicit
atomic, capture/alternative/quantified prefix contexts remain conservative.
Compile-only (Set, Skip) classification reuses leaf identity; no matcher, bytecode
record layout, saved state, Step, capture/frame/journal or resource-limit change.
The existing cumulative1000-node start-class proof and parser256 ceiling remain.
Depth4/5/6/34/200 literal spans, all caller offsets, six newlines, UCP, strict/
tolerant UTF fragments, full captures, exact/exhausted work and cancellation are
pinned to native10.48 JIT. No-LF default-budget controls retain their behavior.
All6108 baseline expectations/frozen35 unchanged. Broader102, byte99 and unsafe100
remain separate; no full PCRE/release/consumer claim. See
PCRE-UTF-DEEP-REPEAT-ENTRY-DESIGN.md and PCRE-UTF-DEEP-REPEAT-ENTRY-PLAN.md.


## UTF capture-wrapper repeat recovery

PcreUtfCaptureRepeatRecoveryReferenceTests extends leading-class Set/Skip
classification through ordinary capturing and noncapturing groups with one shared
depth count. Positive predicate-free CR-excluding/LF-admitting class PLUS retains
existing endpoint recovery through depth4; deeper recognized chains retain LF
entry without a repeat-skip marker. Greedy/lazy/possessive/count and named/local
case forms reuse existing byte captures and comparisons.
A bounded whole-root compiler check admits capture chains only without reference/
call/conditional/unknown dependencies, conservatively including zero-count/DEFINE
contexts. It charges1000 combined branch/node visits and bounds its heap queue,
checking cancellation; noncapture-only classification is unchanged. Broader native
capture dependency/source pruning and optimizer102 remain pending.
No parser/AST/membership/source flags/UTF validation/matcher/program/instruction
layout/Step/capture/frame/journal/runtime state or resource-ceiling change. All6187
baseline expectations/frozen35 unchanged. Byte99/unsafe100 separate; no full PCRE,
public/package/release/consumer claim. See
PCRE-UTF-CAPTURE-REPEAT-RECOVERY-DESIGN.md and PCRE-UTF-CAPTURE-REPEAT-RECOVERY-PLAN.md.


## UTF leading-capture dependencies

PcreUtfCaptureDependencyReferenceTests distinguishes prefix capture slots from
references/tests of later slots. The compiler collects traversed prefix numbers
and returns separate Eligible/Skip results from its bounded whole-root scan. A
related resolved backreference or capture-condition target clears Skip while
retaining class recognition; unrelated targets keep depth<=4 repeat acceleration.
All duplicate-name slots and syntactic condition/assertion bodies are observed.
Zero-count group bodies remain visible as native OP_SKIPZERO requires; direct
zero-count backreference repeats, calls, DEFINE/other conditions, unknowns and
over-bound analysis retain conservative baseline admission.
The existing1000 branch/node/queue proof and parser256/capture256 ceilings remain;
slot observations check cancellation. No parser/AST/membership/source metadata/
UTF validation/matcher/program/instruction layout/Step/capture/frame/journal/
runtime-state or raised-limit change. All6309 baseline expectations/frozen35
unchanged. Broader102/byte99/unsafe100 remain; no public/release/consumer claim. See
PCRE-UTF-CAPTURE-DEPENDENCIES-DESIGN.md and PCRE-UTF-CAPTURE-DEPENDENCIES-PLAN.md.


## UTF direct zero-count reference pruning

PcreUtfZeroReferencePruningReferenceTests pins native omission of a direct
Backreference repeat with maximum0. The existing capture-dependency classifier
skips that child without clearing Skip or rejecting class recognition. Retained
zero-count group/assertion/condition bodies still traverse normally; nested
direct zeros and later live references preserve their distinct native effects.
Numeric/relative/named/duplicate targets, lazy/possessive/count forms and shared
wrapper depth use the existing Set/Skip proof and endpoint recovery.
One compiler handler changes; no parser/AST/source flags/membership/UTF validation/
matcher/program/instruction layout/Step/capture/frame/journal/runtime-state or
resource-ceiling change. All6464 baseline expectations/frozen35 unchanged. Existing
compile1000/slots256/cancellation bounds remain. Leading-prefix pruning before
the selected PLUS, calls/DEFINE/other conditions and broader102/byte99/unsafe100
remain; no public/release/consumer claim. See
PCRE-UTF-ZERO-REFERENCE-PRUNING-DESIGN.md and PCRE-UTF-ZERO-REFERENCE-PRUNING-PLAN.md.


## UTF entry through pruned reference prefixes

PcreUtfPrunedPrefixReferenceTests pins native first-class entry after direct
zero-count references before a leading PLUS disappear. A shared compiler helper
skips direct Repeat0Backreference or possessive AtomicRepeat0Backreference,
charging2/3 omitted AST nodes and checking CT within1000 visits. It returns the
first retained node or no node for exhausted/all-omitted bodies.
Repeat selection uses it at root/group bodies; total group depth and capture-slot
classification remain distinct. The start-class proof skips omitted prefixes only
when the selector already recognized a repeat leaf. Other branch/prefix forms keep
conservative admission; nonpruned proof-node charging is unchanged.
No parser/AST/source flags/membership/UTF validation/matcher/program/instruction
layout/Step/capture/frame/journal/runtime-state/raised-limit change. All6567
baseline expectations/frozen35 unchanged. Retained/nullable/alternative/other
pruned prefixes, calls/DEFINE/other conditions and broader102/byte99/unsafe100
remain; no public/release/consumer claim. See
PCRE-UTF-PRUNED-REFERENCE-PREFIX-DESIGN.md and PCRE-UTF-PRUNED-REFERENCE-PREFIX-PLAN.md.


## UTF entry after retained zero-count prefixes

PcreUtfRetainedZeroPrefixReferenceTests separates bitmap entry from repeat
acceleration after native SKIPZERO brackets. The shared helper returns Node and
BlocksSkip; supported zero group/assertion/atomic/capture-condition prefixes can
be crossed for the first class proof while permanently blocking repeat skipping
along that prefix. Truly omitted references keep prior behavior; trailing retained
brackets do not disable earlier acceleration. Capture slots/dependencies remain
global and retain unset-prefix capture spans.
Selector barriers accumulate across root/group bodies and are ANDed with depth
and capture eligibility. Proof uses only Node under the existing selected-leaf
guard. Cost2/3 plus possessive wrapper1, CT and1000 proof/classification bounds
remain. No parser/AST/source flags/membership/UTF validation/matcher/program/
instruction layout/Step/capture/frame/journal/runtime-state/raised-limit change.
All6668 baseline expectations/frozen35 unchanged. Other nullable/alternative/
pruned starts, calls/DEFINE/other conditions and broader102/byte99/unsafe100 remain;
no public/release/consumer claim. See
PCRE-UTF-RETAINED-ZERO-PREFIX-DESIGN.md and PCRE-UTF-RETAINED-ZERO-PREFIX-PLAN.md.


## UTF entry through pruned atom prefixes

PcreUtfPrunedAtomPrefixReferenceTests pins native entry and acceleration after
direct zero-count literals, character types, classes and newline sequences.
The shared first-node helper treats supported UTF primitive repeats as omitted,
with the same cost-two/nonretained result as direct reference0; an outer
possessive wrapper adds one visit. Retained zero brackets still block repeat
skipping, including brackets containing pruned atoms.
Source newline metadata survives omission: a removed newline atom still changes
candidate behavior. AST/source flags are unchanged. Root/group traversal, capture
dependencies, selected-leaf proof, depth and cancellation/1000-visit bounds remain
shared. No matcher/program/instruction layout/Step/capture/frame/journal/runtime
state/raised-limit changes. All6789 baseline expectations/frozen35 unchanged.
General nullable/alternative/other pruned starts, calls/returned captures, DEFINE/
other conditions and broader102 optimizer provenance remain; byte99 and unsafe100
are separate. No public/release/consumer readiness claim. See
PCRE-UTF-PRUNED-ATOM-PREFIX-DESIGN.md and PCRE-UTF-PRUNED-ATOM-PREFIX-PLAN.md.

The original optional-literal control exposed a further bounded source shape:
QUERY adds its literal start bit but raises early-fail count to3, preventing
type_skip. A direct Minimum0/Maximum1 Literal-not-CR (including its possessive
wrapper) now crosses the same helper with cost2/3 and BlocksSkip=true. Optional
input consumption remains in the unchanged VM; source newline metadata remains
intact. PcreUtfOptionalLiteralPrefixReferenceTests separately pins native spans,
consumed prefixes, modifiers, source CR/LF, depth/dependencies/retained and trailing
controls plus UTF/work/cancellation protocols. Original241 tests are unchanged.
Optional classes/types/groups, larger finite bounds and general nullable forms
remain separate; this does not claim general optional-prefix support.


## UTF entry through optional positive sets

PcreUtfOptionalSetPrefixReferenceTests pins bitmap entry after optional positive
predicate-free sets. Native class study adds their start bits and continues;
CRQUERY raises early-fail count to3, preventing repeat skipping. The shared
first-node helper crosses only min0/max1 ScalarSet nodes passing
IsUtfCrLfOptionalSet: nonempty/nonnegated/no predicates, valid scalar intervals
and no CR member. Each range charges the shared cancellation/1000-visit budget;
repeat/child cost2 and possessive wrapper1 remain. No scalar expansion occurs.
Optional LF consumption and full captures stay in the existing VM. LF is not
required in the optional set; the mandatory selected class still establishes it.
Root/group traversal, accumulated barriers, capture dependencies and source flags
remain unchanged. CR-admitting/negated/predicate/unknown/over-bound/all-optional
forms stay conservative. No matcher/runtime layout/storage/Step/frame/journal/
raised-limit/public/release/consumer changes. All7136 baseline expectations and
frozen35 source blobs unchanged. Broader102 optimizer/nullable/call/DEFINE/
condition forms remain; byte99 and unsafe100 separate. See
PCRE-UTF-OPTIONAL-SET-PREFIX-DESIGN.md and PCRE-UTF-OPTIONAL-SET-PREFIX-PLAN.md.

Author review added PcreUtfOptionalSetCaseFoldReferenceTests before a proof
correction. A caseless set with one canonical folded key may have originated
from native PT_CLIST property-query bytecode, which has different bitmap
availability. Reject that ambiguous shape after bounded range validation;
retain multi-key caseless admission. Original196 tests are unchanged. Ordinary
caseless singletons and different source spellings of one folded key retain
their preexisting conservative path pending source provenance; no case-fold
membership or source metadata is changed.


## Original UTF set source shape before folding

PcreUtfSetSourceShapeReferenceTests pins the distinction between original class
operands and canonical case-fold keys. PcreByteScalarSet now carries internal
init-only Source provenance (Unknown/Literal/Class), default Unknown. The UTF
bracket parser records Literal for one original literal/equal-end range and
Class otherwise, using existing sourceTermCount/soleLiteral before case folding.
Constructor parameters, membership, case closure and ExplicitNewline are intact;
record with-copies and reference-based tree rewrites preserve the provenance.
The existing optional-set proof admits proven Class sources after its positive/
nonempty/predicate-free/scalar-bound/order/CR/cancellation/1000-visit checks, even
when caseless folding leaves one key. Genuine Literal/Unknown one-key sources
stay conservative. Optional input consumption/captures and the Skip barrier
remain shared. Metadata is compile-time only, not emitted into runtime state.
No matcher/program/instruction layout/Step/frame/journal/limit/public/release/
consumer changes. All7359 baseline expectations/frozen35 unchanged. Genuine
singleton classification and broader102 source/nullable/call/DEFINE/condition
forms remain; byte99/unsafe100 separate. See
PCRE-UTF-SET-SOURCE-SHAPE-DESIGN.md and PCRE-UTF-SET-SOURCE-SHAPE-PLAN.md.

The original caseless wide-set no-LF budget fact also exposed unnecessary
interval scanning (already RED before provenance implementation).
PcreUtfCaseFoldRangeWorkTests pins the correction: after the existing Step
charges an interval probe, ScalarSet matching stops if Caseless and the folded
scalar is below that sorted interval. Raw noncaseless ranges retain source-order
search; predicates and negation remain after the range loop. This small matcher
early stop consumes work only for inspected intervals; no Step-function, limit,
program/frame/journal/layout/storage changes. Original198 tests remain unchanged;
observed/unobserved exact/exhausted work and the default budget are verified.

## UTF singleton literal query availability

PcreUtfLiteralCaseQueryReferenceTests distinguishes ordinary optional literal
queries from native caseless PT_CLIST property queries. ScalarSet retains internal
init-only SourceLiteral (default -1) before folding; constructor parameters,
Source provenance, case closure, membership and ExplicitNewline remain intact.
The compiler shares a private bounded PCRE2 10.48 default UCD_CASESET classifier
between plain optional literals and proven Literal bracket sources. All84 default
members remain property queries unless restricted casing suppresses an ASCII-
crossing K/S caseset; Turkish-extra pairs are excluded from this default table.
This is source opcode classification, not a replacement Unicode membership table.
Proven Class sources retain #136 behavior; Unknown one-key sources stay conservative.
Existing positive/nonempty/predicate-free/scalar/order/CR/cancellation/1000-visit
checks, cost2 plus wrapper1, depth/capture eligibility and BlocksSkip stay intact.
No matcher, runtime metadata/layout, Step, frame/journal, limit, public API,
workflow, package, dependency or consumer change. All7565 baseline expectations
and frozen35 remain unchanged. Native15292 checks pin382 new cases/full7947,
covering every84 default member as plain/bracket, ordinary/restricted/mixed/
consumed/deep/capture/modifier controls, six newline modes, UCP, every caller
offset, strict/tolerant fragments, work/exhaustion/cancellation/default budget
and parser257. See PCRE-UTF-LITERAL-CASE-QUERY-DESIGN.md and its PLAN.md.
Broader102 nullable/type/group/negated/predicate/call/DEFINE/condition forms and
native-complete availability remain; byte99/unsafe100 stay separate. This does
not qualify a PCRE release or Grep migration; CommandFramework remains untouched.

## UTF optional type-query prefixes

PcreUtfOptionalTypePrefixReferenceTests pins direct min0/max1 ASCII digit/word/
nonspace and horizontal-space queries. The shared leading-node compiler switch
admits positive AsciiDigit/AsciiWord/HorizontalSpace and negated AsciiSpace as
cost2/retained true. Existing parsed Kind/Negated carries UCP and scoped aD/aW/aS
restrictions; Unicode digit/word/nonspace properties remain excluded, while
horizontal space qualifies under either UCP setting. Every admitted type excludes
CR and has native bitmap study; NOT_VSPACE/ANY/property/CR-consuming/other types
remain conservative. Existing wrapper1/shared1000-visit/cancellation/depth/capture
checks and BlocksSkip apply. No parser/membership/folding/matcher/Step/runtime/
layout/storage/limit/public/package/workflow/dependency/consumer changes.
429 new cases/full8376 preserve all7947 baseline expectations/frozen35. Native
16188 bounded JIT checks (828 full-capture+15360 offset protocols) cover UCP/local
ASCII/caseless restrictions, optional modifiers/consumption, deep/capture/
dependency/mixed and conservative controls, six newlines/every caller offset/
strict-tolerant UTF, exact/exhausted/observed work/cancellation/default budget/
parser257. Larger optional bounds have consumption-only controls; their bitmap
availability remains outside this change. See PCRE-UTF-OPTIONAL-TYPE-PREFIX-DESIGN.md
and PCRE-UTF-OPTIONAL-TYPE-PREFIX-PLAN.md. Keep102 open for bracket predicates,
larger optional bounds, groups/alternatives/negated/property/call/DEFINE/condition
availability; byte99/unsafe100 separate. No complete PCRE/release/Grep-readiness
claim. CommandFramework remains untouched until publication/all known migrations.

## UTF finite optional atom prefixes

PcreUtfFiniteOptionalPrefixReferenceTests extends the already supported optional
literal/set/type source forms to positive finite maxima. The four existing
leading-node cases keep Minimum0 and require Maximum>0 and<int.MaxValue,
reusing literal caseset/restricted-casing classification, original set provenance/
CR/range/predicate/CT/1000-visit guards and the four type Kind/Negated cases.
Native UPTO/TYPEUPTO/nonexact CRRANGE use the relevant query bitmap rule and
raise early-fail count3; existing cost2/wrapper1/retained/BlocksSkip/depth/capture
checks remain shared. Max0 omission, required and unbounded cases are unchanged.
No parser/membership/folding/matcher/Step/instruction/frame/journal/layout/
storage/limit/public/package/workflow/dependency/consumer changes. Bounded
repeats still expand under the existing4096 instruction/visit limits; native65535
bounds are not a promise that every managed pattern of that size compiles.
617 new cases/full8993 preserve all8376 baseline expectations/frozen35. Native
19772 bounded JIT checks (1196 fullcapture+18576 offset protocols) cover finite
bounds/modifiers/source forms/UCP/casing, consumed/deep/capture/dependency/mixed/
excluded controls, six newlines/every offset/strict-tolerant UTF, work/exhaustion/
cancellation/default budget/parser257 and count/expansion rejection. Unbounded
controls pin ordinary consumption only; broader star availability remains open.
See PCRE-UTF-FINITE-OPTIONAL-PREFIX-DESIGN.md and its PLAN.md. Keep102 open for
bracket predicates, unbounded/groups/alternatives/negated/property/call/DEFINE/
condition availability. Byte99/unsafe100 separate; no complete PCRE/release/Grep
readiness claim. CommandFramework untouched until publication/all known migrations.

## UTF star-prefix entry and consumed-run recovery

PcreUtfStarPrefixReferenceTests pins supported Literal/ScalarSet/type stars at
root branch start or after a locally proven retained optional prefix. The compiler
keeps a compile-time root-first star child identity, clears it when following
mandatory set-PLUS selection fails, and gates unbounded admission through that
identity or local BlocksSkip. Finite classifiers/cost2/wrapper1/CT/1000-visit/
source/CR/property/UCP/capture/depth guards remain shared. A root star owns native
type_skip; queries/retained barriers prevent a later star from owning that cache.
The existing SkipCrLfAfterRepeat instruction bit marks only the root star's atom,
or the already supported leading PLUS. Literal/ScalarClass success now records
max(utfRepeatEnd,position) after the charged comparison under the same no-call/
no-assertion guard already used for ScalarSet. Existing per-attempt reset and
failure recovery skip LF only after a consumed marked run ending at CR; empty
prefix runs retain bitmap LF entry. ExplicitNewline/fragment/caller/capture/
backtracking/Step/depth/work/cancellation policies remain intact. No new runtime
layout/state/frame/journal/span/pointer/limit/API/package/workflow/dependency or
consumer change. Parser/membership/folding and CF remain untouched.
919 new cases/full9912 preserve all8993 baseline expectations/frozen35. Native
22916 bounded JIT checks (1796 fullcapture+21120 newline/UCP/offset protocols)
cover consumed/empty runs, first-cache ownership/barriers/mixed stars/modifiers/
source/casing/UCP, root stars before deeper/captured/dependent sets, explicit
newline/excluded controls, strict-tolerant UTF, exact/exhausted/observed work,
cancellation/default budget/parser257/backtrack depth rejection. See
PCRE-UTF-STAR-PREFIX-DESIGN.md and PCRE-UTF-STAR-PREFIX-PLAN.md. First stars inside
wrappers/after erased prefixes remain conservative; their controls pin ordinary
consumption only. Keep102 open for further placement, bracket predicates/group/
alternative/negated/property/call/DEFINE/condition availability; byte99/unsafe100
separate. No complete PCRE/release/Grep-readiness claim; publication/all known
consumer migrations precede any CF removal.

The original atomic-following-star fixtures require bitmap traversal through
Atomic(NoncapturingGroup(body)) only behind a proven root-star barrier. That
bounded walk retains group/visit/capture checks and assigns no later repeat skip
marker; pre-existing unguarded atomic contexts remain conservative.

## UTF star ownership after erased primitives

PcreUtfErasedStarPrefixReferenceTests pins the first surviving star after direct
Max0 primitives/references. IsUtfCrLfErasedRepeat shares the existing six-family
Backreference/Literal/Any/ScalarClass/ScalarSet/NewlineSequence vocabulary between
leading-node cost2/retained-false traversal and root-star discovery. Discovery
checks cancellation per node, charges root1+2 per erased operand+possessive
wrapper1 under the same1000-visit bound, and stops at the first other node.
Retained groups/assertions/conditionals and queries remain ownership barriers;
first stars inside wrappers remain separate proof work. The star still reuses
all source/casing/UCP/CR/predicate/range/capture/depth guards, existing instruction
marker and consumed-run endpoint/reset/recovery. Source ExplicitNewline and its
negated-singleton exception survive erasure. No matcher/parser/membership/folding/
Step/runtime layout/state/frame/journal/span/pointer/limit/API/package/workflow/
dependency/consumer or CF changes.
885 new cases/full10797 preserve all9912 baseline expectations/frozen35. Native
18624 bounded JIT checks (1728capture+16896newline/UCP/offset) cover six erased
families/aliases/sequences through128, modifiers/source/UCP/casing, empty and
consumed runs, newline flags, following capture/dependency/deep/atomic groups,
retained/query barriers, strict-tolerant UTF/work/cancellation/default budget/
parser257/backtrack-depth limits. See PCRE-UTF-ERASED-STAR-PREFIX-DESIGN.md and
PCRE-UTF-ERASED-STAR-PREFIX-PLAN.md. Wrapper-first-star controls pin ordinary
consumption only. Keep102 open for wrappers/other placement/bracket predicates/
group/alternative/negated/property/call/DEFINE/condition availability; byte99/
unsafe100 separate. No complete PCRE/release/Grep-readiness claim; publication/
all known migrations precede any CF removal.

## UTF star prefixes inside noncapturing wrappers

PcreUtfWrappedStarPrefixReferenceTests separates first-star bitmap recognition
from consumed-run recovery. Bounded discovery iteratively descends nonempty
single-branch noncapturing ancestors and existing erased operands, returning
compile-time Atom/Skip. It charges root1/group1/erased2+possessive wrapper1 with
CT/shared1000-visit checks. Skip is true only through four star ancestors;
recognition remains for supported deeper chains. Following captures/deep groups
do not add to that star depth. Capturing/explicit atomic ancestors, alternatives,
retained/query/unsupported nodes stay separate; atomic unwrapping requires a
Repeat child so explicit Atomic(NoncapturingGroup) is not admitted accidentally.
Compiler stores/clears both fields on final mandatory-set proof failure and gates
only the star endpoint marker by Skip. Existing leading-PLUS marker/admission/
source/casing/UCP/CR/predicate/range/capture/depth/BlocksSkip/ExplicitNewline/
fragment/resource guards remain intact. No matcher/Step/runtime layout/state/
frame/journal/span/pointer/limit/API/package/workflow/dependency/consumer/CF changes.
921 new cases/full11718 preserve all10797 baseline expectations/frozen35. Native
18696 bounded JIT checks (1800capture+16896newline/UCP/offset) pin depths1/3/4/5/6/
33/199, empty/consumed runs and the4/5 cache boundary, star source/modifiers/UCP/
casing, erasures across levels, later capture/dependency/deep/atomic groups,
explicit newline/unsupported controls, strict-tolerant UTF/work/cancellation/
default budget/parser257/depth limits. See PCRE-UTF-WRAPPED-STAR-PREFIX-DESIGN.md
and PCRE-UTF-WRAPPED-STAR-PREFIX-PLAN.md. Unsupported ancestor/nullable-prefix
wrapper controls pin ordinary consumption only. Keep102 open for capturing/
atomic ancestors, other placements, bracket predicates/group/alternative/negated/
property/call/DEFINE/condition availability; byte99/unsafe100 separate. No complete
PCRE/release/Grep readiness claim; CF removal requires publication/all migrations.

## UTF star prefixes inside capturing ancestors

PcreUtfCapturedStarPrefixReferenceTests pins native bitmap entry independently
from capture-dependent consumed-run recovery. Existing bounded iterative first-star
discovery now descends nonempty single-branch capturing as well as noncapturing
ancestors. Both charge group1/count toward the same ancestor-depth4 boundary.
Only capture numbers before the star enter a compile-time HashSet; existing
ClassifyUtfCaptureRepeat determines eligibility and disables Skip for related
references/tests while retaining identity. Direct reference0 remains erased;
retained zero-group references remain dependencies. Named/duplicate references
and capture tests use existing resolved numbers. Unrelated/following captures
do not disable the earlier star's cache. Explicit atomic ancestors, alternatives
and unsupported placements remain separate; possessive-repeat unwrapping and
final mandatory-set identity/Skip reset stay intact. Existing source/caseset/r/
UCP/CR/predicate/range/capture/BlocksSkip/ExplicitNewline/fragment/resource guards
and leading-PLUS marker remain. No matcher/parser/Step/runtime state/layout/
instruction/program/frame/journal/snapshot/span/pointer/limit/API/package/version/
workflow/dependency/consumer/CommandFramework changes.
1080 new cases/full12798 preserve all11718 baseline expectations/frozen35. Native
19008 bounded JIT checks (2112fullcapture+16896newline/UCP/offset) pin depths1/3/4/
5/6/33/199, related/unrelated/named/duplicate references/tests, erased-vs-retained
dependencies, mixed ancestors, star source/modifiers/casing/UCP, erased prefixes,
suffix capture/deep/atomic groups, newline flags/unsupported controls and strict/
tolerant UTF/work/cancellation/default budget/parser257/captures257/depth limits.
See PCRE-UTF-CAPTURED-STAR-PREFIX-DESIGN.md and PCRE-UTF-CAPTURED-STAR-PREFIX-PLAN.md.
Keep102 open for atomic ancestors/other placements/bracket predicates/group/
alternative/negated/property/call/DEFINE/condition availability; byte99/unsafe100
separate. No complete PCRE/release/Grep-readiness claim; publication/all known
migrations precede CF removal. Unsupported placement controls pin ordinary consumption.

## UTF star prefixes inside explicit atomic ancestors

PcreUtfAtomicStarPrefixReferenceTests pins native bitmap entry through explicit
atomic ancestors without assigning consumed-star endpoint recovery. Bounded
first-star discovery now accepts Atomic(NoncapturingGroup), charges an extra1
before its existing group1/root1/erased2+possessive-wrapper1 proof costs, and
disables local Skip after any atomic ancestor. Nonempty/single-branch/CT/1000/
capture-classification guards remain. Possessive Atomic(Repeat) retains iterator
rules; outside root stars retain recovery when only their suffix is atomic.
Mandatory-set discovery permits atomic descent only with proven first-star
identity and disables later PLUS Skip during that descent. Existing post-star
atomic traversal costs, final failed-proof identity/Skip reset, source/casing/r/
UCP/CR/predicate/range/capture/BlocksSkip/ExplicitNewline/fragment/resource guards
remain. No matcher/parser/Step/emitter/runtime state/layout/instruction/frame/
journal/span/pointer/snapshot/API/limit/workflow/package/version/dependency/
consumer/CommandFramework changes.
1130 new cases/full13928 preserve all12798 baseline expectations/frozen35. Native
19108 bounded JIT checks (2212strict-tolerant fullcapture+16896newline/UCP/offset)
pin depths1/3/4/5/6/33/199, short/long atomic aliases, mixed ancestors, source/
modifiers/casing/UCP, erased prefixes, related/named/duplicate capture references/
tests, later groups/outside-root cache controls, newline/unsupported controls,
strict-tolerant UTF/work/cancellation/default budget/parser257/captures257/depth.
See PCRE-UTF-ATOMIC-STAR-PREFIX-DESIGN.md and PCRE-UTF-ATOMIC-STAR-PREFIX-PLAN.md.
Keep102 open for other placements/bracket predicates/group/alternative/negated/
property/call/DEFINE/condition bitmap availability; byte99/unsafe100 separate.
Bare atomic PLUS/unsupported placements pin ordinary consumption only. No complete
PCRE/release/Grep-readiness claim; publication/all known migrations precede CF removal.

## UTF atomic mandatory repeats without a star prerequisite

PcreUtfAtomicRepeatEntryReferenceTests pins native mandatory-set bitmap entry
through explicit atomic groups without requiring first-star ownership. Existing
Atomic(NoncapturingGroup) descent now shares ordinary group eligibility; its
local Skip remains unconditionally false. Final single-branch/nonempty/positive
ScalarSet/mandatory unbounded PLUS/LF/no CR/no predicates/capture/depth/CT/1000
guards and optional-source/range/casing/UCP/BlocksSkip rules remain. First-star
discovery, outside-root/non-atomic PLUS recovery, possessive modifier handling
and failed-proof reset are unchanged. No matcher/parser/emitter/Step/runtime
state/layout/instruction/frame/journal/span/pointer/snapshot/API/limit/workflow/
package/version/dependency/consumer/CommandFramework changes.
970 new cases/full14898 preserve all13928 baseline expectations/frozen35. Native
18788 bounded JIT checks (1892strict-tolerant fullcapture+16896newline/UCP/offset)
pin bare/captured PLUS, greedy/lazy/possessive/aliases, depths1/3/4/5/6/33/199,
atomic aliases/mixed ancestors, optional/erased/retained-zero prefixes, capture
dependencies/named/duplicate references/tests, source/casing/UCP/CR/predicate and
outside-root cache controls, strict-tolerant fragments/work/cancellation/default
budget/parser257/captures257/backtrack limits. See
PCRE-UTF-ATOMIC-REPEAT-ENTRY-DESIGN.md and PCRE-UTF-ATOMIC-REPEAT-ENTRY-PLAN.md.
Keep102 open for nullable prefix-group rejoin/other placements and broader
predicate/group/alternative/negated/property/call/DEFINE/condition bitmap availability.
Nullable prefix-only wrapper controls pin ordinary consumption only; initial
native probes confirm their continuation/rejoin gap needs a separate traversal
refactor. Byte99/unsafe100 separate; no complete PCRE/release/Grep readiness claim.
Publication/all known migrations precede CF removal; Grep last.

## UTF continuation after nullable prefix groups

PcreUtfNullableGroupEntryReferenceTests pins bitmap continuation from a fully
nullable prefix group into its enclosing sequence. Existing successful selector
paths remain; only failed proofs with proven first-star identity try a bounded
iterative fallback. Its compile-time stack stores AST branches/next indexes,
never VM frames or subject spans. Group descent requires one nonempty branch;
resume occurs only after existing optional-atom classifiers accept every operand
as nullable. Retained state carries across returns; at least one actual group
return and existing capture classification eligibility are required.
FindUtfCrLfLeadingNode now reports NextIndex/Nullable and accepts defaulted
startIndex/blocksSkip; budget failure differs from nullable success. Shared
mandatory-set validation requires unbounded positive ScalarSet PLUS, nonempty
valid scalar ranges, LF/no CR/no predicates. A compile-only joined-proof flag
lets start-class discovery consume that completed proof; generic traversal stays
for other paths. First-star depth/capture/atomic recovery and all VM instructions
remain; fallback PLUS Skip is explicitly false. CT/1000/source/casing/UCP/range/
capture/fragment/resource guards and failed-proof identity reset remain.
No matcher/parser/emitter/Step/runtime state/layout/instruction/frame/journal/
snapshot/span/pointer/API/limit/workflow/package/dependency/consumer/CF changes.
1408 new cases/full16306 preserve all14898 baseline expectations/frozen35. Native
19664 bounded JIT checks (2768strict-tolerant fullcapture+16896newline/UCP/offset)
pin group types/depths1/3/4/5/6/33/199, empty participating capture spans, nested/
sequential returns, source/modifiers/UCP/casing/erasures/dependencies, required/
CR-admitting/unsupported stops and strict-tolerant UTF/work/cancellation/default
budget/parser257/captures257/backtrack rejection. See
PCRE-UTF-NULLABLE-GROUP-ENTRY-DESIGN.md and PCRE-UTF-NULLABLE-GROUP-ENTRY-PLAN.md.
Keep102 open for unproven-first-star placements and broader predicate/group/
alternative/negated/property/call/DEFINE/condition bitmap availability; byte99/
unsafe100 separate. No complete PCRE/release/Grep readiness claim. Publication/
all known migrations precede CF removal; Grep last.

## UTF finite and empty nullable-prefix continuation

PcreUtfNullablePrefixEntryReferenceTests extends the bounded continuation proof
from #146 to finite optional, omitted-only and empty prefix groups. After the
old successful selector fails, the existing fallback runs only in UTF mode,
without requiring a proven first-star identity. Empty single-branch bodies are
accepted only there. A completed group return sets retained BlocksSkip even
when every body operand vanished, matching native early-fail group counting.
Later supported stars can join the bitmap proof without receiving new recovery
ownership. Existing first-star finder/depth/capture/atomic/cache rules and old
successful proofs remain unchanged; fallback PLUS Skip remains false.
Shared nullable-atom classification, mandatory positive unbounded LF/no-CR set
validation, actual group-return requirement, single-branch/capture eligibility,
CT/1000/source/casing/UCP/range/predicate guards and failed-proof reset remain.
No matcher/parser/emitter/Step/runtime state/layout/instruction/frame/journal/
snapshot/span/pointer/API/limit/workflow/package/dependency/consumer/CF changes.
950 new cases/full17256 preserve all16306 baseline expectations/frozen35. Native
18748 bounded JIT checks (1852strict-tolerant fullcapture+16896newline/UCP/offset)
pin finite bounds/modifiers, erased-only/empty aliases, group types/depths,
empty participating captures, sequential/nested returns, later stars, capture
dependencies, source/casing/UCP/CR/predicate/required/unsupported/final-set stops,
previous first-star ownership and strict/tolerant fragments/work/cancellation/
default budget/parser257/captures257/backtrack rejection. Tests-only run478
established823new failures/16433passes/0skips (all16306baseline+127controls),
zero build warnings/errors; expectations remain unchanged afterward. See
PCRE-UTF-NULLABLE-PREFIX-ENTRY-DESIGN.md and PCRE-UTF-NULLABLE-PREFIX-ENTRY-PLAN.md.
Keep102 open for broader predicate/group/alternative/negated/property/call/
DEFINE/condition bitmap availability and remaining placements. Byte99/unsafe100
separate; no complete PCRE/release/Grep readiness claim. Publication/all known
migrations precede CF removal; Grep last.

## UTF continuation after fully nullable alternatives

PcreUtfNullableAlternativeEntryReferenceTests extends the existing UTF fallback
to prefix groups whose every alternative passes the shared nullable-atom proof.
Compile-time continuation tuples now include group Body/NextBranch. Only after
all branches finish does the walker resume its parent; additional branches each
consume a proof visit. Alternative groups set retained BlocksSkip at entry,
per native early-fail counting, so branch stars gain no new recovery ownership.
A pending-alternative count prevents a nested return from accepting an early
mandatory set before sibling branches are checked. Empty branches qualify;
zero branches, required/unsupported/CR-admitting/predicate paths stop the proof.
Root alternatives and mixed mandatory/nullable or quantified groups remain
outside this extension. Captures across checked branches share the existing
eligibility classifier. Old successful selectors/first-star discovery/depth/
capture/atomic rules, LF/no-CR mandatory-set guard, actual group-return flag,
failed-proof reset, CT/1000 limits and fallback PLUS Skip=false are unchanged.
No matcher/parser/emitter/Step/runtime state/layout/instruction/frame/journal/
snapshot/span/pointer/API/limit/workflow/package/dependency/consumer/CF changes.
913 new cases/full18169 preserve all17256 baseline expectations/frozen35. Native
18671 bounded JIT checks (1772strict-tolerant fullcapture+16896newline/UCP/offset+
3empty-branch breadth) pin group types/depths/order, empty/erased/finite/star
branches, nested/sequential groups, branch-reset/related/named/duplicate captures,
later stars/old ownership and late-branch/nested early-set traps. Strict/tolerant
fragments/work/cancellation/default budget/parser257/capture257/backtrack/breadth
limits remain. Tests-only run480 established724new failures/17445passes/0skips
(all17256baseline+189controls), zero build warnings/errors. Tests/expectations
stay unchanged after RED. See PCRE-UTF-NULLABLE-ALTERNATIVE-ENTRY-DESIGN.md and
PCRE-UTF-NULLABLE-ALTERNATIVE-ENTRY-PLAN.md. Keep102 open for mixed/required/
quantified groups, predicates/negated/property/call/DEFINE/condition bitmap
availability and remaining placements. Byte99/unsafe100 separate. No complete
PCRE/release/Grep readiness claim; publication/all known migrations precede CF
removal; Grep last.

## UTF finite nullable-group repeats

PcreUtfFiniteGroupEntryReferenceTests extends the existing bounded UTF fallback
to finite Minimum0/Maximum>0 repeats of fully nullable prefix group bodies,
including possessive Atomic(Repeat), capture/noncapture/explicit atomic groups
and verified nullable alternatives. Repeat bodies set retained BlocksSkip;
inner stars gain no new recovery owner. A pending-repeat count blocks early
mandatory-set selection after nested returns until every body branch closes.
Continuation tuples retain RepeatMaximum/RepeatStartVisits across siblings.
One repeat/modifier visit plus the group visit is charged on entry; completed
body cost includes that group even when empty. Remaining copies are charged
using division before multiplication, including already charged nested repeats,
so no copy rescans, overflow or1000-visit bypass occurs. Over-budget proofs fail.
Old successful selectors/root/first-star/depth/capture/atomic rules, shared
nullable-atom/set/capture/actual-return/completed-proof guards, cancellation and
fallback PLUS Skip=false are unchanged. No matcher/parser/emitter/Step/runtime
state/layout/instruction/frame/journal/snapshot/span/pointer/API/limit/workflow/
package/dependency/consumer/CommandFramework changes.
1203 new cases/full19372 preserve all18169 baseline expectations/frozen35. Native
19248 bounded JIT checks (2348strict-tolerant fullcapture+16896newline/UCP/offset+
4copied-body captures) pin quantifier aliases/bounds/modifiers, group types/
depths/alternatives/nesting/sequential/source/dependency/recovery cases, greedy
empty versus lazy omitted captures, nested early-set/property traps and strict/
tolerant fragments. Work/exact/exhausted/observed/cancellation/default budget/
parser257/capture257/backtrack/copied bounds/expansion rejection remain. Large
copy controls use ordinary consumption, not native bitmap parity past proof1000.
Tests-only run482 established1061new failures/18311passes/0skips (all18169baseline+
142controls), zero build warnings/errors; tests/expectations unchanged afterward.
See PCRE-UTF-FINITE-GROUP-ENTRY-DESIGN.md and PCRE-UTF-FINITE-GROUP-ENTRY-PLAN.md.
Keep102 open for required-body/positive-minimum/unbounded group repeats, mixed
mandatory/nullable alternatives, root alternatives, predicate/negated/property/
call/DEFINE/condition bitmap availability and remaining placements. Byte99/
unsafe100 separate; no complete PCRE/release/Grep readiness claim. Publication/
all known migrations precede CF removal; Grep last.

## UTF positive finite nullable-group repeats

PcreUtfPositiveGroupEntryReferenceTests extends the existing finite fallback to
positive counts while preserving incoming recovery admission. Ordinary positive
bodies do not force BlocksSkip; the shared classifier still refuses unbounded
branch stars without a retained prefix or independently proven owner. Minimum0
keeps its old barrier; possessive Atomic(Repeat) and explicit atomic descent stop
native early-fail ownership. Existing alternative barriers remain. No repeated
AST child is assigned a new owner, avoiding accidental cache tags on every copy.
Required exact/ranged-count captures, last copied empties and byte spans remain
ordinary VM instructions. Repeated-child capture collection, pending repeat/
alternative guards, measured-copy division-before-multiplication/CT1000 budget,
set/capture/actual-return/completed-proof checks and fallback PLUS Skip=false are
unchanged. Old successful selectors/root/first-star finder/depth/capture policy
and all lowering/runtime code remain. No parser/emitter/matcher/Step/VM state/
layout/instruction/frame/journal/snapshot/span/pointer/API/limit/workflow/package/
dependency/consumer/CommandFramework changes.
1117 new cases/full20489 preserve all19372 baseline expectations/frozen35. Native
19076 bounded JIT checks (2176strict-tolerant fullcapture+16896newline/UCP/offset+
4copied captures) pin exact/ranged counts/modifiers, atomic/possessive/retained/
erased distinctions, group types/depths/alternatives/nesting/sequential/source/
dependencies, old root-owner and unproven positive-first-star consumed-run misses.
Strict/tolerant fragments/work/exact/exhausted/observer/cancellation/default
budget/parser257/capture257/backtrack/copy bounds/expansion rejection remain.
Large-copy controls use ordinary input, not bitmap parity beyond proof1000.
Tests-only run484 established991new failures/19498passes/0skips (all19372baseline+
126controls), zero build warnings/errors; tests/expectations unchanged afterward.
See PCRE-UTF-POSITIVE-GROUP-ENTRY-DESIGN.md and PCRE-UTF-POSITIVE-GROUP-ENTRY-PLAN.md.
Keep102 open for unproven repeated-group first-star ownership, required-body/
unbounded groups, mixed mandatory/nullable and root alternatives, predicate/
negated/property/call/DEFINE/condition bitmap availability and other placements.
Byte99/unsafe100 separate; no complete PCRE/release/Grep readiness claim.
Publication/all known migrations precede CF removal; Grep last.

## UTF nullable unbounded-group continuation

PcreUtfUnboundedGroupEntryReferenceTests extends the existing bounded fallback
to fully nullable group loops. Minimum0/1 loops block inner scalar early-fail
ownership; higher required copies preserve incoming admission unless an existing
atomic/possessive/alternative/retained barrier applies. Unproven first stars in
those earlier copies remain rejected. Group-star identity remains compile-time
metadata but group children never receive scalar recovery Skip eligibility;
primitive identity/depth/capture rules and lowering are unchanged.
Compile-time RepeatMaximum becomes RepeatCopies: finite paths still use Maximum,
unbounded paths use guarded Minimum+1 for required copies plus one loop body.
Minimum>=1000 stops before addition. The body is checked once, every branch must
be fully nullable, and existing group-inclusive division-before-multiplication
charges all bounded study copies/nested costs under1000 visits without rescans.
Pending bodies/alternatives still reject early mandatory sets. Shared set/
capture/actual-return/completed-proof/cancellation/reset guards and fallback PLUS
Skip=false remain. No parser/emitter/matcher/Step/VM state/layout/instruction/
frame/journal/snapshot/span/pointer/API/limit/workflow/package/dependency/consumer/
CommandFramework changes; existing loop progress/resource guards stay intact.
1443 new cases/full21932 preserve all20489 baseline expectations/frozen35. Native
19728 bounded JIT checks (2828strict-tolerant fullcapture+16896newline/UCP/offset+
4copied captures) pin loops/count aliases/modifiers/group types/depths/nesting/
sequential/alternatives/source/dependencies, lazy unset versus required/greedy
empty captures, primitive-owner versus group-loop and guarded versus unproven
required-copy cases. Strict/tolerant fragments/work/exact/exhausted/observer/
cancellation/default budget/parser257/capture257/backtrack/copy/expansion limits
remain. Large-copy controls use ordinary input, not bitmap parity past proof1000.
Tests-only run486 established1301new failures/20631passes/0skips (all20489baseline+
142controls), zero build warnings/errors; tests/expectations unchanged afterward.
See PCRE-UTF-UNBOUNDED-GROUP-ENTRY-DESIGN.md and PCRE-UTF-UNBOUNDED-GROUP-ENTRY-PLAN.md.
Keep102 open for required-body groups, mixed mandatory/nullable/root alternatives,
unproven earlier-copy owners, predicate/negated/property/call/DEFINE/condition
bitmap availability and remaining placements. Byte99/unsafe100 separate; no
complete PCRE/release/Grep readiness claim. Publication/all known migrations
precede CF removal; Grep last.

## UTF word-boundary prefixes

PcreUtfBoundaryPrefixReferenceTests pins boundary traversal along already
supported mandatory positive predicate-free LF/no-CR PLUS proofs and nullable
group continuation. Native set_start_bits and detect_early_fail ignore ASCII/UCP
word boundaries without raising the retained count. Both existing compiler
walkers now cross PcreByteWordBoundary with one bounded proof visit. BlocksSkip,
first-star identity/depth/capture policy, group-return barriers and primitive
owner placement remain; boundaries still execute as runtime assertions.
No parser/matcher/Step/VM state/journal/API/limit/workflow/package/
dependency/consumer/CommandFramework changes. Proof1000 and all resource limits
stay. Broader direct-class/assertion/root-alternative bitmap availability,
required-body groups, unproven required-copy owners and other #102 gates remain.
1414 new cases/full23346 preserve all21932 baseline expectations/frozen35.
Native12703 bounded JIT checks (2788 strict/tolerant full-capture,9792 six-newline/
UCP/every-offset,3long-prefix ordinary controls,120required-copy controls) plus80 initial probes pin ASCII/
UCP/restrictions, positive/negative/repeated boundaries, group/depth/capture/
reference/condition/branch-reset forms, scalar-owner versus group-loop guards,
strict/tolerant fragments/work/exact/exhausted/observer/cancellation/default
budget. Long-boundary controls do not claim bitmap parity beyond proof1000.
Tests-only run488 observed886 new-class failures/22460passes/0skips; all21932
baseline+528new controls passed, zero compiler warnings/errors. Tests unchanged.
See PCRE-UTF-BOUNDARY-PREFIX-DESIGN.md and PCRE-UTF-BOUNDARY-PREFIX-PLAN.md.
Keep102/unsafe100 open; no complete PCRE/release/Grep readiness claim.
Publication/all known migrations precede CF removal; Grep last.

Run489 exposed26 remaining new-class failures (23320passes/0skips; all21932baseline
pass). Extend first-star identity through ordinary first required group copies:
finite positive groups, or unbounded Minimum>=2. Keep Minimum1 unbounded loops
excluded; preserve atomic/possessive/alternative/depth/capture guards. A private
compiler helper consumes skip eligibility once, marking only the first emitted
scalar-star instruction while retaining AST identity for source proof. No new
runtime state; joined nullable-body/copy charging and all1000-visit guards stay.

## UTF boundary-prefixed starter classes

PcreUtfBoundaryStartClassReferenceTests extends the general UTF class proof to
leading word boundaries without requiring a selected PLUS set. Local AddBranches
skips only PcreByteWordBoundary nodes with one charged visit/cancellation per
node, then enqueues the first significant node. Boundary-only branches reject.
The selected-repeat path retains its existing nullable walker. Every alternative
must still exclude CR and at least one must admit LF; group/atomic/required-repeat,
unselected wide PLUS/exact-large/predicate/negated/source-shape guards remain.
No owner selection/marker placement/parser/emitter/matcher/Step/VM state/journal/
API/limit/workflow/package/dependency/consumer/CommandFramework changes.
179 new cases/full23525 preserve all23346baseline expectations/frozen35.
Native7887 bounded JIT checks (324strict/tolerant full-capture,7560six-newline/
UCP/every-offset,3long-prefix ordinary controls) plus48initial probes pin ASCII/
wide/restricted-UCP boundaries, wrappers/depths, alternatives/branch resets,
counts/capture dependencies/casing and CR-including/boundary-only/predicate/
anchor exclusions. Strict/tolerant fragments/work/exact/exhausted/observed/
cancellation/default budget remain; long controls claim no bitmap parity past1000.
Tests-only run491/macOS ARM64/net8 observed113new-class failures/23412passes/0skips,
all23346baseline+66new controls passing,0compiler warnings/errors. Tests unchanged.
See PCRE-UTF-BOUNDARY-START-CLASS-DESIGN.md and PCRE-UTF-BOUNDARY-START-CLASS-PLAN.md.
#102 stays open for broader optional/assertion/predicate/root-alternative/group/
required-copy placements and bitmap availability; unsafe100 remains separate.
No full PCRE/release/Grep readiness claim. Publication/all known migrations
precede CF removal; Grep last.

## UTF erased primitive prefixes before starter classes

PcreUtfErasedStartClassReferenceTests reuses IsUtfCrLfErasedRepeat in the general
class proof. Boundaries cost1, direct zero-count primitive/reference repeats
cost2, and atomic repeat wrappers add1. Charge before addition and cancel per
prefix; exhausted branches reject. Retained groups/assertions/calls and other
unproven prefixes keep their existing admission. Selected-repeat traversal,
owner selection/marker placement and every-alternative CR/LF checks stay.
No parser/emitter/matcher/Step/VM state/journal/API/limit/workflow/package/
dependency/consumer/CommandFramework changes; resource ceilings remain.
199 new cases/full23724 preserve all23525baseline expectations/frozen35.
Native7927 bounded JIT checks (364strict/tolerant full-capture,7560six-newline/
UCP/every-offset,3long ordinary controls) pin erased literal/type/set/property/
newline/reference forms, modifiers/casing/boundaries, wrappers/counts/alternatives/
branch resets/capture dependencies and retained/CR/property/anchor exclusions.
Strict/tolerant fragments/work/exact/exhausted/observed/cancellation/default
budget remain; long controls claim no bitmap parity past proof1000.
Tests-only run493/Linux x64/net8 observed147new-class failures/23577passes/0skips,
all23525baseline+52new controls passing,0compiler warnings/errors. Tests unchanged.
See PCRE-UTF-ERASED-START-CLASS-DESIGN.md and PCRE-UTF-ERASED-START-CLASS-PLAN.md.
PCRE-UTF-RECENT-PROGRESS.md preserves issue102's recent index at GitHub's body
limit; earlier history remains intact. Keep102/unsafe100 and broader bitmap/
optional/assertion/group/required-copy gates open; no full PCRE/release/Grep
readiness claim. Publication/all known migrations precede CF removal; Grep last.

## UTF retained zero-count prefixes before starter classes

PcreUtfRetainedZeroStartClassReferenceTests shares the existing retained-zero
group/assertion/capture-condition list through IsUtfCrLfRetainedZeroRepeat.
The PLUS nullable walker keeps retained=true, cost2/cost3 for an atomic group
child, plus any outer modifier charge. The general class proof accepts the same
list and treats skipped bodies as opaque, charging the atomic child separately.
Boundaries/erased prefixes,1000-visit pre-addition guard,cancellation,exhausted
branches,unsupported kinds,owner/marker and every-alternative CR/LF guards stay.
No parser/emitter/matcher/Step/VM state/journal/API/limit/workflow/package/
dependency/consumer/CommandFramework changes; resource ceilings unchanged.
223 new cases/full23947 preserve all23724baseline expectations/frozen35.
Native7975 bounded JIT checks (412strict/tolerant full-capture,7560six-newline/
UCP/every-offset,3long ordinary controls) pin zero groups/atomic groups/literal-
general lookahead/lookbehind/capture conditions, opaque property/reference/call
bodies, modifiers/boundaries/erased prefixes/wrappers/counts/alternatives/capture
dependencies and exclusions. Existing PLUS/primitive-owner controls remain.
Strict/tolerant fragments/work/exact/exhausted/observed/cancellation/default
budget stay; long controls claim no bitmap parity past1000.
Tests-only run495/Linux x64/net8 observed167new-class failures/23780passes/0skips,
all23724baseline+56new controls passing,0compiler warnings/errors. Tests unchanged.
See PCRE-UTF-RETAINED-ZERO-START-CLASS-DESIGN.md and
PCRE-UTF-RETAINED-ZERO-START-CLASS-PLAN.md; recent index gains one row.
Keep102/unsafe100 and broader bitmap/optional/assertion/group/required-copy
placements open; no full PCRE/release/Grep readiness claim. Publication/all known
migrations precede CF removal; Grep last.

## Finite optional prefixes and proven root stars before UTF starter classes

PcreUtfFiniteOptionalStartClassReferenceTests pins general starter-class entry
after supported finite optional primitives. HasUtfCrLfStartClass reuses the
nullable-prefix walker. Finite repeats keep existing atom,CR-exclusion,UCP and
source-sensitive caseless bitmap guards. Unbounded repeats require selected
PLUS or existing root-star identity plus the original BlocksSkip/identity rule.
The completed class proof is computed before emission and preserves first-star
identity/skip state without a selected PLUS; unproved stars still clear. The
existing owner classifier and one-emission marker helper are unchanged.
This removes the duplicate zero/boundary scan without changing its accepted
kinds,costs,retained state or exhausted-branch rejection. Every-alternative
CR exclusion/at-least-one-LF,required-repeat/wide PLUS/range guards and1000-visit
budget/cancellation remain. No arbitrary nullable-group continuation.
No parser/emitter helper/matcher/Step/VM/journal/API/limit/workflow/package/
dependency/consumer/CommandFramework changes; resource ceilings unchanged.
377 new cases/full24324 preserve all23947baseline expectations/frozen35.
Native6771 bounded JIT checks (560strict/tolerant full-capture,6048six-newline/
UCP/every-offset,3long ordinary,160root-star full-capture checks) cover optional
queries/bounded counts/literals/types/sets/caseless sources,mixed zero/boundary
prefixes,wrappers/required repeats/alternatives/dependencies and exclusions.
The additional PcreUtfRootStarStartClassReferenceTests pins root-star literal/
type/set greedy/lazy/possessive forms and consumed/unconsumed CRLF endpoints.
Existing PLUS owners,strict validation/fragments/caller boundaries,exact/
exhausted/observed work,cancellation/default budget stay. Long ordinary controls
claim no bitmap parity past1000.
Tests-only run497/Linux ARM64/net8 observed166new-class failures/24078passes/
0skips,all23947baseline+131controls passing,0compiler warnings/errors. Initial
finite fix run498 left6root-star controls failing/24238passes/0skips; no baseline
failures. Additional tests-only run499/Linux x64/net8 observed45failures
(6original+39new-star)/24279passes/0skips; all23947baseline+332controls passed,
0compiler warnings/errors. Both test files remain unchanged after their RED.
See PCRE-UTF-FINITE-OPTIONAL-START-CLASS-DESIGN.md and
PCRE-UTF-FINITE-OPTIONAL-START-CLASS-PLAN.md for the evidence-driven scope change;
recent index gains one row. Keep102/unsafe100 and broader optional/assertion/
predicate/group/required-copy/unselected wide PLUS/root-alternative bitmap
availability open. No full PCRE/release/Grep readiness claim. Publication/all
known migrations precede CF removal; Grep last.

## Bitmap-skipped assertions in UTF prefix proofs

PcreUtfSkippedAssertionPrefixReferenceTests pins negative lookahead and all
lookbehind prefixes,including literal/general,positive/negative,non-atomic and
variable forms. FindUtfCrLfLeadingNode admits exactly negative-or-behind
PcreByteLiteralAssertion,negative PcreByteLookahead and every PcreByteLookbehind,
returning cost2/retained=true. Native bitmap study skips these bodies; early-fail
descent stops there,so a following PLUS/star cannot gain its own recovery marker.
Assertion execution,captures-before-match,negative capture rollback,reverse
retries,fragment isolation and existing earlier root-star ownership stay intact.
Positive ahead and arbitrary optional assertion repeats are not added. Shared
source/atom/range/alternative/required-repeat/owner guards,pre-addition1000
budget,cancellation and exhausted branches remain. Body opacity applies only
to bitmap study. No parser/emitter helper/owner finder/matcher/Step/VM/frame/
journal/API/limit/workflow/package/dependency/consumer/CommandFramework changes;
resource ceilings unchanged.297new/full24621 preserve all24324baseline/frozen35.
Native6618 bounded JIT checks (560strict/tolerant full captures,6048six-newline/
UCP/every-offset,3long ordinary,7empty-assertion source probes) pin opaque body
properties/classes,capture dependencies,mixed prefixes,wrappers/counts/
alternatives,PLUS modifiers and earlier-vs-later stars. Empty positive behind
preserves the native recovery barrier; empty negative assertions still fail.
Strict validation/caller boundaries/tolerant fragments,exact/exhausted/observed
work,cancellation/default budget and long ordinary controls remain; no bitmap
parity claim beyond1000. Tests-only run501/Linux x64/net8 observed217new-class
failures/24404passes/0skips,all24324baseline+80controls passing,0compiler warnings/
errors. Tests unchanged after RED. See PCRE-UTF-SKIPPED-ASSERTION-PREFIX-DESIGN.md
and PCRE-UTF-SKIPPED-ASSERTION-PREFIX-PLAN.md; recent index gains one row.
Keep102/unsafe100 and broader positive-ahead/predicate/nullable-group/required-
copy/unselected wide PLUS/root-alternative bitmap availability open. No full
PCRE/release/Grep readiness claim. Publication/all known migrations precede
CommandFramework removal; Grep last.

## Repeated assertions and optional class bodies in UTF prefix proofs

PcreUtfRepeatedAssertionPrefixReferenceTests pins required exact copies of the
already-skipped negative-ahead/all-behind kinds. IsUtfCrLfSkippedAssertion shares
exactly #157's list;direct cost2/retained=true stays. Exact positive copies require
Maximum<=(1000-1)/2 before multiplication,then cost2*Maximum+1/retained=true.
Native investigation found BRAZERO can enter optional assertion bodies,so there
is no blanket optional admission. Four original captured-class cases remained
red after the exact-only fix;all original tests were preserved and310 focused
PcreUtfOptionalAssertionClassReferenceTests cases were added before correction.
Non-exact negative general ahead now also requires every body branch to reach a
mandatory positive,predicate-free supported scalar set through groups. A bounded
(Node?,Body?) compile-time worklist charges pops/branches before enqueue,cancels,
shares visits,and reuses IsUtfCrLfOptionalSet source/range/CR guards. Empty/
unknown/property/CR heads are rejected;body-tail/nullable-return inference is not
added. Non-exact proof uses the same prechecked Maximum and copy cost. Literals/
types/nullable bodies,optional behind/positive ahead and broader placements stay
outside that path. Exact bodies stay opaque; optional class bodies are studied.
Zero-count behavior,earlier root-star identity,later recovery barriers,all source/
alternative/required-repeat guards and pre-addition/wrapper charging remain.
No parser/emitter helper/owner finder/matcher/Step/VM/frame/journal/API/limit/
workflow/package/dependency/consumer/CommandFramework changes; ceilings unchanged.
752new/full25373 preserve all24621baseline/frozen35. Native13538 bounded JIT
checks =844initial captures+6048initial protocol+6long ordinary+592optional-class
captures+6048optional protocol. Counts/modifiers,assertion kinds,capture effects/
rollback/dependencies,opaque-vs-studied properties/classes,mixed prefixes,
wrappers/alternatives,PLUS/star endpoints,empty assertions and exclusions stay
pinned.498 exact copies fit the total proof fixture;499/500/1001-count and long
ordinary controls claim no bitmap parity beyond1000. Strict validation/tolerant
fragments/caller boundaries,exact/exhausted/observed work,cancellation/default
budget stay. The general optional default-budget fixture uses10000 CRLF pairs
under the unchanged1m ceiling; it performs more VM work than literal assertions.
RED503/macOS ARM64/net8:353new-class failures/24710passes/0skips,all24621baseline+
89controls pass. Initial fix504/Linux x64/net8:left4original failures/25059passes/
0skips,allbaseline+438controls pass. Added tests-only RED505/Linux x64/net8:
281failures(4original+277new)/25092passes/0skips,allbaseline+471controls pass.
Compiler warnings/errors0 throughout;both test blobs unchanged after their RED.
See PCRE-UTF-REPEATED-ASSERTION-PREFIX-DESIGN.md and
PCRE-UTF-REPEATED-ASSERTION-PREFIX-PLAN.md for scope corrections;recent index gains
one row. Keep102/unsafe100 and broader optional/positive-ahead/predicate/nullable-
group/required-copy/unselected wide PLUS/root-alternative availability open.
No full PCRE/release/Grep readiness claim. Publication/all known migrations before
CommandFramework removal;Grep last.

PR158 oracle correction: run511 isolated a native-only PCRE.NET JIT host crash
on Windows ARM64 for count500; the managed-only probe passes. Issue159 tracks
it. The three counted ordinary fallback/cancellation controls use the native
interpreter on every platform, preserving all inputs/assertions and full25373.
Bitmap-sensitive cases and long sequential controls retain JIT. Temporary
probes/tracing/serial scheduling are removed before final six-platform CI.
The original442-case blob changes only for this disclosed harness correction;
the focused310-case blob stays unchanged. See the repeated-assertion design.

## Optional negative-ahead mandatory bitmap heads (PR161)

After merged158/160, PcreUtfOptionalAssertionBitmapReferenceTests extends bounded
body study to mandatory literal/type and singleton folded-class heads. Collapsed
nonempty negative literal ahead decodes its first scalar through PcreUtf8.TryRead;
general negative ahead reuses the existing all-branch group/atomic worklist.
Valid mandatory literals exclude CR. Supported types are positive AsciiDigit,
AsciiWord,HorizontalSpace and negative AsciiSpace, preserving UCP/scoped ASCII
Kind distinctions. Positive predicate-free sets share existing bounded range/CR
validation; mandatory CHAR/CHARI/PROP-PT_CLIST study is distinct from optional
TYPEQUERY guards, so optional-atom source/caseless rules stay unchanged. Paired
root optional-atom controls pin that difference. No nullable/empty/repeated-head,
body-tail return,positive-ahead or lookbehind admission is added by this path.

Maximum>0/Minimum!=Maximum and Maximum<=(1000-1)/2 remain before study/multiply;
copy cost2*Maximum+1/retained=true,wrapper/pre-addition accounting,cancellation,
shared visits,root-star ownership and later PLUS/star barriers stay. Exact and
zero paths are unchanged. No parser/emitter/owner/matcher/Step/VM/frame/journal/
API/limit/workflow/contract/dependency/consumer/CommandFramework changes. Existing
budgets and35frozen blobs remain; no spans/pointers in long-lived VM state.

1092new/full26465 retain all25373baseline expectations. Native13324 qualified
Linux x64 JIT checks =2084full-capture+11208six-newline/UCP/every-offset+32resource
checks,with positive JIT sizes and bounded spans. Cases pin counts/modifiers,
collapsed/general/wrapped/alternative heads,captures/rollback/dependencies,
scoped UCP/ASCII/caseless pairs,source exceptions,PLUS/star endpoints,strict
validation/tolerant fragments/caller boundaries,work/cancellation/default budget.
Out-of-proof counted ordinary controls use native interpretation in CI for159;
no bitmap parity claim beyond1000. Broader nullable/repeated/body-prefix/property/
positive-ahead/behind availability stays open102; byte99/unsafe100/native159
remain separate. RED514/Linux x64/net8/9/10:555new-class failures/25507passes/
0skips each;all25373baseline+134newcontrols pass,0compilerwarnings/errors.
New test blobb1ae0e9f2ec8039c5e17f98b23e95b3ed8d2932f is frozen after RED.
See PCRE-UTF-OPTIONAL-ASSERTION-BITMAP-DESIGN.md and PLAN; final CI is in PR161.
No full PCRE/release/Grep readiness claim. Publication/all known migrations
precede CF removal; Grep last.

Scoped-group correction: the initial proof resolved545 of555 failures; the ten
remaining original controls are finite optional scoped noncapturing groups.
PcreUtfOptionalScopedGroupBitmapReferenceTests adds403 cases, preserving the
original689. Its RED516/Linux ARM64/net8/9/10 shows282failures/26183passes/0skips
each, all25373baseline+810newcontrols pass, compiler warnings/errors0. Ten
failures are original controls;272 are focused new cases. Focused test blob
a4326cbb69d0f3e4a8021d9a47adb8603b741e7d stays unchanged after RED.

Finite optional noncapturing single-branch/single-leaf groups share mandatory
head classification with assertion study. Minimum0/Maximum1..499 is checked
before study/multiply; group/body and range visits, copy/wrapper/pre-addition
charges and retained barriers remain bounded. Unsupported shapes are rejected
before charging fallback visits. No general nested/capturing/nullable/tail-return
or unbounded-group admission. Scoped/global casing, consumed prefixes and option
restoration, UCP/ASCII/property/CR controls, ownership and resource contracts are
pinned. Both new test blobs and all baseline expectations remain unchanged.

## Optional noncapturing group mandatory-body heads (PR162)

After merged161, finite optional noncapturing groups can prove all branches
through nested capture/noncapture/atomic wrappers to mandatory literal/type or
positive predicate-free range heads excluding CR. A mandatory head can have a
runtime tail; capture rollback and option restoration remain unchanged. The
existing assertion body worklist becomes HasUtfCrLfMandatoryBodyBitmap, shared
with group study. Preserve the old single-leaf path and exact charges. New body
proofs use a bounded local visit checkpoint, committed only on success so failed
nullable fallback keeps its prior budget. No nullable/repeated/prefix-assertion/
empty-branch/capturing-outer/unbounded-group or tail-return inference is added.

817new/full27282 preserve all26465baseline expectations and35 frozen blobs.
Native7552 qualified Linux x64 JIT checks =1584full-capture+5952six-newline/UCP/
every-offset+16resource; positive JIT sizes and bounded spans. Larger-count/deep-
capture exploratory calls are excluded. Cases pin modifiers, branches/wrappers,
tail failure, consumed-prefix captures, scoped casing/restoration, UCP/ASCII/
property/CR controls, dependencies, PLUS/star ownership, strict/tolerant UTF,
caller offsets, work/default budget/cancellation and count/expansion limits.
Ordinary count199/499/500 controls use native interpretation in CI for159.

Successful-build RED518/Linux x64/net8/9/10:545new-class failures/26737passes/
0skips each; all26465baseline+272newcontrols pass;0compilerwarnings/errors. Test
bloba35d52ccb3df261d4b58d3b22ac3711c4c86da2d remains frozen afterward. Existing
Maximum1..499 pre-study bound,2*Maximum+1/wrapper/pre-addition charges, retained
barriers,cancellation,source/caseless guards and root-star identity are unchanged.
No parser/emitter/owner/matcher/Step/VM/frame/journal/API/limit/workflow/contract/
dependency/consumer/CommandFramework changes or production reference fallback.
See PCRE-UTF-OPTIONAL-GROUP-HEADS-DESIGN.md and PLAN; exact final CI is in PR162.
Keep7/99/100/102/native159 and broader PCRE/release/Grep gates open. Publication/
all known migrations precede CommandFramework removal; Grep remains last.

## Optional capturing-group mandatory bitmap heads (PR163)

After merged162, finite optional numbered/named capturing outer groups share
HasUtfCrLfMandatoryBodyBitmap with noncapturing groups and negative assertions.
Only the body selector is broadened; every branch still needs a supported
mandatory literal/type/positive predicate-free range head excluding CR. Preserve
the prior noncapturing single-leaf path and charges. New capture paths use the
existing bounded success-only visit checkpoint so failed nullable fallback keeps
its prior budget. No capture numbering/journal/rollback/reference/condition/tail/
option-restoration runtime changes. Native CBRA study skips its capture operand
and follows the same bracket body; retained barriers preserve early-fail rules.

987new/full28269 preserve all27282baseline expectations and35frozen blobs. Native
7892 qualified Linux x64 JIT checks =1924full-capture+5952six-newline/UCP/every-
offset+16resource; positive JIT sizes and bounded spans. Cases pin numbered/
named/branch-reset aliases, absent/consumed/last-iteration/rollback captures, tail
failure, references/conditions, scoped options,UCP/ASCII/property/CR controls,
root/nested alternatives,PLUS/star/capture-ancestor ownership,strict/tolerant UTF,
caller offsets,work/default budget/cancellation and count/expansion limits.
Ordinary count199/499/500 controls use native interpretation in CI for159.

Successful-build RED520/Linux ARM64/net8/9/10:645new-class failures/27624passes/
0skips each; all27282baseline+342newcontrols pass;0compilerwarnings/errors. Test
blobae378b6c7de8354f9102e9d7e90f0f95e5cce45a remains frozen afterward. Existing
Minimum0/Maximum1..499 pre-study bound,2*Maximum+1/wrapper/pre-addition charges,
cancellation,source/caseless guards and all owner/limit paths remain unchanged.
No nullable/repeated/prefix-assertion/empty/unbounded/tail-return inference or new
walker. No parser/emitter/owner/matcher/Step/VM/frame/journal/API/limit/workflow/
contract/dependency/consumer/CommandFramework changes or production fallback.
See PCRE-UTF-OPTIONAL-CAPTURE-HEADS-DESIGN.md and PLAN; exact final CI is in PR163.
Keep7/99/100/102/native159 and broader PCRE/release/Grep gates open. Publication
and all known migrations precede CommandFramework removal; Grep remains last.

## Atomic brackets and skipped mandatory-body prefixes (PR164)

After merged163, explicit Atomic{NoncapturingGroup} outer bodies share the
existing mandatory-body bitmap proof, with one wrapper visit charged inside the
success-only checkpoint. Capture/noncapture and prior single-leaf paths stay
unchanged. Each body branch can cross only unquantified word boundaries and
IsUtfCrLfSkippedAssertion nodes before its mandatory head, charging1/2 with
cancellation and checked pre-addition. Branches ending after skipped nodes fail;
all branches still require supported literal/type/positive predicate-free ranges
excluding CR. Native ONCE/WORD_BOUNDARY/ASSERT_NOT/ASSERTBACK study motivates this
shared iterative scan; no recursive call to FindUtfCrLfLeadingNode is introduced.

1364new/full29633 preserve all28269baseline expectations and35frozen blobs. Native
10118 qualified Linux x64 JIT checks =2656full-capture+7440six-newline/UCP/every-
offset+22resource; positive JIT sizes,complete vectors,bounded spans. Cases pin
atomic tail commitment,behind captures before match start,skipped predicate
bodies,branches/rollback/references/conditions,UCP/ASCII,PLUS/star/ancestor owners,
strict/tolerant UTF,caller offsets,work/default budget/cancellation,count/expansion
and skipped-prefix budget controls. Ordinary count199/499/500 and prefix33/999/
1001 controls use native interpretation in CI for159; no bitmap claim beyond1000.

Successful-build RED522/Linux x64/net8/9/10:838new-class failures/28663passes/
0skips each; all28269baseline+394newcontrols pass;0compilerwarnings/errors. All2514
failure records and2514xUnit FAIL records are confined to the new class. Test blob
d0c25ed3fda55ad4c5a649c257dd728a3facce0f remains frozen afterward. Existing finite
copy bounds/costs,retained barriers,casing/source guards,owners and limits stay.
No optional/exact-repeat/general-positive-ahead/erased/nullable-return admission.
No parser/owner/matcher/Step/VM/frame/journal/API/limit/workflow/contract/
dependency/consumer/CommandFramework changes or production fallback. See
PCRE-UTF-BRACKET-PREFIX-BITMAP-DESIGN.md and PLAN; exact final CI is in PR164.
Keep7/99/100/102/native159 and broader PCRE/release/Grep gates open. Publication
and all known migrations precede CommandFramework removal; Grep remains last.

First implementation523 resolved837/838 failures; the remaining original dense
negative-assertion miss exhausted the unchanged default work limit. Focused132
cases add248native literal checks and preserve original1232cases. RED524/Linux
x64/net8/9/10:5failures/29628passes/0skips each, all28269baseline+1231original
controls+128focused controls pass. Failures: original dense miss, two scaled-work
checks (200031 steps for2000CRLFpairs), and two positive-ahead/literal guards.
Focused blob71fd17536ac8736b3f74dd65240a95f15b0ba746 is frozen after RED.

Native ASSERT study explicitly skips a positive ahead immediately followed by
a mandatory literal. Admit only that direct valid CR-excluding literal case,
not general positive-ahead/class continuations. For UTF greedy general atomic
negative ahead Minimum0/Maximum2..499, emit maximum-or-zero retry exits under
one Split while scheduling every original assertion copy. Negative guards restore
body state, so intermediate counts expose the same state to the tail. Bitmap/
owner study retains original AST bounds; positive/lazy/required/direct-literal/
byte-mode/other counts retain prior emission. This necessary emitter extension
fixes redundant tail retries without changing matcher/Step/frames/journal or
limits; all original copies remain, preserving huge expansion rejection.
The focused524 WindowsARM64 setup failed in PowerShell TaskbarJumpList CLR before
repository build, separate from native159; final source receives fresh full CI.


## Positive ahead across skipped bitmap prefixes (PR165)

After merged164, the native PCRE2 10.48 ASSERT/ASSERT_NA start-bitmap study
skips intervening lookarounds and word boundaries before a mandatory literal.
The existing optional-bracket body walker now proves that finite unquantified
prefix shape in one iterative branch scan. A branch must reach a direct valid
CR-excluding literal; unsupported or empty branches fail. Outer visits charge
each prefix exactly once, preserve the success-only checkpoint, cancellation,
count/proof bounds and retained skip/owner barriers. Runtime assertion and
capture semantics remain with the unchanged matcher.

Native JIT corpus:432 literal rows/864 strict+tolerant checks, noncapturing,
capturing and atomic optional brackets, greedy/lazy bounded copies, positive
ahead followed by negative ahead/behind/word boundaries, ASCII/UCP and CRLF/
ordinary subjects. Each native capture vector is complete and byte-bounded.
Successful-build RED526/Linux ARM64/net8/9/10:108 failures in the new class,
29957 passes and zero skips per framework; all29633 baseline and324 new
controls pass, compiler warnings/errors0. The test blob is frozen as
492b487337130e2514dd6a430133821ba49d8ce7. Full final CI is in PR165.

No parser, emitter, matcher, Step, frames, journal, API, limit, dependency,
workflow, consumer or CommandFramework changes. Full source-pinned
Compatibility and consumer migration remain separate; Grep stays last.


## Positive ahead before mandatory type bitmap heads (PR166)

After merged165, the optional-bracket bitmap proof accepts a positive ahead
across unquantified skipped lookarounds and boundaries only if the first
significant node is a direct CR-excluding literal or supported scalar type.
The existing HasUtfCrLfTypeBitmap admits only ASCII digit/word, horizontal
space and negated ASCII space; parser UCP distinctions and CR exclusion stay.
All-branch mandatory heads, wrapper/copy costs, cancellation, success-only
checkpoints, proof limits, retained owners and runtime assertion/capture
behavior remain unchanged. No arbitrary Unicode property, class, repeat,
unsupported or empty branch is admitted by this extension.

Native PCRE.NET1.6.0/PCRE2 10.48 JIT corpus:495 literal cases/990 strict+
tolerant full-capture checks, optional capture/noncapture/atomic wrappers,
horizontal space under ASCII/UCP, ASCII digit/word/nonspace, UCP-excluded
controls, skipped assertion/boundary prefixes and CRLF/ordinary subjects.
Positive native JIT sizes and complete bounded vectors were checked. Test
blob7b35b99cb7ce74c58c5792db76af657c2e1e7306 frozen after successful-
build RED528/Linux ARM64/net8/9/10:126 failures/new class only,30434 passes,
0 skips and0 compiler warnings/errors each; all30065 baseline+369 new controls
pass. Final exact source-head six-platform results are in PR166.

No parser/emitter/matcher/Step/VM/frame/journal/API/limit/workflow/dependency/
consumer/CommandFramework changes or production fallback. Broader PCRE,
release and Grep gates remain open; publication and all known migrations
precede CommandFramework removal. Grep remains last.


## Mandatory literal repeats after positive ahead (PR167)

After merged166, optional-bracket UTF bitmap study looks past positive ahead
when the next significant head is a mandatory direct CR-excluding literal
repeat, including possessive atomic repeat wrappers. Its iterative all-branch
proof uses the existing valid-scalar guard, bounded visits, cancellation and
success-only checkpoint. Zero-minimum repeats and unsupported heads stay on
their former paths. Parser, emitter, matcher, Step, frames, journal, API,
limits, dependencies, workflows and consumers are unchanged.

Pinned native PCRE2 10.48 JIT study showed LF in the first bitmap, CR absent,
and positive JIT sizes for plus, exact, bounded, lazy and possessive literal
repeats. Thirty-five full-capture fixtures / seventy strict+tolerant checks
cover optional capture/noncapture/atomic brackets and CRLF/ordinary subjects.
Successful-build tests-only RED [run532](https://github.com/uniblab/Icod.Expressions/actions/runs/41992936864), Linux ARM64/net8/9/10: seven failures confined to the new class,
30,588 passes, zero skips and compiler warnings/errors per framework. All
30,560 baseline cases and 28 new controls pass. Frozen test blob
2ba39ba6c05fe90f5b2a1222fcc723f4a93775f5 remains unchanged after RED.
Final six-platform results are in PR167. Keep issue102 and broader PCRE,
release and consumer migration gates open; Grep remains last.


## Mandatory direct classes after positive ahead (PR168)

After merged167, the optional-bracket UTF bitmap proof recognizes a direct
mandatory CR-excluding range class following positive lookahead. Its existing
class predicate rejects negated, predicate-bearing, malformed and CR-containing
sets. The speculative continuation check copies the visit count; the iterative
mandatory leaf charges each range once under the existing proof limit and
all-branch requirement. Parser, matcher, assertions, captures, resource limits,
workflows and consumers are unchanged.

Pinned native PCRE2 10.48 JIT reports LF present, CR absent and positive JIT
sizes for six optional-bracket class patterns, including capture/noncapture,
atomic, finite lazy, alternative and mixed-width ranges. Thirty-six full-capture
fixtures / seventy-two strict+tolerant checks cover CRLF and ordinary inputs.
Successful-build test-only RED [run534](https://github.com/uniblab/Icod.Expressions/actions/runs/42002406924), Linux ARM64/net8/9/10: six failures in this new class only,
30,625 passes, zero skips and compiler warnings/errors per framework. All
30,595 baseline tests plus 30 new controls passed. Frozen test blob
6470414d4b34898d85c48ef3932779bf61aa950e is unchanged after RED.
Final six-platform results are in PR168. Issue102 and broader PCRE, release
and consumer migration gates remain open; Grep remains last.


## Mandatory range-class repeats after positive ahead (PR169)

After merged168, the optional-bracket UTF bitmap proof admits positive ahead
before mandatory CR-excluding range-class repeats, including possessive atomic
wrappers. It reuses the established range predicate and bounds; speculative
pre-scan checks use a copied visit count, while the mandatory leaf charges
each range once. Zero-minimum, negated, predicate-bearing, CR-containing and
unproven operands remain on their former paths. Parser, matcher, assertions,
captures, APIs, limits, workflows and consumers are unchanged.

Pinned native PCRE2 10.48 JIT reports LF present, CR absent and positive JIT
sizes for seven optional bracket patterns with plus, exact, lazy, possessive
and mixed-width class repeats. Forty-two full-capture fixtures / eighty-four
strict+tolerant checks cover CRLF and ordinary subjects. Successful-build
[test-only RED536](https://github.com/uniblab/Icod.Expressions/actions/runs/42129649283), macOS ARM64/net8/9/10: seven failures in the new class only,
30,666 passes, zero skips and compiler warnings/errors per framework. All
30,631 baseline tests plus 35 new controls passed. Immutable test blob
7fa5a65a5f7df3c8617d0c80325480cb12f4031a remains unchanged after RED.
Final six-platform results are in PR169. Issue102 and broader PCRE, release
and consumer migration gates remain open; Grep remains last.


## Mandatory supported-type repeats after positive ahead (PR170)

After merged169, the optional-bracket UTF bitmap proof admits positive ahead
before mandatory repeats of types already accepted as direct bitmap heads:
ASCII digit/word, horizontal space and negated ASCII space. It unwraps the
possessive atomic repeat, requires positive minimum and maximum, and delegates
to the existing CR-excluding type predicate. Zero-minimum, UCP Unicode
properties, unsupported negated types and nested operands retain their former
paths. Existing all-branch proof, visit bounds, cancellation and runtime
matcher/capture behavior remain unchanged.

Pinned native PCRE2 10.48 JIT reports LF present, CR absent and positive JIT
sizes for eight optional-bracket patterns with plus, exact, lazy, possessive
and finite supported-type repeats. Seventy-two full-capture fixtures / one
hundred forty-four strict+tolerant checks cover CRLF and ordinary subjects.
Successful-build [test-only RED538](https://github.com/uniblab/Icod.Expressions/actions/runs/42140939691), Linux ARM64/net8/9/10: eight failures in the new class only,
30,737 passes, zero skips and compiler warnings/errors per framework. All
30,673 baseline tests plus 64 new controls passed. Immutable test blob
7e5071a1b728c50a5c8cd22ae1672a7606d3e126 remains unchanged after RED.
Final six-platform results are in PR170. Issue102 and broader PCRE, release
and consumer migration gates remain open; Grep remains last.


## Direct mandatory groups after positive ahead (PR171)

After merged170, the optional-bracket UTF bitmap proof admits positive ahead
before unquantified capture, noncapture or explicit atomic noncapture groups
when every group branch immediately begins with a proven CR-excluding direct
literal, supported type or range class. A bounded, cancellable pre-scan uses
a copied visit count; the existing iterative all-branch body traversal owns
the actual visits and rejects empty, unsupported, nested or repeated group
heads. Runtime assertions/captures, parser, VM, API, limits, workflows and
consumers are unchanged.

Pinned native PCRE2 10.48 JIT reports LF present, CR absent and positive JIT
sizes for five group patterns. Thirty full-capture fixtures / sixty strict+
tolerant checks cover CRLF and ordinary subjects. Successful-build
[test-only RED540](https://github.com/uniblab/Icod.Expressions/actions/runs/42164704456), macOS ARM64/net8/9/10: five failures in the new class only,
30,770 passes, zero skips and compiler warnings/errors per framework. All
30,745 baseline tests plus 25 new controls passed. Immutable test blob
0a3eb5dfaf6a78ecd2f10d636c455b9b933c619e remains unchanged after RED.
Final six-platform results are in PR171. Issue102 and broader PCRE, release
and consumer migration gates remain open; Grep remains last.


## Mandatory group repeat heads after positive ahead (PR172)

After merged171, the optional-bracket UTF bitmap proof admits a positive ahead
before an unquantified capture, noncapture or atomic noncapture group if every
nonempty branch immediately starts with an already proven positive-minimum
literal, supported range-class or supported type repeat. Possessive repeat
wrappers use the same proofs. The cancellable, bounded group pre-scan operates
on a copy of visits; iterative body traversal owns the actual visit budget.
Nullable, unsupported, nested and repeated group heads stay conservative.
Runtime assertions/captures, parser, VM, API, limits and consumers are unchanged.

Pinned PCRE2 10.48 JIT has LF present, CR absent and positive JIT sizes for
six group repeat patterns. Thirty-six full-capture fixtures / seventy-two
strict+tolerant checks cover CRLF and ordinary subjects. Successful-build
[test-only RED542](https://github.com/uniblab/Icod.Expressions/actions/runs/42168129435), Linux ARM64/net8/9/10: six failures in the new class only,
30,805 passes and zero skips per framework. All 30,775 baseline cases and
30 new controls passed. Immutable test blob
1937ffb9da1664c0dbfb167e6f3a6677d87e85d4 remains unchanged after RED.
Final six-platform results are in PR172. Issue102 and broader PCRE, release
and consumer migration gates remain open; Grep remains last.


## Prefixed mandatory groups after positive ahead (PR173)

After merged172, the optional-bracket UTF bitmap proof admits a positive ahead
before unquantified capture, noncapture or atomic noncapture groups whose
branches start with bounded unquantified word boundaries or general/lowered
literal lookaheads and then a proven mandatory literal, range-class, type or positive-
minimum repeat head. The cancellable pre-scan charges a copied visit count;
iterative all-branch body traversal owns actual visits and assertion/capture
execution. Empty, nullable, nested and unsupported heads stay conservative.
The parser, VM, public API, limits and production consumers are unchanged.

Pinned PCRE2 10.48 JIT reports LF present, CR absent and positive JIT sizes
for seven group prefix patterns. Forty-two full-capture fixtures / eighty-four
strict+tolerant checks cover CRLF and ordinary subjects. Successful-build
[test-only RED544](https://github.com/uniblab/Icod.Expressions/actions/runs/42180839711), macOS ARM64/net8/9/10: seven failures in the new class only,
30,846 passes, zero skips and zero compiler warnings/errors per framework.
All 30,811 baseline cases plus 35 new controls passed. Immutable test blob
712b9f523203fad2d37c59dd7eca3ac75d163124 remains unchanged after RED.
Final six-platform results are in PR173. Issue102 and broader PCRE, release
and consumer migration gates remain open; Grep remains last.


## Lookbehind-prefixed mandatory groups after positive ahead (PR174)

After merged173, the optional-bracket UTF bitmap proof admits positive ahead
before unquantified capture, noncapture or atomic noncapture groups whose
branches start with bounded, unquantified general or lowered literal positive/
negative lookbehind and then a proven mandatory literal, range class,
supported type or positive-minimum repeat head. The cancellable pre-scan
charges a copied visit count; iterative all-branch traversal owns actual
visits and assertion/capture execution. Empty, nullable, nested and
unsupported heads remain conservative. Parser, VM, API, limits and consumers
are unchanged.

Pinned PCRE2 10.48 JIT reports LF present, CR absent and positive JIT sizes
for seven group lookbehind patterns. Forty-nine full-capture fixtures /
ninety-eight strict+tolerant checks cover CRLF and ordinary subjects.
Successful-build [test-only RED547](https://github.com/uniblab/Icod.Expressions/actions/runs/42219733083), Linux x64/net8/9/10: seven failures in the new class only,
30,895 passes, zero skips and zero compiler warnings/errors per framework.
All 30,853 baseline cases plus 42 new controls passed. Immutable test blob
d74d1490f9e4dd1531d199577acab5ec5171d151 remains unchanged after RED.
Final six-platform results are in PR174. Issue102 and broader PCRE, release
and consumer migration gates remain open; Grep remains last.


## One nested mandatory group after positive ahead (PR175)

After merged174, the optional-bracket UTF bitmap proof admits positive ahead
before an unquantified group whose first significant head is one nested
unquantified capture, noncapture or atomic noncapture group. Every inner
branch must immediately reach an existing proven literal, range-class,
supported type or positive-minimum repeat head after supported unquantified
assertion/boundary prefixes. The cancellable bounded pre-scan permits only
one nested helper call and charges a copied visit count; iterative all-
branch traversal owns actual visits and runtime assertions/captures. Empty,
nullable, repeated, deeper nested and unsupported heads stay conservative.
Parser, VM, API, limits and consumers are unchanged.

Pinned PCRE2 10.48 JIT reports LF present, CR absent and positive JIT sizes
for six nested group patterns. Thirty-six full-capture fixtures / seventy-
two strict+tolerant checks cover CRLF and ordinary subjects. Successful-build
[test-only RED549](https://github.com/uniblab/Icod.Expressions/actions/runs/42227786853), Linux ARM64/net8/9/10: five failures in the new class only,
30,933 passes, zero skips and zero compiler warnings/errors per framework.
All 30,902 baseline cases plus 31 new controls passed. Immutable test blob
5f9717a09b9fa6810e3e128d9bc3d0f6de8edd33 remains unchanged after RED.
Final six-platform results are in PR175. Issue102 and broader PCRE, release
and consumer migration gates remain open; Grep remains last.


## Bounded deep mandatory groups after positive ahead (PR176)

After merged175, the optional-bracket UTF bitmap proof admits positive ahead
before chains of unquantified capture, noncapture or atomic noncapture group
heads. Every branch must reach an existing proven mandatory literal, range
class, supported type or positive-minimum repeat head after supported
unquantified assertion/boundary prefixes. An explicit pending stack replaces
the depth-one recursive helper. Group, branch and prefix visits charge the
same 1,000-visit budget with cancellation; the speculative pre-scan retains
a copied visit count and iterative all-branch traversal owns actual visits.
Empty, nullable, repeated and unsupported group heads stay conservative.
Parser, VM, API, limits and consumers are unchanged.

Pinned PCRE2 10.48 JIT reports LF present, CR absent and positive JIT sizes
for noncapture chains through depth 128 and capture chains through depth 8.
Twenty-seven full-capture fixtures / fifty-four strict+tolerant checks cover
CRLF and ordinary subjects. Successful-build [test-only RED551](https://github.com/uniblab/Icod.Expressions/actions/runs/42233238070),
Linux x64/net8/9/10: seven failures in the new class only, 30,958 passes,
zero skips and zero compiler warnings/errors per framework. All 30,938
baseline cases plus 20 new controls passed. Immutable test blob
55dfd03c0bbfe3dd0c11ccde921f2264c233180f remains unchanged after RED.
Final six-platform results are in PR176. Issue102 and broader PCRE, release
and consumer migration gates remain open; Grep remains last.


## Mandatory repeated group heads after positive ahead (PR177)

After merged176, the optional-bracket UTF bitmap proof admits positive ahead
before positive-minimum repeats of capture, noncapture or atomic noncapture
groups. Every repeated child branch must reach a proven mandatory CR-excluding
literal, range class, supported type or positive-minimum repeat head after
supported unquantified assertion/boundary prefixes. An explicit stack visits
nested group heads; the repeat, group, branch and prefix checks charge the
existing 1,000-visit budget with cancellation. The speculative pre-scan uses
a copy of visits, and the actual proof applies the same guard. Zero-minimum,
empty, nullable, CR-admitting and unsupported repeated heads remain
conservative. Runtime captures/assertions, parser, API, limits and consumers
are unchanged.

Pinned PCRE2 10.48 JIT reports LF present, CR absent and positive JIT size
for six repeated-group shapes. Eighteen full-capture fixtures / thirty-six
strict+tolerant checks cover CRLF and ordinary subjects. Successful-build
[test-only RED554](https://github.com/uniblab/Icod.Expressions/actions/runs/42314672122),
macOS ARM64/net8/9/10: six failures in the new class only, 30,977 passes,
zero skips and zero compiler warnings/errors per framework. All 30,965
baseline cases plus 12 new controls passed. The earlier syntax-invalid run553
did not count as test RED. Immutable test blob
7df5678a3f3de64d5c72bc69d37514634a68bbab remains unchanged after RED.
Final six-platform results are in PR177. Issue102 and broader PCRE, release
and consumer migration gates remain open; Grep remains last.


## Literal positive ahead before zero-minimum group repeats (PR178)

After merged177, the optional-bracket UTF bitmap proof admits a positive
single-byte ASCII literal lookahead immediately followed by a zero-minimum,
positive-maximum repeat of a capture, noncapture or atomic noncapture group,
optionally possessive. The assertion itself excludes CR from entry even when
the group repeat consumes nothing. Its child branches must still reach proven
mandatory CR-excluding heads. A copied bounded visit count charges the
assertion, repeat, group and branch proof with cancellation; runtime assertion
and capture execution remain unchanged. Other nullable continuations,
unsupported heads and wider assertions remain conservative. Parser, VM, API,
limits and consumers are unchanged.

Pinned PCRE2 10.48 JIT reports LF present, CR absent and positive JIT sizes
for five zero-minimum repeated-group shapes. Fifteen full-capture fixtures /
thirty strict+tolerant checks cover CRLF and ordinary subjects. Successful-
build [test-only RED556](https://github.com/uniblab/Icod.Expressions/actions/runs/42322153350),
Linux ARM64/net8/9/10: five failures in the new class only, 30,993 passes,
zero skips and zero compiler warnings/errors per framework. All 30,983
baseline cases plus 10 new controls passed. Immutable test blob
476fde060f4d6812075a2987c682cb2fde6c203c remains unchanged after RED.
Final six-platform results are in PR178. Issue102 and broader PCRE, release
and consumer migration gates remain open; Grep remains last.


## Word boundary before zero-minimum group repeat (PR179)

After merged178, the optional-bracket UTF bitmap proof admits one
unquantified word boundary between a positive single-byte ASCII literal
lookahead and a zero-minimum, positive-maximum repeat of a capture,
noncapture or atomic noncapture group, optionally possessive. The assertion
excludes CR from entry even if the group repeat consumes nothing. Every
repeated child branch still reaches a proven mandatory CR-excluding head.
The extra boundary charges one visit in the same 1,000-visit budget with
cancellation. Other intermediate assertions, multiple boundaries and
unsupported heads remain conservative. Runtime assertion, boundary,
repeat and capture execution, parser, API, limits and consumers are unchanged.

Pinned PCRE2 10.48 JIT reports LF present, CR absent and positive JIT sizes
for five boundary-prefixed zero-minimum repeated-group shapes. Fifteen
full-capture fixtures / thirty strict+tolerant checks cover CRLF and
ordinary subjects. Successful-build [test-only RED558](https://github.com/uniblab/Icod.Expressions/actions/runs/42328717262),
Linux x64/net8/9/10: five failures in the new class only, 31,008 passes,
zero skips and zero compiler warnings/errors per framework. All 30,998
baseline cases plus 10 new controls passed. Immutable test blob
d016b78205d40b08809852adb0dee445b5bfd19e remains unchanged after RED.
Final six-platform results are in PR179. Issue102 and broader PCRE, release
and consumer migration gates remain open; Grep remains last.

## Two boundaries before zero-minimum group repeat (PR180)

After merged179, the optional-bracket UTF bitmap proof accepts two
unquantified word-boundary nodes between a positive single-byte ASCII
literal lookahead and a zero-minimum, positive-maximum group repeat.
The lookahead excludes CR even if the repeat consumes nothing; every
repeated child branch still requires a mandatory CR-excluding head.
Both boundaries charge visits in the existing 1,000-visit budget.
Other intermediate nodes or longer chains remain conservative; runtime,
parser, API, limits and consumers are unchanged.

Pinned PCRE2 10.48 JIT retains LF and excludes CR for `\b\b` and
`\B\B`, across five group shapes, with positive JIT sizes. Thirty native
full-capture fixtures / sixty strict+tolerant checks cover CRLF and
ordinary subjects. Successful-build [test-only RED560](https://github.com/uniblab/Icod.Expressions/actions/runs/42336029188),
Linux x64/net8/9/10: ten failures in the new CRLF cases only, 31,033
passes, zero skips and zero compiler warnings/errors per framework.
All 31,013 baseline cases plus twenty new controls passed. Immutable
test blob `2e58ca6546e3a7a78a5f31179a3966df0d08d506` remains unchanged.
Final six-platform results are in PR180. Issue102 and broader PCRE,
release and consumer migration gates remain open; Grep remains last.

## Negative literal assertion before zero-minimum group repeat (PR181)

After merged180, the optional-bracket UTF bitmap proof accepts exactly
one negative single-byte ASCII literal assertion, ahead or behind,
between a positive single-byte ASCII literal lookahead and a
zero-minimum, positive-maximum group repeat. The positive assertion
excludes CR even when the repeat consumes nothing. Every repeated child
branch still requires a proven mandatory CR-excluding head. Charge
two visits for the negative assertion under the same 1,000-visit
budget. Other intermediate nodes and longer chains remain conservative;
runtime, parser, API, limits and consumers are unchanged.

Pinned PCRE2 10.48 JIT retains LF and excludes CR for `(?!Z)` and
`(?<!Z)` between the positive lookahead and each of five nullable
group shapes, with positive JIT sizes. Thirty native full-capture
fixtures / sixty strict+tolerant checks cover CRLF and ordinary
subjects. Successful-build [test-only RED562](https://github.com/uniblab/Icod.Expressions/actions/runs/42341748138),
Linux x64/net8/9/10: ten failures in the new CRLF cases only, 31,063
passes, zero skips and zero compiler warnings/errors per framework.
All 31,043 baseline cases plus twenty new controls passed. Immutable
test blob `c00998a881302dad9dabae110adf2a13c871a238` remains unchanged.
Final six-platform results are in PR181. Issue102 and broader PCRE,
release and consumer migration gates remain open; Grep remains last.

## Mixed assertion prefixes before zero-minimum group repeat (PR182)

After merged181, the optional-bracket UTF bitmap proof examines at most
two zero-width nodes between a positive single-byte ASCII literal
lookahead and a zero-minimum, positive-maximum group repeat. Only a
word boundary/nonboundary or a negative single-byte ASCII literal
assertion, ahead or behind, is admitted in each position. Boundaries
cost one visit; negative literal assertions cost two. The positive
lookahead excludes CR even if the repeat consumes nothing, and every
repeated child branch needs a mandatory CR-excluding head under the
existing 1,000-visit budget with cancellation. Other nodes and longer
chains remain conservative. Runtime, parser, API, limits and consumers
are unchanged.

Pinned PCRE2 10.48 JIT retains LF, excludes CR and reports positive JIT
sizes for ten two-node prefixes across five repeated-group shapes.
One hundred fifty native full-capture fixtures / three hundred strict+
tolerant checks cover CRLF and ordinary subjects. Successful-build
[test-only RED564](https://github.com/uniblab/Icod.Expressions/actions/runs/42347478609),
macOS ARM64/net8/9/10: fifty failures in the new CRLF cases only,
31,173 passes, zero skips and zero compiler warnings/errors per framework.
All 31,073 baseline cases plus 100 new controls passed. Immutable test
blob `6162eff08a4234cf1bcf5661939690b2fd323630` remains unchanged.
Final six-platform results are in PR182. Issue102 and broader PCRE,
release and consumer migration gates remain open; Grep remains last.

## Positive middle assertions before zero-minimum group repeats (PR183)

After merged182, the optional-bracket UTF bitmap proof admits one
positive single-byte ASCII literal assertion, ahead or behind, in
either of at most two zero-width middle positions before a zero-minimum,
positive-maximum group repeat. The existing negative literal assertions
and word boundaries remain supported. Each literal assertion charges
two visits and each boundary one under the 1,000-visit budget with
cancellation. The leading positive literal lookahead excludes CR even
when the repeat consumes nothing, and every repeated child branch
requires a mandatory CR-excluding head. Other nodes or longer chains
remain conservative; runtime, parser, API, limits and consumers are
unchanged.

Pinned PCRE2 10.48 JIT retains LF, excludes CR and reports positive
JIT sizes for twelve assertion/boundary prefixes across five group
repeat shapes. One hundred eighty native full-capture fixtures /
360 strict+tolerant checks cover CRLF and ordinary subjects.
Successful-build [test-only RED566](https://github.com/uniblab/Icod.Expressions/actions/runs/42352825688),
macOS ARM64/net8/9/10: sixty failures in the new CRLF cases only,
31,343 passes, zero skips and zero compiler warnings/errors per
framework. All 31,223 baseline cases plus 120 new controls passed.
Immutable test blob `703159731160565b7f76f8f68ddd91f79022fc98`
remains unchanged. Final six-platform results are in PR183. Issue102
and broader PCRE, release and consumer migration gates remain open;
Grep remains last.
+
## Bounded zero-width chains before zero-minimum group repeats (PR184)

After merged183, the specialized optional-bracket UTF bitmap proof no
longer limits the middle prefix to two nodes. A leading positive
single-byte ASCII literal lookahead excludes CR even if the following
group repeat consumes nothing. Any number of supported word boundaries
or single-byte noncaseless ASCII literal assertions may precede the
zero-minimum, positive-maximum group repeat while each middle node,
the leading assertion, the repeat and the mandatory CR-excluding group
head fit the existing 1,000-visit budget. Cancellation is checked at
every middle node. Unrecognized nodes remain outside this proof; parser,
API, runtime limits and consumers are unchanged.

Pinned PCRE2 10.48 JIT retained LF, excluded CR and produced positive
JIT sizes across long boundary/assertion chains and five repeated-group
shapes. Three hundred three native full-capture fixtures / 606
strict+tolerant checks cover CRLF and ordinary subjects, including 900
boundaries. Successful-build [test-only RED568](https://github.com/uniblab/Icod.Expressions/actions/runs/42354718366),
Linux ARM64/net8: 101 new CRLF failures, 31,605 passes, zero skips and
no compiler warnings/errors. All 31,403 baseline cases and 202 new
controls passed. Immutable test blob
`aa4d1bb8415881799a630852b7eaa4c51b1dae04` remains unchanged.
Final six-platform results are in PR184. Issue102 and broader PCRE,
release and consumer migration gates remain open; Grep remains last.

## Wide scalar literal assertions before zero-minimum group repeats (PR185)

After merged184, the specialized optional-bracket UTF bitmap proof decodes
exactly one complete UTF-8 scalar in its leading positive caseful forward
literal assertion, and in each supported caseful middle literal assertion.
The leading scalar must exclude CR. Boundary and assertion visits still
charge the existing 1,000-visit budget, cancellation remains per node,
and each repeated-group child branch still needs a mandatory CR-excluding
head. Multi-scalar and caseless assertions, general lookaround bodies,
and unrecognized nodes remain outside this proof; parser, runtime, API,
limits and consumers are unchanged.

Pinned PCRE2 10.48 JIT retained LF, excluded CR and produced positive JIT
sizes for 105 assertion/group patterns. Three hundred fifteen native
full-capture fixtures / 630 strict+tolerant checks cover U+0100 and
U+1F642 leading lookaheads, wide middle lookaheads/lookbehinds, five
group shapes and CRLF/ordinary subjects. Successful-build
[test-only RED570](https://github.com/uniblab/Icod.Expressions/actions/runs/42369519072),
macOS ARM64/net8/9/10: 95 new CRLF failures and 31,926 passes per framework,
zero skips and no compiler warnings/errors. All 31,706 baseline cases
and 210 new controls passed. Immutable test blob
`feb7d68cb5aea9d6d071add8b58177ed9b80ad9d` remains unchanged.
Final six-platform results are in PR185. Issue102 and broader PCRE,
release and consumer migration gates remain open; Grep remains last.

## Multi-scalar literal assertions before zero-minimum group repeats (PR186)

After merged185, the specialized optional-bracket UTF bitmap proof reads
the first complete scalar of each caseful literal assertion. A leading
positive forward assertion must begin with a CR-excluding scalar; its
remaining literal bytes do not change CR exclusion at the entry. Middle
caseful literal assertions may likewise contain multiple scalars. The
same two-visit assertion charge, 1,000-visit bound, cancellation and
mandatory CR-excluding repeated-group head remain. Caseless/general
lookaround and unrecognized nodes remain outside this proof; parser,
runtime, API, limits and consumers are unchanged.

Pinned PCRE2 10.48 JIT retained LF and excluded CR for 200 lead/middle/
group combinations; a CR-first leading lookahead instead included CR.
Six hundred native full-capture fixtures / 1,200 strict+tolerant checks
cover ASCII/UTF mixed literals, five nullable-group shapes and CRLF/
ordinary subjects. Successful-build
[test-only RED572](https://github.com/uniblab/Icod.Expressions/actions/runs/42371394007),
Linux x64/net8/9/10: 150 new CRLF failures, 32,471 passes and zero skips
per framework, no compiler warnings/errors. All 32,021 baseline cases
and 400 new ordinary controls passed. Immutable test blob
`cb2aa7e1a2bbcc6c6c324eb9e3d16f8f17bc0484` remains unchanged.
Final six-platform results are in PR186. Issue102 and broader PCRE,
release and consumer migration gates remain open; Grep remains last.

## Positive class lookahead before zero-minimum group repeats (PR187)

After merged186, the specialized optional-bracket UTF bitmap proof admits
a leading positive caseful lookahead only when its body has one branch
and one direct CR-excluding scalar set or supported scalar class. The
class head is checked on a success-only visit checkpoint. The general
assertion's body and branch add two visits to the leading assertion and
repeat charge; middle zero-width nodes and the mandatory CR-excluding
repeated-group head keep their established checks. The 1,000-visit bound
and cancellation remain. Multi-node, nullable, predicate, negated or
CR-admitting lookahead bodies remain outside this proof; parser, runtime,
API, limits and consumers are unchanged.

Pinned PCRE2 10.48 JIT retained LF, excluded CR and produced positive
JIT sizes for 90 class/prefix/group patterns. CR-admitting `\s` and
`[B\r]` leads included CR in their native bitmaps. Two hundred seventy
native full-capture fixtures / 540 strict+tolerant checks cover six
leads, three zero-width prefixes, five repeated-group shapes and CRLF/
ordinary subjects. Successful-build
[test-only RED574](https://github.com/uniblab/Icod.Expressions/actions/runs/42374407725),
Linux ARM64/net8/9/10: 90 new CRLF failures, 32,801 passes and zero skips
per framework, no compiler warnings/errors. All 32,621 baseline cases
and 180 new ordinary controls passed. Immutable test blob
`fc233a1e9900250c0df2e418bb0152ee201006a6` remains unchanged.
Final six-platform results are in PR187. Issue102 and broader PCRE,
release and consumer migration gates remain open; Grep remains last.

## Middle class lookahead before zero-minimum group repeats (PR188)

After merged187, the specialized optional-bracket UTF bitmap proof reuses
the one-branch, one-head positive caseful class lookahead check at each
supported middle prefix position. Each middle class assertion charges
its opcode, body, branch and class-range visits on a success-only proof.
The leading assertion still excludes CR; word boundaries, literal
assertions, the 1,000-visit bound, cancellation and mandatory CR-excluding
repeated-group head keep their checks. Other lookaround bodies and
unrecognized nodes remain outside this extension; parser, runtime, API,
limits and consumers are unchanged.

Pinned PCRE2 10.48 JIT retained LF, excluded CR and produced positive
JIT sizes for 70 lead/middle/group patterns. Two hundred ten native
full-capture fixtures / 420 strict+tolerant checks cover two leading
assertions, seven middle prefixes, five repeated-group shapes and CRLF/
ordinary subjects. Successful-build
[test-only RED576](https://github.com/uniblab/Icod.Expressions/actions/runs/42442001453),
macOS ARM64/net8/9/10: 70 new CRLF failures, 33,031 passes and zero skips
per framework, no compiler warnings/errors. All 32,891 baseline cases
and 140 new ordinary controls passed. Immutable test blob
`368303ed4aad8d9ac59b0b827ac3ef8fb7b6d8aa` remains unchanged.
Final six-platform results are in PR188. Issue102 and broader PCRE,
release and consumer migration gates remain open; Grep remains last.

## Middle class lookbehind before zero-minimum group repeats (PR189)

After merged188, the specialized optional-bracket UTF bitmap proof admits
positive caseful direct class lookbehind at a supported middle prefix
position. Reuse the one-branch, one-head CR-excluding class check and charge
the opcode, body, branch and class-range visits on a success-only proof.
The leading assertion remains positive ahead; repeated-group head, word
boundaries, literal assertions, cancellation and 1,000-visit bound keep
their checks. Multi-node, nullable, negated, predicate and CR-admitting
lookbehind bodies remain outside this extension. No parser, runtime, API,
limits, consumer, package, release or workflow change.

Pinned PCRE2 10.48 compile/JIT retained LF and excluded CR for 60
lead/middle/group patterns, with positive JIT sizes and three native
subject checks each. One hundred eighty native full-capture fixtures /
360 strict+tolerant checks cover two leads, six lookbehinds, five group
shapes and CRLF/ordinary subjects. Successful-build
[test-only RED578](https://github.com/uniblab/Icod.Expressions/actions/runs/42447771075),
macOS ARM64/net8/9/10: 60 new CRLF failures, 33,221 passes and zero skips
per framework, no compiler warnings/errors. All 33,101 baseline cases
and 120 new ordinary controls passed. Immutable test blob
`62dd6b45d8a40c6064c67a02107ee7dfd55d9194` remains unchanged.
Final six-platform results are in PR189. Issue102 and broader PCRE,
release and consumer migration gates remain open; Grep remains last.

## Leading class lookbehind before zero-minimum group repeats (PR190)

After merged189, the specialized optional-bracket UTF bitmap proof reuses
the one-branch, one-head positive caseful class assertion check for a
leading lookbehind. It charges the assertion body, branch and class-range
visits on a success-only proof. Middle zero-width nodes, mandatory
CR-excluding repeated-group head, cancellation and 1,000-visit ceiling
keep their checks. Multi-node, nullable, negated, predicate and
CR-admitting lookbehind heads remain outside this proof; parser, runtime,
API, limits and consumers are unchanged.

Pinned PCRE2 10.48 compile/JIT retained LF and excluded CR for 90
lead/prefix/group patterns, with positive JIT sizes and three native
subject checks each. Two hundred seventy native full-capture fixtures /
540 strict+tolerant checks cover six leading lookbehinds, three middle
prefixes, five repeated-group shapes and CRLF/ordinary subjects.
Successful-build [test-only RED580](https://github.com/uniblab/Icod.Expressions/actions/runs/42452100008),
Linux ARM64/net8/9/10: 90 new CRLF failures, 33,461 passes and zero skips
per framework, no compiler warnings/errors. All 33,281 baseline cases
and 180 new ordinary controls passed. Immutable test blob
`b9f1c10f46ac791523fa8365f86426bbfcf3515b` remains unchanged.
Final six-platform results are in PR190. Issue102 and broader PCRE,
release and consumer migration gates remain open; Grep remains last.

## Leading literal lookbehind before zero-minimum group repeats (PR191)

After merged190, the specialized optional-bracket UTF bitmap proof admits
a leading positive caseful literal lookbehind containing exactly one
CR-excluding UTF scalar. The existing mandatory-literal proof checks its
scalar; the prior literal-ahead path and supported middle zero-width
prefixes remain. The mandatory repeated-group head, cancellation and
1,000-visit ceiling keep their guards. Multi-scalar, caseless or
CR-admitting leading literal lookbehind remains outside this extension;
parser, runtime, API, limits and consumers are unchanged.

Pinned PCRE2 10.48 compile/JIT retained LF and excluded CR for 60
lead/prefix/group patterns, with positive JIT sizes and three native
subject checks each. One hundred eighty native full-capture fixtures /
360 strict+tolerant checks cover four leading literals (including a
wide UTF scalar), three middle prefixes, five repeated-group shapes and
CRLF/ordinary subjects. Successful-build
[test-only RED582](https://github.com/uniblab/Icod.Expressions/actions/runs/42455740589),
macOS ARM64/net8/9/10: 60 new CRLF failures, 33,671 passes and zero skips
per framework, no compiler warnings/errors. All 33,551 baseline cases
and 120 new ordinary controls passed. Immutable test blob
`a932566cdf7b3d639b01251b3c3419e8544b5d9b` remains unchanged.
Final six-platform results are in PR191. Issue102 and broader PCRE,
release and consumer migration gates remain open; Grep remains last.

## Leading two-scalar lookbehind before zero-minimum group repeats (PR192)

After merged191, the specialized optional-bracket UTF bitmap proof
validates one or two scalars in a leading positive caseful literal
lookbehind, requiring each to exclude CR. The two-scalar scan is bounded;
existing middle prefix, mandatory repeated-group head, cancellation and
1,000-visit checks remain. Three-scalar, caseless and CR-containing heads
remain outside this extension. Parser, runtime, API, limits and consumers
are unchanged.

Pinned PCRE2 10.48 compile/JIT retained LF and excluded CR for 60
lead/prefix/group patterns with positive JIT sizes and three native
subject checks each. One hundred eighty native full-capture fixtures /
360 strict+tolerant checks cover four two-scalar lookbehinds (including
wide UTF), three middle prefixes, five repeated-group shapes and CRLF/
ordinary subjects. Successful-build
[test-only RED584](https://github.com/uniblab/Icod.Expressions/actions/runs/42465995689),
macOS ARM64/net8/9/10: 60 new CRLF failures, 33,851 passes and zero skips
per framework, no compiler warnings/errors. All 33,731 baseline cases
and 120 new ordinary controls passed. Immutable test blob
`a61b8e7cae700c8096d70594dc13dfa136f906c5` remains unchanged.
Final six-platform results are in PR192. Issue102 and broader PCRE,
release and consumer migration gates remain open; Grep remains last.

## Bounded leading literal lookbehind before zero-minimum group repeats (PR193)

After merged192, the specialized optional-bracket UTF bitmap proof scans
at most eight valid CR-excluding scalars in a leading positive caseful
literal lookbehind. The optional bracket, supported middle zero-width
prefixes, mandatory repeated-group head, cancellation and 1,000-visit
ceiling keep their guards. Longer, caseless and CR-containing lookbehind
heads remain outside this extension. Parser, runtime, API, limits and
consumers are unchanged.

Pinned PCRE2 10.48 compile/JIT retained LF and excluded CR for 90
lead/prefix/group patterns with positive JIT sizes and three native
subject checks each. Two hundred seventy native full-capture fixtures /
540 strict+tolerant checks cover six leading literals of three, four or
eight scalars (including wide UTF), three middle prefixes, five group
shapes and CRLF/ordinary subjects. Successful-build
[test-only RED586](https://github.com/uniblab/Icod.Expressions/actions/runs/42474722466),
Linux ARM64/net8/9/10: 90 new CRLF failures, 34,091 passes and zero skips
per framework, no compiler warnings/errors. All 33,911 baseline cases
and 180 new ordinary controls passed. Immutable test blob
`aac6226ccc391715a79c494fd0da30f46ff8d0c5` remains unchanged.
Final six-platform results are in PR193. Issue102 and broader PCRE,
release and consumer migration gates remain open; Grep remains last.

## CR-containing leading literal lookbehind guard (PR194)

After merged193, pinned PCRE2 10.48 compile/JIT retained LF and excluded
CR for 135 leading lookbehind/prefix/group patterns, with positive JIT
sizes and three native subject positions each. Yet `(?<=\\r)` can match
at the LF within CRLF. The existing CR exclusion in the bounded leading
lookbehind start-skip proof is necessary: admitting that lookbehind would
skip a valid LF candidate. The test-only full-suite run passed before a
production change, so this PR retains the guard and pins native full
match/capture parity as regression coverage.

Three hundred ninety distinct fixtures / 780 strict+tolerant checks
cover nine leading literals, three middle prefixes, five nullable group
shapes and CRLF/ordinary/positive subjects. The single-CR positive
subject duplicates its CRLF fixture and is listed once. Successful-build
[test-only run588](https://github.com/uniblab/Icod.Expressions/actions/runs/42477284630)
passed 34,571 cases per framework on Linux ARM64/net8/9/10, including
frozen214, with zero failed/skipped tests. The initial theory enumerated
15 duplicate IDs, removed before final CI. Final six-platform results
are in PR194. Issue102 and broader PCRE, release and consumer migration
gates remain open; Grep remains last.

## Single-scalar negative assertions before zero-minimum group repeats (PR195)

After merged194, the specialized optional-bracket UTF proof admits a
leading caseful negative literal assertion, ahead or behind, only when
its full body is one valid UTF scalar other than CR/LF. The optional
nullable group and mandatory CR-excluding continuation, supported
middle prefixes, cancellation and 1,000-visit ceiling keep their guards.
Explicit-newline, multi-scalar and caseless negatives remain outside
this extension. Parser, matcher, API, limits and consumers are unchanged.

Pinned PCRE2 10.48 compile/JIT retained LF and excluded CR for 120
lead/prefix/group patterns with positive JIT sizes and two native
subject positions each. Two hundred forty full-capture fixtures / 480
strict+tolerant checks cover six negative B/C/wide-UTF literals and two
explicit-newline controls, three middle prefixes, five nullable repeated
group shapes, and CRLF/ordinary subjects. Successful-build
[test-only RED591](https://github.com/uniblab/Icod.Expressions/actions/runs/42481376973),
Linux ARM64/net8/9/10: 90 new CRLF failures, 34,721 passes and zero skips
per framework, no compiler warnings/errors. All 34,571 baseline cases
and 150 new controls passed. Immutable test blob
`69929f9054b7249a34c631a9192568af41e7fbd8` remains unchanged.
Final six-platform results are in PR195. Issue102 and broader PCRE,
release and consumer migration gates remain open; Grep remains last.

## Bounded multi-scalar negative assertions before zero-minimum group repeats (PR196)

After merged195, the specialized optional-bracket UTF proof scans at most
eight valid CR/LF-excluding scalars in a leading caseful negative literal
lookahead or lookbehind. The optional group, supported middle zero-width
prefixes, mandatory CR-excluding repeated-group head, cancellation and
1,000-visit ceiling keep their guards. Longer, newline-containing or
caseless negative assertions remain outside this extension. Parser,
matcher, API, limits and consumers are unchanged.

Pinned PCRE2 10.48 compile/JIT retained LF and excluded CR for 120
lead/prefix/group patterns, with positive JIT sizes and two native
subject offsets each. Two hundred forty native full-capture fixtures /
480 strict+tolerant checks cover eight negative literals of two, three
or eight scalars (including wide UTF), three middle prefixes, five
nullable group shapes and CRLF/ordinary subjects. Successful-build
[test-only RED594](https://github.com/uniblab/Icod.Expressions/actions/runs/42511015475),
Linux ARM64/net8/9/10: 120 new CRLF failures, 34,931 passes and zero
skips per framework, no compiler warnings/errors. All 34,811 baseline
cases and 120 new ordinary controls passed. Immutable test blob
`f565c50e0a3c77e09c73d135a1767435051513fb` remains unchanged.
Final six-platform results are in PR196. Issue102 and broader PCRE,
release and consumer migration gates remain open; Grep remains last.

## Caseless negative assertions before zero-minimum group repeats (PR197)

After merged196, the specialized optional-bracket UTF proof accepts
caseless leading negative literal lookahead or lookbehind when its
bounded body contains at most eight valid CR/LF-excluding scalars.
The optional group, supported middle zero-width prefixes, mandatory
CR-excluding repeated-group head, cancellation and 1,000-visit ceiling
keep their guards. Longer and newline-containing negative assertions
remain outside this extension. Parser, matcher, API, limits and
consumers are unchanged.

Pinned PCRE2 10.48 compile/JIT retained LF and excluded CR for 210
lead/prefix/group patterns, including special S/K/Kelvin case folds,
with positive JIT sizes and two native subject offsets each. Four
hundred twenty native full-capture fixtures / 840 strict+tolerant checks
cover fourteen negative leads of one to eight scalars (including wide
UTF), three middle prefixes, five nullable group shapes and CRLF/
ordinary subjects. Successful-build
[test-only RED597](https://github.com/uniblab/Icod.Expressions/actions/runs/42602489913),
Linux ARM64/net8/9/10: 210 new CRLF failures, 35,261 passes and zero
skips per framework, no compiler warnings/errors. All 35,051 baseline
cases and 210 new ordinary controls passed. Immutable test blob
`cc504846e40f4c0e87184f92169738b813d3e4a8` remains unchanged.
Initial source-only [run598](https://github.com/uniblab/Icod.Expressions/actions/runs/42603410291)
passed the 70 no-middle CRLF cases but still failed 140 cases with
caseless middle literal assertions per framework. The final proof
permits those only beneath a validated caseless negative leading assertion.
Final six-platform results are in PR197. Issue102 and broader PCRE,
release and consumer migration gates remain open; Grep remains last.
+
## Caseless positive assertions before zero-minimum group repeats (PR198)

The bounded optional-bracket UTF proof now admits a leading caseless
literal positive lookahead or lookbehind before a zero-minimum group
repeat. The lookahead requires a valid CR-excluding first scalar; the
lookbehind validates its complete one-to-eight-scalar body and excludes
CR at every position. After a validated caseless literal lead, a bounded
caseless literal middle assertion may precede the repeat. The existing
mandatory CR-excluding repeated-group head, cancellation and 1,000-visit
ceiling remain in force. CR-containing lookbehinds and longer bodies
remain on the general path. Parser, matcher, API, limits and consumers
are unchanged.

Pinned PCRE2 10.48 compile/JIT retained LF and excluded CR for 150
lead/prefix/group patterns with positive JIT sizes and native match
spans on both CRLF and ordinary subjects. Three hundred full-capture
fixtures / 600 strict+tolerant checks cover ten positive lookahead and
lookbehind leads (including two-scalar, wide UTF, S/K folds), three
middle prefixes, five nullable group shapes and two subjects.
Successful-build [test-only RED600](https://github.com/uniblab/Icod.Expressions/actions/runs/42608674631)
on Linux ARM64/net8/9/10 failed exactly 150 new CRLF cases per framework,
with 35,621 passes and zero skips: all 35,471 earlier fixtures and 150
new ordinary controls passed. The test blob
`394c5ef84d35aabf2ecd5d7209e9a630143725d7` remains unchanged.
Six-platform final results are in PR198. Issue102 and broader PCRE,
release and consumer migration gates remain open; Grep remains last.
+
## Caseless positive classes before zero-minimum group repeats (PR199)

The optional-bracket UTF proof now admits positive class lookahead or
lookbehind under caseless mode, provided the single class/type body
passes the existing bounded CR-exclusion test. Caseless literal middle
assertions may follow such a validated lead. The mandatory CR-excluding
repeated-group head, cancellation and 1,000-visit ceiling retain their
existing guards. Negated or predicate-bearing sets, CR-containing ranges
and unsupported types remain outside this proof. Parser, matcher, API,
limits and consumers are unchanged.

Pinned PCRE2 10.48 compile/JIT retained LF and excluded CR for 270
lead/prefix/group patterns, with positive JIT sizes and correct native
spans on CRLF and ordinary subjects. Five hundred forty full-capture
fixtures / 1,080 strict+tolerant checks cover eighteen positive class
lookahead/lookbehind leads (ASCII ranges, wide UTF, types and S/K/Kelvin
folds), three middle prefixes, five nullable group shapes and two
subjects. Successful-build [test-only RED602](https://github.com/uniblab/Icod.Expressions/actions/runs/42619728071)
on macOS ARM64/net8/9/10 failed exactly 270 new CRLF cases per framework,
with 36,041 passes and zero skips: all 35,771 earlier fixtures and 270
new ordinary controls passed. Immutable test blob
`7007ff894cb5412391fe5871017e7f69ed601f84` remains unchanged.
Final six-platform results are in PR199. Issue102 and broader PCRE,
release and consumer migration gates remain open; Grep remains last.
+
## Caseless negative classes before zero-minimum group repeats (PR200)

The bounded optional-bracket UTF proof now admits leading negative
class lookahead or lookbehind, including caseless bodies, when a single
supported scalar set or type excludes both CR and LF. Negated and
predicate-bearing sets, newline-containing ranges, unsupported types
and compound assertion bodies remain outside this extension. The
existing mandatory CR-excluding repeated-group head, cancellation,
1,000-visit ceiling and bounded middle assertions retain their guards.
Parser, matcher, API, limits and consumers are unchanged.

Pinned PCRE2 10.48 compile/JIT retained LF and excluded CR for 270
lead/prefix/group patterns with positive JIT sizes and correct native
spans on CRLF and ordinary subjects. Five hundred forty full-capture
fixtures / 1,080 strict+tolerant checks cover eighteen negative class
lookahead/lookbehind leads (ASCII ranges, wide UTF, types and S/K/Kelvin
folds), three middle prefixes, five nullable group shapes and two
subjects. Successful-build [test-only RED604](https://github.com/uniblab/Icod.Expressions/actions/runs/42751357677)
on Linux x64/net8/9/10 failed exactly 270 new CRLF cases per framework,
with 36,581 passes and zero skips: all 36,311 earlier fixtures and 270
new ordinary controls passed. Immutable test blob
`d2a479cd1c70ec0679ca45ea1c0973ef63b42727` remains unchanged.
Final six-platform results are in PR200. Issue102 and broader PCRE,
release and consumer migration gates remain open; Grep remains last.
+
## Negative class middle assertions before zero-minimum group repeats (PR201)

The bounded optional-bracket UTF proof now permits a negative class
lookahead or lookbehind after a validated leading literal or class
assertion and before a zero-minimum repeated group. It reuses the
single-body class/type CR/LF exclusion from PR200 and charges the
middle opcode against the established 1,000-visit ceiling. Negated
or predicate-bearing sets, newline-containing ranges, unsupported
types and compound assertion bodies remain outside the proof. The
mandatory CR-excluding repeated-group head, cancellation and other
prefix guards remain. Parser, matcher, API, limits and consumers are
unchanged.

Pinned PCRE2 10.48 compile/JIT retained LF and excluded CR for 540
lead/middle/group patterns with positive JIT sizes and correct native
spans on CRLF and ordinary subjects. One thousand eighty full-capture
fixtures / 2,160 strict+tolerant checks cover six positive/negative
literal/class leads, eighteen negative class middle lookahead/lookbehind
forms (ASCII ranges, wide UTF, types and S/K/Kelvin folds), five nullable
group shapes and two subjects. Successful-build [test-only RED606](https://github.com/uniblab/Icod.Expressions/actions/runs/42754755781)
on Linux x64/net8/9/10 failed exactly 540 new CRLF cases per framework,
with 37,391 passes and zero skips: all 36,851 earlier fixtures and 540
new ordinary controls passed. Immutable test blob
`bfb521fcb21f206cd22d6742f3a36367148509e9` remains unchanged.
Final six-platform results are in PR201. Issue102 and broader PCRE,
release and consumer migration gates remain open; Grep remains last.

## Negative negated-set assertion corpus (PR202)

Six CR/LF-excluding negated sets in leading and middle negative lookahead and
lookbehind compile and preserve pinned PCRE2 10.48 capture spans over 1,680
fixtures. All 3,360 strict/tolerant comparisons passed without a production
change. The broader unsupported syntax and issue102 work remain open.

## Negated-range negative assertions before nullable groups (PR203)

The existing bounded proof now admits a leading or middle negative class
lookahead/lookbehind with one predicate-free negated scalar set and valid
nonempty excluded ranges. Native PCRE2 10.48 start-bitmap study skips negative
assertion bodies; their newline membership need not restrict admission.
The repeated group's mandatory CR-excluding head, single-body shape,
supported placements, middle-opcode charges, cancellation and 1,000-visit
ceiling remain. Each range is validated and charged before committing the
shared visit count. Positive assertion and mandatory-head classification,
source ExplicitNewline metadata, parser, matcher, API and limits are unchanged.

Original immutable test blob `2aba532d50a5b630c5180ab18f3b3d7399387489`
adds840 full-capture fixtures/1680 strict+tolerant checks for endpoint-implicit
CR/LF-excluding ranges. [Test-only run610](https://github.com/uniblab/Icod.Expressions/actions/runs/42760807042)
compiled cleanly on six native runners and failed420 CRLF cases per framework,
with40031 passes and zero skips. The separate48 native guard fixtures retain
ranges excluding onlyCR, onlyLF or neither. [Test-only run611](https://github.com/uniblab/Icod.Expressions/actions/runs/42762304905)
revealed24 additional CRLF misses and24 passing ordinary controls on the
inspected LinuxARM64/macOSARM64 frameworks, establishing that requiring both
newlines to be excluded would be too restrictive. Both test corpora remain
unchanged after RED. Total committed suite:40499 per framework.
See the PR203 design and final PR audit. Broader issue102, release qualification
and consumer migration remain open; Grep stays last.


## Negative scalar-class assertion bitmap entry (PR204)

The bounded negative-class ahead/behind proof now accepts every already-parsed
scalar class and nonempty scalar-set range/predicate union, regardless of
newline membership or negation. PCRE2 10.48 start-bitmap study skips these
negative assertion bodies before studying the repeated group's mandatory
CR-excluding head. Positive assertions, consuming-head classification,
source ExplicitNewline metadata, parser, matcher and limits are unchanged.
Range validation, per-range/per-predicate cancellation and visit charges,
supported leading/middle positions and the shared1,000-visit ceiling remain.

Pinned native10.48 JIT research passed2,560 compile/bitmap/JIT-size checks and
5,120 literal full-capture checks. All start bitmaps includeLF and excludeCR.
The immutable2,560-fixture blob `c6bc29aab5ea31f66ca486336c48d4c0529f7f46`
pins5,120 strict/tolerant comparisons across16 bodies, UCP/caseless modes,
leading/middle negative ahead/behind and five nullable group-repeat shapes.
[Test-only run613](https://github.com/uniblab/Icod.Expressions/actions/runs/42766233793)
on LinuxARM64 compiled cleanly and failed1,240 new CRLF cases per framework,
with41,819 passes and zero skips; all40,499 baseline tests and ordinary controls
passed. Expected final suite:43,059 per framework. Final CI is recorded in PR204.
Broader issue102 and PCRE/release qualification remain open; Grep stays last.


## Regular negative assertion body bitmap entry (PR205)

A bounded structural fallback now recognizes regular negative ahead/behind
bodies containing sequences, alternatives, capturing/noncapturing/atomic groups,
repeats, literals/classes/dot/boundaries and ordinary nested assertions. Native
PCRE2 10.48 start-bitmap study skips the body; the existing following repeated
group must still prove a mandatory CR-excluding head. Empty/nullable inner
paths need no consuming head. Calls, backreferences, conditions, anchors,
match-start reset and unknown/non-atomic assertion forms remain excluded.

The existing single-class path and exact charges remain. The new iterative
compiler-only walk charges bodies, branches, nodes, ranges, predicates and
literal-assertion scalars under cancellation, bounds visited and queued work
at1,000 and commits speculative costs only on complete success. Positive and
literal assertion admission, source metadata, parser, VM, API and limits stay.

Pinned native10.48 research passed2,720 compile/bitmap/positive-JIT-size checks
and5,440 literal full-capture comparisons. All bitmaps includeLF and excludeCR.
The immutable2,720-fixture blob `a6137a8f9f5a14fd9c9adce6509d385f2215cb96`
includes200-deep groups,64 alternatives, UCP/caseless and leading/middle
placements. [Test-only run615](https://github.com/uniblab/Icod.Expressions/actions/runs/42772123598)
on macOSARM64 compiled cleanly and failed1,360 new CRLF cases per framework,
with44,419 passes and zero skips. All43,059 baseline tests and ordinary controls
passed. Expected final suite:45,779 per framework. Final CI is recorded in PR205.
Broader issue102, PCRE/release qualification and Grep migration remain open.


## Pinned Unicode17 general categories

PR207 replaces host .NET/OS category queries with an immutable managed
Unicode17.0.0 table matching PCRE2 10.48. Explicit properties, category-derived
UCP/POSIX classes and UTF capture-name letters/digits share the same lookup;
non-ASCII subroutine numbers retain controlled InvalidSyntax diagnostics.
Packed ranges plus an ASCII fast path occupy16,592 data bytes, require no
runtime files/native dependency and add no VM state, charged work or limits.
Existing complements, caseless category widening, local ASCII policies, byte
projection, spans and cancellation remain shared.

The source-hash-checked C# generator and Linux byte-for-byte regeneration gate
make data ownership reproducible. Unicode permission terms ship in the package.
An independent native fingerprint covers all1,112,064 valid scalars; new JIT
fixtures cover recent assignments, complements/brackets, category capture
selection and UTF names. The test-only run620 built successfully: LinuxARM64
net8 had29 expected new failures,45,782 passes and all45,779 baseline cases
passed. The32-case test blob is
`18e46522c52a6750b83a147350e7b48ae64f65f3`.

This closes general-category version ownership only. Full Unicode fold tables,
scripts/binary properties, grapheme/class algebra and broader native optimizer
parity remain gates. No public API/release/consumer change; frozen35 intact and
Grep last. See `PCRE-UNICODE-CATEGORIES-DESIGN.md`,
`PCRE-UNICODE-CATEGORIES-PLAN.md` and `PCRE-ROADMAP-STATUS.md`.


## Pinned Unicode17 simple-case equivalence

PR208 replaces host Rune uppercase/lowercase round trips and special-case
patches with the complete Unicode17 C/S mapping used by PCRE2 10.48. The
existing Fold/Equals/CloseRanges paths retain scalar widths, byte-UCP
projection, local i/r, source metadata, cancellation and charged matcher work.
ASCII remains a direct fast path; 1,486 immutable packed mappings occupy
11,888 data bytes. There is no new VM state, runtime dependency or public API.

Independent native fingerprints cover semantic equivalence classes over all
1,112,064 valid scalars, both unrestricted and restricted, independently of
canonical key choice. A native JIT probe verified 1,656 existing comparison-path
cases. The 35-case test blob remains
`7f2927cdd90176f0cdee90867035bbc3c8803d73` after successful-build RED622:
Linux ARM64 had 17 expected new failures and 45,829 passes on each framework;
all 45,811 baseline cases passed. Live fixtures include new pairs, multi-member
sets, literals/brackets/ranges, backreferences/assertions, non-growing defaults
and malformed-UTF offsets/capture widths.

The hash-checked C# generator and Linux byte-for-byte gate make this data
reproducible; the existing packaged Unicode notice now covers both sources.
This closes default simple-case table ownership, not full-string F or optional
Turkic T modes. Remaining syntax, optimizer, API/release and consumer gates stay
open; frozen35 intact and Grep last. See `PCRE-UNICODE-CASEFOLD-DESIGN.md`,
`PCRE-UNICODE-CASEFOLD-PLAN.md` and `PCRE-ROADMAP-STATUS.md`.


## Unicode17 script properties

PR209 implements all 175 PCRE2 10.48 scripts and 344 normalized aliases: bare
names mean scx, with explicit sc/Script and scx/Script_Extensions namespaces,
colon/equal separators and existing loose names/complements. Native scx retains
primary membership plus extension bits, including Common/Inherited. Script
predicates ignore i/r/UCP/ASCII policy; byte projection uses the same semantics.

An explicit immutable PropertyValue flows through existing scalar class,
predicate and instruction metadata. Existing ScalarClass/ScalarSet opcodes,
scalar widths, captures/journal, Step and cancellation paths remain. Generated
per-script interval lookup has bounds checks and bounded binary search; no
runtime dependency or new saved state. The C# generator validates three source
hashes and a complete independent native fingerprint over 1,112,064 scalars;
Linux CI verifies exact regeneration, with the Unicode notice in the package.

Native probes verified 7,092 alias/literal and 1,227,800 script-boundary JIT
outcomes. Test data pins 3,508 boundaries, every alias, all256 byte values,
nested grammar/captures, case/ASCII independence, malformed UTF and resources.
The corrected test-only RED626 built successfully: Linux ARM64 had42 new
failures and45,851 passes on net8, with all45,846 baseline cases passing. Old
unfrozen pending-Greek rows now retain rejection coverage for binary/Bidi
families. New/updated test blobs stay unchanged after that RED.

Binary/Bidi properties, script runs, graphemes/class algebra and broader
optimizer/API/release/consumer gates remain open. Frozen35/frozen214 preserved,
no public API/version/publication/migration change, Grep last. See
`PCRE-UNICODE-SCRIPTS-DESIGN.md` and `PCRE-UNICODE-SCRIPTS-PLAN.md`.


## Unicode17 binary and Bidi properties

PR210 completes the PCRE2 10.48 property catalogue:57 binary predicates,
106 normalized binary aliases and23 short Bidi_Class values. Binary names
are bare; BC/Bidi_Class accept colon/equal, and native bare bidi-prefixed
values are preserved. InCB means native non-None membership, and
Bidi_Mirrored follows the native mirroring list. Unassigned Bidi ranges
retain ordered @missing defaults. Long Bidi values, Is/block names,
Boolean assignments and wrong namespaces stay explicitly unsupported.

Binary/Bidi predicates reuse immutable PropertyValue and the existing
ScalarClass/ScalarSet opcodes. They ignore UCP/i/r/ASCII policy, use the
same byte projection and preserve complements, capture widths, journals,
Step and cancellation. Bounded per-property interval searches require no
runtime files, host Unicode service, native fallback or new VM state.
The C# generator checks seven pinned source hashes and the native9-byte
record fingerprint over all1,112,064 valid scalars; Linux CI verifies exact
regeneration. The package includes the expanded Unicode notice.

The test-only RED629 compiled without warnings/errors and had47 expected
feature failures/45,901 passes on each framework. New immutable tests and
fixtures pin7,706 native state boundaries, all aliases/Bidi forms, every
byte value, nested grammar, malformed UTF, resources and diagnostic spans.
Independent native JIT probes verified1,232,960 boundary/capture outcomes
and47 grammar cases. See PCRE-BINARY-BIDI-PROPERTIES-DESIGN.md and
PCRE-BINARY-BIDI-PROPERTIES-PLAN.md for fingerprints and fixed test blobs.

This closes native property coverage, while script-run groups, graphemes,
class algebra, optional Turkic folding and broader optimizer/UTF16/API,
performance/release/consumer gates remain open. Frozen35/frozen214 stay
intact; no public API/version/publication/migration change and Grep last.


## Atomic grapheme clusters

PR211 adds byte/UTF8 \X with PCRE2 10.48's pinned Unicode17 break data.
Each cluster is atomic and nonempty; outer repeats retain whole-cluster
backtracking and existing captures/calls/assertions. Native RI prefix parity,
EP/Extend/ZWJ state and table exceptions are preserved, including RI before
Extend and Prepend before EP. Native Indic-conjunct GB9c segmentation is not
implemented by the oracle; this is native parity, not a blanket UAX claim.
Bracket \X is invalid and consuming lookbehind use remains unsupported.
Fixed/variable-behind nested forward assertion bounds retain native behavior.

A dedicated node/opcode consumes validated bytes within the current fragment
and scope. Every scalar/prefix probe uses the shared Step primitive with its
ref work counter; no Step delegate or per-atom allocation is introduced. A
two-offset, input-derived RI memo avoids quadratic contiguous-prefix scans
without adding saved VM state. Unicode classification uses128 ASCII entries
and2,035 packed range ends (8,268 data bytes), independent of host services.
Hash-checked C# generation, exhaustive native fingerprint and Linux exact
regeneration qualify the data; the package includes its Unicode notice.

The initial successful-build RED631 had53 expected feature failures and45953
passes. Scope-complete RED632 had54 expected failures/45955 passes, zero skips
on every framework; all45948 baseline cases pass. New test/data blobs remain
fixed after RED. Fixtures cover3535 Unicode boundaries, all225 type pairs,
3375 triples at every scalar start, all65536 byte pairs, atomicity/repeats,
captures and nested grammar, malformed UTF, scope bounds, work/cancellation
and linear RI scans. Standalone native JIT probes verified222675 boundary/
capture outcomes and41 grammar/policy outcomes. See PCRE-GRAPHEME-DESIGN.md
and PCRE-GRAPHEME-PLAN.md for the fingerprint and execution evidence.

Extended class algebra, script-run groups, optional Turkic folding, other
pending syntax, optimizer/resource, UTF16/API and realistic performance/
release/consumer gates remain. Frozen35/frozen214 preserved; no public API,
version, publication or migration change. Grep stays last.

The existing malformed-UTF fixture exposed a pinned JIT mode distinction.
MatchInvalidUtf uses a different backward RI context view: literal RI prefixes
are not counted, while26 mapped CJK prefixes can count. The compatibility
predicate preserves that view, with normal forward classification and charged
backward probes. Strict mode keeps its contiguous RI memo. Eight additional
immutable regressions were observed failing in successful-build RED634 before
the correction (9failures/46008passes,46017total). Native probes qualify every
valid prefix scalar and672 compound-prefix cases.


## Perl extended class algebra (PR212)

The internal byte/UTF8 compiler supports Perl `(?[...])` as one scalar:
operand complement, intersection, union (`|`/`+`), subtraction, symmetric
difference and grouping. Native precedence is `!`, then `&`, then equally
binding left-associative `| + - ^`. Operands reuse escaped literals,
shorthands, pinned Unicode predicates, direct POSIX names and ordinary brackets.
Spaces/tabs are ignored locally; other ordinary pattern trivia is not.

Postfix terms avoid recursive parsing/evaluation. Native class nesting is
bounded at15; terms share the4096 compilation budget, including byte-mode
expressions. Operand case closure precedes algebra. Byte expressions project
to a precomputed bitmap with native mixed-Any/property bitmap behavior. UTF
evaluation uses one lazy scratch array per Match, charging every term and
interval/predicate probe to the existing work/cancellation contract. Captures,
frames, malformed fragments and width-one lookbehind use existing machinery.

PcrePerlClassAlgebraReferenceTests and fixed native fixture masks qualify308
expressions ×8 profiles ×256 values and12 expressions ×4 profiles ×7592
Unicode boundary scalars, plus52 total test cases covering grammar, source
offsets, folding, capture/repetition/behind, malformed UTF and exact limits.
The tests-first successful-build RED636 retained all46017 baseline passes.
See PCRE-PERL-CLASS-ALGEBRA-DESIGN.md and PCRE-PERL-CLASS-ALGEBRA-PLAN.md.

UTS#18's ALT_EXTENDED_CLASS activation remains a separate frontend increment.
General start-optimizer equivalence (issue102), script-run groups, UTF code-unit
escape, directives/verbs/callouts, UTF16/API, realistic performance and release/
consumer gates remain open. GNU/BRE/ERE, frozen35/frozen214 and public API stay
intact. Grep stays last; no publication/migration change.


## Opt-in UTS#18 class syntax (PR213)

PcreByteOptions.AltExtendedClass defaults to false. When enabled, brackets
outside Perl `(?[...])` accept nested brackets, implicit union and doubled
`||`, `&&`, `--`, `~~` operators. Implicit union binds first. Explicit
operators at one level must have one kind and associate leftward; nested levels
can choose other operators. Leading ^ complements the whole class. Space/tab
handling follows x/xx scope, quotes remain literal, and Perl leaves keep their
ordinary bracket grammar. The option is internal; public API is unchanged.

A bounded heap-frame frontend emits the existing postfix algebra, preserving
contiguous raw item runs for native byte Any/property behavior. Ordinary
classes without algebra return through the original readers to retain source/
optimizer metadata. Native nesting15, shared compilation4096 and charged UTF
term/range/predicate work/cancellation use the existing contracts and evaluator;
byte expressions remain precomputed bitmaps. Captures/repeats/width-one behind
and malformed-UTF fragments reuse the existing engine.

PcreUtsClassAlgebraReferenceTests and fixed native masks qualify235 expressions
×8 profiles ×256 values plus12 expressions ×4 profiles ×7592 Unicode boundary
scalars. Focused cases pin activation, implicit precedence, operator mixing,
ranges/quotes/POSIX, Perl isolation, original byte diagnostics and limits.
Successful-build RED639 retains46069 baseline passes with58 expected failures.
See PCRE-UTS-CLASS-ALGEBRA-DESIGN.md and PCRE-UTS-CLASS-ALGEBRA-PLAN.md.

Script-run groups are the next syntax increment. UTF code-unit escape, remaining
directives/verbs/callouts, general optimizer/resource conformance (issue102),
UTF16/public API, realistic performance and release/consumer gates remain open.
No new package, production fallback, GNU behavior or frozen35/frozen214 change.
Grep stays last; the user owns publication/migration.


## Script-run groups (PR214)

The internal byte/UTF8 engine supports `(*script_run:...)`/`(*sr:...)` and
`(*atomic_script_run:...)`/`(*asr:...)`. Validation checks the consumed span
after the body. Empty/single-scalar spans accept Unknown; longer spans reject
it, intersect neutral-free primary/extension maps, preserve native directional
Han transitions and require decimal digits from one ten-character family.
Validation uses actual subject properties, independently of i/r/UCP/ASCII flags.
ASR commits an atomic body before validation, as `(*sr:(?>...))` does.

The wrapper is transparent to parser tree rewrites and behind widths. Entry
marks use the existing rollback/call journal; failure retries ordinary body
choices, and recursive returns restore outer starts. The scanner has no scan
allocations and charges scalar decode/classification to shared work and
cancellation. Script marks share the256 compiled-state budget and4096 compile
budget. Empty-fragment and explicit-newline metadata traverse the wrapper;
other start-optimizer proofs remain conservative.

GeneratePcreUnicodeScripts now derives301 run descriptors and1892 start ranges
from its existing source hashes and native full-scalar fingerprint. Three ulong
words hold each script map. Pinned category ranges identify all77 decimal
families, including adjacent families and non-hex-aligned starts. Exact C#
regeneration and independent native masks qualify the added data paths.

PcreScriptRunReferenceTests cover301 descriptor pairs,5111 boundaries in both
orders against20 contexts,77x77 decimal-set pairs at zero/nine endpoints,
20-cubed context triples and all256x256 byte pairs. Focused tests pin aliases,
rollback, recursion, ASR placement, captures/behind, invalid UTF, original spans
and limits. RED642 keeps46127 baseline passes;64 new cases fail as expected
and one quoted-literal case already passes. Tests/data remain fixed after RED.
See PCRE-SCRIPT-RUN-DESIGN.md and PCRE-SCRIPT-RUN-PLAN.md for details.

UTF code-unit escape, remaining directives/verbs/callouts, general optimizer/
resource conformance (issue102), UTF16/API, realistic performance and release/
consumer qualification remain open. Frozen35/frozen214, GNU and public API are
unchanged; no new package or production fallback. Grep stays last.

## Immediate successful scope termination

PR215 adds exact no-argument ACCEPT, including quantification, lexical capture
closes, committed assertion/call scopes and ordinary root NOTEMPTY fallback.
Direct ACCEPT terminates consumed width scans; global errors still validate.
Native JIT ignores synthetic closes inside calls; native non-atomic assertions
with ACCEPT use interpreter fallback. Named ACCEPT stays pending. See
[design](PCRE-ACCEPT-VERB-DESIGN.md) and `PcreAcceptVerbReferenceTests`.

## Scope-aware PRUNE cuts

PR216 supports exact no-argument PRUNE. It cuts only when failure backtracks
to its saved frame, using active scope boundaries and existing guard failure.
Native JIT selected callee values completed before PRUNE survive forced call
failure; right-side values roll back first. Zero-width behind scans continue
past PRUNE. Direct repeats are invalid; grouped repeats are ordinary. Named
forms stay pending. See [design](PCRE-PRUNE-VERB-DESIGN.md) and
`PcrePruneVerbReferenceTests`.

## SKIP restart and PRUNE comparison control

PR217 adds exact no-argument SKIP using shared cut guards and a root restart
integer. Strictly forward targets retry directly, including CRLF midpoint and
fragment end/EOF; equal/backward targets retain normal advancement. The frozen
PRUNE comparison additionally qualifies a bounded two-root-branch literal PLUS
cache and same-literal fallback, without widening general optimizer claims.
See [design](PCRE-SKIP-VERB-DESIGN.md) and
`PcreSkipVerbReferenceTests`. Named forms/MARK remain pending.

## PR218: no-argument COMMIT and first-code-unit entry

Exact COMMIT shares the existing delayed PRUNE/SKIP cut path and local call,
negative/conditional assertion catches. At root, reaching its frame ends the
entire search with normal no-match. Later verbs and successful atomic scopes
can discard it before it triggers. Selected pre-verb callee captures and
right-side rollback retain the pinned JIT behavior. Direct quantifiers reject;
behind width is zero without hiding unbounded tails. All three cuts share the
1024 saved-frame budget, compilation expansion and work/cancellation accounting.

Native first-code-unit entry can skip candidates before a leading COMMIT.
The bounded proof permits one root branch, four group levels and 1000 visits
through ordinary groups, atomic wrappers, required literal repeats, positive
lookahead, empty groups and K; native first-unit study skips leading positive
lookbehind bodies before the required outer literal. It stops at alternatives,
optional repeats, classes, calls, anchors and other assertions. Case-sensitive encoded first
bytes and simple ASCII caseless pairs qualify; Unicode K/S complex casesets
without r and other non-ASCII caseless heads do not. Wider optimizer parity
remains issue102. Every prefix check charges shared work/cancellation.

UTF uses the encoded first byte, so COMMIT/é can trigger at ê or a malformed C3
lead byte. The existing synthetic/gap fragment protocol admits these candidates
without allowing invalid-byte consumption. Root termination is global across
fragments. The existing literal-PLUS/PRUNE/single suffix/same-literal fallback
cache proof also permits one directly preceding COMMIT, discarded by PRUNE.

The first tests-only attempt652 found three duplicate InlineData rows; only
those redundant rows were removed before the clean checkpoint. RED653
run43025191022/LinuxARM64 compiled net8/9/10 with zero warnings/errors and
46647 cases each:142 expected missing-COMMIT failures,46505 passes,0 skips.
No prior fixture or native literal oracle failed. All150 unique new tests freeze
at blob`fba19c8e0627f89056b16302538e3c7da3639b9d`. Independent native exports
pin132 unique literal/profile cases; selected malformed-UTF/CRLF subjects test
every byte offset. Exact1024/1025 and mixed1023/1026 saved-frame cases, expansion,
work limits and cancellation are covered. See PCRE-COMMIT-VERB-DESIGN.md and
PCRE-COMMIT-VERB-PLAN.md.

Named verbs/MARK, THEN, remaining directives/callouts, UTF C, general optimizer/
resource conformance, UTF16/API and realistic consumer performance/release stay
open. Grep stays last; GNU/BRE/ERE, frozen35 and frozen214/120 remain intact.

## PR219: lexical THEN alternatives and scoped cuts

Exact no-argument THEN saves a delayed cut to its nearest lexical alternation.
It discards choices inside the current branch, then retains just the next-
branch frame; a last branch instead retains the group's entry depth and permits
earlier outer retries. Single-branch wrappers and conditional yes/no arms do
not create targets. All assertions, including standalone positive/non-atomic
ones, catch THEN; separately compiled callees cannot target caller alternatives.
With no owner/catch it behaves as root PRUNE. Atomic success and later cuts
can discard THEN; capture/K rollback and selected callee transactions stay shared.

In a program containing exact THEN, each compiled alternation receives one
journaled stack-depth checkpoint; ordinary programs retain their bytecode and
resource use. Compiler ownership carries a mark slot and has-next flag, resets
for assertions/callees and restores before conditional yes/no arms. The VM
captures an absolute live-frame boundary clamped to active scope guards. The
checkpoint reuses StateKind.Mark rollback and callee restore, with no new arena
or extra saved guard. Checkpoints share the256 marks with script/progress state;
all four cuts/ordinary choices share1024 frames and4096 compiled expansion.
Every checkpoint, marker, scope walk and discarded frame charges shared work/
cancellation. Direct quantifiers reject; behind width is zero without hiding
its tail. The bounded COMMIT first-unit proof also crosses leading THEN.

RED656 run43031808664/LinuxARM64 built net8/9/10 with zero warnings/errors:
46814 cases each,159 expected missing-THEN failures,46655 passes,0 skips.
No earlier fixture or native literal oracle failed. All167 new cases remain
fixed at blob`c60d5da76744c4de93d54ffb6be8103c96538052`. Independent native
exports pin149 literal cases (146 byte/UTF,3 UTF-only), plus plain1024/1025 and
mixed1024/1028 frame subjects and shared mark boundary subjects. All selected
malformed-UTF/CRLF byte offsets, resource/cancel paths, roots/nesting/last branches,
outer retries, conditionals, lookarounds/behind, recursion/selected returns,
atomic/repeat/script/grapheme, mixed verbs, K/ACCEPT/options/quotes are covered.
See PCRE-THEN-VERB-DESIGN.md and PCRE-THEN-VERB-PLAN.md.

Named verbs/MARK, remaining directives/callouts, UTF C, wider optimizer/resource
conformance (issue102), UTF16/API and realistic consumer performance/release
remain open. Grep stays last; GNU/BRE/ERE and frozen35/frozen214/120 stay intact.

The final capture qualification distinguishes lexical traps from forced call
quit even when their numeric target depths coincide: owned THEN uses ordinary
rollback; unowned call cuts retain selected pre-verb values. Native JIT also
uses call-local quit for unowned THEN inside a callee atomic positive assertion.
Outside calls and in negative/conditional/non-atomic assertions, catches remain
local. NO_JIT probes show the interpreter distinction. The saved-frame kind
records ownership without another arena or saved frame.

CI657 narrowed the original167 cases to one selected-capture mismatch. A
separate eight-row PcreThenReturnedCaptureReferenceTests file then exposed five
additional owned-trap rollback failures in clean RED658 (run43033445743,
LinuxARM64 job130566254506):46822 total,6 failures,46816 passes,0 skips and zero
warnings/errors on net8/9/10. Original blob c60d5da76744c4de93d54ffb6be8103c96538052
stays fixed; the eight regressions freeze at4bdcbd6448fb873e29f3ad5dfdfe3679187074f4.
All175 new cases are now fixed before the production capture correction.
