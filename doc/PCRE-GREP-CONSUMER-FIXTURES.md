# Grep `-P` consumer fixture inventory

**Status:** initial frozen consumer floor for issue #7, based on `Icod.Grep` commit `2ef1c11f11b77eb876242eb0c92a88d20857db64` (`tests/Grep.Tests/src/PcreTests.cs`) and its 1.6.0 workload reports. It is not full PCRE2 syntax coverage or approval to migrate Grep.

Grep constructs one PCRE.NET 1.6.0 expression per pattern. In a UTF-8 locale it requests UTF, UCP, malformed-UTF matching, and ASCII `\d`; in C/POSIX it matches 8-bit bytes. `tests/Pcre/PcreGrepConsumerFixtureTests.cs` applies exactly these options to the pinned reference and the internal byte matcher on the same source bytes. Expected source-byte spans are stated independently of either engine.

| Grep test | Engine fixture or existing gate | Work remaining in Grep |
|---|---|---|
| `SupportsLookbehindAndBackreferences` | `utf-lookbehind`, `utf-backreference`, `utf-backreference-miss`; numbered capture reference tests | Record selection and output bytes. |
| `KeepsBackslashDAsciiUnderUtf8Ucp` | `utf-ascii-digit-miss`, `utf-posix-digit`, `utf-digit-override` | Multiple records and locale selection. |
| `SupportsUnicodePropertiesInUtf8Locale` | `utf-letter-property` | Unicode-version parity across target frameworks. |
| `PreservesByteModeInCLocale` | `byte-high` | Locale and byte-stream selection. |
| `SupportsWordAndLineRegexpSelection` | Existing boundary, anchor, dot and option tests | Grep's `-w`/`-x` wrappers and selection policy. |
| `SupportsNullDataDotAllAcrossEmbeddedNewlines` | `utf-dotall-embedded-lf` | NUL record framing and output delimiter. |
| `PreservesEmbeddedNulPatternsFromPatternFiles` | `byte-nul-pattern` | Pattern-file parsing and NUL record policy. |
| `SuppressesMalformedUtf8OutputUnlessTextIsForced` | `utf-malformed-subject` | Binary-output suppression and `-a`. |
| `CooperatesWithOnlyMatchingIgnoreCaseAndColor` | `utf-caseless-lookbehind`, `byte-subsequent-offset` | `-o` iteration, coloring, output formatting. |
| `InvalidPatternReturnsUsageError` | `invalid-group`, plus existing compile diagnostics | Grep's status 2 and stderr text. |

## Syntax boundary at this checkpoint

