# Boundary between positive ahead and zero-minimum group repeat

Continue issue #102 after merged PR178. Pinned PCRE2 10.48 JIT retains
LF in the ANYCRLF first bitmap after CR when an optional bracket begins
with a positive literal lookahead, one word boundary and then a
zero-minimum repeat of a capture, noncapture or atomic noncapture group.
PR178's bounded proof accepts the adjacent assertion and repeat but
correctly stops when a boundary separates them.

Extend that narrow branch proof to accept exactly one unquantified word
boundary between the existing positive single-byte ASCII literal
lookahead and the supported zero-minimum group repeat. The lookahead
must exclude CR, and every repeated child branch must retain a proven
mandatory CR-excluding head. Charge the extra boundary visit within the
existing 1,000-visit budget and check cancellation. Other intermediate
assertions, multiple boundaries, nullable continuations and unsupported
heads remain conservative. Runtime matching executes the boundary,
assertion, repeat and captures unchanged; parser, API, limits and consumers
are unchanged.

Pin native JIT full-capture spans in strict and tolerant UTF across
noncapture, capture, finite, atomic and possessive repeated group shapes.
Commit tests before implementation, observe a new-class-only RED, and
require six native Staging OS/architecture jobs on net8/9/10 with
full/frozen214, zero failed/skipped tests and compiler warnings/errors.
Issue #102 stays open; release and consumer migration remain separate;
Grep last.
