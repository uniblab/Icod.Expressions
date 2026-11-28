# PCRE four-mode admission inventory

This inventory began with [#379](https://github.com/uniblab/Icod.Expressions/pull/379), at merge `cd9ec2a`. The current decisions include [native word UTF/UCP directives](PCRE-WORD-MODE-DIRECTIVES.md). It covers the existing parser's construct families, every current caller-option member, and the retained qualification gates. It does not freeze the public API or claim parity for every combination of admitted constructs. The [roadmap](PCRE-ROADMAP-STATUS.md) schedules implementation from these decisions.

## Reading the decisions

The columns initialize raw bytes, strict UTF-8, raw 16-bit units, or strict UTF-16. Recognized source directives can select UTF or UCP after that initialization, using width-appropriate aliases. Source mode changes are a separate decision from caller option exposure.

| Entry | Meaning |
|---|---|
| S | The exact representative compiles and its match/no-match, full capture vector and successful label agree with the pinned reference on the exercised subjects. |
| D/U0 | Native-valid source is deferred by the managed profile with UnsupportedSyntax at original offset 0. |
| D/I0 | Native-valid source is deferred with InvalidSyntax at original offset 0. |
| R/U0 or R/I0 | The reference rejects this source in that width/mode; the managed compiler also rejects it with the listed diagnostic. |

U means UnsupportedSyntax, I means InvalidSyntax, and the number is the managed original-unit offset. Native error numbers and end-of-token error positions are not equated to managed opener diagnostics. A deferred row is an implementation or qualification gap, not a newly supported feature. S remains subject to source shape, mode and resource limits.

The executable source of these 152 decisions is [PcreFourModeAdmissionCases.cs](../tests/Pcre/PcreFourModeAdmissionCases.cs). [PcreFourModeAdmissionTests.cs](../tests/Pcre/PcreFourModeAdmissionTests.cs) runs all four columns against PCRE.NET 1.6.0 / PCRE2 10.48. It checks each supplied subject, empty input, and a prefixed subject, with duplicates removed. Byte input uses UTF-8 encoding as original bytes even in raw mode; word input uses original UTF-16 units. Raw profiles therefore need not identify the same characters. Each width is compared to its own native oracle, without converting captured coordinates. Signed reset endpoints use the authoritative native end rather than the wrapper's clamped Length.

The table is a compact admission audit. Broader policy, malformed-input, Unicode, coordinate, resource and package suites remain the feature-specific evidence. Reference compilation of a deferred shape is checked even when managed execution is unavailable. The selected-ACCEPT gate's native interpreter error is covered separately by the existing word ACCEPT suite.

## Snapshot counts

These counts describe the named representatives, not a percentage of PCRE completion.

| Mode | Supported representatives | Native-valid deferrals | Native-invalid rejections |
|---|---:|---:|---:|
| Raw8 | 138 | 5 | 9 |
| UTF8 | 140 | 6 | 6 |
| Raw16 | 122 | 22 | 8 |
| UTF16 | 123 | 23 | 6 |

## Atoms

| Fixture ID | Representative source | Raw8 | UTF8 | Raw16 | UTF16 |
|---|---|---|---|---|---|
| `literal` | `a` | S | S | S | S |
| `dot` | `(.)` | S | S | S | S |

## Anchors

| Fixture ID | Representative source | Raw8 | UTF8 | Raw16 | UTF16 |
|---|---|---|---|---|---|
| `absolute-anchors` | `\A(a)\z` | S | S | S | S |
| `line-anchors` | `(?m)^(a)$` | S | S | S | S |
| `final-newline` | `(a)\Z` | S | S | S | S |
| `search-start` | `\G(a)` | S | S | S | S |
| `reset-start` | `a\K(b)` | S | S | S | S |
| `word-boundary` | `\b(a)\B` | S | S | S | S |

## Escapes

| Fixture ID | Representative source | Raw8 | UTF8 | Raw16 | UTF16 |
|---|---|---|---|---|---|
| `hex-octal-control` | `(\x61\o{142}\cC)` | S | S | S | S |
| `quoted` | `\Q(a+)\E` | S | S | S | S |
| `fixed-space` | `(\h\v)` | S | S | S | S |
| `newline-sequence` | `(\R)` | S | S | S | S |
| `grapheme` | `(\X)` | S | S | S | S |
| `non-newline` | `(\N)` | S | S | S | S |
| `code-unit` | `(\C)` | S | D/U1 | D/U1 | D/U1 |
| `unicode-escape` | `\N{U+0061}` | R/I0 | S | R/I0 | S |
| `wide-numeric` | `(\x{100})` | R/I1 | S | S | S |
| `bad-escape` | `\q` | R/U0 | R/U0 | R/U0 | R/U0 |

## Classes

| Fixture ID | Representative source | Raw8 | UTF8 | Raw16 | UTF16 |
|---|---|---|---|---|---|
| `bracket` | `([a-c])` | S | S | S | S |
| `negated-bracket` | `([^a])` | S | S | S | S |
| `shorthand` | `(\d\w\s)` | S | S | S | S |
| `posix` | `([[:alpha:]])` | S | S | S | S |
| `category` | `(\p{L})` | S | S | S | S |
| `script` | `(\p{sc=Latin})` | S | S | S | S |
| `script-extension` | `(\p{scx=Latin})` | S | S | S | S |
| `binary` | `(\p{Alphabetic})` | S | S | S | S |
| `bidi` | `(\p{bc=L})` | S | S | S | S |
| `ucp-shorthand` | `(\w+)`; caller UCP | S | S | S | S |
| `any-property` | `(\p{Any})` | S | S | D/U0 | D/U0 |
| `xuc-property` | `(\p{Xuc})` | S | S | D/U0 | D/U0 |
| `perl-algebra` | `((?[[a-z] - [aeiou]]))` | S | S | S | S |
| `uts-algebra` | `([a-z--[aeiou]])`; caller AltExtendedClass | S | S | S | S |

## Groups

| Fixture ID | Representative source | Raw8 | UTF8 | Raw16 | UTF16 |
|---|---|---|---|---|---|
| `alternation` | `(a\|b)` | S | S | S | S |
| `noncapturing` | `(?:a)` | S | S | S | S |
| `named-angle` | `(?<x>a)` | S | S | S | S |
| `named-quote` | `(?'x'a)` | S | S | S | S |
| `named-python` | `(?P<x>a)` | S | S | S | S |
| `branch-reset` | `(?\|(a)\|(b))` | S | S | S | S |
| `comment` | `(?#note)(a)` | S | S | S | S |
| `duplicate-name` | `(?J)(?<x>a)\|(?<x>b)` | S | S | S | S |
| `atomic` | `(?>a\|ab)b` | S | S | S | S |
| `atomic-alias` | `(*atomic:a\|ab)b` | S | S | S | S |

## Repeats

| Fixture ID | Representative source | Raw8 | UTF8 | Raw16 | UTF16 |
|---|---|---|---|---|---|
| `greedy` | `(a+)` | S | S | S | S |
| `lazy` | `(a+?)` | S | S | S | S |
| `possessive` | `(a++)a` | S | S | S | S |
| `counted` | `(a{1,3})` | S | S | S | S |
| `zero-repeat` | `(a){0}b` | S | S | S | S |
| `brace-literal` | `a{z}` | S | S | S | S |

## References

| Fixture ID | Representative source | Raw8 | UTF8 | Raw16 | UTF16 |
|---|---|---|---|---|---|
| `numbered-reference` | `(a)\1` | S | S | S | S |
| `named-reference` | `(?<x>a)\k<x>` | S | S | S | S |
| `relative-reference` | `(a)\g{-1}` | S | S | S | S |

## Calls

| Fixture ID | Representative source | Raw8 | UTF8 | Raw16 | UTF16 |
|---|---|---|---|---|---|
| `named-call` | `(?(DEFINE)(?<x>a))(?&x)` | S | S | S | S |
| `numbered-call` | `(a)(?1)` | S | S | S | S |
| `relative-call` | `(a)(?-1)` | S | S | S | S |
| `recursive-call` | `(?(DEFINE)(?<x>a(?&x)?))(?&x)` | S | S | S | S |
| `selected-returns` | `(?(DEFINE)(?<g>(a)))(?&g(2))\2` | S | S | S | S |

## Assertions

| Fixture ID | Representative source | Raw8 | UTF8 | Raw16 | UTF16 |
|---|---|---|---|---|---|
| `positive-ahead` | `(?=(a))a` | S | S | S | S |
| `negative-ahead` | `(?!(b))a` | S | S | S | S |
| `positive-behind` | `(?<=(a))b` | S | S | S | S |
| `negative-behind` | `(?<!(b))a` | S | S | S | S |
| `variable-behind` | `(?<=a{1,2})b` | S | S | S | S |
| `nonatomic-ahead` | `(?*(a\|ab))\1c` | S | S | S | S |
| `nonatomic-behind` | `(?<*(a))b` | S | S | S | S |
| `alias-pla` | `(*pla:a)a` | S | S | S | S |
| `alias-positive_lookahead` | `(*positive_lookahead:a)a` | S | S | S | S |
| `alias-nla` | `(*nla:b)a` | S | S | S | S |
| `alias-negative_lookahead` | `(*negative_lookahead:b)a` | S | S | S | S |
| `alias-plb` | `(*plb:a)a` | S | S | S | S |
| `alias-positive_lookbehind` | `(*positive_lookbehind:a)a` | S | S | S | S |
| `alias-nlb` | `(*nlb:b)a` | S | S | S | S |
| `alias-negative_lookbehind` | `(*negative_lookbehind:b)a` | S | S | S | S |
| `alias-napla` | `(*napla:a)a` | S | S | S | S |
| `alias-non_atomic_positive_lookahead` | `(*non_atomic_positive_lookahead:a)a` | S | S | S | S |
| `alias-naplb` | `(*naplb:a)a` | S | S | S | S |
| `alias-non_atomic_positive_lookbehind` | `(*non_atomic_positive_lookbehind:a)a` | S | S | S | S |
| `reset-in-assertion` | `(?=a\K)` | R/I0 | R/I0 | R/I0 | R/I0 |
| `allowed-reset-in-assertion` | `(?=a\K)`; caller AllowLookaroundBackslashK | S | S | S | S |

## Conditions

| Fixture ID | Representative source | Raw8 | UTF8 | Raw16 | UTF16 |
|---|---|---|---|---|---|
| `capture-condition` | `(a)?(?(1)b\|c)` | S | S | S | S |
| `assertion-condition` | `(?(?=(a))a\|b)` | S | S | S | S |
| `recursion-condition` | `(?(DEFINE)(?<x>a(?(R)a\|b)))(?&x)` | S | S | S | S |
| `version-condition` | `(?(VERSION>=10.48)a\|b)` | S | S | S | S |
| `definition` | `(?(DEFINE)(?<x>a))(?&x)` | S | S | S | S |

## Scripts

| Fixture ID | Representative source | Raw8 | UTF8 | Raw16 | UTF16 |
|---|---|---|---|---|---|
| `script-sr` | `(*sr:(a+))` | S | S | S | S |
| `script-script_run` | `(*script_run:(a+))` | S | S | S | S |
| `script-asr` | `(*asr:(a+))` | S | S | S | S |
| `script-atomic_script_run` | `(*atomic_script_run:(a+))` | S | S | S | S |

## Verbs

| Fixture ID | Representative source | Raw8 | UTF8 | Raw16 | UTF16 |
|---|---|---|---|---|---|
| `fail` | `(*FAIL)\|a` | S | S | S | S |
| `fail-alias` | `(*F)\|a` | S | S | S | S |
| `accept` | `(a)(*ACCEPT)z` | S | S | S | S |
| `mark` | `(*MARK:M)(a)` | S | S | S | S |
| `mark-alias` | `(*:M)(a)` | S | S | S | S |
| `prune` | `a(*PRUNE)b\|ac` | S | S | S | S |
| `commit` | `a(*COMMIT)b\|ac` | S | S | S | S |
| `then` | `a(*THEN)b\|ac` | S | S | S | S |
| `skip` | `a(*SKIP)b\|a` | S | S | S | S |
| `named-skip` | `a(*MARK:M)(*SKIP:M)b\|a` | S | S | S | S |
| `named-prune` | `a(*PRUNE:M)b\|ac` | S | S | S | S |
| `named-commit` | `a(*COMMIT:M)b\|ac` | S | S | S | S |
| `named-then` | `a(*THEN:M)b\|ac` | S | S | S | S |
| `named-accept` | `(a)(*ACCEPT:M)z` | S | S | S | S |
| `named-fail` | `(*FAIL:M)\|a` | S | S | S | S |

## Inline options

| Fixture ID | Representative source | Raw8 | UTF8 | Raw16 | UTF16 |
|---|---|---|---|---|---|
| `scope-i` | `(?i:(a))` | S | S | S | S |
| `scope-m` | `(?m:^(a)$)` | S | S | S | S |
| `scope-n` | `(?n)(a)(?<x>b)` | S | S | S | S |
| `scope-s` | `(?s:(.))` | S | S | S | S |
| `scope-U` | `(?U:(a+))` | S | S | S | S |
| `scope-J` | `(?J)(?<x>a)\|(?<x>b)` | S | S | S | S |
| `scope-r` | `(?ir:(k))` | S | S | S | S |
| `scope-x` | `(?x) (a) #note` | S | S | S | S |
| `scope-xx` | `(?xx)[ a ]` | S | S | S | S |
| `scope-a` | `(?a)(w)` | S | S | S | S |
| `scope-aD` | `(?aD)(d)` | S | S | S | S |
| `scope-aS` | `(?aS)(s)` | S | S | S | S |
| `scope-aW` | `(?aW)(w)` | S | S | S | S |
| `scope-aP` | `(?aP)([[:alpha:]])` | S | S | S | S |
| `scope-aT` | `(?aT)([[:xdigit:]])` | S | S | S | S |
| `scope-reset` | `(?is)(?^)(a)` | S | S | S | S |

## Directives

| Fixture ID | Representative source | Raw8 | UTF8 | Raw16 | UTF16 |
|---|---|---|---|---|---|
| `prefix-LF` | `(*LF)(a)` | S | S | S | S |
| `prefix-CR` | `(*CR)(a)` | S | S | S | S |
| `prefix-NUL` | `(*NUL)(a)` | S | S | S | S |
| `prefix-CRLF` | `(*CRLF)(a)` | S | S | S | S |
| `prefix-ANYCRLF` | `(*ANYCRLF)(a)` | S | S | S | S |
| `prefix-ANY` | `(*ANY)(a)` | S | S | S | S |
| `prefix-BSR_UNICODE` | `(*BSR_UNICODE)(a)` | S | S | S | S |
| `prefix-BSR_ANYCRLF` | `(*BSR_ANYCRLF)(a)` | S | S | S | S |
| `prefix-UTF` | `(*UTF)(a)` | S | S | S | S |
| `prefix-UCP` | `(*UCP)(a)` | S | S | S | S |
| `prefix-CASELESS_RESTRICT` | `(*CASELESS_RESTRICT)(a)` | S | S | D/U0 | D/U0 |
| `prefix-NO_JIT` | `(*NO_JIT)(a)` | S | S | D/U0 | D/U0 |
| `prefix-NO_AUTO_POSSESS` | `(*NO_AUTO_POSSESS)(a)` | S | S | D/U0 | D/U0 |
| `prefix-NO_START_OPT` | `(*NO_START_OPT)(a)` | S | S | D/U0 | D/U0 |
| `prefix-NO_DOTSTAR_ANCHOR` | `(*NO_DOTSTAR_ANCHOR)(a)` | S | S | D/U0 | D/U0 |
| `prefix-NOTEMPTY` | `(*NOTEMPTY)(a)` | S | S | D/U0 | D/U0 |
| `prefix-NOTEMPTY_ATSTART` | `(*NOTEMPTY_ATSTART)(a)` | S | S | D/U0 | D/U0 |
| `prefix-UTF8` | `(*UTF8)(a)` | S | S | R/U0 | R/U0 |
| `prefix-UTF16` | `(*UTF16)(a)` | R/U0 | R/U0 | S | S |
| `prefix-UTF32` | `(*UTF32)(a)` | R/U0 | R/U0 | R/U0 | R/U0 |
| `prefix-TURKISH_CASING` | `(*TURKISH_CASING)(?i)(i)` | R/I17 | S | R/U0 | D/U0 |
| `limit-MATCH` | `(*LIMIT_MATCH=10000)a` | D/U0 | D/U0 | D/U0 | D/U0 |
| `limit-DEPTH` | `(*LIMIT_DEPTH=10000)a` | D/U0 | D/U0 | D/U0 | D/U0 |
| `limit-HEAP` | `(*LIMIT_HEAP=10000)a` | D/U0 | D/U0 | D/U0 | D/U0 |

## Callouts

| Fixture ID | Representative source | Raw8 | UTF8 | Raw16 | UTF16 |
|---|---|---|---|---|---|
| `callout-number` | `(?C2)a` | D/U0 | D/U0 | D/U0 | D/U0 |
| `callout-string` | `(?C'check')a` | D/U0 | D/U0 | D/U0 | D/U0 |

## Admission bounds

| Fixture ID | Representative source | Raw8 | UTF8 | Raw16 | UTF16 |
|---|---|---|---|---|---|
| `repeat-cap` | `a{65}` | S | S | D/U0 | D/U0 |
| `leaf-cap` | `[65 source members]` | S | S | D/U0 | D/U0 |
| `depth-cap` | `65 nested (?:...) groups` | S | S | D/U0 | D/U0 |
| `erased-cap` | `(?:a{65}){0}a` | S | S | D/U0 | D/U0 |
| `uncalled-cap` | `(?(DEFINE)(?<x>a{65}))a` | S | S | D/U0 | D/U0 |

## Qualification gates

| Fixture ID | Representative source | Raw8 | UTF8 | Raw16 | UTF16 |
|---|---|---|---|---|---|
| `selected-accept-gate` | `\A(?(DEFINE)(?<x>(a)(*ACCEPT:N)b))(?&x(2))\2` | S | S | D/U0 | D/U0 |
| `asserted-commit-gate` | `(*COMMIT:N)(?=a).` | S | S | D/U0 | D/U0 |

## Invalid syntax

| Fixture ID | Representative source | Raw8 | UTF8 | Raw16 | UTF16 |
|---|---|---|---|---|---|
| `malformed-group` | `a(` | R/I1 | R/I1 | R/I1 | R/I1 |
| `misplaced-directive` | `a(*LF)` | R/U1 | R/U1 | R/U1 | R/U1 |

## Caller option exposure

The following 55 rows reconcile every settable member of PcreByteOptions, including its internal work observer. A named word member is an exposed binding, not a promise that all native option combinations are qualified. A dash is an unexposed word caller option. Both word modes have the same option surface. The six Policy members are also checked independently against PcreMatchPolicy. Reflection guards make an added, removed or renamed option fail until this inventory is updated.

[Option decisions](../tests/Pcre/PcreFourModeOptionCases.cs) identify the existing byte qualification suite. Word behavior is additionally covered by the word casing, UCP, ASCII, match-policy, literal, reset, algebra and resource suites. Their bounded contracts remain authoritative.

| Byte member (raw8/UTF8) | Word binding (raw16/UTF16) | Existing byte evidence | Constraint or next work |
|---|---|---|---|
| `MatchWorkLimit` | `MatchWorkLimit` | [PcreResourceContractTests](../tests/Pcre/PcreResourceContractTests.cs) | Managed work units, independent of native match/depth/heap counters. |
| `MaxVarLookbehind` | `-` | [PcreVariableLookbehindLimitReferenceTests](../tests/Pcre/PcreVariableLookbehindLimitReferenceTests.cs) | Word profile uses the fixed 255-character variable-behind ceiling. |
| `MaxPatternLength` | `MaxPatternLength` | [PcrePatternLengthLimitReferenceTests](../tests/Pcre/PcrePatternLengthLimitReferenceTests.cs) | Word source also retains its independent hard cap of 4096 units. |
| `Anchored` | `Policy.Anchored` | [PcreMatchPolicyTests](../tests/Pcre/PcreMatchPolicyTests.cs) | Word defaults use Policy; both widths also accept per-call policy. |
| `EndAnchored` | `Policy.EndAnchored` | [PcreMatchPolicyTests](../tests/Pcre/PcreMatchPolicyTests.cs) | Word defaults use Policy; both widths also accept per-call policy. |
| `NotEmpty` | `Policy.NotEmpty` | [PcreCallerEmptyMatchReferenceTests](../tests/Pcre/PcreCallerEmptyMatchReferenceTests.cs) | Word defaults use Policy; both widths also accept per-call policy. |
| `NotEmptyAtStart` | `Policy.NotEmptyAtStart` | [PcreCallerEmptyMatchReferenceTests](../tests/Pcre/PcreCallerEmptyMatchReferenceTests.cs) | Word defaults use Policy; both widths also accept per-call policy. |
| `NotBol` | `Policy.NotBol` | [PcreCallerLineBoundaryReferenceTests](../tests/Pcre/PcreCallerLineBoundaryReferenceTests.cs) | Word defaults use Policy; both widths also accept per-call policy. |
| `NotEol` | `Policy.NotEol` | [PcreCallerLineBoundaryReferenceTests](../tests/Pcre/PcreCallerLineBoundaryReferenceTests.cs) | Word defaults use Policy; both widths also accept per-call policy. |
| `FirstLine` | `-` | [PcreFirstLineCompileReferenceTests](../tests/Pcre/PcreFirstLineCompileReferenceTests.cs) | Retain existing byte semantics; qualify missing word caller exposure separately. |
| `Literal` | `@CompileLiteral` | [PcreLiteralCompileReferenceTests](../tests/Pcre/PcreLiteralCompileReferenceTests.cs) | Explicit word entry point. Some flags (e.g. AltExtendedClass) are inert there; native LITERAL conflict parity remains deferred. |
| `NoAutoPossess` | `-` | [PcreNoAutoPossessReferenceTests](../tests/Pcre/PcreNoAutoPossessReferenceTests.cs) | Retain existing byte semantics; qualify missing word caller exposure separately. |
| `NoStartOptimize` | `-` | [PcreNoStartOptimizeReferenceTests](../tests/Pcre/PcreNoStartOptimizeReferenceTests.cs) | Retain existing byte semantics; qualify missing word caller exposure separately. |
| `NoDotStarAnchor` | `-` | [PcreNoDotStarAnchorReferenceTests](../tests/Pcre/PcreNoDotStarAnchorReferenceTests.cs) | Retain existing byte semantics; qualify missing word caller exposure separately. |
| `Caseless` | `Caseless` | [PcreCallerCasePolicyReferenceTests](../tests/Pcre/PcreCallerCasePolicyReferenceTests.cs) | Retain the existing width-specific contract and qualification. |
| `CaselessRestricted` | `CaselessRestricted` | [PcreCallerCasePolicyReferenceTests](../tests/Pcre/PcreCallerCasePolicyReferenceTests.cs) | Retain the existing width-specific contract and qualification. |
| `TurkishCasing` | `-` | [PcreTurkishCasingReferenceTests](../tests/Pcre/PcreTurkishCasingReferenceTests.cs) | Byte support requires effective UTF and excludes global restricted casing. |
| `DotAll` | `-` | [PcreCallerPatternOptionReferenceTests](../tests/Pcre/PcreCallerPatternOptionReferenceTests.cs) | Word inline scopes cover i/m/n/s/U/J/r/x/xx; caller exposure is a separate decision. |
| `Multiline` | `-` | [PcreCallerPatternOptionReferenceTests](../tests/Pcre/PcreCallerPatternOptionReferenceTests.cs) | Word inline scopes cover i/m/n/s/U/J/r/x/xx; caller exposure is a separate decision. |
| `DollarEndOnly` | `-` | [PcreCallerPatternOptionReferenceTests](../tests/Pcre/PcreCallerPatternOptionReferenceTests.cs) | Word inline scopes cover i/m/n/s/U/J/r/x/xx; caller exposure is a separate decision. |
| `AltCircumflex` | `-` | [PcreCallerPatternOptionReferenceTests](../tests/Pcre/PcreCallerPatternOptionReferenceTests.cs) | Word inline scopes cover i/m/n/s/U/J/r/x/xx; caller exposure is a separate decision. |
| `NoAutoCapture` | `-` | [PcreCallerPatternOptionReferenceTests](../tests/Pcre/PcreCallerPatternOptionReferenceTests.cs) | Word inline scopes cover i/m/n/s/U/J/r/x/xx; caller exposure is a separate decision. |
| `Ungreedy` | `-` | [PcreCallerPatternOptionReferenceTests](../tests/Pcre/PcreCallerPatternOptionReferenceTests.cs) | Word inline scopes cover i/m/n/s/U/J/r/x/xx; caller exposure is a separate decision. |
| `DuplicateNames` | `-` | [PcreCallerPatternOptionReferenceTests](../tests/Pcre/PcreCallerPatternOptionReferenceTests.cs) | Word inline scopes cover i/m/n/s/U/J/r/x/xx; caller exposure is a separate decision. |
| `MatchUnsetBackreference` | `-` | [PcreCallerPatternOptionReferenceTests](../tests/Pcre/PcreCallerPatternOptionReferenceTests.cs) | Word inline scopes cover i/m/n/s/U/J/r/x/xx; caller exposure is a separate decision. |
| `Extended` | `-` | [PcreCallerPatternOptionReferenceTests](../tests/Pcre/PcreCallerPatternOptionReferenceTests.cs) | Word inline scopes cover i/m/n/s/U/J/r/x/xx; caller exposure is a separate decision. |
| `ExtendedMore` | `-` | [PcreCallerPatternOptionReferenceTests](../tests/Pcre/PcreCallerPatternOptionReferenceTests.cs) | Word inline scopes cover i/m/n/s/U/J/r/x/xx; caller exposure is a separate decision. |
| `MatchWord` | `-` | [PcreWordLineModeReferenceTests](../tests/Pcre/PcreWordLineModeReferenceTests.cs) | Implicit word/line wrappers are not word-engine caller options. |
| `MatchLine` | `-` | [PcreWordLineModeReferenceTests](../tests/Pcre/PcreWordLineModeReferenceTests.cs) | Implicit word/line wrappers are not word-engine caller options. |
| `Utf8` | `Utf16` | [PcreUtfStartDirectiveReferenceTests](../tests/Pcre/PcreUtfStartDirectiveReferenceTests.cs) | Width-native mode selector; not a subject conversion. |
| `MatchInvalidUtf` | `-` | [PcreInvalidUtfCaptureReferenceTests](../tests/Pcre/PcreInvalidUtfCaptureReferenceTests.cs) | Tolerant UTF16 needs its own validation-window and recovery contract. |
| `NeverUtf` | `-` | [PcreCallerCompileLockoutReferenceTests](../tests/Pcre/PcreCallerCompileLockoutReferenceTests.cs) | Retain existing byte semantics; qualify missing word caller exposure separately. |
| `NeverUcp` | `-` | [PcreCallerCompileLockoutReferenceTests](../tests/Pcre/PcreCallerCompileLockoutReferenceTests.cs) | Retain existing byte semantics; qualify missing word caller exposure separately. |
| `NeverBackslashC` | `-` | [PcreCallerCompileLockoutReferenceTests](../tests/Pcre/PcreCallerCompileLockoutReferenceTests.cs) | Retain existing byte semantics; qualify missing word caller exposure separately. |
| `Ucp` | `Ucp` | [PcreCallerAsciiPolicyReferenceTests](../tests/Pcre/PcreCallerAsciiPolicyReferenceTests.cs) | Retain the existing width-specific contract and qualification. |
| `NeverCallout` | `-` | [PcreNeverCalloutReferenceTests](../tests/Pcre/PcreNeverCalloutReferenceTests.cs) | Retain existing byte semantics; qualify missing word caller exposure separately. |
| `AllowLookaroundBackslashK` | `AllowLookaroundBackslashK` | [PcreLookaroundResetReferenceTests](../tests/Pcre/PcreLookaroundResetReferenceTests.cs) | Retain the existing width-specific contract and qualification. |
| `AltExtendedClass` | `AltExtendedClass` | [PcreUtsClassAlgebraReferenceTests](../tests/Pcre/PcreUtsClassAlgebraReferenceTests.cs) | Retain the existing width-specific contract and qualification. |
| `AltBsux` | `-` | [PcreAltBsuxReferenceTests](../tests/Pcre/PcreAltBsuxReferenceTests.cs) | Retain existing byte semantics; qualify missing word caller exposure separately. |
| `ExtraAltBsux` | `-` | [PcreExtraAltBsuxReferenceTests](../tests/Pcre/PcreExtraAltBsuxReferenceTests.cs) | Retain existing byte semantics; qualify missing word caller exposure separately. |
| `AltVerbNames` | `-` | [PcreAltVerbNameReferenceTests](../tests/Pcre/PcreAltVerbNameReferenceTests.cs) | Retain existing byte semantics; qualify missing word caller exposure separately. |
| `AllowSurrogateEscapes` | `-` | [PcreSurrogateEscapeReferenceTests](../tests/Pcre/PcreSurrogateEscapeReferenceTests.cs) | Escaped surrogate policy is independent of malformed UTF subject recovery. |
| `BadEscapeIsLiteral` | `-` | [PcreBadEscapeLiteralReferenceTests](../tests/Pcre/PcreBadEscapeLiteralReferenceTests.cs) | Retain existing byte semantics; qualify missing word caller exposure separately. |
| `AsciiBsD` | `AsciiBsD` | [PcreCallerAsciiPolicyReferenceTests](../tests/Pcre/PcreCallerAsciiPolicyReferenceTests.cs) | Retain the existing width-specific contract and qualification. |
| `AsciiBsS` | `AsciiBsS` | [PcreCallerAsciiPolicyReferenceTests](../tests/Pcre/PcreCallerAsciiPolicyReferenceTests.cs) | Retain the existing width-specific contract and qualification. |
| `AsciiBsW` | `AsciiBsW` | [PcreCallerAsciiPolicyReferenceTests](../tests/Pcre/PcreCallerAsciiPolicyReferenceTests.cs) | Retain the existing width-specific contract and qualification. |
| `AsciiPosix` | `AsciiPosix` | [PcreCallerAsciiPolicyReferenceTests](../tests/Pcre/PcreCallerAsciiPolicyReferenceTests.cs) | Retain the existing width-specific contract and qualification. |
| `AsciiDigit` | `AsciiDigit` | [PcreCallerAsciiPolicyReferenceTests](../tests/Pcre/PcreCallerAsciiPolicyReferenceTests.cs) | Retain the existing width-specific contract and qualification. |
| `PythonOctal` | `-` | [PcreNumericEscapeOptionReferenceTests](../tests/Pcre/PcreNumericEscapeOptionReferenceTests.cs) | Retain existing byte semantics; qualify missing word caller exposure separately. |
| `NoBackslashZero` | `-` | [PcreNumericEscapeOptionReferenceTests](../tests/Pcre/PcreNumericEscapeOptionReferenceTests.cs) | Retain existing byte semantics; qualify missing word caller exposure separately. |
| `EscapedCrIsLf` | `-` | [PcreEscapedCrReferenceTests](../tests/Pcre/PcreEscapedCrReferenceTests.cs) | Retain existing byte semantics; qualify missing word caller exposure separately. |
| `AllowEmptyClass` | `-` | [PcreEmptyClassReferenceTests](../tests/Pcre/PcreEmptyClassReferenceTests.cs) | Retain existing byte semantics; qualify missing word caller exposure separately. |
| `AnyCrLfNewline` | `-` | [PcreStartDirectiveReferenceTests](../tests/Pcre/PcreStartDirectiveReferenceTests.cs) | Word source newline/BSR directives exist; caller context settings remain deferred. |
| `BsrAnyCrLf` | `-` | [PcreStartDirectiveReferenceTests](../tests/Pcre/PcreStartDirectiveReferenceTests.cs) | Word source newline/BSR directives exist; caller context settings remain deferred. |
| `WorkObserver` | `WorkObserver` | [PcreResourceContractTests](../tests/Pcre/PcreResourceContractTests.cs) | Internal deterministic cancellation hook, not a PCRE2 option. |

## Admission limits and independent deferrals

| Gate | Current decision and evidence |
|---|---|
| Word source and expansion | At most 4,096 source units, 64 source group levels, 64 branches/choice products, 64 finite copies, 64 unbounded repeats and 4,096 expanded nodes. These are bounded-profile limits, not general PCRE2 syntax errors. [Compiler](../src/Pcre/PcreWordSequenceCompiler.cs) and the admission-bound rows pin representative exclusions, including uncalled definitions. |
| Word classes | Ordinary leaves retain the 64-member admission rule and their existing stored-member limit. Extended classes cap source members at 64 per leaf and aggregate folded members and terms independently at 4,096. Erased and uncalled bodies are validated. [Algebra evidence](PCRE-WORD-CLASS-ALGEBRA.md) |
| Shared parser/resources | Parser depth/capture limits remain separate from word lowering. Both execution paths bound frames, journals, work and cancellation; word retries/calls/history share 1,024 frames. Native LIMIT_MATCH/DEPTH/HEAP directives remain deferred; managed work units must not be relabeled as native counters. [Resource contract](PCRE-RESOURCE-ERROR-CONTRACT.md) |
| Standalone Any/Xuc on words | Deferred by the ordinary word predicate gate. Both predicates already work as extended-class operands. Admit ordinary forms only with their own raw/UTF, negation, repeat, search and native-width cases. |
| Code-unit escapes | Raw8 execution is admitted. Raw16 and UTF code-unit execution remain deferred. Raw16 is a separate, smaller increment; UTF requires split-scalar capture, restart, validation and JIT/interpreter decisions. [Byte evidence](PCRE-BYTE-CODE-UNIT-DESIGN.md) |
| ACCEPT with selected returns | Explicitly deferred on words, including erased definitions. The pinned word interpreter can return INTERNAL (-44) where JIT succeeds. Preserve the exclusion until dispatch and capture-return behavior have a contract. [Word ACCEPT](PCRE-WORD-ACCEPT.md) |
| Asserted-only COMMIT entry | Explicitly deferred when the first-unit proof comes only from a positive assertion. Required-unit and start-bitmap interactions need separate qualification. [Word cuts](PCRE-WORD-CUTS.md) |
| Tolerant UTF16 and NO_UTF_CHECK | Neither is exposed. Specify original-unit recovery, validation windows, malformed starts/captures and reuse before adding either policy. Strict whole-source and subject-window behavior remains covered by the word suites. [Malformed width tests](../tests/Pcre/PcreDualWidthMalformedSubjectTests.cs) |
| Executable and automatic callouts | Deferred. Manual callout tokens are rejected in all four modes. Native acceptance without a callback is not callback support; callback data, continuation, cancellation and exception behavior still need design. |
| Partial, iteration, substitution | No public PCRE delivery claim. Soft/hard partial results, empty-match retry, replacement language and bounded native-width output remain P13 work. |
| Byte optimizer and malformed fragments | Existing #102 start-search differences and #100's bounded valid-fragment contract remain separate gates. This audit does not close them. [Byte contract](PCRE-INTERNAL-CONTRACT.md) |

## Implementation sequence from this audit

**Completed: word global mode resolution before validation.** Native UTF/UTF16 and UCP prefixes now select the effective word mode using the owned original source. Width-inappropriate aliases remain rejected; validation, literal-entry independence, original offsets and caller/source behavior have native fixtures. [Implementation evidence](PCRE-WORD-MODE-DIRECTIVES.md)
1. **Add remaining word global policies in bounded groups.** CASELESS_RESTRICT and NOTEMPTY/ATSTART can use existing comparisons and acceptance policy. Treat NO_JIT and optimizer directives as separate dispatch/search qualification, with the existing JIT/interpreter capture, label and restart distinctions. Turkish casing needs its own scalar-comparison work.
2. **Close caller exposure and small primitive gaps.** Start with ordinary Any/Xuc and raw16 code-unit matching, then expose the relevant source-supported caller flags and newline/BSR context choices. Preserve literal option-combination distinctions until their contract is qualified.
3. **Reconcile admission and deferred execution.** Address selected-return ACCEPT, asserted COMMIT, resource counters, broader lowering and UTF code-unit behavior with reduced native fixtures. Tolerant UTF16, NO_UTF_CHECK and callback execution remain independent decisions.
4. **Freeze a supported profile and public delivery.** Expand cross-width/native fixtures and controlled-host measurements; finish partial/iteration/substitution; freeze the public API and ordinary package consumers. Grep remains the last migration.

The next implementation PR adds word CASELESS_RESTRICT and NOTEMPTY/ATSTART. The required word package gate is now 82 protocols.
