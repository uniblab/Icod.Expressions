# Named capture result inventory addendum

## PR287: named captures on bound match results

Successful byte matches carry their exact compiled CaptureInfo snapshot.
GetCapture(name) selects the first participating native table slot;
empty captures remain present, absent/all-unset names return null, and metadata
distinguishes those states. Names are exact ordinal; null is an argument error.
Return original byte spans without copying/decoding subjects or allocating on
lookup. Preserve historical duplicate-reference resolution and all numbered
capture/mark/search/VM/work semantics. Bind at both success constructors using
one existing metadata reference, without result clones or metadata snapshots.

Independent pinned PCRE2 substring_get_byname preflight has 976 complete outcomes:
403 present (259 empty),31 unset,266 absent,276 no-match. All JIT0/positive size;
no case is discarded. Reuse all214 frozen PR286 inputs. 985 additions give106,379
tests. Corrected successful-build RED707 /105,672 passes /zero skips isolates
unused accessors after native/existing span checks; no native mismatch remains.
Freeze new data/assertion/consumer/C/XML sources before binding implementation.
The initial new adapter sorted native order and failed three branch-reset rows;
remove that sort, retaining all976 native literals. Canonical numeric metadata
remains sorted separately from owned native lookup order.

A fifth package record requires976 named protocols; preserve42+1+2, four
allocation, six fragment and214 metadata records and isolated feed/cache.
Record raw allocation totals for the added result field; fixed growth bounds
remain unchanged. No public API/dependency/version/workflow/release/migration
change. Only two Compile links are an old-test-file exception; prior C# tests/
A05 stay fixed. Final six-host106,379/frozen214/120, all target builds/five package
records and actual support/script gates require zero failures/skips/coded errors.

| Shared inventory | Increment recorded here |
|---|---|
| [Roadmap](PCRE-ROADMAP-STATUS.md) | P09 bound result/named access; broader release gates open |
| [Recent progress](PCRE-UTF-RECENT-PROGRESS.md) | Native976 outcomes/985 tests; getter checkpoint before binding |
| [Syntax](PCRE-SYNTAX-ACCEPTANCE-MATRIX.md) | Grammar/reference selection unchanged; named-substring semantics explicit |
| [Resources/errors](PCRE-RESOURCE-ERROR-CONTRACT.md) | Fixed metadata reference per result, no lookup allocation, matching limits/cancellation retained |
| [Grep fixtures](PCRE-GREP-CONSUMER-FIXTURES.md) | Five package records; actual consumer migration remains separate |

See [design](PCRE-NAMED-CAPTURE-RESULT-DESIGN.md),
[plan](PCRE-NAMED-CAPTURE-RESULT-PLAN.md) and
[PR287](https://github.com/uniblab/Icod.Expressions/pull/287). Review baseline
inventories/design/plan/addendum and whole branch before qualification. Large
syntax/resource/Grep inventories retain their merged-base blobs, continuing
PR286's reviewed small-addendum arrangement. Final identities/raw metrics/results/
completion/cleanup/ready remain external on PR287 without source/docs/test edits
after GREEN. #7/#102/footer fixed; broader P09, mandatory native16/P10-P18,
remaining P01/Stage C and actual migrations remain open. Grep last; user owns
merge/tags/publication.
