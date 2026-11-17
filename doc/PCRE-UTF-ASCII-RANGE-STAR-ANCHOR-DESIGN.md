# UTF ASCII plus range-union star anchoring

Continue merged273 at4112df2d7463776fc92e74d85695723df31bc069,
tree493892e78861a451a07d20a92ac51cb926543ee1; floor100,961 tests.
Execute inline, no subagents or local .NET SDK installation. Development
branch/PR updates are authorized; user owns merge/tags/package publication.
Preserve337 prior test files/332 C# sources/frozen35. Keep #7/#102 open,
Grep last and issue7's existing publication-blocked footer unchanged.

## Native evidence

PCRE2 10.48 pin7978954dbd2efc6f2196869290553cf1871b4ce6 and SLJIT
de0259c7aaf36aa40cba8014f3fad3edde9307f9, independently built with JIT,
reproduce all530 merged273 groups/29,225 protocols, including signed reset
spans. PCRE.NET1.6.0 remains the CI oracle. All matching JIT_COMPLETE calls
return0 and have positive JIT sizes.

Positive ordinary UTF classes combining a positive ASCII property with
literal ranges covering128..0x10FFFF lower to native ALLANY. Both
`[\p{ASCII}\x80-\x{10FFFF}]*?B` and reversed/split/overlapping forms miss
on tolerant UTF C3A942 fromoffset1; NoDotStarAnchor instead matches2:1.
Case-sensitive ASCII aliases, extra union predicates and literal endpoint
bridges retain this source provenance. Negative outer/negative-only ASCII,
holes at128/max and the surrogate gap do not establish this proof.

Caseless canonical closure and retained ranges below128 are deliberately
outside the new proof. Their valid aligned protocols/work fallback are pinned;
do not claim raw split-offset parity for those broader source cohorts.

## Bounded implementation

Modify only PcreByteProgram.cs. Preserve existing direct Any, exact full
range, full-range-member, literal-union and predicate paths and charges.
Add one independent compile-local MaximumInstructions=4096 counter shared
across leading paths. Its combined cost counts predicates and every complete
range pass. This is not a public admission or runtime-work limit.

After the old literal-union proof, attempt the additional proof only when
that proof did not succeed, the outer ordinary class is positive, and both
range/predicate lists are nonempty. Before scanning, precheck the complete
predicate list plus first range pass using subtraction to avoid overflow.
An oversized first combined list consumes no partial budget. Visit every
admitted predicate with cancellation and one charge; identify only exact
positive Ascii/Category:null/PropertyValue:0. If absent, keep old fallback.

Start coveredEnd=127. Precheck each whole range pass, then visit every member
with cancellation and one charge, extending when Start<=coveredEnd+1.
Reject any retained range starting below128 after completing that pass.
Never accept before the complete pass reaches0x10FFFF. Stop after a pass
makes no progress or another whole pass will not fit. No allocation/sorting,
node mutation or runtime state is needed. Low ranges cannot rescue an
exhausted old literal-union proof. Preserve old predicate charges even when
the additional proof independently succeeds; exhausted old predicate work
may still use its independent result.

Keep all entry/global-AST/capture31/32/cuts/calls/targets/NoDot/COMMIT guards,
source ownership, casing, validation/cancellation priorities and exact work
limits. No parser/AST/VM/API/dependency/workflow/public-limit changes.

## Native-first corpus and RED prediction

186 matching groups/11,090 literal protocols,15 diagnostics and72
supplementary cases add273 tests, reaching101,234. Dynamic native preflight
checks46 groups/138 literal outcomes. Cover combined4096/4097, reversed
63-range passes with127/128 predicates, shared alternatives, independence
from all three old budgets, preservation of every established path after
new-budget exhaustion, whole-list scans after early coverage/ASCII, no
partial charge for oversized lists and no-progress termination. Include
work/cancellation, owned sliced sources, marks/captures, instruction4090/4091,
capture/depth256 and global-AST fallback.

Predict68 tests-only failures:44 matching identities (including COMMIT145/146
whose entry policy cannot be predicted just by disabling native anchoring)
and24 supplementary identities. All100,961 old tests and205 new controls
must pass. Reconcile exact observed identities and freeze all three new C#
Git blobs before production. Native assertions precede managed comparison;
no malformed-whole-subject or split-offset strict rows are matching claims.

## Observed RED and pre-production freeze

Tests-only headc316f33cf3b8a5cd46bad7f88d45f1ec463b9873,
tree763de422e116046463833141dd0208488e649a0a; proposed merge
92be7d20df9bc8cb2db2ca4fe0bd4344f0dbe931 has ordered base/source parents
and the identical content tree. Run44155913703/Windows x64 job134296472987
confirms exactly68 failures/101,166 passes/101,234 total, zero skips and
zero compiler/analyzer warnings/errors. All44 matching and24 supplementary
identities match the prediction; every failure is in the new reference class
and none has an AssertNative stack frame. All100,961 prior tests and205 new
controls pass. Strict research exclusions:476 malformed-whole-subject and
374 split-offset rows, excluded before native matching.

Matching IDs:3/5/7/11/13/15/19/21/23/27/29/31/35/37/39/43/45/47/51/53/55/
59/61/63/67/69/71/75/77/79/129/133/139/141/143/145/146/155/163/165/167/
169/171/180. Supplementary:8 work,2 ownership,1 combined admission,
1 repeated-pass admission,1 shared admission,3 old-budget independence,
6 established-path preservation,1 oversized-list and1 no-progress boundary.

Freeze these Git blobs before production:

- Protocols1 d6921082685c492a72076a1f157f161abe4bcc1b
- Protocols2 449b69df7e9d0402b095602263bc6030fb86858b
- ReferenceTests 02abe1a086c12000dc62e49d3dc4964d3a80b397

All337 prior test files/332 C# sources and frozen35 are unchanged. No test
edits follow this checkpoint. Additional deferred OffsetLimit research is
preserved in [its own note](PCRE-OFFSET-LIMIT-RESEARCH.md); no API is added.

## Delivery gate

Whole-branch author review, old-test/frozen identity checks, local support
gates and all seven inventories must precede final qualification. Build all
three library targets; execute full101,234 net10 tests plus frozen214/120
on six native OS/architecture hosts. Require zero failures/skips/compiler
warnings/errors and every configured support/package gate. Record exact
source/base/tree/proposed merge and logs externally on PR274; no source/doc
edits after final GREEN. Ready readback and identity-checked scratch cleanup
complete delivery. Broader optimizer parity and Grep/release remain separate.
