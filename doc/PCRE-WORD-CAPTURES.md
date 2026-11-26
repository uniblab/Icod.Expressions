# Native 16-bit captures and rollback

The bounded word compiler now admits numbered and named capturing groups,
including nested alternatives, greedy/lazy group repetition, branch-reset
numbering, inline duplicate names and inline no-auto-capture mode. It uses
the shared parser and capture metadata. UTF names are read from the owned
word source; their 128-unit bound counts original UTF-16 code units. Raw
word names retain the native ASCII restriction, even with UCP enabled.

## Execution and results

Each capturing return context retains its group number and original start.
Completing that group writes its final span. Before each retry is saved, the
frame records the capture journal length. Retrying restores every later
write, then restores the matching position and return contexts. This covers
alternative failure, greedy shortening, lazy growth and policy-rejected
completions. Starting a new top-level branch or candidate resets all slots.

Captures from earlier successful iterations remain when a later iteration
does not enter that inner group. For example, `(a(b)?)+` on `aba` ends with
group 1 at `2:1` and group 2 at `1:1`. Failed iterations cannot overwrite
those spans. Empty captures have length zero; groups that never participate,
including zero-count groups, remain unset.

Successful matches now return an internal `PcreWordMatch`, mirroring the
byte result. Its read-only capture collection includes group zero and every
numbered slot. `CaptureInfo` binds the compiled immutable metadata, and
`GetCapture(name)` returns the first participating duplicate in native
registration order. Unknown and unset names return null; null queries are
argument errors. Canonical number metadata remains sorted independently of
lookup order. Existing group-zero tests now inspect capture slot zero.

The successful result owns the completed call's capture array behind a
read-only wrapper. Matching retains no subject memory in the result and
does not transcode or copy the subject. Spans are relative to the supplied
memory view and remain stable across later calls, source mutation and
concurrent use of the same compiled pattern. Values can be sliced from the
caller's subject using these original code-unit coordinates.

## Resources and compatibility

Group entry and completion retain their previous work charges. Each undone
capture write charges one additional step, with cancellation checked before
and after the observer. Captures are journaled only while a retry can restore
them; journal storage is discarded when no frame can refer to it. Quota,
cancellation and depth failures return no partial match.

The shared parser limits numbered captures to 256. Existing word admission
bounds remain: 64 nested groups, 64 static paths, up to 64 retained unbounded
repeats, finite maxima/minima of 64, 4,096 weighted nodes and 4,096 pattern
units. Runtime retries retain the 1,024-frame ceiling and shared work budget.
Zero-count bodies still contribute validation-lookbehind metadata.

Backreferences, lookarounds, subroutine calls, atomic/possessive groups and
verbs remain rejected by the word compiler. This is an internal execution
milestone; it does not introduce the public PCRE or package consumer API.

## Evidence and next work

116 fixtures across eight compiled defaults and 64 call masks add 59,392
PCRE2-16 comparisons of status, every capture slot and named metadata/lookup.
They include raw isolated surrogates, supplementary scalars, CRLF sequences,
empty/unset groups, retry ordering, branch reset, duplicate names and Unicode
names. The standalone oracle now emits the complete capture vector while
preserving its existing group-zero protocol for capture-free patterns.
Native build/provenance remains pinned as documented in
[word-sequence evidence](PCRE-WORD-SEQUENCES.md).
Standalone preflight produced 47,916 matches and 11,476 misses with no
compile/match errors. The LF-terminated output SHA-256 is
`c99582c5f7444ad37d56df098ad435154b1526678a5d550b17276de2b0054ee4`.

Additional managed tests cover cross-width capture spans and values,
exact work boundaries including rollback, cancellation, frame/work failure,
name/capture/depth limits, ownership, view-relative coordinates and concurrent
calls. Full managed and package qualification is recorded on the pull request.

Next execution work is backreferences, followed by lookarounds, calls and
verbs. Broader newline search, graphemes, package-facing word consumers,
cross-width qualification and public API/resource closure remain open.
