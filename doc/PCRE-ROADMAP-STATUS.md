# PCRE roadmap status

Reviewed after merged PR237, with PR238 in progress. Issue #7 remains the delivery roadmap; the
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

PR237 merged at f9b28d2a9a31342a46b3e92679d82ce11f8f5028;
main43388625282 and scripts43388625167 are green. Its66,451 tests/framework
remain the baseline. PR238 adds alternate verb names as described below;
final qualification and delivery evidence are recorded on the PR and issue7.

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
