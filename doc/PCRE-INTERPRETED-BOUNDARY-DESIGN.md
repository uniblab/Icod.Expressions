# Interpreted word boundaries at malformed UTF starts

Match-time anchoring selects the interpreted UTF fragment path. When the
effective start points to a malformed byte, the interpreter can evaluate an
initial empty fragment. Both fragment edges classify as nonword, so `\B`
succeeds and `\b` fails there. On the default/JIT path an interior synthetic
candidate with invalid context can reject both assertions.

Allow the interpreted initial candidate independently of the general nullable
proof. Reuse a real fragment ending at the start when it supplies preceding
context. Restrict the invalid-boundary rejection to the default/JIT path and
use the existing scalar classifiers on both paths. The fragment's end still
prevents consuming malformed bytes. Candidate entry and opcode execution use
the existing work budget and cancellation checks.

The native corpus exercises all 64 match policies per definition, including
both anchor flags, empty-match policy, UTF policy, and repeated calls through
the original subject views. The package consumer uses the same cases.
