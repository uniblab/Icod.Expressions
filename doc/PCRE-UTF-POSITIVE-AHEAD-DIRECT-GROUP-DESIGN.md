# Positive lookahead before direct mandatory groups

Continue issue #102 after merged PR170 (30,745 tests; frozen 214). The
optional-bracket UTF bitmap proof can look past positive ahead before direct
mandatory literals, classes, supported types and their proven repeats. It
still stops before a mandatory group that begins with such a direct head.
Pinned native PCRE2 10.48 retains LF after CR in its first bitmap under
ANYCRLF for these one-level groups.

Admit only an unquantified capture, noncapture or explicit atomic noncapture
group whose every branch begins immediately with a direct head accepted by
the existing literal, class or type predicates. Probe the group's branches
iteratively against a copy of the visit count, with cancellation and bounded
range charges; the existing mandatory-body traversal validates each branch
again and owns the actual visit count. A missing, empty or unsupported branch,
skipped prefix, nested group, repeated group and unproven head stay on their
existing conservative paths. Preserve all-branch requirement, success-only
checkpoints, runtime assertions/captures, parser, VM and public API.

Native PCRE2 10.48 with JIT enabled reports LF present, CR absent and positive
JIT sizes for optional noncapture, capture and atomic groups, direct range
class heads and a two-branch group. Pin full capture spans on CRLF and ordinary
subjects in strict and tolerant UTF. Commit tests before production and
observe a successful-build RED confined to this class. Require six native
Staging OS/architecture jobs on net8/9/10, full suite and frozen214 with no
failed/skipped tests or compiler warnings/errors, plus all configured gates.
Keep issue #102 open. Release and consumer migration remain separate; Grep
last.
