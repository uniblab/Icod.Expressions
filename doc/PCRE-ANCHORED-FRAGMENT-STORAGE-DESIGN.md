# Bounded storage for anchored tolerant UTF fragments

Base merged PR284 `4f74d60f8d8d0431570852bb0ac58fd9688a3060`;
104,781 prior tests. This P09 allocation/resource increment preserves the
Revision 4 order: deliberate 8-bit freeze before P10 lowering; native16 remains
mandatory before stable release. No agents, Python or local SDK installation.
User owns merges/tags/publication; #7/#102/footer stay fixed; Grep last.

## Problem and bounded change

Tolerant UTF validation eagerly stores every valid fragment in a List even for
caller-anchored or proven dot-star-anchored searches. Alternating A/FF input has
512 fragments at1KiB and32,768 at64KiB. Its metadata allocation scales with the
whole subject before a first-fragment match or miss. No transcoding is involved.

Keep the entire validation loop and every Step, strict diagnostic, cancellation
and requested-offset check. For anchored searches retain only the first real
fragment whose end is at/after the requested offset. Earlier fragments have no
candidate; after this fragment, anchored retry terminates. Forward SKIP positions
come from consumption within that same fragment. Existing nullable synthetic
requested-byte candidates remain at the front, including continuation-byte
entries. A real fragment ending exactly at the requested byte must remain for
lookbehind/boundary context. The current insertion/gap logic stays unchanged.

Unanchored matching must retain its exact fragment collection and search order.
No streaming rescan, extra work charge, validation shortcut, subject copy,
transcode, parser/proof/VM instruction change, new quota or public API is needed.
Only fragment retention in PcreByteMatcher.cs changes after successful-build RED.
The constant-storage claim applies to anchored fragment metadata, not captures,
backtracking, unanchored fragments or all managed/native memory.

## Independent native contract and RED

Original C preflight builds pinned PCRE2 10.48
7978954dbd2efc6f2196869290553cf1871b4ce6 / SLJIT
de0259c7aaf36aa40cba8014f3fad3edde9307f9. It records1,568 short protocols:
14 patterns x13 subjects xboth caller-anchor settings xevery caller offset.
Cover valid/invalid edges, multibyte/supplementary splits, empty/context assertions,
G, CRLF, captures/references, SKIP, dot-star ownership and optimizer opt-outs.
All literal outcomes survive, including misses; no native mismatches are filtered.
Twelve additional large-subject protocols establish six allocation profiles at
1,024/65,536 bytes. Every compile has JIT0/positive size. Output SHA256:
`307808a233ae5d02f3e14afe7f77a02d6d5043cb933eb8a7a00aa192beefe02f`.

390 new tests:364 grouped native-first protocols,18 allocation checks (six
profiles xplain/observed/cancellable paths), and eight validation budget/
cancellation/strict-error controls. Expected total105,171. Tests-only source
keeps production unchanged: expect18 allocation-growth failures /105,153 passes /
zero skips, with all native outcomes and old tests passing. Reconcile actual
identities/causes before freezing all new data/assertion/consumer/C/XML sources.
Do not weaken a growth threshold or native expected outcome after the freeze.

## Measurement and package integration

Reuse the frozen PR284 callback-only counter unchanged. Prepare/compile outside
measurement, pass original padded visible views, warm each size eight times and
alternate eight measured visits per size. Match/capture validation overhead is
included. Require large-small <=4,096 bytes per call. Native semantic outcomes
precede each managed measurement. Plain, observed and cancellable paths cover
allocation independently. Whole-subject validation must still exhaust a1,023
budget or cancel at the1,024th scan step even for an immediate anchored match;
strict UTF must still report a late invalid byte before matching.

Add one separate package result for six fragment profiles. Preserve existing
42+1+2 and four raw/UTF/UCP allocation records, their helpers/data/probe and
fresh candidate feed/cache. Shared action enforces exact names, sizes,
iterations, nonnegative raw counters and bounds. Keep all workflow call sites,
release behavior, dependencies, package identity and public API fixed.
Only two new source links are an old-test-project XML exception. All prior C#
test blobs and frozen A05 source35/corpus stay byte-identical.

