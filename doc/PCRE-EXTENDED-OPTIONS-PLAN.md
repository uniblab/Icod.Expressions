# Internal PCRE extended options implementation plan

**Goal:** Support pattern-side x/xx and comments without changing runtime matching.

**Architecture:** Source-local trivia readers and extended-mode snapshots in the
existing heap parser frames. Existing atoms and repeat/capture nodes remain the
only compiler input.

**Tech stack:** C# 13; net8/net9/net10; pinned PCRE.NET 1.6.0 / PCRE2 10.48.

**Spec:** [PCRE-EXTENDED-OPTIONS-DESIGN.md](PCRE-EXTENDED-OPTIONS-DESIGN.md).

## Constraints and review focus

- Root project/solution, src/tests/samples; UTF-8 LF; Debug locally, Staging PR.
- No public API/version/dependency, consumer, matcher or runtime-frame changes.
- No preprocessing or duplicate byte/UTF parser: retain source-byte indexes.
- x/xx restoration must happen before surrounding quantifiers are parsed.
- Trivia cannot split a token or make escaped/quoted/class text into comments.
- xx range and leading bracket syntax must retain their PCRE2 meanings.
- Explicit LF/AnyCRLF must control comments independently of BSR and DotAll.

## Task 1: Reference rules and tests

Read pinned docs/source, write tests/Pcre/PcreExtendedOptionReferenceTests.cs
with whole/all capture span comparisons and independent selected expectations.
Cover the design's lexical boundaries and resource policies. Commit tests and
these notes, open a draft PR and inspect expected missing-feature CI failures
before production edits. Record any native fixture corrections explicitly.

## Task 2: Parser implementation

Add PcreByteParser.Extended.cs for option/trivia helpers. Extend the shared
option reader and GroupOptions snapshot with x/xx state. Integrate skipping
with Parse/ParseAlternatives/AppendNode and ParseLiteralAssertion, preserving
quote state and direct-option repetition checks. Keep internal numeric/header/
name/property token readers unchanged. In byte/UTF brackets, skip xx SP/HT only
at item/range boundaries, preserving negation, first ] and trailing hyphens.
Update current inventories and obsolete rejection fixtures. Run whitespace and
frozen-source checks; commit only the bounded parser change.

## Task 3: Independent review and final verification

Request a read-only whole-branch review with a fresh subagent, including spec,
plan, exact base/head and boundary cases. Address concrete findings with tests.
Inspect every final Staging job, framework counts, frozen corpus and component/
API/package gates on the exact head. Record red/green evidence and review in
the PR; mark ready for user review/merge only after all six are green.
