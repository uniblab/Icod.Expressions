# Native raw 16-bit oracle plan

Base `e934a49fdabb9f88a3f1d7763ee38de2f1956c67`; 104,050 prior tests.
See [design](PCRE-RAW16-NATIVE-ORACLE-DESIGN.md).
No agents/Python/SDK installation. Preserve prior C# sources/tests; the sole
prior-file exception corrects the native README generated-header include path
and links its raw-mode note to the new bridge documentation.
Keep #7/#102
and issue #7 footer unchanged, Grep last; user owns merge/tags/publication.

## Task 1: native contracts

- [x] Inspect pinned native exports and independently compile C layout facts.
- [x] Pin136 raw/UTF native protocols and reuse150 frozen UTF catalog rows.
- [x] Publish tests-only draft, reconcile294 expected adapter failures and
  freeze the case catalog/assertions before implementing the helper.

## Task 2: bounded test-only ABI adapter

- [x] Implement direct 16-bit calls, explicit UTF selection/effective flags,
  owned results and exception-safe cleanup without production changes.
- [x] Freeze helper, review whole branch, pass local gates and update all
  seven inventories before final qualification.
- [ ] Upload reviewed source/tree with a branch-head lease.

## Task 3: qualify and deliver

- [ ] Six native104,344 net10 results /frozen214 cases /120 methods; all three
  library targets; zero failures/skips/coded warnings/errors.
- [ ] All configured support/package/script gates green; record skipped jobs.
- [ ] Verify identities/issues/reviews, preserve evidence, clean exact task
  scratch and observe ready-for-review readback.

Final Task 2/3 completion stays on PR281; no source/docs/test edits after GREEN.
