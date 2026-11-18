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
