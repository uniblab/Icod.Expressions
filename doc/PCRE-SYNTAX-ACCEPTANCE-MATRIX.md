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
| `^`, `$`, `\A`, `\Z`, `\z`, `\b`, `\B`, `\G` | Supported subset under current LF or internal AnyCrLf convention; `\G` tests the original requested match offset | Anchor and boundary tests; `PcreMatchPositionReferenceTests` pins nonzero offsets and alternatives |
| `\K` reported match start | Resets group zero and whole-match spans; numbered captures retained, reset state journaled on fallback; invalid in brackets/lookarounds or with direct quantifiers | `PcreMatchPositionReferenceTests`; branch/atomic/repetition rollback, byte spans and empty paths |
| Dot, shorthand `\d\D\s\S\w\W`, simple/control/hex escapes | Supported subset with explicit UTF/UCP and ASCII-`\d` policy | Atom, scalar, space/word/digit tests and Grep consumer fixtures |
| `\h\H\v\V` horizontal/vertical whitespace | Fixed PCRE2 lists, independent of UCP/caseless and newline convention; byte and UTF-8 atoms and ordinary bracket members | `PcreSpaceEscapeReferenceTests`; all 256 bytes, Unicode members/boundaries, captures, CRLF offsets and invalid-UTF barriers |
| `\R` newline sequence | Atomic CRLF-first matching; Unicode line endings by default, internal `BsrAnyCrLf` restriction independent of newline convention; invalid in brackets | `PcreNewlineSequenceReferenceTests`; membership, captures/repetition, every CRLF/invalid-UTF offset, work and expansion limits |
| `\N` non-newline, UTF `\N{U+...}` literal | Bare form obeys LF/AnyCRLF and ignores DotAll; counted repetitions supported; Unicode codepoint form shares braced-hex literal/range/fixed-assertion rules | `PcreNonNewlineReferenceTests`; byte/scalar membership, braced-form disambiguation, captures, offsets and diagnostics |
| Bare and braced octal literals | Default up-to-three-digit octals and `\o{...}` in byte/UTF atoms, brackets/ranges and fixed literal assertions; byte/scalar range checks, brace-edge space/tab | `PcreNumericEscapeReferenceTests`; all 256 byte values, UTF bounds, quoting, offsets and diagnostics; extra Python/NO_BS0 modes pending |
| Ordinary `[ ]` ranges, negation, shorthand unions, 14 POSIX names | Supported subset in byte and UTF modes; nested/extended class algebra pending, rejected | Bracket and POSIX tests; Unicode table/version reconciliation |
| `\p`/`\P` general categories | Supported subset, including category aliases; scripts, binary and special properties pending, rejected | `PcreUtf8GeneralCategoryTests`; version-pinned property parity |
| `* + ? {n,m}` greedy/lazy and possessive forms | Supported subset, bounded expansion and work; direct lookaround repeats use finite PCRE2 bounds (unlimited maximum becomes minimum plus one) | Counted-repeat, atomic and `PcreAssertionRepeatReferenceTests`; capture growth, optional rollback, zero counts, UTF offsets and resource limits; stress limits against consumer workloads |
| `(?>...)` atomic group | Supported subset; `(*atomic:...)` alias pending, rejected | `PcreAtomicTests` |
| Bare decimal, absolute and signed relative numeric `\gN`/`\g{N}` backreferences | Default reference/octal disambiguation within 256 captures; braced g accepts edge space/tab. Subroutine forms remain pending, rejected | Backreference/capture tests, `PcreNumericEscapeReferenceTests`; current-branch numbering, leading-8/9 forward references and entire decimal runs |
| Named captures and backreferences | Three unique declaration forms, five reference forms; compile-time aliases to numbered slots, forward references, case-sensitive names and a 128-byte name limit. UTF letters/digits use runtime categories. Duplicate-name options, subroutines and bracket-literal `\g`/`\k` remain pending | `PcreNamedCaptureReferenceTests`; all captures, rollback, UTF/invalid-UTF offsets, name grammar and boundaries; see `PCRE-NAMED-CAPTURES-DESIGN.md` |
| `(?|...)` branch-reset captures | Parser-scoped numbering, highest-branch continuation, same-number name aliases and ordinary numbered capture/journal semantics | `PcreBranchResetReferenceTests`; unequal widths, nesting, relative references, aliases, repeated captures, invalid-UTF offsets and resource boundaries |
| `(?=...)`, `(?!...)` atomic lookahead | General bodies over existing grammar, options, numbered/named captures, alternatives/repeats and nested assertions. Inline bytecode shares guards/journal; first success commits positive captures, negatives discard body state; original caller/fragment retained. Direct quantifiers use finite bounds with ordinary greedy/lazy/U and possessive choices; aliases remain pending | `PcreGeneralLookaheadReferenceTests`; capture transactions, spans beyond G0, UTF offsets, 256 depth and guard-inclusive saved-path/work limits |
| `(?<=...)`, `(?<!...)` lookbehind | Fixed literal body, including direct quantifiers; general bodies/length rules and aliases pending, rejected | Existing literal lookaround and explicit diagnostic controls |
| Leading/mid-pattern/scoped `i`, `m`, `n`, `s`, `U`, `x`/`xx`; leading UTF `aD` toggles | Compile-time branch propagation, local casing and group restoration. n suppresses unnamed slots; named groups still capture. U reverses ordinary/question-suffix repetition preference; possessive repeats stay greedy. x/xx ignore source trivia at token boundaries, with bracket-only SP/HT in xx; explicit x downgrades xx. Empty/reset/J/extra flags and scoped/mixed aD remain pending | `PcreOptionScopeReferenceTests`, `PcreCaptureRepeatOptionReferenceTests`, `PcreExtendedOptionReferenceTests`, `PcreLeadingOptionCombinationReferenceTests`; numbering, references, all quantifier families, UTF, limits/work and invalid-UTF/CRLF offsets |
| `\Q...\E` literal quoting, isolated `\E` | Supported subset in ordinary patterns, ordinary brackets and fixed literal assertions; UTF-8 scalars and source-byte spans retained | `PcreQuotedLiteralTests`, pinned reference spans; see `PCRE-QUOTED-LITERALS-DESIGN.md` |
| `\X\C`, other escapes | Pending, rejected; no literal reinterpretation | Representative compile-diagnostic assertions; prioritize by actual Grep patterns |
| `(?#...)` and x/xx line comments | Supported at token boundaries, including quantifier/suffix joins and fixed assertion bodies. Quotes/escapes and ordinary classes retain literal text; LF/AnyCRLF govern line termination | `PcreExtendedOptionReferenceTests`; original byte diagnostics and pinned whitespace/range rules |
| `(?[...])` extended classes | Pending, rejected | Representative compile-diagnostic assertions; separate class algebra design |
| Conditionals, recursion/subroutine references, callouts | Pending, rejected | Representative compile-diagnostic assertions; evaluate consumer need and bounded resource design |
| `(*...)` directives, newline/BSR settings, control verbs and assertion/atomic aliases | Pending, rejected at opening `(`; current internal newline choice is a compile option | Representative compile-diagnostic assertions; decide which directives Grep needs |
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
the most direct remaining gaps include broader lookbehind/length semantics,
other references/options and Unicode table parity.
Full workload measurements and command-level Grep comparisons follow before
public API/release review. No CommandFramework ownership change belongs to
this checkpoint.
