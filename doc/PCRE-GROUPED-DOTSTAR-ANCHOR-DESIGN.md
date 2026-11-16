# Mandatory leading groups in DotAll-star anchoring

Continue merged PR259 (`2ec23cead259d7d4ee6ecf25565253898988c53a`, reviewed
tree `8a6df6850679ec31ce446b5a2998110179ca8767`); baseline95,757 tests.
Standing user direction authorizes inline implementation/review without
subagents or approval pauses. User owns merge/tag/publication; Grep stays last.

## Intent and approaches

Extend the existing bounded direct-root proof through a chain of mandatory,
single-branch capturing/noncapturing groups. Native PCRE2 10.48 is_anchored
recurses into those groups; a referenced enclosing capture prevents inference.
Keep the same owned program policy and candidate loop, including the disabled
anchor's zero-width COMMIT entry proof. This closes a defined part of #102.

Selected implementation: iteratively peel only the first mandatory group of
each single branch, recording native's enclosing capture bitmap. Reuse the
existing lexical AST walk to reject overlapping references and global cuts.
Duplicated reference numbers participate; capture numbers32+ share native bit0.
Direct-root stars retain their existing eligibility. Unknown/over-budget shapes
decline inference without adding a compile capacity error. No recursion.

Full native recursive inference would also cover multiple alternatives,
assertions, conditions and some repeats; that broader scope remains deferred.
Moving the proof to emitted instructions loses erased lexical PRUNE/SKIP and
reference provenance and would duplicate the existing AST boundary.

## Constraints and review boundaries

- Production changes only PcreByteProgram.cs; parser/AST/matcher/opcodes/frames/
  diagnostics/API/dependencies/workflows and all prior fixtures remain fixed.
- Preserve native all-branches rule by requiring one branch at every peeled
  level. Stop at atomic groups/assertions/conditions/repeated groups/empty
  prefixes. The existing direct possessive-star wrapper still qualifies.
- Reuse existing byte ALLANY provenance, caller/source NoDotStarAnchor and
  NoStartOptimize/NoAutoPossess independence. Preserve the4096 proof bound,
  cancellation and fallback; references to unrelated captures remain eligible.
- Native-first pinned match/capture/mark protocols, diagnostics and resource/
  ownership/view/validation/capacity controls must fail for the missing proof
  before production changes. Freeze new C# fixtures after exact RED.
- Keep frozen35 source blobs and214 outcomes/120 methods. C#13 builds
  net8/net9/net10; execution only net10 on all six native hosts.
- Update all seven inventories before final qualification; final GREEN/review/
  source/base/tree/merge identities belong on PR260 and issue7 footer only.

## Separate NO_JIT research

Native10.48/JIT-complete return0 and zero generated JIT size with an active
`(*NO_JIT)` switch to interpreter semantics. Independently reproduced578
merged259 groups/53,205 protocols; adding the prefix changes447 results across
49 profiles, including tolerant UTF split starts, COMMIT, ACCEPT, empty-match
policies and caller anchoring/FIRSTLINE. For example on C3A941 at offset1,
`(*NO_DOTSTAR_ANCHOR)(?s).*?(*COMMIT)A` changes from no-match to2:1.
Acceptance as a no-op would be incorrect. Keep NO_JIT pending until interpreter
policy has its own design and pinned corpus; no existing pending rows change.

Native-first inventory:257 matching groups/15,084 protocols,15 diagnostics and34 supplementary cases (306 new/96,063 total). Keep372 strict split-offset(-36) research rows separate. All296 prior files/291 C# sources remain fixed. Native PCRE2 tag7978954dbd2efc6f2196869290553cf1871b4ce6, SLJITde0259c7aaf36aa40cba8014f3fad3edde9307f9; every compiled protocol returns0 from JIT complete.
