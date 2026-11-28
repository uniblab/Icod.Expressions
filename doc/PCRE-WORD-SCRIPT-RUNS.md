# Native 16-bit script runs

The internal raw 16-bit and strict UTF-16 engines accept `(*sr:...)`,
`(*script_run:...)`, `(*asr:...)` and `(*atomic_script_run:...)` through the
shared parser. Matching and captures use original code-unit positions. Raw
mode classifies individual units, while strict UTF-16 decodes valid scalars.

## Shared classification

The byte and word walkers share a scalar-fed `PcreScriptRunState`. It retains
the pinned script-extension intersections, Common/Inherited handling, native
directional Han transitions and decimal-set identity rules. The walkers keep
their own native-width navigation and charge every decode and classification.
Classification uses actual consumed subject values and remains independent of
case-folded pattern values, UCP shorthand selection and the host Unicode version.

Empty and single-character runs are valid, including a singleton with Unknown
script. Unknown is rejected in longer runs. A raw surrogate is one unit and can
form a singleton; a raw surrogate pair remains two independently classified
units. Each completed or retried run gets fresh classification state. Nested
runs do not share an intersection or digit-set constraint.

## Scope and retry behavior

The matcher saves the real body-entry position independently of captures and
`\K`. After normal body completion it validates exactly the consumed span.
Ordinary runs retain body choices, so validation failure can retry a shorter
match or another alternative. Atomic runs commit their body choices before
validating, matching the shared parser and native ordering.

For subject `aβ`, these forms demonstrate the distinction:

| Pattern | Native result |
|---|---|
| `\A(*sr:.*)β\z` | Match after the run retries to `a`. |
| `\A(*asr:.*)β\z` | No match after the committed body consumes `aβ`. |
| `\A(?>(*sr:.*))β\z` | Match after validation succeeds inside the outer atomic group. |

Captures, alternatives, repeated runs, calls, conditions, assertions and control
verbs use the existing continuation and capture journals. Scoped `ACCEPT` skips
an unfinished run check just as the native engine skips its body suffixes.
The run wrapper participates in the existing bounded source walk and continuation
ordering. First-unit and LF-entry proofs inspect its body where appropriate.

## Qualification and remaining gates

`PcreWordScriptRunTests` compares complete captures and successful labels with
PCRE2-16 at every valid caller start, under eight compiled policies and all
64 call masks. Cases include all aliases, nested execution, early acceptance,
scope rollback, `\K`, source anchoring, CRLF search and raw surrogate subjects.
Forty-one formerly unsupported source shapes now have explicit admission and
native-result cases in both modes. Existing rejection fixtures retain a finite
repeat beyond the word profile's 64-copy limit.

Additional checks cover all pinned script descriptors and boundaries as
singletons and with paired Latin, Han and combining-mark contexts. All 77 decimal
sets are compared in zero/nine pairs, with UCP on and off, and all pinned context
triples check directional transitions. Resource checks exercise exact budgets,
one-less budgets, cancellation at every observed transition, and reuse in both
dispatch modes. Owned patterns, sliced input, durable captures and concurrent
reuse retain their original-unit contracts. Selected shared Unicode cases use
the independent cross-width coordinate mapper.

Eight package cases bring the word result to 66. Its unsupported-feature sentinel
now uses the still-deferred word code-unit escape. Source length, expansion,
finite-repeat and shared frame limits retain their existing caps. General syntax,
optimizer, hostile-input and public delivery qualification remains open in the
[roadmap](PCRE-ROADMAP-STATUS.md).

The reference remains PCRE.NET 1.6.0 with PCRE2 10.48. See the
[native oracle provenance](../tests/Pcre/Native/README.md) and
[pinned native matcher](https://github.com/PCRE2Project/pcre2/blob/pcre2-10.48/src/pcre2_match.c).
Native word extended-class algebra is the next implementation increment.
