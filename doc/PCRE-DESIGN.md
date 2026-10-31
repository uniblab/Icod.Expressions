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
