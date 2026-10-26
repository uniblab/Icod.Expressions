# PCRE2 reference conformance fixtures

**Status:** internal test tranche under issue #7; no consumer or production engine change.

## Intent

Compare selected already-supported internal matcher behavior with the same `PCRE.NET` 1.6.0 `PcreRegex8Bit` reference that Grep uses. Keep the reference dependency in `/tests` only; `Icod.Expressions.csproj`, its packed single-library artifact, and all production code remain independent of PCRE.NET. Pin the package version and operate on original byte arrays, not decoded/re-encoded subjects. Match Grep's UTF/UCP/ASCII-`\d`/invalid-UTF compile options for the corresponding cases.

## Scope and approach

Use a table of stable supported syntax with byte subjects and optional start offsets. Compare reference and internal success/no-match plus whole-match source-byte start/length. Include byte and UTF literals, braced hex, counted repetition, bracket ranges, caseless Unicode examples, fixed assertions and invalid-UTF fragment barriers. Keep malformed compile diagnostics, capture-specific parity and exhaustive Unicode tables outside this first matrix: the reference wrapper's diagnostic categories and group representation differ from the internal result. A mismatch must fail with the case name and both results; do not silently skip or normalize discrepancies.

The oracle runs only in tests. The CI's existing net8/net9/net10 and six OS/architecture matrix, frozen GNU corpus and package-only smoke provide integration evidence. This fixture set is deliberately finite: passing does not establish PCRE2 10.48 compatibility or authorize Grep migration. Continue expanding it with focused cases, pinned invalid-UTF offsets, captures and performance gates.

## Options and risks

`PcreRegex8Bit` must receive `PcreOptions.Utf | Ucp | MatchInvalidUtf` plus `PcreExtraCompileOptions.AsciiBsD` for Grep-like UTF cases, and `Caseless` where appropriate. Byte cases omit UTF/UCP. Test-only native reference assets must restore and run on all six matrix RIDs; the library's packed dependency closure must remain unchanged. If a discrepancy appears, first distinguish actual behavior from a different compile option or reference version, then correct the implementation or document a gate with a failing test kept in a draft PR.
