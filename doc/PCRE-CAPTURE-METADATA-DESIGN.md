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

## Observed RED, freeze and whole-branch review

Tests-only source16797ad96ae8e43bbb0eac1fb399e6187c399644, tree
87bf5016c76bdde9c1a345e4a2fa09461491688a; proposed merge
78dacef28ac633c8127ef730a47318891c4a1946. Run44448790030: all six native
hosts build net8/net9/net10 with zero coded compiler warnings/errors and report
exactly222 failed /105,172 passed /zero skips /105,394 total. The same222
unique identities reach PcreCompiledBytePattern.get_CaptureInfo NotImplemented.
Every native count/name/match assertion and existing managed span check passes
before that getter; all105,171 prior tests and pre-cancellation control pass.

Freeze before replacing scaffolding:

- samples/PackageSmoke/PcreCaptureInfoCases.cs: 1bc11ad0fe986311321d7c2c0403d29b122f69bd.
- samples/PackageSmoke/PcreCaptureInfoConsumer.cs: 0290e7a4abadf02a6aec3f2caab610b91f0a9bfe.
- tests/Icod.Expressions.Tests.csproj: b4098218cec188905fe1b041e2956fab97ecc67e.
- tests/Pcre/Native/capture-info-oracle.c: 40621050437913f319dc07aa395bf9e54b0cf5f8.
- tests/Pcre/PcreCaptureInfoTests.cs: b5e2625898063f2b5ddcda8a6eb1afa7304aba94.

Completed producer blobs reviewed before qualification:

- src/Pcre/PcreCaptureInfo.cs: 24c5b6380467025781fbaa9d9eda45a0fe0381a5.
- src/Pcre/PcreCompiledBytePattern.cs: cbde63cb6e0a6a9ae885e44e7f1e3019c72b5b60.
- src/Pcre/PcreByteCompiler.cs: 89605e2197414797d9130ad483dd310302d25eb4.
- src/Pcre/PcreByteParser.NamedCaptures.cs: fefb68039acb80bf2e1b88fa0cd3eb976eaa1b05.

The parser only exposes a snapshot factory; RegisterCaptureName, resolution and
all AST/program/matcher bodies stay fixed. Compiler wiring occurs after program
admission. The snapshot copies numbers, reuses owned immutable name strings,
sorts only new arrays, uses an ordinal private dictionary and exposes read-only
wrappers. Unknown-empty/null-error behavior and stable allocation-free views
follow the frozen tests. Captures cap256; a first distinct name cannot reuse an
already-named number, so at most256 unique names and256 numbers per name bound
copying/sorting. Cancellation checks surround bounded sorts and each copy loop.
No borrowed source/AST retention, per-match metadata allocation or quota change.

Whole18-file author review covers native C/source provenance, all214 unfiltered
results and independent canonical ordering, no-match/unused-slot metadata,
Unicode/branch-reset aliases/capacities, ownership and mutation boundaries,
lookup identity/casing/null behavior and after-diagnostic reads. Reviewed package
source-linked typed API assertions, padded views/overwritten source, fourth JSON
record and unchanged old protocol/allocation/isolation enforcement. No introduced
blocking findings; no independent reviewer/agents claimed.

All379 prior C# test blobs remain byte-identical among393 prior test files; only
two XML Compile links are the documented exception. A05 source35/corpus, matcher
(including PR285 retention), old package helpers/data/probe, public API, root
projects/dependencies/version and workflow/release call sites stay fixed. Local
A05/corpus, classifier, eight-job/thirteen-rejection prerequisite and whitespace
checks pass. This design/plan and all five shared inventories precede final
source qualification. Final exact identities/results/completion/cleanup/ready
stay externally on PR286 with no source/docs/test edits after GREEN.

## Inventory publication adjustment before qualification

Automatic review rejected the large resource inventory replacement despite
verification of its exact public baseline plus the2,437-byte authored update.
The rejected file replacement is omitted. Large syntax/resource/Grep inventories
remain byte-identical to base; no alternative endpoint publishes their payloads.
The [small status addendum](PCRE-CAPTURE-METADATA-STATUS.md) records this increment
for all five shared inventories, linked from roadmap/recent progress. Reviewed
all five baseline inventories, design/plan and addendum before qualification.
This is a documentation publication adjustment; implementation/frozen fixtures
are unchanged. Final branch has18 files. No approval-dependent action remains.