| Status | Concrete examples | Verification |
|---|---|---|
| Supported subset | Grep's literal lookbehind, single-digit numbered backreference, `\p{L}`, POSIX digit class, ASCII `\d` and `(?-aD)`, leading `(?s)`, byte NUL/high byte | Consumer fixture table and prior PCRE reference tests. |
| Supported internal syntax extensions | Any/Xan/Xps/Xsp/Xwd/Xuc properties share existing scalar predicates and UTF source class metadata preserves CR/LF continuation; named/decimal references and branch-reset groups reuse numbered slots; local J allows duplicate names whose references select the first participating declaration, with each lookup sharing the work/cancellation budget; ordinary braces use literal nodes unless the entire counted grammar matches, with shared repeatability checks and strict explicit braced tokens; octals reuse literal nodes; local i/m/s scopes compile comparison settings; n suppresses unnamed slots and U selects repeat preference; x/xx/comments add source-local lexical rules; empty/reset option headers reuse compile-time scopes and preserve U/J/ASCII restrictions; local/combined ASCII restriction masks select d/s/w, word boundaries and POSIX policies with native aP/aT overlap and group restoration; local r metadata restricts fold crossings across literals/ranges/references/assertions, with independent UCP POSIX/property casing; atomic-group and atomic-lookaround aliases share existing header/IR/VM paths; non-atomic positive assertions retain retries with bounded integer scope contexts and shared journal rollback; recursion conditions test only the current active call number, preserving global bare-name capture precedence and shared branch commitment; capture-participation conditions select one branch through immutable numbered targets and shared capture state; atomic assertion conditions use shared guard/scope transactions and preserve matching-body captures for either polarity; parenthesized subroutine return lists retain selected nested captures through the shared journal with global/duplicate resolution; recursive/whole-pattern calls reuse the same bounded guards/journal with JIT-compatible capture-changing zero progress; subroutine calls use first definitions, shared out-of-line bodies, integer guards and journaled capture restoration with non-atomic retries; bounded consumed subroutine calls in lookbehind reuse the shared heap width dependencies and existing fixed/variable reverse paths; static DEFINE/VERSION conditions retain global declarations and validation while selecting one Jump path, with compatibility version10.48; general atomic lookahead adds shared-VM guard/capture transactions; direct assertion repeats normalize unlimited maxima to minimum plus one and preserve iteration captures; general fixed-length lookbehind adds branch-specific character rewind; bounded-variable bodies add longest-first retries, min/max analysis and pinned JIT scoped ends; consumed references resolve full-pattern capture-body lengths with lexical/cycle controls | `PcreSpecialPropertyReferenceTests`, `PcreClassNewlineMetadataReferenceTests`, `PcreNamedCaptureReferenceTests`, `PcreBranchResetReferenceTests`, `PcreNumericEscapeReferenceTests`, `PcreBraceGrammarReferenceTests`, `PcreOptionScopeReferenceTests`, `PcreCaptureRepeatOptionReferenceTests`, `PcreExtendedOptionReferenceTests`, `PcreResetOptionReferenceTests`, `PcreAsciiDigitOptionReferenceTests`, `PcreAsciiRestrictionReferenceTests`, `PcreCaselessRestrictionReferenceTests`, `PcreDuplicateNameReferenceTests`, `PcreCaptureConditionReferenceTests`, `PcreRecursionConditionReferenceTests`, `PcreAssertionConditionReferenceTests`, `PcreStaticConditionReferenceTests`, `PcreRecursiveCallReferenceTests`, `PcreReturnedCaptureReferenceTests`, `PcreSubroutineReferenceTests`, `PcreSubroutineLookbehindReferenceTests`, `PcreNonAtomicAssertionReferenceTests`, `PcreGroupAliasReferenceTests`, `PcreGeneralLookaheadReferenceTests`, `PcreAssertionRepeatReferenceTests`, `PcreFixedLookbehindReferenceTests`, `PcreVariableLookbehindReferenceTests`, `PcreLookbehindReferenceLengthTests`; this does not migrate Grep or establish a broader public compatibility claim. |
| Rejected by the internal compiler for now | Unbounded consumed lookbehind `(?<=a+)b` | Explicit unsupported-syntax assertions in the fixture tests. Reference acceptance is not a claim of internal support. |
| Pending conformance | Invalid UTF-8 offset exhaustiveness, Unicode and caseless data versions, broader assertion and option scopes, PCRE2 properties, verbs, capture boundaries and diagnostic equivalence | Separate issue #7 tranches; no fallback engine. |

## Resource and performance gates still open

Existing internal tests cover cancellation, work limits, backtracking depth and malformed-byte barriers. Grep's `Icod.Grep-1.6.0-T6.7-PCRE-Closure-Report.md`, `benchmarks/README.md`, and T6.8 stress plan cover dense `-o`, 32 independent expressions, long records, large pattern sets and record scaling. Their measured PCRE2 JIT baseline is the comparison target; the capture-journal allocation test in PR #43 is only a synthetic internal guard. Freeze the full workload inputs, outputs and measured thresholds before claiming parity or changing Grep.

## Byte class CRLF search follow-up

