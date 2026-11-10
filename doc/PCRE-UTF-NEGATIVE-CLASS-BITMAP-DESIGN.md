# Negative class assertion bodies before nullable group repeats

Continue issue #102 after merged PR203 under the standing instruction to
implement successive tranches without design-approval pauses. Keep the existing
bounded optional-group proof and its mandatory CR-excluding repeated-group
head. PCRE2 10.48 `set_start_bits` skips negative ahead/behind bodies; their
class membership does not determine the following consuming start bitmap.

## Approach

Extend `HasUtfCrLfNegativeClassAssertionBitmap` to all already-parsed scalar
class kinds and scalar-set unions, including newline-containing positive sets,
Unicode predicates and their complements. Validate and charge set ranges and
predicates under cancellation and the existing 1,000-visit ceiling; commit
prefix charges only on success. Keep the one-branch/one-node assertion shape,
supported leading/middle positions, and repeated-group mandatory-head proof.

This completes the class-body category within that bounded shape. Repeated,
grouped and multi-node assertion bodies remain outside this increment. Keep
positive assertion admission unchanged. Changing parser newline provenance or
matcher bumpalong globally would affect unrelated patterns and duplicate the
compiler proof; those alternatives are rejected. No public API, limits,
dependencies, frozen source, GNU semantics, consumers, or release changes.

## Evidence and constraints

Pin PCRE2 10.48 JIT start bitmaps and full native spans before source changes.
Add tests first for newline-containing ranges, scalar types, Unicode predicates,
positive/negative predicate unions, caseful/caseless and UCP modes, leading and
middle lookahead/lookbehind, five nullable repeat shapes, CRLF and ordinary
subjects. Preserve the test blob after successful-build RED in CI.

Use C#13/net8/9/10 and six native Staging OS/architecture runners. No subagents,
Python, SDK or PowerShell installation. Local native C probes are temporary
oracle research; managed validation uses CI. Preserve all40499 baseline tests,
frozen214 expectations and frozen35 source. Issue102 remains open; Grep last.
