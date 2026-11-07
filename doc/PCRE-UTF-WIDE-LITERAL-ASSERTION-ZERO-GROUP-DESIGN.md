# UTF-scalar literal assertions before nullable group repeats

## Intent and native evidence

Continue issue #102 after merged184. The specialized optional-bracket
first-bitmap proof currently accepts only one-byte ASCII literal assertions.
Pinned PCRE2 10.48 compile/JIT probes retain LF and exclude CR for leading
positive lookaheads of U+0100 and U+1F642, plus positive and negative wide
literal lookahead/lookbehind in the middle prefix, over five nullable-group
shapes. All 105 native probes produced positive JIT sizes and the CRLF match.

## Scope and choice

Decode exactly one complete UTF-8 scalar from each literal assertion under
the already UTF-only proof. The leading assertion must remain positive,
forward, caseful and CR-excluding; middle assertions remain caseful. Retain
the existing per-node charges, 1,000-visit limit, cancellation and mandatory
CR-excluding repeated-group head. A literal containing multiple scalars,
caseless assertions, general lookaround bodies, and other nodes remain outside
this proof. Widening the generic bitmap walker would alter unrelated shapes;
special casing one encoding width would miss other valid UTF scalars.

Pin PCRE2 full match and capture spans against the compiled engine on 315
fixtures / 630 strict+tolerant checks spanning three leading assertions,
seven middle prefixes, five repeated-group shapes and three subjects.
Keep the frozen source and immutable-test audits and all established gates.
