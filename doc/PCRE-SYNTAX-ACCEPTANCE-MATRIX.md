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
