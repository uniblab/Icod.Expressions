# Positive leading two-scalar lookbehind implementation plan

1. Commit 180 native full-capture fixtures against unchanged production
   code. Observe successful-build RED for 60 LF entries per framework;
   baseline and 120 ordinary controls pass. Freeze the test blob.
2. Validate one or two UTF scalars in a leading positive caseful literal
   lookbehind, requiring each to exclude CR. Retain existing guards.
3. Append evidence to four canonical inventories and the recent index.
   Verify six native Staging jobs on .NET8/9/10, frozen214, seven equal
   Grep workloads, frozen35 sources, build scripts, immutable tests and
   exact PR merge tree.
4. Mark the PR ready for review. Keep issue #102 open; Grep migration last.
