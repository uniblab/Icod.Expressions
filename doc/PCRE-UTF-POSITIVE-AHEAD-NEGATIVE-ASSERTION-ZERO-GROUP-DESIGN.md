# Negative literal assertion before a zero-minimum group repeat

Continue issue #102 after merged PR180. Native PCRE2 10.48 JIT retains
LF and excludes CR in the ANYCRLF first bitmap when an optional bracket
starts with `(?=B)(?!Z)` or `(?=B)(?<!Z)` before a zero-minimum group
repeat. The current exact branch proof accepts boundaries after the
positive literal assertion, but stops at a negative assertion.

Admit exactly one unquantified negative single-byte ASCII literal
lookahead or lookbehind between the positive single-byte ASCII literal
lookahead and a supported zero-minimum, positive-maximum group repeat.
The positive assertion excludes CR even when the repeat consumes nothing.
Every repeated child branch must have a proven mandatory CR-excluding
head. Charge the intervening assertion in the existing 1,000-visit
budget and check cancellation. Other intermediate assertions, longer
chains and unsupported heads remain conservative. Runtime assertion,
repeat and capture execution, parser, API, limits and consumers stay
unchanged.

Pin native JIT full-capture spans in strict and tolerant UTF for both
negative assertion forms, five repeat shapes and three subjects. Commit
tests before implementation, observe a new-class-only RED and require
six native Staging OS/architecture jobs on net8/9/10 with full/frozen214
and zero failed/skipped tests or compiler warnings/errors. Keep issue
#102 open; release and migrations remain separate, with Grep last.
