# UCP xdigit fragment recovery plan

Base `9d4c50f39c9a3ac3e45f0177a784c066b31d634d`; 102,866 existing tests.
See [design](PCRE-UTF-XDIGIT-FRAGMENT-RECOVERY-DESIGN.md).
Inline execution; no agents, Python or SDK installation. User owns merge,
tags and publication. Preserve 357 prior test files / 352 C# sources / frozen
A05 source 35. Keep #7/#102 open, Grep last and issue #7 footer unchanged.

## Task 1: native cause and frozen fixtures

- [x] Reproduce 402 merged PR277 groups / 21,952 protocols on exact pins.
- [x] Isolate iterator early-failure analysis and restore the unmodified oracle.
- [x] Pin 428 groups / 23,860 protocols, 15 diagnostics and 42 supplementary
  cases; independently preflight 40 supplementary native outcomes.
- [ ] Publish tests-only draft; reconcile 132 predicted failures and freeze
  all five new C# blobs before production.

## Task 2: bounded metadata and candidate recovery

- [ ] Add the conservative direct-root proof, owned program bool and existing
  fragment-loop cutoff; preserve all existing ALLANY bodies and Step sites.
- [ ] Review whole branch, pass local support gates and update both documents
  plus all five shared inventories before final source upload.
- [ ] Upload the exact reviewed tree with a branch-head lease.

## Task 3: qualify and deliver

- [ ] Six full 103,351 net10 results / frozen 214 cases / 120 methods each,
  net8/net9/net10 builds, zero failures/skips/compiler warnings/errors.
- [ ] All configured supporting/package/script gates green; record skipped jobs.
- [ ] Verify source/base/tree/merge/issue identities, preserve external evidence,
  clean task scratch and observe ready-for-review readback.

Final Task 2/3 completion remains on PR278 to preserve the qualified source
tree. No source/docs/test edits follow final GREEN.
