# Native 16-bit extended-class algebra

The internal raw16 and strict UTF16 profiles accept Perl `(?[...])` classes
and caller-selected `AltExtendedClass` UTS bracket algebra. Union,
intersection, subtraction, XOR and complement retain the shared parser's
precedence and nesting rules. Ordinary brackets keep their existing meaning
when the option is false. Literal compilation continues to treat brackets
and operators as literal source.

## Original-unit evaluation

Both word modes retain scalar-expression terms directly. Raw mode consumes
one 16-bit unit, including an isolated surrogate. UTF16 decodes one valid
scalar and consumes its original one or two code units. Neither path projects
the expression onto the byte universe or converts the subject to another width.
Complement therefore includes high raw units and supplementary UTF scalars.

The byte and word matchers share an iterative postfix evaluator. Each operand
applies its own case closure before Boolean operations. Raw mode uses ASCII
folding unless UCP is enabled; strict UTF16 uses the pinned Unicode folds.
Restricted casing remains lexical. Range closure in raw UCP mode stays within
the BMP. Properties, including expression operands `Any` and `Xuc`, use the
shared pinned classifier on the actual consumed value.

Each Match owns its Boolean scratch buffer. Evaluation overwrites the live
prefix, so candidates and retry frames retain no expression state. Every term,
range and predicate shares the match work counter, observer and cancellation
token. Captures, references, calls, assertions, repetition, script runs and
control verbs continue through the existing word continuation journal.

The bounded LF-entry study evaluates expression membership for CR and LF.
This preserves native search entry inside CRLF when a leading expression can
start with LF but cannot start with CR, including ranges without an explicit
newline literal. Literal newline provenance remains separate from membership.

## Admission and qualification

The existing 4,096-unit source cap, finite-repeat limits and shared 1,024-frame
limit remain. Source leaves admit at most 64 members. A separate aggregate
4,096-member bound covers stored ranges and predicates after case closure;
terms have their own aggregate 4,096 bound. Validation visits erased repeats
and uncalled definitions. The parser retains its 15-level expression nesting
bound. Parse errors and admission errors retain original word offsets.

`PcreWordClassAlgebraTests` compares operators, nested grammar, captures,
scope rollback and successful labels with PCRE2-16 at every valid caller start,
under eight compiled policies and all 64 call masks. Targeted native checks
cover raw high units and surrogates, supplementary scalars, UCP on/off and
ordinary/restricted casing. Independent frozen Unicode masks qualify all
543 existing expressions over U+0000–00FF and 24 property expressions across
the pinned scalar boundary corpus. The byte algebra reference suites also
exercise the shared evaluator.

Resource checks pin exact and one-less budgets, cancellation at every observed
step, subsequent reuse and aggregate admission. Owned source, sliced subjects,
durable results and concurrent Match calls retain their original-unit contracts.
Selected Unicode fixtures compare byte and word captures through the independent
coordinate mapper. Eight package cases extend the required word result to 74.

The reference remains PCRE.NET 1.6.0 with PCRE2 10.48. The native16 oracle passes
`PCRE2_ALT_EXTENDED_CLASS` as its compile option without changing the native ABI.
See [oracle provenance](../tests/Pcre/Native/README.md) and the
[pinned header](https://github.com/PCRE2Project/pcre2/blob/pcre2-10.48/src/pcre2.h.generic).
The next increment inventories syntax, directives and options across all four
modes. Wider admission, malformed-input policy, optimizer/resource conformance
and public PCRE delivery remain open in the [roadmap](PCRE-ROADMAP-STATUS.md).
