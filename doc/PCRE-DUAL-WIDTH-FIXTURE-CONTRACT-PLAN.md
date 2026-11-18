# Dual-width fixture contract plan

Base `40184bb937bca12a0fa9587efe57d1c55eb7fbe9`; 103,848 prior tests.
See [design](PCRE-DUAL-WIDTH-FIXTURE-CONTRACT-DESIGN.md).
Work inline; no agents/Python/SDK installation. Preserve existing test/source
identities and issue #7 footer. User owns merge/tags/publication; Grep last.

## Task 1: native protocols and tests-only contract

- [x] Inspect pinned wrapper/native sources and build independent 8/16 oracles.
- [x] Pin 140 shared cases, ten compile errors, eight malformed-subject errors,
  two raw 16-bit witnesses and 44 independent coordinate controls.
- [ ] Publish tests-only draft, reconcile 194 intended adapter failures and
  freeze the four fixture/assertion sources before implementing the helper.

## Task 2: independent coordinate normalization

- [ ] Implement only the test-support helper; retain original byte offsets,
  reject invalid/bisected inputs, preserve empty/unset/signed ranges.
- [ ] Freeze helper, review whole branch, run local gates and update all seven
  inventories before final qualification.
- [ ] Upload exact reviewed source/tree with a branch-head lease.

## Task 3: qualify and deliver

- [ ] Six native 104,050 net10 results / frozen 214 cases / 120 methods each;
  net8/net9/net10 builds; zero failures/skips/coded warnings/errors.
- [ ] All configured support/package/script gates green; identify skipped jobs.
- [ ] Verify source/base/tree/merge/review/issue identities, preserve evidence,
  remove exact disposable research and observe ready-for-review readback.

No production source, public API, dependency, workflow or package version change.
Final Task 2/3 completion stays on PR280; no source/docs/test edits after GREEN.
