# Optional assertion bitmap implementation plan

Execute inline; no subagents. Spec: PCRE-UTF-OPTIONAL-ASSERTION-BITMAP-DESIGN.md.

- [ ] Pin native literal spans and six-newline/UCP/every-offset protocol results;
  generate a new immutable test class and resource/cancellation controls. Publish
  tests-only draft and observe new-class RED with all25373 baseline tests passing.
- [ ] Extend HasUtfCrLfOptionalAssertionClass into a shared node-aware bitmap
  helper. Reuse PcreUtf8.TryRead for collapsed nonempty negative literal ahead;
  add mandatory literal/type heads to the existing all-branch worklist. Extract
  shared mandatory set range and four-type guards without changing optional-atom
  source rules. Keep exact/zero paths, budgets, ownership and runtime unchanged.
- [ ] Append four inventories and one recent index row. Verify original test
  hashes/source blobs, whitespace, C# compilation and author self-review. Publish
  source; audit all six native jobs and every configured gate at the exact head.
- [ ] Update only issue102 recent footer after success; link complete evidence
  and leave ready/unmerged PR for the user. Keep broader PCRE/release/Grep gates
  and #99/#100/#159 open.
