# DotAll-star anchoring across alternatives

Continue merged PR260 at393afb6551ca1eb6c67e1f0b9ab35ec294b54680,
treeab93d151eb436416219d16ad6aea440c44175a4d;96,063 baseline tests.
Execute inline under standing user direction, without subagents/approval pauses.
User owns merge/tag/publication; #7/#102 remain open; Grep stays last.

## Intent and approaches

Native PCRE2 10.48 is_anchored requires every alternative to qualify. Extend
the existing leading-group proof to root/group alternatives whose every branch
starts with a qualifying DotAll star, directly or through mandatory groups.
Retain the existing owned program policy and candidate loop, including disabled
NoDotStarAnchor's zero-width COMMIT entry proof.

Selected approach: bounded iterative frontier of alternative bodies. Every
branch must be nonempty and have a mandatory leading capturing/noncapturing
group or the existing eligible star. Collect the union of leading enclosing
capture bits. The subsequent global lexical reference map must intersect none
of that union: this is equivalent to testing each path's enclosing map against
the global native backref_map, because every path must pass. Duplicate names,
forward/erased references and native's shared bit0 for capture numbers32+
participate. Reuse existing lexical cuts/targets/provenance checks.

Recursive source traversal is unnecessary and would weaken the bounded heap
policy. Bytecode-only inference loses erased lexical cuts/reference provenance.
Full native inference additionally covers explicit anchors mixed with stars,
some repeated groups, assertions and conditions; those remain separately scoped.

## Boundaries and validation

- Only PcreByteProgram.cs changes in production; parser/AST/opcodes/frames/
  emitter/matcher/VM/diagnostics/API/dependencies/workflows remain fixed.
- Every root/nested alternative must qualify; one empty/non-star/atomic/
  assertion/optional path declines the whole inference. Preserve the existing
  direct possessive-star wrapper and byte ALLANY provenance.
- Cancellation plus4096 total frontier visits/pending entries; the existing
  bounded lexical walk remains. Exhaustion declines inference without adding
  an admission error. No recursion or borrowed lifetime escapes.
- Preserve caller/source NoDotStarAnchor and independence from NoStartOptimize/
  NoAutoPossess, global lexical PRUNE/SKIP/targets exclusions, COMMIT entry,
  capture/mark ownership, original diagnostics and all compile capacities.
- Native-first pinned protocols and exact tests-only RED before implementation.
  Freeze new C# fixtures; all298 prior files/293 C# sources and frozen35 fixed.
- C#13 builds net8/net9/net10; tests only net10 on six native hosts. Keep214
  frozen outcomes/120 methods. All seven inventories precede final qualification;
  final GREEN/author-review/source/base/tree/merge evidence goes on PR261 and
  issue7 footer only, preserving its original2600-character prefix.

NO_JIT interpreter semantics and broader optimizer/API/consumer/release gates
remain pending as documented by PR260; successful proof expansion is not an
unrestricted PCRE2 compatibility claim.


Native-first inventory:171 matching groups/13,041 protocols,15 diagnostics and34 supplementary cases (220 new/96,283 total). Keep282 strict split-offset(-36) research rows separate. Independently reproduce257 merged260 groups/15,033 protocols and44 supplemental native profiles. Native PCRE2 tag7978954dbd2efc6f2196869290553cf1871b4ce6/SLJITde0259c7aaf36aa40cba8014f3fad3edde9307f9; every compiled oracle profile returns0 from JIT complete. All298 prior files/293 C# sources and frozen35 remain fixed. The two-branch instruction ceiling is4082 first-branch literals (Split1 + two stars10 + tailB1 + Jump1 + Accept1 =14 overhead);4083 exceeds4096.


### Pre-freeze scope qualification

