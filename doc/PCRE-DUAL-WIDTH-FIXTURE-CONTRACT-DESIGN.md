# Native dual-width fixture and coordinate contract

Continue merged PR279 at `40184bb937bca12a0fa9587efe57d1c55eb7fbe9`, tree
`27d71bad1331733c533300d2f965703f624d405a`; baseline 103,848 tests.
This is P00/P01 test infrastructure following the Revision 4 stocktake,
not P09/P10/P11/P12 closure and not a production 16-bit implementation.
No agents, Python or local SDK installation. User owns merges/tags/publication;
keep #7/#102 open and unchanged, Grep last and all prior tests/A05 fixed.

## Oracle and representation scope

Use unmodified native PCRE2 10.48 `7978954dbd2efc6f2196869290553cf1871b4ce6`
and SLJIT `de0259c7aaf36aa40cba8014f3fad3edde9307f9`, building both native
widths with JIT. Existing PCRE.NET 1.6.0 remains the six-host CI dependency.
Inspected wrapper source `09639d45d4c03a8269fa2cde6edc7a7383bbc18c` confirms
PcreRegex is truly native 16-bit but always adds UTF. Do not present that API
as a raw 16-bit oracle. The standalone [native harness](../tests/Pcre/Native/README.md)
records both raw and UTF modes with explicit native code-unit hex values.

The first shared contract compares valid Unicode source and subject sequences
under UTF at both widths. Each immutable row carries separate pattern/subject
8-bit and 16-bit encodings, both native start offsets, both capture protocols,
normalized scalar captures, both mark encodings and a canonical named-slot map.
Strict fixture encoding equality is asserted before matching. The existing
managed byte engine must retain the independently pinned byte results; there
is no managed 16-bit matcher yet. Name metadata is checked on both native
oracles without inventing a public managed name-table API.

Keep no-match, unset and participating empty captures distinct. Record all
numbered slots, including trailing unset groups. Normalize start/end endpoints
separately, preserving negative reported lengths from allowed assertion K.
Captures outside group zero remain valid when within the original subject.
Successful marks normalize to text but retain their original native hex too.
Failed/partial marks, callouts and partial results are not claimed supported.

Compile errors retain native error codes and both source offsets; scalar
conversion applies only at legal source boundaries. The existing managed
compiler's independent diagnostic convention is not relabeled as PCRE2's.
Malformed subject inputs have separate width/error rows. Never normalize
malformed UTF as if the two invalid encodings represented the same scalars.

## Independent test-only coordinate adapter

A helper under tests/Pcre, not src, creates byte-boundary and UTF-16-boundary
maps with System.Text.Rune decoding. It never calls PcreUtf8, matcher navigation,
Unicode classification or the production VM. Reject invalid encodings,
different scalar sequences, out-of-range endpoints, split UTF-8 scalars,
split surrogate pairs, malformed capture syntax and arithmetic overflow.
No snapping, replacement decoding or zero-width/unset conflation is allowed.
Arrays are test-only fixture infrastructure, not production subject storage.

Expected normalized values are independently obtained from native captures
and a JavaScript scalar-boundary model during generation. Thus agreement of
two native offsets alone cannot hide the same defect in both normalization
paths. Include code-unit/scalar transition values through U+10FFFF and explicit
negative/range/error controls, with original byte results pinned separately.

## Native-first scope and delivery

Forty-four shared patterns produce 140 matching cases / 280 native executions.
Ten compile errors add 20 native executions. Eight malformed subjects add
width-specific native execution errors; two raw 16-bit witnesses prove the
wrapper/raw distinction. All successful compiles require JIT return zero and
positive sizes. Covers captures/names/duplicate slots, branch reset, ordered
alternatives/rollback, lookarounds, recursion/conditions/DEFINE, marks/cuts,
Unicode/UCP/graphemes, CRLF/NUL, offsets and reversed K ranges.

The 150 shared/diagnostic rows, 44 coordinate cases and eight malformed-subject
cases add 202 tests: 104,050 total. Publish an intentionally unimplemented
coordinate adapter in a tests-only draft: expect 194 coordinate-dependent
failures / 103,856 passes / zero skips. All native assertions, all 103,848 prior
tests and eight width-specific controls must pass first. Reconcile failure
identities, then freeze all five fixture/assertion C# files before implementing
only the test-support adapter. Freeze the finished helper separately. No
production code changes occur at any stage; this RED is the missing fixture
adapter contract, not a claim that a managed 16-bit engine was implemented.

