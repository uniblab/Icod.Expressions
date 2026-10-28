# In-house PCRE design for the Grep migration

**Status:** proposed, tracked by [issue #7](https://github.com/uniblab/Icod.Expressions/issues/7). The existing `Icod.Expressions` GNU BRE/ERE alpha and Grep's PCRE.NET path remain production implementations during this work.

## Intent and constraints

Build a managed Perl-compatible matcher owned by `Icod.Expressions`, then migrate `Icod.Grep -P` only after conformance, resource, packaging, and performance gates pass. Grep currently uses PCRE.NET 1.6.0 / PCRE2 10.48 on authoritative record bytes; it also uses CommandFramework's GNU regex and locale contracts for other modes. The new engine must not silently change `-P` results, byte offsets, error statuses, or platform text translation. It must not use PCRE.NET or `System.Text.RegularExpressions` at runtime.

Keep the GNU/POSIX engine independent. Its documented leftmost-longest result selection differs from the standard PCRE2 matcher's first successful path at the earliest start. PCRE2's alternative DFA matching algorithm lacks capture and backreference support and is not Perl compatible. The new path therefore needs ordered backtracking and capture state rather than a syntax extension to the GNU parser. [PCRE2 matching algorithms](https://www.pcre.org/current/doc/html/pcre2matching.html) describe this distinction.

This is an alpha design. The current package's legacy namespaces and any experimental PCRE API are not commitments to a stable shape; later work may refactor more than namespaces.

## Internal byte foundation status

The internal foundation recognizes raw byte literals (including NUL and high bytes), ordered `|`, capturing and noncapturing parentheses, `^` and `$` at whole-input boundaries, and greedy/lazy `*`, `+`, and `?`. Stacked syntax slices add dot, selected literal/control/hex escapes, non-UCP ASCII `\d`, `\s`, `\w` with complements, ordinary byte brackets with fourteen ASCII POSIX class names, single-digit numbered backreferences, and absolute or signed relative numeric `\g` references within the 256-capture ceiling. Numbered capture spans include group zero. Compile-time diagnostics reject malformed groups, quantifiers and classes. Backtracking has per-match work and saved-path limits with distinct diagnostics, and cancellation can interrupt it. This internal subset is not a public PCRE feature claim.

The internal byte VM journals prior capture, capture-start, and repetition-mark values while saved paths exist. Split frames retain byte offsets and journal checkpoints instead of cloning all capture and mark arrays. Atomic pruning discards inner paths while preserving outer fallbacks. A warmed allocation regression guards this internal improvement; Grep's documented correctness and performance workloads still require separate comparison before consumer migration.

| Syntax or mode | Status for the draft byte foundation |
|---|---|
| Raw bytes, `|`, capturing groups, anchors, `*`, `+`, `?` and their lazy forms | Implemented for the internal API. |
| Dot excluding LF; `\a`, `\e`, `\f`, `\n`, `\r`, `\t`, `\x` (one or two hex digits), escaped punctuation; ASCII `\d`, `\s`, `\w` and complements | Implemented in the stacked byte-atoms PR for 8-bit non-UTF, non-UCP mode. |
| Ordinary bracket ranges, negation, escaped byte items, shorthand unions and fourteen non-UCP ASCII POSIX names | Implemented in the stacked byte-classes PR for 8-bit non-UTF mode. |
| Single-digit and numeric `\gN`/`\g{N}` backreferences | Absolute and signed relative forms resolve to groups 1–256 in 8-bit non-UTF and UTF-8 modes; caseful by default, ASCII byte folding or provisional Unicode scalar folding with `(?i)` or the internal compile option. Captures keep source-byte spans, and UTF references consume subject scalar widths. Bare multi-digit/octal ambiguity and named forms remain pending. See `PCRE-ABSOLUTE-G-REFERENCE-DESIGN.md` and `PCRE-RELATIVE-G-REFERENCE-DESIGN.md`. |
| `(?:...)` noncapturing groups | Implemented for 8-bit non-UTF mode; nested captures retain their numbers. |
| Fixed literal `(?=...)`, `(?!...)`, `(?<=...)`, `(?<!...)` over authoritative bytes | Implemented internally in byte and UTF-8 modes; zero-width, with no capture allocation and work/cancellation accounting. UTF-8 caseless assertions compare scalars and lookbehind counts scalar width; assertions cannot inspect across invalid-subject fragment barriers. Nonliteral assertion bodies remain unsupported. |
| `\A`, `\Z`, `\z` subject anchors | Implemented internally with LF as the final-newline convention for `\Z`; `\A` and `\z` remain strict subject edges. |
| Default `^` and `$` | `^` tests absolute start; `$` tests absolute end or before one final LF. Other option-dependent behavior remains pending. |
| ASCII byte-mode `\b` and `\B` word boundaries | Implemented internally using the same non-UCP ASCII `\w` definition. Bracket `\b` remains backspace; bracket `\B` is unsupported. |
| Leading `i`, `m`, `s` option groups | Combinations such as `(?ims-s)`, unsetting with `(?-i)`, and repeated leading groups are supported in byte and UTF-8 modes. Caseless folds ASCII byte literals, brackets, literal lookarounds and backreferences in byte mode and provisionally folds Unicode scalars in UTF mode, including `Lu`/`Ll`/`Lt` properties. DotAll permits newline under the current convention; multiline gives `^` and `$` line boundaries. The separate UTF-only `(?aD)` and `(?-aD)` groups may appear among these prefixes. Mid-pattern, scoped, other option letters and mixed `aD` in one group remain unsupported. |
| Internal `PcreByteOptions.Caseless` | Caller-supplied caseless compile option uses ASCII byte behavior or provisional Unicode scalar behavior for UTF literals, properties, backreferences and fixed literal assertions; leading options preserve or explicitly unset it. Grep `-i` integration remains pending. |
| Internal `PcreByteOptions.Utf8` | Opt-in strict UTF-8 scalar literals and dot preserve source-byte spans, with invalid pattern/subject diagnostics and character-boundary search offsets. Ordinary UTF brackets combine literal ranges, supported shorthands/properties and all fourteen POSIX predicates in caseful or provisional caseless mode. Caseless ranges are compiled to folded intervals; wide ranges can add compilation work. Nonliteral lookarounds remain explicitly unsupported. |
| Internal `PcreByteOptions.MatchInvalidUtf` | Opt-in PCRE2-style valid UTF-8 fragments in malformed subjects, implying UTF-8 and preserving whole-source byte spans. Consuming atoms and backreferences cannot cross a malformed sequence; fragment edges govern word boundaries while `\\z`/`\\Z` require the whole-subject end. Pinned PCRE2 10.48 exhaustive offset conformance remains pending. |
| UTF-8 `\p`/`\P` general categories | The abbreviated C/L/M/N/P/S/Z groups and two-letter Unicode general categories (including `Lc`/`L&`) work as scalar atoms and bracket members, with complements and original byte spans. Loose name matching and `gc:`/`General_Category=` forms are supported. Category data comes from the target .NET runtime; version reconciliation remains a conformance gate before Grep migration. Script, binary and other properties remain unsupported. |
| UTF-8 UCP digit policy | Internal `Ucp` and `AsciiBsD` options distinguish ASCII `\d` from Unicode POSIX `[[:digit:]]` and explicit `\p{Nd}`. One leading `(?aD)` or `(?-aD)` selects the `\d` restriction. UTF brackets combine these predicates with literal scalar ranges; scoped POSIX options and Unicode-version parity remain pending. |
| UTF-8 UCP word behavior | Scalar `\w`/`\W` use Unicode L/N/Mn/Pc under UCP and ASCII word membership without it; `\b`/`\B` compare adjacent UTF scalars under UCP, while non-UCP retains ASCII boundaries. Bracket unions follow the same scalar predicates. |
| UTF-8 UCP whitespace | Scalar `\s`/`\S` use Unicode separators plus HT/LF/VT/FF/CR, U+0085 and U+180E under UCP; without UCP they use the six ASCII whitespace characters. Standalone POSIX space/word brackets share those scalar and word predicates. |
| `\h`, `\H`, `\v`, `\V` whitespace classes | Fixed PCRE2 horizontal/vertical lists and complements work as byte and UTF-8 atoms and bracket members, independently of UCP, caseless and newline convention. Byte mode includes NBSP in `\h` and NEL in `\v`; UTF mode consumes whole scalars with original byte spans. `\v` matches one character. See `PCRE-SPACE-ESCAPES-DESIGN.md`. |
| `\R` newline sequence | Internally lowered to an atomic CRLF-first alternation plus single-character line endings. Byte mode includes LF/VT/FF/CR/NEL; UTF adds LS/PS. Internal `BsrAnyCrLf` restricts it to CRLF/CR/LF independently of the newline convention; pattern-side BSR directives remain pending. Invalid inside brackets, and general assertion bodies remain unsupported. See `PCRE-NEWLINE-SEQUENCES-DESIGN.md`. |
| Counted `{n}`, `{n,m}`, `{n,}`, `{,m}` repetitions | Implemented internally on supported atoms and groups with ordered greedy/lazy backtracking. Numeric bounds are at most 65535; compilation is capped at 4096 instructions and 4096 node visits, and repeat state at 256 marks, with a distinct compile diagnostic. |
| Braced `\x{...}` hexadecimal values | Implemented internally for byte and UTF-8 atoms, ordinary bracket members/ranges and fixed literal assertions. Byte values are at most 0xff; UTF values must be Unicode scalars. |
| `\N` and UTF `\N{U+...}` | Bare non-newline matching shares dot's LF/AnyCRLF convention with DotAll forced off. Counted forms such as `\N{2,3}` use ordinary repetition; only the `U+` prefix selects a codepoint literal. UTF codepoint forms share braced-hex validation, byte spans, brackets/ranges and fixed literal assertions; bare `\N` is invalid in brackets and nonliteral lookaround remains pending. See `PCRE-NON-NEWLINE-DESIGN.md`. |
| Atomic `(?>...)` and possessive `*+`, `++`, `?+`, `{n,m}+` | Implemented internally as scoped pruning of backtracking choices. Nested groups retain captures, and preceding alternatives can still backtrack. Broader atomic aliases and performance conformance remain pending. |
| `\Q...\E` literal quoting | Implemented internally for ordinary patterns, ordinary brackets and fixed literal assertions, including isolated `\E`, unterminated quotes through pattern end, and UTF-8 scalar literals. A quoted hyphen in a bracket is a member; quoted characters can end a range if its hyphen lies outside the quote. See `PCRE-QUOTED-LITERALS-DESIGN.md`. |
| Other escapes, remaining inline options and other special groups | Explicit compile-time `UnsupportedSyntax` diagnostic. |
| Script, binary and special UCP property names, scoped POSIX options and full Unicode fold-table conformance, pinned invalid-UTF offset conformance, broader backreference syntax, general lookaround, atomic aliases, backtracking verbs, recursion and broader PCRE2 syntax | Pending separate implementation and conformance fixtures. |

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

[`PCRE-SYNTAX-ACCEPTANCE-MATRIX.md`](PCRE-SYNTAX-ACCEPTANCE-MATRIX.md) records the current supported shapes, representative explicitly rejected PCRE2 families, consumer-owned behavior and the next conformance gates. A representative `(*...)` directive is now diagnosed as pending syntax at its opening byte rather than as a quantifier error; malformed groups remain invalid syntax. This inventory is a checkpoint for prioritizing issue #7, not a complete PCRE2 implementation claim.

[`PCRE-RESOURCE-ERROR-CONTRACT.md`](PCRE-RESOURCE-ERROR-CONTRACT.md) records the internal diagnostic and limit outcomes against Grep's observed error and cancellation behavior. The saved-path boundary is pinned in `PcreResourceContractTests`; numeric limits remain provisional and Grep's future adapter still needs command-level resource fixtures before migration.

`tests/Pcre/PcreReferenceConformanceTests.cs` compares a finite supported-syntax matrix against Grep's pinned PCRE.NET 1.6.0 reference on original byte inputs. This is a test-only dependency and whole-match span gate; it does not establish full PCRE2 10.48 compatibility or add a production dependency.

`tests/Pcre/PcreReferenceCaptureTests.cs` checks selected numbered group participation and source-byte spans against the same pinned reference. Unset and participating empty captures are distinct. This finite test-only matrix is another conformance gate, not a complete capture or syntax claim.

`doc/PCRE-GREP-CONSUMER-FIXTURES.md` maps every current Grep `PcreTests.cs` case to an engine byte fixture or a consumer-owned behavior. `tests/Pcre/PcreGrepConsumerFixtureTests.cs` applies Grep's C/POSIX and UTF-8 options to the pinned reference and the internal matcher on the same bytes, and explicitly rejects selected pending syntax. The inventory also names the Grep workloads that still need correctness and performance comparison.

`tools/PcreWorkloadComparison` reproduces the seven pinned Grep PCRE benchmark inputs at the matcher-component level. Its `--smoke` mode verifies byte spans and selection on bounded input in Linux x64 Staging CI; `--measure` verifies full inputs and prints alternating-round timings and allocations for a controlled reference host. The tool does not time Grep's command path, and these measurements cannot be compared directly to published 1.6.0 command timings. See `doc/PCRE-GREP-WORKLOAD-PROBE-DESIGN.md` for provenance and remaining gates.

## Delivery and exit gates

1. Freeze Grep and reference-engine input/output fixtures, including malformed byte and cancellation/resource cases. Record an explicit syntax coverage matrix.
2. Implement the internal parser and ordered byte matcher in small, independently tested steps: literal/alternation/anchors, groups and quantifiers, lookarounds/backreferences, classes/options/Unicode, and limits. Keep unknown constructs as compile errors.
3. Run the Expressions net8/net9/net10 suite on Linux, Windows and macOS. Verify one library package and a package-only consumer; no native PCRE dependency may appear in that package.
4. Compare Grep results and its documented benchmark/stress workloads with the existing PCRE2 path. Decide measured performance thresholds from the baseline before switching production. Do not claim parity from a small hand-picked subset.
5. After every other known consumer has migrated, use a separate Grep PR to migrate both BRE/ERE and `-P` to the validated Expressions package, run its cross-platform tests and all six RID archive/package smokes, then remove PCRE.NET. Only after Grep and every other known consumer is migrated and validated may a separate reviewed CommandFramework ownership change remove/forward its regex code.

Do not publish a PCRE-capable package version or switch Grep based solely on an internal milestone. Version and public API decisions belong to the conformance release review.
