# Native 16-bit failure verbs and path labels

The internal raw16 and strict UTF16 sequence entry now admits `(*FAIL)`, `(*F)`,
their named forms, `(*MARK:name)` and `(*:name)`. Failure enters the existing
backtracking path. MARK records the successful path's label without consuming
subject units or adding a saved retry frame. Direct quantifiers on these verbs
remain syntax errors; grouped repetition uses the existing group runner.

## Original word labels

The shared parser retains the original UTF-16 label separately from its byte
name field. Grammar placeholders never become label data. Labels are immutable
strings owned by the compiled pattern and exposed on successful word matches as
`MarkName`; results retain their label across subsequent and concurrent matches.
No subject copying or transcoding is introduced.

Native 16-bit labels have a 65,535-unit capacity, while byte labels retain their
255-byte limit. The word profile checks its smaller 4,096-unit whole-pattern
budget first, including label units after a NUL and both halves of a surrogate
pair. A 4,088-unit MARK label reaches that pattern ceiling. Raw mode preserves isolated surrogate
units. Strict mode validates the entire source before parsing, including label
text after a NUL. At execution, only the prefix before the first NUL is visible.
An empty FAIL argument emits no label assignment; a nonempty argument beginning
with NUL still assigns an empty visible label. Backslashes, spaces and extended
mode comment characters remain literal label text in the admitted profile.
Alternate verb-name escape options remain outside the word profile.

## State and resources

Every existing retry frame saves the current label alongside capture checkpoints,
call identity and subject visibility. Failed alternatives, iterations, calls and
assertions restore that value. Successful positive assertions, atomic groups and
calls retain their label. Both signs of a matching assertion condition retain the
test body's label for the selected arm, just as they retain its captures.
Non-atomic assertion retries restore the label at the resumed choice.

MARK and plain FAIL each consume one existing transition. Named FAIL consumes a
MARK transition followed by a FAIL transition. Label selection is constant-time
at match time: NUL truncation happens during compilation. Labels add no independent
history stack; the 1,024 saved-frame limit and atomic reclamation are unchanged.
All source arms and ignored tails remain subject to the existing bounded grammar,
node, path, repetition and nesting limits. Native FAIL lookbehind width termination
continues to come from the shared parser. Unresolved nested lookbehinds in
native width-terminated tails still return UnsupportedSyntax; four native-valid
patterns explicitly pin this existing bounded-profile limitation.

## Evidence and remaining work

542 fixtures across eight compiled defaults and all 64 call masks contribute
277,504 full-vector and successful-label comparisons against pinned PCRE2-16.
The native preflight produced 200,704 matches and 76,800 misses with no errors.
Its LF output SHA-256 is
`c86aa8058a4e2898d56070bcfa3e10be84c77b7ee12858a54c4e691826d54167`.
The existing word policy oracle accepts `ORACLE_MARKS=1` to append successful
labels as four-digit hexadecimal code units; its default output is unchanged.
Provenance remains in [word-sequence evidence](PCRE-WORD-SEQUENCES.md).

Additional regressions pin explicit labels and captures, cross-width values,
exact work limits, cancellation at each transition of rollback examples, saved
frame boundaries, source/result ownership, concurrent reuse, syntax offsets,
label-length boundaries and strict UTF validation. Six-host managed builds,
tests, package consumers and support checks are recorded on the pull request.

ACCEPT, PRUNE, COMMIT, THEN, SKIP and match-start reset remain open for word
execution. These require early-exit and cut scopes beyond ordinary failure and
label rollback. Broader newline/grapheme support, native profile qualification
and public/package delivery also remain open.
