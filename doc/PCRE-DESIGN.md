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
| Decimal and numeric `\gN`/`\g{N}` backreferences | Absolute and signed relative forms resolve to groups 1–256 in 8-bit non-UTF and UTF-8 modes; caseful by default, ASCII byte folding or pinned Unicode17 simple-scalar folding with `(?i)` or the internal compile option. Captures keep source-byte spans, and UTF references consume subject scalar widths. Unique named declarations and references alias the same numbered slots. Bare decimal runs follow default PCRE2 reference/octal disambiguation using the current syntactic capture count; leading 8/9 permits forward references. Numeric g braces accept edge space/tab. See `PCRE-NUMERIC-ESCAPES-DESIGN.md`. See `PCRE-ABSOLUTE-G-REFERENCE-DESIGN.md` and `PCRE-RELATIVE-G-REFERENCE-DESIGN.md`. |
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
| `\G` search start and `\K` reported start | G retains caller start inside lookahead; K remains invalid in brackets; assertion nesting requires caller AllowLookaroundBackslashK. Opt-in preserves signed end-minus-start spans, including reversed endpoints and reports before the requested offset. `\G` tests the original requested byte offset; `\K` resets group-zero and whole-match reporting while preserving earlier numbered captures. Capture-start slot zero is journaled for rollback across branches, atomic sections and repetitions. See `PCRE-MATCH-POSITIONS-DESIGN.md`. |
| Default `^` and `$` | `^` tests absolute start; `$` tests absolute end or before one final LF. Other option-dependent behavior remains pending. |
| ASCII byte-mode `\b` and `\B` word boundaries | Implemented internally using the same non-UCP ASCII `\w` definition. Bracket `\b` remains backspace; bracket `\B` is unsupported. |
| Leading, mid-pattern and scoped `i`, `m`, `n`, `s`, `U`, `J`, `r`, `x`/`xx`, `aD`/`aS`/`aW`/`aP`/`aT`/bare `a` options | Combinations and setting/unsetting compile at source locations, propagate to later alternatives and restore at every group boundary. Comparison instructions retain local casing; dot/anchors retain local DotAll/multiline. n suppresses unnamed capture allocation while named groups still capture. U reverses default repetition greediness; a question suffix reverses it again, while possessive repeats are always greedy. Quantifiers around groups use the restored enclosing mode. x ignores fixed Pattern_White_Space and line comments; xx additionally ignores literal SP/HT in brackets. Ordinary `(?#...)` comments are independent of x. Lexical skipping preserves source indexes, quotes, token boundaries and fixed assertion bodies. Explicit x downgrades xx; only adjacent xx selects it. ASCII restriction tokens share the scanner and a five-bit group snapshot; aP sets POSIX plus POSIX-digit, aT sets POSIX-digit, and bare a sets all bits. Unset wins per bit. Predicates and b/B retain their local policy. Local r blocks ASCII/non-ASCII fold crossings while preserving ordinary Unicode case pairs and explicit property semantics; UCP POSIX lower/upper retain Ll/Lu. Empty and reset headers, including empty hyphen sections, preserve U/J/ASCII restrictions while reset clears i/m/n/r/s/x/xx. J permits later duplicate declarations, restores at group boundaries and survives reset; all name references resolve globally and commit to the first participating declaration. See `PCRE-DUPLICATE-NAMES-DESIGN.md`. See `PCRE-OPTION-SCOPE-DESIGN.md`, `PCRE-CAPTURE-REPEAT-OPTIONS-DESIGN.md`, `PCRE-EXTENDED-OPTIONS-DESIGN.md`, `PCRE-RESET-OPTIONS-DESIGN.md`, `PCRE-ASCII-DIGIT-OPTIONS-DESIGN.md`, `PCRE-ASCII-RESTRICTIONS-DESIGN.md` and `PCRE-CASELESS-RESTRICTIONS-DESIGN.md`. |
| Internal `PcreByteOptions.Caseless` | Caller-supplied caseless compile option uses ASCII non-UCP byte behavior or shared Unicode17 simple-case behavior for byte-UCP/UTF literals, properties, backreferences and fixed literal assertions; leading options preserve or explicitly unset it. Grep `-i` integration remains pending. |
| Internal `PcreByteOptions.Utf8` | Opt-in strict UTF-8 scalar literals and dot preserve source-byte spans, with invalid pattern/subject diagnostics and character-boundary search offsets. Ordinary UTF brackets combine literal ranges, supported shorthands/properties and all fourteen POSIX predicates in caseful or provisional caseless mode. Caseless ranges are compiled to folded intervals; wide ranges can add compilation work. Nonliteral lookarounds remain explicitly unsupported. |
| Internal `PcreByteOptions.MatchInvalidUtf` | Opt-in PCRE2-style valid UTF-8 fragments in malformed subjects, implying UTF-8 and preserving whole-source byte spans. Consuming atoms and backreferences cannot cross a malformed sequence; fragment edges govern word boundaries while `\\z`/`\\Z` require the whole-subject end. Pinned PCRE2 10.48 exhaustive offset conformance remains pending. |
| Byte/UTF-8 `\p`/`\P` general categories | The abbreviated C/L/M/N/P/S/Z groups and two-letter Unicode general categories (including `Lc`/`L&`) work as scalar atoms and bracket members, with complements and original byte spans. Loose name matching and `gc:`/`General_Category=` forms are supported. Category data is pinned to Unicode17.0.0, matching PCRE2 10.48, independently of .NET/OS data. Default simple-case tables are also pinned; additional Unicode modes remain separate. Any/Xan/Xps/Xsp/Xwd/Xuc now share scalar predicates with exact complements and local-mode independence; source operand provenance controls UTF class CR/LF search metadata before folding. The existing gc/category-prefix aliases remain a library extension restricted to categories. Script/scx properties now use pinned Unicode17 data; binary and other properties remain unsupported. See `PCRE-SPECIAL-PROPERTIES-DESIGN.md`. |
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
| Binary and remaining UCP property names and additional Unicode modes, pinned invalid-UTF offset conformance, broader backreference syntax, script runs, backtracking verbs and broader PCRE2 syntax | Pending separate implementation and conformance fixtures. |

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


## Mandatory range-class repeats after positive ahead (PR169)

After merged168, the optional-bracket UTF bitmap proof admits positive ahead
before mandatory CR-excluding range-class repeats, including possessive atomic
wrappers. It reuses the established range predicate and bounds; speculative
pre-scan checks use a copied visit count, while the mandatory leaf charges
each range once. Zero-minimum, negated, predicate-bearing, CR-containing and
unproven operands remain on their former paths. Parser, matcher, assertions,
captures, APIs, limits, workflows and consumers are unchanged.

