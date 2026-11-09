# Caseless negative assertion implementation plan

1. Commit 240 native full-capture fixtures against unchanged production
   code. Observe successful-build RED limited to 120 CRLF entries per
   framework; baseline and 120 ordinary controls pass. Freeze test blob.
2. Admit caseless leading negative literals under the existing eight
   non-CR/LF-scalar guard. Keep all other proof conditions.
3. Update four canonical inventories and recent index. Verify six
   native Staging jobs on .NET8/9/10, frozen214, seven equal Grep
   workloads, frozen35 source, build scripts, immutable tests and
   exact PR merge tree.
4. Mark ready for review. Keep issue #102 open; Grep last.