Preserve 367 prior test files / 362 C# sources, A05 source 35 and frozen corpus.
Review the complete branch and update this design/plan plus all five shared
inventories before final qualification. Build net8/net9/net10, execute full
net10 and frozen 214 cases / 120 methods on six native hosts, zero failed/skipped
tests and coded warnings/errors, all configured support/package/script gates.
Final evidence and plan completion stay on PR280 without source/docs/test
edits after final GREEN. Raw 16-bit CI, Perl/reducer, broader matrices and P01
closure remain open, as do migration/normalization and the other release gates.

Before freezing, add two explicit replacement-decoding counterexamples:
invalid UTF-8 80 versus valid U+FFFD, and valid U+FFFD versus an isolated
UTF-16 high surrogate. Both must be rejected even though a replacement decoder
could make them appear equal. This strengthens the coordinate contract without
changing any native fixture or prior test expectation. The refined totals are
202 new / 104,050 total, expecting 194 adapter failures and 103,856 passes.

The same 150 native rows are exposed through a separate PcreDualWidthCases
MemberData catalog. This preserves all expected values and case identities
while allowing future managed width-16 tests to consume the exact same data
without copying or editing the current assertions. Freeze this catalog too.

## Observed RED, assertion freeze and author review

Tests-only head `9b2966e13164411d7df178fb89e4ac7653eab5ce`, tree
`dd7b01d6e26d47c5e9b961db78f15116a51f23c6`, proposed merge
`215fdf468ba9392d91863dabbc0e7f2e69002108`. Run 44338870876 / Linux x64
job 134923362072 builds all targets with zero coded warnings/errors, then
reports exactly 194 failures / 103,856 passes / 104,050 total / zero skips.
All 194 detailed failure blocks reach the intentionally unimplemented
coordinate constructor: 140 shared matches, ten native compile diagnostics,
ten scalar boundaries, 17 invalid/different inputs, 16 endpoint/protocol
errors and one empty/unset/reversed control. There are no unrelated failures.
Both native oracles, the current managed byte results, every prior test and
all eight width-specific malformed-subject controls pass before normalization.

Freeze before implementing the test helper:

- `tests/Pcre/PcreDualWidthCases.cs`: `9fab1e91ffa35e712f9fc8277b049fdd1eaa0675`
- `tests/Pcre/PcreDualWidthProtocols.cs`: `ca1ec364bf573b880cd1542232dd5721f30240c7`
- `tests/Pcre/PcreDualWidthReferenceTests.cs`: `f292221d77ee11b1a985295b352b9f8c39c67165`
- `tests/Pcre/PcreDualWidthCoordinateTests.cs`: `09b030a30d63df1caa9106b980a146ea182a7732`
- `tests/Pcre/PcreDualWidthMalformedSubjectTests.cs`: `6ffdec4bf7c08b74c236afd9ffae3d02b9fc8e6c`

The finished helper blob is `cb4ae2d3b219ceb91aff69b1283625bf9008f0e9`. No fixture/assertion
file changes after its freeze; no production source changes at all.

Independent native preflight has 310 executions including raw witnesses;
combined output SHA256 `e5df76d9cdfb0ccc7f4405d53091006f0ee46ef32d5fd6154784202d26d75f95`.
Native harness blob `3c393ebf047a1d8a76b0c075df97c9ce958df9b1` is retained.
A separate catalog comparison confirms all 150 row values survived the
InlineData-to-MemberData extraction unchanged.

Whole-branch author review covers native code-unit/endian conventions, explicit
UTF scope, wrapper raw-mode limitation, source/subject identity, paired strict
Rune decoding, terminal/interior boundaries, signed endpoints, overflow and
format failures, capture/name/mark/error preservation and future catalog reuse.
The helper holds only private boundary arrays and no caller input; decoding is
independent of matcher navigation and classification. It stays outside src and
the production package. Entire src tree, API, project/dependency/workflow/version
files and all 367 prior test files / 362 C# sources remain byte-for-byte fixed.
No unresolved introduced finding; independent agent review was not performed.

Local A05/corpus/candidate, CI classifier, eight main jobs/thirteen rejection
controls and whitespace checks pass. All seven inventories precede final
qualification. Final exact identities, six-host results, cleanup and plan
completion remain external on PR280; no source/docs/test edits after GREEN.