Pinned native PCRE2 10.48 JIT reports LF present, CR absent and positive JIT
sizes for seven optional bracket patterns with plus, exact, lazy, possessive
and mixed-width class repeats. Forty-two full-capture fixtures / eighty-four
strict+tolerant checks cover CRLF and ordinary subjects. Successful-build
[test-only RED536](https://github.com/uniblab/Icod.Expressions/actions/runs/42129649283), macOS ARM64/net8/9/10: seven failures in the new class only,
30,666 passes, zero skips and compiler warnings/errors per framework. All
30,631 baseline tests plus 35 new controls passed. Immutable test blob
7fa5a65a5f7df3c8617d0c80325480cb12f4031a remains unchanged after RED.
Final six-platform results are in PR169. Issue102 and broader PCRE, release
and consumer migration gates remain open; Grep remains last.


## Mandatory supported-type repeats after positive ahead (PR170)

After merged169, the optional-bracket UTF bitmap proof admits positive ahead
before mandatory repeats of types already accepted as direct bitmap heads:
ASCII digit/word, horizontal space and negated ASCII space. It unwraps the
possessive atomic repeat, requires positive minimum and maximum, and delegates
to the existing CR-excluding type predicate. Zero-minimum, UCP Unicode
properties, unsupported negated types and nested operands retain their former
paths. Existing all-branch proof, visit bounds, cancellation and runtime
matcher/capture behavior remain unchanged.

Pinned native PCRE2 10.48 JIT reports LF present, CR absent and positive JIT
sizes for eight optional-bracket patterns with plus, exact, lazy, possessive
and finite supported-type repeats. Seventy-two full-capture fixtures / one
hundred forty-four strict+tolerant checks cover CRLF and ordinary subjects.
Successful-build [test-only RED538](https://github.com/uniblab/Icod.Expressions/actions/runs/42140939691), Linux ARM64/net8/9/10: eight failures in the new class only,
30,737 passes, zero skips and compiler warnings/errors per framework. All
30,673 baseline tests plus 64 new controls passed. Immutable test blob
7e5071a1b728c50a5c8cd22ae1672a7606d3e126 remains unchanged after RED.
Final six-platform results are in PR170. Issue102 and broader PCRE, release
and consumer migration gates remain open; Grep remains last.


## Direct mandatory groups after positive ahead (PR171)

After merged170, the optional-bracket UTF bitmap proof admits positive ahead
before unquantified capture, noncapture or explicit atomic noncapture groups
when every group branch immediately begins with a proven CR-excluding direct
literal, supported type or range class. A bounded, cancellable pre-scan uses
a copied visit count; the existing iterative all-branch body traversal owns
the actual visits and rejects empty, unsupported, nested or repeated group
heads. Runtime assertions/captures, parser, VM, API, limits, workflows and
consumers are unchanged.

Pinned native PCRE2 10.48 JIT reports LF present, CR absent and positive JIT
sizes for five group patterns. Thirty full-capture fixtures / sixty strict+
tolerant checks cover CRLF and ordinary subjects. Successful-build
[test-only RED540](https://github.com/uniblab/Icod.Expressions/actions/runs/42164704456), macOS ARM64/net8/9/10: five failures in the new class only,
30,770 passes, zero skips and compiler warnings/errors per framework. All
30,745 baseline tests plus 25 new controls passed. Immutable test blob
0a3eb5dfaf6a78ecd2f10d636c455b9b933c619e remains unchanged after RED.
Final six-platform results are in PR171. Issue102 and broader PCRE, release
and consumer migration gates remain open; Grep remains last.


## Mandatory group repeat heads after positive ahead (PR172)

After merged171, the optional-bracket UTF bitmap proof admits a positive ahead
before an unquantified capture, noncapture or atomic noncapture group if every
nonempty branch immediately starts with an already proven positive-minimum
literal, supported range-class or supported type repeat. Possessive repeat
wrappers use the same proofs. The cancellable, bounded group pre-scan operates
on a copy of visits; iterative body traversal owns the actual visit budget.
Nullable, unsupported, nested and repeated group heads stay conservative.
Runtime assertions/captures, parser, VM, API, limits and consumers are unchanged.

Pinned PCRE2 10.48 JIT has LF present, CR absent and positive JIT sizes for
six group repeat patterns. Thirty-six full-capture fixtures / seventy-two
strict+tolerant checks cover CRLF and ordinary subjects. Successful-build
[test-only RED542](https://github.com/uniblab/Icod.Expressions/actions/runs/42168129435), Linux ARM64/net8/9/10: six failures in the new class only,
30,805 passes and zero skips per framework. All 30,775 baseline cases and
30 new controls passed. Immutable test blob
1937ffb9da1664c0dbfb167e6f3a6677d87e85d4 remains unchanged after RED.
Final six-platform results are in PR172. Issue102 and broader PCRE, release
and consumer migration gates remain open; Grep remains last.


## Prefixed mandatory groups after positive ahead (PR173)

After merged172, the optional-bracket UTF bitmap proof admits a positive ahead
before unquantified capture, noncapture or atomic noncapture groups whose
branches start with bounded unquantified word boundaries or general/lowered
literal lookaheads and then a proven mandatory literal, range-class, type or positive-
minimum repeat head. The cancellable pre-scan charges a copied visit count;
iterative all-branch body traversal owns actual visits and assertion/capture
execution. Empty, nullable, nested and unsupported heads stay conservative.
The parser, VM, public API, limits and production consumers are unchanged.

Pinned PCRE2 10.48 JIT reports LF present, CR absent and positive JIT sizes
for seven group prefix patterns. Forty-two full-capture fixtures / eighty-four
strict+tolerant checks cover CRLF and ordinary subjects. Successful-build
[test-only RED544](https://github.com/uniblab/Icod.Expressions/actions/runs/42180839711), macOS ARM64/net8/9/10: seven failures in the new class only,
30,846 passes, zero skips and zero compiler warnings/errors per framework.
All 30,811 baseline cases plus 35 new controls passed. Immutable test blob
712b9f523203fad2d37c59dd7eca3ac75d163124 remains unchanged after RED.
Final six-platform results are in PR173. Issue102 and broader PCRE, release
and consumer migration gates remain open; Grep remains last.


## Lookbehind-prefixed mandatory groups after positive ahead (PR174)

After merged173, the optional-bracket UTF bitmap proof admits positive ahead
before unquantified capture, noncapture or atomic noncapture groups whose
branches start with bounded, unquantified general or lowered literal positive/
negative lookbehind and then a proven mandatory literal, range class,
supported type or positive-minimum repeat head. The cancellable pre-scan
charges a copied visit count; iterative all-branch traversal owns actual
visits and assertion/capture execution. Empty, nullable, nested and
unsupported heads remain conservative. Parser, VM, API, limits and consumers
are unchanged.

Pinned PCRE2 10.48 JIT reports LF present, CR absent and positive JIT sizes
for seven group lookbehind patterns. Forty-nine full-capture fixtures /
ninety-eight strict+tolerant checks cover CRLF and ordinary subjects.
Successful-build [test-only RED547](https://github.com/uniblab/Icod.Expressions/actions/runs/42219733083), Linux x64/net8/9/10: seven failures in the new class only,
30,895 passes, zero skips and zero compiler warnings/errors per framework.
All 30,853 baseline cases plus 42 new controls passed. Immutable test blob
d74d1490f9e4dd1531d199577acab5ec5171d151 remains unchanged after RED.
Final six-platform results are in PR174. Issue102 and broader PCRE, release
and consumer migration gates remain open; Grep remains last.


## One nested mandatory group after positive ahead (PR175)

After merged174, the optional-bracket UTF bitmap proof admits positive ahead
before an unquantified group whose first significant head is one nested
unquantified capture, noncapture or atomic noncapture group. Every inner
branch must immediately reach an existing proven literal, range-class,
supported type or positive-minimum repeat head after supported unquantified
assertion/boundary prefixes. The cancellable bounded pre-scan permits only
one nested helper call and charges a copied visit count; iterative all-
branch traversal owns actual visits and runtime assertions/captures. Empty,
nullable, repeated, deeper nested and unsupported heads stay conservative.
Parser, VM, API, limits and consumers are unchanged.

Pinned PCRE2 10.48 JIT reports LF present, CR absent and positive JIT sizes
for six nested group patterns. Thirty-six full-capture fixtures / seventy-
two strict+tolerant checks cover CRLF and ordinary subjects. Successful-build
[test-only RED549](https://github.com/uniblab/Icod.Expressions/actions/runs/42227786853), Linux ARM64/net8/9/10: five failures in the new class only,
30,933 passes, zero skips and zero compiler warnings/errors per framework.
All 30,902 baseline cases plus 31 new controls passed. Immutable test blob
5f9717a09b9fa6810e3e128d9bc3d0f6de8edd33 remains unchanged after RED.
Final six-platform results are in PR175. Issue102 and broader PCRE, release
and consumer migration gates remain open; Grep remains last.


## Bounded deep mandatory groups after positive ahead (PR176)

After merged175, the optional-bracket UTF bitmap proof admits positive ahead
before chains of unquantified capture, noncapture or atomic noncapture group
heads. Every branch must reach an existing proven mandatory literal, range
class, supported type or positive-minimum repeat head after supported
unquantified assertion/boundary prefixes. An explicit pending stack replaces
the depth-one recursive helper. Group, branch and prefix visits charge the
same 1,000-visit budget with cancellation; the speculative pre-scan retains
a copied visit count and iterative all-branch traversal owns actual visits.
Empty, nullable, repeated and unsupported group heads stay conservative.
Parser, VM, API, limits and consumers are unchanged.

Pinned PCRE2 10.48 JIT reports LF present, CR absent and positive JIT sizes
for noncapture chains through depth 128 and capture chains through depth 8.
Twenty-seven full-capture fixtures / fifty-four strict+tolerant checks cover
CRLF and ordinary subjects. Successful-build [test-only RED551](https://github.com/uniblab/Icod.Expressions/actions/runs/42233238070),
Linux x64/net8/9/10: seven failures in the new class only, 30,958 passes,
zero skips and zero compiler warnings/errors per framework. All 30,938
baseline cases plus 20 new controls passed. Immutable test blob
55dfd03c0bbfe3dd0c11ccde921f2264c233180f remains unchanged after RED.
Final six-platform results are in PR176. Issue102 and broader PCRE, release
and consumer migration gates remain open; Grep remains last.


## Mandatory repeated group heads after positive ahead (PR177)

After merged176, the optional-bracket UTF bitmap proof admits positive ahead
before positive-minimum repeats of capture, noncapture or atomic noncapture
groups. Every repeated child branch must reach a proven mandatory CR-excluding
literal, range class, supported type or positive-minimum repeat head after
supported unquantified assertion/boundary prefixes. An explicit stack visits
nested group heads; the repeat, group, branch and prefix checks charge the
existing 1,000-visit budget with cancellation. The speculative pre-scan uses
a copy of visits, and the actual proof applies the same guard. Zero-minimum,
empty, nullable, CR-admitting and unsupported repeated heads remain
conservative. Runtime captures/assertions, parser, API, limits and consumers
are unchanged.

Pinned PCRE2 10.48 JIT reports LF present, CR absent and positive JIT size
for six repeated-group shapes. Eighteen full-capture fixtures / thirty-six
strict+tolerant checks cover CRLF and ordinary subjects. Successful-build
[test-only RED554](https://github.com/uniblab/Icod.Expressions/actions/runs/42314672122),
macOS ARM64/net8/9/10: six failures in the new class only, 30,977 passes,
zero skips and zero compiler warnings/errors per framework. All 30,965
baseline cases plus 12 new controls passed. The earlier syntax-invalid run553
did not count as test RED. Immutable test blob
7df5678a3f3de64d5c72bc69d37514634a68bbab remains unchanged after RED.
Final six-platform results are in PR177. Issue102 and broader PCRE, release
and consumer migration gates remain open; Grep remains last.


## Literal positive ahead before zero-minimum group repeats (PR178)

After merged177, the optional-bracket UTF bitmap proof admits a positive
single-byte ASCII literal lookahead immediately followed by a zero-minimum,
positive-maximum repeat of a capture, noncapture or atomic noncapture group,
optionally possessive. The assertion itself excludes CR from entry even when
the group repeat consumes nothing. Its child branches must still reach proven
mandatory CR-excluding heads. A copied bounded visit count charges the
assertion, repeat, group and branch proof with cancellation; runtime assertion
and capture execution remain unchanged. Other nullable continuations,
unsupported heads and wider assertions remain conservative. Parser, VM, API,
limits and consumers are unchanged.

Pinned PCRE2 10.48 JIT reports LF present, CR absent and positive JIT sizes
for five zero-minimum repeated-group shapes. Fifteen full-capture fixtures /
thirty strict+tolerant checks cover CRLF and ordinary subjects. Successful-
build [test-only RED556](https://github.com/uniblab/Icod.Expressions/actions/runs/42322153350),
Linux ARM64/net8/9/10: five failures in the new class only, 30,993 passes,
zero skips and zero compiler warnings/errors per framework. All 30,983
baseline cases plus 10 new controls passed. Immutable test blob
476fde060f4d6812075a2987c682cb2fde6c203c remains unchanged after RED.
Final six-platform results are in PR178. Issue102 and broader PCRE, release
and consumer migration gates remain open; Grep remains last.


## Word boundary before zero-minimum group repeat (PR179)

After merged178, the optional-bracket UTF bitmap proof admits one
unquantified word boundary between a positive single-byte ASCII literal
lookahead and a zero-minimum, positive-maximum repeat of a capture,
noncapture or atomic noncapture group, optionally possessive. The assertion
excludes CR from entry even if the group repeat consumes nothing. Every
repeated child branch still reaches a proven mandatory CR-excluding head.
The extra boundary charges one visit in the same 1,000-visit budget with
cancellation. Other intermediate assertions, multiple boundaries and
unsupported heads remain conservative. Runtime assertion, boundary,
repeat and capture execution, parser, API, limits and consumers are unchanged.

Pinned PCRE2 10.48 JIT reports LF present, CR absent and positive JIT sizes
for five boundary-prefixed zero-minimum repeated-group shapes. Fifteen
full-capture fixtures / thirty strict+tolerant checks cover CRLF and
ordinary subjects. Successful-build [test-only RED558](https://github.com/uniblab/Icod.Expressions/actions/runs/42328717262),
Linux x64/net8/9/10: five failures in the new class only, 31,008 passes,
zero skips and zero compiler warnings/errors per framework. All 30,998
baseline cases plus 10 new controls passed. Immutable test blob
d016b78205d40b08809852adb0dee445b5bfd19e remains unchanged after RED.
Final six-platform results are in PR179. Issue102 and broader PCRE, release
and consumer migration gates remain open; Grep remains last.

## Two boundaries before zero-minimum group repeat (PR180)

After merged179, the optional-bracket UTF bitmap proof accepts two
unquantified word-boundary nodes between a positive single-byte ASCII
literal lookahead and a zero-minimum, positive-maximum group repeat.
The lookahead excludes CR even if the repeat consumes nothing; every
repeated child branch still requires a mandatory CR-excluding head.
Both boundaries charge visits in the existing 1,000-visit budget.
Other intermediate nodes or longer chains remain conservative; runtime,
parser, API, limits and consumers are unchanged.

Pinned PCRE2 10.48 JIT retains LF and excludes CR for `\b\b` and
`\B\B`, across five group shapes, with positive JIT sizes. Thirty native
full-capture fixtures / sixty strict+tolerant checks cover CRLF and
ordinary subjects. Successful-build [test-only RED560](https://github.com/uniblab/Icod.Expressions/actions/runs/42336029188),
Linux x64/net8/9/10: ten failures in the new CRLF cases only, 31,033
passes, zero skips and zero compiler warnings/errors per framework.
All 31,013 baseline cases plus twenty new controls passed. Immutable
test blob `2e58ca6546e3a7a78a5f31179a3966df0d08d506` remains unchanged.
Final six-platform results are in PR180. Issue102 and broader PCRE,
release and consumer migration gates remain open; Grep remains last.

## Negative literal assertion before zero-minimum group repeat (PR181)

After merged180, the optional-bracket UTF bitmap proof accepts exactly
one negative single-byte ASCII literal assertion, ahead or behind,
between a positive single-byte ASCII literal lookahead and a
zero-minimum, positive-maximum group repeat. The positive assertion
excludes CR even when the repeat consumes nothing. Every repeated child
branch still requires a proven mandatory CR-excluding head. Charge
two visits for the negative assertion under the same 1,000-visit
budget. Other intermediate nodes and longer chains remain conservative;
runtime, parser, API, limits and consumers are unchanged.

Pinned PCRE2 10.48 JIT retains LF and excludes CR for `(?!Z)` and
`(?<!Z)` between the positive lookahead and each of five nullable
group shapes, with positive JIT sizes. Thirty native full-capture
fixtures / sixty strict+tolerant checks cover CRLF and ordinary
subjects. Successful-build [test-only RED562](https://github.com/uniblab/Icod.Expressions/actions/runs/42341748138),
Linux x64/net8/9/10: ten failures in the new CRLF cases only, 31,063
passes, zero skips and zero compiler warnings/errors per framework.
All 31,043 baseline cases plus twenty new controls passed. Immutable
test blob `c00998a881302dad9dabae110adf2a13c871a238` remains unchanged.
Final six-platform results are in PR181. Issue102 and broader PCRE,
release and consumer migration gates remain open; Grep remains last.

## Mixed assertion prefixes before zero-minimum group repeat (PR182)

After merged181, the optional-bracket UTF bitmap proof examines at most
two zero-width nodes between a positive single-byte ASCII literal
lookahead and a zero-minimum, positive-maximum group repeat. Only a
word boundary/nonboundary or a negative single-byte ASCII literal
assertion, ahead or behind, is admitted in each position. Boundaries
cost one visit; negative literal assertions cost two. The positive
lookahead excludes CR even if the repeat consumes nothing, and every
repeated child branch needs a mandatory CR-excluding head under the
existing 1,000-visit budget with cancellation. Other nodes and longer
chains remain conservative. Runtime, parser, API, limits and consumers
are unchanged.

Pinned PCRE2 10.48 JIT retains LF, excludes CR and reports positive JIT
sizes for ten two-node prefixes across five repeated-group shapes.
One hundred fifty native full-capture fixtures / three hundred strict+
tolerant checks cover CRLF and ordinary subjects. Successful-build
[test-only RED564](https://github.com/uniblab/Icod.Expressions/actions/runs/42347478609),
macOS ARM64/net8/9/10: fifty failures in the new CRLF cases only,
31,173 passes, zero skips and zero compiler warnings/errors per framework.
All 31,073 baseline cases plus 100 new controls passed. Immutable test
blob `6162eff08a4234cf1bcf5661939690b2fd323630` remains unchanged.
Final six-platform results are in PR182. Issue102 and broader PCRE,
release and consumer migration gates remain open; Grep remains last.

## Positive middle assertions before zero-minimum group repeats (PR183)

After merged182, the optional-bracket UTF bitmap proof admits one
positive single-byte ASCII literal assertion, ahead or behind, in
either of at most two zero-width middle positions before a zero-minimum,
positive-maximum group repeat. The existing negative literal assertions
and word boundaries remain supported. Each literal assertion charges
two visits and each boundary one under the 1,000-visit budget with
cancellation. The leading positive literal lookahead excludes CR even
when the repeat consumes nothing, and every repeated child branch
requires a mandatory CR-excluding head. Other nodes or longer chains
remain conservative; runtime, parser, API, limits and consumers are
unchanged.

Pinned PCRE2 10.48 JIT retains LF, excludes CR and reports positive
JIT sizes for twelve assertion/boundary prefixes across five group
repeat shapes. One hundred eighty native full-capture fixtures /
360 strict+tolerant checks cover CRLF and ordinary subjects.
Successful-build [test-only RED566](https://github.com/uniblab/Icod.Expressions/actions/runs/42352825688),
macOS ARM64/net8/9/10: sixty failures in the new CRLF cases only,
31,343 passes, zero skips and zero compiler warnings/errors per
framework. All 31,223 baseline cases plus 120 new controls passed.
Immutable test blob `703159731160565b7f76f8f68ddd91f79022fc98`
remains unchanged. Final six-platform results are in PR183. Issue102
and broader PCRE, release and consumer migration gates remain open;
Grep remains last.
+
## Bounded zero-width chains before zero-minimum group repeats (PR184)

After merged183, the specialized optional-bracket UTF bitmap proof no
longer limits the middle prefix to two nodes. A leading positive
single-byte ASCII literal lookahead excludes CR even if the following
group repeat consumes nothing. Any number of supported word boundaries
or single-byte noncaseless ASCII literal assertions may precede the
zero-minimum, positive-maximum group repeat while each middle node,
the leading assertion, the repeat and the mandatory CR-excluding group
head fit the existing 1,000-visit budget. Cancellation is checked at
every middle node. Unrecognized nodes remain outside this proof; parser,
API, runtime limits and consumers are unchanged.

Pinned PCRE2 10.48 JIT retained LF, excluded CR and produced positive
JIT sizes across long boundary/assertion chains and five repeated-group
shapes. Three hundred three native full-capture fixtures / 606
strict+tolerant checks cover CRLF and ordinary subjects, including 900
boundaries. Successful-build [test-only RED568](https://github.com/uniblab/Icod.Expressions/actions/runs/42354718366),
Linux ARM64/net8: 101 new CRLF failures, 31,605 passes, zero skips and
no compiler warnings/errors. All 31,403 baseline cases and 202 new
controls passed. Immutable test blob
`aa4d1bb8415881799a630852b7eaa4c51b1dae04` remains unchanged.
Final six-platform results are in PR184. Issue102 and broader PCRE,
release and consumer migration gates remain open; Grep remains last.

## Wide scalar literal assertions before zero-minimum group repeats (PR185)

After merged184, the specialized optional-bracket UTF bitmap proof decodes
exactly one complete UTF-8 scalar in its leading positive caseful forward
literal assertion, and in each supported caseful middle literal assertion.
The leading scalar must exclude CR. Boundary and assertion visits still
charge the existing 1,000-visit budget, cancellation remains per node,
and each repeated-group child branch still needs a mandatory CR-excluding
head. Multi-scalar and caseless assertions, general lookaround bodies,
and unrecognized nodes remain outside this proof; parser, runtime, API,
limits and consumers are unchanged.

Pinned PCRE2 10.48 JIT retained LF, excluded CR and produced positive JIT
sizes for 105 assertion/group patterns. Three hundred fifteen native
full-capture fixtures / 630 strict+tolerant checks cover U+0100 and
U+1F642 leading lookaheads, wide middle lookaheads/lookbehinds, five
group shapes and CRLF/ordinary subjects. Successful-build
[test-only RED570](https://github.com/uniblab/Icod.Expressions/actions/runs/42369519072),
macOS ARM64/net8/9/10: 95 new CRLF failures and 31,926 passes per framework,
zero skips and no compiler warnings/errors. All 31,706 baseline cases
and 210 new controls passed. Immutable test blob
`feb7d68cb5aea9d6d071add8b58177ed9b80ad9d` remains unchanged.
Final six-platform results are in PR185. Issue102 and broader PCRE,
release and consumer migration gates remain open; Grep remains last.

## Multi-scalar literal assertions before zero-minimum group repeats (PR186)

After merged185, the specialized optional-bracket UTF bitmap proof reads
the first complete scalar of each caseful literal assertion. A leading
positive forward assertion must begin with a CR-excluding scalar; its
remaining literal bytes do not change CR exclusion at the entry. Middle
caseful literal assertions may likewise contain multiple scalars. The
same two-visit assertion charge, 1,000-visit bound, cancellation and
mandatory CR-excluding repeated-group head remain. Caseless/general
lookaround and unrecognized nodes remain outside this proof; parser,
runtime, API, limits and consumers are unchanged.

Pinned PCRE2 10.48 JIT retained LF and excluded CR for 200 lead/middle/
group combinations; a CR-first leading lookahead instead included CR.
Six hundred native full-capture fixtures / 1,200 strict+tolerant checks
cover ASCII/UTF mixed literals, five nullable-group shapes and CRLF/
ordinary subjects. Successful-build
[test-only RED572](https://github.com/uniblab/Icod.Expressions/actions/runs/42371394007),
Linux x64/net8/9/10: 150 new CRLF failures, 32,471 passes and zero skips
per framework, no compiler warnings/errors. All 32,021 baseline cases
and 400 new ordinary controls passed. Immutable test blob
`cb2aa7e1a2bbcc6c6c324eb9e3d16f8f17bc0484` remains unchanged.
Final six-platform results are in PR186. Issue102 and broader PCRE,
release and consumer migration gates remain open; Grep remains last.

## Positive class lookahead before zero-minimum group repeats (PR187)

After merged186, the specialized optional-bracket UTF bitmap proof admits
a leading positive caseful lookahead only when its body has one branch
and one direct CR-excluding scalar set or supported scalar class. The
class head is checked on a success-only visit checkpoint. The general
assertion's body and branch add two visits to the leading assertion and
repeat charge; middle zero-width nodes and the mandatory CR-excluding
repeated-group head keep their established checks. The 1,000-visit bound
and cancellation remain. Multi-node, nullable, predicate, negated or
CR-admitting lookahead bodies remain outside this proof; parser, runtime,
API, limits and consumers are unchanged.

Pinned PCRE2 10.48 JIT retained LF, excluded CR and produced positive
JIT sizes for 90 class/prefix/group patterns. CR-admitting `\s` and
`[B\r]` leads included CR in their native bitmaps. Two hundred seventy
native full-capture fixtures / 540 strict+tolerant checks cover six
leads, three zero-width prefixes, five repeated-group shapes and CRLF/
ordinary subjects. Successful-build
[test-only RED574](https://github.com/uniblab/Icod.Expressions/actions/runs/42374407725),
Linux ARM64/net8/9/10: 90 new CRLF failures, 32,801 passes and zero skips
per framework, no compiler warnings/errors. All 32,621 baseline cases
and 180 new ordinary controls passed. Immutable test blob
`fc233a1e9900250c0df2e418bb0152ee201006a6` remains unchanged.
Final six-platform results are in PR187. Issue102 and broader PCRE,
release and consumer migration gates remain open; Grep remains last.

## Middle class lookahead before zero-minimum group repeats (PR188)

After merged187, the specialized optional-bracket UTF bitmap proof reuses
the one-branch, one-head positive caseful class lookahead check at each
supported middle prefix position. Each middle class assertion charges
its opcode, body, branch and class-range visits on a success-only proof.
The leading assertion still excludes CR; word boundaries, literal
assertions, the 1,000-visit bound, cancellation and mandatory CR-excluding
repeated-group head keep their checks. Other lookaround bodies and
unrecognized nodes remain outside this extension; parser, runtime, API,
limits and consumers are unchanged.

Pinned PCRE2 10.48 JIT retained LF, excluded CR and produced positive
JIT sizes for 70 lead/middle/group patterns. Two hundred ten native
full-capture fixtures / 420 strict+tolerant checks cover two leading
assertions, seven middle prefixes, five repeated-group shapes and CRLF/
ordinary subjects. Successful-build
[test-only RED576](https://github.com/uniblab/Icod.Expressions/actions/runs/42442001453),
macOS ARM64/net8/9/10: 70 new CRLF failures, 33,031 passes and zero skips
per framework, no compiler warnings/errors. All 32,891 baseline cases
and 140 new ordinary controls passed. Immutable test blob
`368303ed4aad8d9ac59b0b827ac3ef8fb7b6d8aa` remains unchanged.
Final six-platform results are in PR188. Issue102 and broader PCRE,
release and consumer migration gates remain open; Grep remains last.

## Middle class lookbehind before zero-minimum group repeats (PR189)

After merged188, the specialized optional-bracket UTF bitmap proof admits
positive caseful direct class lookbehind at a supported middle prefix
position. Reuse the one-branch, one-head CR-excluding class check and charge
the opcode, body, branch and class-range visits on a success-only proof.
The leading assertion remains positive ahead; repeated-group head, word
boundaries, literal assertions, cancellation and 1,000-visit bound keep
their checks. Multi-node, nullable, negated, predicate and CR-admitting
lookbehind bodies remain outside this extension. No parser, runtime, API,
limits, consumer, package, release or workflow change.

Pinned PCRE2 10.48 compile/JIT retained LF and excluded CR for 60
lead/middle/group patterns, with positive JIT sizes and three native
subject checks each. One hundred eighty native full-capture fixtures /
360 strict+tolerant checks cover two leads, six lookbehinds, five group
shapes and CRLF/ordinary subjects. Successful-build
[test-only RED578](https://github.com/uniblab/Icod.Expressions/actions/runs/42447771075),
macOS ARM64/net8/9/10: 60 new CRLF failures, 33,221 passes and zero skips
per framework, no compiler warnings/errors. All 33,101 baseline cases
and 120 new ordinary controls passed. Immutable test blob
`62dd6b45d8a40c6064c67a02107ee7dfd55d9194` remains unchanged.
Final six-platform results are in PR189. Issue102 and broader PCRE,
release and consumer migration gates remain open; Grep remains last.

## Leading class lookbehind before zero-minimum group repeats (PR190)

After merged189, the specialized optional-bracket UTF bitmap proof reuses
the one-branch, one-head positive caseful class assertion check for a
leading lookbehind. It charges the assertion body, branch and class-range
visits on a success-only proof. Middle zero-width nodes, mandatory
CR-excluding repeated-group head, cancellation and 1,000-visit ceiling
keep their checks. Multi-node, nullable, negated, predicate and
CR-admitting lookbehind heads remain outside this proof; parser, runtime,
API, limits and consumers are unchanged.

Pinned PCRE2 10.48 compile/JIT retained LF and excluded CR for 90
lead/prefix/group patterns, with positive JIT sizes and three native
subject checks each. Two hundred seventy native full-capture fixtures /
540 strict+tolerant checks cover six leading lookbehinds, three middle
prefixes, five repeated-group shapes and CRLF/ordinary subjects.
Successful-build [test-only RED580](https://github.com/uniblab/Icod.Expressions/actions/runs/42452100008),
Linux ARM64/net8/9/10: 90 new CRLF failures, 33,461 passes and zero skips
per framework, no compiler warnings/errors. All 33,281 baseline cases
and 180 new ordinary controls passed. Immutable test blob
`b9f1c10f46ac791523fa8365f86426bbfcf3515b` remains unchanged.
Final six-platform results are in PR190. Issue102 and broader PCRE,
release and consumer migration gates remain open; Grep remains last.

## Leading literal lookbehind before zero-minimum group repeats (PR191)

After merged190, the specialized optional-bracket UTF bitmap proof admits
a leading positive caseful literal lookbehind containing exactly one
CR-excluding UTF scalar. The existing mandatory-literal proof checks its
scalar; the prior literal-ahead path and supported middle zero-width
prefixes remain. The mandatory repeated-group head, cancellation and
1,000-visit ceiling keep their guards. Multi-scalar, caseless or
CR-admitting leading literal lookbehind remains outside this extension;
parser, runtime, API, limits and consumers are unchanged.

Pinned PCRE2 10.48 compile/JIT retained LF and excluded CR for 60
lead/prefix/group patterns, with positive JIT sizes and three native
subject checks each. One hundred eighty native full-capture fixtures /
360 strict+tolerant checks cover four leading literals (including a
wide UTF scalar), three middle prefixes, five repeated-group shapes and
CRLF/ordinary subjects. Successful-build
[test-only RED582](https://github.com/uniblab/Icod.Expressions/actions/runs/42455740589),
macOS ARM64/net8/9/10: 60 new CRLF failures, 33,671 passes and zero skips
per framework, no compiler warnings/errors. All 33,551 baseline cases
and 120 new ordinary controls passed. Immutable test blob
`a932566cdf7b3d639b01251b3c3419e8544b5d9b` remains unchanged.
Final six-platform results are in PR191. Issue102 and broader PCRE,
release and consumer migration gates remain open; Grep remains last.

## Leading two-scalar lookbehind before zero-minimum group repeats (PR192)

After merged191, the specialized optional-bracket UTF bitmap proof
validates one or two scalars in a leading positive caseful literal
lookbehind, requiring each to exclude CR. The two-scalar scan is bounded;
existing middle prefix, mandatory repeated-group head, cancellation and
1,000-visit checks remain. Three-scalar, caseless and CR-containing heads
remain outside this extension. Parser, runtime, API, limits and consumers
are unchanged.

Pinned PCRE2 10.48 compile/JIT retained LF and excluded CR for 60
lead/prefix/group patterns with positive JIT sizes and three native
subject checks each. One hundred eighty native full-capture fixtures /
360 strict+tolerant checks cover four two-scalar lookbehinds (including
wide UTF), three middle prefixes, five repeated-group shapes and CRLF/
ordinary subjects. Successful-build
[test-only RED584](https://github.com/uniblab/Icod.Expressions/actions/runs/42465995689),
macOS ARM64/net8/9/10: 60 new CRLF failures, 33,851 passes and zero skips
per framework, no compiler warnings/errors. All 33,731 baseline cases
and 120 new ordinary controls passed. Immutable test blob
`a61b8e7cae700c8096d70594dc13dfa136f906c5` remains unchanged.
Final six-platform results are in PR192. Issue102 and broader PCRE,
release and consumer migration gates remain open; Grep remains last.

## Bounded leading literal lookbehind before zero-minimum group repeats (PR193)

After merged192, the specialized optional-bracket UTF bitmap proof scans
at most eight valid CR-excluding scalars in a leading positive caseful
literal lookbehind. The optional bracket, supported middle zero-width
prefixes, mandatory repeated-group head, cancellation and 1,000-visit
ceiling keep their guards. Longer, caseless and CR-containing lookbehind
heads remain outside this extension. Parser, runtime, API, limits and
consumers are unchanged.

Pinned PCRE2 10.48 compile/JIT retained LF and excluded CR for 90
lead/prefix/group patterns with positive JIT sizes and three native
subject checks each. Two hundred seventy native full-capture fixtures /
540 strict+tolerant checks cover six leading literals of three, four or
eight scalars (including wide UTF), three middle prefixes, five group
shapes and CRLF/ordinary subjects. Successful-build
[test-only RED586](https://github.com/uniblab/Icod.Expressions/actions/runs/42474722466),
Linux ARM64/net8/9/10: 90 new CRLF failures, 34,091 passes and zero skips
per framework, no compiler warnings/errors. All 33,911 baseline cases
and 180 new ordinary controls passed. Immutable test blob
`aac6226ccc391715a79c494fd0da30f46ff8d0c5` remains unchanged.
Final six-platform results are in PR193. Issue102 and broader PCRE,
release and consumer migration gates remain open; Grep remains last.

## CR-containing leading literal lookbehind guard (PR194)

After merged193, pinned PCRE2 10.48 compile/JIT retained LF and excluded
CR for 135 leading lookbehind/prefix/group patterns, with positive JIT
sizes and three native subject positions each. Yet `(?<=\\r)` can match
at the LF within CRLF. The existing CR exclusion in the bounded leading
lookbehind start-skip proof is necessary: admitting that lookbehind would
skip a valid LF candidate. The test-only full-suite run passed before a
production change, so this PR retains the guard and pins native full
match/capture parity as regression coverage.

Three hundred ninety distinct fixtures / 780 strict+tolerant checks
cover nine leading literals, three middle prefixes, five nullable group
shapes and CRLF/ordinary/positive subjects. The single-CR positive
subject duplicates its CRLF fixture and is listed once. Successful-build
[test-only run588](https://github.com/uniblab/Icod.Expressions/actions/runs/42477284630)
passed 34,571 cases per framework on Linux ARM64/net8/9/10, including
frozen214, with zero failed/skipped tests. The initial theory enumerated
15 duplicate IDs, removed before final CI. Final six-platform results
are in PR194. Issue102 and broader PCRE, release and consumer migration
gates remain open; Grep remains last.

## Single-scalar negative assertions before zero-minimum group repeats (PR195)

After merged194, the specialized optional-bracket UTF proof admits a
leading caseful negative literal assertion, ahead or behind, only when
its full body is one valid UTF scalar other than CR/LF. The optional
nullable group and mandatory CR-excluding continuation, supported
middle prefixes, cancellation and 1,000-visit ceiling keep their guards.
Explicit-newline, multi-scalar and caseless negatives remain outside
this extension. Parser, matcher, API, limits and consumers are unchanged.

Pinned PCRE2 10.48 compile/JIT retained LF and excluded CR for 120
lead/prefix/group patterns with positive JIT sizes and two native
subject positions each. Two hundred forty full-capture fixtures / 480
strict+tolerant checks cover six negative B/C/wide-UTF literals and two
explicit-newline controls, three middle prefixes, five nullable repeated
group shapes, and CRLF/ordinary subjects. Successful-build
[test-only RED591](https://github.com/uniblab/Icod.Expressions/actions/runs/42481376973),
Linux ARM64/net8/9/10: 90 new CRLF failures, 34,721 passes and zero skips
per framework, no compiler warnings/errors. All 34,571 baseline cases
and 150 new controls passed. Immutable test blob
`69929f9054b7249a34c631a9192568af41e7fbd8` remains unchanged.
Final six-platform results are in PR195. Issue102 and broader PCRE,
release and consumer migration gates remain open; Grep remains last.

## Bounded multi-scalar negative assertions before zero-minimum group repeats (PR196)

After merged195, the specialized optional-bracket UTF proof scans at most
eight valid CR/LF-excluding scalars in a leading caseful negative literal
lookahead or lookbehind. The optional group, supported middle zero-width
prefixes, mandatory CR-excluding repeated-group head, cancellation and
1,000-visit ceiling keep their guards. Longer, newline-containing or
caseless negative assertions remain outside this extension. Parser,
matcher, API, limits and consumers are unchanged.

Pinned PCRE2 10.48 compile/JIT retained LF and excluded CR for 120
lead/prefix/group patterns, with positive JIT sizes and two native
subject offsets each. Two hundred forty native full-capture fixtures /
480 strict+tolerant checks cover eight negative literals of two, three
or eight scalars (including wide UTF), three middle prefixes, five
nullable group shapes and CRLF/ordinary subjects. Successful-build
[test-only RED594](https://github.com/uniblab/Icod.Expressions/actions/runs/42511015475),
Linux ARM64/net8/9/10: 120 new CRLF failures, 34,931 passes and zero
skips per framework, no compiler warnings/errors. All 34,811 baseline
cases and 120 new ordinary controls passed. Immutable test blob
`f565c50e0a3c77e09c73d135a1767435051513fb` remains unchanged.
Final six-platform results are in PR196. Issue102 and broader PCRE,
release and consumer migration gates remain open; Grep remains last.

## Caseless negative assertions before zero-minimum group repeats (PR197)

After merged196, the specialized optional-bracket UTF proof accepts
caseless leading negative literal lookahead or lookbehind when its
bounded body contains at most eight valid CR/LF-excluding scalars.
The optional group, supported middle zero-width prefixes, mandatory
CR-excluding repeated-group head, cancellation and 1,000-visit ceiling
keep their guards. Longer and newline-containing negative assertions
remain outside this extension. Parser, matcher, API, limits and
consumers are unchanged.

Pinned PCRE2 10.48 compile/JIT retained LF and excluded CR for 210
lead/prefix/group patterns, including special S/K/Kelvin case folds,
with positive JIT sizes and two native subject offsets each. Four
hundred twenty native full-capture fixtures / 840 strict+tolerant checks
cover fourteen negative leads of one to eight scalars (including wide
UTF), three middle prefixes, five nullable group shapes and CRLF/
ordinary subjects. Successful-build
[test-only RED597](https://github.com/uniblab/Icod.Expressions/actions/runs/42602489913),
Linux ARM64/net8/9/10: 210 new CRLF failures, 35,261 passes and zero
skips per framework, no compiler warnings/errors. All 35,051 baseline
cases and 210 new ordinary controls passed. Immutable test blob
`cc504846e40f4c0e87184f92169738b813d3e4a8` remains unchanged.
Initial source-only [run598](https://github.com/uniblab/Icod.Expressions/actions/runs/42603410291)
passed the 70 no-middle CRLF cases but still failed 140 cases with
caseless middle literal assertions per framework. The final proof
permits those only beneath a validated caseless negative leading assertion.
Final six-platform results are in PR197. Issue102 and broader PCRE,
release and consumer migration gates remain open; Grep remains last.
+
## Caseless positive assertions before zero-minimum group repeats (PR198)

The bounded optional-bracket UTF proof now admits a leading caseless
literal positive lookahead or lookbehind before a zero-minimum group
repeat. The lookahead requires a valid CR-excluding first scalar; the
lookbehind validates its complete one-to-eight-scalar body and excludes
CR at every position. After a validated caseless literal lead, a bounded
caseless literal middle assertion may precede the repeat. The existing
mandatory CR-excluding repeated-group head, cancellation and 1,000-visit
ceiling remain in force. CR-containing lookbehinds and longer bodies
remain on the general path. Parser, matcher, API, limits and consumers
are unchanged.

Pinned PCRE2 10.48 compile/JIT retained LF and excluded CR for 150
lead/prefix/group patterns with positive JIT sizes and native match
spans on both CRLF and ordinary subjects. Three hundred full-capture
fixtures / 600 strict+tolerant checks cover ten positive lookahead and
lookbehind leads (including two-scalar, wide UTF, S/K folds), three
middle prefixes, five nullable group shapes and two subjects.
Successful-build [test-only RED600](https://github.com/uniblab/Icod.Expressions/actions/runs/42608674631)
on Linux ARM64/net8/9/10 failed exactly 150 new CRLF cases per framework,
with 35,621 passes and zero skips: all 35,471 earlier fixtures and 150
new ordinary controls passed. The test blob
`394c5ef84d35aabf2ecd5d7209e9a630143725d7` remains unchanged.
Six-platform final results are in PR198. Issue102 and broader PCRE,
release and consumer migration gates remain open; Grep remains last.
+
## Caseless positive classes before zero-minimum group repeats (PR199)

The optional-bracket UTF proof now admits positive class lookahead or
lookbehind under caseless mode, provided the single class/type body
passes the existing bounded CR-exclusion test. Caseless literal middle
assertions may follow such a validated lead. The mandatory CR-excluding
repeated-group head, cancellation and 1,000-visit ceiling retain their
existing guards. Negated or predicate-bearing sets, CR-containing ranges
and unsupported types remain outside this proof. Parser, matcher, API,
limits and consumers are unchanged.

Pinned PCRE2 10.48 compile/JIT retained LF and excluded CR for 270
lead/prefix/group patterns, with positive JIT sizes and correct native
spans on CRLF and ordinary subjects. Five hundred forty full-capture
fixtures / 1,080 strict+tolerant checks cover eighteen positive class
lookahead/lookbehind leads (ASCII ranges, wide UTF, types and S/K/Kelvin
folds), three middle prefixes, five nullable group shapes and two
subjects. Successful-build [test-only RED602](https://github.com/uniblab/Icod.Expressions/actions/runs/42619728071)
on macOS ARM64/net8/9/10 failed exactly 270 new CRLF cases per framework,
with 36,041 passes and zero skips: all 35,771 earlier fixtures and 270
new ordinary controls passed. Immutable test blob
`7007ff894cb5412391fe5871017e7f69ed601f84` remains unchanged.
Final six-platform results are in PR199. Issue102 and broader PCRE,
release and consumer migration gates remain open; Grep remains last.
+
## Caseless negative classes before zero-minimum group repeats (PR200)

The bounded optional-bracket UTF proof now admits leading negative
class lookahead or lookbehind, including caseless bodies, when a single
supported scalar set or type excludes both CR and LF. Negated and
predicate-bearing sets, newline-containing ranges, unsupported types
and compound assertion bodies remain outside this extension. The
existing mandatory CR-excluding repeated-group head, cancellation,
1,000-visit ceiling and bounded middle assertions retain their guards.
Parser, matcher, API, limits and consumers are unchanged.

Pinned PCRE2 10.48 compile/JIT retained LF and excluded CR for 270
lead/prefix/group patterns with positive JIT sizes and correct native
spans on CRLF and ordinary subjects. Five hundred forty full-capture
fixtures / 1,080 strict+tolerant checks cover eighteen negative class
lookahead/lookbehind leads (ASCII ranges, wide UTF, types and S/K/Kelvin
folds), three middle prefixes, five nullable group shapes and two
subjects. Successful-build [test-only RED604](https://github.com/uniblab/Icod.Expressions/actions/runs/42751357677)
on Linux x64/net8/9/10 failed exactly 270 new CRLF cases per framework,
with 36,581 passes and zero skips: all 36,311 earlier fixtures and 270
new ordinary controls passed. Immutable test blob
`d2a479cd1c70ec0679ca45ea1c0973ef63b42727` remains unchanged.
Final six-platform results are in PR200. Issue102 and broader PCRE,
release and consumer migration gates remain open; Grep remains last.
+
## Negative class middle assertions before zero-minimum group repeats (PR201)

The bounded optional-bracket UTF proof now permits a negative class
lookahead or lookbehind after a validated leading literal or class
assertion and before a zero-minimum repeated group. It reuses the
single-body class/type CR/LF exclusion from PR200 and charges the
middle opcode against the established 1,000-visit ceiling. Negated
or predicate-bearing sets, newline-containing ranges, unsupported
types and compound assertion bodies remain outside the proof. The
mandatory CR-excluding repeated-group head, cancellation and other
prefix guards remain. Parser, matcher, API, limits and consumers are
unchanged.

Pinned PCRE2 10.48 compile/JIT retained LF and excluded CR for 540
lead/middle/group patterns with positive JIT sizes and correct native
spans on CRLF and ordinary subjects. One thousand eighty full-capture
fixtures / 2,160 strict+tolerant checks cover six positive/negative
literal/class leads, eighteen negative class middle lookahead/lookbehind
forms (ASCII ranges, wide UTF, types and S/K/Kelvin folds), five nullable
group shapes and two subjects. Successful-build [test-only RED606](https://github.com/uniblab/Icod.Expressions/actions/runs/42754755781)
on Linux x64/net8/9/10 failed exactly 540 new CRLF cases per framework,
with 37,391 passes and zero skips: all 36,851 earlier fixtures and 540
new ordinary controls passed. Immutable test blob
`bfb521fcb21f206cd22d6742f3a36367148509e9` remains unchanged.
Final six-platform results are in PR201. Issue102 and broader PCRE,
release and consumer migration gates remain open; Grep remains last.

## CR/LF-excluding negated sets in negative assertions (PR202)

Pinned PCRE2 10.48 reports LF in the start bitmap and CR absent for 840
leading and middle assertion patterns. The existing managed matcher already
visits LF after CR for these patterns because the negated class explicitly
mentions CR/LF. PR202 adds 1,680 native full-capture fixtures and 3,360
strict/tolerant parity checks. Its test-only run passed all 39,611 tests per
framework on the completed runners. No new optimizer proof or public API is
claimed; issue102 and the broader release and Grep gates remain open.

## Negated-range negative assertions before nullable groups (PR203)

The existing bounded proof now admits a leading or middle negative class
lookahead/lookbehind with one predicate-free negated scalar set and valid
nonempty excluded ranges. Native PCRE2 10.48 start-bitmap study skips negative
assertion bodies; their newline membership need not restrict admission.
The repeated group's mandatory CR-excluding head, single-body shape,
supported placements, middle-opcode charges, cancellation and 1,000-visit
ceiling remain. Each range is validated and charged before committing the
shared visit count. Positive assertion and mandatory-head classification,
source ExplicitNewline metadata, parser, matcher, API and limits are unchanged.

Original immutable test blob `2aba532d50a5b630c5180ab18f3b3d7399387489`
adds840 full-capture fixtures/1680 strict+tolerant checks for endpoint-implicit
CR/LF-excluding ranges. [Test-only run610](https://github.com/uniblab/Icod.Expressions/actions/runs/42760807042)
compiled cleanly on six native runners and failed420 CRLF cases per framework,
with40031 passes and zero skips. The separate48 native guard fixtures retain
ranges excluding onlyCR, onlyLF or neither. [Test-only run611](https://github.com/uniblab/Icod.Expressions/actions/runs/42762304905)
revealed24 additional CRLF misses and24 passing ordinary controls on the
inspected LinuxARM64/macOSARM64 frameworks, establishing that requiring both
newlines to be excluded would be too restrictive. Both test corpora remain
unchanged after RED. Total committed suite:40499 per framework.
See the PR203 design and final PR audit. Broader issue102, release qualification
and consumer migration remain open; Grep stays last.


## Negative scalar-class assertion bitmap entry (PR204)

The bounded negative-class ahead/behind proof now accepts every already-parsed
scalar class and nonempty scalar-set range/predicate union, regardless of
newline membership or negation. PCRE2 10.48 start-bitmap study skips these
negative assertion bodies before studying the repeated group's mandatory
CR-excluding head. Positive assertions, consuming-head classification,
source ExplicitNewline metadata, parser, matcher and limits are unchanged.
Range validation, per-range/per-predicate cancellation and visit charges,
supported leading/middle positions and the shared1,000-visit ceiling remain.

Pinned native10.48 JIT research passed2,560 compile/bitmap/JIT-size checks and
5,120 literal full-capture checks. All start bitmaps includeLF and excludeCR.
The immutable2,560-fixture blob `c6bc29aab5ea31f66ca486336c48d4c0529f7f46`
pins5,120 strict/tolerant comparisons across16 bodies, UCP/caseless modes,
leading/middle negative ahead/behind and five nullable group-repeat shapes.
[Test-only run613](https://github.com/uniblab/Icod.Expressions/actions/runs/42766233793)
on LinuxARM64 compiled cleanly and failed1,240 new CRLF cases per framework,
with41,819 passes and zero skips; all40,499 baseline tests and ordinary controls
passed. Expected final suite:43,059 per framework. Final CI is recorded in PR204.
Broader issue102 and PCRE/release qualification remain open; Grep stays last.


## Regular negative assertion body bitmap entry (PR205)

A bounded structural fallback now recognizes regular negative ahead/behind
bodies containing sequences, alternatives, capturing/noncapturing/atomic groups,
repeats, literals/classes/dot/boundaries and ordinary nested assertions. Native
PCRE2 10.48 start-bitmap study skips the body; the existing following repeated
group must still prove a mandatory CR-excluding head. Empty/nullable inner
paths need no consuming head. Calls, backreferences, conditions, anchors,
match-start reset and unknown/non-atomic assertion forms remain excluded.

The existing single-class path and exact charges remain. The new iterative
compiler-only walk charges bodies, branches, nodes, ranges, predicates and
literal-assertion scalars under cancellation, bounds visited and queued work
at1,000 and commits speculative costs only on complete success. Positive and
literal assertion admission, source metadata, parser, VM, API and limits stay.

Pinned native10.48 research passed2,720 compile/bitmap/positive-JIT-size checks
and5,440 literal full-capture comparisons. All bitmaps includeLF and excludeCR.
The immutable2,720-fixture blob `a6137a8f9f5a14fd9c9adce6509d385f2215cb96`
includes200-deep groups,64 alternatives, UCP/caseless and leading/middle
placements. [Test-only run615](https://github.com/uniblab/Icod.Expressions/actions/runs/42772123598)
on macOSARM64 compiled cleanly and failed1,360 new CRLF cases per framework,
with44,419 passes and zero skips. All43,059 baseline tests and ordinary controls
passed. Expected final suite:45,779 per framework. Final CI is recorded in PR205.
Broader issue102, PCRE/release qualification and Grep migration remain open.


## Pinned Unicode17 general categories

PR207 replaces host .NET/OS category queries with an immutable managed
Unicode17.0.0 table matching PCRE2 10.48. Explicit properties, category-derived
UCP/POSIX classes and UTF capture-name letters/digits share the same lookup;
non-ASCII subroutine numbers retain controlled InvalidSyntax diagnostics.
Packed ranges plus an ASCII fast path occupy16,592 data bytes, require no
runtime files/native dependency and add no VM state, charged work or limits.
Existing complements, caseless category widening, local ASCII policies, byte
projection, spans and cancellation remain shared.

The source-hash-checked C# generator and Linux byte-for-byte regeneration gate
make data ownership reproducible. Unicode permission terms ship in the package.
An independent native fingerprint covers all1,112,064 valid scalars; new JIT
fixtures cover recent assignments, complements/brackets, category capture
selection and UTF names. The test-only run620 built successfully: LinuxARM64
net8 had29 expected new failures,45,782 passes and all45,779 baseline cases
passed. The32-case test blob is
`18e46522c52a6750b83a147350e7b48ae64f65f3`.

This closes general-category version ownership only. Full Unicode fold tables,
scripts/binary properties, grapheme/class algebra and broader native optimizer
parity remain gates. No public API/release/consumer change; frozen35 intact and
Grep last. See `PCRE-UNICODE-CATEGORIES-DESIGN.md`,
`PCRE-UNICODE-CATEGORIES-PLAN.md` and `PCRE-ROADMAP-STATUS.md`.


## Pinned Unicode17 simple-case equivalence

PR208 replaces host Rune uppercase/lowercase round trips and special-case
patches with the complete Unicode17 C/S mapping used by PCRE2 10.48. The
existing Fold/Equals/CloseRanges paths retain scalar widths, byte-UCP
projection, local i/r, source metadata, cancellation and charged matcher work.
ASCII remains a direct fast path; 1,486 immutable packed mappings occupy
11,888 data bytes. There is no new VM state, runtime dependency or public API.

Independent native fingerprints cover semantic equivalence classes over all
1,112,064 valid scalars, both unrestricted and restricted, independently of
canonical key choice. A native JIT probe verified 1,656 existing comparison-path
cases. The 35-case test blob remains
`7f2927cdd90176f0cdee90867035bbc3c8803d73` after successful-build RED622:
Linux ARM64 had 17 expected new failures and 45,829 passes on each framework;
all 45,811 baseline cases passed. Live fixtures include new pairs, multi-member
sets, literals/brackets/ranges, backreferences/assertions, non-growing defaults
and malformed-UTF offsets/capture widths.

The hash-checked C# generator and Linux byte-for-byte gate make this data
reproducible; the existing packaged Unicode notice now covers both sources.
This closes default simple-case table ownership, not full-string F or optional
Turkic T modes. Remaining syntax, optimizer, API/release and consumer gates stay
open; frozen35 intact and Grep last. See `PCRE-UNICODE-CASEFOLD-DESIGN.md`,
`PCRE-UNICODE-CASEFOLD-PLAN.md` and `PCRE-ROADMAP-STATUS.md`.


## Unicode17 script properties

PR209 implements all 175 PCRE2 10.48 scripts and 344 normalized aliases: bare
names mean scx, with explicit sc/Script and scx/Script_Extensions namespaces,
colon/equal separators and existing loose names/complements. Native scx retains
primary membership plus extension bits, including Common/Inherited. Script
predicates ignore i/r/UCP/ASCII policy; byte projection uses the same semantics.

An explicit immutable PropertyValue flows through existing scalar class,
predicate and instruction metadata. Existing ScalarClass/ScalarSet opcodes,
scalar widths, captures/journal, Step and cancellation paths remain. Generated
per-script interval lookup has bounds checks and bounded binary search; no
runtime dependency or new saved state. The C# generator validates three source
hashes and a complete independent native fingerprint over 1,112,064 scalars;
Linux CI verifies exact regeneration, with the Unicode notice in the package.

Native probes verified 7,092 alias/literal and 1,227,800 script-boundary JIT
outcomes. Test data pins 3,508 boundaries, every alias, all256 byte values,
nested grammar/captures, case/ASCII independence, malformed UTF and resources.
The corrected test-only RED626 built successfully: Linux ARM64 had42 new
failures and45,851 passes on net8, with all45,846 baseline cases passing. Old
unfrozen pending-Greek rows now retain rejection coverage for binary/Bidi
families. New/updated test blobs stay unchanged after that RED.

Binary/Bidi properties, script runs, graphemes/class algebra and broader
optimizer/API/release/consumer gates remain open. Frozen35/frozen214 preserved,
no public API/version/publication/migration change, Grep last. See
`PCRE-UNICODE-SCRIPTS-DESIGN.md` and `PCRE-UNICODE-SCRIPTS-PLAN.md`.


## Unicode17 binary and Bidi properties

PR210 completes the PCRE2 10.48 property catalogue:57 binary predicates,
106 normalized binary aliases and23 short Bidi_Class values. Binary names
are bare; BC/Bidi_Class accept colon/equal, and native bare bidi-prefixed
values are preserved. InCB means native non-None membership, and
Bidi_Mirrored follows the native mirroring list. Unassigned Bidi ranges
retain ordered @missing defaults. Long Bidi values, Is/block names,
Boolean assignments and wrong namespaces stay explicitly unsupported.

Binary/Bidi predicates reuse immutable PropertyValue and the existing
ScalarClass/ScalarSet opcodes. They ignore UCP/i/r/ASCII policy, use the
same byte projection and preserve complements, capture widths, journals,
Step and cancellation. Bounded per-property interval searches require no
runtime files, host Unicode service, native fallback or new VM state.
The C# generator checks seven pinned source hashes and the native9-byte
record fingerprint over all1,112,064 valid scalars; Linux CI verifies exact
regeneration. The package includes the expanded Unicode notice.

The test-only RED629 compiled without warnings/errors and had47 expected
feature failures/45,901 passes on each framework. New immutable tests and
fixtures pin7,706 native state boundaries, all aliases/Bidi forms, every
byte value, nested grammar, malformed UTF, resources and diagnostic spans.
Independent native JIT probes verified1,232,960 boundary/capture outcomes
and47 grammar cases. See PCRE-BINARY-BIDI-PROPERTIES-DESIGN.md and
PCRE-BINARY-BIDI-PROPERTIES-PLAN.md for fingerprints and fixed test blobs.

This closes native property coverage, while script-run groups, graphemes,
class algebra, optional Turkic folding and broader optimizer/UTF16/API,
performance/release/consumer gates remain open. Frozen35/frozen214 stay
intact; no public API/version/publication/migration change and Grep last.


## Atomic grapheme clusters

PR211 adds byte/UTF8 \X with PCRE2 10.48's pinned Unicode17 break data.
Each cluster is atomic and nonempty; outer repeats retain whole-cluster
backtracking and existing captures/calls/assertions. Native RI prefix parity,
EP/Extend/ZWJ state and table exceptions are preserved, including RI before
Extend and Prepend before EP. Native Indic-conjunct GB9c segmentation is not
implemented by the oracle; this is native parity, not a blanket UAX claim.
Bracket \X is invalid and consuming lookbehind use remains unsupported.
Fixed/variable-behind nested forward assertion bounds retain native behavior.

A dedicated node/opcode consumes validated bytes within the current fragment
and scope. Every scalar/prefix probe uses the shared Step primitive with its
ref work counter; no Step delegate or per-atom allocation is introduced. A
two-offset, input-derived RI memo avoids quadratic contiguous-prefix scans
without adding saved VM state. Unicode classification uses128 ASCII entries
and2,035 packed range ends (8,268 data bytes), independent of host services.
Hash-checked C# generation, exhaustive native fingerprint and Linux exact
regeneration qualify the data; the package includes its Unicode notice.

The initial successful-build RED631 had53 expected feature failures and45953
passes. Scope-complete RED632 had54 expected failures/45955 passes, zero skips
on every framework; all45948 baseline cases pass. New test/data blobs remain
fixed after RED. Fixtures cover3535 Unicode boundaries, all225 type pairs,
3375 triples at every scalar start, all65536 byte pairs, atomicity/repeats,
captures and nested grammar, malformed UTF, scope bounds, work/cancellation
and linear RI scans. Standalone native JIT probes verified222675 boundary/
capture outcomes and41 grammar/policy outcomes. See PCRE-GRAPHEME-DESIGN.md
and PCRE-GRAPHEME-PLAN.md for the fingerprint and execution evidence.

Extended class algebra, script-run groups, optional Turkic folding, other
pending syntax, optimizer/resource, UTF16/API and realistic performance/
release/consumer gates remain. Frozen35/frozen214 preserved; no public API,
version, publication or migration change. Grep stays last.

The existing malformed-UTF fixture exposed a pinned JIT mode distinction.
MatchInvalidUtf uses a different backward RI context view: literal RI prefixes
are not counted, while26 mapped CJK prefixes can count. The compatibility
predicate preserves that view, with normal forward classification and charged
backward probes. Strict mode keeps its contiguous RI memo. Eight additional
immutable regressions were observed failing in successful-build RED634 before
the correction (9failures/46008passes,46017total). Native probes qualify every
valid prefix scalar and672 compound-prefix cases.


## Perl extended class algebra (PR212)

The internal byte/UTF8 compiler supports Perl `(?[...])` as one scalar:
operand complement, intersection, union (`|`/`+`), subtraction, symmetric
difference and grouping. Native precedence is `!`, then `&`, then equally
binding left-associative `| + - ^`. Operands reuse escaped literals,
shorthands, pinned Unicode predicates, direct POSIX names and ordinary brackets.
Spaces/tabs are ignored locally; other ordinary pattern trivia is not.

Postfix terms avoid recursive parsing/evaluation. Native class nesting is
bounded at15; terms share the4096 compilation budget, including byte-mode
expressions. Operand case closure precedes algebra. Byte expressions project
to a precomputed bitmap with native mixed-Any/property bitmap behavior. UTF
evaluation uses one lazy scratch array per Match, charging every term and
interval/predicate probe to the existing work/cancellation contract. Captures,
frames, malformed fragments and width-one lookbehind use existing machinery.

PcrePerlClassAlgebraReferenceTests and fixed native fixture masks qualify308
expressions ×8 profiles ×256 values and12 expressions ×4 profiles ×7592
Unicode boundary scalars, plus52 total test cases covering grammar, source
offsets, folding, capture/repetition/behind, malformed UTF and exact limits.
The tests-first successful-build RED636 retained all46017 baseline passes.
See PCRE-PERL-CLASS-ALGEBRA-DESIGN.md and PCRE-PERL-CLASS-ALGEBRA-PLAN.md.

UTS#18's ALT_EXTENDED_CLASS activation remains a separate frontend increment.
General start-optimizer equivalence (issue102), script-run groups, UTF code-unit
escape, directives/verbs/callouts, UTF16/API, realistic performance and release/
consumer gates remain open. GNU/BRE/ERE, frozen35/frozen214 and public API stay
intact. Grep stays last; no publication/migration change.


## Opt-in UTS#18 class syntax (PR213)

PcreByteOptions.AltExtendedClass defaults to false. When enabled, brackets
outside Perl `(?[...])` accept nested brackets, implicit union and doubled
`||`, `&&`, `--`, `~~` operators. Implicit union binds first. Explicit
operators at one level must have one kind and associate leftward; nested levels
can choose other operators. Leading ^ complements the whole class. Space/tab
handling follows x/xx scope, quotes remain literal, and Perl leaves keep their
ordinary bracket grammar. The option is internal; public API is unchanged.

A bounded heap-frame frontend emits the existing postfix algebra, preserving
contiguous raw item runs for native byte Any/property behavior. Ordinary
classes without algebra return through the original readers to retain source/
optimizer metadata. Native nesting15, shared compilation4096 and charged UTF
term/range/predicate work/cancellation use the existing contracts and evaluator;
byte expressions remain precomputed bitmaps. Captures/repeats/width-one behind
and malformed-UTF fragments reuse the existing engine.

PcreUtsClassAlgebraReferenceTests and fixed native masks qualify235 expressions
×8 profiles ×256 values plus12 expressions ×4 profiles ×7592 Unicode boundary
scalars. Focused cases pin activation, implicit precedence, operator mixing,
ranges/quotes/POSIX, Perl isolation, original byte diagnostics and limits.
Successful-build RED639 retains46069 baseline passes with58 expected failures.
See PCRE-UTS-CLASS-ALGEBRA-DESIGN.md and PCRE-UTS-CLASS-ALGEBRA-PLAN.md.

Script-run groups are the next syntax increment. UTF code-unit escape, remaining
directives/verbs/callouts, general optimizer/resource conformance (issue102),
UTF16/public API, realistic performance and release/consumer gates remain open.
No new package, production fallback, GNU behavior or frozen35/frozen214 change.
Grep stays last; the user owns publication/migration.


## Script-run groups (PR214)

The internal byte/UTF8 engine supports `(*script_run:...)`/`(*sr:...)` and
`(*atomic_script_run:...)`/`(*asr:...)`. Validation checks the consumed span
after the body. Empty/single-scalar spans accept Unknown; longer spans reject
it, intersect neutral-free primary/extension maps, preserve native directional
Han transitions and require decimal digits from one ten-character family.
Validation uses actual subject properties, independently of i/r/UCP/ASCII flags.
ASR commits an atomic body before validation, as `(*sr:(?>...))` does.

The wrapper is transparent to parser tree rewrites and behind widths. Entry
marks use the existing rollback/call journal; failure retries ordinary body
choices, and recursive returns restore outer starts. The scanner has no scan
allocations and charges scalar decode/classification to shared work and
cancellation. Script marks share the256 compiled-state budget and4096 compile
budget. Empty-fragment and explicit-newline metadata traverse the wrapper;
other start-optimizer proofs remain conservative.

GeneratePcreUnicodeScripts now derives301 run descriptors and1892 start ranges
from its existing source hashes and native full-scalar fingerprint. Three ulong
words hold each script map. Pinned category ranges identify all77 decimal
families, including adjacent families and non-hex-aligned starts. Exact C#
regeneration and independent native masks qualify the added data paths.

PcreScriptRunReferenceTests cover301 descriptor pairs,5111 boundaries in both
orders against20 contexts,77x77 decimal-set pairs at zero/nine endpoints,
20-cubed context triples and all256x256 byte pairs. Focused tests pin aliases,
rollback, recursion, ASR placement, captures/behind, invalid UTF, original spans
and limits. RED642 keeps46127 baseline passes;64 new cases fail as expected
and one quoted-literal case already passes. Tests/data remain fixed after RED.
See PCRE-SCRIPT-RUN-DESIGN.md and PCRE-SCRIPT-RUN-PLAN.md for details.

UTF code-unit escape, remaining directives/verbs/callouts, general optimizer/
resource conformance (issue102), UTF16/API, realistic performance and release/
consumer qualification remain open. Frozen35/frozen214, GNU and public API are
unchanged; no new package or production fallback. Grep stays last.

## PR233: optional unset backreferences

Internal default-false MatchUnsetBackreference implements PCRE2_MATCH_UNSET_BACKREF
(0x200). Existing nonparticipating captures match empty without becoming set;
capture conditions, declaration-ordered duplicate selection and participating
empty/nonempty comparisons keep their existing semantics. Missing references
remain errors with the established global validation order and byte diagnostics.

One selected heap RewriteTree pass marks canonical Backreference nodes after
name resolution and before condition/call/width/final-target resolution. The
default path skips the pass. UnsetMatchesEmpty lowers into unused Backreference
Second; null lookup advances pc only for Second=1, without source reads or
capture/journal mutation. The existing instruction still charges work. Selected
consumed references reject at their owning lookbehind opener, even under zero
repeats or through calls; existing lookahead/DEFINE/FAIL skips remain shared.

A bounded pure-instruction empty-path proof extends existing STARTLINE metadata
for selected nullable references under ANY/MatchInvalidUtf. It uses at most the
existing 1,000 start-analysis instruction/visit bound, heap storage and cancellation
polls; the existing root line-anchor proof and matcher exclusion/physical-EOF
rules remain authoritative. Calls/assertions/conditions/verbs are outside this
proof. A mandatory leading-B control retains first-character search. Nonnullable
optional-capture/reference prefixes, including ANY/multiline ^(?:(A)?backref1)B
after a split Unicode newline, remain a known wider optimizer gap in #102.

The 1,740 new cases include 1,652 independently pinned PCRE2 10.48/JIT profiles,
32 selected width errors and 40 legacy diagnostics. Supplement all octets, UTF
and malformed-fragment offsets, prior caller policies, owned source/options/raw
labels, all 256 user captures, exact zero-width work, cancellation and quotas.
Native loop arrays compile before managed comparisons. All 246 earlier test
files remain unchanged; GNU/frozen35/214 outcomes/120 methods, API and dependencies
are preserved. This is internal conformance work; Grep migration remains last.

Expanded tests-only RED699 (run43247650585, Linux x64 job131284532504), source
d05a98f991131d65045e4d4925d696850608766c, retained merged PR232 production.
Each net8/9/10 built with zero warnings/errors and ran 61,422 tests: 495 expected
failures, 60,927 passes, zero skips. The 454 primary differences/framework match
independent native flag-off research. Default/earlier fixtures, diagnostics, UTF
validation and native literal/oracle checks passed. Initial RED698 was provisional;
the expanded fixture was frozen at23ebe126b9e8096f4527e4479fdd89e7a9cb5223
before production edits. Require all 18 native results at61,422 passes and the
existing frozen, Unicode, parity/API/corpus, resource, packaging and script gates.

Other syntax/modes/directives/callouts, labels on failed/partial results, wider
optimizer/resources (#102), UTF16/exported API and realistic consumer/release
qualification remain open. See [design](PCRE-UNSET-BACKREFERENCES-DESIGN.md) and
[plan](PCRE-UNSET-BACKREFERENCES-PLAN.md).

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

Default-false internal AllowLookaroundBackslashK selects native extra0x40. A readonly parser snapshot gates the existing active-assertion K prohibition. Local reset/options cannot change the global policy. The existing canonical reset/capture-start instruction and shared journal provide positive retention, negative/failure restoration, non-atomic retries, conditional and callee behavior. No source rewriting or runtime flag is introduced.

Reported ByteLength remains signed end-start in internal match/capture0 records; lookahead may end before the reported start and lookbehind may report before the requested start. Native EndIndex is authoritative because the wrapper Length clamps reversed spans. Other captures retain their own endpoints. Exported API/enumeration/substitution choices remain separate.

The1,616 new cases include1,574 primary native profiles (1,510 matching/query/capture/success-label and64 compile profiles),1,032 selected offset protocols and16 supplemental work/capacity/implicit-guard profiles. All accepted native matching uses JIT0 and negative matching requires no-match(-1). Fifty-eight primary profiles report reversed group0 endpoints. Two UTF-C profiles outside native JIT support remain separate compile/unsupported boundaries. Another54 native review probes cover selected reset/i/r scopes, UTF/casing prefixes and lexical callee resets.

Tests-only [RED737](https://github.com/uniblab/Icod.Expressions/actions/runs/43444152881), source `552f6672bac9d57214d556d73fb76478429cd340`, Linux ARM job131949099519: net8/9/10 each built with zero compiler warnings/errors, then ran **1,406 expected failures / 79,328 passes / 80,734 total / zero skips**. Only missing-policy cases failed:1,366 primary spans,3 error boundaries,24 offset theories,4 work/ownership cases,2 capacity/depth facts,1 quota fact and6 implicit-guard profiles per framework. All79,118 earlier tests and210 new controls passed. All native literal endpoint assertions passed. Freeze `a6cd825ba357ca8be6f11fd3ff788032f0c24217` before production; no fixture refinements were needed.

See [design](PCRE-LOOKAROUND-RESET-DESIGN.md) and [plan](PCRE-LOOKAROUND-RESET-PLAN.md). All259 earlier files, GNU/frozen35/214 outcomes/120methods and matcher/IR/journal/shared limits stay fixed. Final all18 GREEN/supporting/review/identities live on PR243/issue7.

## PR244: ordinary escaped UTF literal scalars

Shared ParseEscapeCore now sends a valid non-ASCII UTF scalar to existing ParseQuotedLiteral before ASCII dispatch. It consumes exactly one complete validated scalar and returns the ordinary literal; byte mode remains byte-oriented. No source rewrite/new option/IR/opcode/matcher/journal/limit changes. Class/range closure, Unicode17 casing, scopes/repeats/widths, captures/references/calls/assertions and bounded search metadata use existing consumers.

The993 new cases include956 primary native profiles (854 matching/query/capture/success-label and102 compile profiles),1,320 selected offset protocols and15 paired work/capacity profiles. Accepted native matching requires JIT0 and negative matching requires no-match(-1). Four UTF-C profiles outside native JIT support remain separate compile/unsupported controls.

Tests-only [RED739](https://github.com/uniblab/Icod.Expressions/actions/runs/43461757644), source `7198004337030e2d39008bc20843809b0fe5dc4d`, Linux ARM job132010183721: net8/9/10 each built with zero compiler warnings/errors and ran **687 expected failures / 81,040 passes / 81,727 total / zero skips**. Failures were619 primary profiles,35 later-error boundaries,24 offset theories,5 work/ownership cases,2 capacity/depth facts,1 quota and1 strict-subject fact per framework. All80,734 earlier tests and306 new controls passed; every native literal assertion passed. Freeze `1d6cf18667352f51c8d3a245560803af8fe1ab31` before production; no fixture refinements.

Two earlier facts intentionally retire temporary escaped-e-acute UnsupportedSyntax assertions. Their same inputs and caller selections now require pinned native0:2 acceptance and managed parity, with every unrelated quote/C/directive/surrogate guard retained. Both facts remain; the other258 earlier test files stay hash-identical. The frozen new fixture also covers those exact source/caller selections, guarding against a masked regression. No invented caller flag or escape-recovery interpretation is introduced.

Whole-pattern UTF validation, caller/prior/later-error ownership, quotes/comments/raw labels and alternate verb-name decoding remain separate. Canonical hex-source work/cancellation and owned source/labels are pinned. See [design](PCRE-ESCAPED-UTF-LITERALS-DESIGN.md) and [plan](PCRE-ESCAPED-UTF-LITERALS-PLAN.md). Final all18/supporting/review/identities live on PR244/issue7; exported API and consumer/release gates remain open.

## PR245: caller-selected literal compilation

Caller-selected `Literal` treats the entire original pattern as data, including metacharacters, escapes, quoting, inline options, start directives, comments, captures, callouts and verbs. It reads bytes or validated UTF scalars into existing canonical literal nodes, retaining caller casing and the existing word/line wrappers. No source prefix changes UTF/UCP, newline/BSR, notempty or casing settings.

Only the options compatibility predicate, compiler entry guard, global-prefix bypass, parser snapshot/dispatch and focused literal parser partial change. Canonical nodes preserve byte-authoritative captures, work/cancellation, scalar folding, owned source and compiled instruction limits. No alternate matcher or source quoting transformation is introduced.

Pinned PCRE2 10.48 permits caller caseless, UTF/tolerant UTF, word/line and restricted/Turkish casing; newline/BSR remain compile-context choices. Its literal masks allow both casing extras even though the public manpage mentions only word/line. The other31 exposed regex options are rejected with InvalidSyntax at byte0 identifying literal compilation, before source validation. Existing cancellation/nonpositive-work-limit priority and subsequent whole-source UTF/Turkish validation remain shared. Default-false Literal preserves every earlier test hash.

Tests-only [RED743](https://github.com/uniblab/Icod.Expressions/actions/runs/43483771907), source `02f144580c5af853578e682d3a5ae3a648252b2f`, Linux ARM job132086996194: net8/9/10 each built with zero compiler warnings/errors and ran **786 expected failures / 84,574 passes / 85,360 total / zero skips**. Failures were466 primary profiles,316 compatibility/error boundaries,1 work/ownership case and3 structural cases per framework. All81,727 earlier tests and2,847 new controls passed; every native assertion passed. Freeze `842c7045a4a7ed25d1f486f56702644eed8ac53c` before production. Before this clean RED, seven overlaps were deduplicated and the new helper's managed offset argument was corrected; no expected outcome was weakened.

See [design](PCRE-LITERAL-COMPILE-DESIGN.md) and [plan](PCRE-LITERAL-COMPILE-PLAN.md). GNU/BRE/ERE, frozen35/214 outcomes/120methods, IR/opcodes/matcher/journal/shared limits/exported API/dependencies/workflows stay fixed. Final all18 native results and every supporting gate, author review and exact source/base/tree/proposed-merge identities belong on PR245 and only issue7 footer; no documentation-only CI push. UTF-C execution, remaining syntax/modes/callouts/directives, broader102, UTF16/public API and consumer/release gates remain open; Grep last.

## PR246: caller-selected anchored compilation

Default-false caller `Anchored` implements PCRE2_ANCHORED0x80000000 by trying only the requested candidate byte offset. Alternatives, backtracking, assertions, recursive root0/callees, ACCEPT/cuts and labels remain normal at that candidate. A reported start moved by backslash-K may differ from the original candidate. Caller literal compilation, casing, source prefixes and word/line/newline policies remain shared.

The compiler snapshots the option into immutable program policy metadata after ordinary compilation. The matcher gates unanchored byte/suffix/COMMIT entry, admits only the requested candidate and stops after its failure before SKIP/fragment/CRLF retries. Existing UTF/offset validation and fragment context remain intact. The parser, IR nodes/opcodes, scopes/journal and shared ceilings are unchanged; no source-level backslash-G wrapper or subject slicing changes recursion or capacity.

Tests-only [RED746](https://github.com/uniblab/Icod.Expressions/actions/runs/43578255592), source `33ed1a4a6e5344fa53df7aeb03c6c462ab371706`, Linux ARM job132397293572: net8/net9/net10 each built with zero compiler warnings/errors and ran **131 expected failures / 85,554 passes / 85,685 total / zero skips**. Failures were128 protocol groups,2 bounded-byte-budget cases and1 ownership case/framework. Every pinned native assertion, all85,360 earlier tests and194 new controls passed. Freeze `95620f1478537b71880c07c64e20474c3de537ad` before production. Before clean RED, one new byte-range diagnostic control was corrected to preserve its existing hyphen offset2; native108/offset4 and all earlier tests remained unchanged.

See [design](PCRE-ANCHORED-COMPILE-DESIGN.md) and [plan](PCRE-ANCHORED-COMPILE-PLAN.md). All262 prior test files and frozen35 production blobs remain hash-fixed; GNU/BRE/ERE and214 frozen outcomes/120 methods are preserved. Final all18 native results, every configured supporting gate, whole-branch author review and exact source/base/tree/proposed-merge identities belong on PR246 and only issue7 footer, without a documentation-only CI push. ENDANCHORED/FIRSTLINE, UTF-C execution, callouts, remaining syntax/modes/directives, tolerant-surrogate exclusions, failed/partial labels, broader102, NO_UTF_CHECK/UTF16/public API and consumer/release gates remain open; Grep last.

## PR247: caller-selected end-anchored compilation

Default-false caller `EndAnchored` implements PCRE2_ENDANCHORED0x20000000. Root completion must consume through the physical subject end. Short ordinary completion backtracks through existing alternatives; short root `(*ACCEPT)` abandons the entire candidate, discarding its choices/cuts before unanchored restart. Assertion and callee ACCEPT remain scoped. Caller Anchored independently constrains candidate selection, and Literal/casing/word/line/newline/source policies compose normally.

The compiler snapshots EndAnchored alongside Anchored in immutable program metadata after ordinary compilation. Normal ACCEPT gates consumed position against input.Length before publishing captures/labels. Root EarlyAccept checks after synthetic capture closes and scoped exits, discarding saved choices only when short. Tolerant empty patterns use shared candidate traversal under EndAnchored, so fragment ends and reported backslash-K starts never substitute for physical EOF. Parser/root0/callee IR/opcodes/scopes/journal/shared ceilings stay unchanged; no source wrapper or subject slicing.

Tests-only [RED748](https://github.com/uniblab/Icod.Expressions/actions/runs/43585144086), source `7140659a6924969d2fa5dd4ea658cfc49bcc2719`, Linux ARM job132417911226: net8/net9/net10 each built with zero compiler warnings/errors and ran **385 expected failures / 85,865 passes / 86,250 total / zero skips**. Failures were384 missing-policy protocol groups and1 ownership case/framework, all after successful native assertions. All85,685 earlier tests and180 new controls passed. Freeze `2e39a5a936a8358973932a230d53f37abd7c9199` before production; no fixture correction or prior-test change was needed.

See [design](PCRE-END-ANCHORED-COMPILE-DESIGN.md) and [plan](PCRE-END-ANCHORED-COMPILE-PLAN.md). All263 prior test files and frozen35 production blobs remain hash-fixed; GNU/BRE/ERE and214 frozen outcomes/120 methods are preserved. Final all18 native results, configured supporting gates, whole-branch author review and exact source/base/tree/proposed-merge identities belong on PR247 and only issue7 footer, without a documentation-only CI push. FIRSTLINE, UTF-C execution, callouts, remaining syntax/modes/directives, tolerant-surrogate exclusions, failed/partial labels, broader102, NO_UTF_CHECK/UTF16/public API and consumer/release gates remain open; Grep last.
