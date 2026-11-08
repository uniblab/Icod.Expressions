# CR-containing leading literal lookbehind verification plan

1. Probe the pinned native engine's first bitmap, JIT and subject spans
   for CR-containing leads before nullable group repeats.
2. Run full-match and capture reference fixtures against unchanged
   production code. If the cases pass, preserve the CR guard. Remove
   duplicate theory rows so every enumerated case runs exactly once.
3. Document the native bitmap versus CRLF candidate distinction in the
   canonical inventories and recent index. Verify six native Staging
   jobs on .NET8/9/10, frozen214, seven equal Grep workloads, frozen35
   sources, build scripts, immutable tests and exact PR merge tree.
4. Mark the PR ready for review. Keep issue #102 open; Grep migration last.
