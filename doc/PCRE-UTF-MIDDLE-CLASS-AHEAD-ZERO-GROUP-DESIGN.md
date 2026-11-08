# Middle class lookaheads before nullable group repeats

## Intent and native evidence

Continue issue #102 after merged187. The optional-bracket UTF start-bitmap
proof now accepts one positive CR-excluding direct class lookahead at the
beginning, but rejects the same lookahead in the middle zero-width prefix.
Pinned PCRE2 10.48 compile/JIT probes retain LF and exclude CR for two
leading assertions, seven middle class/boundary combinations and five
nullable group repeat shapes. All 70 patterns produced JIT code and the
CRLF match.

## Scope and choice

Reuse the existing one-branch, one-head positive class lookahead proof
for middle nodes. Charge the middle assertion opcode separately from its
body, branch and range visits on a success-only checkpoint. Keep existing
word boundaries, literal assertions, 1,000-visit bound, cancellation and
mandatory CR-excluding repeated-group head. Other lookaround bodies,
caseless, predicate, negated and CR-admitting class heads remain outside
this extension. The leading assertion still establishes CR exclusion.

Pin native match and capture spans for 210 fixtures / 420 strict+tolerant
checks across two leads, seven middle prefixes, five repeat shapes and
three subjects. Keep frozen source and immutable-test audits and all
established gates.
