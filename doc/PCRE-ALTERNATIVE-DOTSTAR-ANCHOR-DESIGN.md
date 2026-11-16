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


Native-first inventory:172 matching groups/13,132 protocols,15 diagnostics and34 supplementary cases (221 new/96,284 total). Keep282 strict split-offset(-36) research rows separate. Independently reproduce257 merged260 groups/15,033 protocols and44 supplemental native profiles. Native PCRE2 tag7978954dbd2efc6f2196869290553cf1871b4ce6/SLJITde0259c7aaf36aa40cba8014f3fad3edde9307f9; every compiled oracle profile returns0 from JIT complete. All298 prior files/293 C# sources and frozen35 remain fixed. The two-branch instruction ceiling is4082 first-branch literals (Split1 + two stars10 + tailB1 + Jump1 + Accept1 =14 overhead);4083 exceeds4096.
