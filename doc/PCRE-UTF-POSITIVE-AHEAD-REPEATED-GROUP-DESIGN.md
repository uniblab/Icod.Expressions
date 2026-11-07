# Mandatory repeated group heads after positive ahead

Continue issue #102 after merged PR176. PCRE2 10.48 JIT reports LF in the
ANYCRLF first bitmap and CR absent when a positive ahead precedes a
positive-minimum repeat of a group with mandatory CR-excluding heads.
This holds for noncapture and capture alternatives, finite repeats,
atomic groups, one containing group and a word-boundary prefix. The
current speculative bitmap proof accepts group heads and primitive
repeats separately, but cannot establish a repeated group head.

Unwrap an optional possessive wrapper around a repeat. Require a positive
minimum and maximum and a capture, noncapture or atomic noncapture group
child. Visit every child branch through the existing bounded iterative
direct-group proof. Charge repeats, groups, branches and prefixes within
the same 1,000-visit budget and check cancellation. The actual proof must
apply the same guard. Zero-minimum, empty, nullable, CR-admitting and
unsupported heads stay conservative. Captures and assertions continue to
execute in the existing VM; parser, API, limits and consumers stay as is.

Pin native JIT full capture spans in strict and tolerant UTF. Commit tests
before implementation and observe new-class-only RED. Then require six
native Staging OS/architecture jobs on net8/9/10, full/frozen214, zero
failed/skipped tests and compiler warnings/errors. Issue #102 stays open;
release and consumer migration remain separate; Grep last.
