# Negated-range negative assertions before nullable group repeats

Continue PR203 and issue #102 under the instruction to continue implementation.
Pinned PCRE2 10.48 evidence recorded by the tests-first PR covers 420 patterns
with LF present and CR absent in native start bitmaps. The original 840
full-capture fixtures / 1,680 strict and tolerant comparisons remain immutable
at blob `2aba532d50a5b630c5180ab18f3b3d7399387489`.

## Approach and scope

Extend only the existing bounded negative-class assertion proof. A negated,
predicate-free scalar set qualifies when its valid excluded ranges collectively
cover both CR and LF. Check cancellation and charge each range against the
existing 1,000-visit ceiling. Keep positive assertion and mandatory-head
classification unchanged. Preserve source ExplicitNewline metadata: range
interiors are not source endpoints. Changing that metadata globally would
broaden unrelated search behavior; altering the matcher would duplicate a
compiler proof already used for these prefixes.

Keep the single-body assertion shape, leading/middle placement checks, mandatory
CR-excluding repeated-group head, optional group bounds, and all existing
resource/cancellation limits. No parser, matcher, production fallback, API,
dependency, consumer, or frozen-source changes. No subagents, Python, SDK or
PowerShell installation. Build on net8.0/net9.0/net10.0 in Staging on six native
OS/architecture runners. Grep migration stays last; issue #102 stays open.

## Evidence and review

Original test-only run610 compiled without warnings/errors on all six runners;
each target framework failed exactly 420 new CRLF cases and passed 40,031
others, with no skips. Add separate native parity guards for negated ranges
excluding only CR, only LF, or neither; preserve both original test corpora.
Review complement membership, malformed/empty/predicate exclusions, case-folded
range behavior, middle-opcode accounting, and shared visit propagation.
Use CI because the local workspace has no .NET SDK; do not claim local test runs.
