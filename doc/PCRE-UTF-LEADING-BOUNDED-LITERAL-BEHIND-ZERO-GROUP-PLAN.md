# Bounded leading literal lookbehind implementation plan

1. Commit 270 native full-capture fixtures against unchanged production
   code. Observe successful-build RED for 90 LF entries per framework;
   baseline and 180 ordinary controls pass. Freeze the test blob.
2. Raise only the leading positive literal lookbehind scan from two to
   eight valid CR-excluding UTF scalars. Keep established guards.
3. Append evidence to four canonical inventories and the recent index.
   Verify six native Staging jobs on .NET8/9/10, frozen214, seven equal
   Grep workloads, frozen35 sources, build scripts, immutable tests and
   exact PR merge tree.
4. Mark the PR ready for review. Keep issue #102 open; Grep migration last.
