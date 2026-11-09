# Caseless negative assertion implementation plan

1. Commit 420 native full-capture fixtures against unchanged production
   code. Observe successful-build RED limited to 210 CRLF entries per
   framework; baseline and 210 ordinary controls pass. Freeze test blob.
2. Admit caseless leading negative literals under the existing eight
   non-CR/LF-scalar guard. Initial GREEN left 140 middle-assertion
   failures per framework; permit caseless literal middle assertions
   only for a validated caseless negative lead. Keep all other guards.
3. Update four canonical inventories and recent index. Verify six
   native Staging jobs on .NET8/9/10, frozen214, seven equal Grep
   workloads, frozen35 source, build scripts, immutable tests and
   exact PR merge tree.
4. Mark ready for review. Keep issue #102 open; Grep last.
