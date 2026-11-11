# Script Run Implementation Plan

Goal: four native script-run group aliases, byte/UTF8, shared bounded VM.
Spec: doc/PCRE-SCRIPT-RUN-DESIGN.md. Execute inline, no subagents.

## Constraints and review focus

C#13/net8/9/10, UTF8+LF, no Python/SDK/PowerShell installation or new package.
No production fallback; preserve frozen35/frozen214/120 and fixed PR211–213
fixtures. Baseline46127. User owns merge/tag/publication; Grep last. Review
native order-dependent Han states, extension intersections, adjacent Nd sets,
Unknown singleton exception, ASR commit before validation, recursive mark
restoration, transparent tree/width rewrites, original span versus K/G0,
invalid fragments and resource cancellation. Existing optimizer gaps stay102.

1. Publish PcreScriptRunReferenceTests.cs and PcreScriptRunFixtureData.cs with
   independent native masks and focused cases. Replace the obsolete unfrozen
   pending-script-run row with a native-valid pending verb. Require clean-build
   unsupported-syntax RED with46127 baseline passes, then fix new test/data blobs.
2. Add AST wrapper/group aliases and transparent tree/width/empty/newline paths.
   Compile journaled Mark/body/ScriptRunCheck with shared state bounds; lower
   ASR with atomic body inside validation. Add charged scalar scanner using
   compressed primary/map descriptors. Extend existing script generator/data
   and pinned-category decimal-set lookup; exact regeneration must pass.
3. Update four inventories and roadmap. Author-review all changes and proposed
   merge tree. Require18 native framework/platform results, frozen214/35,
   five Unicode regenerators, workload/package/API/extraction/corpus/glibc,
   build scripts and all applicable full compatibility gates. Record evidence,
   mark ready; keep worktree/branch for user merge.

## Execution evidence

Tests-onlyd977efb / RED642 run42941044977, Linux ARM64 job130262663198:
all frameworks compile with0warnings/errors; 46192 cases,64 expected new
unsupported-script-run failures,46128 passes including46127 baseline cases and
one already-supported quoted-literal case,0skips. Fixed test blob
fa5bd8561a423f957e39be39df31478993c2f10b; mask fixture
6cb0cbfa000ef8e7642305ea141e5f7fbd093cd1. Both remain unchanged after RED.

Implementation66a1477 / RED643 run42942079010, Linux x64 job130266030513:
all frameworks46192 cases/2failures/46190passes/0skips, clean compilation.
All65 new script-run cases pass. Two older pending-syntax rows in
PcreGroupAliasReferenceTests still expect sr/asr to be unsupported; retarget
those unfrozen pending rows to native-valid COMMIT/THEN verbs. This is test
inventory maintenance for newly implemented syntax, not a behavioral fixture
change. The new fixed test/data blobs and all frozen cases remain unchanged;
no production correction is needed. Re-run complete qualification including
script data regeneration and all three source-pinned compatibility jobs.
