# Bounded zero-width chains before nullable groups plan

1. Commit the 303 native-reference cases against unchanged production code.
   Run the native PR matrix; expect failures specifically for LF on CRLF, with
   the B and BC controls passing and the frozen suite unchanged.
2. Replace the two-node count gate with a minimum two-node shape gate. Retain
   per-node cost, cancellation and the 1,000-visit bound in the specialized
   proof. Re-run the full native CI matrix, frozen214 and build/pack gates.
3. Append the audited result to the four canonical inventories and recent
   progress index. Confirm seven equal Grep workloads, build scripts on three
   OSes, immutable test blob, frozen35 source and the exact PR merge tree.
   Mark the draft ready after all checks pass. Keep issue #102 open.
