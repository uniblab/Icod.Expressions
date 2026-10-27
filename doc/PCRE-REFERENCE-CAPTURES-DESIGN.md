# PCRE2 reference capture parity

**Status:** internal conformance tranche under issue #7; no consumer or production engine change.

## Intent

Extend the pinned `PCRE.NET` 1.6.0 test oracle from whole-match spans to numbered group spans on the original byte subject. Compare participation, byte start and byte length for group zero and every capture. An unset group is distinct from a participating empty group. Use the 8-bit span API's `Groups` collection, whose positions remain source-byte offsets; never decode and re-encode the subject.

## Approach

Keep a separate explicit case table in `tests/Pcre/PcreReferenceCaptureTests.cs` with independently stated expected spans. Each case first checks the native reference, then the internal match on the same bytes and options. Include alternation rollback, optional and empty captures, repetitions with earlier nested captures retained, backreferences, atomic choices, high byte/NUL, UTF multibyte scalars, malformed UTF fragment offsets, and nonzero search starts. The oracle is test-only, as in the existing whole-match matrix.

`tests/Pcre/PcreInvalidUtfCaptureReferenceTests.cs` adds a bounded comparison across all 256 one-byte subjects and selected multi-byte subjects at every start offset. Sixteen supported patterns exercise optional and unset groups, empty captures, alternation, repetition, backreferences, lookbehind, Unicode scalars and properties, and CRLF assertions. The comparison checks match success, every numbered group's participation, and source-byte start/length against the pinned reference with Grep-equivalent UTF/UCP/ASCII-`\d`/invalid-UTF and CRLF newline settings. It reports a total mismatch count and the first 25 discrepancies without requiring decoded subject strings.

## Boundaries

Only already-supported constructs are included. Named and multi-digit captures, capture histories, captures inside arbitrary assertions, diagnostics, broad PCRE2 syntax and Unicode version parity remain separate gates. A mismatch remains visible as a failed case and must be investigated before calling this tranche ready. Passing a finite matrix does not authorize Grep migration.
