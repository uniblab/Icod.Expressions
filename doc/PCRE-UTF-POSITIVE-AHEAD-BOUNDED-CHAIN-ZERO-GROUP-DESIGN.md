# Bounded zero-width chains before nullable groups

## Intent and native evidence

Continue issue #102 by matching PCRE2 10.48's UTF LF entry bitmap when a
positive ASCII literal lookahead is followed by a long chain of zero-width
boundaries or ASCII literal assertions and then a zero-minimum group repeat.
Native compile/JIT probes kept LF and excluded CR for 3 through 900 repeated
boundaries or assertions and the five existing nullable-group shapes; captures
were checked on CRLF subjects. The current two-node cap prevents those entries
in our compiled matcher, which can lose the LF match under `(*ANYCRLF)`.

## Scope and choice

Remove the arbitrary chain-length cap from the existing specialized proof.
Keep its assertion restrictions, cancellation checks and 1,000-visit budget.
Each boundary costs one visit; each literal assertion costs two. The mandatory
CR-excluding group-head proof still must fit the remaining budget. This extends
only the already proven shape, and does not claim general zero-width or nullable
group support. Broadening the generic first-bitmap walker would affect more
placements; a fixed higher node cap would merely move the mismatch.

Pin native and compiled match spans and all captures for strict and tolerant
UTF modes across five group shapes, four prefix units, five repetition counts,
three subjects, and a 900-boundary budget case. Preserve the frozen-source and
immutable-test audits, all CI gates and the seven-workload Grep comparison.
