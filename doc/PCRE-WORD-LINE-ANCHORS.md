# Native 16-bit LF line assertions

The bounded word-sequence compiler admits `^`, `$` and `\Z` using the
existing LF newline convention. Inline `(?m)` and `(?-m)` select multiline
behavior through the shared parser. Assertions may appear in alternatives,
finite choices and all four supported unbounded-repeat continuations.

Without multiline mode, `^` requires subject start and `$` accepts subject
end or the position before one final LF. In multiline mode, `^` also accepts
positions after LF, except the position after a final LF; `$` accepts positions
before any LF. `\Z` accepts subject end or the position before a final LF
regardless of multiline mode. `\A` and `\z` retain absolute semantics.

Compiled and per-call `NotBol` and `NotEol` restrictions combine. They affect
`^` and `$`, while absolute assertions and `\Z` ignore them. In multiline
mode the restrictions suppress subject-boundary matches, leaving interior
LF boundaries available. A CR in CRLF is an ordinary unit under this LF
profile. Other newline conventions remain outside this increment.

Matching reads the original raw-word or strict UTF-16 subject view and
reports original code-unit coordinates. The existing work quota charges
each assertion evaluation, including retries inside repeats. Following
PCRE2, neither caret nor `\Z` extends the strict validation window backward;
the one-character window for `\A` and word boundaries remains distinct.

## Evidence

Forty fixtures across eight compiled defaults and 64 call masks compare
20,480 outcomes with pinned PCRE2-16. Standalone native preflight returned
2,868 matches and 17,612 misses with no native errors. Its LF-terminated
output SHA-256 is
`0051df02cde5c6b489c09d117daf619a2a64652814bd3adf4b015edb70b0700f`.
The matrix covers start/end/empty policies, inline multiline changes,
LF and CRLF subjects, offsets, alternatives, repeat continuations, raw
surrogates and supplementary Unicode. Separate tests cover managed
cross-width results, malformed-prefix validation, view limits and work budgets.

The oracle revisions are pinned in [word-sequence evidence](PCRE-WORD-SEQUENCES.md).
General nested execution, captures, other newline conventions, graphemes
and broader P10–P12 gates remain open.
