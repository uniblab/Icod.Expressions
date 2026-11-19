# Named captures on bound byte match results

Base merged PR286 f98b529e6a45e6c79a7fdbf2578a8c67b749b8e2;
105,394 prior tests. Extend P09's internal consumer contract from compiled capture
metadata to named access on successful results. No agents/Python/SDK install.
Keep #7/#102/footer fixed; Grep last; user owns merge/tags/publication. Broader
P09/native16/P10-P18/P01/Stage C and actual migrations remain open.

## Result contract and native selection

Each successful PcreByteMatch carries the exact immutable CaptureInfo snapshot
from its compiled pattern. GetCapture(name) returns the first participating slot
in that name's native table order, or null when the name is absent or
all its slots are unset. A participating empty capture is present, including at
EOF. CaptureInfo.GetGroupNumbers distinguishes unknown from known-unset names.
Null is an ArgumentNullException. Lookup is ordinal/case-sensitive/exact; NUL in
a .NET query is data, never a C-string truncation. Return original byte index/
length values, including captures outside group zero. Do not copy subject text.

Native named-substring selection follows native table order. Branch-reset duplicate
names can retain historical order2,1: the native substring API and references
prefer participating slot2. PR286's canonical GetGroupNumbers view is deliberately
sorted1,2 and must remain unchanged. Retain a separate owned lookup-order view
from the parser's validated registration list. Preserve every existing parser/VM/
reference decision and numbered capture/mark.

Use immutable native lookup-order numbers and an indexed loop over the
existing readonly captures. Bound lookup by256 slots; no lookup allocation,
normalization, scan of subject bytes or VM work charge. Results remain stable
across later matches/compilations and retain metadata independently of the pattern.

Bind the snapshot directly at both existing success constructors: the tolerant
empty fast path and ordinary Accept. Thread one metadata reference from the
compiled pattern through the matcher entry; no post-match record clone, new
snapshot or extra result object. The added reference may add one object field
per successful result; raw allocation totals must be re-recorded. Paired-input
growth bounds stay fixed. Errors/no-match/cancellation never return a success.

## Independent preflight and tests first

Reuse PR286's214 frozen native-first pattern/subject/option rows unchanged.
An original C probe calls pinned PCRE2 10.48 substring_get_byname, retaining its
return code and copied bytes, independently verifying selected native table slot/
ovector coordinates against that copy. PCRE2 source pcre2_substring.c157..178
confirms first participating native table entry, preserving empty participation.
PCRE2 pin7978954dbd2efc6f2196869290553cf1871b4ce6; SLJIT
de0259c7aaf36aa40cba8014f3fad3edde9307f9.

976 lookup rows:403 present (259 empty),31 known-unset,266 absent names and276
no-match controls. All compile with JIT0/positive size; no outcome is discarded.
Native output SHA256:
`33143a379fbf2c5598648f0ec857f6cc0782e95fd027d31d3518789b2ee049c2`.
Raw/UTF/UCP, aliases/historical order, recursion/DEFINE/assertions, no-auto/literal,
unused groups, binary/NUL, Unicode names/order,256 slots and128-byte names inherit
the frozen native catalog. Native C-string queries contain no embedded NUL; the
managed exact-query NUL control is explicitly separate.

985 new tests give106,379 total:976 native-first protocols, three unknown-query
rows and six facts for null, metadata/result lifetime, tolerant empty fast path,
zero-allocation warmed lookup, pre-cancellation and work exhaustion. Tests-only
production scaffolding adds unused CaptureInfo/GetCapture accessor stubs only.
Expect707 new failures /105,672 passes /zero skips.276 no-match rows and two
resource/cancellation controls pass alongside all105,394 prior tests. One null
query has an expected exception-type mismatch; the other failures reach missing
result accessors. Verify native and existing managed numbered spans before any
new getter. Reconcile identities, then freeze new fixtures/assertions/consumer/
C/XML before binding/lookup implementation. No old C# assertion changes.

## Package, inventory and qualification

Add a fifth package result requiring976 named-capture protocols. Keep existing
42+1+2, four allocation, six fragment-allocation and214 metadata records, fresh
isolated feed/cache and old helpers/data/probe. Typed consumer overwrites caller
pattern bytes, passes padded subjects and verifies named coordinates/bytes and
exact metadata identity. Only two new test-project source links are an old-test-
file XML exception. Existing public API/version/dependencies/workflows/release
and actual consumer migrations stay fixed.

Record this increment in a small status addendum covering the five shared
inventories, linked from roadmap/recent progress. Preserve the three large syntax/
resource/Grep inventories unchanged, continuing PR286's reviewed publication
arrangement. Review baseline inventories, this design/plan and addendum plus whole
branch before final qualification. Build library/consumer net8/net9/net10; execute
full net10 106,379/frozen214 cases/120 methods on six native OS/arch hosts, zero
failed/skipped/coded compiler warnings/errors, all five package records and actual
configured compatibility/Gnulib/support/script gates. A05 source35/corpus and all
prior C# test blobs stay fixed. Final identities/raw metrics/results/completion/
exact research cleanup/ready readback stay externally on PR287; no source/docs/
test edits after GREEN. No broader release gate closes from named result lookup.

## Native-order adapter correction before freeze

