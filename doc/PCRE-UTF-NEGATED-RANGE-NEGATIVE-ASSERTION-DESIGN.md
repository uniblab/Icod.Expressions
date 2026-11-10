# Negated-range negative assertions before nullable group repeats

Continue PR203 and issue #102 under the instruction to continue implementation.
The original 840 full-capture fixtures / 1,680 strict and tolerant comparisons
remain immutable at blob `2aba532d50a5b630c5180ab18f3b3d7399387489`.

## Approach and scope

Extend only the existing bounded negative-class assertion proof to a negated,
predicate-free scalar set with valid nonempty excluded ranges. Check
cancellation and charge each range against the existing 1,000-visit ceiling.
Keep positive assertion and mandatory-head classification unchanged. Preserve
source ExplicitNewline metadata: range interiors are not source endpoints.
Changing that metadata globally would broaden unrelated search behavior;
altering the matcher would duplicate the existing compiler proof.

Native PCRE2 10.48's `set_start_bits` skips OP_ASSERT_NOT and OP_ASSERTBACK_NOT
without inspecting their bodies, then studies the following repeated group.
The negated set need not exclude CR/LF; the repeated group's mandatory head
still must exclude CR. This is deliberately limited to the established
single-body class assertion shape, supported leading/middle placements and
nullable repeated-group proof; it does not generalize all negative assertions.
Source: [pcre2_study.c](https://github.com/PCRE2Project/pcre2/blob/pcre2-10.48/src/pcre2_study.c#L1393).

Keep the optional group bounds and all existing resource/cancellation limits.
No parser, matcher, production fallback, API, dependency, consumer, or frozen
source changes. No subagents, Python, SDK or PowerShell installation. Build on
net8.0/net9.0/net10.0 in Staging on six native OS/architecture runners.
Grep migration stays last; issue #102 stays open.

## Evidence and review

Original test-only run610 compiled without warnings/errors on all six runners;
each target framework failed exactly420 new CRLF cases and passed40031 others,
with no skips. Pinned native evidence recorded by the original PR covers420
patterns with LF present and CR absent in start bitmaps.

Separate48 native parity fixtures cover negated ranges excluding onlyCR,
onlyLF, or neither, paired caseful/caseless, ahead/behind, leading/middle and
CRLF/ordinary. Test-only run611 added24 CRLF failures and24 passing ordinary
controls: total444 failures/40055 passes on the inspected LinuxARM64 and
macOSARM64 frameworks. This disproved the initial proposed requirement that
the negated set exclude both newlines. Preserve both test corpora unchanged;
admit these bounded negated sets based on the native assertion-skip rule.

Review malformed/empty/predicate exclusions, case-folded range behavior,
middle-opcode accounting, shared visit propagation and mandatory group-head
validation. Use CI because the local workspace has no .NET SDK; do not claim
local test runs. Original baseline tests and frozen expectations stay intact.
