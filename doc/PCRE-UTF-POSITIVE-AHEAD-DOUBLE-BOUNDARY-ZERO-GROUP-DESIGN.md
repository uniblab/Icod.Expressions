# Two boundaries before a zero-minimum group repeat

Continue issue #102 after merged PR179. Native PCRE2 10.48 JIT retains
LF and excludes CR from the ANYCRLF first bitmap when an optional bracket
starts with `(?=B)\b\b` or `(?=B)\B\B` before a zero-minimum group repeat.
The existing exact branch proof stops at one intermediate boundary.

Extend only that proof to accept exactly two unquantified word-boundary
nodes between a positive single-byte ASCII literal lookahead and a
supported zero-minimum, positive-maximum group repeat. The lookahead
excludes CR even when the repeat consumes nothing. Every repeated group
branch must have a proven mandatory CR-excluding head. Charge both
boundaries against the existing 1,000-visit budget and check cancellation.
Other intermediate nodes and longer chains remain conservative. Matching,
parser, public API, limits and consumers stay unchanged.

Pin native JIT full-capture spans in strict and tolerant UTF for both
boundary forms, five repeat shapes and three subjects. Commit tests before
implementation and observe a new-class-only RED. Require six native Staging
OS/architecture jobs on net8/9/10 with full/frozen214 and zero failed or
skipped tests and compiler warnings/errors. Keep issue #102 open; release
and migration remain separate, with Grep last.
