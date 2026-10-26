# In-house PCRE design for the Grep migration

**Status:** proposed, tracked by [issue #7](https://github.com/uniblab/Icod.Expressions/issues/7). The existing `Icod.Expressions` GNU BRE/ERE alpha and Grep's PCRE.NET path remain production implementations during this work.

## Intent and constraints

Build a managed Perl-compatible matcher owned by `Icod.Expressions`, then migrate `Icod.Grep -P` only after conformance, resource, packaging, and performance gates pass. Grep currently uses PCRE.NET 1.6.0 / PCRE2 10.48 on authoritative record bytes; it also uses CommandFramework's GNU regex and locale contracts for other modes. The new engine must not silently change `-P` results, byte offsets, error statuses, or platform text translation. It must not use PCRE.NET or `System.Text.RegularExpressions` at runtime.

Keep the GNU/POSIX engine independent. Its documented leftmost-longest result selection differs from the standard PCRE2 matcher's first successful path at the earliest start. PCRE2's alternative DFA matching algorithm lacks capture and backreference support and is not Perl compatible. The new path therefore needs ordered backtracking and capture state rather than a syntax extension to the GNU parser. [PCRE2 matching algorithms](https://www.pcre.org/current/doc/html/pcre2matching.html) describe this distinction.

This is an alpha design. The current package's legacy namespaces and any experimental PCRE API are not commitments to a stable shape; later work may refactor more than namespaces.

## Internal byte foundation status

The internal foundation recognizes raw byte literals (including NUL and high bytes), ordered `|`, capturing and noncapturing parentheses, `^` and `$` at whole-input boundaries, and greedy/lazy `*`, `+`, and `?`. Stacked syntax slices add dot, selected literal/control/hex escapes, non-UCP ASCII `\d`, `\s`, `\w` with complements, ordinary byte brackets with fourteen ASCII POSIX class names, and single-digit numbered backreferences. Numbered capture spans include group zero. Compile-time diagnostics reject malformed groups, quantifiers and classes. Backtracking has per-match work and saved-path limits with distinct diagnostics, and cancellation can interrupt it. This internal subset is not a public PCRE feature claim.

| Syntax or mode | Status for the draft byte foundation |
|---|---|
| Raw bytes, `|`, capturing groups, anchors, `*`, `+`, `?` and their lazy forms | Implemented for the internal API. |
| Dot excluding LF; `\a`, `\e`, `\f`, `\n`, `\r`, `\t`, `\x` (one or two hex digits), escaped punctuation; ASCII `\d`, `\s`, `\w` and complements | Implemented in the stacked byte-atoms PR for 8-bit non-UTF, non-UCP mode. |
| Ordinary bracket ranges, negation, escaped byte items, shorthand unions and fourteen non-UCP ASCII POSIX names | Implemented in the stacked byte-classes PR for 8-bit non-UTF mode. |
| Single-digit numbered byte backreferences | Implemented for 8-bit non-UTF mode; caseful by default, ASCII caseless with `(?i)` or the internal compile option. Multi-digit and named forms remain pending. |
| `(?:...)` noncapturing groups | Implemented for 8-bit non-UTF mode; nested captures retain their numbers. |
| Fixed literal `(?=...)`, `(?!...)`, `(?<=...)`, `(?<!...)` over authoritative bytes | Implemented internally in byte and UTF-8 modes; zero-width, with no capture allocation and work/cancellation accounting. UTF-8 escapes become encoded scalars and assertions cannot inspect across invalid-subject fragment barriers. Nonliteral assertion bodies remain unsupported. |
| `\A`, `\Z`, `\z` subject anchors | Implemented internally with LF as the final-newline convention for `\Z`; `\A` and `\z` remain strict subject edges. |
| Default `^` and `$` | `^` tests absolute start; `$` tests absolute end or before one final LF. Other option-dependent behavior remains pending. |
| ASCII byte-mode `\b` and `\B` word boundaries | Implemented internally using the same non-UCP ASCII `\w` definition. Bracket `\b` remains backspace; bracket `\B` is unsupported. |
| One leading `(?s)` | Implemented internally for 8-bit byte mode; dot matches LF in every top-level alternative. Scoped and mid-pattern flags remain unsupported. |
| One leading `(?i)` | Implemented internally for 8-bit non-UTF mode with ASCII letter folding for literals, brackets, literal lookarounds, and backreferences; high bytes remain exact. Other placements and flag combinations remain unsupported. |
| One leading `(?m)` | Implemented internally with LF line anchors: `^` matches subject start and after internal LF; `$` matches before any LF or at subject end. Absolute subject escapes remain unchanged. Scoped and combined flags remain unsupported. |
| Internal `PcreByteOptions.Caseless` | Caller-supplied ASCII caseless compile option uses the same byte behavior as leading `(?i)`; a leading `(?s)` preserves it. Grep `-i` integration remains pending. |
| Internal `PcreByteOptions.Utf8` | Opt-in strict UTF-8 scalar literals and dot preserve source-byte spans, with invalid pattern/subject diagnostics and character-boundary search offsets. Ordinary case-sensitive literal bracket sets/ranges and standalone POSIX digit/word/space brackets are implemented; mixed UTF brackets, caseless and nonliteral lookarounds remain explicitly unsupported. |
| Internal `PcreByteOptions.MatchInvalidUtf` | Opt-in PCRE2-style valid UTF-8 fragments in malformed subjects, implying UTF-8 and preserving whole-source byte spans. Consuming atoms and backreferences cannot cross a malformed sequence; fragment edges govern word boundaries and `\\z`/`\\Z` but not line anchors. Pinned PCRE2 10.48 exhaustive offset conformance remains pending. |
| UTF-8 `\p{L}` and `\P{L}` | Unicode general-category letters and their complement, including `\p{^L}`, `\P{^L}` and unbraced `\pL` / `\PL`, are implemented internally with original byte spans. The Unicode category data comes from the target .NET runtime; version reconciliation remains a conformance gate before Grep migration. Properties beyond L/Nd and UTF bracket unions with POSIX/shorthand/property predicates remain unsupported. |
| UTF-8 UCP digit policy | Internal `Ucp` and `AsciiBsD` options distinguish ASCII `\d` from Unicode POSIX `[[:digit:]]` and explicit `\p{Nd}`. One leading `(?aD)` or `(?-aD)` selects the `\d` restriction. Standalone UTF POSIX digit/word/space and ordinary literal scalar brackets are recognized; mixed class unions, scoped options and Unicode-version parity remain pending. |
| UTF-8 UCP word behavior | Scalar `\w`/`\W` use Unicode L/N/Mn/Pc under UCP and ASCII word membership without it; `\b`/`\B` compare adjacent UTF scalars under UCP, while non-UCP retains ASCII boundaries. UTF bracket unions remain pending. |
| UTF-8 UCP whitespace | Scalar `\s`/`\S` use Unicode separators plus HT/LF/VT/FF/CR, U+0085 and U+180E under UCP; without UCP they use the six ASCII whitespace characters. Standalone POSIX space/word brackets share those scalar and word predicates. |
| Braced hex, other escapes, `{m,n}`, remaining inline options and other special groups | Explicit compile-time `UnsupportedSyntax` diagnostic. |
| Broader UCP and UTF-8 class unions/case-folding, pinned invalid-UTF offset conformance, broader backreference syntax, general lookaround, atomic groups, possessive repetition, backtracking verbs, recursion and broader PCRE2 syntax | Pending separate implementation and conformance fixtures. |

The initial parser limits group nesting, total captures, and repeat-state marks to 256 each. The matcher executes ordered bytecode with an explicit stack bounded at 1,024 saved paths and a work budget of 1,000,000 steps by default; long linear matches do not spend stack slots. Empty iterations proceed to the continuation once while retaining captures and alternative paths. These limits are provisional and require stress and performance review before a consumer switch.

## Approaches

| Approach | Tradeoff | Decision |
|---|---|---|
| Extend the GNU BRE/ERE parser and matcher | Shares code but risks changing its POSIX selection, captures, and byte behavior; PCRE lookarounds/backtracking need a separate execution model. | Reject. |
| Wrap PCRE.NET or .NET Regex behind an Expressions API | Fastest integration, but does not meet the in-house engine goal and retains native/runtime dependency or divergent semantics. | Reject as production implementation; use reference engines only for isolated oracle fixtures. |
| Build a separate parser and ordered matcher over source bytes | Allows explicit Perl semantics, offset tracking, and resource controls without disturbing shipped GNU contracts; needs sustained conformance and optimization work. | Choose. |

## Boundaries

- Put PCRE implementation under `src/Pcre/` and tests under `tests/Pcre/`, matching the repository's root project, `/src`, `/tests`, `/samples` layout. Keep the first compiler/matcher API internal and friend-accessible to tests; design the public package surface only after the consumer contract is characterized.
- Compile into immutable instructions or nodes and copy the caller's pattern bytes before retaining them. Matching state owns the input position, capture spans, option scope, and a bounded backtracking stack. Try candidate start positions in byte order; at one start, explore alternatives and greedy/lazy choices in syntax order. Captures roll back with abandoned paths. An atomic group discards its internal alternatives at the boundary.
- Preserve the original byte buffer and report source-byte start/length for match and captures. C/POSIX consumes bytes as 8-bit units. UTF-8 mode decodes valid scalar values while retaining byte boundaries; malformed sequences follow the existing Grep `MatchInvalidUtf` policy. Do not turn a malformed byte into a lossy replacement character or move a start offset into a continuation byte.
- Make cancellation and work/depth limits deterministic from the first internal milestone. The compiled pattern owns an immutable default match-work budget of 1,000,000 attempted candidate/node steps (positive overrides may be supplied at compile time); each new match starts a fresh budget. This is an internal provisional default, to be measured and revised before Grep consumes the engine. Reject unsupported syntax at compile time with a position and diagnostic; do not reinterpret it as a literal or silently fall back to another engine. Match-limit failure is distinct from no match.
- Keep Grep's record framing, CRLF translation, binary-content policy, color, `-o`, `-w`, `-x`, diagnostics and output formatting in Grep. The new library returns a match and captures, not formatted output.

## Grep compatibility matrix

| Current Grep behavior | Fixture/gate |
|---|---|
| `(?<=foo)bar`, `^(\w+)\s+\1$`, ordered alternatives and captures | `tests/Grep.Tests/src/PcreTests.cs`, plus engine first-path/capture rollback cases. |
| UTF-8/UCP `\p{L}`, POSIX `[[:digit:]]`, ASCII-only `\d`, `(?-aD)` override | C.UTF-8 character-class fixtures and the current Grep `KeepsBackslashDAsciiUnderUtf8Ucp` case. |
| C locale `\xFF`, embedded NUL pattern, UTF-8 invalid byte matching | Exact byte fixtures with source-byte offsets and lengths. |
| `(?s)` with `-z`, caseless lookbehind, `-w`, `-x`, `-o`, color | Existing Grep command tests and matched-span fixtures. |
| Bad pattern and resource failure | Controlled compile/match diagnostics; Grep status 2. |
| Windows default text translation and raw `-U` | Existing Grep edge tests and six RID package/archive smoke. |
| 32 independent patterns, dense `-o`, long records | Existing Grep benchmark and stress scenarios, compared to its 1.6.0 JIT baseline. |

The matrix is the first consumer floor, not the complete PCRE2 syntax promise. Expand a tracked feature table from the [PCRE2 syntax reference](https://www.pcre.org/current/doc/html/pcre2syntax.html), and distinguish supported, intentionally rejected, and pending constructs. In particular, lookbehind length rules, option scope, backtracking verbs, recursion, and invalid UTF behavior need explicit decisions. PCRE2 documents invalid UTF matching as a compile option specific to the standard matcher in its [API reference](https://www.pcre.org/current/doc/html/pcre2api.html).

## Delivery and exit gates

1. Freeze Grep and reference-engine input/output fixtures, including malformed byte and cancellation/resource cases. Record an explicit syntax coverage matrix.
2. Implement the internal parser and ordered byte matcher in small, independently tested steps: literal/alternation/anchors, groups and quantifiers, lookarounds/backreferences, classes/options/Unicode, and limits. Keep unknown constructs as compile errors.
3. Run the Expressions net8/net9/net10 suite on Linux, Windows and macOS. Verify one library package and a package-only consumer; no native PCRE dependency may appear in that package.
4. Compare Grep results and its documented benchmark/stress workloads with the existing PCRE2 path. Decide measured performance thresholds from the baseline before switching production. Do not claim parity from a small hand-picked subset.
5. After every other known consumer has migrated, use a separate Grep PR to migrate both BRE/ERE and `-P` to the validated Expressions package, run its cross-platform tests and all six RID archive/package smokes, then remove PCRE.NET. Only after Grep and every other known consumer is migrated and validated may a separate reviewed CommandFramework ownership change remove/forward its regex code.

Do not publish a PCRE-capable package version or switch Grep based solely on an internal milestone. Version and public API decisions belong to the conformance release review.
