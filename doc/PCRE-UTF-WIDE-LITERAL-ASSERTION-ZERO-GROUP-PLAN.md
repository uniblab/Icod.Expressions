# UTF-scalar literal assertions before nullable group repeats plan

1. Commit the 315 native-reference cases against unchanged production code.
   Confirm a successful-build RED run with the expected LF entry failures
   only; ordinary controls and all baseline cases must pass.
2. Change the specialized proof to accept a complete single UTF-8 scalar
   for its leading and middle literal assertions. Retain all shape, visit,
   cancellation and group-head checks. Run full native CI on net8/9/10.
3. Append exact evidence to the four canonical inventories and recent
   progress index. Verify six native jobs, frozen214, seven equal Grep
   workloads, frozen35 source, immutable tests, build scripts on three OSes
   and the exact PR merge tree before marking the PR ready. Keep #102 open.
