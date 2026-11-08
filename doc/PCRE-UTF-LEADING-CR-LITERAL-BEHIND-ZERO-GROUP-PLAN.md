# CR-containing leading literal lookbehind implementation plan

1. Commit 405 native full-capture fixtures against unchanged production
   code. Observe successful-build RED limited to CR-containing LF entries;
   ordinary controls and LF-only leads must pass. Freeze the test blob.
2. Permit valid CR scalars only in the existing bounded caseful positive
   leading literal lookbehind scan. Keep established guards.
3. Append evidence to four canonical inventories and the recent index.
   Verify six native Staging jobs on .NET8/9/10, frozen214, seven equal
   Grep workloads, frozen35 sources, build scripts, immutable tests and
   exact PR merge tree.
4. Mark the PR ready for review. Keep issue #102 open; Grep migration last.
