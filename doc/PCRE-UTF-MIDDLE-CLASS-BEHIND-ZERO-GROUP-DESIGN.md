# Positive middle class lookbehind before nullable group repeats

## Intent and native evidence

Continue issue #102 after merged188. The specialized optional-bracket UTF
start-bitmap proof admits a positive direct class lookahead in a middle
zero-width prefix but rejects a positive direct class lookbehind there.
Pinned PCRE2 10.48 compiled/JIT probes over 60 lead/middle/group patterns
retain LF and exclude CR, with positive JIT sizes. Native full-capture
matching succeeds at LF after CR and at the ordinary class positions.

## Scope and choice

Allow a middle positive lookbehind with exactly one branch and one caseful
direct positive scalar set or supported scalar class. Reuse the existing
CR-excluding mandatory-head proof and charge its opcode, body, branch and
head visits. Retain the leading assertion, optional bracket, repeated-group
head, cancellation and 1,000-visit bound. Generalizing the whole bitmap
walker would affect unrelated placements; this limited proof keeps that
work separate. Multi-node, nullable, negated, predicate and CR-admitting
lookbehind bodies remain outside this extension. Parser, runtime, API,
limits, consumers and release behavior remain unchanged.

Pin full spans and captures for 180 native fixtures / 360 strict and
tolerant checks across two leads, six middle lookbehinds, five group
shapes and three subjects. Test-only RED precedes the source change.
