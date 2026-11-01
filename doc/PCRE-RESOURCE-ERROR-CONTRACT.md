# PCRE error and resource contract checkpoint

**Status:** issue #7 internal fixture gate. These limits and diagnostics are
provisional; this document does not approve a public PCRE API or Grep switch.

## Grep-facing boundary

The pinned `Icod.Grep` source at `2ef1c11f11b77eb876242eb0c92a88d20857db64`
uses PCRE.NET 1.6.0 / PCRE2 10.48 with `Compiled` (JIT requested), and in a
UTF-8 locale also requests UTF, UCP, `MatchInvalidUtf`, and ASCII `\d`.
`PcreTests.InvalidPatternReturnsUsageError` asserts status 2 and nonempty
stderr for `-P '('`. Grep catches `PcreException` while compiling patterns;
its command-level `OperationCanceledException` handler returns canceled
status 130. `PcrePattern.Find` checks cancellation immediately before and
after the native match, rather than providing an interrupt inside it. Match
resource failures have no separate command fixture yet.

The future Expressions adapter must map compile and match diagnostics to
Grep's error path (status 2 with stderr), keep successful no-match as status 1
when no record is selected, and let cancellation reach the canceled path.
The wording of native PCRE exceptions is not frozen as an API contract.
No adapter is implemented in this tranche; these are migration requirements.

## Internal outcome matrix

| Condition | Internal outcome | Evidence and consumer relevance |
|---|---|---|
| Malformed or pending pattern | `InvalidSyntax` or `UnsupportedSyntax` with source-pattern byte offset; no compiled pattern | `PcreByteFoundationTests`, `PcreSyntaxAcceptanceTests`; both must become Grep errors when the adapter exists. |
| Bad UTF-8 pattern | `InvalidUtf8Pattern` at offending byte | `PcreUtf8InvalidSubjectTests`; Grep's current string-to-pattern encoding does not directly expose arbitrary raw pattern bytes. |
| Nonpositive work limit; pattern nesting/capture/repeat ceilings | `InvalidWorkLimit`, `PatternDepthLimitExceeded`, `PatternCaptureLimitExceeded`, `PatternRepeatLimitExceeded` | Foundation, counted-repeat and `PcreAssertionRepeatReferenceTests`; finite direct assertion repeats need no progress marks but remain bounded by compiled instructions/node visits, including normalized `{65535,}`. Provisional internal compile policy, not native limit parity. |
| Start outside input, or inside a scalar in strict UTF mode | `InvalidStartOffset` | Foundation and UTF tests; current Grep filters out-of-range offsets before calling PCRE.NET. |
| Invalid subject in strict UTF mode | `InvalidUtf8Subject` with offending byte offset | UTF tests; Grep requests tolerant `MatchInvalidUtf`, so strict-mode diagnostics are an internal API condition. |
| Malformed subject in Grep-like tolerant UTF mode | Match or successful no-match on valid fragments | Invalid UTF offset/capture reference suites; invalid bytes are not an error. |
| Ordinary miss | Successful result with null match | Foundation tests; must not be treated as a resource failure. |
| Exhausted match steps | `MatchWorkLimitExceeded`, no match | Foundation and optimization tests; 1,000,000 default steps is provisional. |
| More than 1,024 live saved paths or assertion guards | `MatchDepthLimitExceeded`, no match | `PcreResourceContractTests` pins 1,024/1,025 lazy optional paths on empty input. General lookahead guards share this ceiling, pinned at 1,023 choices plus one guard and nested-guard variants in `PcreGeneralLookaheadReferenceTests`. General fixed-length lookbehind uses the same guards and ceiling (`PcreFixedLookbehindReferenceTests`). This is an explicit-frame ceiling, not PCRE2 recursion depth. |
| Cancellation before/during compilation or matching | `OperationCanceledException` | Foundation and resource tests; distinct from all diagnostics. |

