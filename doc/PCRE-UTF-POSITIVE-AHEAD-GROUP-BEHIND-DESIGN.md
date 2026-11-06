# Positive lookahead before lookbehind-prefixed mandatory groups

Continue issue #102 after merged PR173. Native PCRE2 10.48 retains LF after
CR in the ANYCRLF first bitmap when optional-bracket positive lookahead
precedes an unquantified group whose branches begin with lookbehind and
then a proven mandatory CR-excluding head. The existing group pre-scan
recognizes boundary and lookahead prefixes but not lookbehind. The
iterative body proof already skips the supported unquantified lookbehind.

Admit unquantified literal and general positive/negative lookbehind prefixes
inside one-level capture, noncapture and atomic noncapture groups, with
nonempty branches and existing all-branch requirements. Charge each prefix
against the bounded, cancellable speculative visit count. Require an
already proven mandatory literal, range-class, supported type or positive-
minimum repeat head. The iterative body proof owns the actual budget;
empty, nullable, nested and unsupported heads remain conservative. Keep
runtime assertions/captures, parser, VM, public API and consumers intact.

Native JIT probes show LF present, CR absent and positive JIT sizes for
seven patterns covering single-character and class lookbehind, positive
and negative, capture/atomic groups, alternatives and repeat heads. Pin
strict/tolerant full capture spans on CRLF and ordinary subjects. Commit
tests first and observe a new-class-only RED. Qualify with six native
Staging OS/architecture jobs over net8/9/10, frozen214 and configured
gates with zero failed/skipped tests or compiler warnings/errors.
Issue #102 remains open; release and consumer migration are separate,
Grep last.
