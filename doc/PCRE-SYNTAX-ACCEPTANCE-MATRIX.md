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
| `^`, `$`, `\A`, `\Z`, `\z`, `\b`, `\B` | Supported subset under current LF or internal AnyCrLf convention; `\G` pending, rejected | Anchor and boundary tests; compare Grep newline and nonzero-offset cases |
| Dot, shorthand `\d\D\s\S\w\W`, simple/control/hex escapes | Supported subset with explicit UTF/UCP and ASCII-`\d` policy | Atom, scalar, space/word/digit tests and Grep consumer fixtures |
| `\h\H\v\V` horizontal/vertical whitespace | Fixed PCRE2 lists, independent of UCP/caseless and newline convention; byte and UTF-8 atoms and ordinary bracket members | `PcreSpaceEscapeReferenceTests`; all 256 bytes, Unicode members/boundaries, captures, CRLF offsets and invalid-UTF barriers |
| Ordinary `[ ]` ranges, negation, shorthand unions, 14 POSIX names | Supported subset in byte and UTF modes; nested/extended class algebra pending, rejected | Bracket and POSIX tests; Unicode table/version reconciliation |
| `\p`/`\P` general categories | Supported subset, including category aliases; scripts, binary and special properties pending, rejected | `PcreUtf8GeneralCategoryTests`; version-pinned property parity |
| `* + ? {n,m}` greedy/lazy and possessive forms | Supported subset, bounded expansion and work; no assertion repetition | Counted-repeat and atomic tests; stress limits against consumer workloads |
| `(?>...)` atomic group | Supported subset; `(*atomic:...)` alias pending, rejected | `PcreAtomicTests` |
| `\1`…`\9`, absolute and signed relative numeric `\gN`/`\g{N}` backreferences | Supported subset within 256 captures; bare multi-digit/octal ambiguity, named and subroutine forms pending, rejected | Backreference and capture tests, `PcreReferenceConformanceTests`; see the absolute and relative `\g` design notes |
| `(?=...)`, `(?!...)`, `(?<=...)`, `(?<!...)` | Fixed literal body only; classes, alternation, captures, variable lengths and PCRE2 aliases pending, rejected | Literal lookaround tests; define general assertion capture and length semantics before extending |
| Leading `i`, `m`, `s` option groups and UTF `aD` toggles | Supported subset across top-level alternatives; scoped/mid-pattern flags and other letters pending, rejected | `PcreLeadingOptionCombinationReferenceTests`; design option scope separately |
| `\Q...\E` literal quoting, isolated `\E` | Supported subset in ordinary patterns, ordinary brackets and fixed literal assertions; UTF-8 scalars and source-byte spans retained | `PcreQuotedLiteralTests`, pinned reference spans; see `PCRE-QUOTED-LITERALS-DESIGN.md` |
| `\K`, `\G`, `\N\R\X\C`, octal/other escapes | Pending, rejected; no literal reinterpretation | Representative compile-diagnostic assertions; prioritize by actual Grep patterns |
| Named and branch-reset captures, `(?#...)`, `(?[...])` extended classes | Pending, rejected | Representative compile-diagnostic assertions; separate capture numbering design |
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
the pinned reference. The most direct gaps are general lookaround/capture
semantics, broader backreferences, option scope and Unicode table parity.
Full workload measurements and command-level Grep comparisons follow before
public API/release review. No CommandFramework ownership change belongs to
this checkpoint.