Byte class CRLF continuation now preserves negated source operand count/shape and literal/range endpoints, including the native negated-singleton exception. Historical positive interior-range flags remain until broader start-optimization coverage prevents lost matches; known false positives stay in #99. The bounded byte two-atom negated-singleton/all-byte class plus literal prefix preserves JIT LF candidate entry; optional single-branch capture/noncapture wrappers qualify. Broader prefix optimizer shapes remain tracked in #99. UTF behavior and all VM work/cancellation/resource paths remain shared. See `PCRE-BYTE-CRLF-DESIGN.md` and `PcreByteCrLfSearchReferenceTests`.

## UTF leading-class CRLF entry

Mandatory positive predicate-free ASCII bracket starts which exclude CR now retain native JIT LF entry, including all-qualified alternatives, capture/noncapture/atomic wrappers and required variable/exact1 repeats. At least one qualifying start class must admit LF, and root/worklist analysis is conservatively capped at1,000 visits to respect native start-bitmap exhaustion. Source newline metadata remains independent; all candidates keep existing VM work/cancellation and original fragment/capture protocols. General fixed/optional/assertion/call/Unicode starts remain in #102; byte #99/#101 behavior is unchanged. See `PCRE-UTF-CRLF-START-CLASS-DESIGN.md` and `PcreUtfCrLfStartClassReferenceTests`.

## Control escapes

Printable ASCII `\cX` operands share the existing literal path across byte/UTF classes, captures and lookbehind widths, with original byte indexes and existing work/cancellation. Missing/nonprintable/non-ASCII operands are InvalidSyntax at the escape; outer structural and invalid UTF diagnostics retain their precedence. Quoting and x/xx token boundaries are preserved. See `PCRE-CONTROL-ESCAPES-DESIGN.md` and `PcreControlEscapeReferenceTests`. Other escapes and #99/#102 optimizer gates remain pending.

## Start directives

Exact consecutive `(*LF)`, `(*ANYCRLF)`, `(*BSR_UNICODE)` and `(*BSR_ANYCRLF)`
from original byte0 independently override the existing newline and BSR compile
settings, with last setting winning in each family. The parser retains original
bytes and offsets; groups/reset/calls cannot unset these global choices.
Quoted/bracketed text is ordinary text, and misplaced/unrecognized directives
retain explicit rejection with existing enclosing-assertion precedence: direct
lookbehind bodies can report the lookbehind opening instead of the directive
opening. Existing matching, fragments,
work/cancellation and limits are shared without new runtime state. See
`PCRE-START-DIRECTIVES-DESIGN.md` and `PcreStartDirectiveReferenceTests`. Other
newline settings/directives, #99/#102 optimizer parity and public/consumer gates
remain separate.

## Ordinary bracket reference escapes

Inside ordinary brackets, `\g` and `\k` are literal letters and consume no
following reference-looking delimiters, names or digits. They share existing
byte/UTF class ranges, negation, i/r folding, quoting/trivia, width/capture and
resource protocols. Outside reference/call grammar and original-byte diagnostic
precedence stay unchanged. See `PCRE-BRACKET-REFERENCE-ESCAPES-DESIGN.md` and
`PcreBracketReferenceEscapeTests`. Other escapes/algebra and public/consumer
conformance gates remain separate.

## UTF/UCP start directives

The shared original-byte0 prefix also accepts `(*UTF)`, `(*UTF8)` and `(*UCP)`.
UTF/UTF8 enable UTF, UCP independently enables UCP, and caller enables remain set.
One immutable compile-settings scan resolves all seven supported tokens before
mode selection and whole-source UTF validation; newline/BSR retain their
independent last-wins rules. Original bytes/body offsets and early cancellation/
work-limit precedence are preserved. Byte UCP is supported without enabling UTF; UTF
directives do not enable invalid-subject tolerance. Existing matcher/fragment/
capture/width/resource paths are shared. See `PCRE-UTF-START-DIRECTIVES-DESIGN.md`
and `PcreUtfStartDirectiveReferenceTests`. Other directives and public/consumer
conformance gates remain separate.

