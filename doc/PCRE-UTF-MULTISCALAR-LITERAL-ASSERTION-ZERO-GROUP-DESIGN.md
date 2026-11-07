# Multi-scalar literal assertions before nullable group repeats

## Intent and native evidence

Continue issue #102 after merged185. The specialized optional-bracket
first-bitmap proof currently accepts a literal assertion only if it holds
exactly one complete UTF scalar. Pinned PCRE2 10.48 compile/JIT probes
retain LF and exclude CR for positive leading lookaheads beginning B or
U+0100 and containing a second scalar, with eight middle assertion/boundary
prefixes and five nullable-group shapes. A leading `(?=\rB)` instead
includes CR in the bitmap; the first scalar determines this exclusion.

## Scope and choice

Use the already parsed literal assertion's first complete UTF-8 scalar
to prove that the leading positive, forward, caseful lookahead excludes
CR. Permit multi-scalar caseful literal assertions in the supported middle
prefix while retaining the current two-visit charge per assertion and
1,000-visit bound. Parser-produced literal assertion bytes are valid UTF-8;
the decoder still verifies that the first scalar is complete. Preserve
the same mandatory group-head, cancellation and shape checks. Caseless
assertions, general lookaround bodies, and unrecognized nodes remain
outside this specialized proof. A general bitmap walker change would
affect unrelated placements.

Pin full match and capture spans to native PCRE2 on 600 fixtures / 1,200
strict+tolerant checks spanning five leads, eight middle prefixes, five
repeated-group shapes and three subjects. Keep the frozen source and
immutable-test audits and all established gates.
