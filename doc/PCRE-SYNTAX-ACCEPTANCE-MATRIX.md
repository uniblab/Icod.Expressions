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
| Ordinary `[ ]` ranges, negation, shorthand unions, 14 POSIX names | Supported subset in byte and UTF modes; nested/extended class algebra pending, rejected | Bracket and POSIX tests; Unicode table/version reconciliation |
| `\p`/`\P` general categories and special properties | Byte/UTF categories plus Any/Xan/Xps/Xsp/Xwd/Xuc; loose names, P/caret complements and property predicates independent of UCP/ASCII/i/r. Category-prefixed aliases remain a library extension for categories only. UTF class source operands/endpoints preserve CR/LF flags before folding, including the native negated-singleton exception. Scripts/binary properties remain pending, rejected | `PcreBytePropertyReferenceTests`, `PcreSpecialPropertyReferenceTests`, `PcreClassNewlineMetadataReferenceTests`, `PcreUtf8GeneralCategoryTests`; see `PCRE-SPECIAL-PROPERTIES-DESIGN.md`; Unicode-version and byte search-optimization parity remain gates |
| `* + ? {n,m}` greedy/lazy and possessive forms | Supported subset, bounded expansion and work; direct lookaround repeats use finite PCRE2 bounds (unlimited maximum becomes minimum plus one) | Counted-repeat, atomic and `PcreAssertionRepeatReferenceTests`; capture growth, optional rollback, zero counts, UTF offsets and resource limits; stress limits against consumer workloads |
| `(?>...)` atomic group | Supported subset, including exact lowercase `(*atomic:...)` | `PcreAtomicTests`, `PcreGroupAliasReferenceTests` |
| Bare decimal, absolute and signed relative numeric `\gN`/`\g{N}` backreferences | Default reference/octal disambiguation within 256 captures; braced g accepts edge space/tab. Nonrecursive subroutine forms are distinct calls, described below | Backreference/capture tests, `PcreNumericEscapeReferenceTests`; current-branch numbering, leading-8/9 forward references and entire decimal runs |
| Named captures and backreferences | Three declaration forms, five reference forms; unique names alias numbered slots, J permits duplicate names whose references select the first participating slot in declaration order without comparison fallback, forward references, case-sensitive names and a 128-byte name limit. UTF letters/digits use runtime categories. Nonrecursive subroutines are described below; bracket-literal `\g`/`\k` consume only the escaped letter and use ordinary class rules | `PcreNamedCaptureReferenceTests`, `PcreDuplicateNameReferenceTests`; all captures, rollback, UTF/invalid-UTF offsets, name grammar and boundaries; see `PCRE-NAMED-CAPTURES-DESIGN.md` and `PCRE-DUPLICATE-NAMES-DESIGN.md` |
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
| No-argument `(*FAIL)` / `(*F)` | Unconditional zero-consumption failure through shared rollback; nonrepeatable directly, grouped repeats ordinary; direct failure ends consumed-width branch and suppresses nested behinds in its width tail, while global validation/full lexical metadata remain | `PcreFailureVerbReferenceTests`; see `PCRE-FAILURE-VERBS-DESIGN.md`; captures/scopes/widths, malformed offsets, fragments and resource bounds; arguments/other verbs pending |
| Ordinary bracket `\g`/`\k` | Literal g/k only; following names, delimiters and digits remain class source; outside reference/call grammar unchanged | `PcreBracketReferenceEscapeTests`; see `PCRE-BRACKET-REFERENCE-ESCAPES-DESIGN.md`; ranges/folding/quotes/widths/captures and original-byte/resource protocols |
| `\cX` control escapes | Printable ASCII operand20..7E, a..z uppercase then XOR40; byte/UTF literals, classes/ranges, assertions and widths share existing paths; source delimiters and x/xx trivia are consumed as operands | `PcreControlEscapeReferenceTests`; see `PCRE-CONTROL-ESCAPES-DESIGN.md`; original byte diagnostics, CR/LF metadata, quotes, folding, offsets and work/cancellation |
| Byte `\C` | Any one byte including NUL/newlines/high bytes; existing ALLANY lowering, width1 and byte prefix proof. Invalid in brackets; UTF C remains explicitly pending | `PcreByteCodeUnitReferenceTests`; see `PCRE-BYTE-CODE-UNIT-DESIGN.md`; offsets, captures, behind, modes and resource gates |
| `\X`, other escapes | Pending, rejected; no literal reinterpretation | Representative compile-diagnostic assertions; prioritize by actual Grep patterns |
| `(?#...)` and x/xx line comments | Supported at token boundaries, including quantifier/suffix joins and fixed assertion bodies. Quotes/escapes and ordinary classes retain literal text; LF/ANYCRLF/CR/NUL/CRLF/ANY govern line termination | `PcreExtendedOptionReferenceTests`; original byte diagnostics and pinned whitespace/range rules |
| `(?[...])` extended classes | Pending, rejected | Representative compile-diagnostic assertions; separate class algebra design |
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
the remaining gaps include Unicode table parity, character-set algebra and other unsupported syntax.
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

## Finite optional prefixes before UTF starter classes

PcreUtfFiniteOptionalStartClassReferenceTests pins general starter-class entry
after supported finite optional primitives. HasUtfCrLfStartClass reuses the
nullable-prefix walker with allowUnboundedOptionalRepeats equal to selected
PLUS presence; other callers default true. Finite repeats keep existing atom,
CR-exclusion,UCP and source-sensitive caseless bitmap guards. Unbounded repeats
still require both that flag and the existing BlocksSkip/owner-identity rule.
This removes the duplicate zero/boundary scan without changing its accepted
kinds,costs,retained state or exhausted-branch rejection. Every-alternative
CR exclusion/at-least-one-LF,required-repeat/wide PLUS/range guards and1000-visit
budget/cancellation remain. No new group continuation or recovery owner.
No parser/emitter/matcher/Step/VM/journal/API/limit/workflow/package/dependency/
consumer/CommandFramework changes; all resource ceilings unchanged.
297 new cases/full24244 preserve all23947baseline expectations/frozen35.
Native6611 bounded JIT checks (560strict/tolerant full-capture,6048six-newline/
UCP/every-offset,3long ordinary controls) cover greedy/lazy/possessive queries/
bounded counts,literals/types/sets/caseless sources,mixed zero/boundary prefixes,
wrappers/required repeats/alternatives/dependencies and exclusions. Existing
PLUS/primitive-owner controls,strict validation/fragments/caller boundaries,
exact/exhausted/observed work,cancellation/default budget stay. Long ordinary
controls claim no bitmap parity past1000.
Tests-only run497/Linux ARM64/net8 observed166new-class failures/24078passes/
0skips,all23947baseline+131new controls passing,0compiler warnings/errors.
Tests unchanged. See PCRE-UTF-FINITE-OPTIONAL-START-CLASS-DESIGN.md and
PCRE-UTF-FINITE-OPTIONAL-START-CLASS-PLAN.md; recent index gains one row.
Keep102/unsafe100 and broader optional/assertion/predicate/group/required-copy/
unselected wide PLUS/root-alternative bitmap availability open. No full PCRE/
release/Grep readiness claim. Publication/all known migrations precede CF
removal; Grep last.
