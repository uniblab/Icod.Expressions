# UTF range-union implementation plan

Base a90946818ae9831c2ede700f5ff1db9c6cc34924; floor100,340 tests.
Spec: [design](PCRE-UTF-RANGE-UNION-STAR-ANCHOR-DESIGN.md).
Inline execution; no subagents or local .NET SDK installation. Branch and PR
updates are authorized development work; user owns merge/tags/package release.
Preserve331 prior test files/326 C# sources/frozen35. #7/#102 remain open;
Grep last; issue7 footer unchanged under its existing publication block.

## Task1: native evidence and frozen tests

- [x] Build the exact PCRE2 10.48/SLJIT pins and reproduce455 merged272 groups/
  55,384 protocols, including signed reset lengths.
- [x] Generate530 matching groups/29,225 native protocols,15 diagnostics and
  72 supplementary cases; complete40 dynamic groups/120 literal preflights.
- [ ] Publish tests-only draft; verify RED196/100,761/100,957 with all prior
  tests passing and no unrelated failure, then freeze all five new C# blobs.

## Task2: coverage proof and review

- [ ] Add only a compile-local shared4096 coverage counter and a bounded,
  cancellation-aware complete-pass frontier proof in PcreByteProgram.cs.
- [ ] Keep old constant/member/predicate paths and charges, refuse explicit
  full-range members in the new proof, and preserve exhaustion-as-fallback.
- [ ] Review the whole branch against every design boundary; preserve old
  tests/source/frozen identities and pass local support/whitespace gates.
- [ ] Update all seven inventories before final qualification and upload
  the exact reviewed tree using a branch-head lease.

## Task3: qualify and deliver

- [ ] Six fresh full100,957 net10 results, frozen214/120, all three library
  targets and configured support gates; zero failures/skips/build warnings.
- [ ] Verify source/base/tree/proposed-merge identities, issue states, final
  evidence on the PR and ready readback; preserve the tested source tree.
- [ ] Identity-checked task scratch cleanup after evidence preservation.

Final Task3 completion is recorded externally on the PR rather than causing
a documentation-only CI repeat after final GREEN.
