# Leading negative assertions before nullable group repeats

## Native evidence and intent

After merged #194, probe pinned PCRE2 10.48 for negative assertions
before the existing nullable repeated-group and CR-excluding final class.
One hundred twenty patterns across eight leading negative assertions, three middle
prefixes and five group shapes have LF set, CR clear in the native first
bitmap, positive JIT size, and expected CRLF and ordinary match offsets.

## Scope

The `(?<!\r)` and `(?!\n)` controls already pass through explicit-newline
source handling. Successful-build test-only RED on the first four leads
found 30 new CRLF failures per framework for `(?<!B)` and `(?!B)`;
the 34,571 baseline and 90 controls passed. The extended test-only
matrix pins B, C and wide UTF caseful negative literals without explicit
source newlines before implementation. It contains 240 full-match and
capture fixtures / 480 strict and tolerant checks. Extend only the
existing bounded optional-group proof for single-scalar negative literal
lookbehind or lookahead that excludes explicit CR/LF. Keep source, caller offset,
capture, cancellation and 1,000-visit guards. Parser, matcher, API,
limits and consumers are outside this change.
