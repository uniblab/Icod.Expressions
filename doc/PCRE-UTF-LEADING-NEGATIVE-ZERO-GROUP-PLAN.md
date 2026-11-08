# Leading negative assertion implementation plan

1. Commit native-reference tests before production changes; watch the
   successful-build RED and confirm all 34,571 baseline plus ordinary
   and explicit-newline negative controls pass. The first RED isolated
   30 `(?<!B)`/`(?!B)` failures; expand B to C and wide UTF before fixing.
2. Extend only the leading optional-group proof for one caseful,
   CR/LF-excluding negative literal assertion ahead or behind. Keep all guards.
3. Update four canonical inventories and recent index. Verify six
   native Staging jobs on .NET8/9/10, frozen214, seven equal Grep
   workloads, frozen35 source, build scripts, immutable test blob and
   exact PR merge tree.
4. Mark ready for review. Leave issue #102 open; Grep migration last.
