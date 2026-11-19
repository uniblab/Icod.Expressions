# Immutable internal capture metadata

Base merged PR285 a96bd8b0631b35888b14c005b405a49bcddf6dad;
105,171 prior tests. Advance the P09 internal consumer contract. A compiled
pattern currently exposes matching but no capture-count/name mapping. Keep PCRE
internal, one grammar/VM, original offsets and all match semantics. No agents,
Python or SDK installation. User owns merge/tags/publication; #7/#102/footer fixed;
Grep last. Broader P09, native16/P10-P18, P01/Stage C and real migrations stay open.

## Contract

Expose one owned PcreCaptureInfo snapshot through compiled-pattern CaptureInfo.
CaptureCount excludes group zero and includes unused/DEFINE/trailing capture
slots. Names are unique and ordered by StringComparer.Ordinal (UTF-16 ordinal),
not native UTF-8 name-table byte order. GetGroupNumbers performs ordinal,
case-sensitive exact lookup and returns sorted distinct positive slot numbers.
Duplicate names can identify several slots; different permitted aliases can
identify the same slot. Do not change the parser's historical declaration order
used by duplicate-name references/conditions. Metadata is identification, not
a new rule for selecting a participating named capture.

Unknown names return an empty list; null is an ArgumentNullException. Names and
number lists cannot be mutated through exposed collection interfaces or arrays.
Return stable metadata/lookup views without per-read allocation. Keep metadata
available before any match and after misses/resource diagnostics; other compiled
patterns cannot change it. Pattern bytes are already owned at compile entry;
metadata retains strings/number snapshots, not caller buffers, source or AST.
The width-neutral capture information can later be reused by native16 lowering.

Copy the parser's existing validated name map only after parsing/program admission
succeed. Sort copied names/numbers, leaving grammar/resolution collections fixed.
Cancellation is checked throughout snapshot copying and around bounded sorts.
Existing capture slots cap at256, each name at128 bytes, unique names at most256
and each distinct-number list at most256. No new quota, admission restriction,
public API, matcher/VM/work-limit or per-match allocation change is needed.

## Native-first fixtures and tests-only checkpoint

Original C preflight uses PCRE2 10.48
7978954dbd2efc6f2196869290553cf1871b4ce6 / SLJIT
de0259c7aaf36aa40cba8014f3fad3edde9307f9. All214 cases compile with JIT0/
positive size:133 matches/81 misses. Record native capture count, every raw native
name-table entry, full numbered spans and JIT facts. Canonical fixture maps are
independently normalized in JavaScript by UTF-16 ordinal names and distinct
ascending numbers. No case or native mismatch is discarded. Output SHA256:
`eeefb6b89d903ead1067b14670c09e209cedc3687152756461586f44b24ffdf3`.

Cover raw/UTF/UCP, caller/source duplicate and no-auto-capture, literal text,
all three declaration spellings, case-sensitive/Unicode names, ordinal-vs-byte
ordering, branch-reset aliases/declaration order, DEFINE/recursion/assertion
captures, optional/erased/unset groups, binary/NUL,256 slots and128-byte names.
PCRE.NET1.6.0 provides the six-host native reference; its pinned source confirms
PatternInfo.CaptureCount excludes zero and the available name lookup APIs.

223 new tests give105,394 total:214 native-first consumer cases, three missing-name
lookups and six facts for null/immutability/identity/resource/no-allocation/
pre-cancellation. Existing managed match spans are checked before metadata access,
so RED cannot hide a previously mismatching match behind a missing getter.
The tests-only source has only new unused metadata type/accessor stubs in
production; old compile/match behavior stays unchanged. Expect222 failures at
NotImplemented CaptureInfo /105,172 passes /zero skips, including all105,171 old
tests plus pre-cancel control. Reconcile identities before freezing all new case/
assertion/consumer/C/XML sources. Replace only scaffolding and wire owned metadata
from parser through compiler after RED; keep every old C# test unchanged.

## Package, review and qualification

The package-only consumer shares frozen214 native fixtures and typed API use,
overwriting caller source after compilation and retaining padded subject views.
Add a fourth structured record requiring214 successful metadata protocols. Keep
existing42+1+2, four allocation and six fragment-allocation records, isolated
fresh feed/cache, all workflow call sites, version/dependencies/release fixed.
Only two test-project source links are an old-test-file XML exception.

Review whole branch and this design/plan plus five shared inventories before
final source qualification. Build library/consumer net8/net9/net10; execute net10
full105,394/frozen214 cases/120 methods on six native OS/arch hosts, zero failed/
skipped/coded compiler warnings/errors, all four package records and every actual
configured compatibility/Gnulib/support/script gate. A05 source35/corpus and all
prior C# test blobs remain fixed. Final source/tree/base/merge identities, results,
plan completion, exact research cleanup and ready readback stay externally on
PR286 without source/docs/test edits after GREEN. No broader release gate closes
merely from capture metadata.
