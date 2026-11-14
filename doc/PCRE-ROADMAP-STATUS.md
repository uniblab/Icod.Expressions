# PCRE roadmap status

Reviewed after merged PR246, with PR247 in progress. Issue #7 remains the delivery roadmap; the
[syntax matrix](PCRE-SYNTAX-ACCEPTANCE-MATRIX.md),
[resource contract](PCRE-RESOURCE-ERROR-CONTRACT.md) and
[Grep fixture inventory](PCRE-GREP-CONSUMER-FIXTURES.md) define the detailed gates.

## Present position

The internal byte/UTF8 matcher implements ordered alternatives, captures and
backreferences, recursive subroutines, atomic/non-atomic assertions, bounded
variable lookbehind, conditions, repetition, newline policies, malformed-UTF
fragments and guarded resource/cancellation behavior. The observed Grep fixture
floor is mapped, and component correctness/performance probes run in CI. These
are substantial implementation milestones, not an unrestricted PCRE2 claim.
PRs147–205 principally expanded native start-optimization/CRLF entry parity;
issue102 still tracks the wider optimizer gap. PR206 accelerated Intel CI while
retaining each framework's complete suite and frozen-result qualification.

## Remaining order

1. **Complete conformance and Unicode data ownership.** General categories
   are pinned to PCRE2 10.48's Unicode17 in PR207. PR208 pins the complete
   default simple-case mapping, closing host-version dependence in that path.
   Continue offsets, resources and malformed-UTF evidence;
   keep native optimizer differences and unsafe native cases explicitly tracked.
2. **Expand deliberate syntax coverage.** Script/script-extension properties are
   implemented in PR209; PR210 completes the native binary/Bidi catalogue and
   PR211 adds atomic grapheme clusters. PR212 adds Perl class algebra; PR213 adds
   UTS#18 activation, PR214 adds script-run groups, PR215 adds scoped ACCEPT and
   PR216 adds no-argument PRUNE; PR217 adds SKIP candidate restart and one bounded
   PRUNE repeat-cache parity proof. PR218 adds scoped COMMIT root termination
   with bounded native first-code-unit entry. PR219 adds no-argument THEN with
   lexical alternation retry and assertion/callee catches. PR220 adds MARK and
   its shorthand with successful-path byte labels on the internal result. PR221
   adds named SKIP using bounded visible MARK history and shared scoped cuts.
   PR222 adds successful labels for named PRUNE/COMMIT/THEN. PR223 adds
   named ACCEPT/FAIL/F using authentic implicit MARK and existing scope exits.
   PR224 adds the CASELESS_RESTRICT prefix through the existing (?r) policy.
   PR225 adds UTF Turkish casing through the shared scalar folding policy.
   PR226 adds caller Python-octal and bare-zero policies to shared numeric parsing.
   PR227 adds caller escaped-CR remapping through canonical literal parsing.
   PR228 adds caller-selected empty classes through canonical nodes and bounded UTS terms.
   PR229 adds caller s/m/n/U/J/x/xx initialization through the existing scoped state.
   PR230 adds native word/line compilation through existing typed guards and scopes.
   PR231 completes caller ASCII type restriction initialization through the same mask.
   PR232 adds dollar/circumflex compile options through canonical anchor lowering.
   PR233 adds optional unset backreferences through the existing capture instruction.
   PR234 extends bounded STARTLINE search to opaque reference prefixes in both policies.
   PR235 adds caller UTF/UCP/code-unit restrictions at existing compile boundaries.
   PR236 adds standard alternate backslash-u/U/x grammar through canonical literals.
   PR237 adds extra braced-u scalars and native malformed literal-brace ownership.
   PR238 adds alternate control-verb name decoding through existing label machinery.
   PR239 adds optional surrogate escape code points while retaining UTF validation.
   PR240 adds opt-in native bad-escape literal recovery through the existing parser.
   PR241 adds caller prohibition of manual callouts at existing atom dispatch.
   PR242 adds caller casing policies and bounded marked-scalar-set newline search.
   PR243 adds caller opt-in for match-start resets in supported assertions.
   PR244 adds ordinary escaped UTF literals through the validated scalar reader.
   PR245 adds whole-pattern literal compilation through canonical literal nodes.
   PR246 adds caller-anchored candidate selection through immutable program policy.
   PR247 adds caller end-anchored root completion through the same policy metadata.
   UTF code-unit escape,
   remaining directives/verbs/callouts and other matrix gaps require their own
   pinned reference fixtures. Decide any intentional exclusions before release;
   successful compilation must never imply an unsupported construct works.
3. **Finish architecture/API and consumer performance review.** Preserve the
   byte-authoritative contract; complete the planned UTF16 peer/public API
   decisions and realistic workload measurements. Component smoke comparisons
   do not establish command-level equivalence or release readiness.
4. **Publish and migrate consumers, with Grep last.** Qualify the package and
   public compatibility contract, then migrate consumers in the approved order.
   Grep additionally needs command diagnostics/cancellation, benchmark/stress and
   all six-RID packaging/archive/behavior gates. Remove its PCRE.NET runtime only
   after qualification. Keep CommandFramework production ownership until every
   known consumer migration is complete.

## Current increment and next syntax work

PR246 merged at153273df6fd58266599e0dc495f15da4f51b3bda;
its85,685 tests/framework remain the baseline. Main43583523079 and scripts
43583523076 are green. PR247 adds end-anchored compilation as described below;
final qualification/review/delivery identities are recorded on PR247/issue7.

## PR234: backreferences blocking first-character search

Extend PR233 STARTLINE metadata from selected nullable bodies to pure programs
with an opaque reference reachable before consumption. Native compile_branch
disables inferred first characters at references; JIT scan_prefix cannot cross
them before any character, and STARTLINE precedes FIRSTMAP. Default references
are equally opaque. ANY/multiline ^()backref1B on LS+B misses at split offset1
but matches3:1/capture1=3:0 at boundary3 under either reference policy.

Only PcreByteProgram.cs changes: rename the existing private helper to
HasBackreferenceStartBarrier, require any canonical reference and accept a
nonconsuming path reaching Backreference or Accept. Mandatory literals and
one/two-member classes keep native prefix-search priority. A supplementary
linear proof admits exactly one positive, predicate-free ASCII class with more
than two members before a reference: it cannot supply JIT first-character-pair
search. Additional consumers, branches/loops, non-ASCII ranges, predicates and
Turkish casing stay outside that proof. Every branch must still pass the existing
AST first-caret proof. Keep pure-instruction eligibility,1,000 instructions/visits,
heap storage and cancellation. No matcher/parser/IR-shape/source/capture/journal/work/quota/
API/dependency changes. Calls/assertions/conditions/verbs and over-budget
programs remain outside this conservative proof; broader #102 stays open.

The949 new cases include936 independently pinned PCRE2 10.48/JIT profiles
and284 preflighted supplemental native profiles. Cover both reference/casing
policies, aliases, participation/duplicates, repeats, wrappers/alternatives,
NEL/LS/PS split/boundary offsets, malformed fragments, strict boundaries, UCP,
owned source/options, deep/full-capacity/large-prefix proofs and shared work/
cancellation/quotas. Native arrays compile before managed comparison. All247
earlier test files and GNU/frozen35/214 outcomes/120 methods remain fixed.

