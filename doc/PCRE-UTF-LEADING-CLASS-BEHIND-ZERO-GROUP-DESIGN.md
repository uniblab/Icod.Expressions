# Positive leading class lookbehind before nullable group repeats

## Intent and native evidence

Continue issue #102 after merged189. The optional-bracket UTF start-bitmap
proof now accepts a one-head class lookbehind in a supported middle prefix,
but rejects that same positive lookbehind as the leading assertion. Pinned
PCRE2 10.48 compile/JIT probes over 90 lead/prefix/group patterns retained
LF and excluded CR, with positive JIT sizes. All three subject matches per
pattern had the expected native spans.

## Scope and choice

Permit only a leading positive caseful lookbehind whose body has one branch
and one direct positive CR-excluding scalar set or supported scalar class.
Reuse the established class-assertion proof, body/branch/head visit charges,
and the existing optional bracket, middle zero-width scan, mandatory
repeated-group head, cancellation and 1,000-visit ceiling. A general bitmap
walker would affect unrelated placements; keep it separate. Multi-node,
nullable, negated, predicate and CR-admitting heads remain outside this
bounded proof. Parser, matcher, API, limits and consumers are unchanged.

Pin full spans and captures for 270 native fixtures / 540 strict and
tolerant checks: six leading lookbehinds, three middle prefixes, five
repeated-group shapes and three subjects. Observe test-only RED first.
