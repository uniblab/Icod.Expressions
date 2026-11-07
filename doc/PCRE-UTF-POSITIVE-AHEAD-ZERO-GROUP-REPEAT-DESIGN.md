# Literal positive ahead before zero-minimum group repeats

Continue issue #102 after merged PR177. Pinned PCRE2 10.48 JIT retains
LF in the ANYCRLF first bitmap after CR when an optional bracket begins
with positive lookahead for B and then contains a zero-minimum repeat of
a group. The repeat may consume nothing, so the mandatory group-repeat
proof from PR177 correctly rejects it. The positive assertion itself
excludes CR from any successful nonempty entry into that optional body.

Recognize only a branch consisting of a positive single-scalar literal
lookahead immediately followed by a zero-minimum, positive-maximum repeat
of a capture, noncapture or atomic noncapture group, optionally wrapped
by a possessive modifier. Require the assertion scalar to be valid and
different from CR, and prove every repeated child branch has a supported
CR-excluding mandatory head with the existing iterative group proof.
Charge the assertion, repeat, group and branch visits against the existing
1,000-visit budget and check cancellation. This specific branch can be
accepted as CR-excluding because its assertion cannot succeed at CR;
runtime matching still executes both nodes. No inference is made for
arbitrary positive assertions, other nullable continuations or groups
with unsupported heads. Parser, VM, API, limits and consumers stay as is.

Pin native JIT full-capture spans in strict and tolerant UTF for
noncapture, capture, finite, atomic and possessive zero-minimum group
repeats. Commit tests before implementation and observe a new-class-only
RED. Then require six native Staging OS/architecture jobs on net8/9/10,
full/frozen214, zero failed/skipped tests and compiler warnings/errors.
Issue #102 remains open; release and consumer migration remain separate;
Grep last.
