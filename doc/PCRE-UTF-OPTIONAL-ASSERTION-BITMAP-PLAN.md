# Optional assertion bitmap implementation plan

Execute inline; no subagents. Spec: PCRE-UTF-OPTIONAL-ASSERTION-BITMAP-DESIGN.md.

- [x] Pin native literal spans and six-newline/UCP/every-offset protocol results;
  generate a new immutable test class and resource/cancellation controls. Publish
  tests-only draft and observe new-class RED with all25373 baseline tests passing.
- [x] Extend HasUtfCrLfOptionalAssertionClass into a shared node-aware bitmap
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

## Focused correction

- [x] Publish403 focused scoped-group cases with production unchanged from the
  first fix; observe RED confined to the two new classes and preserve both blobs.
- [x] Share mandatory leaf classification with a finite optional noncapturing
  single-leaf proof. Preserve old direct optional-atom guards and fallback budgets.
- [ ] Qualify all six jobs/full26465/frozen214 on net8/9/10 and all configured
  gates, then update this tranche's inventories and issue102 recent footer.

Observed focused RED: run516/Linux ARM64, all three TFMs;282failures (10original
+272focused),26183passes,0skips; all25373baseline+810newcontrols pass; compiler
warnings/errors0. Original and focused test blobs remain frozen. Initial proof
run515 left10original failures; its545 resolved failures remain covered.
