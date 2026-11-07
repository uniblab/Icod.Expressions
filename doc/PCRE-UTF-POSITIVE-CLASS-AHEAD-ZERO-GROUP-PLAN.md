# Positive class lookahead before nullable group repeats plan

1. Commit 270 native-reference cases against unchanged production code.
   Confirm successful-build RED for LF entry only, with ordinary controls
   and all baseline cases passing.
2. Add a bounded, success-only direct-head check for the leading positive
   class lookahead in the specialized nullable-group proof. Keep all other
   shape, visit, cancellation and group-head checks. Run full native CI.
3. Update four canonical inventories and the progress index with exact
   evidence. Verify six native jobs, frozen214, seven equal Grep workloads,
   frozen35 source, immutable tests, three-OS build scripts and exact PR
   merge tree before marking ready. Keep #102 open.
