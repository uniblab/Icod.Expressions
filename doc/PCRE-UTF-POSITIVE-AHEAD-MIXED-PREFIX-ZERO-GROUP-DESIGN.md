# Mixed assertions before a zero-minimum group repeat

Continue issue #102 after merged PR181. Native PCRE2 10.48 JIT retains LF
and excludes CR from the ANYCRLF first bitmap after a positive literal
lookahead followed by two zero-width nodes before a zero-minimum group
repeat. This includes negative single-byte ASCII literal lookahead or
lookbehind and word boundary or nonboundary in either order, plus pairs
of negative assertions. The current proof admits two boundaries or one
negative assertion, but stops at these short combinations.

Inspect at most two middle nodes. Admit only unquantified word-boundary
nodes or negative single-byte ASCII literal assertions. Charge one
budget visit per boundary and two per negative assertion, plus the
existing assertion/repeat cost. Keep the 1,000-visit limit and check
cancellation. Require the positive single-byte ASCII literal lookahead
to exclude CR, and a proven mandatory CR-excluding head in every child
branch of the zero-minimum, positive-maximum group repeat. Longer or
other intermediate chains remain conservative. Runtime execution,
parser, API, limits and consumers are unchanged.

Pin native JIT full-capture spans in strict and tolerant UTF for ten
two-node combinations, five repeat shapes and three subjects. Commit
test fixtures first, observe new-class-only RED, then require all six
native Staging OS/architecture jobs on net8/9/10 with full/frozen214
and no failed/skipped tests or compiler warnings/errors. Issue #102
remains open; release and migrations remain separate, with Grep last.
