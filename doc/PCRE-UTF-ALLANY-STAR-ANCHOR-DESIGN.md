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
