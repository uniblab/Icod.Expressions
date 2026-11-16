# UTF ASCII-complement classes in dot-star anchoring

Continue merged261 at03ae93196c438d01dd15c6b32eba81d85e2c8831,
treea2009b7f41df7b77e20a02cd853b182e050c61af;96,283 baseline tests.
Execute inline under standing user direction; no subagents/approval pauses.
User owns merge/tag/publication; #7/#102 remain open; Grep stays last.

## Native cause and selected scope

The deferred PR261 profile102 uses `(?:[\s\S]*A|[\d\D]*B)` in tolerant
UTF. Native PCRE2 10.48 lowers those non-UCP classes to OP_ALLANY; the existing
managed star proof recognizes Any/byte classes but excludes UTF scalar sets.
On C3A942 at offset1 native misses while managed retries and returns2:1.

Research distinguishes source provenance from semantic universality. Native
anchors `[\s\S]*B` without UCP, but not with ordinary UCP predicates;
`[\p{Nd}\P{Nd}]*B` is also unanchored. Explicit ASCII restrictions restore
eligible shorthand/POSIX predicates. compile_class builds a full byte bitmap
and includes all higher characters for complementary ASCII specials, yielding
OP_ALLANY. Native Unicode-property lists can retain OP_XCLASS even when their
semantic union is universal.

Selected implementation: extend only the existing star predicate in
PcreByteProgram.cs, recognizing a UTF PcreByteScalarSet with ordinary Class
source, no outer negation/ranges and exactly two opposite predicates of the
same AsciiDigit/AsciiWord/AsciiSpace kind, null category and zero property value.
Those kinds come from ASCII shorthand/POSIX parsing, not Unicode property
aliases. This constant-time two-predicate proof closes the recorded profile
without adding source metadata or changing the class matching instruction.

Normalizing such nodes to Any would alter existing matching/work provenance.
Testing arbitrary semantic full sets would misclassify native property classes.
Parser-wide native class provenance would cover more cases but needs a broader
design. Additional members, full ranges, h/v pairs, Any properties, mixed
Unicode/ASCII predicates and class algebra remain research, even where native
also lowers them to ALLANY. Preserve byte-mode and existing proof availability.

## Boundaries and validation

- One production file only; parser/AST/emitter/matcher/VM/diagnostics/API/
  dependencies/workflows fixed. No new opcode or quota/admission change.
- Keep bounded group/alternative/capture-reference/32+ bitmap proof, lexical
  cuts/targets, caller/source NoDotStarAnchor and disabled-anchor COMMIT entry.
  NoStartOptimize/NoAutoPossess remain independent; ordinary-dot STARTLINE and
  NO_JIT interpreter policy stay separately scoped.
- Native-first protocols include the original91 research rows, byte controls,
  ASCII restrictions/UCP/property counterexamples, ordering/negation/duplicates,
  groups/alternatives/references/cuts, signed captures/marks, malformed/split
  offsets and owned views/work/cancellation/validation/full capacities.
- Freeze new C# after exact tests-only RED. All300 prior files/295 C# sources
  and frozen35 stay byte-identical. C#13 builds net8/net9/net10; execution only
  net10 on six native hosts; preserve214 frozen outcomes/120 methods.
- All seven inventories precede qualification. Final GREEN/author-review/source/
  base/tree/merge evidence goes on PR262/issue7 footer only, preserving its
  original2600-character prefix; no documentation-only CI repeat.


Native-first inventory:196 matching groups/15,938 PCRE2 10.48/JIT0 protocols,15 diagnostics and34 supplementary cases (245 new/96,528 total). Keep846 strict split-offset(-36) rows separate. Reproduce171 merged261 groups/13,041 protocols, the exact original91 deferred protocols and44 supplemental native profiles. Native tag7978954dbd2efc6f2196869290553cf1871b4ce6/SLJITde0259c7aaf36aa40cba8014f3fad3edde9307f9. All300 prior files/295 C# sources/frozen35 remain fixed.


## Observed RED and fixture freeze

Tests-only source1c9ccd0c178e87a6a905ec4fc9c5d94633c1d0db/treef704874e7d0d6016eba5788ec6d0aefc30bfb23f/checkout4d3e3f04393eb09314de991d3aeeb47773f67f2b (identical tree/ordered base-head parents), [run43879354675](https://github.com/uniblab/Icod.Expressions/actions/runs/43879354675), Linux x64 job133340771591:60 exact expected failures/96,468 passes/96,528 total/zero skips/compiler or analyzer warnings/errors.53 matching IDs1,5,6,11,12,17,18,23,24,29,30,35,36,41,42,47,48,51,53,55,57,59,61,63,69,75,85,86,91,92,93,97,98,106,113,114,121,122,123,124,125,169,170,171,172,177,178,180,184,187,188,195,196; remaining failures6 UTF disabled=false work cases and1 tolerant ownership case. Every native preflight, all96,283 prior tests and185 new controls pass; the original91 research rows fail only after their native preflight. No fixture correction needed. Production still equals merged261.

Freeze before production: reference 3cf50f4013bd25f45926409d1bd00672e17862c9; protocols 26eed11290838b33f6c0aa5f34cf9f4f073f8604. No later fixture edits. All300 prior files/295 C# sources/frozen35 remain byte-identical. All seven inventories precede final source qualification; final GREEN/author-review/source identities belong externally on PR262/issue7 footer.


Implementation and whole-branch author review before qualification: PcreByteProgram.cs only,17 insertions/3 deletions, adds a UTF eligibility arm and constant-time helper. Require ordinary positive Class source, empty ranges, exactly two opposite predicates of the same AsciiDigit/AsciiWord/AsciiSpace kind, null categories and zero property values. Do not normalize class instructions or infer arbitrary universal sets. Existing frontier/capture/global cut/target/proof-budget logic, NoDotStarAnchor/COMMIT entry, start/possessification policy and all parser/AST/emitter/matcher/VM/API/workflow boundaries remain fixed. Two immutable predicate copies introduce no borrowed lifetime or new resource limit.

Reviewed all five plan boundaries: native source/kind discrimination and UCP/property counterexamples, aliases/scoped restrictions/casing and byte controls, all91 original protocols plus malformed31/32 capture-dependency witnesses, cuts/marks/caller-source policies, exact observed/unobserved budgets/cancellation/owned views/signed captures/diagnostics/full capacities. Both frozen new C# files and all300 prior files/295 C# sources/frozen35 remain unchanged. Local frozen source/corpus/A05/classifier/main-job/diff gates pass. No introduced unresolved critical/important/minor findings; broader class provenance remains separately scoped. All seven inventories precede final source qualification; final GREEN/author-review/exact source identities remain external on PR262/issue7 footer.
