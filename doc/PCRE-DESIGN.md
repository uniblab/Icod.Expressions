# In-house PCRE design for the Grep migration

**Status:** proposed, tracked by [issue #7](https://github.com/uniblab/Icod.Expressions/issues/7). The existing `Icod.Expressions` GNU BRE/ERE alpha and Grep's PCRE.NET path remain production implementations during this work.

## Intent and constraints

Build a managed Perl-compatible matcher owned by `Icod.Expressions`, then migrate `Icod.Grep -P` only after conformance, resource, packaging, and performance gates pass. Grep currently uses PCRE.NET 1.6.0 / PCRE2 10.48 on authoritative record bytes; it also uses CommandFramework's GNU regex and locale contracts for other modes. The new engine must not silently change `-P` results, byte offsets, error statuses, or platform text translation. It must not use PCRE.NET or `System.Text.RegularExpressions` at runtime.

Keep the GNU/POSIX engine independent. Its documented leftmost-longest result selection differs from the standard PCRE2 matcher's first successful path at the earliest start. PCRE2's alternative DFA matching algorithm lacks capture and backreference support and is not Perl compatible. The new path therefore needs ordered backtracking and capture state rather than a syntax extension to the GNU parser. [PCRE2 matching algorithms](https://www.pcre.org/current/doc/html/pcre2matching.html) describe this distinction.

This is an alpha design. The current package's legacy namespaces and any experimental PCRE API are not commitments to a stable shape; later work may refactor more than namespaces.

## Internal byte foundation status

The internal foundation recognizes raw byte literals (including NUL and high bytes), ordered `|`, capturing and noncapturing parentheses, `^` and `$` at whole-input boundaries, and greedy/lazy `*`, `+`, and `?`. Stacked syntax slices add dot, selected literal/control/hex escapes, non-UCP ASCII `\d`, `\s`, `\w` with complements, ordinary byte brackets with fourteen ASCII POSIX class names, single-digit numbered backreferences, and absolute or signed relative numeric `\g` references within the 256-capture ceiling. Numbered capture spans include group zero. Compile-time diagnostics reject malformed groups, quantifiers and classes. Backtracking has per-match work and saved-path limits with distinct diagnostics, and cancellation can interrupt it. This internal subset is not a public PCRE feature claim.

The internal byte VM journals prior capture, capture-start, and repetition-mark values while saved paths exist. Split and assertion-guard frames retain byte offsets, journal checkpoints and scope depths instead of cloning capture/mark arrays. General lookahead uses inline bytecode and typed guards on the same bounded stack: positive success keeps captures atomically, while negative paths restore body state. Atomic pruning discards inner paths while preserving outer fallbacks. A warmed allocation regression guards this internal improvement; Grep's documented correctness and performance workloads still require separate comparison before consumer migration.

| Syntax or mode | Status for the draft byte foundation |
|---|---|
| Raw bytes, `|`, capturing groups, anchors, `*`, `+`, `?` and their lazy forms | Implemented for the internal API. |
| Dot excluding LF; `\a`, `\e`, `\f`, `\n`, `\r`, `\t`, `\x` (one or two hex digits), escaped punctuation; ASCII `\d`, `\s`, `\w` and complements | Implemented in the stacked byte-atoms PR for 8-bit non-UTF, non-UCP mode. |
| Start directives `(*LF)`, `(*ANYCRLF)`, `(*BSR_UNICODE)`, `(*BSR_ANYCRLF)` | Exact consecutive byte0 prefix with independent last-wins global newline/BSR override; original bytes and existing compile/matcher paths retained. `PcreStartDirectiveReferenceTests`; see `PCRE-START-DIRECTIVES-DESIGN.md`. |
| Start directives `(*UTF)`, `(*UTF8)`, `(*UCP)` | Enable existing UTF/UCP modes before validation through immutable compile settings shared with newline/BSR. Original source retained; Byte UCP is supported independently of UTF. `PcreUtfStartDirectiveReferenceTests`; see `PCRE-UTF-START-DIRECTIVES-DESIGN.md`. |
| No-argument `(*FAIL)` / `(*F)` | Failure leaf/opcode use shared backtracking; direct repeats invalid. Native branch width stops at failure, with width-tail nested-behind suppression and full global validation retained. `PcreFailureVerbReferenceTests`; see `PCRE-FAILURE-VERBS-DESIGN.md`. Named/other verbs remain pending. |
| Ordinary bracket `\g`/`\k` | Literal g/k only without consuming suffixes; existing byte/UTF class/range/fold/width/capture/resource paths. Outside reference grammar unchanged. `PcreBracketReferenceEscapeTests`; see `PCRE-BRACKET-REFERENCE-ESCAPES-DESIGN.md`. |
| `\cX` control escapes |Printable ASCII operand20..7E, a..z uppercase then XOR40; byte/UTF literals, classes/ranges, assertions and widths share existing paths; source delimiters and x/xx trivia are consumed as operands; `PcreControlEscapeReferenceTests`; see `PCRE-CONTROL-ESCAPES-DESIGN.md`; original byte diagnostics, CR/LF metadata, quotes, folding, offsets and work/cancellation |
| Ordinary bracket ranges, negation, escaped byte items, shorthand unions and fourteen non-UCP ASCII POSIX names | Implemented in the stacked byte-classes PR for 8-bit non-UTF mode. |
| Decimal and numeric `\gN`/`\g{N}` backreferences | Absolute and signed relative forms resolve to groups 1–256 in 8-bit non-UTF and UTF-8 modes; caseful by default, ASCII byte folding or provisional Unicode scalar folding with `(?i)` or the internal compile option. Captures keep source-byte spans, and UTF references consume subject scalar widths. Unique named declarations and references alias the same numbered slots. Bare decimal runs follow default PCRE2 reference/octal disambiguation using the current syntactic capture count; leading 8/9 permits forward references. Numeric g braces accept edge space/tab. See `PCRE-NUMERIC-ESCAPES-DESIGN.md`. See `PCRE-ABSOLUTE-G-REFERENCE-DESIGN.md` and `PCRE-RELATIVE-G-REFERENCE-DESIGN.md`. |
| Named captures and references | Unique `(?<name>...)`, `(?'name'...)`, and `(?P<name>...)` declarations retain ordinary capture numbering. `\k<name>`, `\k'name'`, `\k{name}`, `\g{name}` and `(?P=name)` resolve at compile time, including forward references. Names are case sensitive and at most 128 source bytes, with ASCII or UTF letter/decimal-digit grammar. Results remain numbered byte spans. Identical names may repeat for the same branch-reset slot; local J permits distinct slots per name; references select the first participating slot in declaration order with no comparison fallback. Native ordered registration can let an earlier matching name preempt a later number conflict. Nonrecursive calls now use globally resolved first definitions, shared out-of-line bodies and journaled return restoration; bounded calls in lookbehind use first-definition heap width dependencies; R/Rdigits/R&name conditions now test innermost active call context; parenthesized calls can now retain selected nested captures through the shared return journal; recursive/whole-pattern calls now share those guards and explicit resource budgets, including valid same-position capture changes. See `PCRE-RECURSIVE-CALLS-DESIGN.md`. See `PCRE-RETURNED-CAPTURES-DESIGN.md`. See `PCRE-RECURSION-CONDITIONS-DESIGN.md`. See `PCRE-SUBROUTINE-LOOKBEHIND-DESIGN.md`. See `PCRE-SUBROUTINES-DESIGN.md`. See `PCRE-NAMED-CAPTURES-DESIGN.md` and `PCRE-DUPLICATE-NAMES-DESIGN.md`. |
| `(?:...)` noncapturing groups | Implemented for 8-bit non-UTF mode; nested captures retain their numbers. |
| `(?|...)` branch-reset groups | Each direct alternative reuses entry capture numbers; following groups start after the highest slot used by any branch. Nested resets, relative references and same-slot name aliases resolve during parsing. Existing numbered VM slots preserve captures, repetitions and rollback. The 256-capture ceiling applies to the highest slot, while depth/expansion/work limits remain in force. See `PCRE-BRANCH-RESET-DESIGN.md`. |
| Fixed literal `(?=...)`, `(?!...)`, `(?<=...)`, `(?<!...)` over authoritative bytes | Implemented internally in byte and UTF-8 modes; zero-width, with no capture allocation and work/cancellation accounting. UTF-8 caseless assertions compare scalars and lookbehind counts scalar width; assertions cannot inspect across invalid-subject fragment barriers. General lookahead and fixed-length lookbehind bodies use the shared VM as described below. |
| General atomic `(?=...)` and `(?!...)` | Existing grammar, options, captures/names/references, alternatives, repeats and nested supported assertions execute on the original subject/fragment. Positive success keeps captures, possibly beyond G0, and prunes body retries; negative paths discard temporary mutations. Outer rollback restores prior captures and repeat state. Fixed literals preserve the original instruction/work path. Direct assertion quantifiers use finite counted lowering: unlimited maxima become minimum plus one; captures can change across zero-width iterations, optional retries restore earlier values, and possessive repeats commit all repeat choices. Fixed-length lookbehind shares these guards; bounded variable lookbehind extends these guards; consumed reference lengths resolve globally; short/long atomic assertion aliases share the same group path; non-atomic positive assertions extend these paths below; recursion conditions test the innermost active call through integer context targets; recursive calls now share the existing integer call and assertion contexts. See `PCRE-RECURSIVE-CALLS-DESIGN.md`. See `PCRE-RECURSION-CONDITIONS-DESIGN.md`. See `PCRE-GROUP-ALIASES-DESIGN.md`. See `PCRE-GENERAL-LOOKAHEAD-DESIGN.md` and `PCRE-ASSERTION-REPEAT-DESIGN.md`. |
| General fixed-length `(?<=...)` and `(?<!...)` | Classes, captures/names, groups, exact repeats, options/comments and nested assertions have fixed character lengths per top-level branch; those lengths may differ between direct alternatives. Heap analysis validates after complete parsing and reference resolution, including discarded zero-repeat bodies. Per-branch Reverse instructions run inside shared guards, respect malformed UTF fragment barriers, and preserve full nested-ahead visibility and source-byte captures. Compatible literals keep the original opcode. Bounded variable bodies extend this foundation below; consumed references use global capture-body length resolution below. See `PCRE-FIXED-LOOKBEHIND-DESIGN.md`. |
| Capture-participation conditional groups | Absolute/relative numbers and three name forms resolve globally, including forwards. Empty captures count and duplicate names test ANY participating slot. A capture-test instruction selects one branch without fallback; journal rollback can re-evaluate it on outer retry. Missing runtime else is empty, but native behind width analysis uses only explicit branches. Bare R/Rdigits actual names take precedence; DEFINE and VERSION comparisons use static conditions below. See `PCRE-CAPTURE-CONDITIONS-DESIGN.md`. |
| Static DEFINE/VERSION conditions | DEFINE skips execution while retaining global declarations; VERSION compares ASCII integer tuples against internal compatibility10.48. Jump selects one branch; all bodies still validate/compile. DEFINE0 and context-dependent nested width suppression, VERSION explicit widths and ignored-runtime repeats retain native analysis. All bodies set newline metadata; nullable analysis follows the chosen branch. See `PCRE-STATIC-CONDITIONS-DESIGN.md`. |
| Assertion conditional groups | Traditional atomic positive/negative ahead/behind and twelve symbolic/alpha spellings select one branch. Matching condition bodies preserve captures for both polarities; failed bodies restore the journal. One shared conditional guard commits body choices and restores the caller scope without a fallback branch. Non-atomic condition forms and repeated condition assertions are invalid; nested resumable bodies remain supported. See `PCRE-ASSERTION-CONDITIONS-DESIGN.md`. |
| Resumable positive lookahead and lookbehind | `(?*...)` / `(?<*...)`, short napla/naplb and long non_atomic_positive_lookahead/lookbehind preserve body/reverse retries after success. The existing journal restores captures and marks. Stable integer scope IDs hide assertion end limits during continuation and restore them on retry; every guard pop/prune releases one LIFO arena node, bounded by1024 saved frames. Atomic/negative/possessive commitments and width restrictions remain shared. See `PCRE-NONATOMIC-ASSERTIONS-DESIGN.md`. |
| Bounded variable-length lookbehind | Heap min/max analysis supports finite repeats, unequal nested alternatives and consuming `\R`. If any direct branch varies, all branch maxima must fit255. Longest available start precedes body greedy/lazy preference; one compact retry frame journals each next start. Pinned Compiled/JIT scoped ends affect all branches, nested assertions and end tests, then restore with scope checkpoints. Tolerant UTF maximum rewind follows pinned invalid-barrier rules. Consumed references extend this foundation below. See `PCRE-VARIABLE-LOOKBEHIND-DESIGN.md`. |
| Consumed backreferences in lookbehind | Complete numeric/name resolution precedes a global heap capture/lexical map and min/max dependency evaluation. Bounded capture-body widths, forward definitions, empty/unset participation and transitive references use existing fixed/variable bytecode. Lexical own-group references, consumed cycles, unbounded target bodies and global branch-reset syntax reject at the owning opener. Zero-width lookahead references add no consumed edge. Shared heap reconstruction removes recursive named rewriting; every behind validates even if erased. See `PCRE-LOOKBEHIND-REFERENCES-DESIGN.md`. |
| `\A`, `\Z`, `\z` subject anchors | Implemented internally with LF as the final-newline convention for `\Z`; `\A` and `\z` remain strict subject edges. |
| `\G` search start and `\K` reported start | G retains caller start inside lookahead; K is invalid inside assertion nesting and in brackets. `\G` tests the original requested byte offset; `\K` resets group-zero and whole-match reporting while preserving earlier numbered captures. Capture-start slot zero is journaled for rollback across branches, atomic sections and repetitions. See `PCRE-MATCH-POSITIONS-DESIGN.md`. |
| Default `^` and `$` | `^` tests absolute start; `$` tests absolute end or before one final LF. Other option-dependent behavior remains pending. |
| ASCII byte-mode `\b` and `\B` word boundaries | Implemented internally using the same non-UCP ASCII `\w` definition. Bracket `\b` remains backspace; bracket `\B` is unsupported. |
| Leading, mid-pattern and scoped `i`, `m`, `n`, `s`, `U`, `J`, `r`, `x`/`xx`, `aD`/`aS`/`aW`/`aP`/`aT`/bare `a` options | Combinations and setting/unsetting compile at source locations, propagate to later alternatives and restore at every group boundary. Comparison instructions retain local casing; dot/anchors retain local DotAll/multiline. n suppresses unnamed capture allocation while named groups still capture. U reverses default repetition greediness; a question suffix reverses it again, while possessive repeats are always greedy. Quantifiers around groups use the restored enclosing mode. x ignores fixed Pattern_White_Space and line comments; xx additionally ignores literal SP/HT in brackets. Ordinary `(?#...)` comments are independent of x. Lexical skipping preserves source indexes, quotes, token boundaries and fixed assertion bodies. Explicit x downgrades xx; only adjacent xx selects it. ASCII restriction tokens share the scanner and a five-bit group snapshot; aP sets POSIX plus POSIX-digit, aT sets POSIX-digit, and bare a sets all bits. Unset wins per bit. Predicates and b/B retain their local policy. Local r blocks ASCII/non-ASCII fold crossings while preserving ordinary Unicode case pairs and explicit property semantics; UCP POSIX lower/upper retain Ll/Lu. Empty and reset headers, including empty hyphen sections, preserve U/J/ASCII restrictions while reset clears i/m/n/r/s/x/xx. J permits later duplicate declarations, restores at group boundaries and survives reset; all name references resolve globally and commit to the first participating declaration. See `PCRE-DUPLICATE-NAMES-DESIGN.md`. See `PCRE-OPTION-SCOPE-DESIGN.md`, `PCRE-CAPTURE-REPEAT-OPTIONS-DESIGN.md`, `PCRE-EXTENDED-OPTIONS-DESIGN.md`, `PCRE-RESET-OPTIONS-DESIGN.md`, `PCRE-ASCII-DIGIT-OPTIONS-DESIGN.md`, `PCRE-ASCII-RESTRICTIONS-DESIGN.md` and `PCRE-CASELESS-RESTRICTIONS-DESIGN.md`. |
| Internal `PcreByteOptions.Caseless` | Caller-supplied caseless compile option uses ASCII non-UCP byte behavior or shared provisional Unicode behavior for byte-UCP/UTF literals, properties, backreferences and fixed literal assertions; leading options preserve or explicitly unset it. Grep `-i` integration remains pending. |
| Internal `PcreByteOptions.Utf8` | Opt-in strict UTF-8 scalar literals and dot preserve source-byte spans, with invalid pattern/subject diagnostics and character-boundary search offsets. Ordinary UTF brackets combine literal ranges, supported shorthands/properties and all fourteen POSIX predicates in caseful or provisional caseless mode. Caseless ranges are compiled to folded intervals; wide ranges can add compilation work. Nonliteral lookarounds remain explicitly unsupported. |
| Internal `PcreByteOptions.MatchInvalidUtf` | Opt-in PCRE2-style valid UTF-8 fragments in malformed subjects, implying UTF-8 and preserving whole-source byte spans. Consuming atoms and backreferences cannot cross a malformed sequence; fragment edges govern word boundaries while `\\z`/`\\Z` require the whole-subject end. Pinned PCRE2 10.48 exhaustive offset conformance remains pending. |
| Byte/UTF-8 `\p`/`\P` general categories | The abbreviated C/L/M/N/P/S/Z groups and two-letter Unicode general categories (including `Lc`/`L&`) work as scalar atoms and bracket members, with complements and original byte spans. Loose name matching and `gc:`/`General_Category=` forms are supported. Category data comes from the target .NET runtime; version reconciliation remains a conformance gate before Grep migration. Any/Xan/Xps/Xsp/Xwd/Xuc now share scalar predicates with exact complements and local-mode independence; source operand provenance controls UTF class CR/LF search metadata before folding. The existing gc/category-prefix aliases remain a library extension restricted to categories. Script, binary and other properties remain unsupported. See `PCRE-SPECIAL-PROPERTIES-DESIGN.md`. |
| UTF-8 UCP digit policy | Internal `Ucp` and `AsciiBsD` options distinguish ASCII `\d` from Unicode POSIX `[[:digit:]]` and explicit `\p{Nd}`. Leading, mid-pattern, scoped and combined aD settings select the local `\d`/`\D` restriction; reset preserves it and unset wins. Byte UCP now uses these same predicates over its one-byte domain. UTF brackets combine these predicates with literal scalar ranges; aP restricts all POSIX classes and aT only digit/xdigit; Unicode POSIX punct follows pinned execution (P plus ASCII symbols, despite stale below256 manual prose); Unicode-version parity remains pending. |
| UTF-8 UCP word behavior | Scalar `\w`/`\W` use Unicode L/N/Mn/Pc under UCP and ASCII word membership without it; `\b`/`\B` compare adjacent UTF scalars under UCP, while non-UCP retains ASCII boundaries. Local aW selects ASCII word classes and boundaries independently of POSIX word. Bracket unions follow the same scalar predicates. |
| UTF-8 UCP whitespace | Scalar `\s`/`\S` use Unicode separators plus HT/LF/VT/FF/CR, U+0085 and U+180E under UCP; without UCP they use the six ASCII whitespace characters. Local aS selects ASCII s/S; aP independently selects ASCII POSIX space/word. h/v/R stay independent. |
| `\h`, `\H`, `\v`, `\V` whitespace classes | Fixed PCRE2 horizontal/vertical lists and complements work as byte and UTF-8 atoms and bracket members, independently of UCP, caseless and newline convention. Byte mode includes NBSP in `\h` and NEL in `\v`; UTF mode consumes whole scalars with original byte spans. `\v` matches one character. See `PCRE-SPACE-ESCAPES-DESIGN.md`. |
| `\R` newline sequence | Internally lowered to an atomic CRLF-first alternation plus single-character line endings. Byte mode includes LF/VT/FF/CR/NEL; UTF adds LS/PS. Internal `BsrAnyCrLf` restricts it to CRLF/CR/LF independently of the newline convention; pattern-side BSR directives select the same option from an exact start prefix. Invalid inside brackets. General lookahead and nested lookahead inside fixed behind may use it; consuming `\R` behind uses bounded variable length1..2. See `PCRE-NEWLINE-SEQUENCES-DESIGN.md`. |
| Counted repetitions and literal braces | Complete `{n}`, `{n,m}`, `{n,}`, `{,m}` shapes are recognized before numeric interpretation; ASCII SP/HT is allowed at edges/comma. Bounds cap65535; genuine overflow/reversed counts reject, while malformed/incomplete shapes lex as ordinary literal braces character by character, including x/xx/quoting/group boundaries. All repeat/nonrepeatable/duplicate checks share classification. Existing ordered greedy/lazy/U/possessive lowering and4096 instruction/node/256-mark limits remain. Explicit braced escape/name/property grammars stay strict. See `PCRE-BRACE-GRAMMAR-DESIGN.md`. |
| Octal `\0`, ambiguous bare digit escapes and `\o{...}` | Up to three bare octal digits, or explicit braced octal values with edge space/tab, lower to byte/UTF literals. Brackets interpret numeric escapes as octal literals or literal 8/9; ranges, fixed literal assertions and repetition reuse existing primitives. Values must fit a byte or a Unicode scalar. Extra Python/NO_BS0 modes remain pending. See `PCRE-NUMERIC-ESCAPES-DESIGN.md`. |
| Braced `\x{...}` hexadecimal values | Implemented internally for byte and UTF-8 atoms, ordinary bracket members/ranges and fixed literal assertions. Byte values are at most 0xff; UTF values must be Unicode scalars. |
| `\N` and UTF `\N{U+...}` | Bare non-newline matching shares dot's LF/AnyCRLF convention with DotAll forced off. Counted forms such as `\N{2,3}` use ordinary repetition; only the `U+` prefix selects a codepoint literal. UTF codepoint forms share braced-hex validation, byte spans, brackets/ranges and fixed literal assertions; bare `\N` is invalid in brackets and works in general lookahead and fixed-length lookbehind. See `PCRE-NON-NEWLINE-DESIGN.md`. |
| Atomic `(?>...)` and possessive `*+`, `++`, `?+`, `{n,m}+` | Implemented internally as scoped pruning of backtracking choices. Nested groups retain captures, and preceding alternatives can still backtrack. The exact lowercase `(*atomic:...)` alias uses the same group path; performance conformance remains pending. See `PCRE-GROUP-ALIASES-DESIGN.md`. |
| `\Q...\E` literal quoting | Implemented internally for ordinary patterns, ordinary brackets and fixed literal assertions, including isolated `\E`, unterminated quotes through pattern end, and UTF-8 scalar literals. A quoted hyphen in a bracket is a member; quoted characters can end a range if its hyphen lies outside the quote. See `PCRE-QUOTED-LITERALS-DESIGN.md`. |
| Other escapes, remaining inline options and other special groups | Explicit compile-time `UnsupportedSyntax` diagnostic. |
| Script, binary and remaining UCP property names and full Unicode fold-table conformance, pinned invalid-UTF offset conformance, broader backreference syntax, script runs, backtracking verbs and broader PCRE2 syntax | Pending separate implementation and conformance fixtures. |

The initial parser limits group nesting, total captures, and repeat-state marks to 256 each. The matcher executes ordered bytecode with an explicit stack bounded at 1,024 saved paths and a work budget of 1,000,000 steps by default; long linear matches do not spend stack slots. Ordinary unbounded empty iterations proceed to the continuation once while retaining captures and alternative paths. Direct assertion repeats instead execute finite bounds, including every required zero-width iteration, and need no progress marks. These limits are provisional and require stress and performance review before a consumer switch.

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

## Byte class CRLF search follow-up

Byte class CRLF continuation now preserves negated source operand count/shape and literal/range endpoints, including the native negated-singleton exception. Historical positive interior-range flags remain until broader start-optimization coverage prevents lost matches; known false positives stay in #99. The bounded byte two-atom negated-singleton/all-byte class plus literal prefix preserves JIT LF candidate entry; optional single-branch capture/noncapture wrappers qualify. Broader prefix optimizer shapes remain tracked in #99. UTF behavior and all VM work/cancellation/resource paths remain shared. See `PCRE-BYTE-CRLF-DESIGN.md` and `PcreByteCrLfSearchReferenceTests`.

## UTF leading-class CRLF entry

Mandatory positive predicate-free ASCII bracket starts which exclude CR now retain native JIT LF entry, including all-qualified alternatives, capture/noncapture/atomic wrappers and required variable/exact1 repeats. At least one qualifying start class must admit LF, and root/worklist analysis is conservatively capped at1,000 visits to respect native start-bitmap exhaustion. Source newline metadata remains independent; all candidates keep existing VM work/cancellation and original fragment/capture protocols. General fixed/optional/assertion/call/Unicode starts remain in #102; byte #99/#101 behavior is unchanged. See `PCRE-UTF-CRLF-START-CLASS-DESIGN.md` and `PcreUtfCrLfStartClassReferenceTests`.

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


## UTF leading-capture dependencies

PcreUtfCaptureDependencyReferenceTests distinguishes prefix capture slots from
references/tests of later slots. The compiler collects traversed prefix numbers
and returns separate Eligible/Skip results from its bounded whole-root scan. A
related resolved backreference or capture-condition target clears Skip while
retaining class recognition; unrelated targets keep depth<=4 repeat acceleration.
All duplicate-name slots and syntactic condition/assertion bodies are observed.
Zero-count group bodies remain visible as native OP_SKIPZERO requires; direct
zero-count backreference repeats, calls, DEFINE/other conditions, unknowns and
over-bound analysis retain conservative baseline admission.
The existing1000 branch/node/queue proof and parser256/capture256 ceilings remain;
slot observations check cancellation. No parser/AST/membership/source metadata/
UTF validation/matcher/program/instruction layout/Step/capture/frame/journal/
runtime-state or raised-limit change. All6309 baseline expectations/frozen35
unchanged. Broader102/byte99/unsafe100 remain; no public/release/consumer claim. See
PCRE-UTF-CAPTURE-DEPENDENCIES-DESIGN.md and PCRE-UTF-CAPTURE-DEPENDENCIES-PLAN.md.


## UTF direct zero-count reference pruning

PcreUtfZeroReferencePruningReferenceTests pins native omission of a direct
Backreference repeat with maximum0. The existing capture-dependency classifier
skips that child without clearing Skip or rejecting class recognition. Retained
zero-count group/assertion/condition bodies still traverse normally; nested
direct zeros and later live references preserve their distinct native effects.
Numeric/relative/named/duplicate targets, lazy/possessive/count forms and shared
wrapper depth use the existing Set/Skip proof and endpoint recovery.
One compiler handler changes; no parser/AST/source flags/membership/UTF validation/
matcher/program/instruction layout/Step/capture/frame/journal/runtime-state or
resource-ceiling change. All6464 baseline expectations/frozen35 unchanged. Existing
compile1000/slots256/cancellation bounds remain. Leading-prefix pruning before
the selected PLUS, calls/DEFINE/other conditions and broader102/byte99/unsafe100
remain; no public/release/consumer claim. See
PCRE-UTF-ZERO-REFERENCE-PRUNING-DESIGN.md and PCRE-UTF-ZERO-REFERENCE-PRUNING-PLAN.md.


## UTF entry through pruned reference prefixes

PcreUtfPrunedPrefixReferenceTests pins native first-class entry after direct
zero-count references before a leading PLUS disappear. A shared compiler helper
skips direct Repeat0Backreference or possessive AtomicRepeat0Backreference,
charging2/3 omitted AST nodes and checking CT within1000 visits. It returns the
first retained node or no node for exhausted/all-omitted bodies.
Repeat selection uses it at root/group bodies; total group depth and capture-slot
classification remain distinct. The start-class proof skips omitted prefixes only
when the selector already recognized a repeat leaf. Other branch/prefix forms keep
conservative admission; nonpruned proof-node charging is unchanged.
No parser/AST/source flags/membership/UTF validation/matcher/program/instruction
layout/Step/capture/frame/journal/runtime-state/raised-limit change. All6567
baseline expectations/frozen35 unchanged. Retained/nullable/alternative/other
pruned prefixes, calls/DEFINE/other conditions and broader102/byte99/unsafe100
remain; no public/release/consumer claim. See
PCRE-UTF-PRUNED-REFERENCE-PREFIX-DESIGN.md and PCRE-UTF-PRUNED-REFERENCE-PREFIX-PLAN.md.


## UTF entry after retained zero-count prefixes

PcreUtfRetainedZeroPrefixReferenceTests separates bitmap entry from repeat
acceleration after native SKIPZERO brackets. The shared helper returns Node and
BlocksSkip; supported zero group/assertion/atomic/capture-condition prefixes can
be crossed for the first class proof while permanently blocking repeat skipping
along that prefix. Truly omitted references keep prior behavior; trailing retained
brackets do not disable earlier acceleration. Capture slots/dependencies remain
global and retain unset-prefix capture spans.
Selector barriers accumulate across root/group bodies and are ANDed with depth
and capture eligibility. Proof uses only Node under the existing selected-leaf
guard. Cost2/3 plus possessive wrapper1, CT and1000 proof/classification bounds
remain. No parser/AST/source flags/membership/UTF validation/matcher/program/
instruction layout/Step/capture/frame/journal/runtime-state/raised-limit change.
All6668 baseline expectations/frozen35 unchanged. Other nullable/alternative/
pruned starts, calls/DEFINE/other conditions and broader102/byte99/unsafe100 remain;
no public/release/consumer claim. See
PCRE-UTF-RETAINED-ZERO-PREFIX-DESIGN.md and PCRE-UTF-RETAINED-ZERO-PREFIX-PLAN.md.


## UTF entry through pruned atom prefixes

PcreUtfPrunedAtomPrefixReferenceTests pins native entry and acceleration after
direct zero-count literals, character types, classes and newline sequences.
The shared first-node helper treats supported UTF primitive repeats as omitted,
with the same cost-two/nonretained result as direct reference0; an outer
possessive wrapper adds one visit. Retained zero brackets still block repeat
skipping, including brackets containing pruned atoms.
Source newline metadata survives omission: a removed newline atom still changes
candidate behavior. AST/source flags are unchanged. Root/group traversal, capture
dependencies, selected-leaf proof, depth and cancellation/1000-visit bounds remain
shared. No matcher/program/instruction layout/Step/capture/frame/journal/runtime
state/raised-limit changes. All6789 baseline expectations/frozen35 unchanged.
General nullable/alternative/other pruned starts, calls/returned captures, DEFINE/
other conditions and broader102 optimizer provenance remain; byte99 and unsafe100
are separate. No public/release/consumer readiness claim. See
PCRE-UTF-PRUNED-ATOM-PREFIX-DESIGN.md and PCRE-UTF-PRUNED-ATOM-PREFIX-PLAN.md.

The original optional-literal control exposed a further bounded source shape:
QUERY adds its literal start bit but raises early-fail count to3, preventing
type_skip. A direct Minimum0/Maximum1 Literal-not-CR (including its possessive
wrapper) now crosses the same helper with cost2/3 and BlocksSkip=true. Optional
input consumption remains in the unchanged VM; source newline metadata remains
intact. PcreUtfOptionalLiteralPrefixReferenceTests separately pins native spans,
consumed prefixes, modifiers, source CR/LF, depth/dependencies/retained and trailing
controls plus UTF/work/cancellation protocols. Original241 tests are unchanged.
Optional classes/types/groups, larger finite bounds and general nullable forms
remain separate; this does not claim general optional-prefix support.


## UTF entry through optional positive sets

PcreUtfOptionalSetPrefixReferenceTests pins bitmap entry after optional positive
predicate-free sets. Native class study adds their start bits and continues;
CRQUERY raises early-fail count to3, preventing repeat skipping. The shared
first-node helper crosses only min0/max1 ScalarSet nodes passing
IsUtfCrLfOptionalSet: nonempty/nonnegated/no predicates, valid scalar intervals
and no CR member. Each range charges the shared cancellation/1000-visit budget;
repeat/child cost2 and possessive wrapper1 remain. No scalar expansion occurs.
Optional LF consumption and full captures stay in the existing VM. LF is not
required in the optional set; the mandatory selected class still establishes it.
Root/group traversal, accumulated barriers, capture dependencies and source flags
remain unchanged. CR-admitting/negated/predicate/unknown/over-bound/all-optional
forms stay conservative. No matcher/runtime layout/storage/Step/frame/journal/
raised-limit/public/release/consumer changes. All7136 baseline expectations and
frozen35 source blobs unchanged. Broader102 optimizer/nullable/call/DEFINE/
condition forms remain; byte99 and unsafe100 separate. See
PCRE-UTF-OPTIONAL-SET-PREFIX-DESIGN.md and PCRE-UTF-OPTIONAL-SET-PREFIX-PLAN.md.

Author review added PcreUtfOptionalSetCaseFoldReferenceTests before a proof
correction. A caseless set with one canonical folded key may have originated
from native PT_CLIST property-query bytecode, which has different bitmap
availability. Reject that ambiguous shape after bounded range validation;
retain multi-key caseless admission. Original196 tests are unchanged. Ordinary
caseless singletons and different source spellings of one folded key retain
their preexisting conservative path pending source provenance; no case-fold
membership or source metadata is changed.


## Original UTF set source shape before folding

PcreUtfSetSourceShapeReferenceTests pins the distinction between original class
operands and canonical case-fold keys. PcreByteScalarSet now carries internal
init-only Source provenance (Unknown/Literal/Class), default Unknown. The UTF
bracket parser records Literal for one original literal/equal-end range and
Class otherwise, using existing sourceTermCount/soleLiteral before case folding.
Constructor parameters, membership, case closure and ExplicitNewline are intact;
record with-copies and reference-based tree rewrites preserve the provenance.
The existing optional-set proof admits proven Class sources after its positive/
nonempty/predicate-free/scalar-bound/order/CR/cancellation/1000-visit checks, even
when caseless folding leaves one key. Genuine Literal/Unknown one-key sources
stay conservative. Optional input consumption/captures and the Skip barrier
remain shared. Metadata is compile-time only, not emitted into runtime state.
No matcher/program/instruction layout/Step/frame/journal/limit/public/release/
consumer changes. All7359 baseline expectations/frozen35 unchanged. Genuine
singleton classification and broader102 source/nullable/call/DEFINE/condition
forms remain; byte99/unsafe100 separate. See
PCRE-UTF-SET-SOURCE-SHAPE-DESIGN.md and PCRE-UTF-SET-SOURCE-SHAPE-PLAN.md.

The original caseless wide-set no-LF budget fact also exposed unnecessary
interval scanning (already RED before provenance implementation).
PcreUtfCaseFoldRangeWorkTests pins the correction: after the existing Step
charges an interval probe, ScalarSet matching stops if Caseless and the folded
scalar is below that sorted interval. Raw noncaseless ranges retain source-order
search; predicates and negation remain after the range loop. This small matcher
early stop consumes work only for inspected intervals; no Step-function, limit,
program/frame/journal/layout/storage changes. Original198 tests remain unchanged;
observed/unobserved exact/exhausted work and the default budget are verified.

## UTF singleton literal query availability

PcreUtfLiteralCaseQueryReferenceTests distinguishes ordinary optional literal
queries from native caseless PT_CLIST property queries. ScalarSet retains internal
init-only SourceLiteral (default -1) before folding; constructor parameters,
Source provenance, case closure, membership and ExplicitNewline remain intact.
The compiler shares a private bounded PCRE2 10.48 default UCD_CASESET classifier
between plain optional literals and proven Literal bracket sources. All84 default
members remain property queries unless restricted casing suppresses an ASCII-
crossing K/S caseset; Turkish-extra pairs are excluded from this default table.
This is source opcode classification, not a replacement Unicode membership table.
Proven Class sources retain #136 behavior; Unknown one-key sources stay conservative.
Existing positive/nonempty/predicate-free/scalar/order/CR/cancellation/1000-visit
checks, cost2 plus wrapper1, depth/capture eligibility and BlocksSkip stay intact.
No matcher, runtime metadata/layout, Step, frame/journal, limit, public API,
workflow, package, dependency or consumer change. All7565 baseline expectations
and frozen35 remain unchanged. Native15292 checks pin382 new cases/full7947,
covering every84 default member as plain/bracket, ordinary/restricted/mixed/
consumed/deep/capture/modifier controls, six newline modes, UCP, every caller
offset, strict/tolerant fragments, work/exhaustion/cancellation/default budget
and parser257. See PCRE-UTF-LITERAL-CASE-QUERY-DESIGN.md and its PLAN.md.
Broader102 nullable/type/group/negated/predicate/call/DEFINE/condition forms and
native-complete availability remain; byte99/unsafe100 stay separate. This does
not qualify a PCRE release or Grep migration; CommandFramework remains untouched.

## UTF optional type-query prefixes

PcreUtfOptionalTypePrefixReferenceTests pins direct min0/max1 ASCII digit/word/
nonspace and horizontal-space queries. The shared leading-node compiler switch
admits positive AsciiDigit/AsciiWord/HorizontalSpace and negated AsciiSpace as
cost2/retained true. Existing parsed Kind/Negated carries UCP and scoped aD/aW/aS
restrictions; Unicode digit/word/nonspace properties remain excluded, while
horizontal space qualifies under either UCP setting. Every admitted type excludes
CR and has native bitmap study; NOT_VSPACE/ANY/property/CR-consuming/other types
remain conservative. Existing wrapper1/shared1000-visit/cancellation/depth/capture
checks and BlocksSkip apply. No parser/membership/folding/matcher/Step/runtime/
layout/storage/limit/public/package/workflow/dependency/consumer changes.
429 new cases/full8376 preserve all7947 baseline expectations/frozen35. Native
16188 bounded JIT checks (828 full-capture+15360 offset protocols) cover UCP/local
ASCII/caseless restrictions, optional modifiers/consumption, deep/capture/
dependency/mixed and conservative controls, six newlines/every caller offset/
strict-tolerant UTF, exact/exhausted/observed work/cancellation/default budget/
parser257. Larger optional bounds have consumption-only controls; their bitmap
availability remains outside this change. See PCRE-UTF-OPTIONAL-TYPE-PREFIX-DESIGN.md
and PCRE-UTF-OPTIONAL-TYPE-PREFIX-PLAN.md. Keep102 open for bracket predicates,
larger optional bounds, groups/alternatives/negated/property/call/DEFINE/condition
availability; byte99/unsafe100 separate. No complete PCRE/release/Grep-readiness
claim. CommandFramework remains untouched until publication/all known migrations.

## UTF finite optional atom prefixes

PcreUtfFiniteOptionalPrefixReferenceTests extends the already supported optional
literal/set/type source forms to positive finite maxima. The four existing
leading-node cases keep Minimum0 and require Maximum>0 and<int.MaxValue,
reusing literal caseset/restricted-casing classification, original set provenance/
CR/range/predicate/CT/1000-visit guards and the four type Kind/Negated cases.
Native UPTO/TYPEUPTO/nonexact CRRANGE use the relevant query bitmap rule and
raise early-fail count3; existing cost2/wrapper1/retained/BlocksSkip/depth/capture
checks remain shared. Max0 omission, required and unbounded cases are unchanged.
No parser/membership/folding/matcher/Step/instruction/frame/journal/layout/
storage/limit/public/package/workflow/dependency/consumer changes. Bounded
repeats still expand under the existing4096 instruction/visit limits; native65535
bounds are not a promise that every managed pattern of that size compiles.
617 new cases/full8993 preserve all8376 baseline expectations/frozen35. Native
19772 bounded JIT checks (1196 fullcapture+18576 offset protocols) cover finite
bounds/modifiers/source forms/UCP/casing, consumed/deep/capture/dependency/mixed/
excluded controls, six newlines/every offset/strict-tolerant UTF, work/exhaustion/
cancellation/default budget/parser257 and count/expansion rejection. Unbounded
controls pin ordinary consumption only; broader star availability remains open.
See PCRE-UTF-FINITE-OPTIONAL-PREFIX-DESIGN.md and its PLAN.md. Keep102 open for
bracket predicates, unbounded/groups/alternatives/negated/property/call/DEFINE/
condition availability. Byte99/unsafe100 separate; no complete PCRE/release/Grep
readiness claim. CommandFramework untouched until publication/all known migrations.

## UTF star-prefix entry and consumed-run recovery

PcreUtfStarPrefixReferenceTests pins supported Literal/ScalarSet/type stars at
root branch start or after a locally proven retained optional prefix. The compiler
keeps a compile-time root-first star child identity, clears it when following
mandatory set-PLUS selection fails, and gates unbounded admission through that
identity or local BlocksSkip. Finite classifiers/cost2/wrapper1/CT/1000-visit/
source/CR/property/UCP/capture/depth guards remain shared. A root star owns native
type_skip; queries/retained barriers prevent a later star from owning that cache.
The existing SkipCrLfAfterRepeat instruction bit marks only the root star's atom,
or the already supported leading PLUS. Literal/ScalarClass success now records
max(utfRepeatEnd,position) after the charged comparison under the same no-call/
no-assertion guard already used for ScalarSet. Existing per-attempt reset and
failure recovery skip LF only after a consumed marked run ending at CR; empty
prefix runs retain bitmap LF entry. ExplicitNewline/fragment/caller/capture/
backtracking/Step/depth/work/cancellation policies remain intact. No new runtime
layout/state/frame/journal/span/pointer/limit/API/package/workflow/dependency or
consumer change. Parser/membership/folding and CF remain untouched.
919 new cases/full9912 preserve all8993 baseline expectations/frozen35. Native
22916 bounded JIT checks (1796 fullcapture+21120 newline/UCP/offset protocols)
cover consumed/empty runs, first-cache ownership/barriers/mixed stars/modifiers/
source/casing/UCP, root stars before deeper/captured/dependent sets, explicit
newline/excluded controls, strict-tolerant UTF, exact/exhausted/observed work,
cancellation/default budget/parser257/backtrack depth rejection. See
PCRE-UTF-STAR-PREFIX-DESIGN.md and PCRE-UTF-STAR-PREFIX-PLAN.md. First stars inside
wrappers/after erased prefixes remain conservative; their controls pin ordinary
consumption only. Keep102 open for further placement, bracket predicates/group/
alternative/negated/property/call/DEFINE/condition availability; byte99/unsafe100
separate. No complete PCRE/release/Grep-readiness claim; publication/all known
consumer migrations precede any CF removal.

The original atomic-following-star fixtures require bitmap traversal through
Atomic(NoncapturingGroup(body)) only behind a proven root-star barrier. That
bounded walk retains group/visit/capture checks and assigns no later repeat skip
marker; pre-existing unguarded atomic contexts remain conservative.

## UTF star ownership after erased primitives

PcreUtfErasedStarPrefixReferenceTests pins the first surviving star after direct
Max0 primitives/references. IsUtfCrLfErasedRepeat shares the existing six-family
Backreference/Literal/Any/ScalarClass/ScalarSet/NewlineSequence vocabulary between
leading-node cost2/retained-false traversal and root-star discovery. Discovery
checks cancellation per node, charges root1+2 per erased operand+possessive
wrapper1 under the same1000-visit bound, and stops at the first other node.
Retained groups/assertions/conditionals and queries remain ownership barriers;
first stars inside wrappers remain separate proof work. The star still reuses
all source/casing/UCP/CR/predicate/range/capture/depth guards, existing instruction
marker and consumed-run endpoint/reset/recovery. Source ExplicitNewline and its
negated-singleton exception survive erasure. No matcher/parser/membership/folding/
Step/runtime layout/state/frame/journal/span/pointer/limit/API/package/workflow/
dependency/consumer or CF changes.
885 new cases/full10797 preserve all9912 baseline expectations/frozen35. Native
18624 bounded JIT checks (1728capture+16896newline/UCP/offset) cover six erased
families/aliases/sequences through128, modifiers/source/UCP/casing, empty and
consumed runs, newline flags, following capture/dependency/deep/atomic groups,
retained/query barriers, strict-tolerant UTF/work/cancellation/default budget/
parser257/backtrack-depth limits. See PCRE-UTF-ERASED-STAR-PREFIX-DESIGN.md and
PCRE-UTF-ERASED-STAR-PREFIX-PLAN.md. Wrapper-first-star controls pin ordinary
consumption only. Keep102 open for wrappers/other placement/bracket predicates/
group/alternative/negated/property/call/DEFINE/condition availability; byte99/
unsafe100 separate. No complete PCRE/release/Grep-readiness claim; publication/
all known migrations precede any CF removal.

## UTF star prefixes inside noncapturing wrappers

PcreUtfWrappedStarPrefixReferenceTests separates first-star bitmap recognition
from consumed-run recovery. Bounded discovery iteratively descends nonempty
single-branch noncapturing ancestors and existing erased operands, returning
compile-time Atom/Skip. It charges root1/group1/erased2+possessive wrapper1 with
CT/shared1000-visit checks. Skip is true only through four star ancestors;
recognition remains for supported deeper chains. Following captures/deep groups
do not add to that star depth. Capturing/explicit atomic ancestors, alternatives,
retained/query/unsupported nodes stay separate; atomic unwrapping requires a
Repeat child so explicit Atomic(NoncapturingGroup) is not admitted accidentally.
Compiler stores/clears both fields on final mandatory-set proof failure and gates
only the star endpoint marker by Skip. Existing leading-PLUS marker/admission/
source/casing/UCP/CR/predicate/range/capture/depth/BlocksSkip/ExplicitNewline/
fragment/resource guards remain intact. No matcher/Step/runtime layout/state/
frame/journal/span/pointer/limit/API/package/workflow/dependency/consumer/CF changes.
921 new cases/full11718 preserve all10797 baseline expectations/frozen35. Native
18696 bounded JIT checks (1800capture+16896newline/UCP/offset) pin depths1/3/4/5/6/
33/199, empty/consumed runs and the4/5 cache boundary, star source/modifiers/UCP/
casing, erasures across levels, later capture/dependency/deep/atomic groups,
explicit newline/unsupported controls, strict-tolerant UTF/work/cancellation/
default budget/parser257/depth limits. See PCRE-UTF-WRAPPED-STAR-PREFIX-DESIGN.md
and PCRE-UTF-WRAPPED-STAR-PREFIX-PLAN.md. Unsupported ancestor/nullable-prefix
wrapper controls pin ordinary consumption only. Keep102 open for capturing/
atomic ancestors, other placements, bracket predicates/group/alternative/negated/
property/call/DEFINE/condition availability; byte99/unsafe100 separate. No complete
PCRE/release/Grep readiness claim; CF removal requires publication/all migrations.

## UTF star prefixes inside capturing ancestors

PcreUtfCapturedStarPrefixReferenceTests pins native bitmap entry independently
from capture-dependent consumed-run recovery. Existing bounded iterative first-star
discovery now descends nonempty single-branch capturing as well as noncapturing
ancestors. Both charge group1/count toward the same ancestor-depth4 boundary.
Only capture numbers before the star enter a compile-time HashSet; existing
ClassifyUtfCaptureRepeat determines eligibility and disables Skip for related
references/tests while retaining identity. Direct reference0 remains erased;
retained zero-group references remain dependencies. Named/duplicate references
and capture tests use existing resolved numbers. Unrelated/following captures
do not disable the earlier star's cache. Explicit atomic ancestors, alternatives
and unsupported placements remain separate; possessive-repeat unwrapping and
final mandatory-set identity/Skip reset stay intact. Existing source/caseset/r/
UCP/CR/predicate/range/capture/BlocksSkip/ExplicitNewline/fragment/resource guards
and leading-PLUS marker remain. No matcher/parser/Step/runtime state/layout/
instruction/program/frame/journal/snapshot/span/pointer/limit/API/package/version/
workflow/dependency/consumer/CommandFramework changes.
1080 new cases/full12798 preserve all11718 baseline expectations/frozen35. Native
19008 bounded JIT checks (2112fullcapture+16896newline/UCP/offset) pin depths1/3/4/
5/6/33/199, related/unrelated/named/duplicate references/tests, erased-vs-retained
dependencies, mixed ancestors, star source/modifiers/casing/UCP, erased prefixes,
suffix capture/deep/atomic groups, newline flags/unsupported controls and strict/
tolerant UTF/work/cancellation/default budget/parser257/captures257/depth limits.
See PCRE-UTF-CAPTURED-STAR-PREFIX-DESIGN.md and PCRE-UTF-CAPTURED-STAR-PREFIX-PLAN.md.
Keep102 open for atomic ancestors/other placements/bracket predicates/group/
alternative/negated/property/call/DEFINE/condition availability; byte99/unsafe100
separate. No complete PCRE/release/Grep-readiness claim; publication/all known
migrations precede CF removal. Unsupported placement controls pin ordinary consumption.

## UTF star prefixes inside explicit atomic ancestors

PcreUtfAtomicStarPrefixReferenceTests pins native bitmap entry through explicit
atomic ancestors without assigning consumed-star endpoint recovery. Bounded
first-star discovery now accepts Atomic(NoncapturingGroup), charges an extra1
before its existing group1/root1/erased2+possessive-wrapper1 proof costs, and
disables local Skip after any atomic ancestor. Nonempty/single-branch/CT/1000/
capture-classification guards remain. Possessive Atomic(Repeat) retains iterator
rules; outside root stars retain recovery when only their suffix is atomic.
Mandatory-set discovery permits atomic descent only with proven first-star
identity and disables later PLUS Skip during that descent. Existing post-star
atomic traversal costs, final failed-proof identity/Skip reset, source/casing/r/
UCP/CR/predicate/range/capture/BlocksSkip/ExplicitNewline/fragment/resource guards
remain. No matcher/parser/Step/emitter/runtime state/layout/instruction/frame/
journal/span/pointer/snapshot/API/limit/workflow/package/version/dependency/
consumer/CommandFramework changes.
1130 new cases/full13928 preserve all12798 baseline expectations/frozen35. Native
19108 bounded JIT checks (2212strict-tolerant fullcapture+16896newline/UCP/offset)
pin depths1/3/4/5/6/33/199, short/long atomic aliases, mixed ancestors, source/
modifiers/casing/UCP, erased prefixes, related/named/duplicate capture references/
tests, later groups/outside-root cache controls, newline/unsupported controls,
strict-tolerant UTF/work/cancellation/default budget/parser257/captures257/depth.
See PCRE-UTF-ATOMIC-STAR-PREFIX-DESIGN.md and PCRE-UTF-ATOMIC-STAR-PREFIX-PLAN.md.
Keep102 open for other placements/bracket predicates/group/alternative/negated/
property/call/DEFINE/condition bitmap availability; byte99/unsafe100 separate.
Bare atomic PLUS/unsupported placements pin ordinary consumption only. No complete
PCRE/release/Grep-readiness claim; publication/all known migrations precede CF removal.

## UTF atomic mandatory repeats without a star prerequisite

PcreUtfAtomicRepeatEntryReferenceTests pins native mandatory-set bitmap entry
through explicit atomic groups without requiring first-star ownership. Existing
Atomic(NoncapturingGroup) descent now shares ordinary group eligibility; its
local Skip remains unconditionally false. Final single-branch/nonempty/positive
ScalarSet/mandatory unbounded PLUS/LF/no CR/no predicates/capture/depth/CT/1000
guards and optional-source/range/casing/UCP/BlocksSkip rules remain. First-star
discovery, outside-root/non-atomic PLUS recovery, possessive modifier handling
and failed-proof reset are unchanged. No matcher/parser/emitter/Step/runtime
state/layout/instruction/frame/journal/span/pointer/snapshot/API/limit/workflow/
package/version/dependency/consumer/CommandFramework changes.
970 new cases/full14898 preserve all13928 baseline expectations/frozen35. Native
18788 bounded JIT checks (1892strict-tolerant fullcapture+16896newline/UCP/offset)
pin bare/captured PLUS, greedy/lazy/possessive/aliases, depths1/3/4/5/6/33/199,
atomic aliases/mixed ancestors, optional/erased/retained-zero prefixes, capture
dependencies/named/duplicate references/tests, source/casing/UCP/CR/predicate and
outside-root cache controls, strict-tolerant fragments/work/cancellation/default
budget/parser257/captures257/backtrack limits. See
PCRE-UTF-ATOMIC-REPEAT-ENTRY-DESIGN.md and PCRE-UTF-ATOMIC-REPEAT-ENTRY-PLAN.md.
Keep102 open for nullable prefix-group rejoin/other placements and broader
predicate/group/alternative/negated/property/call/DEFINE/condition bitmap availability.
Nullable prefix-only wrapper controls pin ordinary consumption only; initial
native probes confirm their continuation/rejoin gap needs a separate traversal
refactor. Byte99/unsafe100 separate; no complete PCRE/release/Grep readiness claim.
Publication/all known migrations precede CF removal; Grep last.

## UTF continuation after nullable prefix groups

PcreUtfNullableGroupEntryReferenceTests pins bitmap continuation from a fully
nullable prefix group into its enclosing sequence. Existing successful selector
paths remain; only failed proofs with proven first-star identity try a bounded
iterative fallback. Its compile-time stack stores AST branches/next indexes,
never VM frames or subject spans. Group descent requires one nonempty branch;
resume occurs only after existing optional-atom classifiers accept every operand
as nullable. Retained state carries across returns; at least one actual group
return and existing capture classification eligibility are required.
FindUtfCrLfLeadingNode now reports NextIndex/Nullable and accepts defaulted
startIndex/blocksSkip; budget failure differs from nullable success. Shared
mandatory-set validation requires unbounded positive ScalarSet PLUS, nonempty
valid scalar ranges, LF/no CR/no predicates. A compile-only joined-proof flag
lets start-class discovery consume that completed proof; generic traversal stays
for other paths. First-star depth/capture/atomic recovery and all VM instructions
remain; fallback PLUS Skip is explicitly false. CT/1000/source/casing/UCP/range/
capture/fragment/resource guards and failed-proof identity reset remain.
No matcher/parser/emitter/Step/runtime state/layout/instruction/frame/journal/
snapshot/span/pointer/API/limit/workflow/package/dependency/consumer/CF changes.
1408 new cases/full16306 preserve all14898 baseline expectations/frozen35. Native
19664 bounded JIT checks (2768strict-tolerant fullcapture+16896newline/UCP/offset)
pin group types/depths1/3/4/5/6/33/199, empty participating capture spans, nested/
sequential returns, source/modifiers/UCP/casing/erasures/dependencies, required/
CR-admitting/unsupported stops and strict-tolerant UTF/work/cancellation/default
budget/parser257/captures257/backtrack rejection. See
PCRE-UTF-NULLABLE-GROUP-ENTRY-DESIGN.md and PCRE-UTF-NULLABLE-GROUP-ENTRY-PLAN.md.
Keep102 open for unproven-first-star placements and broader predicate/group/
alternative/negated/property/call/DEFINE/condition bitmap availability; byte99/
unsafe100 separate. No complete PCRE/release/Grep readiness claim. Publication/
all known migrations precede CF removal; Grep last.

## UTF finite and empty nullable-prefix continuation

PcreUtfNullablePrefixEntryReferenceTests extends the bounded continuation proof
from #146 to finite optional, omitted-only and empty prefix groups. After the
old successful selector fails, the existing fallback runs only in UTF mode,
without requiring a proven first-star identity. Empty single-branch bodies are
accepted only there. A completed group return sets retained BlocksSkip even
when every body operand vanished, matching native early-fail group counting.
Later supported stars can join the bitmap proof without receiving new recovery
ownership. Existing first-star finder/depth/capture/atomic/cache rules and old
successful proofs remain unchanged; fallback PLUS Skip remains false.
Shared nullable-atom classification, mandatory positive unbounded LF/no-CR set
validation, actual group-return requirement, single-branch/capture eligibility,
CT/1000/source/casing/UCP/range/predicate guards and failed-proof reset remain.
No matcher/parser/emitter/Step/runtime state/layout/instruction/frame/journal/
snapshot/span/pointer/API/limit/workflow/package/dependency/consumer/CF changes.
950 new cases/full17256 preserve all16306 baseline expectations/frozen35. Native
18748 bounded JIT checks (1852strict-tolerant fullcapture+16896newline/UCP/offset)
pin finite bounds/modifiers, erased-only/empty aliases, group types/depths,
empty participating captures, sequential/nested returns, later stars, capture
dependencies, source/casing/UCP/CR/predicate/required/unsupported/final-set stops,
previous first-star ownership and strict/tolerant fragments/work/cancellation/
default budget/parser257/captures257/backtrack rejection. Tests-only run478
established823new failures/16433passes/0skips (all16306baseline+127controls),
zero build warnings/errors; expectations remain unchanged afterward. See
PCRE-UTF-NULLABLE-PREFIX-ENTRY-DESIGN.md and PCRE-UTF-NULLABLE-PREFIX-ENTRY-PLAN.md.
Keep102 open for broader predicate/group/alternative/negated/property/call/
DEFINE/condition bitmap availability and remaining placements. Byte99/unsafe100
separate; no complete PCRE/release/Grep readiness claim. Publication/all known
migrations precede CF removal; Grep last.

## UTF continuation after fully nullable alternatives

PcreUtfNullableAlternativeEntryReferenceTests extends the existing UTF fallback
to prefix groups whose every alternative passes the shared nullable-atom proof.
Compile-time continuation tuples now include group Body/NextBranch. Only after
all branches finish does the walker resume its parent; additional branches each
consume a proof visit. Alternative groups set retained BlocksSkip at entry,
per native early-fail counting, so branch stars gain no new recovery ownership.
A pending-alternative count prevents a nested return from accepting an early
mandatory set before sibling branches are checked. Empty branches qualify;
zero branches, required/unsupported/CR-admitting/predicate paths stop the proof.
Root alternatives and mixed mandatory/nullable or quantified groups remain
outside this extension. Captures across checked branches share the existing
eligibility classifier. Old successful selectors/first-star discovery/depth/
capture/atomic rules, LF/no-CR mandatory-set guard, actual group-return flag,
failed-proof reset, CT/1000 limits and fallback PLUS Skip=false are unchanged.
No matcher/parser/emitter/Step/runtime state/layout/instruction/frame/journal/
snapshot/span/pointer/API/limit/workflow/package/dependency/consumer/CF changes.
913 new cases/full18169 preserve all17256 baseline expectations/frozen35. Native
18671 bounded JIT checks (1772strict-tolerant fullcapture+16896newline/UCP/offset+
3empty-branch breadth) pin group types/depths/order, empty/erased/finite/star
branches, nested/sequential groups, branch-reset/related/named/duplicate captures,
later stars/old ownership and late-branch/nested early-set traps. Strict/tolerant
fragments/work/cancellation/default budget/parser257/capture257/backtrack/breadth
limits remain. Tests-only run480 established724new failures/17445passes/0skips
(all17256baseline+189controls), zero build warnings/errors. Tests/expectations
stay unchanged after RED. See PCRE-UTF-NULLABLE-ALTERNATIVE-ENTRY-DESIGN.md and
PCRE-UTF-NULLABLE-ALTERNATIVE-ENTRY-PLAN.md. Keep102 open for mixed/required/
quantified groups, predicates/negated/property/call/DEFINE/condition bitmap
availability and remaining placements. Byte99/unsafe100 separate. No complete
PCRE/release/Grep readiness claim; publication/all known migrations precede CF
removal; Grep last.

## UTF finite nullable-group repeats

PcreUtfFiniteGroupEntryReferenceTests extends the existing bounded UTF fallback
to finite Minimum0/Maximum>0 repeats of fully nullable prefix group bodies,
including possessive Atomic(Repeat), capture/noncapture/explicit atomic groups
and verified nullable alternatives. Repeat bodies set retained BlocksSkip;
inner stars gain no new recovery owner. A pending-repeat count blocks early
mandatory-set selection after nested returns until every body branch closes.
Continuation tuples retain RepeatMaximum/RepeatStartVisits across siblings.
One repeat/modifier visit plus the group visit is charged on entry; completed
body cost includes that group even when empty. Remaining copies are charged
using division before multiplication, including already charged nested repeats,
so no copy rescans, overflow or1000-visit bypass occurs. Over-budget proofs fail.
Old successful selectors/root/first-star/depth/capture/atomic rules, shared
nullable-atom/set/capture/actual-return/completed-proof guards, cancellation and
fallback PLUS Skip=false are unchanged. No matcher/parser/emitter/Step/runtime
state/layout/instruction/frame/journal/snapshot/span/pointer/API/limit/workflow/
package/dependency/consumer/CommandFramework changes.
1203 new cases/full19372 preserve all18169 baseline expectations/frozen35. Native
19248 bounded JIT checks (2348strict-tolerant fullcapture+16896newline/UCP/offset+
4copied-body captures) pin quantifier aliases/bounds/modifiers, group types/
depths/alternatives/nesting/sequential/source/dependency/recovery cases, greedy
empty versus lazy omitted captures, nested early-set/property traps and strict/
tolerant fragments. Work/exact/exhausted/observed/cancellation/default budget/
parser257/capture257/backtrack/copied bounds/expansion rejection remain. Large
copy controls use ordinary consumption, not native bitmap parity past proof1000.
Tests-only run482 established1061new failures/18311passes/0skips (all18169baseline+
142controls), zero build warnings/errors; tests/expectations unchanged afterward.
See PCRE-UTF-FINITE-GROUP-ENTRY-DESIGN.md and PCRE-UTF-FINITE-GROUP-ENTRY-PLAN.md.
Keep102 open for required-body/positive-minimum/unbounded group repeats, mixed
mandatory/nullable alternatives, root alternatives, predicate/negated/property/
call/DEFINE/condition bitmap availability and remaining placements. Byte99/
unsafe100 separate; no complete PCRE/release/Grep readiness claim. Publication/
all known migrations precede CF removal; Grep last.

## UTF positive finite nullable-group repeats

PcreUtfPositiveGroupEntryReferenceTests extends the existing finite fallback to
positive counts while preserving incoming recovery admission. Ordinary positive
bodies do not force BlocksSkip; the shared classifier still refuses unbounded
branch stars without a retained prefix or independently proven owner. Minimum0
keeps its old barrier; possessive Atomic(Repeat) and explicit atomic descent stop
native early-fail ownership. Existing alternative barriers remain. No repeated
AST child is assigned a new owner, avoiding accidental cache tags on every copy.
Required exact/ranged-count captures, last copied empties and byte spans remain
ordinary VM instructions. Repeated-child capture collection, pending repeat/
alternative guards, measured-copy division-before-multiplication/CT1000 budget,
set/capture/actual-return/completed-proof checks and fallback PLUS Skip=false are
unchanged. Old successful selectors/root/first-star finder/depth/capture policy
and all lowering/runtime code remain. No parser/emitter/matcher/Step/VM state/
layout/instruction/frame/journal/snapshot/span/pointer/API/limit/workflow/package/
dependency/consumer/CommandFramework changes.
1117 new cases/full20489 preserve all19372 baseline expectations/frozen35. Native
19076 bounded JIT checks (2176strict-tolerant fullcapture+16896newline/UCP/offset+
4copied captures) pin exact/ranged counts/modifiers, atomic/possessive/retained/
erased distinctions, group types/depths/alternatives/nesting/sequential/source/
dependencies, old root-owner and unproven positive-first-star consumed-run misses.
Strict/tolerant fragments/work/exact/exhausted/observer/cancellation/default
budget/parser257/capture257/backtrack/copy bounds/expansion rejection remain.
Large-copy controls use ordinary input, not bitmap parity beyond proof1000.
Tests-only run484 established991new failures/19498passes/0skips (all19372baseline+
126controls), zero build warnings/errors; tests/expectations unchanged afterward.
See PCRE-UTF-POSITIVE-GROUP-ENTRY-DESIGN.md and PCRE-UTF-POSITIVE-GROUP-ENTRY-PLAN.md.
Keep102 open for unproven repeated-group first-star ownership, required-body/
unbounded groups, mixed mandatory/nullable and root alternatives, predicate/
negated/property/call/DEFINE/condition bitmap availability and other placements.
Byte99/unsafe100 separate; no complete PCRE/release/Grep readiness claim.
Publication/all known migrations precede CF removal; Grep last.

## UTF nullable unbounded-group continuation

PcreUtfUnboundedGroupEntryReferenceTests extends the existing bounded fallback
to fully nullable group loops. Minimum0/1 loops block inner scalar early-fail
ownership; higher required copies preserve incoming admission unless an existing
atomic/possessive/alternative/retained barrier applies. Unproven first stars in
those earlier copies remain rejected. Group-star identity remains compile-time
metadata but group children never receive scalar recovery Skip eligibility;
primitive identity/depth/capture rules and lowering are unchanged.
Compile-time RepeatMaximum becomes RepeatCopies: finite paths still use Maximum,
unbounded paths use guarded Minimum+1 for required copies plus one loop body.
Minimum>=1000 stops before addition. The body is checked once, every branch must
be fully nullable, and existing group-inclusive division-before-multiplication
charges all bounded study copies/nested costs under1000 visits without rescans.
Pending bodies/alternatives still reject early mandatory sets. Shared set/
capture/actual-return/completed-proof/cancellation/reset guards and fallback PLUS
Skip=false remain. No parser/emitter/matcher/Step/VM state/layout/instruction/
frame/journal/snapshot/span/pointer/API/limit/workflow/package/dependency/consumer/
CommandFramework changes; existing loop progress/resource guards stay intact.
1443 new cases/full21932 preserve all20489 baseline expectations/frozen35. Native
19728 bounded JIT checks (2828strict-tolerant fullcapture+16896newline/UCP/offset+
4copied captures) pin loops/count aliases/modifiers/group types/depths/nesting/
sequential/alternatives/source/dependencies, lazy unset versus required/greedy
empty captures, primitive-owner versus group-loop and guarded versus unproven
required-copy cases. Strict/tolerant fragments/work/exact/exhausted/observer/
cancellation/default budget/parser257/capture257/backtrack/copy/expansion limits
remain. Large-copy controls use ordinary input, not bitmap parity past proof1000.
Tests-only run486 established1301new failures/20631passes/0skips (all20489baseline+
142controls), zero build warnings/errors; tests/expectations unchanged afterward.
See PCRE-UTF-UNBOUNDED-GROUP-ENTRY-DESIGN.md and PCRE-UTF-UNBOUNDED-GROUP-ENTRY-PLAN.md.
Keep102 open for required-body groups, mixed mandatory/nullable/root alternatives,
unproven earlier-copy owners, predicate/negated/property/call/DEFINE/condition
bitmap availability and remaining placements. Byte99/unsafe100 separate; no
complete PCRE/release/Grep readiness claim. Publication/all known migrations
precede CF removal; Grep last.

## UTF word-boundary prefixes

PcreUtfBoundaryPrefixReferenceTests pins boundary traversal along already
supported mandatory positive predicate-free LF/no-CR PLUS proofs and nullable
group continuation. Native set_start_bits and detect_early_fail ignore ASCII/UCP
word boundaries without raising the retained count. Both existing compiler
walkers now cross PcreByteWordBoundary with one bounded proof visit. BlocksSkip,
first-star identity/depth/capture policy, group-return barriers and primitive
owner placement remain; boundaries still execute as runtime assertions.
No parser/matcher/Step/VM state/journal/API/limit/workflow/package/
dependency/consumer/CommandFramework changes. Proof1000 and all resource limits
stay. Broader direct-class/assertion/root-alternative bitmap availability,
required-body groups, unproven required-copy owners and other #102 gates remain.
1414 new cases/full23346 preserve all21932 baseline expectations/frozen35.
Native12703 bounded JIT checks (2788 strict/tolerant full-capture,9792 six-newline/
UCP/every-offset,3long-prefix ordinary controls,120required-copy controls) plus80 initial probes pin ASCII/
UCP/restrictions, positive/negative/repeated boundaries, group/depth/capture/
reference/condition/branch-reset forms, scalar-owner versus group-loop guards,
strict/tolerant fragments/work/exact/exhausted/observer/cancellation/default
budget. Long-boundary controls do not claim bitmap parity beyond proof1000.
Tests-only run488 observed886 new-class failures/22460passes/0skips; all21932
baseline+528new controls passed, zero compiler warnings/errors. Tests unchanged.
See PCRE-UTF-BOUNDARY-PREFIX-DESIGN.md and PCRE-UTF-BOUNDARY-PREFIX-PLAN.md.
Keep102/unsafe100 open; no complete PCRE/release/Grep readiness claim.
Publication/all known migrations precede CF removal; Grep last.

Run489 exposed26 remaining new-class failures (23320passes/0skips; all21932baseline
pass). Extend first-star identity through ordinary first required group copies:
finite positive groups, or unbounded Minimum>=2. Keep Minimum1 unbounded loops
excluded; preserve atomic/possessive/alternative/depth/capture guards. A private
compiler helper consumes skip eligibility once, marking only the first emitted
scalar-star instruction while retaining AST identity for source proof. No new
runtime state; joined nullable-body/copy charging and all1000-visit guards stay.

## UTF boundary-prefixed starter classes

PcreUtfBoundaryStartClassReferenceTests extends the general UTF class proof to
leading word boundaries without requiring a selected PLUS set. Local AddBranches
skips only PcreByteWordBoundary nodes with one charged visit/cancellation per
node, then enqueues the first significant node. Boundary-only branches reject.
The selected-repeat path retains its existing nullable walker. Every alternative
must still exclude CR and at least one must admit LF; group/atomic/required-repeat,
unselected wide PLUS/exact-large/predicate/negated/source-shape guards remain.
No owner selection/marker placement/parser/emitter/matcher/Step/VM state/journal/
API/limit/workflow/package/dependency/consumer/CommandFramework changes.
179 new cases/full23525 preserve all23346baseline expectations/frozen35.
Native7887 bounded JIT checks (324strict/tolerant full-capture,7560six-newline/
UCP/every-offset,3long-prefix ordinary controls) plus48initial probes pin ASCII/
wide/restricted-UCP boundaries, wrappers/depths, alternatives/branch resets,
counts/capture dependencies/casing and CR-including/boundary-only/predicate/
anchor exclusions. Strict/tolerant fragments/work/exact/exhausted/observed/
cancellation/default budget remain; long controls claim no bitmap parity past1000.
Tests-only run491/macOS ARM64/net8 observed113new-class failures/23412passes/0skips,
all23346baseline+66new controls passing,0compiler warnings/errors. Tests unchanged.
See PCRE-UTF-BOUNDARY-START-CLASS-DESIGN.md and PCRE-UTF-BOUNDARY-START-CLASS-PLAN.md.
#102 stays open for broader optional/assertion/predicate/root-alternative/group/
required-copy placements and bitmap availability; unsafe100 remains separate.
No full PCRE/release/Grep readiness claim. Publication/all known migrations
precede CF removal; Grep last.

## UTF erased primitive prefixes before starter classes

PcreUtfErasedStartClassReferenceTests reuses IsUtfCrLfErasedRepeat in the general
class proof. Boundaries cost1, direct zero-count primitive/reference repeats
cost2, and atomic repeat wrappers add1. Charge before addition and cancel per
prefix; exhausted branches reject. Retained groups/assertions/calls and other
unproven prefixes keep their existing admission. Selected-repeat traversal,
owner selection/marker placement and every-alternative CR/LF checks stay.
No parser/emitter/matcher/Step/VM state/journal/API/limit/workflow/package/
dependency/consumer/CommandFramework changes; resource ceilings remain.
199 new cases/full23724 preserve all23525baseline expectations/frozen35.
Native7927 bounded JIT checks (364strict/tolerant full-capture,7560six-newline/
UCP/every-offset,3long ordinary controls) pin erased literal/type/set/property/
newline/reference forms, modifiers/casing/boundaries, wrappers/counts/alternatives/
branch resets/capture dependencies and retained/CR/property/anchor exclusions.
Strict/tolerant fragments/work/exact/exhausted/observed/cancellation/default
budget remain; long controls claim no bitmap parity past proof1000.
Tests-only run493/Linux x64/net8 observed147new-class failures/23577passes/0skips,
all23525baseline+52new controls passing,0compiler warnings/errors. Tests unchanged.
See PCRE-UTF-ERASED-START-CLASS-DESIGN.md and PCRE-UTF-ERASED-START-CLASS-PLAN.md.
PCRE-UTF-RECENT-PROGRESS.md preserves issue102's recent index at GitHub's body
limit; earlier history remains intact. Keep102/unsafe100 and broader bitmap/
optional/assertion/group/required-copy gates open; no full PCRE/release/Grep
readiness claim. Publication/all known migrations precede CF removal; Grep last.

## UTF retained zero-count prefixes before starter classes

PcreUtfRetainedZeroStartClassReferenceTests shares the existing retained-zero
group/assertion/capture-condition list through IsUtfCrLfRetainedZeroRepeat.
The PLUS nullable walker keeps retained=true, cost2/cost3 for an atomic group
child, plus any outer modifier charge. The general class proof accepts the same
list and treats skipped bodies as opaque, charging the atomic child separately.
Boundaries/erased prefixes,1000-visit pre-addition guard,cancellation,exhausted
branches,unsupported kinds,owner/marker and every-alternative CR/LF guards stay.
No parser/emitter/matcher/Step/VM state/journal/API/limit/workflow/package/
dependency/consumer/CommandFramework changes; resource ceilings unchanged.
223 new cases/full23947 preserve all23724baseline expectations/frozen35.
Native7975 bounded JIT checks (412strict/tolerant full-capture,7560six-newline/
UCP/every-offset,3long ordinary controls) pin zero groups/atomic groups/literal-
general lookahead/lookbehind/capture conditions, opaque property/reference/call
bodies, modifiers/boundaries/erased prefixes/wrappers/counts/alternatives/capture
dependencies and exclusions. Existing PLUS/primitive-owner controls remain.
Strict/tolerant fragments/work/exact/exhausted/observed/cancellation/default
budget stay; long controls claim no bitmap parity past1000.
Tests-only run495/Linux x64/net8 observed167new-class failures/23780passes/0skips,
all23724baseline+56new controls passing,0compiler warnings/errors. Tests unchanged.
See PCRE-UTF-RETAINED-ZERO-START-CLASS-DESIGN.md and
PCRE-UTF-RETAINED-ZERO-START-CLASS-PLAN.md; recent index gains one row.
Keep102/unsafe100 and broader bitmap/optional/assertion/group/required-copy
placements open; no full PCRE/release/Grep readiness claim. Publication/all known
migrations precede CF removal; Grep last.

## Finite optional prefixes and proven root stars before UTF starter classes

PcreUtfFiniteOptionalStartClassReferenceTests pins general starter-class entry
after supported finite optional primitives. HasUtfCrLfStartClass reuses the
nullable-prefix walker. Finite repeats keep existing atom,CR-exclusion,UCP and
source-sensitive caseless bitmap guards. Unbounded repeats require selected
PLUS or existing root-star identity plus the original BlocksSkip/identity rule.
The completed class proof is computed before emission and preserves first-star
identity/skip state without a selected PLUS; unproved stars still clear. The
existing owner classifier and one-emission marker helper are unchanged.
This removes the duplicate zero/boundary scan without changing its accepted
kinds,costs,retained state or exhausted-branch rejection. Every-alternative
CR exclusion/at-least-one-LF,required-repeat/wide PLUS/range guards and1000-visit
budget/cancellation remain. No arbitrary nullable-group continuation.
No parser/emitter helper/matcher/Step/VM/journal/API/limit/workflow/package/
dependency/consumer/CommandFramework changes; resource ceilings unchanged.
377 new cases/full24324 preserve all23947baseline expectations/frozen35.
Native6771 bounded JIT checks (560strict/tolerant full-capture,6048six-newline/
UCP/every-offset,3long ordinary,160root-star full-capture checks) cover optional
queries/bounded counts/literals/types/sets/caseless sources,mixed zero/boundary
prefixes,wrappers/required repeats/alternatives/dependencies and exclusions.
The additional PcreUtfRootStarStartClassReferenceTests pins root-star literal/
type/set greedy/lazy/possessive forms and consumed/unconsumed CRLF endpoints.
Existing PLUS owners,strict validation/fragments/caller boundaries,exact/
exhausted/observed work,cancellation/default budget stay. Long ordinary controls
claim no bitmap parity past1000.
Tests-only run497/Linux ARM64/net8 observed166new-class failures/24078passes/
0skips,all23947baseline+131controls passing,0compiler warnings/errors. Initial
finite fix run498 left6root-star controls failing/24238passes/0skips; no baseline
failures. Additional tests-only run499/Linux x64/net8 observed45failures
(6original+39new-star)/24279passes/0skips; all23947baseline+332controls passed,
0compiler warnings/errors. Both test files remain unchanged after their RED.
See PCRE-UTF-FINITE-OPTIONAL-START-CLASS-DESIGN.md and
PCRE-UTF-FINITE-OPTIONAL-START-CLASS-PLAN.md for the evidence-driven scope change;
recent index gains one row. Keep102/unsafe100 and broader optional/assertion/
predicate/group/required-copy/unselected wide PLUS/root-alternative bitmap
availability open. No full PCRE/release/Grep readiness claim. Publication/all
known migrations precede CF removal; Grep last.

## Bitmap-skipped assertions in UTF prefix proofs

PcreUtfSkippedAssertionPrefixReferenceTests pins negative lookahead and all
lookbehind prefixes,including literal/general,positive/negative,non-atomic and
variable forms. FindUtfCrLfLeadingNode admits exactly negative-or-behind
PcreByteLiteralAssertion,negative PcreByteLookahead and every PcreByteLookbehind,
returning cost2/retained=true. Native bitmap study skips these bodies; early-fail
descent stops there,so a following PLUS/star cannot gain its own recovery marker.
Assertion execution,captures-before-match,negative capture rollback,reverse
retries,fragment isolation and existing earlier root-star ownership stay intact.
Positive ahead and arbitrary optional assertion repeats are not added. Shared
source/atom/range/alternative/required-repeat/owner guards,pre-addition1000
budget,cancellation and exhausted branches remain. Body opacity applies only
to bitmap study. No parser/emitter helper/owner finder/matcher/Step/VM/frame/
journal/API/limit/workflow/package/dependency/consumer/CommandFramework changes;
resource ceilings unchanged.297new/full24621 preserve all24324baseline/frozen35.
Native6618 bounded JIT checks (560strict/tolerant full captures,6048six-newline/
UCP/every-offset,3long ordinary,7empty-assertion source probes) pin opaque body
properties/classes,capture dependencies,mixed prefixes,wrappers/counts/
alternatives,PLUS modifiers and earlier-vs-later stars. Empty positive behind
preserves the native recovery barrier; empty negative assertions still fail.
Strict validation/caller boundaries/tolerant fragments,exact/exhausted/observed
work,cancellation/default budget and long ordinary controls remain; no bitmap
parity claim beyond1000. Tests-only run501/Linux x64/net8 observed217new-class
failures/24404passes/0skips,all24324baseline+80controls passing,0compiler warnings/
errors. Tests unchanged after RED. See PCRE-UTF-SKIPPED-ASSERTION-PREFIX-DESIGN.md
and PCRE-UTF-SKIPPED-ASSERTION-PREFIX-PLAN.md; recent index gains one row.
Keep102/unsafe100 and broader positive-ahead/predicate/nullable-group/required-
copy/unselected wide PLUS/root-alternative bitmap availability open. No full
PCRE/release/Grep readiness claim. Publication/all known migrations precede
CommandFramework removal; Grep last.

## Repeated assertions and optional class bodies in UTF prefix proofs

PcreUtfRepeatedAssertionPrefixReferenceTests pins required exact copies of the
already-skipped negative-ahead/all-behind kinds. IsUtfCrLfSkippedAssertion shares
exactly #157's list;direct cost2/retained=true stays. Exact positive copies require
Maximum<=(1000-1)/2 before multiplication,then cost2*Maximum+1/retained=true.
Native investigation found BRAZERO can enter optional assertion bodies,so there
is no blanket optional admission. Four original captured-class cases remained
red after the exact-only fix;all original tests were preserved and310 focused
PcreUtfOptionalAssertionClassReferenceTests cases were added before correction.
Non-exact negative general ahead now also requires every body branch to reach a
mandatory positive,predicate-free supported scalar set through groups. A bounded
(Node?,Body?) compile-time worklist charges pops/branches before enqueue,cancels,
shares visits,and reuses IsUtfCrLfOptionalSet source/range/CR guards. Empty/
unknown/property/CR heads are rejected;body-tail/nullable-return inference is not
added. Non-exact proof uses the same prechecked Maximum and copy cost. Literals/
types/nullable bodies,optional behind/positive ahead and broader placements stay
outside that path. Exact bodies stay opaque; optional class bodies are studied.
Zero-count behavior,earlier root-star identity,later recovery barriers,all source/
alternative/required-repeat guards and pre-addition/wrapper charging remain.
No parser/emitter helper/owner finder/matcher/Step/VM/frame/journal/API/limit/
workflow/package/dependency/consumer/CommandFramework changes; ceilings unchanged.
752new/full25373 preserve all24621baseline/frozen35. Native13538 bounded JIT
checks =844initial captures+6048initial protocol+6long ordinary+592optional-class
captures+6048optional protocol. Counts/modifiers,assertion kinds,capture effects/
rollback/dependencies,opaque-vs-studied properties/classes,mixed prefixes,
wrappers/alternatives,PLUS/star endpoints,empty assertions and exclusions stay
pinned.498 exact copies fit the total proof fixture;499/500/1001-count and long
ordinary controls claim no bitmap parity beyond1000. Strict validation/tolerant
fragments/caller boundaries,exact/exhausted/observed work,cancellation/default
budget stay. The general optional default-budget fixture uses10000 CRLF pairs
under the unchanged1m ceiling; it performs more VM work than literal assertions.
RED503/macOS ARM64/net8:353new-class failures/24710passes/0skips,all24621baseline+
89controls pass. Initial fix504/Linux x64/net8:left4original failures/25059passes/
0skips,allbaseline+438controls pass. Added tests-only RED505/Linux x64/net8:
281failures(4original+277new)/25092passes/0skips,allbaseline+471controls pass.
Compiler warnings/errors0 throughout;both test blobs unchanged after their RED.
See PCRE-UTF-REPEATED-ASSERTION-PREFIX-DESIGN.md and
PCRE-UTF-REPEATED-ASSERTION-PREFIX-PLAN.md for scope corrections;recent index gains
one row. Keep102/unsafe100 and broader optional/positive-ahead/predicate/nullable-
group/required-copy/unselected wide PLUS/root-alternative availability open.
No full PCRE/release/Grep readiness claim. Publication/all known migrations before
CommandFramework removal;Grep last.

PR158 oracle correction: run511 isolated a native-only PCRE.NET JIT host crash
on Windows ARM64 for count500; the managed-only probe passes. Issue159 tracks
it. The three counted ordinary fallback/cancellation controls use the native
interpreter on every platform, preserving all inputs/assertions and full25373.
Bitmap-sensitive cases and long sequential controls retain JIT. Temporary
probes/tracing/serial scheduling are removed before final six-platform CI.
The original442-case blob changes only for this disclosed harness correction;
the focused310-case blob stays unchanged. See the repeated-assertion design.

## Optional negative-ahead mandatory bitmap heads (PR161)

After merged158/160, PcreUtfOptionalAssertionBitmapReferenceTests extends bounded
body study to mandatory literal/type and singleton folded-class heads. Collapsed
nonempty negative literal ahead decodes its first scalar through PcreUtf8.TryRead;
general negative ahead reuses the existing all-branch group/atomic worklist.
Valid mandatory literals exclude CR. Supported types are positive AsciiDigit,
AsciiWord,HorizontalSpace and negative AsciiSpace, preserving UCP/scoped ASCII
Kind distinctions. Positive predicate-free sets share existing bounded range/CR
validation; mandatory CHAR/CHARI/PROP-PT_CLIST study is distinct from optional
TYPEQUERY guards, so optional-atom source/caseless rules stay unchanged. Paired
root optional-atom controls pin that difference. No nullable/empty/repeated-head,
body-tail return,positive-ahead or lookbehind admission is added by this path.

Maximum>0/Minimum!=Maximum and Maximum<=(1000-1)/2 remain before study/multiply;
copy cost2*Maximum+1/retained=true,wrapper/pre-addition accounting,cancellation,
shared visits,root-star ownership and later PLUS/star barriers stay. Exact and
zero paths are unchanged. No parser/emitter/owner/matcher/Step/VM/frame/journal/
API/limit/workflow/contract/dependency/consumer/CommandFramework changes. Existing
budgets and35frozen blobs remain; no spans/pointers in long-lived VM state.

1092new/full26465 retain all25373baseline expectations. Native13324 qualified
Linux x64 JIT checks =2084full-capture+11208six-newline/UCP/every-offset+32resource
checks,with positive JIT sizes and bounded spans. Cases pin counts/modifiers,
collapsed/general/wrapped/alternative heads,captures/rollback/dependencies,
scoped UCP/ASCII/caseless pairs,source exceptions,PLUS/star endpoints,strict
validation/tolerant fragments/caller boundaries,work/cancellation/default budget.
Out-of-proof counted ordinary controls use native interpretation in CI for159;
no bitmap parity claim beyond1000. Broader nullable/repeated/body-prefix/property/
positive-ahead/behind availability stays open102; byte99/unsafe100/native159
remain separate. RED514/Linux x64/net8/9/10:555new-class failures/25507passes/
0skips each;all25373baseline+134newcontrols pass,0compilerwarnings/errors.
New test blobb1ae0e9f2ec8039c5e17f98b23e95b3ed8d2932f is frozen after RED.
See PCRE-UTF-OPTIONAL-ASSERTION-BITMAP-DESIGN.md and PLAN; final CI is in PR161.
No full PCRE/release/Grep readiness claim. Publication/all known migrations
precede CF removal; Grep last.

Scoped-group correction: the initial proof resolved545 of555 failures; the ten
remaining original controls are finite optional scoped noncapturing groups.
PcreUtfOptionalScopedGroupBitmapReferenceTests adds403 cases, preserving the
original689. Its RED516/Linux ARM64/net8/9/10 shows282failures/26183passes/0skips
each, all25373baseline+810newcontrols pass, compiler warnings/errors0. Ten
failures are original controls;272 are focused new cases. Focused test blob
a4326cbb69d0f3e4a8021d9a47adb8603b741e7d stays unchanged after RED.

Finite optional noncapturing single-branch/single-leaf groups share mandatory
head classification with assertion study. Minimum0/Maximum1..499 is checked
before study/multiply; group/body and range visits, copy/wrapper/pre-addition
charges and retained barriers remain bounded. Unsupported shapes are rejected
before charging fallback visits. No general nested/capturing/nullable/tail-return
or unbounded-group admission. Scoped/global casing, consumed prefixes and option
restoration, UCP/ASCII/property/CR controls, ownership and resource contracts are
pinned. Both new test blobs and all baseline expectations remain unchanged.

## Optional noncapturing group mandatory-body heads (PR162)

After merged161, finite optional noncapturing groups can prove all branches
through nested capture/noncapture/atomic wrappers to mandatory literal/type or
positive predicate-free range heads excluding CR. A mandatory head can have a
runtime tail; capture rollback and option restoration remain unchanged. The
existing assertion body worklist becomes HasUtfCrLfMandatoryBodyBitmap, shared
with group study. Preserve the old single-leaf path and exact charges. New body
proofs use a bounded local visit checkpoint, committed only on success so failed
nullable fallback keeps its prior budget. No nullable/repeated/prefix-assertion/
empty-branch/capturing-outer/unbounded-group or tail-return inference is added.

817new/full27282 preserve all26465baseline expectations and35 frozen blobs.
Native7552 qualified Linux x64 JIT checks =1584full-capture+5952six-newline/UCP/
every-offset+16resource; positive JIT sizes and bounded spans. Larger-count/deep-
capture exploratory calls are excluded. Cases pin modifiers, branches/wrappers,
tail failure, consumed-prefix captures, scoped casing/restoration, UCP/ASCII/
property/CR controls, dependencies, PLUS/star ownership, strict/tolerant UTF,
caller offsets, work/default budget/cancellation and count/expansion limits.
Ordinary count199/499/500 controls use native interpretation in CI for159.

Successful-build RED518/Linux x64/net8/9/10:545new-class failures/26737passes/
0skips each; all26465baseline+272newcontrols pass;0compilerwarnings/errors. Test
bloba35d52ccb3df261d4b58d3b22ac3711c4c86da2d remains frozen afterward. Existing
Maximum1..499 pre-study bound,2*Maximum+1/wrapper/pre-addition charges, retained
barriers,cancellation,source/caseless guards and root-star identity are unchanged.
No parser/emitter/owner/matcher/Step/VM/frame/journal/API/limit/workflow/contract/
dependency/consumer/CommandFramework changes or production reference fallback.
See PCRE-UTF-OPTIONAL-GROUP-HEADS-DESIGN.md and PLAN; exact final CI is in PR162.
Keep7/99/100/102/native159 and broader PCRE/release/Grep gates open. Publication/
all known migrations precede CommandFramework removal; Grep remains last.

## Optional capturing-group mandatory bitmap heads (PR163)

After merged162, finite optional numbered/named capturing outer groups share
HasUtfCrLfMandatoryBodyBitmap with noncapturing groups and negative assertions.
Only the body selector is broadened; every branch still needs a supported
mandatory literal/type/positive predicate-free range head excluding CR. Preserve
the prior noncapturing single-leaf path and charges. New capture paths use the
existing bounded success-only visit checkpoint so failed nullable fallback keeps
its prior budget. No capture numbering/journal/rollback/reference/condition/tail/
option-restoration runtime changes. Native CBRA study skips its capture operand
and follows the same bracket body; retained barriers preserve early-fail rules.

987new/full28269 preserve all27282baseline expectations and35frozen blobs. Native
7892 qualified Linux x64 JIT checks =1924full-capture+5952six-newline/UCP/every-
offset+16resource; positive JIT sizes and bounded spans. Cases pin numbered/
named/branch-reset aliases, absent/consumed/last-iteration/rollback captures, tail
failure, references/conditions, scoped options,UCP/ASCII/property/CR controls,
root/nested alternatives,PLUS/star/capture-ancestor ownership,strict/tolerant UTF,
caller offsets,work/default budget/cancellation and count/expansion limits.
Ordinary count199/499/500 controls use native interpretation in CI for159.

Successful-build RED520/Linux ARM64/net8/9/10:645new-class failures/27624passes/
0skips each; all27282baseline+342newcontrols pass;0compilerwarnings/errors. Test
blobae378b6c7de8354f9102e9d7e90f0f95e5cce45a remains frozen afterward. Existing
Minimum0/Maximum1..499 pre-study bound,2*Maximum+1/wrapper/pre-addition charges,
cancellation,source/caseless guards and all owner/limit paths remain unchanged.
No nullable/repeated/prefix-assertion/empty/unbounded/tail-return inference or new
walker. No parser/emitter/owner/matcher/Step/VM/frame/journal/API/limit/workflow/
contract/dependency/consumer/CommandFramework changes or production fallback.
See PCRE-UTF-OPTIONAL-CAPTURE-HEADS-DESIGN.md and PLAN; exact final CI is in PR163.
Keep7/99/100/102/native159 and broader PCRE/release/Grep gates open. Publication
and all known migrations precede CommandFramework removal; Grep remains last.

## Atomic brackets and skipped mandatory-body prefixes (PR164)

After merged163, explicit Atomic{NoncapturingGroup} outer bodies share the
existing mandatory-body bitmap proof, with one wrapper visit charged inside the
success-only checkpoint. Capture/noncapture and prior single-leaf paths stay
unchanged. Each body branch can cross only unquantified word boundaries and
IsUtfCrLfSkippedAssertion nodes before its mandatory head, charging1/2 with
cancellation and checked pre-addition. Branches ending after skipped nodes fail;
all branches still require supported literal/type/positive predicate-free ranges
excluding CR. Native ONCE/WORD_BOUNDARY/ASSERT_NOT/ASSERTBACK study motivates this
shared iterative scan; no recursive call to FindUtfCrLfLeadingNode is introduced.

1364new/full29633 preserve all28269baseline expectations and35frozen blobs. Native
10118 qualified Linux x64 JIT checks =2656full-capture+7440six-newline/UCP/every-
offset+22resource; positive JIT sizes,complete vectors,bounded spans. Cases pin
atomic tail commitment,behind captures before match start,skipped predicate
bodies,branches/rollback/references/conditions,UCP/ASCII,PLUS/star/ancestor owners,
strict/tolerant UTF,caller offsets,work/default budget/cancellation,count/expansion
and skipped-prefix budget controls. Ordinary count199/499/500 and prefix33/999/
1001 controls use native interpretation in CI for159; no bitmap claim beyond1000.

Successful-build RED522/Linux x64/net8/9/10:838new-class failures/28663passes/
0skips each; all28269baseline+394newcontrols pass;0compilerwarnings/errors. All2514
failure records and2514xUnit FAIL records are confined to the new class. Test blob
d0c25ed3fda55ad4c5a649c257dd728a3facce0f remains frozen afterward. Existing finite
copy bounds/costs,retained barriers,casing/source guards,owners and limits stay.
No optional/exact-repeat/general-positive-ahead/erased/nullable-return admission.
No parser/owner/matcher/Step/VM/frame/journal/API/limit/workflow/contract/
dependency/consumer/CommandFramework changes or production fallback. See
PCRE-UTF-BRACKET-PREFIX-BITMAP-DESIGN.md and PLAN; exact final CI is in PR164.
Keep7/99/100/102/native159 and broader PCRE/release/Grep gates open. Publication
and all known migrations precede CommandFramework removal; Grep remains last.

First implementation523 resolved837/838 failures; the remaining original dense
negative-assertion miss exhausted the unchanged default work limit. Focused132
cases add248native literal checks and preserve original1232cases. RED524/Linux
x64/net8/9/10:5failures/29628passes/0skips each, all28269baseline+1231original
controls+128focused controls pass. Failures: original dense miss, two scaled-work
checks (200031 steps for2000CRLFpairs), and two positive-ahead/literal guards.
Focused blob71fd17536ac8736b3f74dd65240a95f15b0ba746 is frozen after RED.

Native ASSERT study explicitly skips a positive ahead immediately followed by
a mandatory literal. Admit only that direct valid CR-excluding literal case,
not general positive-ahead/class continuations. For UTF greedy general atomic
negative ahead Minimum0/Maximum2..499, emit maximum-or-zero retry exits under
one Split while scheduling every original assertion copy. Negative guards restore
body state, so intermediate counts expose the same state to the tail. Bitmap/
owner study retains original AST bounds; positive/lazy/required/direct-literal/
byte-mode/other counts retain prior emission. This necessary emitter extension
fixes redundant tail retries without changing matcher/Step/frames/journal or
limits; all original copies remain, preserving huge expansion rejection.
The focused524 WindowsARM64 setup failed in PowerShell TaskbarJumpList CLR before
repository build, separate from native159; final source receives fresh full CI.


## Positive ahead across skipped bitmap prefixes (PR165)

After merged164, the native PCRE2 10.48 ASSERT/ASSERT_NA start-bitmap study
skips intervening lookarounds and word boundaries before a mandatory literal.
The existing optional-bracket body walker now proves that finite unquantified
prefix shape in one iterative branch scan. A branch must reach a direct valid
CR-excluding literal; unsupported or empty branches fail. Outer visits charge
each prefix exactly once, preserve the success-only checkpoint, cancellation,
count/proof bounds and retained skip/owner barriers. Runtime assertion and
capture semantics remain with the unchanged matcher.

Native JIT corpus:432 literal rows/864 strict+tolerant checks, noncapturing,
capturing and atomic optional brackets, greedy/lazy bounded copies, positive
ahead followed by negative ahead/behind/word boundaries, ASCII/UCP and CRLF/
ordinary subjects. Each native capture vector is complete and byte-bounded.
Successful-build RED526/Linux ARM64/net8/9/10:108 failures in the new class,
29957 passes and zero skips per framework; all29633 baseline and324 new
controls pass, compiler warnings/errors0. The test blob is frozen as
492b487337130e2514dd6a430133821ba49d8ce7. Full final CI is in PR165.

No parser, emitter, matcher, Step, frames, journal, API, limit, dependency,
workflow, consumer or CommandFramework changes. Full source-pinned
Compatibility and consumer migration remain separate; Grep stays last.


## Positive ahead before mandatory type bitmap heads (PR166)

After merged165, the optional-bracket bitmap proof accepts a positive ahead
across unquantified skipped lookarounds and boundaries only if the first
significant node is a direct CR-excluding literal or supported scalar type.
The existing HasUtfCrLfTypeBitmap admits only ASCII digit/word, horizontal
space and negated ASCII space; parser UCP distinctions and CR exclusion stay.
All-branch mandatory heads, wrapper/copy costs, cancellation, success-only
checkpoints, proof limits, retained owners and runtime assertion/capture
behavior remain unchanged. No arbitrary Unicode property, class, repeat,
unsupported or empty branch is admitted by this extension.

Native PCRE.NET1.6.0/PCRE2 10.48 JIT corpus:495 literal cases/990 strict+
tolerant full-capture checks, optional capture/noncapture/atomic wrappers,
horizontal space under ASCII/UCP, ASCII digit/word/nonspace, UCP-excluded
controls, skipped assertion/boundary prefixes and CRLF/ordinary subjects.
Positive native JIT sizes and complete bounded vectors were checked. Test
blob7b35b99cb7ce74c58c5792db76af657c2e1e7306 frozen after successful-
build RED528/Linux ARM64/net8/9/10:126 failures/new class only,30434 passes,
0 skips and0 compiler warnings/errors each; all30065 baseline+369 new controls
pass. Final exact source-head six-platform results are in PR166.

No parser/emitter/matcher/Step/VM/frame/journal/API/limit/workflow/dependency/
consumer/CommandFramework changes or production fallback. Broader PCRE,
release and Grep gates remain open; publication and all known migrations
precede CommandFramework removal. Grep remains last.


## Mandatory literal repeats after positive ahead (PR167)

After merged166, optional-bracket UTF bitmap study looks past positive ahead
when the next significant head is a mandatory direct CR-excluding literal
repeat, including possessive atomic repeat wrappers. Its iterative all-branch
proof uses the existing valid-scalar guard, bounded visits, cancellation and
success-only checkpoint. Zero-minimum repeats and unsupported heads stay on
their former paths. Parser, emitter, matcher, Step, frames, journal, API,
limits, dependencies, workflows and consumers are unchanged.

Pinned native PCRE2 10.48 JIT study showed LF in the first bitmap, CR absent,
and positive JIT sizes for plus, exact, bounded, lazy and possessive literal
repeats. Thirty-five full-capture fixtures / seventy strict+tolerant checks
cover optional capture/noncapture/atomic brackets and CRLF/ordinary subjects.
Successful-build tests-only RED [run532](https://github.com/uniblab/Icod.Expressions/actions/runs/41992936864), Linux ARM64/net8/9/10: seven failures confined to the new class,
30,588 passes, zero skips and compiler warnings/errors per framework. All
30,560 baseline cases and 28 new controls pass. Frozen test blob
2ba39ba6c05fe90f5b2a1222fcc723f4a93775f5 remains unchanged after RED.
Final six-platform results are in PR167. Keep issue102 and broader PCRE,
release and consumer migration gates open; Grep remains last.


## Mandatory direct classes after positive ahead (PR168)

After merged167, the optional-bracket UTF bitmap proof recognizes a direct
mandatory CR-excluding range class following positive lookahead. Its existing
class predicate rejects negated, predicate-bearing, malformed and CR-containing
sets. The speculative continuation check copies the visit count; the iterative
mandatory leaf charges each range once under the existing proof limit and
all-branch requirement. Parser, matcher, assertions, captures, resource limits,
workflows and consumers are unchanged.

Pinned native PCRE2 10.48 JIT reports LF present, CR absent and positive JIT
sizes for six optional-bracket class patterns, including capture/noncapture,
atomic, finite lazy, alternative and mixed-width ranges. Thirty-six full-capture
fixtures / seventy-two strict+tolerant checks cover CRLF and ordinary inputs.
Successful-build test-only RED [run534](https://github.com/uniblab/Icod.Expressions/actions/runs/42002406924), Linux ARM64/net8/9/10: six failures in this new class only,
30,625 passes, zero skips and compiler warnings/errors per framework. All
30,595 baseline tests plus 30 new controls passed. Frozen test blob
6470414d4b34898d85c48ef3932779bf61aa950e is unchanged after RED.
Final six-platform results are in PR168. Issue102 and broader PCRE, release
and consumer migration gates remain open; Grep remains last.