Initial tests-only sourcefe2625bd090f4c61772142066877e2ee8ef1ed6e/tree2906ff3a01abdc1ef68ead3198a1733a6a176c4a, [run43870480693](https://github.com/uniblab/Icod.Expressions/actions/runs/43870480693), Linux x64 job133310013154:43 failures/96,241 passes/96,284 total/zero skips/compiler or analyzer warnings/errors; all96,063 prior tests and every native preflight pass.42 failures concern this frontier expansion (33 matching profiles,8 work cases,1 ownership). Profile102 is a separate pre-existing UTF full-class provenance gap: `(?:[\s\S]*A|[\d\D]*B)` under mode5 has native ALLOPTIONS anchored=true, while the managed scalar-set node is deliberately outside the existing Any/byte-ALLANY proof. On C3A942 at offset1 native misses and managed returns2:1.

Keep that profile/91 native protocols as deferred research pinned in the original tests-only commit; remove only its new, unfrozen InlineData row before qualification. No other new assertion/expectation/ID changes; all298 prior files/293 C# sources/frozen35 remain byte-identical. Byte mode retains its eligible full-class coverage; strict UTF boundary controls remain. A separate native-provenance design is needed before recognizing UTF full classes/property complements as ALLANY. Freeze after the corrected42-failure tests-only run; this PR preserves existing star provenance and changes only the all-branches frontier.


## Corrected RED and freeze before production

Tests-only sourceeac741dbb0b7869098b3fb9e00adec78f323263e/treebdffce36a006d979c05d9fb2a01d369095681f99, checkout221382a282bad857c9f75cb85505224defbf7da3 with the same tree/ordered base-head parents, [run43871928507](https://github.com/uniblab/Icod.Expressions/actions/runs/43871928507), Linux x64 job133315239300:42 exact expected managed failures/96,241 passes/96,283 total/zero skips/compiler or analyzer warnings/errors. Matching IDs3,6,9,12,15,18,21,24,27,30,33,36,57,60,63,66,109,110,118,119,120,124,125,137,144,145,151,152,153,154,159,160,169; remaining failures8 disabled=false work cases and1 tolerant ownership case. Every native preflight, all96,063 prior tests and178 new controls pass. Production still equals merged260.

Freeze before production: reference 4e00d532c6567c547dc7c86a5691569cb24b5dc4; protocols bde0dd044e3bde230ea9fc1fbaefdb9bc72ecbf8. No subsequent fixture changes. Preserve all298 prior files/293 C# sources/frozen35 byte-for-byte. Merged260 main43868986288/scripts43868986428 are fully green, with six96,063 net10 results/frozen214/120/all3 library targets/zero compiler or analyzer warnings/errors; Intel aggregate133312727077 verifies the same single result. All seven inventories precede final qualification; final GREEN/author-review/source identities remain external.


Implementation and whole-branch author review before qualification: one production file,37 insertions/21 deletions, replaces single-path peeling with a bounded alternative-body stack. Every branch validates its mandatory leading group/star; capture bits are unioned and tested by the unchanged global lexical walk. Requiring each path to avoid the global backref map is equivalent to checking the union. Zero-branch/empty/optional/atomic/assertion/unknown paths decline. Existing star/ALLANY provenance, target/cut exclusions and disabled-anchor COMMIT entry stay fixed. Frontier visits plus pending entries cannot exceed4096; exhaustion returnsfalse, with cancellation at each body/branch. No parser/emitter/matcher/VM/API/dependency/workflow changes.

Reviewed all five plan boundaries against native fixtures and source, including sibling/nested alternatives and unchanged single paths, dependency union/duplicate/erased/32+ bits, cuts/marks/source-caller policies, exact observed/unobserved budgets/cancellation and ownership/diagnostics/full capacities/proof exhaustion. Both new frozen C# blobs and all298 prior files/293 C# sources/frozen35 remain fixed. Local frozen-source/corpus/A05/classifier/main-job/diff gates pass. No introduced unresolved critical/important/minor findings; the separately pinned pre-existing UTF full-class provenance gap remains research for its own increment. All seven inventories precede final source qualification; final GREEN/author-review/source identities belong externally on PR261/issue7 footer.
