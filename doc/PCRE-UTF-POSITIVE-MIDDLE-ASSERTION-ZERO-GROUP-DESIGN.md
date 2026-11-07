# Positive literal assertions before zero-minimum group repeats

Continue issue #102 after merged PR182. Pinned PCRE2 10.48 JIT retains
LF and excludes CR in the ANYCRLF first bitmap when an optional bracket
begins with a positive ASCII literal lookahead, followed by a positive
single-byte literal assertion, optionally alongside one boundary or
negative literal assertion, then a zero-minimum group repeat. The
current proof accepts the prior negative and boundary prefixes but
stops at the intermediate positive literal assertion.

Admit an unquantified positive single-byte ASCII literal lookahead or
lookbehind as either of at most two middle nodes. Retain the existing
single-byte ASCII negative assertion and word-boundary options. Charge
two visits per literal assertion and one per boundary under the existing
1,000-visit limit, checking cancellation. Require the leading positive
lookahead to exclude CR and each repeated group branch to have a
mandatory CR-excluding head. Longer chains and other middle nodes
remain conservative. The runtime matcher, parser, public API, limits
and consumers are unchanged.

Pin native JIT full-capture spans in strict and tolerant UTF across
twelve assertion/boundary forms, five group repeat shapes and three
subjects. Commit the test fixtures first, observe new-class-only RED,
and require six native Staging OS/architecture jobs on net8/9/10 with
full/frozen214 and zero failed/skipped tests or compiler warnings/errors.
Keep issue #102 open; release and consumer migration remain separate,
with Grep last.
