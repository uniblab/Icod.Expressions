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
