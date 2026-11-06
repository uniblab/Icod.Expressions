# Positive lookahead before one nested mandatory group

Continue issue #102 after merged PR174. Native PCRE2 10.48 retains LF after
CR in the ANYCRLF first bitmap when positive lookahead in an optional
bracket precedes a mandatory group whose first head is another group with
proven mandatory branches. The current bounded group pre-scan rejects
that head, while the iterative body proof already traverses its branches.

Admit one nested unquantified capture, noncapture or atomic noncapture
group as the first significant head of each outer group branch. The inner
branches must each have an already proven literal, range-class, supported
type or positive-minimum repeat head after supported unquantified assertion
or boundary prefixes. Reuse the helper with an explicit maximum depth of
one nested group, the visit budget and cancellation checks; speculative
visits remain copied from the actual proof. Empty/nullable branches,
deeper nesting, repeated groups and unsupported heads remain conservative.
Runtime captures/assertions, parser, VM, API and consumers are unchanged.

Pinned native JIT probes show LF present, CR absent and positive JIT sizes
for six one-nested-group patterns. Pin strict and tolerant full capture
spans on CRLF and ordinary subjects, commit tests first and observe RED
limited to the new cases. Require six native Staging OS/architecture jobs
on net8/9/10 with frozen214 and configured gates, zero failed/skipped tests
and zero compiler warnings/errors. Keep issue #102 open; release and
consumer migration separate, Grep last.