PCRE2 exposes match, heap and depth limits with different accounting; its
[API documentation](https://www.pcre.org/current/doc/html/pcre2api.html) states
that the depth limit is ignored by JIT matching. Do not equate the internal
saved-path count with PCRE2's depth limit or compare numeric thresholds across
engines. Test the selected Grep workloads and error behavior separately.

General fixed-length lookbehind validates character widths after complete parsing and name resolution with
heap traversal and bounded sums/products (65,535 per new general branch). Byte
and UTF scalar rewind spends match work and observes cancellation within the
current valid fragment. Bounded variable lengths use min/max analysis and cap every branch at255 if
any branch varies. Reverse retry frames share saved-path/journal ceilings, use
a byte-sized remaining count and spend work. Scoped end limits restore through
stable saved assertion-scope IDs. Consumed references resolve bounded capture-body widths through a global heap
map and consumed-only dependency traversal; lexical self/cycles, unbounded bodies
and global branch reset remain controlled unsupported syntax. Erased references
still validate their target. Name/tree reconstruction and width dependency
traversal use explicit heap frames with cancellation. Native's independent 2,000
branch-analysis accounting is not a managed numeric-limit parity claim.

Empty/reset option headers add no runtime state or work units. Bare settings
consume neither a capture slot nor a nesting frame; scoped empty/reset groups
use the existing nesting ceiling. Reset n restores ordinary capture allocation;
reset preserves U, J and ASCII restrictions. Scoped/mixed ASCII restrictions use the same reader
and a five-bit snapshot; immutable class predicates and existing b/B metadata
need no VM state. `PcreAsciiRestrictionReferenceTests` covers mask algebra,
policy scopes and unchanged depth/capture/work/cancellation limits.
`PcreAsciiDigitOptionReferenceTests` and `PcreResetOptionReferenceTests` pin
these boundaries and unchanged match-work/cancellation outcomes.

Local r uses immutable node/instruction comparison metadata, with no option
stack or saved-frame changes. Restricted range keys remain in the existing
scalar-domain bitmap and retain cancellation polling. UCP POSIX Ll/Lu stay
category-specific independently of explicit property case widening.
`PcreCaselessRestrictionReferenceTests` covers scope, captures, optimized/general
assertions, range closure and unchanged depth/capture/work/cancellation limits.

Duplicate named references share immutable declaration-ordered slot lists, each
bounded by capture256. Native ordered registration may let an earlier matching
name preempt a later number conflict; distinct names remain bounded by256, with
aggregate name/slot pairs at most65,536 and candidate integer storage at most256KiB.
Every participation lookup spends match work and polls cancellation.
Selection commits before the existing comparison and allocates no matching state.
Unique names retain the numbered fast path. Actual duplicate consumed names reject
lookbehind width analysis; J-only, numeric and lookahead-only controls remain valid.
`PcreDuplicateNameReferenceTests` pins source-order/empty/mismatch selection, scope,
registration errors and compile/match budgets.

Atomic-group and atomic-lookaround aliases reuse the same heap GroupFrame and
semantic lowering: one nesting level, no capture slot and no added matching work.
Exact byte headers preserve original source offsets and assertion context; all
width, journal, saved-path and work/cancellation gates remain shared.
`PcreGroupAliasReferenceTests` covers the nine prefixes and distinct pending forms.

Non-atomic positive assertions retain body retries and guards under the same1024
saved-frame ceiling. A scope arena stores five integers per live guard (at most
20KiB of integer payload); every guard pop/prune releases its last arena node.
Frames store integer current/parent IDs, never spans or capture snapshots. On
success, scope limits hide behind the parent ID; retry restores the original
context while the journal rolls back captures and marks. All atomic/negative
pruning shares this scope lifetime path. The arena resets with candidate state.
`PcreNonAtomicAssertionReferenceTests` pins retries, scope reuse, width/cap ordering
and unchanged compile, saved-path, match-work and cancellation boundaries.

Capture-participation conditions add no saved path or journal state: CaptureTest
spends work and polls cancellation for each target lookup, then chooses exactly
one branch. Targets resolve globally into shared immutable arrays; empty captures
participate and duplicate names test ANY slot. All explicit bodies validate and
contribute native behind widths; omitted runtime else is empty independently of
width analysis. `PcreCaptureConditionReferenceTests` pins branch commitment, name
priority, forward/relative numbering, width quirks and compile/match resource gates.

Assertion conditions use one existing guard slot while evaluating, then release it
and its arena node on completion. Matching bodies commit captures for either
polarity; body exhaustion restores the shared journal and resumes its other
branch. No conditional Split, capture snapshot or extra frame/scope fields are
introduced. Conditional plus inner assertion each count toward source depth.
`PcreAssertionConditionReferenceTests` pins guard accounting, sibling arena
reclamation, work/cancellation and unchanged compile ceilings.

Ordinary brace classification scans a local source cursor and polls cancellation
while checking complete count grammar before numeric decoding. Non-counted
source becomes ordinary literal nodes and therefore spends existing instruction
and match-work budgets; valid-shaped overflow/reversed counts remain errors.
No compiler/VM state or limits change. `PcreBraceGrammarReferenceTests` pins these
distinctions, source byte diagnostics and existing resource outcomes.

Static DEFINE/VERSION conditions use existing Jump instructions with no saved
path, scope or journal state. Both bodies compile and count toward existing
instruction/node budgets. Ignored-runtime false-one-branch repeats retain their
AST widths and logical repeat-count compile charge without runtime marks or
expansion. Global capture/reference validation remains; DEFINE nested behind
width checks follow native global-versus-consumed scan context. Version tuples
cap each component at1000 and use internal compatibility10.48, independently of
package version. `PcreStaticConditionReferenceTests` pins these contracts.

## Remaining release work

The bounded internal taxonomy is testable now. Before migration, add
command-level Grep fixtures for a managed match-work and saved-path failure,
assert status 2 and useful stderr, and test cancellation on the new adapter.
Measure whether the default budgets admit Grep's seven documented workloads,
large pattern sets and long records without disproportionate memory use.
Choose thresholds from those measurements. Synthetic process-wide memory caps
are not a portable parity oracle, as Grep's T6.8 resource report explains.
Grep and CommandFramework production code remain unchanged here.

Nonrecursive subroutines share the1,024 saved-frame ceiling: every call guard
counts, including successful inactive calls retained for backtracking. Integer
call-scope IDs and a four-integer guard-owned arena record preserve retries;
return compensation uses the shared journal and spends work per inspected
mutation. Numbered captures/starts and private repeat marks restore; G0/K persists.
Callable bodies compile once per referenced target after main Accept, sharing
4,096 instruction/visit and256 repeat-mark ceilings. No capture snapshots,
span/pointer frames or runtime native fallback. Explicit returned selections now
retain numbered values while starts/marks restore; the existing Call payload and
ReturnPc-1 lookup leave saved frames and arena records unchanged. At most256 sorted
unique targets support bounded binary searches (at most9 comparisons), after each
journal entry spends work/polls cancellation. Compile reference storage is linear
in source entries; duplicate expansion normalizes to at most256 slots. See
`PcreReturnedCaptureReferenceTests` and `PCRE-RETURNED-CAPTURES-DESIGN.md`.
Consumed calls with return lists reject at the owning behind opener, including
transitive/erased calls; ahead/DEFINE width bypasses remain.
Recursive/whole-pattern calls now share the same guards, journal and budgets;
no CLR recursion, same-position loop heuristic, arena growth or new opcode.
The pinned JIT accepts capture-changing same-position recursion, while an
unproductive first recursive branch reports a resource error before later
alternatives are tried. Existing MatchDepthLimitExceeded/MatchWorkLimitExceeded
remain library limits, not PCRE2 JIT-stack/match threshold parity. Root target0
reuses the resolved semantic root and shares instruction/visit limits. Consumed
recursive dependencies/whole-root calls reject at the owning behind, preserving
ahead/DEFINE bypasses; unresolved suppressed-behind targets stay rejected. See
`PcreRecursiveCallReferenceTests` and `PCRE-RECURSIVE-CALLS-DESIGN.md`. Bounded consumed subroutine
calls in lookbehind now use the existing min/max dependency heap and first
definitions, with variable255/all-fixed65535/intermediate65535 width limits.
Calls and backreferences share active dependency validation, including before
zero-count erasure; existing backreference duplicate/lexical restrictions remain.
No additional runtime/compiler instruction or frame state is required.
See `PcreSubroutineReferenceTests`, `PcreSubroutineLookbehindReferenceTests`,
`PCRE-SUBROUTINES-DESIGN.md` and `PCRE-SUBROUTINE-LOOKBEHIND-DESIGN.md`.

Recursion conditions reuse active call IDs and the call target number retained in
the fourth arena integer (at most16KiB integer payload under the same1024 guard
ceiling). They never scan inactive saved guards or ancestor calls. Duplicate named
target tests spend work per candidate and poll cancellation; no capture mutation,
new saved-frame fields or journal copies. R/R0 mean any call; exact global names
win bare R/digit ambiguity. See `PcreRecursionConditionReferenceTests` and
`PCRE-RECURSION-CONDITIONS-DESIGN.md`.

Special UTF properties reuse scalar atom/bracket predicate work and existing
widths; AnyScalar/UniversalCharacterName add no VM state, frames or scalar-range
expansion. Category namespaces cannot enable special names. UTF class CR/LF
metadata now comes from original literal/range endpoints and operand shape before
folding, with the native negated-singleton exception; class membership remains
unchanged. Byte start-optimization parity remains a separate
gate. See `PcreSpecialPropertyReferenceTests`, `PcreClassNewlineMetadataReferenceTests`
and `PCRE-SPECIAL-PROPERTIES-DESIGN.md`.

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
