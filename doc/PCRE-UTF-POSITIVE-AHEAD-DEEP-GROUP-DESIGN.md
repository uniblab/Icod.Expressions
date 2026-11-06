# Positive lookahead before bounded deep mandatory groups

Continue issue #102 after merged PR175. Native PCRE2 10.48 retains LF after
CR in the ANYCRLF first bitmap through mandatory noncapture chains of
depth 2, 3, 4, 8, 16, 32, 64 and 128, and capture chains of depth 2, 4
and 8. The existing speculative group proof stops at one nested group.

Traverse unquantified capture, noncapture or atomic noncapture heads with
an explicit stack. Every branch must reach an existing proven mandatory
literal, range-class, supported type or positive-minimum repeat head after
supported unquantified assertion/boundary prefixes. Charge the same
1,000-visit budget for groups, branches and prefixes; check cancellation
on each iteration. The pending stack remains bounded by charged branch
visits. As before, the caller passes a speculative copy of visits and the
iterative body proof owns actual charges. Empty/nullable branches,
repeated groups and unsupported heads remain conservative. No recursion,
runtime matcher change, parser/API change or consumer migration.

Pin PCRE2 10.48 JIT full capture spans in strict and tolerant UTF at
noncapture depths 2, 4, 8, 16, 64 and 128 and capture depths 2, 4, 8.
Commit tests before production, observe a new-class-only RED, then run
six native Staging OS/architecture jobs on net8/9/10, frozen214 and
configured gates with zero failed/skipped tests or compiler warnings/errors.
Issue #102 remains open; release and consumer migration remain separate,
Grep last.