## Unconditional failure verbs

Exact no-argument `(*FAIL)` and `(*F)` fail without consuming subject and use the
shared backtracking/journal path. Direct quantifiers are InvalidSyntax at their
original quantifier byte; groups containing failure remain repeatable. Consumed
lookbehind widths stop at a direct failure in each branch, skipping later widths
and nested-behind checks in that width tail. Global syntax/reference validation
and full-source newline metadata still apply; earlier widths/later alternatives
and global-mode nested behinds remain checked. Named FAIL/F arguments and other
verbs remain pending. See `PCRE-FAILURE-VERBS-DESIGN.md` and
`PcreFailureVerbReferenceTests`. Public/release/consumer and broader optimizer
conformance gates remain separate.

## Nonempty-match start directives

The shared original-byte0 prefix also accepts `(*NOTEMPTY)` and
`(*NOTEMPTY_ATSTART)`. Flags accumulate globally and survive inline resets.
Whole-match acceptance rejects empty reported spans everywhere for NOTEMPTY, or
only at the original caller offset for ATSTART; K resets affect the reported
span. Rejection uses shared rollback, allowing alternative/lazy fallback while
retaining atomic commitment and ordinary empty assertion/call success. Flagged
tolerant empty programs take the existing candidate/fragment path; nullable
metadata remains intact. See `PCRE-NOTEMPTY-DIRECTIVES-DESIGN.md` and
`PcreNotEmptyDirectiveReferenceTests`. Other directives/verbs, optimizer, public/
release and consumer gates remain separate.

## CR and NUL newline conventions

The byte0 prefix also selects `(*CR)` or `(*NUL)`, last-wins with LF/ANYCRLF.
Dot and bare N, line/final-newline anchors and x/xx comments share one global
single-character convention; raw NUL is length-delimited data, CRLF under CR
is two characters. DotAll changes dot only; R remains exclusively BSR-owned.
Existing LF/ANYCRLF candidate/fragment rules remain. See
`PCRE-CR-NUL-NEWLINES-DESIGN.md` and `PcreCrNulNewlineReferenceTests`.
CRLF-only/Unicode ANY and existing optimizer/public/release/consumer gates remain
separate; pending acceptance representatives now cover CRLF and ANY.

## CRLF-only newline convention

`(*CRLF)` is a last-wins newline prefix option. Only the complete pair is a
newline; lone CR/LF remain ordinary. Dot/N, anchors and extended comments use
the existing active end/fragment boundaries, including fixed versus variable
behind end-cap differences. R remains BSR-owned. Byte CRLF dot/N share the Any
instruction with UTF; existing byte two-atom prefix scanning recognizes this
leaf, while UTF retains native pair skipping from zero. Caller LF entry is
preserved in both. See `PCRE-CRLF-NEWLINE-DESIGN.md` and
`PcreCrLfNewlineReferenceTests`. Unicode ANY, broader #99/#102 optimizers and
public/release/consumer gates remain separate.

## ANY newline convention and consumed-CR anchors

`(*ANY)` completes the six newline directives, last-wins in the shared prefix.
The existing fixed vertical-space list supplies LF/VT/FF/CR/NEL and UTF LS/PS;
NUL remains ordinary, R remains BSR-owned. Scalar-length/previous-scalar helpers
keep UTF continuation bytes out of comment and anchor decisions. Native JIT
consumed-CR midpoint caret/dollar/Z behavior is pinned under ANY and corrected
under ANYCRLF; CRLF-only remains pair-only. Candidate bumpalong and bounded
byte/UTF prefix differences stay shared. See `PCRE-ANY-NEWLINE-DESIGN.md` and
`PcreAnyNewlineReferenceTests`. Broader optimization/Unicode-version/public/
release/consumer gates remain separate.

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
