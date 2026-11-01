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
