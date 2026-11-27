# PCRE roadmap status

## Native 16-bit subroutine calls and recursion

The [subroutine record](PCRE-WORD-SUBROUTINES.md) adds numbered/relative/named
calls, DEFINE blocks, forward and recursive targets, whole-pattern recursion
and selected returned captures. Integer call edges reuse compiled source bodies;
return guards restore caller captures while retaining callee retry choices.
237 fixtures add 121,344 full-vector PCRE2-16 policy comparisons. Counted call
repeats also receive the native final-required empty-progress check in both
widths. Conditions and verbs are next; quantified DEFINE blocks remain open.

## Native 16-bit atomic groups and possessive repeats

The [atomic-group record](PCRE-WORD-ATOMIC-GROUPS.md) adds consuming atomic
groups and aliases, repeated atomic groups, and possessive atom/group/reference
counts. Commit guards keep consumption and successful captures, prune inner
choices and preserve outer rollback through the existing iterative runner.
339 fixtures add 173,568 full-vector PCRE2-16 policy comparisons. Native capture
checks also corrected final-required empty progress in counted group repeats
in both byte and word execution. Calls,
conditions and verbs are next.

## Native 16-bit direct assertion repeats

The [assertion-repeat record](PCRE-WORD-ASSERTION-REPEATS.md) adds greedy,
lazy and possessive direct quantifiers to atomic and non-atomic lookarounds.
Normalized finite iterations preserve capture growth at an unchanged subject
position; optional exits and possessive commits reuse existing retry guards.
243 fixtures add 124,416 full-vector PCRE2-16 policy comparisons. General
atomic groups and consuming possessive repetition are next, then calls,
conditions and verbs.

## Native 16-bit non-atomic assertions

The [non-atomic assertion record](PCRE-WORD-NONATOMIC-ASSERTIONS.md) adds
resumable positive lookahead and fixed/bounded-variable lookbehind. Successful
bodies retain their choices and capture checkpoints for later suffix failure.
Retry frames also restore native variable-lookbehind endpoint visibility.
132 fixtures add 67,584 full-vector PCRE2-16 policy comparisons. Directly
quantified assertions are next, followed by calls, conditions and verbs.

## Native 16-bit atomic lookbehind

The [lookbehind record](PCRE-WORD-LOOKBEHIND.md) adds positive and negative
fixed/bounded-variable assertions. Original branch widths drive whole-scalar
or raw-unit rewinds; body choices retry before shorter lengths. Assertion
guards retain successful captures, restore failed attempts and preserve
atomic commitment. The strict validation window now includes retained
lookbehind widths. 143 fixtures add 73,216 full-vector PCRE2-16 policy
comparisons. Non-atomic and directly quantified assertions remain next,
followed by calls, conditions and verbs.

## Native 16-bit atomic lookahead

The [lookahead record](PCRE-WORD-LOOKAHEAD.md) adds positive and negative
atomic assertions, nested bodies and original-position restoration. Positive
success keeps captures and prunes internal alternatives; negative assertions
restore attempted captures. Guard frames, pruning and rollback share the
existing work and depth limits. 111 fixtures add 56,832 full-vector PCRE2-16
policy comparisons. Lookbehind and the remaining non-atomic/directly
quantified assertion forms are next, followed by calls and verbs.

## Native 16-bit backreferences

The [backreference record](PCRE-WORD-BACKREFERENCES.md) adds numbered,
relative and named references, including greedy/lazy repetition, earlier
iteration captures and reference-local casing. Comparisons read original
subject memory and charge every captured scalar/raw unit and duplicate-name
lookup to the shared work quota. Repeated references reuse the existing
group loop and empty-progress guards. 107 fixtures add 54,784 full-vector
PCRE2-16 policy comparisons. Lookarounds are the next execution work,
followed by calls and verbs.

## Native 16-bit captures and rollback

The [capture record](PCRE-WORD-CAPTURES.md) adds numbered/named captures,
branch-reset numbering and duplicate-name lookup to the nested word executor.
Retry frames restore completed spans through a match-local journal; successful
results own immutable capture slots and share the compiled name metadata.
UTF names use the original word source and native code-unit length limit.
116 fixtures add 59,392 full-vector PCRE2-16 policy comparisons, plus
cross-width values, resource, ownership and concurrency checks. Backreferences
are the next execution work, followed by lookarounds, calls and verbs.

## Native 16-bit repeated noncapturing groups

The [repeated-group record](PCRE-WORD-REPEATED-GROUPS.md) adds greedy and
lazy finite/unbounded group counts, nested iteration continuations and
empty-progress guards. Bodies compile once; runtime retries share the work
quota and a new 1,024-frame ceiling with a distinct depth diagnostic. Eighty
fixtures add 40,960 PCRE2-16 policy comparisons. Static choice/node bounds
count retained bodies once, independently of iteration count. Capture
rollback and durable capture results are the next execution work.

## Native 16-bit nested noncapturing alternatives

The [nested-alternative record](PCRE-WORD-NESTED-ALTERNATIVES.md) adds
noncapturing groups, ordered branches and scoped options to the iterative
word executor. Return contexts preserve repeat/alternative ordering across
group boundaries, including finite suffix choices and policy-rejected
completions. Sixty fixtures add 30,720 PCRE2-16 policy comparisons. Groups
share the existing path, repeat and expanded-node bounds, with nesting
limited to 64. Repeated groups, empty-progress rules and capture rollback
are the next execution work.

## Native 16-bit explicit repeat frames

The [repeat-frame record](PCRE-WORD-REPEAT-FRAMES.md) replaces recursive
continuation matching and construction with iterative execution and extends
sequential unbounded atoms from four to 64 per path. Finite choices retain
the shared 64-path bound. Sixty fixtures across eight defaults and 64 call
masks add 30,720 PCRE2-16 comparisons, with exact quota, cancellation and
state-reset checks. General nested groups/repeats and capture rollback are
the next execution work; broader newline search and grapheme parity remain
open.

## Native 16-bit single-unit newline and BSR profiles

The [newline-profile record](PCRE-WORD-NEWLINE-PROFILES.md) adds effective
LF, CR and NUL conventions and the independent Unicode/ANYCRLF BSR selectors
through the shared prefix parser. Forty fixtures across eight defaults and
64 call masks add 20,480 PCRE2-16 comparisons. Effective CRLF, ANYCRLF and
ANY conventions still need candidate-advancement and prefix-proof parity;
graphemes, general nested execution and durable captures remain open.

## Native 16-bit newline sequences

The [newline-sequence record](PCRE-WORD-NEWLINE-SEQUENCES.md) adds `\R`
with the default Unicode BSR profile, including atomic CRLF consumption and
whole-sequence rewinds at every supported repeat level. Forty fixtures
across eight defaults and 64 call masks add 20,480 PCRE2-16 comparisons.
BSR directives, other global newline conventions, graphemes, general nested
execution and durable captures remain open.

## Native 16-bit LF line assertions

The [line-assertion record](PCRE-WORD-LINE-ANCHORS.md) adds `^`, `$`, `\Z`
and inline multiline mode under the existing LF convention. Forty fixtures
across eight defaults and 64 call masks add 20,480 PCRE2-16 comparisons,
including `NotBol`/`NotEol`, finite choices and repeat continuations.
Other newline conventions, general nested execution and durable captures
remain open.

## Native 16-bit word boundaries

The [word-boundary record](PCRE-WORD-BOUNDARIES.md) adds `\b` and `\B`
using ASCII or pinned Unicode word membership, including finite choices
and all four supported repeat depths. Forty fixtures add 15,360 PCRE2-16
comparisons. Strict validation now includes the preceding character when
a boundary or `\A` occurs anywhere in the pattern. General nested
execution, durable captures and broader newline/grapheme handling remain open.

## Native 16-bit four unbounded repeats and five choice levels

The [four-repeat record](PCRE-WORD-FOUR-UNBOUNDED.md) extends the common
bounded builder to four unbounded atoms with finite choices at every
depth. Forty-three fixtures across six defaults and 64 call masks add
16,512 PCRE2-16 comparisons. Five or more repeats, general nested
execution and durable captures remain open.

## Native 16-bit finite choices around three unbounded repeats

The [three-repeat choice record](PCRE-WORD-THREE-REPEAT-CHOICES.md) admits
finite variable counts at every depth around three unbounded atoms. A
shared bounded builder retains the existing zero-, one- and two-repeat
ordering, and the combined choice product remains at most 64 paths.
Fifty fixtures across six defaults and 64 call masks add 19,200 PCRE2-16
outcomes. Four or more unbounded repeats, general nested execution and
durable captures remain open.

## Native 16-bit three unbounded atom continuations

The [three-repeat record](PCRE-WORD-THREE-UNBOUNDED.md) admits three
unbounded atoms in a straight top-level path, including exact counts and
assertions between them. Fifty-one fixtures across six defaults and 64 call
masks add 19,584 PCRE2-16 outcomes. Finite variable choices after the first
repeat in a three-repeat path, further repeats, general nested execution
and durable captures remain open.

## Native 16-bit finite choices around two unbounded repeats

The [nested choice record](PCRE-WORD-TWO-REPEAT-CHOICES.md) admits finite
variable counts before, between and after two unbounded atoms. Choice
products remain bounded at 64 across top-level paths, and each finite
choice is tried at its correct greedy or lazy repeat depth. Forty-nine
fixtures across six defaults and 64 call masks add 18,816 PCRE2-16
outcomes. Three or more unbounded repeats, general nested execution and
durable captures remain open.

## Native 16-bit two unbounded atom continuations

The [two-repeat record](PCRE-WORD-TWO-UNBOUNDED.md) admits two unbounded
atoms in a straight top-level path, including exact counts and assertions
between them. Greedy or lazy second-repeat counts retry inside each
first-repeat count. Forty-nine fixtures across six defaults and 64 call
masks add 18,816 PCRE2-16 outcomes. Finite variable choices after the
first repeat, more than two unbounded atoms, general nested execution and
durable captures remain open.

## Native 16-bit unbounded repeat with finite suffix choices

The [word suffix-choice record](PCRE-WORD-UNBOUNDED-SUFFIX-CHOICES.md)
admits finite variable counts after a single unbounded atom, trying the
ordered suffix choices at every greedy or lazy unbounded count. Prefix and
suffix choice products share the global 64-path admission bound. Forty-nine
fixtures across six defaults and 64 call masks add 18,816 PCRE2-16
outcomes. Multiple unbounded repeats, nested execution and durable captures
remain open.

## Native 16-bit unbounded repeat continuations

The [word continuation record](PCRE-WORD-UNBOUNDED-CONTINUATIONS.md) admits
one unbounded atom before a straight suffix, with subject-driven greedy or
lazy retries and scalar rewinds. Forty-six fixtures across six defaults and
64 call masks add 17,664 PCRE2-16 outcomes. Multiple unbounded repeats,
finite variable suffix choices, nested execution and durable captures remain
open.

## Native 16-bit unbounded tail atom

The [word tail-repeat record](PCRE-WORD-TAIL-UNBOUNDED.md) admits one
subject-driven `*`, `+` or `{m,}` atom at the end of a top-level path,
with lazy or greedy preference and a minimum up to 64. Forty-four fixtures
across six defaults and 64 call masks add 16,896 PCRE2-16 outcomes.
Unbounded continuations, nested execution and durable captures remain open.

## Native 16-bit multiple bounded variable repeats

The [word multiple-repeat record](PCRE-WORD-MULTIPLE-REPEATS.md) admits
several finite variable atom counts per top-level path. The product of
ordered count choices is bounded by the existing 64-path limit. Forty-three
fixtures across six defaults and 64 call masks add 16,512 PCRE2-16 outcomes.
Unbounded repeats, nested execution and durable captures remain open.

## Native 16-bit bounded variable repetition

The [word variable-repeat record](PCRE-WORD-VARIABLE-REPEATS.md) admits one
finite variable-count atom per top-level path, with greedy or lazy ordered
count retries and a 64-path global bound. Forty-seven fixtures across six
defaults and 64 call masks add 18,048 PCRE2-16 outcomes. Unbounded or multiple
variable repeats, nested execution and durable captures remain open.

## Native 16-bit fixed-count atom repetition

The [word repeat record](PCRE-WORD-FIXED-REPEATS.md) admits exact counts up to
64 for supported atoms by bounded compile-time expansion. Each branch holds
at most 4,096 nodes; the ordinary match work counter charges each comparison.
Fifty fixtures across six defaults and 64 call masks add 19,200 PCRE2-16
outcomes. Variable repeats, nested execution, durable captures and full
cross-width parity remain open.

## Native 16-bit ordered top-level alternatives

The [word alternative record](PCRE-WORD-ALTERNATIVES.md) admits up to 64
ordered top-level straight branches. Each candidate start tries source
branches in order, with empty-match and end-anchored policy rejection
allowing later branches at the same start. Fifty-four fixtures across six
defaults and 64 call masks add 20,736 PCRE2-16 outcomes in raw and strict
UTF-16 modes. Repetition, nested execution and captures remain open.

## Native 16-bit caseless literals and classes

The [word caseless record](PCRE-WORD-CASELESS.md) admits caller and inline
caseless matching for straight literals, bounded brackets and pinned scalar
predicates. Raw mode without UCP uses ASCII folding; strict UTF-16 and raw
UCP use pinned Unicode 17 simple folding while preserving original code-unit
coordinates. Fifty-two fixtures across six defaults and 64 call masks add
19,968 PCRE2-16 outcomes. The following alternative increment adds ordered
top-level branches; repetition and captures remain open.

## Native 16-bit UCP shorthand and POSIX classes

The [word UCP class record](PCRE-WORD-UCP-CLASSES.md) binds the shared
Unicode-aware `\d`, `\w`, `\s`, POSIX and special-property predicates to
raw words and strict UTF-16 scalars. Caller UCP and the five ASCII
restrictions select membership without changing subject representation.
Seventy-five fixtures across six defaults and 64 call masks add 28,800
PCRE2-16 outcomes. The following caseless increment adds folding; ordered
execution and captures remain open.

## Native 16-bit binary and bidi properties

The [word binary and bidi record](PCRE-WORD-BINARY-BIDI.md) admits the pinned
Unicode 17 binary properties and bidi classes as standalone atoms and bounded
bracket predicates. Fifty-one raw/strict fixtures across six compiled defaults
and 64 call masks add 19,584 native PCRE2-16 comparisons; an 80-name sweep
checks both modes. The following UCP class increment adds Unicode-aware
shorthand/POSIX classes; caseless matching, ordered execution and captures
remain open.

## Native 16-bit Unicode script properties

The [word script record](PCRE-WORD-SCRIPTS.md) admits primary script and
script-extension predicates as standalone atoms and bounded bracket members.
Raw units and strict UTF-16 scalars use the pinned Unicode 17 script table.
Forty-one fixtures across six defaults and 64 masks yield 15,744 native
comparisons, plus a 175-name primary/extended sweep in both modes. Binary
and bidi properties follow in the next record; UCP shorthand, casing,
ordered execution and captures remain open.

## Native 16-bit Unicode general categories

The [word general category record](PCRE-WORD-GENERAL-CATEGORIES.md) admits
group and specific Unicode categories in standalone atoms and bounded
bracket sets, with complements and original 16-bit capture coordinates.
Raw surrogate units and strict UTF-16 scalars use the pinned Unicode table.
Forty-seven fixtures across six defaults and 64 masks supply 18,048 native
comparisons, plus a 38-name category sweep in both modes. Binary,
UCP shorthand and casing work remains open, as do ordered execution and
captures.

## Native 16-bit fixed whitespace classes

The [word fixed whitespace record](PCRE-WORD-FIXED-SPACE.md) admits `\h`,
`\H`, `\v` and `\V` as standalone atoms and bounded bracket predicates.
The shared PCRE2 whitespace lists apply to raw units and strict UTF-16
scalars without UCP. Forty-one fixtures across six defaults and 64 call
masks add 15,744 native comparisons. Binary and UCP classes,
caseless word matching, ordered execution and captures remain open.

## Native 16-bit ASCII shorthand and POSIX classes

The [word ASCII class record](PCRE-WORD-ASCII-CLASSES.md) adds `\d`, `\w`, `\s`
and their complements, inside or outside bracket sets, plus default ASCII
POSIX predicates. Shared membership runs on raw word units or strict UTF-16
scalars, with up to 64 combined ranges and predicates per bracket set.
Thirty-six fixtures across six defaults and 64 masks provide 13,824 native
comparisons. Unicode-aware properties, casing,
ordered branches and captures remain follow-on work.

## Native 16-bit literal character classes

The [word class record](PCRE-WORD-CLASSES.md) admits bounded literal, range
and negated bracket classes through the shared scalar-range node. Raw mode
reads 16-bit units and strict UTF-16 reads scalars. Twenty-two fixtures across
six compiled defaults and 64 call masks supply 8,448 native comparisons, with
four managed cross-width checks. Predicate/caseless classes, ordered branches,
captures and the broad P10–P12 gates remain open.

## Native 16-bit dot in straight sequences

The [word dot record](PCRE-WORD-DOT.md) admits dot through the shared parser
and original word-subject reader. Raw mode consumes one unit; strict UTF-16
mode consumes one scalar. LF and dot-all behavior match the bounded native
matrix of 8,448 policy outcomes, with four managed cross-width checks and
exact work-step controls. Wider newline policies, classes, branching and
captures remain open.

## Native 16-bit regex source spelling

The [word source record](PCRE-WORD-PATTERN-SOURCE.md) admits direct BMP,
supplementary and raw surrogate literals through the shared parser while
retaining original pattern code-unit offsets. Its 18 fixtures and all six
compiled defaults and 64 call policies add 6,912 live native16 comparisons;
four valid-Unicode cases also compare managed widths. Broader native16
classes, branches, captures and the P10–P12 release gates remain open.

## ASCII and UTF inventory (16-bit assertion update)

