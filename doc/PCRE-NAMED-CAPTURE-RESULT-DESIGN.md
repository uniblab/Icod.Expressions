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
