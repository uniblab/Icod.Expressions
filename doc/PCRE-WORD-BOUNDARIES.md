# Native 16-bit word boundaries

The bounded word-sequence path admits `\b` and `\B` before, between and
after its supported atoms and repeats. Each assertion compares the word
membership on its two sides without consuming input. Ordinary mode uses
ASCII letters, digits and underscore. UCP uses the pinned Unicode word
predicate; the caller's `AsciiBsW` option and inline `(?aW)` restriction
select ASCII membership. Caseless matching does not change membership.

Raw mode reads original 16-bit units. Strict UTF-16 reads complete scalars
in both directions, including supplementary letters and symbols. Subject
start and end have no adjacent word character, and reads stay within the
supplied memory view. Every assertion and adjacent-character classification
charges the cancellation-aware work quota. Boundary assertions also work
inside finite choices and greedy or lazy repeat continuations.

PCRE2 assigns a one-character validation lookbehind to both word boundaries
and `\A`. The word compiler records that requirement across all branches.
Strict matching rewinds the validation start accordingly, including the
native scan across preceding low-surrogate units. Malformed text earlier
than that window remains outside validation. A split-scalar start retains
its invalid-start diagnostic. This also corrects the previous suffix-only
validation for `\A`; literal compilation retains its existing window.

## Evidence

Forty fixtures across six defaults and 64 call masks compare 15,360 outcomes
with pinned PCRE2-16. Standalone native preflight returned 8,056 matches and
7,304 misses without native errors. Its LF-terminated output SHA-256 is
`aa9dcb5db644f60fae65bf659be5263e1d35707c8523e97fc2014123c747c94f`.
The matrix includes empty subjects and retries, offsets, UCP and ASCII
restrictions, raw surrogates, supplementary characters, alternatives and
repeat continuations. Additional tests cover managed cross-width parity,
malformed validation windows, view boundaries and exact work budgets.

The native source contract is in PCRE2 10.48's `pcre2_compile.c` handling
of `ESC_b`, `ESC_B` and `ESC_A`, and `pcre2_match.c` UTF validation rewind.
The oracle revisions are pinned in [word-sequence evidence](PCRE-WORD-SEQUENCES.md).
General nested execution, captures, broader newline/grapheme handling and
the remaining P10–P12 gates are still open.