Revision 4 of *Icod.Expressions Architecture Critique and Production Development Roadmap*
(2026-09-03, pp. 8–9, 24–25 and 35–42) calls for one PCRE language with two native
code-unit widths. ASCII data can already be searched directly as bytes. The largest
remaining implementation gap is general regex execution on .NET strings. The byte
engine is substantially further along, with its compatibility and release review
still open. This inventory was first recorded after #326 and now includes merged
[PR #363](https://github.com/uniblab/Icod.Expressions/pull/363) and the
subroutine/recursion increment above. Qualification evidence for earlier
milestones remains dated below.

### What callers can exercise today

PCRE entry points are internal. The public, extracted GNU Basic, Extended and Emacs
providers already accept string and byte subjects, preserve source coordinates and
select the leftmost-longest match. PCRE selects the first successful ordered path.
The two families need separate compatibility evidence. [Public provider contract](../README.md#description)

| PCRE mode | Implemented behavior | Outstanding work |
|---|---|---|
| Raw 8-bit, including ASCII | The parser and bounded, iterative matcher operate on original bytes, including NUL and 0x80–0xFF. They implement ordered alternatives, repetition, captures with rollback, backreferences, lookarounds and control verbs. Results use byte offsets. | Freeze the accepted syntax, options, diagnostics and resource behavior against PCRE2-8. Qualify both ASCII-heavy searches and raw high-byte cases. |
| UTF-8 | Forward and reverse scalar reads preserve byte offsets. The engine supports strict validation, tolerant matching across valid fragments, Unicode properties, UCP classes, case folding, word boundaries, graphemes and newline conventions within the tested profile. | Close strict and tolerant error/start-offset coverage, remaining optimizer differences and the byte performance baseline. Decide the UTF code-unit escape and callout contracts. |
| Raw 16-bit | Literal and bounded sequence entry points read original units, including isolated surrogates. The shared parser admits native non-ASCII spelling, dot, bounded scalar classes, absolute and LF/CR/NUL line assertions, word boundaries, newline sequences with independent BSR selectors, ordered paths, nested and repeated groups, finite atom counts and up to 64 retained unbounded repeats, using explicit retry, return and iteration frames. Numbered/named captures have journaled rollback and immutable results; numbered/relative/named backreferences support repetition. Atomic positive/negative lookahead and fixed/bounded-variable lookbehind preserve captures and original positions. Non-atomic positive assertions can resume retained body choices. Direct greedy/lazy/possessive assertion repeats preserve finite count and capture semantics. Consuming atomic groups and possessive atom/group/reference repeats commit inner choices while retaining outer capture rollback. Numbered/relative/named calls, DEFINE blocks, selected returned captures and recursion reuse retained bodies and caller capture checkpoints. Caseless and UCP predicates use pinned Unicode tables. | Add conditions and verbs; qualify the broader grammar and package consumer. |
| Strict UTF-16 | The same two entry points read original `char` memory directly and match supplementary scalars. The admitted sequence grammar includes classes, caseless predicates, word boundaries, LF/CR/NUL line assertions, atomic newline sequences with BSR selectors, ordered paths, nested and repeated groups, finite atom counts and explicit retry/return/iteration frames with empty-progress guards. Numbered/named captures use journaled rollback and immutable results; names read original UTF-16. Backreferences compare captured scalars with reference-local casing and support repetition. Atomic and non-atomic lookahead and fixed/bounded-variable lookbehind share scalar-aware bodies and the capture journal, including native endpoint visibility. Direct greedy/lazy/possessive assertion repeats retain finite count and capture behavior. Consuming atomic groups and possessive repeats keep scalar/unit consumption, capture state and bounded commitment. Calls, DEFINE blocks, selected returned captures and recursion share scalar-aware bodies and the capture journal. Starts, diagnostics and all capture spans use 16-bit code units. | Add graphemes, CRLF/ANYCRLF/ANY newline search behavior, conditions and verbs. Freeze invalid-surrogate behavior, including the disposition of tolerant mode. |

Three choices matter for interpreting these rows. First, raw bytes and UTF-8 both
represent ASCII directly, while UCP and its ASCII restrictions separately control
what expressions such as `\d` match. Keeping Grep's ASCII-digit default and its `(?-aD)`
override is part of its consumer contract. Second, the bounded 16-bit regex
entry accepts native non-ASCII pattern spelling and preserves code-unit offsets.
Third, strict byte matching validates the whole subject, while the
current strict word sequence validates from the requested start, extended by
the largest retained lookbehind width or at least one character when a word
boundary or `\A` needs it. Preserve the native validation window as grammar
and call targets expand.
[Byte syntax matrix](PCRE-SYNTAX-ACCEPTANCE-MATRIX.md) ·
[Word literals](PCRE-WORD-LITERAL.md) · [Word sequences](PCRE-WORD-SEQUENCES.md)

### What the evidence establishes

- **Extraction and foundation (A00–N05).** The accepted A05/E02 comparison preserves
  35 source blobs, 214 focused cases and a 44-case parity corpus. E03 has a
  package-only probe and 725 scratch bridge cases across five consumers. E03/E04
  still need actual migrations and ownership closure. N00–N05 still need distinct
  strict BRE/ERE contracts, a composable Gnulib syntax/profile API, shared
  text/set/analysis infrastructure and a foundation freeze. Experimental PCRE work
  has advanced ahead of those gates. [Extraction ledger](EXTRACTION-READINESS.md)
- **Byte engine (P00–P09).** PCRE2 10.48 provides native 8- and 16-bit references,
  with Perl 5.40.1 used for declared overlap. The byte engine has pinned Unicode 17
  data, a compiled internal API snapshot, package checks, allocation probes and
  seven Grep component workloads. PRs #321–#323 added 49,152 bounded policy
  comparisons with reduction, 393,216 malformed UTF-8 comparisons and 131,072
  seeded grammar comparisons. Each count describes its own corpus. The 8-bit
  freeze still needs an explicit profile and deviation review, broader hostile
  input coverage and representative performance evidence.
  [Internal contract](PCRE-INTERNAL-CONTRACT.md) ·
  [Policy sweep](../tests/Pcre/Native/policy-differential-reduction.md) ·
  [Malformed inputs](../tests/Pcre/Native/malformed-policy-differential.md) ·
  [Seeded grammar](../tests/Pcre/Native/generated-policy-differential.md)
- **Word engine (P10–P12).** PRs #324–#326 established bidirectional native
  navigation, literal matching and the first reuse of the regex parser.
  The literal and sequence suites each compare 7,680 outcomes against PCRE2-16.
  Managed cross-width checks cover selected valid literals and sequences. The
  #326 run passed 108,406 .NET 10 tests on each of six OS/architecture hosts,
  built the library for .NET 8/9/10 and passed 37 existing PCRE package checks.
  A production-facing word package consumer and general cross-width parity are
  still outstanding. [Navigation evidence](PCRE-UTF16-NAVIGATION.md) ·
  [Qualified #326 run](https://github.com/uniblab/Icod.Expressions/actions/runs/45110631402)

### Recommended delivery order and completion criteria

1. **Broaden word execution (P10/P11).** Extend the capture/reference-aware
   nested branch/repeat executor from atomic/possessive groups and quantified
   lookarounds and calls to conditions and verbs. Verify
   empty retries, CRLF, unset captures and original code-unit coordinates at
   each step. Expand the admitted newline and grapheme grammar and preserve
   cancellation and budget exhaustion across each new path.
3. **Finish the byte freeze alongside the word work (P00/P01/P09).** Record
   supported, rejected and deferred forms with native fixtures, including
   callouts and UTF `\C`. Expand the pinned corpus and Perl overlap with provenance.
   Resolve or document the remaining UTF optimizer and malformed-span
   differences. Recheck package behavior, allocations and controlled-host timings
   on ASCII, Unicode and high-byte workloads. [Optimizer issue #102](https://github.com/uniblab/Icod.Expressions/issues/102) ·
   [Malformed-span issue #100](https://github.com/uniblab/Icod.Expressions/issues/100) ·
   [Workload protocol](../tools/PcreWorkloadComparison/README.md)
4. **Prove semantic parity (P12).** Run equivalent valid patterns and subjects
   through both managed widths and both pinned native oracles. Map offsets to
   scalar positions independently in tests. Compare status, all capture spans
   and values, unset groups, names, marks, lookbehind, calls and resource
   classifications. Retain native offsets in failure records. Give malformed
   bytes and unpaired surrogates separate native-width expectations.
   [Coordinate fixtures](../tests/Pcre/PcreDualWidthCoordinateTests.cs)
5. **Qualify the full profile and deliver it (P13–P18).** Implement the agreed
   soft/hard partial, empty-match iteration and budgeted substitution behavior.
   Check partial offsets at truncated input, advancement after an empty match,
   and output limits for named and numbered substitutions. Test the agreed callout
   behavior. Exercise compile, frame, journal and heap limits, cancellation,
   optimized/reference parity and bounded fuzzing with minimized regressions.
   Measure native byte and string workloads separately and rerun BRE/ERE benchmarks
   when changing shared code. Freeze the public
   APIs and validate package-only byte and string consumers alongside BRE/ERE.
   Keep application behavior in its owning projects. Migrate Grep last after its
   command-level, performance and six-RID package/archive gates pass. Retire the
   CommandFramework copy only after publication and validation of every known
   migration. [Bridge record](E03-Compatibility-Bridge.md) ·
   [Grep acceptance inventory](PCRE-GREP-CONSUMER-FIXTURES.md)

## Native width-16 shared-parser sequences

The [sequence record](PCRE-WORD-SEQUENCES.md) binds bounded ASCII regex source
to the existing parser with a 16-bit numeric character domain. Straight
literal/escape sequences and absolute assertions execute over original word
subjects. The native matrix covers 7,680 outcomes, with bounded managed
cross-width parity and quota checks. Non-ASCII regex source, broader lowering,
backtracking and release qualification remain open; literal mode is preserved.

## Native width-16 literal execution

The [word literal record](PCRE-WORD-LITERAL.md) adds bounded case-sensitive
literal compilation and matching over original 16-bit input. Raw and strict
UTF modes reuse shared literal nodes and call policies, with native offsets,
owned compiled values, deterministic work and cancellation. The native
matrix covers 7,680 outcomes; valid-Unicode literals also receive bounded
managed cross-width checks. General regex lowering/backtracking, wider
UTF-16 policies and release qualification remain open.

## Native width-16 navigation foundation

The [UTF-16 navigation record](PCRE-UTF16-NAVIGATION.md) adds direct raw-unit
and strict-scalar reads in both directions, plus bounded compiled-literal
encoding. Exhaustive managed tests and 4,456,448 independent native outcomes
pin the representation boundary. This is a P10 building block; native16
compilation, matching, resource/diagnostic policy and cross-width release
qualification remain open. No shared grammar or byte execution changes.

## PR313 — interpreted FIRSTLINE restart boundaries

The [first-line record](PCRE-INTERPRETED-FIRST-LINE-STATUS.md) starts an
interpreted FIRSTLINE scan at each valid UTF-8 fragment or admitted forward
SKIP target. The corpus adds 3,072 native outcomes across 48 definitions
and two resource cases, bringing the suite to 107,567 tests. A thirtieth
package smoke check uses the same outcomes.

## PR312 — interpreted forward SKIP newline admission

The [forward-skip record](PCRE-INTERPRETED-FORWARD-SKIP-STATUS.md) applies
`FIRSTLINE` and CRLF restart rules to interpreted forward SKIP. The corpus adds
3,072 native outcomes across 48 definitions. Two resource cases bring the suite
to 107,517 tests. A twenty-ninth package record checks the same policies.
Broader roadmap gates remain open.

## PR311 — ordered replay for interpreted named SKIP

The [replay record](PCRE-INTERPRETED-SKIP-REPLAY-STATUS.md) preserves the order
of named SKIP visits when the interpreter retries a candidate. The corpus adds
3,072 native outcomes across 48 definitions. Two resource cases bring the suite
to 107,467 tests. A twenty-eighth package record checks the same policies.
Broader roadmap gates remain open.

## PR310 — named control verb prefixes and unmatched SKIP

The [verb-prefix record](PCRE-VERB-PREFIX-ANCHOR-SKIP-STATUS.md) recognizes
source start anchors after leading MARK and named backtracking verbs. The
corpus adds 3,072 native outcomes across 48 definitions. Two resource cases
bring the suite to 107,417 tests. A twenty-seventh package record checks the
same policies. Broader roadmap gates remain open.

## PR309 — skipped static condition prefixes and unmatched SKIP

The [static-prefix record](PCRE-STATIC-PREFIX-ANCHOR-SKIP-STATUS.md) recognizes
source start anchors after a false one-branch conditional and its ignored
quantifiers. The corpus adds 3,072 native outcomes across 48 definitions. Two
resource cases bring the suite to 107,367 tests. A twenty-sixth package record
checks the same policies. Broader roadmap gates remain open.

## PR308 — exact-zero repeat prefixes and unmatched SKIP

The [zero-repeat record](PCRE-ZERO-REPEAT-ANCHOR-SKIP-STATUS.md) recognizes a
source start anchor after leading repeats with a maximum of zero. The corpus
adds 3,072 native outcomes across 48 definitions. Two resource cases bring
the suite to 107,317 tests. A twenty-fifth package record checks the same
policies. Broader roadmap gates remain open.

## PR307 — mandatory-repeat source anchors and unmatched SKIP

The [repeat-anchor record](PCRE-REPEAT-ANCHOR-SKIP-STATUS.md) recognizes source
start anchors through positive-minimum repeats, including possessive wrappers.
The corpus adds 3,840 native outcomes across 60 definitions. Two resource cases
bring the suite to 107,267 tests. A twenty-fourth package record checks the same
policies. Broader roadmap gates remain open.

## PR306 — assertion-condition source anchors and unmatched SKIP

The [assertion-condition record](PCRE-ASSERTION-CONDITION-ANCHOR-SKIP-STATUS.md)
checks a positive forward guard and its explicit else branch for source start
anchors. The corpus adds 3,456 native outcomes across 54 definitions. Two
resource cases bring the suite to 107,205 tests. A twenty-third package record
checks the same policies. Broader roadmap gates remain open.

## PR305 — conditional source anchors and unmatched SKIP

The [conditional-anchor record](PCRE-CONDITIONAL-ANCHOR-SKIP-STATUS.md) checks
both branches of capture, recursion, and version conditionals for source start
anchors. The corpus adds 3,392 native outcomes across 53 definitions. Two
resource cases bring the suite to 107,149 tests. A twenty-second package record
checks the same policies. Broader roadmap gates remain open.

## PR304 — explicit atomic group source anchors and unmatched SKIP

The [atomic-anchor record](PCRE-ATOMIC-ANCHOR-SKIP-STATUS.md) recognizes source
start anchors through explicit atomic groups, including nested alternatives.
The corpus adds 2,560 native outcomes across 40 definitions. Two resource cases
bring the suite to 107,094 tests. A twenty-first package record checks the same
policies. Broader roadmap gates remain open.

## PR303 — positive lookahead source anchors and unmatched SKIP

The [lookahead-anchor record](PCRE-LOOKAHEAD-ANCHOR-SKIP-STATUS.md) recognizes
source start anchors through positive lookaheads, including non-atomic forms.
The corpus adds 2,304 native outcomes across 36 definitions. Two resource cases
bring the suite to 107,052 tests. A twentieth package record checks the same
policies. Broader roadmap gates remain open.

## PR302 — alternative source anchors and unmatched SKIP

The [alternative-anchor record](PCRE-ALTERNATIVE-ANCHOR-SKIP-STATUS.md) checks
all leading paths through capturing and noncapturing groups for source start
anchors. The corpus adds 2,048 native outcomes across 32 definitions. Two
resource cases bring the suite to 107,014 tests. A nineteenth package record
checks the same policies. Broader roadmap gates remain open.

## PR301 — capture prefixes and source-anchor SKIP

The [capture-prefix record](PCRE-CAPTURE-ANCHOR-SKIP-STATUS.md) recognizes
source start anchors behind mandatory leading capture entries for interpreted
unmatched-name handling. The corpus adds 2,048 native outcomes across 32
definitions. Two resource cases bring the suite to 106,980 tests. An eighteenth
package record checks the same policies. Broader roadmap gates remain open.

## PR300 — direct source start anchors and unmatched SKIP

The [source-anchor record](PCRE-START-ANCHOR-SKIP-STATUS.md) extends the
interpreted unmatched-name stop check to direct absolute-start and ordinary
caret anchors. The corpus adds 2,048 native outcomes across 32 definitions.
Two resource cases bring the suite to 106,946 tests. A seventeenth package
record checks the same policies. Broader roadmap gates remain open.

## PR299 — interpreted named SKIP assertion propagation

The [assertion propagation record](PCRE-ASSERTION-SKIP-STATUS.md) resolves
named SKIP history before choosing a negative or conditional assertion catch.
The corpus supplies 2,048 native outcomes across 32 definitions. Two resource
checks bring the suite to 106,912 tests. A sixteenth package record checks
the same policies. Broader conformance and roadmap gates remain open.

## PR298 — anchored interpreter and unmatched named SKIP

The [unmatched-name record](PCRE-UNMATCHED-SKIP-STATUS.md) preserves failure
before an anchored interpreted attempt can replay an unmatched named SKIP.
The corpus supplies 2,048 native outcomes across 32 definitions. Two resource
checks bring the suite to 106,878 tests. A fifteenth package record checks
the same policies. Broader conformance and roadmap gates remain open.

## PR297 — interpreted named SKIP and returned call history

The [returned-history record](PCRE-RETURNED-SKIP-STATUS.md) makes interpreted
root named-SKIP lookup see MARK history retained by returned subroutine calls.
The native corpus contains 2,048 outcomes across 32 definitions. Two resource
checks bring the suite to 106,844 tests. A fourteenth package record exercises
the same policies. Broader conformance and roadmap gates remain open.

## PR296 — compiled-anchor reference routing

The [compiled-anchor record](PCRE-COMPILED-ANCHOR-STATUS.md) separates native
compile defaults from call-time anchor flags. The C probe and shared native
test adapter apply this routing. The corpus adds 3,072 native outcomes and
48 tests, bringing the suite to 106,810 cases. A thirteenth package record
checks the same compiled-default and call-policy combinations.

## PR295 — interpreted THEN assertion catches

The [THEN scope record](PCRE-INTERPRETED-THEN-STATUS.md) covers failed THEN
inside a called positive assertion. Interpreted matching catches the assertion
and can retry an earlier callee choice. The corpus contains 3,072 native
outcomes and 52 new tests, bringing the suite to 106,762 cases. The twelfth
package record checks the same definitions.

## PR294 — interpreted callee cut capture rollback

The [capture rollback record](PCRE-INTERPRETED-CUT-STATUS.md) covers selected
return captures when a called group fails after a control verb. Match-time
anchor dispatch restores the caller values through the existing journal.
The corpus contains 3,072 native outcomes and 52 new tests, bringing the
suite to 106,710 cases. The eleventh package record uses the same corpus.

## PR293 — compiled internal consumer contract

The [internal contract](PCRE-INTERNAL-CONTRACT.md) records eleven consumer
boundary types, option defaults, diagnostic values, and current acceptance
gaps. PR and main CI compare the compiled declaration snapshot alongside the
public extraction API. Existing runtime and package cases provide behavioral
evidence. Broader P09 conformance, hostile-input, and performance closure remains open.

## PR292 — interpreted word boundaries at malformed UTF starts

The [interpreted boundary addendum](PCRE-INTERPRETED-BOUNDARY-STATUS.md)
records 3,072 native outcomes and 52 tests. Match-time anchoring allows word
boundary assertions to evaluate the empty initial malformed UTF fragment.
The tenth package smoke record runs the same corpus against the packed library.

## PR291: interpreted optional lookbehind at UTF fragment barriers

The [optional rewind addendum](PCRE-OPTIONAL-REWIND-STATUS.md) records bounded
P09 conformance across five inventories. Interpreted tolerant variable lookbehind
uses shorter available context once its minimum is met; retain default/JIT
barrier rejection, required context, retry order/state/work and original bytes.
Native48x64=3,072 outcomes /52 additions gives106,606 tests. Observe RED/freeze
before the one-guard correction; append ninth package record. See [design](PCRE-OPTIONAL-REWIND-DESIGN.md),
[plan](PCRE-OPTIONAL-REWIND-PLAN.md) and [PR291](https://github.com/uniblab/Icod.Expressions/pull/291).
Whole-branch/baseline review precedes qualification; final evidence/completion
stays external without source/docs/test edits after GREEN. #7/#102/footer fixed;
broader P09/native16/P10-P18/P00/P01/Stage C/migrations remain open. Grep last.

## PR290: multiline caret at interpreted UTF fragment ends

The [fragment caret addendum](PCRE-FRAGMENT-CARET-STATUS.md) records this bounded
P09 increment across all five inventories. Multiline caret without alternate-
caret rejects a current interpreted fragment end; preserve default/JIT behavior,
physical z/Z, scoped limits, original offsets and shared work/state. Native48x64=
3,072 outcomes /52 additions gives106,554 tests. Observe semantic RED/freeze
before the guard change; append eighth package record. See [design](PCRE-FRAGMENT-CARET-DESIGN.md),
[plan](PCRE-FRAGMENT-CARET-PLAN.md) and [PR290](https://github.com/uniblab/Icod.Expressions/pull/290).
Whole-branch/baseline review precedes qualification; final evidence/completion
stay external without source/docs/test edits after GREEN. #7/#102/footer fixed;
broader P09/native16/P10-P18/P00/P01/Stage C/migrations stay open. Grep last.

## PR289: COMMIT fragment restarts and physical end anchors

The [COMMIT fragment addendum](PCRE-COMMIT-FRAGMENT-STATUS.md) records this
bounded P09 hostile-input increment across all five shared inventories. A reached
root COMMIT abandons the current interpreted tolerant fragment, allowing later
valid fragments; physical z/Z remains distinct from call EndAnchored acceptance.
Existing default/scoped cuts, state, work and byte coordinates are preserved.

Pinned native52x64=3,328 outcomes /56 additions gives106,502 tests. Semantic
tests-only RED32/106,470 passes follows all native/default comparisons;24 new
controls and all106,446 old tests pass. New catalog/assertion/XML sources freeze
before the one-file matcher correction. A seventh package record reuses the
unchanged consumer and all six earlier records. See [design](PCRE-COMMIT-FRAGMENT-DESIGN.md),
[plan](PCRE-COMMIT-FRAGMENT-PLAN.md), addendum and
[PR289](https://github.com/uniblab/Icod.Expressions/pull/289). Whole-branch/baseline
review precedes final qualification; final identities/raw metrics/results/plan
completion/cleanup/ready remain external without source/docs/test edits after
GREEN. #7/#102/footer fixed; broader P09/native16/P10-P18/P00/P01/Stage C and
actual migrations stay open. Grep last; user merges/tags/publishes.

## PR288: immutable per-call match policy

The [per-call policy addendum](PCRE-MATCH-POLICY-STATUS.md) records this P09
increment across all five shared inventories. One compiled byte pattern accepts
six additive per-call restrictions without program cloning or shared mutation.
Preserve absolute anchors, compiled/source defaults, original bytes, validation,
work and cancellation; defer the same bounded optional-query pruning to effective
empty policy. Native56x64=3,584 outcomes /67 additions gives106,446 tests.

Refined tests-only RED67/106,379 passes follows complete native/default checks;
new catalog/consumer/assertion/C/XML/value sources freeze before implementation.
The sixth package record requires56 definitions/3,584 protocols, preserving all
five earlier records. Whole-branch review and all final six-host/full/frozen/
all-target/package/support gates remain required. See [design](PCRE-MATCH-POLICY-DESIGN.md),
[plan](PCRE-MATCH-POLICY-PLAN.md), addendum and
[PR288](https://github.com/uniblab/Icod.Expressions/pull/288). Final identities/raw
metrics/results/completion/cleanup/ready stay external without source/docs/test
edits after GREEN. #7/#102/footer fixed; broader P09/native16/P10-P18/P00/P01/
Stage C and actual migrations remain open. Grep last; user merges/tags/publishes.

## PR287: named captures on bound match results

The [named result inventory addendum](PCRE-NAMED-CAPTURE-RESULT-STATUS.md) records
this P09 increment for all five shared inventories. Successful results bind the
compiled metadata and expose first-participating native named-substring spans.
Empty/unset/absent states, ordinal lookup and original bytes remain explicit;
parser reference ordering and all matching/work behavior stay fixed.

Independent native preflight976 outcomes /985 new tests gives106,379 total.
Corrected tests-only RED is707 /105,672 passes /zero skips after native/existing
span checks; new source blobs freeze before binding. Require final six-host
full/frozen/five-package/build/support gates. See [design](PCRE-NAMED-CAPTURE-RESULT-DESIGN.md),
[plan](PCRE-NAMED-CAPTURE-RESULT-PLAN.md), addendum and
[PR287](https://github.com/uniblab/Icod.Expressions/pull/287). Final identities/raw
metrics/results/completion/cleanup/ready stay external without source/docs/test
edits after GREEN. #7/#102/footer fixed; broader P09/native16/P10-P18/P01/Stage C
and actual migrations remain open. Grep last; user owns merge/tags/publication.

## PR286: immutable internal capture metadata

The [capture metadata inventory addendum](PCRE-CAPTURE-METADATA-STATUS.md) records
this P09 increment for all five shared inventories. Compiled patterns gain owned,
stable read-only capture counts and ordinal name-to-slot mappings; execution,
parser declaration order and existing diagnostics/work remain fixed.

Independent pinned native preflight has214 count/name/match protocols. All-six
successful-build RED isolates222 missing-accessor failures /105,172 passes /
zero skips /105,394 total, with all old/native match checks passing. Freeze new
assertion/data/consumer/C/XML sources before implementation. Whole-branch review
and final six-host/full/frozen/four-package/support gates remain required.

See [design](PCRE-CAPTURE-METADATA-DESIGN.md),
[plan](PCRE-CAPTURE-METADATA-PLAN.md) and
[PR286](https://github.com/uniblab/Icod.Expressions/pull/286). Final identities,
results/completion/cleanup/ready stay external without source/docs/test edits
after GREEN. #7/#102/footer fixed; broader P09/native16/P10-P18/P01/Stage C and
actual migrations remain open. Grep last; user owns merge/tags/publication.

## PR285: bounded anchored tolerant UTF fragment storage

Tolerant UTF validation currently records every valid fragment before matching.
This increment retains only the first real fragment ending at/after the requested
byte when caller or proven dot-star anchoring already prevents later ordinary
retry. Preserve real end-at-start context and existing synthetic nullable entries.
Unanchored fragment collection/search stays identical. Full subject validation,
every Step, strict diagnostics, cancellation, captures and forward SKIP remain.
No streaming rescan, transcoding, parser/proof/VM change or new public quota.

Independent pinned PCRE2/SLJIT preflight records 1,568 all-offset short protocols
(364 groups) plus 12 large-subject outcomes, all JIT zero/positive size. Six
paired allocation profiles exercise plain, observed and cancellable paths;
eight controls preserve whole-validation budgets/cancellation/strict late errors.
390 additions give 105,171 tests. Observed successful-build RED on all six hosts
is exactly 18 allocation-growth failures /105,153 passes /zero skips, with native/
old cases passing first. New data/assertion/consumer/C/XML sources were frozen
before the three anchored fragment-retention calls and private guard changed.

The isolated package gate adds six raw allocation profiles while preserving
42+1+2 and the earlier four-profile results, fresh feeds/caches and all workflow
call sites. At 1KiB/64KiB require at most 4KiB extra per call after warmup.
No claim extends to unanchored fragments, backtracking/capture storage, native
memory, controlled-host timing or the entire P09 release contract.

See [design](PCRE-ANCHORED-FRAGMENT-STORAGE-DESIGN.md),
[plan](PCRE-ANCHORED-FRAGMENT-STORAGE-PLAN.md) and
[PR285](https://github.com/uniblab/Icod.Expressions/pull/285). Review the whole
branch/seven inventories before final six-host 105,171/frozen214/120, all target
builds and actual package/compatibility/Gnulib/support/script gates, with zero
failures/skips/coded compiler warnings/errors. Final raw metrics/identities,
completion/cleanup/ready remain external on PR285; no source/docs/test edits
after GREEN. Prior C# tests/A05 and #7/#102/footer fixed; sole old-test-file
exception adds two Compile links. Wider P09, native16/P10-P18, remaining P01,
Stage C and actual migration remain open. Grep last; user merges/tags/publishes.

## PR284: paired-input allocation evidence

Bounded P09 evidence now compares four anchored raw/UTF/UCP paths at1KiB and
64KiB, with exact native captures independently established by eight pinned
PCRE2 C probes (JIT0/positive size). The package-only consumer prepares inputs/
patterns outside measurement, uses original padded byte views and validates
complete results during every callback. No matcher/producer change is needed.

A synchronous current-thread managed-allocation probe warms both sizes,
alternates eight measured calls per size and records raw totals. Allow at most
4KiB additional allocation per call. Whole-byte-copy and UTF16-decode fault
controls must exceed that allowance. Callback bounds, original-view identity,
argument guards, cancellation/errors and no-op allocation controls are explicit.
This is covered-profile growth evidence, not absolute zero allocation, native/
retained heap accounting, timing acceptance or proof for every pattern/mode.

21 additions give104,781 total. Successful-build RED isolates21 missing-helper
failures /104,760 passes /zero skips, with native assertions passing first.
Freeze profile/assertion/C/XML sources before implementing only the new helpers;
freeze completed helpers afterward. All376 old C# test blobs and the producer
stay fixed; the sole old-test-file exception adds three Compile source links.

The existing isolated package action retains its42+1+2 protocol checks and
requires a second structured result with four exact profile names, sizes,
iterations, nonnegative counters and passing growth bounds on all six hosts.
Existing workflow call sites, release behavior and package dependencies stay
fixed. Actual configured compatibility/Gnulib/support checks remain mandatory.

See [design](PCRE-ALLOCATION-EVIDENCE-DESIGN.md),
[plan](PCRE-ALLOCATION-EVIDENCE-PLAN.md) and
[PR284](https://github.com/uniblab/Icod.Expressions/pull/284). Review the whole
branch and all seven inventories before final six-host104,781/frozen214/120,
package/allocation and support qualification with zero failures/skips/coded
warnings/errors. Final identities, raw metrics, cleanup and completion stay
external on PR284; no source/docs/test edits after GREEN. Wider P09 contract/
hostile/performance gates, native16/P10-P18, remaining P01, Stage C and actual
consumer migration remain open. #7/#102/footer fixed; Grep last; user merges,
tags and publishes.

## PR283: internal PCRE package-only consumer gate

P09 now has a bounded package-consumer contract using the actual candidate
NuGet binary. The explicitly named Icod.Expressions.PackageSmoke friend keeps
PCRE internal; it is not a new public API. Existing GNU/POSIX sample assertions
remain intact. No matcher/compiler/Unicode source changes are introduced.

32 protocols reuse immutable native-first raw/UTF/UCP fixtures, including
binary/NUL input, supplementary values, captures, lookarounds, repeats and
marks. Ten managed diagnostic/resource cases, one observed-view control and
two cancellation controls give 45 additions (104,760 total). Padded memory
views and overwritten caller pattern buffers exercise authoritative view
coordinates and compiler ownership. The observed callback check is specific
path evidence, not a blanket allocation/performance/no-transcoding freeze.

Observed successful-build RED isolates exactly 45 missing-consumer failures /
104,715 passes /zero skips; every native literal assertion passes. Freeze the
sample case table, new assertions and test-project Compile links before
implementing only the exercise; freeze that completed helper afterward. All
375 prior C# test blobs and A05 source35/corpus stay fixed; the sole prior-test
file exception adds two source links to the existing test project XML.

One common action packs to a fresh unique feed, restores only that candidate
with an isolated cache, builds net8/net9/net10 and executes net10 on all six
native PR/main hosts. Require the structured42-protocol/one-view/two-cancellation
record with zero failures. Main's artifact verification and release workflow
remain unchanged. Metadata/sample/workflow paths require actual full
compatibility/Gnulib gates, not classifier skips.

See [design](PCRE-PACKAGE-CONSUMER-DESIGN.md),
[plan](PCRE-PACKAGE-CONSUMER-PLAN.md) and
[PR283](https://github.com/uniblab/Icod.Expressions/pull/283). Whole-branch review
and all seven inventories precede final six-host104,760/frozen214/120 methods,
package, compatibility and support qualification with zero failures/skips/
coded warnings/errors. Final identities/evidence, cleanup and completion stay
external on PR283; no source/docs/test edits after GREEN. P09 remains open for
the wider contract/hostile/performance freeze; native16/P10-P18, remaining P01,
Stage C and actual consumer migration remain open. Keep #7/#102/footer fixed;
Grep last; user owns merge/tags/publication.

## PR282: pinned Perl overlap and bounded reducer

P00/P01 gains a declared Perl v5.40.1 / Unicode 15.0.0 comparison scope and
an original development runner, with 340 literal overlap cases (221 matches,
119 no-matches). All 680 independently built pinned PCRE2-8/16 probes agree
with Perl's scalar captures, retaining each native start/capture coordinate.
No mismatching rows were discarded. The corpus covers ordered choice,
repetition, captures/references, assertions, recursion/conditions, Unicode
literals and search starts. It makes no general Perl/PCRE or UCD15/17 parity
claim; wider modes, error/mark/callout/partial/iteration families remain open.

CI compares the literals through native8/native16, the direct native16 bridge
and managed bytes. The pinned Perl interpreter remains an independent
reproducible development preflight; no new CI dependency/workflow is added.
All cases/runner are original, with source-release and actual binary provenance.

A test-only native-unit reducer owns its buffers, preserves the start offset,
uses one explicit oracle-evaluation budget and propagates cancellation/errors.
Deterministic chunk deletion reaches a one-unit local fixed point, rather
than promising a globally smallest pair. The callback decides mismatch class
and rejects invalid/unrelated candidates. Byte/char/ushort controls and a
native-backed coordinate-comparison fault supply 31 reducer cases.

371 additions bring the full suite to 104,715. Observed successful-build RED
isolates 31 missing-reducer failures / 104,684 passes / zero skips. Freeze all
three new case/assertion sources and the Perl runner before implementing only
the reducer, then freeze its completed helper. Preserve all 379 old test files /
371 C# sources, the production tree and A05 source35/corpus.

See [design](PCRE-PERL-OVERLAP-REDUCER-DESIGN.md),
[plan](PCRE-PERL-OVERLAP-REDUCER-PLAN.md) and
[PR282](https://github.com/uniblab/Icod.Expressions/pull/282). Whole-branch review
and all seven inventories precede final six-host qualification: all three
library targets, full net10 plus frozen214/120 methods, zero failures/skips/
coded warnings/errors and configured support/package/script gates. Final
identities/evidence, cleanup and completion stay externally on PR282 without
source/docs/test edits after GREEN. P01 remains partial; P09-P18, Stage C and
actual consumer migrations remain open. #7/#102 and issue #7 footer stay fixed;
Grep last; user owns merge/tags/publication.

## PR281: direct raw 16-bit native reference

A test-only adapter now calls the existing PCRE.NET 1.6.0 native
16-bit exports, with explicit raw/UTF options and effective-option checks.
This removes the public string API's forced-UTF restriction from the reference
oracle. It does not implement managed 16-bit execution or change production
source, public API, dependencies, packaging or workflows.

Independent pinned PCRE2 10.48/SLJIT probes provide 136 raw/UTF protocols:
54 matches, 62 no-matches, 14 compile errors and six match errors. Reuse all
150 frozen dual-width UTF cases. Include isolated surrogate code units,
starts inside surrogate pairs, wide literals/classes, UCP, references,
recursion/conditions, lookarounds, signed K ranges, names/marks and source UTF
controls. Native code-unit offsets remain native; no production transcoding.

The bounded C64 ABI copies owned capture/name/mark results before releasing
native programs and buffers; compile/match errors remain distinct from no-match.
ABI/ownership facts and six managed-input guards bring the addition to
294 tests (104,344 total). Successful-build tests-only RED isolates exactly
294 missing-adapter failures / 104,050 passes / zero skips. Freeze the two
case/assertion sources before implementing only the helper, then freeze it.

All 368 prior C# test sources, old C harness, A05 source35/corpus and production
tree stay fixed. The only prior test-file exception corrects the native README
include path to the generated pinned header and links the new bridge. All
310 old protocols and 136
new outcomes reproduce unchanged. See [design](PCRE-RAW16-NATIVE-ORACLE-DESIGN.md),
[plan](PCRE-RAW16-NATIVE-ORACLE-PLAN.md) and
[PR281](https://github.com/uniblab/Icod.Expressions/pull/281).

Whole-branch review and all seven inventories precede final six-host
qualification: net8/net9/net10 builds, full net10 plus frozen 214 cases /
120 methods, zero failures/skips/coded warnings/errors and all configured
support gates. Final source/merge identities, evidence, cleanup and completion
remain external on PR281; no source/docs/test edits after GREEN. Full P01,
Perl/reducer, P09-P18, Stage C normalization and real consumer migration remain
open. Keep #7/#102 and issue #7 footer unchanged; Grep last; user owns merge,
tags and publication.

## PR280: native dual-width fixture contract

P00/P01 now has one reusable native UTF-8/UTF-16 case catalog with explicit
pattern/subject encodings, native start offsets, both capture protocols,
normalized scalar captures, names, marks and compile errors. The existing
managed byte matcher is checked against its pinned byte results. There is
no managed 16-bit matcher, public API, dependency or workflow change.

An independent pinned PCRE2-8/16 C harness records 140 shared matching cases,
ten compile errors, eight width-specific malformed-subject errors and two raw
16-bit witnesses. All matching compiles require JIT zero/positive size.
PCRE.NET 1.6.0 remains the six-host CI oracle; its string API always adds UTF,
so raw 16-bit results are not misrepresented as string-API coverage.

The test-only coordinate adapter uses independent Rune decoding, not matcher
navigation. It rejects malformed/different scalar inputs, split byte/scalar
or surrogate boundaries, invalid endpoints/protocols and arithmetic overflow.
Original byte/UTF-16 offsets remain pinned separately; empty/unset and reversed
ranges stay distinct. Arrays and encoding conversion belong only to test
infrastructure; no production subject transcoding is introduced.

202 new cases (104,050 total) comprise 150 shared/diagnostic rows, 44 coordinate
controls and eight width-specific errors. Observed tests-only RED exactly isolates
194 missing-adapter failures / 103,856 passes; all native assertions, 103,848
prior tests and eight malformed-subject controls pass. Five fixture/assertion
sources are frozen before implementing only the helper; its final blob is frozen too.
Preserve all 367 prior test files / 362 C# sources and A05 source 35/corpus.

See [design](PCRE-DUAL-WIDTH-FIXTURE-CONTRACT-DESIGN.md),
[plan](PCRE-DUAL-WIDTH-FIXTURE-CONTRACT-PLAN.md) and
[PR280](https://github.com/uniblab/Icod.Expressions/pull/280). Whole-branch review
and all seven inventories precede six-host qualification: net8/net9/net10
builds, full net10 plus frozen 214 cases / 120 methods, zero failures/skips/coded
warnings/errors and all configured support gates. Final identities/evidence and
plan completion stay external without source/docs/test edits after GREEN.
Raw 16-bit CI, Perl/reducer and full P01 closure remain open; P09-P18, Stage C
normalization and real consumer migration are not closed by this contract.
Keep #7/#102 and issue #7 footer unchanged; Grep last; user merges/tags/publishes.


## Revision 4 stocktake (2026-11-18)

Re-read all 50 pages of *Icod.Expressions Architecture Critique and Production
Development Roadmap - Revision 4* (2026-09-03), supplied by the user.
Source PDF SHA256: `4048f7622b9d93137d341b4890e9a3b358501cddca7fc3ea04f7c95e6611796d`.
This is a gate assessment, not a percentage inferred from PR or test counts.

| Roadmap gate | Current evidence | Disposition / remaining gate |
|---|---|---|
| A00-A05 in-situ analysis/freeze | [Accepted A05](Icod.Expressions-A05-Final-Candidate.md), [readiness ledger](EXTRACTION-READINESS.md), unchanged 35 source blobs, 214 focused cases / 120 methods and 44-case parity corpus. | Accepted **bounded** baseline; the original A01-A04 standards/profile/provenance gaps remain classified. Not full standards conformance. |
| E00-E02 extraction | Frozen transplant and exact same-run old/new comparison; source/API/corpus verifiers remain CI gates. | Complete within the accepted A05 scope. |
| E03-E04 consumer migration/closure | [Bridge record](E03-Compatibility-Bridge.md), package-only smoke and 725-case scratch bridge checks across five consumers. | Partial. Disposable probes are not actual downstream migrations or a distributable production bridge. CommandFramework ownership removal remains blocked; Grep last. |
| N00-N01 strict POSIX/Gnulib contracts | Public production API remains the inherited CommandFramework namespaces and three GNU-style providers. | Open. Separate strict BRE/ERE facades, complete composable syntax-bit/profile model and independent standards/profile closure are not implemented by the unchanged extraction. |
| N02-N05 common substrate/foundation freeze | Exact input coordinates, preserved POSIX/GNU runtime; PCRE has pinned Unicode/UTF helpers and separate parser/VM. | Partial building blocks, not a completed common HIR/analysis seam or normalized foundation freeze. Do not refactor frozen legacy source as part of a PCRE proof fix. |
| P00-P01 compatibility/oracles | PCRE.NET 1.6.0 / pinned PCRE2 10.48; native8/native16 and normalized fixtures (#280/#281); bounded Perl 5.40.1 overlap and reducer skeleton (#282). | Partial. Broader declared Perl divergence/error coverage and reduction/fuzz closure remain open. |
| P02-P08 parser, VM, captures, advanced syntax, UTF-8/Unicode | Internal dedicated parser, iterative explicit-frame ordered VM, capture journal, references/recursion/conditions, lookarounds/verbs, native byte offsets, UTF validation, properties/scripts/graphemes/class algebra. | Substantial implementation with explicit acceptance gaps. Callout execution, UTF code-unit escape behavior and remaining syntax/invalid-input/source-shape cases prevent claiming full gate closure. |
| P09 8-bit/UTF-8 freeze | Merged #283 has 104,760 tests and six-host isolated internal package consumers; #284 adds four paired-input allocation profiles with copy/decode fault controls. | Partial. Full internal API/deviation and hostile corpus freeze, representative controlled-host timing and broader no-transcoding release evidence remain open. |
| P10-P12 native 16-bit/UTF-16 and cross-width | Production compiler/program/matcher/result types remain byte-specific. Native16 references and shared scalar-coordinate fixtures exist; no managed native16 backend or managed cross-width execution suite. | Open and **mandatory before stable PCRE release**. No whole-subject transcode adapter or cloned parser/HIR counts as completion. |
| P13 partial/iteration/substitution | Current internal match result/API handles complete single matches. | Open: soft/hard partial, native partial offsets, next-match empty retry rules, replacement language/output limits and width-native sinks. |
| P14-P17 security, analyses, performance, fuzz closure | Deterministic work/cancellation, bounded frames/compilation/proof scans, optimized/reference controls, generated Unicode gates, persistent differential cases, seven component workloads. | Partial 8-bit evidence. Full independently controlled resource model, dual-width proof/performance corpus, bounded fuzz/reducer closure and consumer benchmarks remain open. |
| P18 public API/package/release | Existing extraction package sample/API checks; experimental PCRE stays internal. | Open. Both native PCRE widths, cross-width/package-only consumers, API/version/deviation freeze and release audit are required. User owns tags/publication. |

The implementation has deliberately progressed into experimental Stage D while
E03/E04 and Stage C remain open. Preserve this sequencing exception in the
ledger; do not retroactively declare normalization complete. The frozen
POSIX/GNU baseline proves preservation, not the strict POSIX or full Gnulib
contracts requested by Revision 4.

Execution order after this review:

1. #279 correctness and #280/#281 dual-width reference seams are merged.
   Qualify the bounded Perl-overlap/reducer increment #282 on frozen fixtures.
2. Plan deliberate P09 closure and the remaining P00/P01 contracts before
   P10 lowering. Preserve one shared PCRE semantic grammar; specialize
   representations below it.
3. Keep callouts, partial/iteration/substitution, full security/fuzz/performance
   and P18 release evidence as explicit deliverables. Repeated optimizer
   increments alone cannot close the roadmap.
4. Keep actual consumer/bridge and Stage C normalization work separately
   reviewable. No Grep switch, CommandFramework removal, public PCRE contract
   or stable publication follows from this internal increment.

These are repository-evidenced dispositions, not assertions that uninspected
downstream repositories have changed. Current passing build counts are
qualification evidence for the declared subset, not overall completion.


## PR279: finite ASCII union star anchoring

Case-sensitive positive ordinary UTF classes such as `[\D0-9]`, finite
negative classes plus containing positive classes, and disjoint negative
classes preserve native ALLANY star entry. Exact typed finite ASCII predicates,
low 0..127 literal ranges and a negative finite member establish byte coverage
plus high-character inclusion. Caseless, unsupported/Unicode/binary members,
negative Ascii, wider ranges and exact same-kind pairs retain earlier owners
or conservative fallback. No general semantic-universality inference is made.

An eighth independent compile-local 4,096 combined predicate/range counter
is shared across leading paths. Every old proof runs first; old successes
consume no new budget. Precheck entire lists, scan completely with cancellation
and per-item charges, and use existing static bitmaps plus constant-time
low-range ulong masks. No allocation per scan, sorting, parser/matcher/VM,
runtime work, API, dependency, workflow or public quota change. Prior proof
bodies and PR278 iterator-fragment recovery remain byte-for-byte unchanged.

426 groups / 22,214 native protocols, 15 diagnostics and 56 supplementary cases
add 497 tests (103,848 total). Tests-only RED exactly matches 149 matching plus
16 supplementary identities: 165 failures / 103,683 passes / zero skips, native
assertion frames or coded warnings/errors. All 103,351 prior tests and 332 new
controls pass; freeze all five new C# blobs before production. Preserve all
362 prior test files / 357 C# sources and A05 source 35/corpus identities.

See [design](PCRE-UTF-FINITE-ASCII-STAR-ANCHOR-DESIGN.md),
[plan](PCRE-UTF-FINITE-ASCII-STAR-ANCHOR-PLAN.md) and
[PR279](https://github.com/uniblab/Icod.Expressions/pull/279). Whole-branch
author review and all seven inventories precede final qualification. Build
net8/net9/net10; execute full net10 and frozen 214 cases / 120 methods on six
native hosts with all configured support/package/script gates. Final identities,
results and plan completion remain external on the PR, with no source/docs/test
edits after final GREEN. Keep #7/#102 open; Grep last; user owns merge, tags and
publication. Issue #7 footer unchanged; broader optimizer/release/consumer gates
remain open.


## PR278: UCP xdigit iterator fragment recovery

A bounded direct-root proof retains native start-study recovery for unbounded
UCP xdigit complement iterators before selected ASCII literal continuations.
NoDotStarAnchor does not disable this iterator behavior; NoStartOptimize does.
The owned program bool and one integer cutoff reuse the existing fragment
loop after whole-subject validation, preserving its requested zero-width
candidate. Matching does not consume malformed bytes or infer anchoring.

Require exact complementary UnicodeHexDigit predicates, low ASCII ranges,
combined 4,096 list visits and a bounded 4,096 retained-AST walk over the
proved head plus literal/capture/mark/repeat tails. Preserve erased-node
inspection and cancellation. Greedy marked/no-tail/optional, caseless K/S,
non-ASCII continuation, Turkish/surrogate and wider group/call/cut/assertion
shapes remain outside the proof. Initial raw native consumption research
remains on immutable tests-only source; aligned controls retain their spans.

428 groups / 22,936 native protocols, 15 diagnostics and 42 supplementary cases
add 485 tests (103,351 total). Refined RED is exactly 132 managed failures /
103,219 passes; all 116 matching and 16 supplementary identities match. All
102,866 prior tests and 353 new controls pass; native/compiler warning/error
counts are zero. Freeze five new C# blobs before production; preserve all
357 prior test files / 352 C# sources and A05 source 35. All established ALLANY
bodies/budgets, VM instruction handling and Step placement remain unchanged.

See [design](PCRE-UTF-XDIGIT-FRAGMENT-RECOVERY-DESIGN.md), [plan](PCRE-UTF-XDIGIT-FRAGMENT-RECOVERY-PLAN.md) and
[PR278](https://github.com/uniblab/Icod.Expressions/pull/278). Build
net8/net9/net10; execute net10 only on six native hosts with frozen 214 cases /
120 methods and all configured support/package/script gates. All seven
inventories precede final qualification; results, exact identities and final
plan completion stay external on the PR without source/docs/test edits after
GREEN. Keep #7/#102 open; Grep last; user owns merge/tags/publication. Issue
#7 footer unchanged. Broader optimizer/release/consumer readiness remains open.


## PR277: built-in class complement star anchoring

Positive ordinary UTF classes with exact complementary finite ASCII POSIX
classes, horizontal/vertical whitespace lists or UCP POSIX blank preserve
native ALLANY star entry. Member order, duplicates, additional union members,
casing and ASCII restrictions retain typed source provenance. General Unicode
property pairs and UCP xdigit raw entry remain outside this proof; their valid
aligned controls and immutable initial research are documented.

A seventh independent compile-local 4,096-predicate counter is shared across
leading paths. The new overload calls every established proof first and returns
old successes without new charges. Full-list precheck, exact metadata, explicit
positive/negative mask bits, complete scans after early pairs and cancellation
bound the new work. Literal ranges consume no new budget. Exhaustion declines
inference without changing admission or public limits. Existing production
bodies, runtime state and work charging, parser, VM, API and workflows stay fixed.

402 groups / 21,952 native protocols, 15 diagnostics and 58 supplementary cases
add 475 tests (102,866 total). Refined tests-only RED is exactly 142 managed
failures / 102,724 passes: 123 matching and 19 supplementary identities. All
102,391 old tests and 333 new controls pass; native assertions and compiler
warnings/errors are zero. Freeze all five new C# blobs before production;
preserve 352 prior test files / 347 C# sources and A05 source 35.

See [design](PCRE-UTF-CLASS-COMPLEMENT-STAR-ANCHOR-DESIGN.md), [plan](PCRE-UTF-CLASS-COMPLEMENT-STAR-ANCHOR-PLAN.md) and
[PR277](https://github.com/uniblab/Icod.Expressions/pull/277). Build
net8/net9/net10, execute net10 only on six native hosts with frozen 214 cases /
120 methods and all configured support gates. All seven inventories precede
final qualification; exact identities/results and final plan completion stay
external on the PR without source/docs/test edits after GREEN. #7/#102 stay
open; Grep last; user owns merge/tags/publication. Issue #7 footer unchanged.


## PR276: mixed binary/POSIX ASCII star classes

Positive ordinary UTF classes containing positive binary ASCII plus negative
POSIX ASCII retain native ALLANY star entry. Reordered/aliased/duplicate/cased
members and extra union predicates/literals preserve byte spans at malformed
fragments and split offsets. Opposite-polarity and binary-only complements
retain XCLASS behavior; membership equality alone does not prove lowering.

A sixth independent compile-local4096 predicate counter is shared across
leading paths. The new overload calls the unchanged five-/four-counter proofs
first, preserving every established result and charge. Only old failures
attempt a complete, prechecked, cancellation-aware scan for the exact mixed
pair. Scan past early markers; literal ranges are not charged by the new
proof. Already proven classes consume no new work; exhaustion remains fallback.
Runtime state/work, instructions, admission and public limits are unchanged.

324 matching groups/21,456 native protocols,15 diagnostics and93 supplementary
cases add432 tests (102,391 total). Tests-only RED exactly109 failures/102,282
passes; all101,959 prior tests and323 new controls pass. Five C# blobs are
frozen before production;347 prior test files/342 C# sources/frozen35 remain
unchanged. See [design](PCRE-UTF-MIXED-ASCII-STAR-ANCHOR-DESIGN.md),
[plan](PCRE-UTF-MIXED-ASCII-STAR-ANCHOR-PLAN.md) and
[PR276](https://github.com/uniblab/Icod.Expressions/pull/276).
Build net8/net9/net10; execute net10 only on six native hosts. All seven
inventories precede qualification; final evidence/identities remain external
on the PR without source/docs/test edits after GREEN. #7/#102 remain open;
Grep last; user owns merge/tags/package publication. Issue7 footer unchanged.


## PR275: POSIX ASCII unions in UTF star classes

Positive ordinary UTF classes preserve native ALLANY star entry for POSIX
ASCII complement pairs, positive ASCII plus128..0x10FFFF literal coverage,
and negated ASCII plus0..127 coverage. Exact POSIX Ascii nodes remain distinct
from binary ASCII properties, whose semantically equivalent unions can retain
XCLASS provenance. Mixed-source and case-closed one-sided cohorts remain
separate valid-aligned controls without raw split/malformed parity claims.

A fifth independent compile-local4096 counter bounds complete predicate and
range passes across leading paths. An overload invokes the unchanged four-
counter proof first and returns its success, preserving every existing path
and charge without consuming new work. Only old failures attempt the positive
Source:Class POSIX proof. Whole-list prechecks, per-visit cancellation, complete
scans past early pairs/coverage and no-progress/exhaustion fallback remain.
One-sided source-domain guards preserve older literal-union budget ownership.
Instructions, runtime state/work, admission and public limits are unchanged.

506 matching groups/30,390 native protocols,45 diagnostics and174 supplementary
cases add725 tests (101,959 total). Tests-only RED exactly173 failures/101,786
passes, with all101,234 old tests and552 new controls passing. Seven new C#
blobs are frozen before production;340 prior test files/335 C# sources and
frozen35 are unchanged. Native-first evidence, source guards and review gates
are in the [design](PCRE-UTF-POSIX-ASCII-STAR-ANCHOR-DESIGN.md),
[plan](PCRE-UTF-POSIX-ASCII-STAR-ANCHOR-PLAN.md) and
[PR275](https://github.com/uniblab/Icod.Expressions/pull/275).
Build net8/net9/net10, execute net10 only on six native hosts. All seven
inventories precede final qualification; final identities/evidence stay on the
PR without source/docs edits after GREEN. #7/#102 remain open; Grep last;
user owns merge/tags/package publication. Issue7 footer unchanged.


## PR274: ASCII plus high ranges in UTF star classes

Positive ordinary UTF classes with an exact positive ASCII predicate and
retained ranges covering128..0x10FFFF now preserve native ALLANY star entry.
Reordered/split/overlapping ranges and property aliases retain native byte
spans at tolerant split offsets. Negative outer/negative-only ASCII, gaps,
case-closed source cohorts and ranges below128 stay outside the new proof.

A fourth independent compile-local4096 budget counts every predicate and
complete range pass across leading paths. Precheck the complete first combined
list, then every whole range pass; scan past early ASCII/coverage, check
cancellation and stop on no progress or exhaustion. Lower ranges cannot
rescue an exhausted literal-union proof. Preserve every old constant/member/
union/predicate result and charge, including successful old Any scans.
Runtime state/work, instructions, admission and public limits are unchanged.

186 matching groups/11,090 native protocols,15 diagnostics and72 supplementary
cases add273 tests (101,234 total). Tests-only RED is exactly68 failures/
101,166 passes; all100,961 old tests and205 new controls pass. Three new C#
blobs are frozen before production;337 prior test files/332 C# sources and
frozen35 remain unchanged. See [design](PCRE-UTF-ASCII-RANGE-STAR-ANCHOR-DESIGN.md),
[plan](PCRE-UTF-ASCII-RANGE-STAR-ANCHOR-PLAN.md) and
[PR274](https://github.com/uniblab/Icod.Expressions/pull/274).
Build net8/net9/net10, execute net10 only on six native hosts. All seven
inventories precede final qualification; final evidence/identities remain on
the PR without source/docs edits after GREEN. #7/#102 remain open; Grep last;
user owns merge/tags/package publication. Issue7 footer unchanged. Deferred
[OffsetLimit research](PCRE-OFFSET-LIMIT-RESEARCH.md) adds no public API.


## PR273: complete range unions in UTF star classes

Ordinary positive UTF classes whose retained ranges jointly cover the complete
0..0x10FFFF interval qualify for native ALLANY star anchoring. Reordered,
overlapping and literal-bridged unions use bounded complete-list frontier
passes, without sorting or allocation. Holes, including a surrogate-only gap,
negative outer classes and general algebra remain outside this proof.

A new compile-local 4096-visit coverage budget is shared across leading paths
and passes, independently of the existing predicate/member counters. Precheck
each whole list, check cancellation and charge every visit, scan past early
coverage, and stop on no progress or insufficient budget. Explicit full-range
members remain owned by the old member proof. Retain the existing predicate
precheck and complete scan even when coverage independently succeeds, so no
successful old Any proof loses its original charges. Declined coverage retains
the old result; instructions, runtime work and public limits are unchanged.

530 matching groups/29,225 native protocols,15 diagnostics and76 supplementary
cases add621 tests (100,961 total). Preserve331 prior test files/326 C# sources
and frozen35. Native-first RED/frozen-source/author-review evidence is in the
[design](PCRE-UTF-RANGE-UNION-STAR-ANCHOR-DESIGN.md),
[plan](PCRE-UTF-RANGE-UNION-STAR-ANCHOR-PLAN.md) and
[PR273](https://github.com/uniblab/Icod.Expressions/pull/273).
Build net8/net9/net10; execute net10 only on six native hosts. All seven
inventories precede final qualification; final evidence and identities remain
on the PR without source/docs edits after GREEN. #7/#102 stay open; Grep last;
user owns merge/tags/package publication. Issue7's footer remains untouched.


## PR272: explicit full-range members in UTF star classes

An ordinary positive UTF class with multiple retained ranges now preserves
native ALLANY star anchoring when an admitted member is exactly 0..0x10FFFF.
For example, `[\x00-\x{10FFFF}x]*?B` preserves the native miss on tolerant
UTF `éB` from byte offset1. The uncharged single-range path remains first.
The new complete-list scan checks cancellation and charges every admitted
member against an independent 4096-range budget shared across leading paths.
Oversized lists are prechecked without partial charge; declined range proofs
fall through to the existing Any/predicate rules and their original charges.
Negative outer classes, near-full ranges and general coverage/algebra retain
their existing rules. Instructions, runtime work, admission and public limits
are unchanged.

Native 455 matching groups/55,384 protocols, 15 diagnostics and 55 supplementary
cases add 525 tests (100,340 total). Tests-only RED is exactly 152 failures/
100,188 passes, with all 99,815 prior tests and 373 new controls passing;
all four new C# blobs are frozen before production. Preserve 327 prior test
files/322 C# sources/frozen35. Build net8/net9/net10; execute net10 only on six
native hosts. Full qualification and source identities remain external on
[PR272](https://github.com/uniblab/Icod.Expressions/pull/272).
[Design](PCRE-UTF-FULLRANGE-MEMBER-STAR-ANCHOR-DESIGN.md) /
[plan](PCRE-UTF-FULLRANGE-MEMBER-STAR-ANCHOR-PLAN.md).
#7/#102 remain open; Grep last; user owns merge/tag/publication. Issue7's
footer remains untouched under its existing publication block.


Reviewed after merged PR275, with PR276 in progress. Issue #7 remains the delivery roadmap; the
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
   PR248 adds caller first-line candidate windows through a bounded newline frontier.
   PR249 adds caller empty-match rejection through existing global acceptance policy.
   PR250 adds caller NotBol/NotEol through typed caret/dollar provenance.
   PR251 adds caller variable-lookbehind ceilings through existing width analysis.
   PR252 adds caller pattern byte-length admission before ownership copying/parsing.
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

PR251 merged at6da239a485720ccd7299a67c4c7e7e3c8901056d;
its89,568 net10 tests remain the baseline. Main43637643801/scripts43637643663 are green.
PR252 adds caller pattern byte-length limits as described below; final qualification/
review/delivery identities are recorded on PR252/issue7.

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

## PR248: caller-selected first-line compilation

Default-false caller `FirstLine` implements PCRE2_FIRSTLINE0x100. Unanchored candidate starts are limited to positions before or at the first configured newline following the requested offset. Matches can consume across that newline, and reported backslash-K starts may lie beyond it. A later requested offset selects a new window. Caller Anchored ignores the policy; EndAnchored independently constrains completion. Literal/casing/UTF/source/word/line policies compose normally.

The compiler snapshots FirstLine alongside the existing anchoring flags after ordinary compilation. A Step-accounted incremental newline frontier from startOffset gates all later candidate entry before COMMIT first-byte checks, including CRLF/inferred-line/fragment/empty-EOF retries. Explicit forward SKIP is honored before anchored termination; proven unfiltered SKIP entry can bypass FirstLine as in native JIT, while filtered entry keeps the window. A bounded root proof uses existing source shape, four group levels and1,000 visits; broader optimizer shapes remain issue102. Existing newline/UTF readers preserve complete scalar/pair recognition and malformed raw offsets. Unobserved byte/suffix shortcuts are disabled under FirstLine so work accounting cannot cross the window. The first requested candidate spends no frontier work; input consumption and lookbehind remain unrestricted by this start policy. Parser/root0/callee IR/opcodes/scopes/journal/shared ceilings remain unchanged.

Merged247 `bb6056dfb4aa1fd9e3e1ed149eaaa9004cc35fb0`/tree `8a1e5efa9cfdc4c2b359ea6c0ed5c57db472950f` has86,250 tests/framework and264 earlier files. Main43587678445/scripts43587678506 are green. PR248 adds605 tests for86,855/framework. All prior hashes remain fixed; all seven inventories precede source qualification, with final evidence external on PR248/issue7.

Expanded tests-only [RED751](https://github.com/uniblab/Icod.Expressions/actions/runs/43590798617), source `77ed49eca15e2f9c2a77ca9feda7917f485b21fa`, Linux ARM132434872721: net8/net9/net10 each built with zero compiler warnings/errors and ran **368 expected failures /86,487 passes /86,855 total /zero skips**. Failures were341 missing-window groups,24 anchored-SKIP groups,2 bounded-work cases and1 ownership case/framework, all after successful native assertions. All86,250 earlier tests and237 new controls passed. Freeze `68d9c25772810758954234e4df4b45c76677e3dd` before production. Initial RED750 exposed four anchored-SKIP groups beyond the original FirstLine predictions; the fixture was expanded from413 to605 before production, preserving every original expectation and all prior test files.

See [design](PCRE-FIRST-LINE-COMPILE-DESIGN.md) and [plan](PCRE-FIRST-LINE-COMPILE-PLAN.md). All264 earlier test files and frozen35 production blobs remain hash-fixed; GNU/BRE/ERE and214 frozen outcomes/120 methods are preserved. Final all18 results/supporting gates, whole-branch author review and exact source/base/tree/proposed-merge identities belong on PR248 and only issue7 footer, without a documentation-only CI push. UTF-C, callouts, remaining syntax/modes/directives, tolerant-surrogate exclusions, failed/partial labels, broader102, NO_UTF_CHECK/UTF16/public API and realistic consumer/release gates remain open; Grep last.

## UTF prefix qualification correction

Initial source4e24403e0e4c8f552dbaaf1e09b39ba672231b65/CI75243591726169 built cleanly; Linux ARM132437783721 ran4 failures/86,851 passes/86,855 total/zero skips on all three frameworks. Only plain/named SKIP into dot-plus-B in tolerant UTF failed, with both EndAnchored policies. Native10.48 scan_prefix stops at UTF OP_ANY/OP_ALLANY before considering the suffix; the bounded proof now treats UTF dot as opaque regardless of suffix, retaining fixed-width byte suffix filtering. The four frozen regression groups provide RED; all264 earlier test files and fixture68d9c25772810758954234e4df4b45c76677e3dd remain unchanged. Broader prefix proofs stay in issue102. Final all18 qualification/review/identities remain external on PR248/issue7.

## Requested temporary .NET 10 test policy

The user changed tests/Icod.Expressions.Tests.csproj in commit0ab40ea6048c43d2c908d8eef1402a5acfde2bcc to target net10.0 only, then explicitly requested builds for net8.0/net9.0/net10.0 with test execution only on10. PR/main/tag workflows now retain all three library builds on all six native OS/architecture hosts, select net10.0 for full/frozen tests and parity/package smoke execution, and remove the Intel8/9 test jobs. Intel restores/builds the complete solution without TargetFrameworks overrides. The result contract expects net10.0; release prerequisites require all seven current main jobs, including Intel test and aggregation. Existing main-job acceptance/rejection checks were updated (RED against the old guard, then all nine checks GREEN); generic result-validation capabilities remain unchanged.

Pre-policy engine source1ed02620bd83355a351570fe8af655c848309b84/CI75343592621873 passed all18 results at86,855 each. Final qualification after this user-directed configuration change requires six net10 results,214 frozen outcomes/120 methods each, zero failures/skips/compiler warnings/errors, all three library outputs per native host, supporting gates and all configured source-pinned compatibility jobs. The frozen605-case fixture68d9c25772810758954234e4df4b45c76677e3dd, all prior C# test sources and engine sources from1ed0262 remain unchanged. The user project edit and existing main-job gate fixture are intentional configuration changes. Final current-head evidence/review/identities remain external on PR248 and only issue7 footer; this policy supersedes the previous eighteen-current-result requirement. No merge/tag/publish action is authorized here.

## PR249: caller-selected empty-match rejection

Default-false internal caller NotEmpty and NotEmptyAtStart select native match policies 4/8 once for an owned compiled pattern. NotEmpty rejects equal reported group0 endpoints; NotEmptyAtStart rejects them only at the original requested byte offset, permitting later empty results. K resets, including supported assertion resets and reversed spans, retain reported-endpoint semantics. Source directives OR into caller policy; local resets cannot unset it. Native reference options are passed to Match, never Compile.

Four scoped production files initialize existing global settings and preserve native terminal-query parity. For one root branch without subroutine targets, effective NOTEMPTY/ATSTART and a direct trailing greedy 0..1 literal repeat, the compiler marks that literal instruction. Successful consumption discards only its immediately preceding optional Split fallback; a failed literal still takes the zero branch. Both shorthand and counted queries share the proof. Native auto_possessify commits greedy end-of-pattern iterators; NO_AUTO_POSSESS independently toggles the original mismatch. Lazy queries, explicit alternatives and explicit possessive forms retain their own behavior. No extra instructions, opcodes, frames, journal fields, quotas or successful-work charges. Wrapped/class/call/multiple-branch/wider proofs remain #102. Existing scoped acceptance, captures/cuts/labels, source ownership, validation and Literal/anchoring/UTF/casing/word/line/newline policies remain shared.

Merged248 `74af49bb62fcad01f79fdaff2fb26b29abf6611d`/tree `7e48200e810e252d9d45f035e7740ac73c36b85f` has 86,855 tests and 265 prior files. Main43605349078/scripts43605349020 are green. PR249 adds 889 tests for 87,744; builds8/9/10 and net10-only tests remain the policy.

The 889 cases comprise 794 groups/27,652 native10.48/JIT0 matching-offset protocols, 60 preserved compile diagnostics and 35 work/ownership/validation/capacity cases. Native batches precede managed comparisons; negative matching requires -1. All 265 prior test files/260 C# sources and frozen35 source blobs remain fixed. The 502 strict split-offset(-36) research rows stay outside matching, with separate validation tests. Full 256 capture/depth limits, 4095 versus 4096 literal instruction limits and terminal-query exact 4096-instruction capacity are pinned.

Initial production sourcefaf1a8c/CI758 built cleanly and ran 10 failures/87,686 passes/87,696 total/zero skips on Linux ARM132485083628: four lookahead-K/greedy-query native mismatches and six erroneous budget measurements. Snapshot the first observer Count before exact/exhausted runs; the exact run must not inflate the later budget. All assertions/native expectations and 841 prior case identities were retained. Restart tests-only qualification and refreeze after adding 40 native protocol groups/eight budget/capacity controls. This transparently supersedes the initial e99 fixture freeze; no production behavior was adapted to the harness defect.

Corrected tests-only [RED760](https://github.com/uniblab/Icod.Expressions/actions/runs/43609311096), source `a89646740baac3ca1a03c2a26608e1b813f54eb4`, Linux x64 job132490648227: net10 built with zero compiler warnings/errors and ran **569 expected failures / 87,175 passes / 87,744 total / zero skips**. Exactly 558 missing caller-policy groups, two existing source-NOTEMPTY terminal-query gaps, six retry-work and three ownership cases failed after successful native batches; all 86,855 earlier tests and 320 new controls passed. Source-budget controls reached the corrected exact/exhausted checks before caller integration. Freeze `893128484f0a01f399c309b47019c3f8e4e14d8f` before production. The prior work-counter measurement was corrected without changing assertions or native expectations; all 841 earlier case identities remain, with 40 protocol groups/eight controls added before this corrected freeze.

See [design](PCRE-CALLER-EMPTY-MATCH-DESIGN.md) and [plan](PCRE-CALLER-EMPTY-MATCH-PLAN.md). Final qualification requires six fresh net10 results at 87,744 tests each, zero failures/skips/compiler warnings/errors, 214 frozen outcomes/120 methods per result, all three library outputs per native host and every configured supporting gate. Builds remain net8/net9/net10; test execution only net10. All seven inventories precede final source qualification; final GREEN/review/exact source/base/tree/proposed-merge identities belong on PR249 and only issue7 footer, without a documentation-only CI repeat. UTF-C execution, callouts, remaining syntax/directives/match/API choices, tolerant-surrogate exclusions, failed/partial labels, broader #102, UTF16/exported API and realistic consumer/release gates remain open; Grep last. User owns merge/tag/publication.

## PR250: caller line-boundary policies

PR249 merged as eb9751af94f3824113b7715af7cc008d21465841 with the reviewed tree6ecf612749f4a23d04da49d01145c31d2542341b; main43616040552 and scripts43616040546 completed green. Baseline87,744 tests. PR250 adds caller NotBol/NotEol through six scoped production files and typed caret/dollar provenance, preserving explicit absolute anchors and resource limits. This advances internal match-policy coverage; remaining engine/API/consumer gates are unchanged.

The 1,128 new cases pin 1,023 groups/45,411 native PCRE2 10.48/JIT0 match-offset protocols, 69 compile diagnostics and 36 ownership/work/validation/capacity controls. Native batches precede managed comparisons; no-match requires -1. Separate 864 strict split-offset (-36) research rows from matching. Tests-only [RED](https://github.com/uniblab/Icod.Expressions/actions/runs/43619048623), source329e1756bcb62f8eebe53b75a8962c647923ba74/treef759260b064b84559ae4c724c73f7930f34055bd, completed Linux x64 job132520352642: **682 expected failures / 88,190 passes / 88,872 total / zero skips/compiler warnings/errors**. The failure-key multiset is exactly679 missing-policy groups plus3 ownership cases; all87,744 prior tests and446 new controls pass. Freeze before production: common7f3396440e33d1874da11a41ad28e6144a903036, part1=890cd328cca3dac7c01c8d3e044ec18d50d9ef3c, part2=ea04095d4436791070062f8cf7fcda975234502c. All266 prior test files/261 C# sources and frozen35 blobs stay fixed.

See [design](PCRE-CALLER-LINE-BOUNDARIES-DESIGN.md) and [plan](PCRE-CALLER-LINE-BOUNDARIES-PLAN.md). Final qualification requires six fresh net10 results at88,872 tests each, zero failures/skips/compiler warnings/errors,214 frozen outcomes/120 methods per result, all net8/net9/net10 library outputs on each native host and every configured supporting gate. The .NET10-only execution policy remains. All seven inventories precede final source qualification; final GREEN/whole-branch author review/exact source/base/tree/proposed-merge identities belong on PR250 and only issue7 footer, preserving its original2600-character prefix. No documentation-only CI repeat. Wider optimizer #102, remaining syntax/directives/UTF/callouts/API, tolerant-surrogate exclusions, failed/partial labels, realistic consumer/packaging/release gates remain open. Grep last; user owns merge/tag/publication.

## PR251: caller variable-lookbehind ceiling

PR250 merged as505c3acf2c729bb5be531cc4399bad2b7f52c44a/tree7488f706f55c0f3ae3c898068fd7819f2f0daee3, with ordered parents eb9751a then2aec54e. Main43632261155/scripts43632261161 are green;88,872 tests are the baseline. PR251 adds caller MaxVarLookbehind through the existing width analysis. Offset-limit research exposed native JIT optimizer-inferred anchoring/FIRSTLINE interactions; defer that separate integration to the wider optimizer work.

The696 cases comprise410 groups/15,174 native10.48/JIT0 match-offset protocols,267 compile diagnostics and19 work/ownership/validation/capacity controls;294 strict split-offset rows remain separate. Native batches precede managed comparison; signed captures and successful labels are pinned. See [design](PCRE-VARIABLE-LOOKBEHIND-LIMIT-DESIGN.md) and [plan](PCRE-VARIABLE-LOOKBEHIND-LIMIT-PLAN.md) for exact baseline evidence/frozen blobs. Require six fresh net10 results at89,568 passes each, zero failures/skips/compiler warnings/errors,214 frozen outcomes/120 methods per result, all3 library outputs and every configured supporting gate. Builds remain C#13/net8/net9/net10; tests only net10. All seven inventories precede final source qualification. Final GREEN/author review/source/base/tree/proposed-merge evidence belongs on PR251 and only issue7 footer, preserving its original2600-character prefix, with no docs-only CI repeat. Remaining syntax/UTF-code-unit/callout/directive/match/API/optimizer (#102)/consumer/release gates remain open; Grep last; user owns merge/tag/publication.

## PR252: caller pattern byte-length limit

PR251 merged as6da239a485720ccd7299a67c4c7e7e3c8901056d/tree1906a545b08ee5e05545c2a678006809354f4978, ordered parents505c3ac then6ceac5d. Main43637643801/scripts43637643663 are green;89,568 tests are the baseline. PR252 adds caller pattern byte-length admission at the existing compile boundary. Other compile-context/match/API/optimizer and consumer decisions remain separately scoped.

The1,135 cases comprise576 groups/19,240 native10.48/JIT0 match-offset protocols,535 compile diagnostics and24 memory-view/no-read/ownership/priority/work/capacity cases;392 strict split-offset rows remain separate. Native batches precede managed comparisons. See [design](PCRE-PATTERN-LENGTH-LIMIT-DESIGN.md) and [plan](PCRE-PATTERN-LENGTH-LIMIT-PLAN.md) for exact RED/frozen evidence. Require six fresh net10 results at90,703 passes each, zero failures/skips/compiler warnings/errors,214 frozen outcomes/120 methods each, all3 library outputs and every configured supporting gate. Builds remain C#13/net8/net9/net10; tests only10. All seven inventories precede final source qualification; final GREEN/whole-branch author review/source/base/tree/proposed-merge identities belong on PR252 and only issue7 footer, preserving its original2600-character prefix, with no docs-only CI repeat. Remaining UTF-code-unit/callout/syntax/directive/match/API/optimizer (#102)/consumer/release gates stay open; Grep last; user owns merge/tag/publication.

### PR252 corrected oracle qualification

PR252 oracle qualification was restarted because PCRE.NET drops zero-valued native pattern-length settings. Direct PCRE2 zero expectations are unchanged; the test-only positive-limit bridge is proven equivalent across193 profiles/6,562 direct-native rows. Corrected RED43640343531 verifies336 managed failures/90,367 passes/90,703 total and every native preflight before refreezing commonaea2a3360f7761ccdb86329eebe267133bf51ff0. All prior/new assertions and data remain; see the design for the superseded initial proof and exact corrected source/tree/job evidence.

## PR253: caller control of automatic possessification

PR252 merged as `7e55bbf2044b788bb5be6ec27f5d4a75fcfd317c`, tree `fb7b7dacee2c9c8e49992d1770beb85fcd5a70fa`, ordered parents base251 `6da239a485720ccd7299a67c4c7e7e3c8901056d` then reviewed252 `6515433c9a9d930f9848f96f3b55ef5b8eb1fe47`. Main43695122735/scripts43695122742 are green; 90,703 tests are the baseline. PR253 adds caller NoAutoPossess through the existing compile proof. Broader automatic possessification, optimization controls/source directives and other syntax/match/API/consumer/release gates remain separately scoped.

The 876 new cases pin 796 matching groups / 28,380 native PCRE2 10.48/JIT0 match-offset protocols, 57 compile diagnostics and 23 priority/work/ownership/validation/capacity cases. Separate 1,064 strict split-offset (-36) research rows from matching. Native batches precede managed comparisons. See [design](PCRE-NO-AUTO-POSSESS-DESIGN.md) and [plan](PCRE-NO-AUTO-POSSESS-PLAN.md) for RED/frozen evidence. Require six fresh net10 results with 91,579 passes each, zero failures/skips/compiler warnings/errors, 214 frozen outcomes / 120 methods each, all net8/net9/net10 library outputs and every configured supporting gate. All seven inventories precede final source qualification. Final GREEN/whole-branch author review/source/base/tree/proposed-merge identities belong on PR253 and issue7 footer only, preserving its original2600-character prefix; no docs-only CI repeat. Remaining UTF-code-unit/callout/syntax/directive/match/API/optimizer (#102)/consumer/release gates stay open; Grep last; user owns merge/tag/publication.

## PR254: source prefix for automatic possessification control

PR253 merged as `243f83edf3026961ec7d2353ee2527dcdc5a89e9`, tree `7f34417770270df97b167e846c315682dbf4d306`, ordered parents base252 `7e55bbf2044b788bb5be6ec27f5d4a75fcfd317c` then reviewed253 `575ecdc59ff569ccf789edd49854dc5bf803bd61`. Main43700015901/scripts43700015932 are green;91,579 tests are the baseline. PR254 adds source-prefix control through resolved start settings and the existing program gate. Broader optimizer availability, other source directives and remaining syntax/match/API/consumer/release decisions stay separately scoped.

The 776 new cases pin 576 groups / 34,560 native PCRE2 10.48/JIT0 match-offset protocols,164 diagnostics and36 work/ownership/view/no-read/priority/capacity cases. Keep1,404 strict split-offset(-36) research rows separate. See [design](PCRE-NO-AUTO-POSSESS-PREFIX-DESIGN.md) and [plan](PCRE-NO-AUTO-POSSESS-PREFIX-PLAN.md) for native-first RED/frozen evidence. Require six fresh net10 results at92,355 passes each, zero failures/skips/compiler warnings/errors,214 frozen outcomes/120 methods each, all net8/net9/net10 library outputs and every configured supporting gate. All seven inventories precede final source qualification. Final GREEN/whole-branch author review/exact source/base/tree/proposed-merge identities belong on PR254 and issue7 footer only, preserving its original2600-character prefix; no docs-only CI repeat. Remaining syntax/UTF-code-unit/callout/directive/match/API/optimizer (#102)/consumer/release gates stay open; Grep last; user owns merge/tag/publication.

## PR255: caller control of start optimizations

PR254 merged as `463ae290d4281445bedb0cfcc724dd4942004047`, tree `6e26348ba1af4c35ec7b547a1dba9d1d7daf224f`; 92,355 tests form the baseline. PR255 adds default-false caller NoStartOptimize through existing proof gates and private setup accelerators. The refined scope retains existing default proof availability and records fifteen deferred research profiles in the original tests-only source/run. Source (*NO_START_OPT), independent dot-star anchoring and wider optimizer (#102)/syntax/UTF-code-unit/callout/match/API/consumer/release gates remain open.

The 643 new cases pin 547 accepted groups / 42,792 native PCRE2 10.48/JIT0 match-offset protocols, 68 compile diagnostics and 28 work/ownership/unobserved-budget/validation/no-read/capacity cases. Keep 882 strict split-offset (-36) research rows separate. The refined tests-only RED has 139 expected managed failures / 92,859 passes / 92,998 total; all prior tests and native preflights pass. Frozen fixtures and deferred research profiles are recorded in [design](PCRE-NO-START-OPTIMIZE-DESIGN.md); execution follows the [plan](PCRE-NO-START-OPTIMIZE-PLAN.md). Require six fresh native net10 results with 92,998 passes / zero failures/skips/compiler warnings/errors, 214 frozen outcomes / 120 methods each, all net8/net9/net10 library outputs and all configured supporting gates. Final GREEN/author-review/source/base/tree/proposed-merge evidence belongs on PR255 and issue7 footer, preserving the original2600-character prefix; no docs-only CI repeat. Keep #7/#102 open; Grep last; user owns merge/tag/publication.

Qualification correction: byte ParseBracket had combined a legacy positive interior-range start-study workaround with intrinsic newline metadata. Suppress only that promotion when NoStartOptimize is true, preserving actual literal/range-endpoint provenance and default behavior. This extends the production scope to five files without changing parser grammar or AST shape. All12 failing native byte-range profiles and all other frozen fixtures remain unchanged; see the design for the source-pinned RED12 evidence and rationale. All seven inventories include this correction before final source requalification.

## PR256: source prefix for start-optimization control

PR255 merged as `50b53f0f8896235004d879546e660e9640830867`, tree `a40723746f8f42d24d3cca0029ac0db40f7203ce`, ordered parents base254 `463ae290d4281445bedb0cfcc724dd4942004047` then reviewed255 `f4beb3c2bef993c806c0781e1b66f93bd88c8f6a`. Main43723280843/scripts43723280936 completed green; all six fresh native net10 logs verify92,998 passes/214 frozen outcomes/120 methods and all3 library outputs, with Intel aggregate132842646172 verifying the same single Intel result. PR256 adds source-prefix control through resolved start settings; both program and parser metadata consume the setting. All broader engine/API/optimizer/consumer/release gates remain separately scoped.

The 768 new cases pin 622 groups / 46,726 native PCRE2 10.48/JIT complete return0 match-offset protocols, 110 diagnostics and 36 work/budget/cancellation/view/ownership/no-read/validation/capacity cases. Keep192 strict split-offset(-36) research rows separate. Corrected tests-only RED has636 expected managed failures /93,130 passes /93,766 total, with all native preflights and all92,998 prior tests passing; three new C# blobs are frozen. See [design](PCRE-NO-START-OPT-PREFIX-DESIGN.md) and [plan](PCRE-NO-START-OPT-PREFIX-PLAN.md) for source-pinned proof and the six-tuple managed diagnostic mapping correction before freeze. Require six fresh native net10 results with93,766 passes /zero failures/skips/compiler warnings/errors,214 frozen outcomes/120 methods each, all net8/net9/net10 library outputs and every configured supporting gate. All seven inventories precede final source qualification. Final GREEN/author-review/exact source/base/tree/proposed-merge evidence belongs on PR256 and issue7 footer, preserving its original2600-character prefix; no docs-only CI repeat. Broader default optimizer availability, independent dot-star anchoring, other directives/UTF-code-unit/callout/match/API/consumer/release gates remain open; #7/#102 stay open; Grep last; user owns merge/tag/publication.

Qualification inventory correction: the first implementation run passes all768 new cases but exposes3 old pending-syntax rows that still reject now-supported NoStartOpt. Replace only those InlineData directive tokens with still-pending `(*NO_DOTSTAR_ANCHOR)` in PcreGroupAliasReferenceTests, PcreFailureVerbReferenceTests and PcreSyntaxAcceptanceTests. Native preflights validate all3 replacements; assertion/helper bodies, case count and all3 frozen new fixtures remain fixed. All281 other prior files/276 C# sources and frozen35 production blobs remain unchanged. See the design for exact source/run evidence and the rejection-coverage rationale; all seven documents precede final requalification.


## PR257: bounded direct DotAll-star anchoring

Infer an owned anchor for one direct root-leading DotAll Any star (greedy/lazy/possessive), independently of NoStartOptimize and NoAutoPossess. Combine it with caller Anchored in the matcher so tolerant-UTF fragment/split-offset searches cannot retry beyond the requested candidate and missing tails consume one candidate's work. Exclude calls/targets and lexical PRUNE/SKIP, including erased/DEFINE bodies. Iterative AST proof has a4096 visit/pending bound and cancellation; exhaustion/unknown shapes silently fall back without a new capacity restriction. Wider grouped/alternative anchoring, ordinary-dot STARTLINE and caller/source NoDotstarAnchor remain pending; the three prior pending-syntax inventory rows stay unchanged. Two production files only; no parser/AST/opcode/frame/diagnostic/API/dependency/workflow changes.

Pin479 groups/42,953 native PCRE2 10.48/JIT0 protocols,34 diagnostics and44 resource/cancellation/ownership/view/native-independence/validation/no-read/capacity cases:557 new/94,323 total. Keep1,433 strict split offsets separate. All287 prior test files/282 C# sources and frozen35 production blobs remain fixed. See [design](PCRE-DOTSTAR-ANCHORING-DESIGN.md) and [plan](PCRE-DOTSTAR-ANCHORING-PLAN.md) for native-first RED/frozen evidence. Require six fresh net10 results at94,323 passes each,214 frozen outcomes/120 methods, all net8/net9/net10 library outputs, zero failures/skips/compiler warnings/errors and every configured supporting gate. All seven documents precede final source qualification; final GREEN/whole-branch author review/exact source/base/tree/proposed-merge evidence belongs on PR257 and issue7 footer only, preserving its original2600-character prefix; no docs-only CI repeat. Keep #7/#102 and wider optimizer/syntax/directive/UTF-code-unit/callout/match/API/consumer/release gates open. Grep last; user owns merge/tag/publication.


Implementation and whole-branch author review before qualification: only PcreByteProgram.cs/PcreByteMatcher.cs changed production (75 insertions/8 deletions). Program owns the bounded direct-star proof; matcher resolves caller-or-inferred anchor once through all eight existing gates. Explicit atomic groups retain a noncapturing child and cannot be mistaken for possessive atom wrappers. Reference identity, total visited+pending bound, retained erased/DEFINE traversal, unknown-node/call rejection and cancellation are verified. Native-independent policies, UTF/nullability/FIRSTLINE/COMMIT/signed captures/marks, exact observed/unobserved budgets and validation/no-read/owned-source/full capacities are covered by frozen fixtures. All287 prior files/282 C# sources and three frozen new fixtures remain identical; frozen35/corpus/A05/classifier/main-job shell gates pass; diff checks clean. No unresolved author-review findings; final source qualification evidence belongs externally on PR257/issue7.


## Qualification correction: byte ALLANY representation

First implementation source6f26bb2080b87e9e14ea3c9f8669ded6e2ed5d33/tree94480be306f502dd25e42d4576483772b85c4f66, [run43757782428](https://github.com/uniblab/Icod.Expressions/actions/runs/43757782428), Linux x64 job132945669561:6 failures/94,317 passes/94,323 total/zero skips/compiler warnings/errors. Every native protocol, prior test and UTF resource case passes. The six byte-mode inferred-versus-explicit work cases fail because CreateAnyCharacter represents byte DotAll under LF/CR/NUL/AnyCRLF as PcreByteClass(AllBytes, PrefixKind.Exact), while the first proof recognized only PcreByteAny.

Extend only the proof's atom predicate to the existing non-UTF Exact/AllBytes representation. Pinned native compiler/source probes confirm that full ordinary byte classes and non-UTF backslash-C normalize to OP_ALLANY and infer the same anchor; arbitrary bitmap/property provenance stays excluded. Preserve parser/AST/emitter/VM and every frozen new/prior fixture. Existing six frozen byte work cases are the failing regression proof; native match expectations remain unchanged. Review all five boundaries again and requalify the complete source. All seven documents precede final qualification; final GREEN evidence remains external.


## PR258: caller control of dot-star anchoring

Internal caller NoDotStarAnchor defaults false and suppresses inferred dot-star anchoring independently of NoStartOptimize/NoAutoPossess. Explicit caller anchoring remains active. Literal rejects this option at existing InvalidSyntax/offset0 priority before length/UTF/source access. Three production files add the option/admission rule, snapshot the bool and construct owned metadata; matcher/parser/AST/emitter/opcodes/frames/journals/API/dependencies/workflows stay fixed. When an emitted COMMIT exists, retain the existing bounded eligible-root proof solely for its synthetic zero-width requested-byte candidate while disabling the anchor; without COMMIT, the disabled policy skips proof discovery. Preserve all default behavior, other start proofs/private accelerators and unrelated roots. Source NoDotstarAnchor, wider grouped/alternative inference and ordinary-dot STARTLINE remain pending; three prior pending-syntax rows stay fixed.

Pin509 groups/45,361 native PCRE2 10.48/JIT0 protocols,132 diagnostics and69 resource/ownership/view/mark/default-property/validation/no-read/capacity/cut cases:710 new/95,033 total. Separate1,485 strict split offsets. Independently reproduce479 merged257 groups/42,953 protocols before new research. All290 prior files/285 C# sources and frozen35 stay fixed. See [design](PCRE-NO-DOTSTAR-ANCHOR-DESIGN.md) and [plan](PCRE-NO-DOTSTAR-ANCHOR-PLAN.md) for native-first RED/frozen evidence. Require six fresh net10 results at95,033 passes,214 frozen outcomes/120 methods, all net8/net9/net10 library outputs, zero failures/skips/compiler warnings/errors and all configured support gates. All seven documents precede final source qualification; final GREEN/whole-branch author review/exact source/base/tree/proposed-merge evidence belongs on PR258/issue7 footer only, preserving its original2600-character prefix; no docs-only CI repeat. Keep #7/#102 and wider optimizer/syntax/directive/UTF-code-unit/callout/match/API/consumer/release gates open. Grep last; user merges/tags/publishes.


## Observed corrected RED and fixture freeze

Corrected tests-only sourcee83dacdd682b52a268dab99772973ccf5a15cdc0/tree7949e1531cc02ef8e05a5566be5d9f4ff8219f5a, [run43849191960](https://github.com/uniblab/Icod.Expressions/actions/runs/43849191960), Linux x64 job133237322524:149 expected managed failures/94,884 passes/95,033 total/zero skips/compiler warnings/errors. The78 protocol keys exactly match native counter-oracle differences;48 diagnostic failures are enabled-flag Literal192/offset0;23 supplemental failures are18 work,2 ownership,1 default-property and2 no-read priority checks. Every native preflight and all94,323 prior cases/561 new controls pass; production is identical to merged257. Eight disabled Literal controls now use4141/limit1; all native diagnostic expectations, helper/assertion bodies and protocols are fixed.

Freeze before production: reference9012bd950fbda7e94203da58a3a93565849ac939; part1b449c3180b0d9e2b4fd92acfbfa5447a550de35a; part2 5c8269a9025611f8f70a3e5811706d8a4a84607e. Keep all290 prior files/285 C# sources and these three fixtures byte-for-byte fixed. Main43846045256/scripts43846045115 for actual merged257 are now fully green; six native logs confirm94,323 passes/frozen214 outcomes/120 methods/all three library targets. All seven documents precede final source qualification; final GREEN/source identities belong externally on PR258/issue7 footer.


Implementation and whole-branch author review before source qualification: three production files,13 insertions/5 deletions, add the init-only default-false bool/Literal conflict, snapshot the caller and own disabled-anchor plus bounded cut-entry metadata. Matcher/parser/AST/emitter/VM stay fixed. Default false reduces exactly to the prior anchor/candidate rules; explicit caller anchors and current explicit-caret LineStartSearch remain independent. Native is_startline shares the flag for ordinary-dot inference, but that wider proof is not implemented here, so do not suppress existing explicit-caret metadata. The requested split-byte COMMIT remains admitted through the existing synthetic-candidate path. Original-byte admission/no-read/cancellation/lockout order, settings/source ownership, signed captures/marks, exact work and full capacities are reviewed against frozen fixtures. No unresolved critical/important/minor findings. All290 prior files/285 C# sources and all three new frozen blobs remain identical; local frozen35/corpus/A05/classifier/main-job guards and diff checks pass. Final fresh native source qualification follows; final GREEN evidence remains external.


## PR259: source prefix for dot-star anchoring control

Resolve exact uppercase (*NO_DOTSTAR_ANCHOR) in the uninterrupted initial prefix run by caller/source global OR. Repeated/interleaved supported prefixes and ordinary resets retain the policy; Literal bypasses scanning. Two production files add the resolved start-setting bool and route it to the existing compiler policy. Parser/program proofs/emitter/matcher/VM/opcodes/frames/journals/diagnostic/API/dependencies/workflows stay fixed. Preserve independent explicit anchors/start study/possessification, requested-byte zero-width COMMIT, original owned bytes/body offsets, validation/lockout/no-read priority and capacities.

Pin578 groups/53,205 native PCRE2 10.48/JIT0 protocols,108 diagnostics and38 resource/ownership/view/mark/validation/no-read/capacity/cut cases:724 new/95,757 total. Separate1,736 strict split offsets; independently reproduce509 merged258 groups/45,361 protocols. After clean RED/freeze, refresh only3 old pending-syntax InlineData tokens to natively verified LIMIT_MATCH=1000000. Keep all assertion/helper bodies/counts/new frozen fixtures/other290 prior files/285 C# sources/frozen35 fixed. See [design](PCRE-NO-DOTSTAR-ANCHOR-PREFIX-DESIGN.md)/[plan](PCRE-NO-DOTSTAR-ANCHOR-PREFIX-PLAN.md). Require six fresh net10 results at95,757 passes/214 frozen outcomes/120 methods, all net8/net9/net10 outputs, zero failures/skips/compiler warnings/errors and every configured support gate. All seven inventories precede final qualification; final GREEN/author-review/source/base/tree/proposed-merge evidence belongs on PR259/issue7 footer only, preserving its original2600-character prefix; no docs-only CI repeat. #7/#102 and wider grouped/ordinary-dot/optimizer/syntax/directive/UTF-code-unit/callout/match/API/consumer/release gates stay open. Grep last; user merges/tags/publishes.


## Observed RED and fixture freeze

Tests-only source9b7c531e6dd10b8ede20c5881e3dbae72c201308/treed8534fd7bad684b567a085c2aed171fc7eae3f46, [run43855109002](https://github.com/uniblab/Icod.Expressions/actions/runs/43855109002), Linux x64 job133257261945:612 expected managed failures/95,145 passes/95,757 total/zero skips/compiler warnings/errors. Profile IDs exactly match546 prefixed groups and diagnostic IDs match34 post-prefix failures. Other failures are12 work-chain,4 cut-budget,6 ownership,6 capacity,2 instruction-limit and2 prefixed validation cases. Every native preflight, all95,033 prior tests and112 new controls pass. Production and all prior tests are identical to merged258; no fixture corrections were needed.

Freeze before production: reference 2ce8ab92a00fba0d0bc1461478a9a0b496d5e349; part1 60ce63c8b789e1659eca9a4bf2fe81510fb4918f; part2 52699f2cd64c480329cd4c8fbb2bd945252141be. Preserve all three new C# blobs byte-for-byte. Permit only the three documented pending-syntax InlineData token substitutions after this freeze; keep every assertion/helper body, count, other290 prior files/285 C# sources and frozen35 fixed. Actual merged258 main43852888931/scripts43852889196 are fully green, with six native95,033 results/frozen214/120/all three library targets. All seven documents precede final qualification; final GREEN/source identities remain external.


Implementation and whole-branch author review before qualification: two production files,5 insertions/3 deletions, add the resolved global bool and pass it to the existing compiler. No parser/program/matcher/proof/VM changes. Exact source spelling/placement, repeats/interleaving and Literal/quoted/class/comment/mark contexts; caller/source OR, resets and independent explicit-anchor/start/possessification/cut policies; original-byte length/whole-source UTF/lockout/diagnostic/no-read priority; owned source/settings/signed captures/marks, exact observed/unobserved budgets/cancellation and full capacities are reviewed against frozen fixtures. Three independent native replacement probes confirm LIMIT_MATCH=1000000 simple-byte match, FAIL no-match and UTF match (all JIT0). Exactly3 InlineData tokens changed; all helper/assertion bodies/counts, other290 prior files/285 C# sources and3 new frozen fixtures stay fixed. Incorrect replacement handling would leave a pending-directive rejection gap; final full qualification verifies it. No unresolved critical/important/minor findings. Local frozen35/corpus/A05/classifier/main-job guards and diff checks pass. All seven documents precede final source qualification; final GREEN/source evidence belongs externally on PR259/issue7 footer.


## PR260: mandatory leading groups in DotAll-star anchoring

Extend the existing bounded direct-root proof through a mandatory single-branch chain of capturing/noncapturing groups. Track native enclosing-capture bits1–31 and shared bit0 for numbers32+; reject overlapping numeric/named/duplicate/forward/erased references while retaining unrelated captures. Keep global lexical PRUNE/SKIP/targets exclusions, existing ALLANY provenance and owned NoDotStarAnchor policy, including disabled-anchor COMMIT entry. Iterative peeling/walk preserve cancellation and4096 proof fallback without a new admission limit. Broader alternatives, repeated groups, assertions/conditions, ordinary-dot STARTLINE and other #102 availability remain deferred. Only PcreByteProgram.cs changes in production; parser/AST/matcher/VM/API/dependencies/workflows stay fixed.

Pin257 matching groups/15,033 native PCRE2 10.48/JIT0 protocols,15 diagnostics and34 resource/work/ownership/view/capture/mark/validation/capacity cases:306 new/96,063 total. Keep372 strict split offsets separate;51 malformed-fragment protocols from3 deferred repeated/plain-dot boundary controls remain research. Preserve every prior InlineData/outcome/count/assertion; correct only3 PR259 unused-ID warning labels/non-null diagnostic assertions, leaving other293 prior files/288 C# sources and frozen35 fixed. See [design](PCRE-GROUPED-DOTSTAR-ANCHOR-DESIGN.md)/[plan](PCRE-GROUPED-DOTSTAR-ANCHOR-PLAN.md) for native-first RED/refreeze evidence and precise correction rationale.

NO_JIT remains pending: independently reproducing578 merged259 groups/53,205 protocols then disabling native JIT changes447 results across49 profiles; it cannot be accepted as a no-op. Interpreter policy needs its own implementation. All seven inventories precede final source qualification. Require six fresh net10 records at96,063 passes/214 frozen outcomes/120 methods, all net8/net9/net10 outputs, no failures/skips/compiler or analyzer warnings/errors, and every configured support gate. Final GREEN/whole-branch author review/source/base/tree/proposed-merge identities belong externally on PR260/issue7 footer, preserving the original2600-character prefix; no docs-only CI repeat. #7/#102 stay open; Grep last; user merges/tags/publishes.


## Corrected RED and freeze before production

Tests-only source1eb9a633bace9695c337af87798b4540f4a7e6f7/tree49d5e932f7c4bd4eaa4bee2f340a1f476325b869, [run43865152771](https://github.com/uniblab/Icod.Expressions/actions/runs/43865152771), Linux x64 job133291935616:52 expected managed failures/96,011 passes/96,063 total/zero skips/compiler or analyzer warnings/errors. Exact43 matching profile IDs are3,6,9,12,15,18,21,24,27,30,39,42,45,48,51,54,57,60,63,66,69,72,75,78,81,84,171,177,180,183,186,220,221,222,227,228,229,230,235,236,242,244,247. Other failures are8 disabled=false work-chain cases and1 tolerant ownership case. Every native preflight, all95,757 prior tests and254 new controls pass. Production is still identical to merged259.

Freeze new C# blobs: reference 358a29cc14376f2fef4b4c5d9007dba1580819da; protocols ed24ca095e2ee720df1afbb3f5029ce01f681391. Freeze the3 prior label-only warning corrections as well: reference 936920fe4f63b39ddfb2a5cf3ef3e9aa153bd3b3; part1 6fbc748cbdc612edbd5a264e1dd51d5922119bc7; part2 ca56011d77b99049dda7537670c2faf3a3eec7e3. Every prior InlineData/outcome/count/existing assertion and all other293 prior files/288 C# sources/frozen35 remain fixed. No fixture edits after this freeze.


Implementation/whole-branch author review before qualification: one production file peels mandatory single branches with bounded iteration/cancellation, then reuses direct star provenance/global lexical eligibility and native enclosing/reference bitmap rules. Direct-root map0 leaves prior eligibility unchanged; numbers32+ share bit0. No parser/emitter/matcher/VM/API/dependency/workflow change. Reviewed all five plan boundaries, including unrelated and erased/duplicate references, atomic/optional/multi-branch barriers, source/caller controls and COMMIT entry, proof exhaustion versus existing admission limits, signed captures/marks/owned slices, exact observed/unobserved budgets/cancellation and full capacities. Five frozen C# blobs remain byte-identical; exactly3 prior label-only corrections retain every old InlineData/outcome/count/assertion, other293 prior files/288 C# sources fixed. Local frozen35/corpus/A05/classifier/main-job/diff gates pass. No unresolved critical/important/minor findings. All seven inventories precede final source qualification; final GREEN and reviewed source identities remain external.


## PR261: DotAll-star anchoring across alternatives

Extend the existing bounded leading-group proof to root/nested alternatives only when every nonempty branch begins with a qualifying DotAll star, directly or through mandatory capturing/noncapturing groups. Use an iterative frontier and the union of leading enclosing-capture bits: all paths passing the native global backref map is equivalent to no intersection with their union. Preserve duplicate/forward/erased reference dependencies and native32+ shared bit0, unrelated captures, lexical PRUNE/SKIP/targets exclusions, ALLANY provenance, THEN/MARK/COMMIT and disabled-anchor requested-byte entry. Caller/source NoDotStarAnchor and NoStartOptimize/NoAutoPossess independence remain intact. Frontier visits/pending storage and the lexical walk retain4096 limits/cancellation; exhaustion declines inference without adding admission errors. Only PcreByteProgram.cs changes in production; parser/AST/emitter/matcher/VM/API/dependencies/workflows remain fixed.

Pin171 matching groups/13,041 native PCRE2 10.48/JIT0 protocols,15 diagnostics and34 work/ownership/view/capture/mark/validation/capacity cases:220 new/96,283 total. Keep282 strict split offsets separate. Independently reproduce257 merged260 groups/15,033 protocols and44 supplemental native profiles. Freeze new C# after exact expected tests-only RED; all298 prior files/293 C# sources/frozen35 stay byte-identical. See [design](PCRE-ALTERNATIVE-DOTSTAR-ANCHOR-DESIGN.md)/[plan](PCRE-ALTERNATIVE-DOTSTAR-ANCHOR-PLAN.md). Two-branch instruction ceiling:4082 first-branch literals plus14 overhead reaches4096;4083 remains over limit.

All seven inventories precede final source qualification. Require six fresh native net10 records at96,283 passes/frozen214 outcomes/120 methods, zero failures/skips/compiler or analyzer warnings/errors, all net8/net9/net10 library outputs and every configured support gate. Final GREEN/whole-branch author review/source/base/tree/proposed-merge identities belong on PR261/issue7 footer only, preserving its original2600-character prefix; no docs-only CI repeat. Mixed explicit-anchor/star alternatives, repeated groups, assertion/conditional/ordinary-dot inference, NO_JIT interpreter policy and wider#102/API/consumer/release gates remain open; #7/#102 stay open; Grep last; user merges/tags/publishes.


### Pre-freeze scope qualification

Initial tests-only sourcefe2625bd090f4c61772142066877e2ee8ef1ed6e/tree2906ff3a01abdc1ef68ead3198a1733a6a176c4a, [run43870480693](https://github.com/uniblab/Icod.Expressions/actions/runs/43870480693), Linux x64 job133310013154:43 failures/96,241 passes/96,284 total/zero skips/compiler or analyzer warnings/errors; all96,063 prior tests and every native preflight pass.42 failures concern this frontier expansion (33 matching profiles,8 work cases,1 ownership). Profile102 is a separate pre-existing UTF full-class provenance gap: `(?:[\s\S]*A|[\d\D]*B)` under mode5 has native ALLOPTIONS anchored=true, while the managed scalar-set node is deliberately outside the existing Any/byte-ALLANY proof. On C3A942 at offset1 native misses and managed returns2:1.

Keep that profile/91 native protocols as deferred research pinned in the original tests-only commit; remove only its new, unfrozen InlineData row before qualification. No other new assertion/expectation/ID changes; all298 prior files/293 C# sources/frozen35 remain byte-identical. Byte mode retains its eligible full-class coverage; strict UTF boundary controls remain. A separate native-provenance design is needed before recognizing UTF full classes/property complements as ALLANY. Freeze after the corrected42-failure tests-only run; this PR preserves existing star provenance and changes only the all-branches frontier.


## Corrected RED and freeze before production

Tests-only sourceeac741dbb0b7869098b3fb9e00adec78f323263e/treebdffce36a006d979c05d9fb2a01d369095681f99, checkout221382a282bad857c9f75cb85505224defbf7da3 with the same tree/ordered base-head parents, [run43871928507](https://github.com/uniblab/Icod.Expressions/actions/runs/43871928507), Linux x64 job133315239300:42 exact expected managed failures/96,241 passes/96,283 total/zero skips/compiler or analyzer warnings/errors. Matching IDs3,6,9,12,15,18,21,24,27,30,33,36,57,60,63,66,109,110,118,119,120,124,125,137,144,145,151,152,153,154,159,160,169; remaining failures8 disabled=false work cases and1 tolerant ownership case. Every native preflight, all96,063 prior tests and178 new controls pass. Production still equals merged260.

Freeze before production: reference 4e00d532c6567c547dc7c86a5691569cb24b5dc4; protocols bde0dd044e3bde230ea9fc1fbaefdb9bc72ecbf8. No subsequent fixture changes. Preserve all298 prior files/293 C# sources/frozen35 byte-for-byte. Merged260 main43868986288/scripts43868986428 are fully green, with six96,063 net10 results/frozen214/120/all3 library targets/zero compiler or analyzer warnings/errors; Intel aggregate133312727077 verifies the same single result. All seven inventories precede final qualification; final GREEN/author-review/source identities remain external.


Implementation and whole-branch author review before qualification: one production file,37 insertions/21 deletions, replaces single-path peeling with a bounded alternative-body stack. Every branch validates its mandatory leading group/star; capture bits are unioned and tested by the unchanged global lexical walk. Requiring each path to avoid the global backref map is equivalent to checking the union. Zero-branch/empty/optional/atomic/assertion/unknown paths decline. Existing star/ALLANY provenance, target/cut exclusions and disabled-anchor COMMIT entry stay fixed. Frontier visits plus pending entries cannot exceed4096; exhaustion returnsfalse, with cancellation at each body/branch. No parser/emitter/matcher/VM/API/dependency/workflow changes.

Reviewed all five plan boundaries against native fixtures and source, including sibling/nested alternatives and unchanged single paths, dependency union/duplicate/erased/32+ bits, cuts/marks/source-caller policies, exact observed/unobserved budgets/cancellation and ownership/diagnostics/full capacities/proof exhaustion. Both new frozen C# blobs and all298 prior files/293 C# sources/frozen35 remain fixed. Local frozen-source/corpus/A05/classifier/main-job/diff gates pass. No introduced unresolved critical/important/minor findings; the separately pinned pre-existing UTF full-class provenance gap remains research for its own increment. All seven inventories precede final source qualification; final GREEN/author-review/source identities belong externally on PR261/issue7 footer.


## PR262: UTF ASCII-complement classes in star anchoring

Close the original PR261 profile102/91 protocols by recognizing a UTF ordinary Class scalar set with no ranges/outer negation and exactly two opposite predicates of the same ASCII digit/word/space kind (null category/zero property value). Native compile_class lowers these ASCII special/POSIX pairs to ALLANY; ordinary UCP/Unicode-property complements retain different provenance and must not infer anchoring merely because their semantic union is universal. Use a constant-time two-predicate check in PcreByteProgram.cs only; preserve class matching/work instructions and all parser/AST/matcher/VM/API/dependency/workflow boundaries. Existing bounded group/alternative/capture/32+ bit/global cut/target proof and caller/source NoDotStarAnchor/COMMIT entry remain intact and independent from start/possessification settings.

Pin196 groups/15,938 native PCRE2 10.48/JIT0 protocols,15 diagnostics and34 work/ownership/view/capture/mark/validation/capacity cases:245 new/96,528 total;846 strict split offsets remain separate. Independently reproduce171 merged261 groups/13,041 protocols, all original91 research protocols unchanged and44 supplemental native profiles. Stronger malformed/split subjects include repeated captures and31/32 enclosing-capture dependency witnesses. Freeze new C# after exact expected tests-only RED; all300 prior files/295 C# sources/frozen35 stay byte-identical. See [design](PCRE-UTF-ALLANY-STAR-ANCHOR-DESIGN.md)/[plan](PCRE-UTF-ALLANY-STAR-ANCHOR-PLAN.md).

Broader class provenance (additional members/ranges, h/v/Any properties, mixed Unicode/ASCII and algebra), mixed-anchor/repeated/assertion/conditional/ordinary-dot inference, NO_JIT interpreter policy and wider#102/API/consumer/release gates remain separately scoped. Require six fresh native net10 results at96,528 passes/frozen214 outcomes/120 methods, all net8/net9/net10 library outputs, no failures/skips/compiler or analyzer warnings/errors and every configured support gate. All seven inventories precede final qualification; final GREEN/author-review/source/base/tree/proposed-merge evidence belongs on PR262/issue7 footer, preserving the original2600-character prefix; no docs-only CI repeat. #7/#102 open; Grep last; user owns merge/tag/publication.


## Observed RED and fixture freeze

Tests-only source1c9ccd0c178e87a6a905ec4fc9c5d94633c1d0db/treef704874e7d0d6016eba5788ec6d0aefc30bfb23f/checkout4d3e3f04393eb09314de991d3aeeb47773f67f2b (identical tree/ordered base-head parents), [run43879354675](https://github.com/uniblab/Icod.Expressions/actions/runs/43879354675), Linux x64 job133340771591:60 exact expected failures/96,468 passes/96,528 total/zero skips/compiler or analyzer warnings/errors.53 matching IDs1,5,6,11,12,17,18,23,24,29,30,35,36,41,42,47,48,51,53,55,57,59,61,63,69,75,85,86,91,92,93,97,98,106,113,114,121,122,123,124,125,169,170,171,172,177,178,180,184,187,188,195,196; remaining failures6 UTF disabled=false work cases and1 tolerant ownership case. Every native preflight, all96,283 prior tests and185 new controls pass; the original91 research rows fail only after their native preflight. No fixture correction needed. Production still equals merged261.

Freeze before production: reference 3cf50f4013bd25f45926409d1bd00672e17862c9; protocols 26eed11290838b33f6c0aa5f34cf9f4f073f8604. No later fixture edits. All300 prior files/295 C# sources/frozen35 remain byte-identical. All seven inventories precede final source qualification; final GREEN/author-review/source identities belong externally on PR262/issue7 footer.


Implementation and whole-branch author review before qualification: PcreByteProgram.cs only,23 insertions/3 deletions, adds a UTF eligibility arm and constant-time helper. Require ordinary positive Class source, empty ranges, exactly two opposite predicates of the same AsciiDigit/AsciiWord/AsciiSpace kind, null categories and zero property values. Do not normalize class instructions or infer arbitrary universal sets. Existing frontier/capture/global cut/target/proof-budget logic, NoDotStarAnchor/COMMIT entry, start/possessification policy and all parser/AST/emitter/matcher/VM/API/workflow boundaries remain fixed. Two immutable predicate copies introduce no borrowed lifetime or new resource limit.

Reviewed all five plan boundaries: native source/kind discrimination and UCP/property counterexamples, aliases/scoped restrictions/casing and byte controls, all91 original protocols plus malformed31/32 capture-dependency witnesses, cuts/marks/caller-source policies, exact observed/unobserved budgets/cancellation/owned views/signed captures/diagnostics/full capacities. Both frozen new C# files and all300 prior files/295 C# sources/frozen35 remain unchanged. Local frozen source/corpus/A05/classifier/main-job/diff gates pass. No introduced unresolved critical/important/minor findings; broader class provenance remains separately scoped. All seven inventories precede final source qualification; final GREEN/author-review/exact source identities remain external on PR262/issue7 footer.


## Frozen-fixture source qualification correction

First implementation sourceee38eb9a6cf86221991b3a2b7b779b0f5351102f/treecf0448dfc1ba4c62c0b8055a1f371e3ae2d38f37, [run43881053289](https://github.com/uniblab/Icod.Expressions/actions/runs/43881053289), Linux x64 job133346724983:1 failure/96,527 passes/96,528 total/zero skips/compiler or analyzer warnings/errors. All prior cases and244 other new cases pass; frozen profile125 fails after native preflight. `(?aD)[[:digit:]\D]*B` with UCP/tolerant UTF leaves POSIX digit Unicode while restricting only backslash-D. Native still emits ALLANY: the negated ASCII digit covers all higher characters and the Unicode positive digit fills its ASCII bitmap holes. The original exact-kind assumption was too narrow; no parser bug or fixture change.

Extend only the helper to accept a positive DecimalDigit with negative AsciiDigit, in either operand order, alongside existing same-ASCII-family complements. Keep ordinary positive Class/no ranges/two opposite predicates/null categories/zero property values and all other guards. Four independent native/JIT0 probes verify both source families/orderings (aD POSIX-positive+backslash-negative; aP backslash-positive+POSIX-negative); two reverse-policy counterexamples stay unanchored. On C3A942 at1 true cases miss; reverse cases match2:1. Do not extend arbitrary mixed Unicode/ASCII classes. Both frozen new C# blobs, all300 prior files/295 C# sources/frozen35 stay unchanged.

Production remains one file,23 insertions/3 deletions. Follow-up author review confirms the hole-cover proof, order independence and negative-side ASCII requirement; ordinary UCP/Unicode-property complement counterexamples remain excluded. Existing class instructions/work, frontier/capture/cut/caller/COMMIT policy, all capacities and parser/VM/API/workflows stay fixed. Local guards/diff and frozen preservation checks pass. All seven inventories precede corrected source qualification; final GREEN/exact source evidence remains external.


## PR263: UTF horizontal/vertical complement classes in star anchoring

Extend the existing constant-time source-aware Class predicate to exact opposite HorizontalSpace/VerticalSpace pairs in either order, including UCP/casing/restrictions. Native compile_class normalizes these explicit list/complement pairs to ALLANY. Rename IsAsciiComplementAllAny to IsComplementAllAny; retain ordinary positive Class source/no ranges/two predicates/null categories/zero property values and existing ASCII/mixed decimal recognition. Only PcreByteProgram.cs changes in production; retain class instructions/work, bounded group/alternative/dependency/32+ bit/global cut/target proof, NoDotStarAnchor/disabled-anchor COMMIT entry and independent start/possessification controls. Parser/AST/emitter/matcher/VM/API/dependencies/workflows unchanged.

Pin246 matching groups/23,704 native PCRE2 10.48/JIT0 protocols,15 diagnostics and34 work/ownership/view/capture/mark/validation/capacity cases:295 new/96,823 total. Keep2,712 strict split-offset(-36) research rows separate. Reproduce all196 merged262 groups/15,938 protocols independently. Cover both families/orders, UCP/casing/scoped restrictions, byte modes, same-sign/outer-negative/mixed/property counterexamples, groups/alternatives/capture references/31-32 bitmap boundaries/cuts/marks/caller-source policies and full capacities. Freeze new C# after exact tests-only RED; all302 prior files/297 C# sources/frozen35 remain byte-identical. See [design](PCRE-UTF-SPACE-COMPLEMENT-ANCHOR-DESIGN.md)/[plan](PCRE-UTF-SPACE-COMPLEMENT-ANCHOR-PLAN.md).

Additional members/ranges/Any properties, other mixed Unicode/ASCII pairs and algebra, mixed-anchor/repeated/assertion/conditional/ordinary-dot inference, NO_JIT interpreter policy and wider#102/API/consumer/release gates remain separately scoped. Require six fresh native net10 results at96,823 passes/frozen214 outcomes/120 methods, all net8/net9/net10 library outputs, zero failures/skips/compiler or analyzer warnings/errors and every configured support gate. Seven inventories precede final source qualification; final GREEN/author-review/source/base/tree/merge evidence belongs on PR263/issue7 footer, preserving its original2600-character prefix; no docs-only CI repeat. #7/#102 open; Grep last; user owns merge/tag/publication.


## Observed RED, freeze and implementation review

Tests-only sourceae3ab2e40ca6162aad0326ed5967747c64e0ae24/tree07515eefece2b6d72fc152c3d3549c3d9ddbca99, checkoutec2835b77caaf1b9949040a46db6e8a1512b5f84 (identical tree; ordered base3568507574eeab3f18e4d5ef2b4a805923e76157/head parents), [run43890422396](https://github.com/uniblab/Icod.Expressions/actions/runs/43890422396), Linux x64 job133379801191:76 expected managed failures/96,747 passes/96,823 total/zero skips/compiler or analyzer warnings/errors.69 matching IDs7,8,13,14,19,20,25,26,34,35,40,41,46,47,52,53,61,62,67,68,73,74,79,80,88,89,94,95,100,101,106,107,110,112,114,116,118,120,122,128,134,144,145,150,151,152,156,157,165,172,173,179,180,181,182,187,188,190,194,197,201,239,240,241,242,243,244,245,246; remaining failures6 disabled=false UTF work cases and1 tolerant ownership case. All native preflights, all96,528 prior tests and219 new controls pass. Three COMMIT entry witnesses128/152/165 supplement the66 ordinary inferred-vs-disabled witnesses: native can execute zero-width COMMIT at a split byte before class consumption; the existing bounded entry proof needs the new class families. No fixture corrections.

Freeze before production: reference8360c699d30a5756b897da28e8c283b864c46a3f; protocols7466e135e4ebbd2a648698555876e29c5fc6db40. Keep both blobs and all302 prior files/297 C# sources/frozen35 byte-identical. Merged262 main43888718709/scripts43888718494 are fully green: six96,528 net10 results/frozen214/120/all3 library outputs/no compiler or analyzer warnings/errors; Intel aggregate133380441980 verifies the same single native result. No retry/code change needed.

Implementation and whole-branch author review before qualification: PcreByteProgram.cs only renames IsComplementAllAny and adds HorizontalSpace/VerticalSpace to same-kind opposite predicate eligibility. Existing mixed DecimalDigit-positive/AsciiDigit-negative exception remains exact. Ordinary positive Class/no ranges/two opposite predicates/null categories/zero property values unchanged; parser emits h/H/v/V independently from UCP/ASCII restrictions and property aliases. Native explicit list union/complement normalization supports both operand orders and ignores casing. No semantic-wide universality inference or class instruction normalization.

Reviewed provenance/counterexamples; groups/alternatives/enclosing dependency31-32 bits/lexical cuts; NoDot/COMMIT/start/possessification policies; unchanged instructions/work/exact observed-unobserved budgets/cancellation/ownership/signed captures/marks/diagnostics/full capacities/proof exhaustion; source/tree/frozen preservation. Surrounding proof traversal, admission limits and all parser/AST/emitter/matcher/VM/API/dependency/workflow boundaries remain fixed. No introduced unresolved critical/important/minor findings. Local frozen-source/corpus/A05/classifier/main-job/diff gates pass. Seven inventories precede final source qualification; final GREEN/author-review/exact source evidence remains external.


## PR264: UTF Any-property star anchoring

Extend only PcreByteProgram.cs: rename IsComplementAllAny to IsUtfAllAny; admit a positive AnyScalar ScalarClass atom or an ordinary positive Class/no ranges/exactly one positive AnyScalar predicate (null category/zero property value). Native PCRE2 10.48 explicitly lowers PT_ANY to ALLANY; negative Any becomes an empty class. Retain all prior complement eligibility, scalar instructions/work and bounded group/alternative/capture31-32 bitmap/global-cut/target/NoDot/COMMIT/start/possessification policy. Parser/AST/emitter/matcher/VM/API/dependencies/workflows fixed.

Pin254 matching groups/22,492 native JIT0 protocols,15 diagnostics/34 supplementary cases:303 new/97,126 total. Keep2,300 strict split offsets separate; reproduce all246 merged263 groups/23,704 protocols. Direct/singleton/positive-caret/loose-name aliases, UCP/casing/restrictions, byte controls, negative Any/Unicode-property counterexamples, groups/alternatives/dependencies/cuts/marks/policies/work/ownership/diagnostics/full capacities are covered. Freeze new C# after exact tests-only RED; all304 prior files/299 C# sources/frozen35 remain byte-identical. See [design](PCRE-UTF-ANY-PROPERTY-STAR-ANCHOR-DESIGN.md)/[plan](PCRE-UTF-ANY-PROPERTY-STAR-ANCHOR-PLAN.md).

Additional members/ranges/outer negations/algebra/other semantic full classes, byte AnyProperty work inference, target/call and other anchor inference, NO_JIT interpreter policy and wider#102/API/consumer/release gates remain separate. Research/target controls use NoDotStarAnchor where needed. Require six fresh97,126 net10 passes/frozen214 outcomes/120 methods/all3 library targets/zero failures/skips/compiler or analyzer warnings/errors and all support gates. Seven inventories before final qualification; final GREEN/review/source/base/tree/merge evidence external on PR264/issue7 footer, original2600-character prefix intact; no docs-only CI repeat. #7/#102 open; Grep last; user merges/tags/publishes.


## Observed RED, freeze and implementation review

Tests-only source9709708516817332be7e07f8ba1736ed08c56948/tree420766b319e21d168362ee43e2afe675a5e68678, checkout813c6143362bf66a4a6bf82b52aabbc2d6ed9c34 (identical tree/ordered basea7ea3fc91a609bfaa1dd4e14bfdc19dda4acd6ff/head parents), [run43895809155](https://github.com/uniblab/Icod.Expressions/actions/runs/43895809155), Linux x64 job133398921378:78 exact expected managed failures/97,048 passes/97,126 total/zero skips/compiler or analyzer warnings/errors.71 matching IDs4,5,10,11,16,17,22,23,28,29,34,35,40,41,46,47,52,53,58,59,64,65,70,71,76,77,82,83,88,89,94,95,100,101,106,107,110,112,114,116,118,120,122,128,134,144,145,150,151,152,156,157,165,172,173,179,180,181,182,187,188,190,194,235,236,237,238,239,240,241,242 (68 ordinary anchor witnesses plus3 existing zero-width COMMIT entry witnesses128/152/165),6 disabled=false UTF work cases and1 tolerant ownership case. Every native preflight, all96,823 prior tests and225 new controls pass; no fixture corrections.

Freeze before production: referenceca1d0a8e081eb41420ceecf29e8e1f8486bdd087; protocolsdcb716417f128ef009384a7c30bfb450683a30da. Both new blobs and304 prior test files/299 C# sources/frozen35 remain byte-identical. Merged263 main43894601891/scripts43894602469 fully green attempt1:six96,823 net10 results/frozen214/120/all3 library targets/no compiler or analyzer warnings/errors; Intel aggregate133401222539 verifies the same single result; all3 scripts31 checks/Ubuntu8 guards. No retry/code change.

Implementation/whole-branch author review before qualification: PcreByteProgram.cs only renames IsUtfAllAny and admits an exact positive AnyScalar ScalarClass or one-predicate ordinary positive Class with no ranges (null categories/zero property values). Length0 or>2 declines; non-Any singletons decline; length2 retains every prior complement rule. Native compile.c PT_ANY handling and compile_class singleton behavior support both parser forms/positive-caret/loose-name aliases; parser emits negative Any separately and algebra as an expression. No arbitrary semantic universal-set inference, source metadata change or instruction normalization. UTF callsite preserves byte AnyProperty behavior.

Reviewed all plan boundaries: source/provenance/alias/negation/property counterexamples; groups/alternatives/dependency31-32 bits/global cuts/targets; source/caller NoDot/zero-width COMMIT/start/possessification; unchanged instructions/work/exact observed-unobserved budgets/owned slices/signed captures/marks/validation/cancellation/diagnostics/full capacities/proof exhaustion; frozen/source/tree preservation. No new allocation, borrowed lifetime or quota/admission limit. Parser/AST/emitter/matcher/VM/API/dependencies/workflows fixed. No introduced unresolved critical/important/minor findings. Local frozen-source/corpus/A05/classifier/main-job/diff gates pass. Seven inventories precede final source qualification; final GREEN/review/exact source identities remain external.


## PR265: UTF double-negated Any singleton star anchoring

Extend only IsUtfAllAny in PcreByteProgram.cs: ordinary Class/no ranges/exactly one AnyScalar predicate/null category/zero property value requires equal predicate/outer negation flags. Preserve positive Any singletons and standalone Any; admit exact [^\P{Any}] and [^\p{^Any}] aliases. Reject outer negation before the unchanged two-predicate complement branch. Native compile_class lowers these exact double negations to ALLANY. No semantic-wide negation algebra or instruction normalization; all bounded group/alternative/dependency31-32 bitmap/global-cut/target/NoDot/COMMIT/start/possessification policies and parser/AST/emitter/matcher/VM/API/dependencies/workflows remain fixed.

Pin248 groups/24,787 native PCRE2 10.48/JIT0 protocols,15 diagnostics/34 supplementary cases:297 new/97,423 total;2,507 strict split offsets separate. Reproduce all254 merged264 groups/22,492 protocols independently. Cover both double-negative forms/loose aliases/UCP/casing/restrictions, negative Any/outer-negative complement/mixed/range controls, byte Exact bitmap controls, groups/alternatives/dependencies/cuts/marks/policies/work/budgets/ownership/signed captures/diagnostics/validation/cancellation/full capacities/proof exhaustion. Freeze new C# after exact tests-only RED; all306 prior files/301 C# sources/frozen35 byte-identical. See [design](PCRE-UTF-DOUBLE-NEGATED-ANY-ANCHOR-DESIGN.md)/[plan](PCRE-UTF-DOUBLE-NEGATED-ANY-ANCHOR-PLAN.md).

Additional members/ranges/duplicates/algebra/other full-set or target inference and wider#102/API/consumer/release gates remain separate. Seven inventories before final qualification; require six fresh97,423 net10 passes/frozen214 outcomes/120 methods/all3 library targets/zero failures/skips/compiler or analyzer warnings/errors and every support gate. Final GREEN/review/source/base/tree/merge evidence external on PR265/issue7 footer; original2600-character prefix preserved; no docs-only CI repeat. #7/#102 open; Grep last; user merges/tags/publishes.


## Observed RED, freeze and implementation review

Tests-only source316cac8f885b3e5cc223edd398fd8cba8b0f89f5/treeb1a675f65ab2f10dded3aac7880d0f54ce7a8a38, checkout8935ca2d833ee52340ff8215d23f7169f71016b4 (identical tree/ordered base96aad43cbd40c74f6586ea0e82e747606f5df58f/head parents), [run43937691525](https://github.com/uniblab/Icod.Expressions/actions/runs/43937691525), Linux x64 job133546523537:74 exact expected managed failures/97,349 passes/97,423 total/zero skips/compiler or analyzer warnings/errors.67 matching IDs7,8,13,14,19,20,25,26,34,35,40,41,46,47,52,53,61,62,67,68,73,74,79,80,88,89,94,95,100,101,106,107,110,112,114,116,118,120,122,128,134,144,145,150,151,152,156,157,165,172,173,179,180,181,182,187,188,190,194,237,238,239,240,241,242,243,244; remaining failures6 disabled=false UTF work cases/1 tolerant ownership case. Every native preflight, all97,126 prior tests and223 new controls pass. No fixture correction.

Freeze before production:reference6edf26795d16f40a050604359a2fc18289c85d97;protocolsb7432881d1708ba6e99f4ebaaecf867b9fb07e71. Both new blobs and306 prior files/301 C# sources/frozen35 remain byte-identical. Merged264 main43935934306/scripts43935934247 fully green attempt1:six97,126 net10 results/frozen214/120/all3 library targets/no compiler or analyzer warnings/errors; Intel aggregate133547334580 verifies one native result;3 scripts31 checks/Ubuntu8 guards.

Implementation and whole-branch author review before qualification: PcreByteProgram.cs only keeps exact ordinary Class/no ranges/one AnyScalar/null category/zero property value and compares predicate.Negated==set.Negated. This preserves positive singleton Any and recognizes the double negation. Move outer-negation rejection to the two-predicate branch; keep direct positive Any and every prior complement condition unchanged. Zero or>2 predicates decline; non-Any singletons decline; every negated two-predicate set declines. Native PT_ANY class lowering, not arbitrary semantic universality, supports this exact rule. No node normalization/new allocation/borrowed lifetime/quota or admission change. Byte Exact full-class inference and all instruction/work metadata remain fixed.

Reviewed all plan boundaries: source/kind/count/range/negation parity and property/loose-alias controls; existing direct/positive Any and ASCII/horizontal/vertical/mixed-decimal complements; grouped alternatives/dependency31-32 bits/global cuts/targets; source/caller NoDot/COMMIT/start/possessification; exact observed-unobserved budgets/owned slices/signed captures/marks/diagnostics/validation/cancellation/full capacities/proof exhaustion; frozen/source/tree preservation. Parser/AST/emitter/matcher/VM/API/dependencies/workflows fixed. No introduced unresolved critical/important/minor findings. Local frozen-source/corpus/A05/classifier/main-job/diff gates pass. All seven inventories precede final qualification; final GREEN/review/source/base/tree/merge evidence remains external.


This bounded full-range increment follows merged265; broader universality and consumer/release gates remain open.


## PR266: UTF full scalar-range star anchoring

Continue merged #265 at 410447d1c0de322ec5d6ab56760716abf9233a16,
tree 17a90db0123408d1ea38c441e6b920a1630dc394; 97,423 baseline tests.
PCRE2 10.48 fills its byte bitmap and marks all higher characters for the
positive literal range 0..10FFFF, then emits ALLANY. A leading star over
[\x{0}-\x{10FFFF}] therefore anchors. On C3A942 at offset 1 in tolerant UTF,
native misses; the previous managed proof could retry and return 2:1.

Only IsUtfAllAny in PcreByteProgram.cs changes in production (5 additions,
2 deletions). Require ordinary Class source, positive outer class, no predicates,
and exactly one stored range with Start 0 and End 0x10FFFF. Retain the nonempty
range rejection before all existing singleton Any negation-parity and two-predicate
complement rules. Standalone positive Any, byte Exact bitmap inference, and every
bounded group/alternative/dependency 31/32 bitmap/global cut/target/NoDot/COMMIT/
start/possessification gate remain unchanged. Existing caseless closure can merge
ranges into this exact stored interval; no new normalization or range algebra.
Negated/near-full/partial ranges and property mixtures do not satisfy this guard.
Multiple ranges/additional members remain outside this rule unless existing
case closure already produces the exact one-range shape. Matching instructions,
work metadata and parser/AST/emitter/matcher/VM/API/dependencies/workflows are fixed.
No new allocation, borrowed lifetime, quota or admission limit.

Native-first fixtures reproduce all 248 merged265 groups/24,787 protocols using
PCRE2 tag 7978954dbd2efc6f2196869290553cf1871b4ce6 and SLJIT
de0259c7aaf36aa40cba8014f3fad3edde9307f9; every JIT_COMPLETE result is 0.
New fixtures pin 265 matching groups/31,988 native protocols, 15 diagnostic and
34 supplementary cases: 314 new cases, 97,737 expected total. Cover three full
range spellings, NUL/U+10FFFF endpoints, missing endpoints/negative/property/
range-mixture controls, byte 0..FF controls, casing/UCP/restrictions, groups,
alternatives, dependencies/cuts/marks, caller/source policies, malformed/split UTF,
empty-match policies, signed captures, exact observed/unobserved work budgets,
owned slices/options, validation/cancellation, full instruction/capture/depth
capacities and proof-exhaustion fallback.

### Fixture correction, observed RED and freeze

Initial tests-only source ac672e37be972dad9c58e0223c848c4b71314fa8,
[run 43950586072](https://github.com/uniblab/Icod.Expressions/actions/runs/43950586072),
Linux x64 job 133591678536: 185 failures/97,552 passes/97,737 total. Exactly
119 new groups contained malformed strict-UTF prefixes before a valid start:
native validates the relevant suffix, while our documented strict API validates
the whole subject. Correct fixture selection before freezing; retain explicit
whole-subject validation assertions and unchanged production. Keep 7,497 research
rows separate: 4,046 malformed-whole-subject rows and 3,451 split offsets; native
itself reports 5,117 errors. All 10,234 retained strict comparisons independently
pass whole-subject UTF validation and scalar-aligned starts.

Corrected tests-only source b6495a0a98ab005920bfc07212f6a0bca2a9aac9,
tree 6cab833dd9159b08f7b42435c142cb657e704214, checkout
f1984db953063c0173eea5e9c6053cff1205ded3 (identical tree; ordered base/head
parents), [run 43951573405](https://github.com/uniblab/Icod.Expressions/actions/runs/43951573405),
Linux x64 job 133595405101: exactly 66 expected managed failures, 97,671 passes,
97,737 total, zero skips/compiler or analyzer warnings/errors. The 59 matching
IDs are 7,8,13,14,19,20,25,26,34,35,40,41,46,47,52,53,61,62,67,68,73,74,79,80,
83,85,87,89,91,93,95,101,107,117,118,123,124,125,129,130,138,145,146,152,153,
154,155,160,161,163,167,237,239,241,243,245,247,249,251; add six disabled=false
UTF work cases and one tolerant ownership case. All native preflights, all 97,423
prior tests and 248 new controls pass. The only fixture correction was the
pre-freeze strict-validation separation described above.

Freeze before production: reference fb741b7492915b336be0d3f9361d3203e9e9be17;
protocols 9f438c3b89315114b12c7593c7078e6128e6ab84. Preserve both new blobs,
all 308 prior test files/303 C# sources and frozen35 byte-identically.

Merged265 closure: main 43948453014/scripts 43948452986 green at attempt 1.
Six native 97,423 net10 results/frozen214 outcomes/120 methods, all three library
targets, zero compiler or analyzer warnings/errors; Intel aggregate 133592837315
verifies the same single native result. Script jobs 133584240263/133584240822/
133584240882 each pass 31 checks; Ubuntu passes eight main-job rejection cases.
The actual merge tree matches reviewed9a9eb9d11569d48b7dff3b5e0fb54ed577e8846f
and tested proposed merge260b7b456db363e865b130d0b36fb7ebd07c1aa7. No retry.

### Review and final qualification

Whole-branch author review checks source/kind/count/range/outer-negation/property
boundaries and existing case closure; all prior Any/complement eligibility;
bounded traversal/dependency31-32/global cuts/targets; NoDot/COMMIT/start/
possessification; fixed instructions/work/exact budgets/ownership/signed captures/
marks/diagnostics/validation/cancellation/full capacities/proof exhaustion; source,
tree and frozen preservation. No introduced unresolved critical/important/minor
findings. Execute inline under standing direction; no subagents/approval pauses.

All seven inventories precede final source qualification: these five shared
inventories plus [design](PCRE-UTF-FULLRANGE-STAR-ANCHOR-DESIGN.md) and
[plan](PCRE-UTF-FULLRANGE-STAR-ANCHOR-PLAN.md). Require six fresh 97,737/97,737
net10 results/frozen214 outcomes/120 methods, net8/net9/net10 library outputs,
zero failures/skips/compiler or analyzer warnings/errors and every support gate.
Final GREEN/review/exact source/base/tree/merge identities remain external on
PR266 and issue7's footer, preserving the original 2,600-character prefix.
No documentation-only commit or repeat after final GREEN. #7/#102 stay open;
Grep last; user owns merging/tagging/publication. Additional members/duplicates/
range unions/property mixtures/algebra and wider #102/API/consumer/release work
remain separately scoped.


Follow merged266 with the exact two-Any proof; wider optimizer/API/consumer/release work remains open.


## PR267: UTF Any-predicate pair star anchoring

Ordinary Class/no ranges/exactly two AnyScalar predicates (null categories,
zero property values) now qualify for native ALLANY anchoring when the outer
class is positive and either predicate is positive, or when the outer class is
negative and both predicates are negative. Preserve direct/singleton Any,
full-range and all other complement rules. Only IsUtfAllAny changes; class
instructions/work and all traversal/policy/resource boundaries remain fixed.

Pin 305 groups/36,436 native PCRE2 10.48 JIT0 protocols, 15 diagnostic and 34
supplementary cases: 354 new cases/98,091 total. Exact tests-only RED is
86 failures/98,005 passes; freeze all three new C# blobs before production and
preserve 310 prior files/305 C# sources/frozen35. Full native/RED/freeze/author
review records are in the [design](PCRE-UTF-ANY-PAIR-STAR-ANCHOR-DESIGN.md) and
[plan](PCRE-UTF-ANY-PAIR-STAR-ANCHOR-PLAN.md). All seven inventories precede
final qualification. Six native net10 suites/all three library targets/support
gates remain required; final evidence/identities belong on
[PR267](https://github.com/uniblab/Icod.Expressions/pull/267) and issue7 footer.
No documentation-only repeat after final GREEN. #7/#102 stay open; Grep last.


## PR268: bounded UTF Any-only list star anchoring

Continue merged #267 with bounded UTF Any-only list star anchoring. Preserve existing fast paths and admission. The suite adds 455 tests, reaching 98,546; optimizer compatibility and broader delivery work remain open.

See the [design](PCRE-UTF-ANY-LIST-STAR-ANCHOR-DESIGN.md),
[plan](PCRE-UTF-ANY-LIST-STAR-ANCHOR-PLAN.md) and
[PR268](https://github.com/uniblab/Icod.Expressions/pull/268) for native,
frozen-test, review and qualification evidence. #7/#102 stay open; Grep last.


## PR270: positive Any in mixed UTF star classes

Follow merged #269 with bounded positive Any dominance in ordinary UTF
classes containing other properties or ranges. Add 620 tests (99,166 total).
Main269 is green with its dedicated Gnulib host and eight-job completion guard;
broader optimizer, API, release and consumer work remains open.

See the [design](PCRE-UTF-ANY-MIXED-STAR-ANCHOR-DESIGN.md),
[plan](PCRE-UTF-ANY-MIXED-STAR-ANCHOR-PLAN.md) and
[PR270](https://github.com/uniblab/Icod.Expressions/pull/270) for native,
frozen-test, review and qualification evidence. #7/#102 stay open; Grep last.

## PR271: full-range dominance with UTF properties

An ordinary positive UTF class with exactly one retained range0..0x10FFFF
keeps native ALLANY star anchoring when property predicates are also present.
The existing constant-time proof ignores redundant union predicates and
consumes no shared predicate visits, including after the4096-visit budget.
Retained source/negation/range guards, entry/global-AST limits and runtime
work/state/admission remain unchanged. Native579 groups/69,072 protocols,
15 diagnostics and55 supplementary cases add649 tests (99,815 total).
Build net8/net9/net10; execute net10 only on six native hosts.
[Design](PCRE-UTF-FULLRANGE-MIXED-STAR-ANCHOR-DESIGN.md) /
[plan](PCRE-UTF-FULLRANGE-MIXED-STAR-ANCHOR-PLAN.md) /
[PR271](https://github.com/uniblab/Icod.Expressions/pull/271).
#7/#102 remain open; Grep last.