## Review and qualification

Review whole branch and this design/plan plus five shared inventories before
final source qualification. Build library and consumer net8/net9/net10; execute
net10 full105,171/frozen214 cases/120 methods on six native OS/arch hosts with
zero failures/skips/coded compiler warnings/errors. Require all three package
records plus actual configured compatibility/Gnulib/support/script gates.
Record source/tree/base/proposed merge identities, raw per-host allocations,
plan completion, cleanup and ready readback externally on PR285. No source/docs/
test edits after GREEN. Remove only exact disposable pcre285-research directory.
Wider P09 internal API/deviation/hostile/physical timing gates, native16/P10-P18,
remaining P01, Stage C and real downstream migration remain open.

## Observed RED, freeze and completed author review

Tests-only source f3773d3d42519a37ec52d1d75a79de2cb716906f, tree
16093c9878aa965fa4a39818b33d15670a02d3c4; proposed merge
3a09cb78a1041f36b02303c95cbec43d5eee5af4. Run44424946992: all six native
hosts build net8/net9/net10, zero coded compiler warnings/errors, and report
exactly18 allocation failures /105,153 passes /zero skips /105,171 total.
Failure identities agree: six profiles xplain/observed/cancellable. All native
assertions,364 protocol groups/eight resource controls and104,781 old tests pass.
Plain caller-match raw totals across eight calls are136,768/8,395,456 bytes
for1KiB/64KiB. Other profiles expose the same whole-fragment-list growth.

Freeze before production modification:

- samples/PackageSmoke/PcreFragmentAllocationCases.cs: 569056aad8145dffe30a3f0ac12942c820a1c680.
- samples/PackageSmoke/PcreFragmentAllocationConsumer.cs: 5fe969a5fe1baf417dacf326d23090e52fc7bc76.
- tests/Icod.Expressions.Tests.csproj: a455db94a98ac6e416c528c88a2476412c1d505f.
- tests/Pcre/Native/anchored-fragment-oracle.c: 11d54c4b3fbb146b0078efa2582f5cd2d8cf583b.
- tests/Pcre/PcreAnchoredFragmentCases.cs: f6a14dc64095bc18370409087f37b07dacc2d149.
- tests/Pcre/PcreAnchoredFragmentStorageTests.cs: 93932b18c45311c7a1d65b6cdfd52f7cb6d88c77.

The completed matcher blob is ebf280ee662d311646dbe3493ba5b44cbdf396fc. Only three
UTF fragment append calls and a private retention guard change. The guard
accepts every unanchored fragment identically, or the first end>=startOffset
when anchored. It includes an end exactly at the requested byte. All validation
Step calls, strict diagnostics, context scan/synthetic insertion/gap order,
forced-SKIP handling and VM code remain byte-identical. Raw mode still adds
its single original fragment. No rescan, buffer copy or new work/limit state.

Whole18-file author review covers native provenance, all retained statuses/
captures, original padded byte views and every caller offset; anchored selection
and context/forward-SKIP proof; exact work-budget/cancellation/strict checks;
callback-only accounting and six profiles on three paths; package JSON names/
sizes/iterations/counters/bounds and old protocol/isolation enforcement.
Prior389 test files/377 C# blobs remain fixed except the documented two XML
links. Existing package helpers/data/counter and A05 source35/corpus are fixed.
No public API, dependency, package/version, workflow call-site or release change.
All seven inventories precede final source qualification. No introduced blocking
finding; no independent reviewer or agents are claimed. Local A05/corpus, CI
classifier, eight-job/thirteen-rejection prerequisite and whitespace checks pass.
Final source identities, raw metrics, cleanup/completion/ready stay on PR285
without source/docs/test edits after GREEN. Broader release gates remain open.
