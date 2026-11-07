# Positive class lookahead before nullable group repeats

## Intent and native evidence

Continue issue #102 after merged186. The optional-bracket UTF start-bitmap
proof currently accepts a leading positive literal lookahead, but not a
positive lookahead with a single CR-excluding bracket class or supported
shorthand. Pinned PCRE2 10.48 compile/JIT probes retain LF and exclude CR
for six such leads, three short middle prefixes and five nullable-group
shapes. A CR-admitting `\s` or `[B\r]` lookahead includes CR in its bitmap.

## Scope and choice

Admit a positive general lookahead only when its body has exactly one
branch and one caseful direct scalar set or supported scalar class.
Reuse the existing CR-excluding head proof with a success-only visit
checkpoint. Charge the assertion body and branch in addition to the
existing leading and repeated-group visits. Keep the existing supported
middle prefixes, 1,000-visit bound, cancellation and mandatory CR-excluding
repeated-group head. Multi-node, nullable, negated, predicate or CR-admitting
lookahead bodies remain outside this proof. Generalizing the whole bitmap
walker would affect unrelated placements.

Pin full match and capture spans against native PCRE2 for 270 fixtures /
540 strict+tolerant checks across six leads, three prefixes, five repeated
group shapes and three subjects. Preserve the frozen source and immutable
test audits and all established gates.
