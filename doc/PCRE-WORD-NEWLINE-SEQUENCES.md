# Native 16-bit newline sequences

The bounded word-sequence compiler admits `\R` through the shared parser.
Its existing Unicode BSR profile recognizes CRLF as one indivisible newline
sequence, and recognizes lone CR, LF, VT, FF, NEL, LINE SEPARATOR and
PARAGRAPH SEPARATOR. This fixed list is independent of UCP and is available
in both raw-word and strict UTF-16 modes. Dot and line anchors retain their
existing LF convention; admitting `\R` does not change that convention.

Newline sequences work in ordered alternatives, exact and finite variable
counts, and all four admitted unbounded-repeat levels with finite choices.
Greedy backtracking rewinds a complete previously consumed newline sequence,
including both CRLF units. Each repeat retains its own starting position so
that an LF consumed after a prefix's CR is rewound as one unit. Lazy repeats
also consume CRLF atomically. For example, `\A\R+\n\z` cannot match a single
CRLF by splitting it, while `\A\r\R*\n\z` can retry the star at zero.

Matching reads the supplied subject view directly and reports original
16-bit code-unit coordinates. A view ending after CR treats that CR as a
complete newline, even if an LF exists outside the view. Strict validation
still starts at the requested offset unless another assertion requires an
earlier character. Atom consumption and each complete rewind share the
existing work quota and cancellation checks.

## Evidence

Forty fixtures across eight compiled defaults and 64 call masks compare
20,480 outcomes with pinned PCRE2-16. Standalone native preflight returned
14,048 matches and 6,432 misses with no native errors. Its LF-terminated
output SHA-256 is
`25643fc036997c52f98ef92388a68ea1b2201a5a1cded67a0caf55a5db8a01be`.
The matrix covers every supported newline, CRLF atomicity, finite choices,
all four repeat levels, raw surrogates, supplementary prefixes and policies.
Additional checks compare managed widths and exercise exact work budgets,
cancellation during rewind, view bounds and the strict validation window.

The oracle revisions are pinned in [word-sequence evidence](PCRE-WORD-SEQUENCES.md).
BSR directives, other global newline conventions, graphemes, possessive and
general nested execution, durable captures and broader P10–P12 gates remain open.
