# Positive lookahead before prefixed mandatory groups

Continue issue #102 after merged PR172. Native PCRE2 10.48 retains LF after
CR in the ANYCRLF first bitmap when a positive lookahead in an optional
bracket precedes an unquantified group whose branch begins with a word
boundary or lookahead and then a mandatory CR-excluding head. The group
pre-scan currently insists that the first node itself be a direct head;
the existing iterative body proof already skips supported assertions.

Admit only capture, noncapture and atomic noncapture groups with nonempty
branches. Scan each branch's leading unquantified word boundaries and
supported negative/positive lookaheads (including lowered literal assertions)
under the existing visit limit and cancellation policy, then require a proven
mandatory literal,
range-class or supported type head or positive-minimum repeat. Use the
caller-provided copy of the visit count so unsuccessful speculation cannot
spend the real budget. Leave empty, nullable, nested or unsupported heads
conservative; the iterative body proof remains the final authority. Preserve
runtime assertion behavior, captures, parser, VM, API and consumers.

Native JIT probes show LF present, CR absent and positive JIT sizes for
seven prefixed group patterns. Pin strict and tolerant full capture spans
on CRLF and ordinary subjects. Commit tests first, observe new-class-only
RED, then run six native Staging OS/architecture jobs over net8/9/10 and
frozen 214, with zero test failures/skips or compiler warnings/errors.
Keep issue #102 open; release and consumer migrations remain separate,
Grep last.