Initial source79020f66e8434f12cdc3e90dc2a20c61491d50c9, tree
9c381457e9c809464d1f362c13c399e8f4db709b, run44456089067 reported707 failures
/105,672 passes on Linux ARM64 job135331327184, zero coded compiler warnings/
errors. Three identities145/151/157 failed the new reference adapter before
managed lookup: it sorted native indices and selected0:1/value63 instead of
the independently pinned C API1:1/value64. All other failures reached new
accessor stubs. The expected native literals are unchanged.

Remove only the new adapter OrderBy so PCRE.NET preserves its actual native
name-table order. Its pinned PcrePatternInfo.GetGroupIndexesByName returns the
original InternalRegex.CaptureNames list. All976 C API outcomes remain frozen
data; no mismatching row is removed. Requalify a corrected tests-only source
and reconcile all707 failures at missing accessors before fixture freeze and
production implementation. Snapshot separate native lookup order without changing
the prior canonical numeric metadata API.

## Corrected RED, fixture freeze and author review

Corrected tests-only source1a940b68d8beb9ae378949e78a23e395b09ba7ea, tree
225ddf363640c3a89852cd113ac4601dd1a8b329; proposed merge
a829b81be8363d674b23b67b873a11255ab6bb96. Run44456930891 /Linux x64
job135334261663 builds all targets, zero coded compiler warnings/errors, and
reports707 failed /105,672 passed /zero skips /106,379 total. All707 unique
identities reach new accessors:706 GetCapture (one expected null exception-type
mismatch) and one CaptureInfo getter. No native-reference mismatch remains.
All105,394 old tests,276 no-match rows and two resource controls pass. This
corrected witness precedes fixture freeze and production implementation; four
additional native hosts independently confirm the same set before final review.

Freeze after the adapter correction, before implementation:

- samples/PackageSmoke/PcreNamedCaptureCases.cs: d466e22e4ff535a93780dadefe90e4c9d3a30d05.
- samples/PackageSmoke/PcreNamedCaptureConsumer.cs: afbd58dda98ea47de25d57ba9d4bdc9bd9a2cb31.
- tests/Pcre/PcreNamedCaptureResultTests.cs: c322edec7530de784ba9523a291f8b54a7065c0a.
- tests/Pcre/Native/named-capture-result-oracle.c: 913df56f7dfb8a754c86e1f6894112909462ec58.
- tests/Icod.Expressions.Tests.csproj: 8ce0588adc5c1ab8820218ad59be45f45f82eef6.

Completed producer blobs reviewed before qualification:

- src/Pcre/PcreByteMatcher.cs: 31f3a65c64d7d91cb52a669faba96189e39d7527.
- src/Pcre/PcreByteResult.cs: 23df64b4ad7e693a79df75722c4e39ffe36b0324.
- src/Pcre/PcreCaptureInfo.cs: ae277997093bdc45f134191e6bea96067bae5810.
- src/Pcre/PcreCompiledBytePattern.cs: e3cba8f07ba1fe5d1474d9bed665831c458a0c15.

CaptureInfo stores canonical/lookup read-only views per name. The original
registration copy is the lookup view; if already ascending, both views share
it. Only an out-of-order list gets a second sorted owned array. Prior numeric
metadata names/counts/views remain stable; parser registration/resolution code
is byte-identical. Sorting/copying retains cancellation and existing256 bounds.

Results require their exact compiled metadata in the record constructor. One
reference flows through the existing matcher entry to both success constructors;
all other matcher code is byte-identical. No extra match/result clone or metadata
snapshot. GetCapture indexes the immutable lookup/capture lists, preserving
empty participation and null/ordinal behavior without enumeration allocation.
No search/VM/Step/state/journal/capture/mark/limit or byte-navigation change.

Whole18-file author review covers independent native API bytes/coordinates, the
three ordering witnesses, untouched976 literals/old214 catalog, native adapter
correction and clean missing-accessor RED, binding at both exits, metadata and
result lifetime, ordinal/unknown/null/empty/unset behavior and warmed zero lookup
allocation. Reviewed fixed result-field cost and unchanged paired-growth gates,
fifth package JSON enforcement/typed original-byte consumers/old isolation and
checks. Baseline five inventories/design/plan/small addendum are reviewed; large
syntax/resource/Grep baselines remain untouched. No blocking introduced finding;
no independent reviewer/agents claimed.

All380 old C# test blobs among396 prior test files remain fixed; only two XML
Compile links are the stated exception. A05 source35/corpus and old package
helpers/data/probe remain fixed. Existing public API/root projects/dependencies/
version/workflow/release/migrations stay fixed. Local A05/corpus, classifier,
eight-job/thirteen-rejection prerequisite and whitespace checks pass. Final
source identities/native results/raw allocation metrics/completion/cleanup/ready
stay externally on PR287 without source/docs/test edits after GREEN.

All six corrected-checkpoint native hosts subsequently confirm the exact707
missing-accessor identities /105,672 passes /zero skips /106,379 total, all
library targets built and zero coded compiler warnings/errors: Linux x64
135334261663, Linux ARM64 135334261566, Windows x64 135334261387, Windows ARM64
135334261427, macOS ARM64 135334261323, macOS x64 135334260999. There are no
remaining native-reference mismatches; the fixture blobs above remain unchanged.