Clean tests-only RED702 (run43255390418, Linux x64 job131312086289), source
1107f679f0a8da3b3ae069d0934e5c825a53c36c, retained merged PR233 production.
Each net8/9/10 built0warnings/errors and ran62,371 tests:295 expected failures,
62,076 passes,0skips. All285 primary differences/framework match independent
native split/boundary research, including87 default and198 selected failures.
Earlier fixtures, strict/fallback controls and native literal/oracle checks
passed. Freeze fixture f78f3d24ff62419bfefd9672a902259138fbbb7e before
production; require all18 native results and every configured supporting gate.

Qualification after the frozen six-case correction: source
`a00c32c0f9531808415991f99dd6cb4a2bfc5f38`, [CI704](https://github.com/uniblab/Icod.Expressions/actions/runs/43260813850)
and [scripts266](https://github.com/uniblab/Icod.Expressions/actions/runs/43260813804)
are green. All18 native platform/framework results pass62,371 tests each, with
zero failures/skips/build warnings/errors and214 frozen outcomes in120 methods.
Frozen35, five Unicode generators, seven equal component workloads, extraction/
POSIX/glibc/API/corpus, packaging and three package-smokes pass. Three-OS script
gates pass; full consumer compatibility is skipped by the PCRE-only classifier.
The949-case fixture and247 earlier test files are unchanged. An additional1,728
native JIT checks confirm class-pair priority across casing/UCP/reference policies,
root wrappers andNEL/LS/PS offsets. Whole-branch author review found no remaining
issues in this scope; independent review was not performed. Broader optimizer
shapes remain in #102. Final documentation changes preserve the qualified
source, test, packaging and workflow trees.

Other syntax/modes/directives/callouts, failed/partial labels, wider optimizer/
resources (#102), UTF16/exported API and realistic consumer/release gates remain
open. Grep stays last. See [design](PCRE-BACKREFERENCE-STARTLINE-DESIGN.md) and
[plan](PCRE-BACKREFERENCE-STARTLINE-PLAN.md).

## PR235: caller UTF, UCP and code-unit restrictions

Default-false internal NeverUtf, NeverUcp and NeverBackslashC caller options
constrain existing compilation modes and tokens. Effective UTF includes
MatchInvalidUtf and the uninterrupted UTF/UTF8 prefix. NeverUtf rejects it before
whole-pattern UTF validation; NeverUcp rejects effective UCP after that validation
and before Turkish-casing conflicts. Both return InvalidSyntax at the original
prefix-end body offset and identify the caller restriction. Existing cancellation
and invalid-work-limit checks retain their earlier priority.

The parser snapshots NeverBackslashC and rejects a real C escape after prior
bracket/algebra rejection, before the existing unsupported UTF-C branch. It owns
the escape's opening byte, including DEFINE/zero-repeat/failed-tail and late-name
contexts. Quotes, comments, escaped backslashes, control operands and raw MARK
labels stay inert. Ordinary class errors retain their escape offset; Perl algebra
retains its expression opener. Unsupported UTF-C execution, including erased C,
and other unimplemented prefixes remain explicit boundaries.

Only PcreByteOptions.cs, PcreByteCompiler.cs and PcreByteParser.cs change in
production: three bool properties, staged compiler guards and a readonly token
policy. No matcher/IR/journal/work/quota/public API/dependency/workflow changes.
All248 earlier test files and frozen35/214 outcomes/120 methods remain fixed.

The2,513 new cases include2,499 independently pinned PCRE2 10.48 profiles
(1,821 restriction/UTF diagnostics,120 legacy class controls,558 accepted JIT
span/label profiles), plus3,072 preflighted all-octet profiles and shared
resource/ownership/structural/legacy controls. Native codes and native end offsets
are asserted before managed diagnostics; native captures/labels before matching.

Clean tests-only [RED708](https://github.com/uniblab/Icod.Expressions/actions/runs/43369721925),
source1b50d1a6d4e5c0950c45d83a3d6d5d06b5684b5d, LinuxARM job131702547003,
built net8/9/10 with0warnings/errors. Each ran64,884 tests:1,657 expected policy
failures,63,227 passes,0skips. All1,653 primary caller-policy profiles and4 octet
theories failed for the missing restrictions; earlier/native/UTF/class/ownership/
quota/work controls passed. Freeze fixture68cd3d1c943c27bfd82ac7d67a3968ae80c17db8
before production. Initial fixture-only corrections retained established
Perl-algebra diagnostic ownership and the unsupported UTF-C boundary.

Qualification requires all18 native results at64,884 tests/framework and every
configured Unicode/workload/extraction/POSIX/API/corpus/frozen/package/script gate.
Final results, author review and reviewed source/proposed-merge identities are
recorded in [PR235](https://github.com/uniblab/Icod.Expressions/pull/235) and issue7.
Wider optimizer/resources (#102), other syntax/modes/directives/callouts, failed/
partial labels, UTF16/exported API and realistic consumer/release gates remain
open. User owns merge/tag/publication; Grep stays last. See
[design](PCRE-CALLER-COMPILE-LOCKOUTS-DESIGN.md) and
[plan](PCRE-CALLER-COMPILE-LOCKOUTS-PLAN.md).

## PR236: alternate backslash-u, U and x grammar

Default-false internal AltBsux snapshots standard PCRE2 10.48 ALT_BSUX. Exactly
four adjacent hex digits after u produce one canonical character; U is literal
uppercase U. An x token requires two adjacent digits. Incomplete/nonhex sequences
consume only the escape letter, leaving every following source byte to ordinary
syntax. A local cursor validates all digits before committing the source index
or testing character bounds, so incomplete high-valued prefixes cannot trigger
premature range errors. Byte mode caps values at255; UTF rejects surrogates and
does not combine adjacent surrogate escapes.

Fallback braces use the existing quantifier/literal grammar: backslash-u{2}
repeats literal u twice, and backslash-x{41} repeats literal x41 times. Standard
ALT_BSUX does not enable EXTRA_ALT_BSUX braced-u syntax. A produced backslash is
one literal node and cannot re-tokenize later source. Quotes/comments/control
operands/raw MARK names stay inert, caller reset scopes retain the flag, and
whole-pattern UTF validation and previous caller restrictions keep precedence.
Class/range/algebra diagnostic ownership and canonical casing/captures/widths/
newlines/search metadata/work/cancellation/structural quotas remain shared.

Production changes only PcreByteOptions.cs, PcreByteParser.cs and the focused
PcreByteParser.AltBsux.cs partial. No AST/IR/matcher/journal/work/quota/public API/
dependency/workflow changes. All249 earlier test files and frozen35/214 outcomes/
120 methods remain fixed. The586 new cases include554 unique pinned native
profiles (491 accepted JIT,60 compile diagnostics and3 strict-UTF subject errors), plus1,115 supplementary JIT0
compile profiles (1,024 all-value,84 offset-pattern,6 structural/work/owned and1
produced-backslash caller-policy control). Native arrays compile before managed
comparison. Literal captures/labels and diagnostic codes/end offsets are pinned
independently. Six newlines, every selected subject byte offset, malformed UTF,
default diagnostics and resource/ownership/precedence controls are included.

Clean refined tests-only [RED713](https://github.com/uniblab/Icod.Expressions/actions/runs/43380230552),
source523784aaad7838a9399f745e1d62662ad6ef5052, LinuxARM job131736180443,
built net8/9/10 with0warnings/errors. Each ran65,470 tests:430 expected managed
failures,65,040 passes,0skips. The410 primary differences and20 supplemental
cases failed only for missing alternate grammar; native/default/earlier/UTF
controls passed. Freeze fixture780b5a5583aaef908bcae74bbb4cea48e4cc0d35 before
reapplying production. Earlier RED711/freeze were superseded: three strict-UTF
subject errors were incorrectly called nonmatches. Those exact profiles now pin
native error-4 and managed InvalidUtf8Subject0; no span/compile-error expectation
was weakened. Four redundant compile-error rows were removed earlier. Reader
source is byte-identical to the initial implementation712.

Qualification requires all18 native results at65,470 tests/framework and every
configured Unicode/workload/extraction/POSIX/API/corpus/frozen/package/script gate.
Final results, author review and reviewed source/proposed-merge identities are
recorded in [PR236](https://github.com/uniblab/Icod.Expressions/pull/236) and issue7.
Extra braced-u and surrogate policies, other syntax/modes/directives/callouts,
failed/partial labels, broader optimizer/resources (#102), UTF16/exported API and
realistic consumer/release gates remain open. User owns merge/tag/publication;
Grep stays last. See [design](PCRE-ALT-BSUX-DESIGN.md) and
[plan](PCRE-ALT-BSUX-PLAN.md).

## PR237: extra alternate braced-u and literal-brace fallback

Default-false internal ExtraAltBsux implements PCRE2 10.48 EXTRA_ALT_BSUX and
implies standard alternate u/U/x grammar independently of AltBsux. Complete
braced-u tokens accept adjacent hex digits, arbitrary leading zeros and Unicode
scalars up to10ffff in UTF, or255 in byte mode. Whitespace is not allowed inside
the token. UTF surrogate escapes remain invalid and adjacent escapes do not pair.

The focused reader uses a local uint cursor and per-digit cancellation. Complete
closing braces precede scalar/bounds validation, so an out-of-range32-bit value
followed by nonhex text falls back literally. True32-bit overflow fails before
shifting, even in malformed or unterminated tokens. Diagnostic ownership remains
the escape opener, or the established Perl-algebra opener. In a class, malformed
braced-u returns literal u and leaves the opening brace for the class grammar.
Outside classes it consumes that brace and returns a parser-private marker.
AppendNode immediately emits canonical case-scoped u and brace literals, applying
normal repeats/trivia only to the brace. The marker never reaches persistent
syntax/IR. This preserves fallback repeat ownership without a synthetic group,
source rewriting or re-tokenization of a decoded backslash. Quotes/comments/raw
labels/control operands remain inert and caller resets preserve the policy.

Only PcreByteOptions.cs, PcreByteParser.cs, PcreByteParser.AltBsux.cs and
PcreByteParser.Groups.cs change in production. No canonical AST/IR/matcher/
journal/work/quota/public API/dependency/workflow changes. Existing classes,
assertions/calls/captures, scalar widths/casing/newline/search metadata, source/
option ownership, work/cancellation/structural limits and caller restrictions
retain their shared paths. All250 earlier test files and frozen35/214 outcomes/
120 methods remain fixed.

The981 new cases include940 unique pinned native profiles:730 span/capture/label,
204 compile diagnostics and6 strict-UTF subject errors. Supplemental preflight
checks2,310 compile profiles (2,303 JIT0 accepts/7 expected rejects) and17,040
selected offset protocols. Cover all256 octets, all four caller selections,
byte/UTF/UCP/tolerant/caseless/extended/UTS policies, malformed/complete/overflow
and zero-padding boundaries, six newlines, grammar contexts and ownership/shared
resource controls. Native no-match(-1) is distinguished from other negative
match results. Native compile arrays/error assertions precede managed comparisons;
compile errors are deduplicated independently of match subjects.

Clean refined tests-only [RED716](https://github.com/uniblab/Icod.Expressions/actions/runs/43385834110),
source f5554d6e5e48781ccf42aeeb5d5fcb7e3fd2b32e, LinuxARM job131754114215,
built net8/9/10 with0warnings/errors. Each ran66,451 tests:751 expected managed
failures,65,700 passes,0skips. Failures comprise533 span profiles,176 compile
diagnostics,3 strict-subject controls and39 supplemental cases, all for missing
extra grammar; native/default/earlier controls pass. The first749-failure RED715
was extended before freeze by2 local case-scope theories, retaining every earlier
expectation. Freeze fixture9cd132c867fd8775818b5517f7a633068c66713d before production.

Qualification requires all18 native results at66,451 tests/framework and every
configured Unicode/workload/extraction/POSIX/API/corpus/frozen/package/script gate.
Final GREEN, author review and exact source/base/proposed-merge identities are
recorded on [PR237](https://github.com/uniblab/Icod.Expressions/pull/237) and issue7,
with repo documentation included before the qualification commit. Surrogate
allowance and alternate verb-name decoding, other syntax/modes/directives/callouts,
failed/partial labels, broader optimizer/resources (#102), UTF16/exported API and
realistic consumer/release gates remain open. User owns merge/tag/publication;
Grep stays last. See [design](PCRE-EXTRA-ALT-BSUX-DESIGN.md) and
[plan](PCRE-EXTRA-ALT-BSUX-PLAN.md).

## PR238: alternate control-verb name grammar

Default-false internal AltVerbNames implements PCRE2 10.48 ALT_VERBNAMES for
MARK/shorthand, named SKIP, PRUNE/COMMIT/THEN and ACCEPT/FAIL/F. A focused decoder
accepts literal data escapes and local Q/E quotes. Regex types/anchors/references
are invalid name tokens. Names remain case-sensitive; decoded delimiters and
backslashes remain bytes and cannot enter regex syntax. Escaped/raw UTF labels
retain native UTF8 encoding without enabling ordinary escaped-UTF regex syntax.

The parser snapshots the caller flag and retains the old raw reader when false.
Literal controls/numeric/octal/hex/N/u/U policies and the alternate literal-brace
marker reuse existing readers. Name-specific rejection precedes token policies
such as NeverBackslashC. Extended/More whitespace and # comments use shared
newlines outside local quotes; escaped whitespace remains data. The private
SkipIgnoredPatternText gains default-true allowInlineComments; only names pass
false so inline (?#...) remains name data and outer regex comments are unchanged.
Raw UTF advances by complete source characters before trivia processing.

Raw source255-byte limits and MARK's nonempty requirement include ignored trivia,
escape spellings and quote delimiters. They apply at the real unquoted closing
parenthesis, after earlier escape errors. A nonempty argument that decodes empty
uses the existing NUL name representation, preserving named versus unnamed SKIP
and implicit MARK transactions without new canonical AST/IR/matcher/journal flags.
Owned byte buffers retain source/options isolation and CString visible-name rules.

The name decoder changes PcreByteOptions.cs, PcreByteParser.cs,
PcreByteParser.PathMarks.cs, PcreByteParser.Extended.cs and the new
PcreByteParser.AltVerbNames.cs. Shared work/cancellation/structural limits,
GNU/BRE/ERE, public API/dependencies/workflows remain unchanged. All251 earlier
test files and frozen35/214 outcomes/120 methods stay fixed.

The856 new cases include833 unique pinned native profiles:671 byte span/capture/
success-label and162 compile-diagnostic/offset profiles. Supplemental preflight
checks1,137 compile profiles (1,130 JIT0 accepts/7 expected rejects) and6,240
selected offset protocols. Cover every label octet, all named verbs, literal and
forbidden/numeric escapes, raw byte limits/error precedence, quote/close/comment/
case/reset scope, empty/NUL identity, caller policies, six newlines, strict/tolerant
UTF, owned source/options and shared resources. Native no-match(-1) is separated
from other negative results; failed/partial labels remain outside the result
contract. Native arrays/error assertions precede managed comparison; compile
errors are deduplicated independently of match subjects.

Clean tests-only [RED718](https://github.com/uniblab/Icod.Expressions/actions/runs/43392128698),
source2d0953fd995c0783cfc06e5a3251630587077ae7, LinuxARM job131774325691,
built net8/9/10 with0warnings/errors. Each ran67,307 tests:650 expected managed
failures,66,657 passes,0skips. Failures comprise490 success-label/span profiles,
139 compile diagnostics and21 supplemental cases; native/default/earlier controls
pass. Name fixture44ef5f4a1ca903df65c2f48c3c02a1db0d7f55e5 froze before production.
CI719 then cleared the decoder cases, exposing one frozen ANY/tolerant-UTF caret/
MARK split-offset theory. Native JIT scan_prefix stops at OP_MARK; no collected
character means STARTLINE precedes FIRSTMAP. The raw canonical name has the same
rule. PcreByteProgram.cs now adds HasLinearPathMarkStartBarrier beside the existing
reference proof. Require at most1,000 instructions, only literals/PathMark/
captures/line-end anchors/atomic boundaries/final Accept, a PathMark before any
consumption, and the existing AST first-caret proof. Reject branches/loops/calls/
assertions/classes/references/other verbs; retain literal-before-MARK priority.
No opcode, instruction shape, matcher/journal/work/quota changes. Wider #102 stays
open. Six production files change, including this bounded compile-time metadata.

PcreMarkStartLineReferenceTests adds718 cases:704 pinned JIT profiles for raw/
decoded/empty labels, NEL/LS/PS split/boundary offsets, wrappers/casing/UCP, empty
bodies and priority/byte/strict controls;14 supplemental profiles cover1,000-
instruction boundaries, over-budget valid-boundary fallback and256 captures.
Combined increment:1,574 new cases,1,537 primary native profiles,1,151 supplemental
compile profiles (1,144 JIT0/7 expected rejects), and6,240 offset protocols.

Integration tests-only [RED720](https://github.com/uniblab/Icod.Expressions/actions/runs/43396124768),
sourcef6424db14a3f18e655baf928c213ffd8f4e559f9, LinuxARM job131787334583,
built net8/9/10 with0warnings/errors. Each ran68,025 tests:367 expected failures,
67,658passes,0skips. All360 added split-offset profiles,4 literal-boundary cases,
2 capture-capacity cases and the original1 theory failed; all name/earlier/native/
priority/boundary controls passed. Freeze MARK fixture
1e238d6f3cdbd5d4a1b70420dd9a1d9debc341a8 before compiler correction. Both frozen
fixtures remain unchanged.


Qualification requires all18 native results at68,025 tests/framework and every
configured Unicode/workload/extraction/POSIX/API/corpus/frozen/package/script gate.
Final GREEN, whole-branch author review and base/head/tree/proposed-merge identity
live on [PR238](https://github.com/uniblab/Icod.Expressions/pull/238) and issue7,
with repo docs included before qualification. Surrogate allowance, other syntax/
modes/directives/callouts, failed/partial labels, wider optimizer/resources (#102),
UTF16/exported API and realistic consumer/release gates remain open. User owns
merge/tag/publication; Grep stays last. See [design](PCRE-ALT-VERB-NAMES-DESIGN.md)
and [plan](PCRE-ALT-VERB-NAMES-PLAN.md).

## PR239: optional surrogate escapes and tolerant literals

Default-false internal AllowSurrogateEscapes implements PCRE2 10.48 extra-option1.
The parser snapshots it and gates three existing UTF surrogate checks in ordinary
braced hex/octal/N and standard/extra u grammar. Byte limits, values above10FFFF,
malformed syntax, uint overflow, owning offsets and caller restrictions retain
their existing paths. Adjacent surrogates remain separate code points; caller
policy survives reset scopes. Canonical integer literals/ranges retain widths,
negation and algebra. Surrogate folds are identity; valid Unicode17 mappings and
other out-of-range guards are unchanged. Non-Rune literal assertions use existing
general assertion lowering. EncodeCodePoint writes native surrogate bytes for
decoded labels and existing COMMIT first-code-unit metadata only.

Source and strict-subject TryRead validation remain unchanged. Native JIT also
allows encoded surrogate bytes through explicit literal comparisons in tolerant
mode while general classes/types reject them. A derived HasSurrogateLiterals
program property records emitted Literal instructions. Positive ordinary classes
with original Source=Literal surrogate singleton/equal-end range use the existing
Literal opcode; duplicate/range/algebra classes retain their typed paths. Only
when MatchInvalidUtf and this property are true, TryReadLiteral admits complete
ED/A0..BF/80..BF units for fragment scanning, literal comparison, nullable entry
and assertion rewind/retry positions. General scalar/class/type readers remain
strict. FF, truncated encodings and other malformed bytes remain hard barriers.
Labels and erased literals do not activate this path. Work/cancellation and all
journals/limits remain shared; no new opcode, instruction type or matching engine.

**Explicit boundary:** compiled tolerant surrogate literals combined with
WordBoundary, Backreference or ScriptRunCheck return UnsupportedSyntax at byte0.
These native malformed-context rules need separate qualification. The bounded
post-compilation scan polls cancellation within the existing4,096-instruction
ceiling. Strict UTF, label-only names and erased literals retain those constructs.
This deliberate restriction applies only to the new tolerant literal path.
NO_UTF_CHECK execution and UTF16 remain separate gates.

Nine production files change: PcreByteOptions.cs, PcreByteParser.cs,
PcreByteParser.AltBsux.cs, PcreByteParser.Groups.cs,
PcreByteParser.AltVerbNames.cs, PcreUtf8.cs, PcreUtf8CaseFold.cs,
PcreByteProgram.cs and PcreByteMatcher.cs. The program gains one derived metadata
property; AST/instruction types, opcodes, journals, shared limits, public API,
dependencies and workflows are unchanged. All253 earlier test files and frozen35/
214 outcomes/120 methods remain fixed.

The1,830 new cases include1,799 unique primary native profiles:1,332 matching or
explicit-boundary/strict controls,437 compile diagnostics and30 strict-subject
errors. Supplemental preflight checks10,240 JIT0 profiles (all2,048 label encodings
in three modes and all2,048 literal byte matches in two modes), three restriction
errors and4,500 selected offset protocols. Coverage includes numeric spellings/
bounds/priority, singleton versus duplicate/range/algebra classes, erased and
repeated bodies, fixed/variable and non-atomic assertions, calls/graphemes,
nonpairing, casing/UCP/Turkish/reset scopes, raw/decoded/inert labels, ownership,
hard malformed barriers and shared work/cancellation/capture/depth/expansion.
All-surrogate label fingerprint remains
F268065A0C6B161BC958261B53D01F16174B8107640F79B8C5A262DAF1E0690E.

Tests-only [RED723](https://github.com/uniblab/Icod.Expressions/actions/runs/43402553616),
source846c85ee20f193dd02c8d857343c0666508e0c38, LinuxARM job131809003628,
built net8/9/10 without compiler warnings/errors: each572 expected failures,
68,703 passes,69,275total,0skips. Only new missing-policy cases failed; native,
default and earlier controls passed. The unfrozen fixture first retained existing
outer-expression ownership for Perl/UTS errors without changing native expectations.
Freeze751bd09a727ca5ea584b48527538f5d77ccddd07 before the initial implementation.

CI724 then exposed22 retained failures per framework (10 native literal spans and
12 offset theories): the initial design incorrectly treated surrogate encodings
as barriers even for direct native literal comparisons. Keep the fixture fixed.
Add separate tests-only [RED725](https://github.com/uniblab/Icod.Expressions/actions/runs/43406280480),
sourcef5b742ba0c4d40e496dd5f94dfb4bbfc3e277ec9, LinuxARM job131821296383.
All frameworks built without compiler warnings/errors: each201 expected failures,
69,654 passes,69,855total,0skips. The added164 span profiles,6 explicit guards,
2 all-value theories,6 offset theories and1 work fact failed with the original22.
Native/strict/erased/label/default/earlier controls passed. Freeze second fixture
444f833a7190b5a41a9f4cf229bae89f3f939b56 before the matcher correction. Both fixtures
remain unchanged.

Qualification requires all18 native results at69,855 tests/framework and every
configured supporting gate. All repository inventories precede the final source
head; final GREEN, author review and exact base/head/tree/proposed-merge identity
live on [PR239](https://github.com/uniblab/Icod.Expressions/pull/239) and issue7's
roadmap footer without another documentation-only CI run. Tolerant surrogate
word/reference/script combinations, remaining syntax/modes/directives/callouts,
failed/partial labels, wider optimizer/resources (#102), UTF16/exported API and
realistic consumer/release gates remain open. User owns merge/tag/publication;
Grep last. See [design](PCRE-SURROGATE-ESCAPES-DESIGN.md) and
[plan](PCRE-SURROGATE-ESCAPES-PLAN.md).

## PR240: opt-in literal recovery for bad escapes

Default-false internal BadEscapeIsLiteral implements PCRE2 10.48 extra-option2.
The parser snapshots the option and wraps its existing escape reader. Successful
escapes and the disabled path retain that reader. Eligible InvalidSyntax or
UnsupportedSyntax failures restore pending reference state and consume only the
original escape letter (or a terminal backslash). Following source is parsed
normally: byte-invalid `\x{100}` becomes x repeated100 times, and `\g{0}` becomes
g repeated zero times. Original offsets, scopes and quantifier ownership survive;
there is no source rewriting, alternate AST, instruction or matching engine.

Eligibility follows native error stages, not the library's diagnostics alone.
A local, cancellable numeric probe uses native65535 bounds and current capture
count to distinguish malformed references from valid syntax beyond the internal
256-capture ceiling. Unresolved targets remain errors after parsing. Class
property failures and valid escapes forbidden as class members remain errors.
Perl class operands suppress recovery with finally restoration; UTS operands
allow recovery. Alternate verb-name decoding explicitly bypasses the option.
Native NeverBackslashC failures outside classes can recover; source UTF validation,
NeverUTF/UCP guards, resource diagnostics and cancellation retain their priority.
Valid unimplemented UTF code-unit execution/ordinary escaped UTF, fake option
prefixes and PR239's tolerant-surrogate exclusions remain explicit errors.

The new fixture has1,555 cases:1,150 native spans/captures/success labels,
383 native compile diagnostics,3 all-octet theories,12 newline/offset theories
and7 facts. Primary native profiles total1,533; supplemental native preflight
covers4,176 selected offset protocols. The octet theories compare768 values
native-first. Twenty-four unsupported native JIT UTF-C profiles stay separate;
negative matching evidence is native no-match(-1), never a JIT error. Coverage
includes all52 ASCII escape letters, data/control/numeric/alternate syntax,
references at both ceilings, properties and class/label contexts, caller policies,
case/reset/extended scopes, malformed UTF and source/options/label ownership.
Canonical-work equality, work limits, cancellation and capture/depth/expansion
ceilings are checked after recovery. Earlier255 test files remain unchanged.

The initial tests-only CI727 found five incorrect, unfrozen managed diagnostic
offsets: three byte-class errors own the escape slash rather than the UTF/algebra
opening, and two errors after a valid call own the later slash. Correcting those
expectations retained every case and native expectation. Refined tests-only RED
and the frozen fixture identity follow.

Refined tests-only [RED728](https://github.com/uniblab/Icod.Expressions/actions/runs/43415326648), source `a5551e54a4c9f4df6262524f5704481aae24bd21`, Linux ARM job131851844227: net8/9/10 each built with zero warnings/errors, then ran449 expected failures/70,961 passes/71,410total/zero skips. All failures were in the new fixture (424 accepted cases,5 post-recovery diagnostics,3 octet theories,12 offset theories and5 facts); every earlier test and1,106 new controls passed. Fixture `130f1ba203c0b0c64a479435ac418710448d0f19` froze before production and remains unchanged.


The baseline is merged239 `6e0d56f3962100f0f48657d9d7af61d203d46ad3`,
tree `25c5d9d3c4095f12e45f233234cf862758d2b755`,69,855 tests/framework;
main43410260851 and scripts43410260857 are green. Qualification requires all18
native results at71,410 tests/framework,214 frozen outcomes in120 methods,
zero failures/skips/compiler warnings/errors, frozen35 and all configured
Unicode/workload/extraction/POSIX/glibc/API/corpus/package/build-script gates.
No public API, dependencies, workflows, matcher, journal or shared-limit changes.
Whole-branch review, final GREEN and source/base/tree/proposed-merge evidence
belong on [PR240](https://github.com/uniblab/Icod.Expressions/pull/240) and only
issue7's roadmap footer. Repository inventories precede final qualification.
Remaining syntax/modes/directives/callouts, failed/partial labels, wider optimizer
and resource gaps (#102), NO_UTF_CHECK/UTF16/exported API, realistic consumer
performance and release gates remain open. User owns merge/tag/publication;
Grep migrates last and issue7 remains open.

## PR241: caller prohibition of manual callouts

Default-false internal NeverCallout implements PCRE2 10.48 extra-option0x8000.
The parser snapshots it and rejects real `(?C` atom tokens before callout
argument parsing. Numeric/string, malformed/incomplete, nested, zero-repeated,
unreachable and definition-body tokens all receive InvalidSyntax at their
original opening byte. Native203 reports the byte after C; internal diagnostics
retain the existing token-opening convention. The disabled path still explicitly
rejects unimplemented callout execution; no callout is silently ignored.

The existing atom dispatch follows quote/trivia processing and has the current
conditional frame. A leading conditional callout requires one byte after C to
reach native dispatch: truncated `(?(?C` has native128 first and retains the
existing internal UnsupportedSyntax at its inner opening. This does not parse
or execute conditional callouts. Source UTF/caller-mode validation and prior
syntax errors retain priority; later unresolved references and compiled repeat
expansion do not preempt the guard. Library capture/depth/expansion ceilings and
cancellation remain shared. BadEscapeIsLiteral does not recover group errors.
Caller reset/unset scopes cannot disable this immutable compile policy.

No global source scan is used. Quotes, comments, ordinary/Perl/UTS classes and
raw/decoded labels keep their own readers and canonical matching. Only three
production files change: options, parser initialization and Groups atom dispatch.
No AST/IR/opcodes, matcher/journal/shared-limit/exported-API/dependency/workflow
changes. Earlier256 test files and frozen35/214 outcomes/120 methods stay fixed.

The1,447 new cases include1,428 primary native profiles (168 matching spans/
captures/success labels and1,260 compile profiles), three quoted-octet theories,
12 newline/offset theories and four ownership/work/resource/capacity facts.
Native preflight adds768 quoted scalar/octet comparisons,2,820 selected offset
protocols and eight fact/capacity profiles. All256 bytes after a real header
are checked in byte/strict-UTF/tolerant modes; invalid source UTF wins first.
Four default leading-conditional callout profiles are native JIT-unsupported
and are separated from compiled matching/no-match evidence. Accepted matching
profiles require JIT0; negative matching results require native no-match(-1).
Guard errors occur before JIT. Native results and managed regression evidence
follow; the implementation plan records the same regression gate.

Refined tests-only [RED731](https://github.com/uniblab/Icod.Expressions/actions/runs/43425307698), source `3306ed9677a09d35e3f2cbf7f03cf72e850a5cf5`, Linux ARM job131885343849: net8/9/10 each built without compiler warnings/errors, then ran716 expected failures/72,141 passes/72,857total/zero skips. Only715 new guard diagnostics and one capacity fact failed; all71,410 earlier tests and731 new controls passed. Fixture `a95af4ea2353748653500573cfd680b8a5984ccd` froze before production and remains unchanged. Initial unfrozen RED730 had718 failures because two byte-range offsets incorrectly used the UTF opening convention; correcting only those offsets preserved every case and native expectation.


The baseline is merged240 `99a7a99baf52fdcb41db73e0dcfa21dd89c68c0f`,
tree `0f7215505e3a92510a15a5511d74baa0e881cc1e`,71,410 tests/framework;
main43422411224 and scripts43422411127 are green. Qualification requires all18
native results at72,857 tests/framework, zero failures/skips/compiler warnings/
errors,214 frozen outcomes in120 methods, frozen35 and every configured Unicode/
workload/extraction/POSIX/glibc/API/corpus/package/build-script gate. Repository
inventories precede final qualification. Final GREEN, whole-branch author review
and exact source/base/tree/proposed-merge identity belong on
[PR241](https://github.com/uniblab/Icod.Expressions/pull/241) and only issue7's
roadmap footer, without another documentation-only CI run.

Callout execution, lookaround backslash-K allowance, valid unimplemented UTF
code-unit/escaped-UTF syntax and PR239's tolerant-surrogate word/reference/script
exclusions remain open. Remaining syntax/modes/directives, failed/partial labels,
wider optimizer/resources (#102), NO_UTF_CHECK/UTF16/exported API, realistic
consumer performance and release gates remain open. User owns merge/tag/
publication, issue7 stays open and Grep migrates last. See
[design](PCRE-NEVER-CALLOUT-DESIGN.md) and [plan](PCRE-NEVER-CALLOUT-PLAN.md).

## PR242: caller casing policies and bounded marked-set search

Default-false internal CaselessRestricted and TurkishCasing implement PCRE2 10.48
extra-options0x80 and0x10000. Existing start settings initialize from the caller,
then exact source prefixes OR into them. Neither enables i/UTF, and original
bytes/consumed-prefix offsets remain authoritative. Local r set/unset/reset is
lexical; Turkish persists globally, with local r selecting restricted-default
comparisons. Global caller/source restriction conflicts with global Turkish
before body parsing. Turkish requires effective UTF in8-bit mode; source UTF
validation and NeverUTF/UCP priority retain their existing paths.

Initial integration changed only options and two start-setting initializers.
Existing parser/program/comparison consumers, Unicode17 folding and mode-specific
proofs receive the same policy as source prefixes. Tests-only RED733, source
c77092cbfb760f9c0e1452353d84bc68316773a5, LinuxARM job131907925887, built all
net8/9/10 with0warnings/errors: each678 expected failures/76,852 passes/77,530total/
0skips. Every earlier test and3,995 new controls passed. The first fixture
`94b6ffd9c834cbbc10ab03d97cf3c98b7e303c08` froze before that integration.

CI734 passed77,529/77,530 per framework, leaving one frozen ANY/tolerant/Turkish
split-newline offset. Native caller and source-prefix activation both give no
match when the requested byte lies inside LS before a marked scalar set. A
leading MARK stops native JIT scan_prefix before collecting a character, so
STARTLINE precedes FIRSTMAP. The existing linear proof covered following
literals but excluded ScalarSet. It now treats ScalarSet as consuming, retaining
leading MARK before consumption, AST first-caret requirement,1,000-instruction
cap, cancellation and all other opcode exclusions. Literal/scalar-before-MARK
priority is preserved. No matcher, opcode, journal or shared-limit changes.
Broader optimizer issue102 remains open.

Separate tests-only [RED735](https://github.com/uniblab/Icod.Expressions/actions/runs/43437776788),
source `3089e3febffc47eb0efdd364619788fbd0d75549`, LinuxARM job131927213552,
built net8/9/10 with0warnings/errors: each286 expected failures/78,832 passes/
79,118total/0skips. Failures were original1+281 new native-no-match split profiles
and4 new work/capture/budget cases; native/boundary/priority/default/strict/byte/
earlier controls passed. New fixture `2fc86257fe9c14bf84d94d6d982ccbdabc503b05`
froze before the compiler correction. Both fixtures remain unchanged.

Final increment:6,261 new cases, including6,236 primary native profiles (5,718
span/capture/success-label and518 compile profiles). Supplemental native evidence
includes776 complete class-membership comparisons,3,528 selected offset protocols
and24 work/ownership/capacity/budget profiles. All accepted profiles use JIT0 and
negative matching is no-match(-1). Cover all four case selections, I/i/İ/ı and
Kelvin/long-s pairs, scoped/reset options, comparison consumers, prefixes,
word/line modes, source/caller/global error priority, malformed offsets, owned
state, marked singleton/range/negative/predicate sets, wrappers, mark ordering,
no-caret/byte/strict controls and bounded resources/work/cancellation.

Only three production files change: options, start settings and the bounded
compiler proof. No AST/IR/opcode/matcher/journal/shared-limit/exported-API/
dependency/workflow changes. All257 earlier test files remain hash-identical,
as do GNU/BRE/ERE and frozen35/214 outcomes/120methods. Baseline merged241 is
`e27a301cc8493ac457db8c2a5f2b95fd58e3f2e8`, tree
`cbdd3ee4735486e5db4bb8a5c885a058ed9dc524`,72,857 tests/framework;
main43429623367/scripts43429623412 are green. Qualification requires all18 native
results at79,118 tests/framework,0failures/skips/compiler warnings/errors,
214frozen outcomes/120methods and every configured supporting gate. All repository
inventories precede qualification; final GREEN, author review and exact source/
base/tree/proposed-merge evidence live on [PR242](https://github.com/uniblab/Icod.Expressions/pull/242)
and only issue7's footer, without another documentation-only CI run.

Callout execution, lookaround backslash-K allowance, unimplemented UTF code-unit/
escaped-UTF syntax, PR239's tolerant-surrogate word/reference/script exclusions,
remaining syntax/modes/directives, failed/partial labels, wider optimizer/resources
(#102), NO_UTF_CHECK/UTF16/exported API, realistic consumer performance and release
gates remain open. User owns merge/tag/publication, issue7 stays open and Grep
migrates last. See [design](PCRE-CALLER-CASE-POLICIES-DESIGN.md) and
[plan](PCRE-CALLER-CASE-POLICIES-PLAN.md).

## PR243: optional assertion match-start resets

Default-false internal AllowLookaroundBackslashK implements native extra0x40 by snapshotting one parser boolean and gating the existing assertion prohibition. Canonical reset lowering and the shared assertion/call journal retain positive state and restore failed/negative/retried state. Local options/reset cannot alter the global policy. Only options/parser change; no matcher/IR/opcode/journal/shared-limit/exported API/dependency/workflow changes.

Reported internal group0 endpoints remain authoritative, including signed end-start when lookahead reports beyond its end and lookbehind reports before the requested offset. The oracle wrapper Length clamps, so the fixture compares EndIndex-Index. Other captures keep their own spans; no public iteration/extraction API is introduced.

The1,616 new cases include1,574 primary native profiles (1,510 matching/query/capture/success-label and64 compile profiles),1,032 selected offset protocols and16 supplemental work/capacity/implicit-guard profiles. All accepted native matching uses JIT0 and negative matching requires no-match(-1). Fifty-eight primary profiles report reversed group0 endpoints. Two UTF-C profiles outside native JIT support remain separate compile/unsupported boundaries. Another54 native review probes cover selected reset/i/r scopes, UTF/casing prefixes and lexical callee resets.

Tests-only [RED737](https://github.com/uniblab/Icod.Expressions/actions/runs/43444152881), source `552f6672bac9d57214d556d73fb76478429cd340`, Linux ARM job131949099519: net8/9/10 each built with zero compiler warnings/errors, then ran **1,406 expected failures / 79,328 passes / 80,734 total / zero skips**. Only missing-policy cases failed:1,366 primary spans,3 error boundaries,24 offset theories,4 work/ownership cases,2 capacity/depth facts,1 quota fact and6 implicit-guard profiles per framework. All79,118 earlier tests and210 new controls passed. All native literal endpoint assertions passed. Freeze `a6cd825ba357ca8be6f11fd3ff788032f0c24217` before production; no fixture refinements were needed.

Baseline merged242 is e3a772301a021a65797f44d5d397444661532288/tree525de953b3e6f05f4ef49caf783863bfd2179ace,79,118 tests/framework; main43442096467/scripts43442096348 are green. All259 earlier test files and GNU/frozen35/214 outcomes/120methods stay fixed. Final qualification requires all18 native results at80,734 tests/framework with0failures/skips/compiler warnings/errors and every configured supporting gate. All seven repository inventories precede final source qualification; final GREEN/review/exact identities live on PR243 and only issue7 footer, avoiding another documentation-only CI run.

Callout execution, unimplemented UTF-C/escaped-UTF syntax, PR239 tolerant-surrogate word/reference/script exclusions, remaining syntax/modes/directives, failed/partial labels, wider optimizer/resources102, NO_UTF_CHECK/UTF16/exported API and realistic consumer/release gates remain open. Issue7 stays open; user owns merge/tag/publish; Grep last. See [design](PCRE-LOOKAROUND-RESET-DESIGN.md) and [plan](PCRE-LOOKAROUND-RESET-PLAN.md).

## PR244: ordinary escaped UTF literal scalars

Use existing ParseQuotedLiteral at UTF non-ASCII escape entry after the trailing-backslash check, before ASCII dispatch. One complete validated scalar becomes the canonical literal. Remove the obsolete UnsupportedSyntax tail and update the recovery comment. All class/range/case/width/capture/reference/call/assertion/optimization consumers remain existing; no new flag, source rewrite, IR/opcode/matcher/journal/shared-limit/API/dependency/workflow change.

The993 new cases include956 primary native profiles (854 matching/query/capture/success-label and102 compile profiles),1,320 selected offset protocols and15 paired work/capacity profiles. Accepted native matching requires JIT0 and negative matching requires no-match(-1). Four UTF-C profiles outside native JIT support remain separate compile/unsupported controls.

Tests-only [RED739](https://github.com/uniblab/Icod.Expressions/actions/runs/43461757644), source `7198004337030e2d39008bc20843809b0fe5dc4d`, Linux ARM job132010183721: net8/9/10 each built with zero compiler warnings/errors and ran **687 expected failures / 81,040 passes / 81,727 total / zero skips**. Failures were619 primary profiles,35 later-error boundaries,24 offset theories,5 work/ownership cases,2 capacity/depth facts,1 quota and1 strict-subject fact per framework. All80,734 earlier tests and306 new controls passed; every native literal assertion passed. Freeze `1d6cf18667352f51c8d3a245560803af8fe1ab31` before production; no fixture refinements.

Two earlier facts intentionally retire temporary escaped-e-acute UnsupportedSyntax assertions. Their same inputs and caller selections now require pinned native0:2 acceptance and managed parity, with every unrelated quote/C/directive/surrogate guard retained. Both facts remain; the other258 earlier test files stay hash-identical. The frozen new fixture also covers those exact source/caller selections, guarding against a masked regression. No invented caller flag or escape-recovery interpretation is introduced.

Baseline2438d3221a738a4d69639dc1ac11e2d0459bf04c3f0/treee2ab37e841f9c0a207a1b9e170051693558ae906 has80,734 tests/framework; main43459503753/scripts43459504967 are green. GNU/frozen35/214 outcomes/120methods stay fixed. Final qualification requires all18 native results at81,727 tests/framework with0failures/skips/compiler warnings/errors and every configured supporting gate. All seven inventories precede qualification; final GREEN/review/exact source/base/tree/proposed-merge identities live on PR244 and only issue7 footer, without another documentation-only source push.

UTF-C execution, callouts, PR239 tolerant-surrogate word/reference/script exclusions, remaining syntax/modes/directives, failed/partial labels, wider optimizer/resources102, NO_UTF_CHECK/UTF16/exported API and realistic consumer/release gates remain open. Issue7 stays open; user owns merge/tag/publish; Grep last. See [design](PCRE-ESCAPED-UTF-LITERALS-DESIGN.md) and [plan](PCRE-ESCAPED-UTF-LITERALS-PLAN.md).

## PR245: caller-selected literal compilation

Caller-selected `Literal` treats the entire original pattern as data, including metacharacters, escapes, quoting, inline options, start directives, comments, captures, callouts and verbs. It reads bytes or validated UTF scalars into existing canonical literal nodes, retaining caller casing and the existing word/line wrappers. No source prefix changes UTF/UCP, newline/BSR, notempty or casing settings.

Pinned PCRE2 10.48 permits caller caseless, UTF/tolerant UTF, word/line and restricted/Turkish casing; newline/BSR remain compile-context choices. Its literal masks allow both casing extras even though the public manpage mentions only word/line. The other31 exposed regex options are rejected with InvalidSyntax at byte0 identifying literal compilation, before source validation. Existing cancellation/nonpositive-work-limit priority and subsequent whole-source UTF/Turkish validation remain shared. Default-false Literal preserves every earlier test hash.

Baseline244 `075e6adc284187fd8716dc109af130fc6e533cb9`/tree `328e5acad7569d300a752a6604a2d94a6ab90204` has81,727 tests/framework and261 prior test files, all fixed. PR245 adds3,633 tests for85,360/framework. All seven inventories precede qualification; final CI/review/identity evidence remains on PR245/issue7.

Tests-only [RED743](https://github.com/uniblab/Icod.Expressions/actions/runs/43483771907), source `02f144580c5af853578e682d3a5ae3a648252b2f`, Linux ARM job132086996194: net8/9/10 each built with zero compiler warnings/errors and ran **786 expected failures / 84,574 passes / 85,360 total / zero skips**. Failures were466 primary profiles,316 compatibility/error boundaries,1 work/ownership case and3 structural cases per framework. All81,727 earlier tests and2,847 new controls passed; every native assertion passed. Freeze `842c7045a4a7ed25d1f486f56702644eed8ac53c` before production. Before this clean RED, seven overlaps were deduplicated and the new helper's managed offset argument was corrected; no expected outcome was weakened.

See [design](PCRE-LITERAL-COMPILE-DESIGN.md) and [plan](PCRE-LITERAL-COMPILE-PLAN.md). GNU/BRE/ERE, frozen35/214 outcomes/120methods, IR/opcodes/matcher/journal/shared limits/exported API/dependencies/workflows stay fixed. Final all18 native results and every supporting gate, author review and exact source/base/tree/proposed-merge identities belong on PR245 and only issue7 footer; no documentation-only CI push. UTF-C execution, remaining syntax/modes/callouts/directives, broader102, UTF16/public API and consumer/release gates remain open; Grep last.

## PR246: caller-selected anchored compilation

Default-false caller `Anchored` implements PCRE2_ANCHORED0x80000000 by trying only the requested candidate byte offset. Alternatives, backtracking, assertions, recursive root0/callees, ACCEPT/cuts and labels remain normal at that candidate. A reported start moved by backslash-K may differ from the original candidate. Caller literal compilation, casing, source prefixes and word/line/newline policies remain shared.

The compiler snapshots the option into immutable program policy metadata after ordinary compilation. The matcher gates unanchored byte/suffix/COMMIT entry, admits only the requested candidate and stops after its failure before SKIP/fragment/CRLF retries. Existing UTF/offset validation and fragment context remain intact. The parser, IR nodes/opcodes, scopes/journal and shared ceilings are unchanged; no source-level backslash-G wrapper or subject slicing changes recursion or capacity.

Merged245 `03c105b5a13890059f20ff51a074f67d180a20f6`/tree `4868b519b9205cf6327fc501f86e041836ccf023` has85,360 tests/framework and262 prior files. Main43575611111/scripts43575611113 are green. PR246 adds325 tests for85,685/framework; all earlier hashes remain fixed. All seven inventories precede source qualification, with final evidence external on PR246/issue7.

Tests-only [RED746](https://github.com/uniblab/Icod.Expressions/actions/runs/43578255592), source `33ed1a4a6e5344fa53df7aeb03c6c462ab371706`, Linux ARM job132397293572: net8/net9/net10 each built with zero compiler warnings/errors and ran **131 expected failures / 85,554 passes / 85,685 total / zero skips**. Failures were128 protocol groups,2 bounded-byte-budget cases and1 ownership case/framework. Every pinned native assertion, all85,360 earlier tests and194 new controls passed. Freeze `95620f1478537b71880c07c64e20474c3de537ad` before production. Before clean RED, one new byte-range diagnostic control was corrected to preserve its existing hyphen offset2; native108/offset4 and all earlier tests remained unchanged.

See [design](PCRE-ANCHORED-COMPILE-DESIGN.md) and [plan](PCRE-ANCHORED-COMPILE-PLAN.md). All262 prior test files and frozen35 production blobs remain hash-fixed; GNU/BRE/ERE and214 frozen outcomes/120 methods are preserved. Final all18 native results, every configured supporting gate, whole-branch author review and exact source/base/tree/proposed-merge identities belong on PR246 and only issue7 footer, without a documentation-only CI push. ENDANCHORED/FIRSTLINE, UTF-C execution, callouts, remaining syntax/modes/directives, tolerant-surrogate exclusions, failed/partial labels, broader102, NO_UTF_CHECK/UTF16/public API and consumer/release gates remain open; Grep last.

## PR247: caller-selected end-anchored compilation

Default-false caller `EndAnchored` implements PCRE2_ENDANCHORED0x20000000. Root completion must consume through the physical subject end. Short ordinary completion backtracks through existing alternatives; short root `(*ACCEPT)` abandons the entire candidate, discarding its choices/cuts before unanchored restart. Assertion and callee ACCEPT remain scoped. Caller Anchored independently constrains candidate selection, and Literal/casing/word/line/newline/source policies compose normally.

The compiler snapshots EndAnchored alongside Anchored in immutable program metadata after ordinary compilation. Normal ACCEPT gates consumed position against input.Length before publishing captures/labels. Root EarlyAccept checks after synthetic capture closes and scoped exits, discarding saved choices only when short. Tolerant empty patterns use shared candidate traversal under EndAnchored, so fragment ends and reported backslash-K starts never substitute for physical EOF. Parser/root0/callee IR/opcodes/scopes/journal/shared ceilings stay unchanged; no source wrapper or subject slicing.

Merged246 `153273df6fd58266599e0dc495f15da4f51b3bda`/tree `62a2e3ee4994368a362a044ab98dc67f02aab3a9` has85,685 tests/framework and263 prior files. Main43583523079/scripts43583523076 are green. PR247 adds565 tests for86,250/framework; all earlier hashes remain fixed. All seven inventories precede source qualification; final evidence remains external on PR247/issue7.

Tests-only [RED748](https://github.com/uniblab/Icod.Expressions/actions/runs/43585144086), source `7140659a6924969d2fa5dd4ea658cfc49bcc2719`, Linux ARM job132417911226: net8/net9/net10 each built with zero compiler warnings/errors and ran **385 expected failures / 85,865 passes / 86,250 total / zero skips**. Failures were384 missing-policy protocol groups and1 ownership case/framework, all after successful native assertions. All85,685 earlier tests and180 new controls passed. Freeze `2e39a5a936a8358973932a230d53f37abd7c9199` before production; no fixture correction or prior-test change was needed.

See [design](PCRE-END-ANCHORED-COMPILE-DESIGN.md) and [plan](PCRE-END-ANCHORED-COMPILE-PLAN.md). All263 prior test files and frozen35 production blobs remain hash-fixed; GNU/BRE/ERE and214 frozen outcomes/120 methods are preserved. Final all18 native results, configured supporting gates, whole-branch author review and exact source/base/tree/proposed-merge identities belong on PR247 and only issue7 footer, without a documentation-only CI push. FIRSTLINE, UTF-C execution, callouts, remaining syntax/modes/directives, tolerant-surrogate exclusions, failed/partial labels, broader102, NO_UTF_CHECK/UTF16/public API and consumer/release gates remain open; Grep last.
