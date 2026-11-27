# Native 16-bit atomic groups and possessive repeats

Raw 16-bit and strict UTF-16 sequences now admit `(?>...)`, the `(*atomic:...)`
alias, and possessive repetition of admitted atoms, groups and backreferences.
Atomic groups can themselves be repeated. Existing directly quantified
assertions retain their established lowering and work accounting.

## Commit boundaries and captures

Atomic entry saves a capture checkpoint and the enclosing continuation on
the shared retry stack. The body can retry its own branches and repeat counts
until it succeeds. Completion discards only the body's saved choices, keeps
its successful captures, and retains the consumed subject position. An
exhausted body restores its entry capture state and fails into the enclosing
retry stack. Outer alternatives and repetitions remain independently retryable.

For example, `(?>a|ab)c` fails on `abc`: after the first alternative consumes
`a`, the suffix cannot reopen the atomic choice. Conversely, `(?>ab|a)c`
can match `ac` because its first alternative fails inside the body before
commitment. Captures from that failed alternative are rolled back.

A possessive quantifier places one commit boundary around the entire ordinary
repeat. This retains existing finite-count and empty-progress behavior while
preventing a later suffix from reducing its count or reopening body choices.
Repeating an explicit atomic group instead commits each iteration separately;
the enclosing repeat can still surrender complete iterations.

Nested atomic and non-atomic assertions, capture references, branch-reset and
duplicate-name groups reuse the same capture journal. Guard entry, body return,
choice pruning and capture undo all retain bounded work and cancellation checks.
Variable-lookbehind endpoint visibility is saved and restored by these guards,
including when an outer choice resumes after an atomic attempt.

## Compiler and resource behavior

Source group bodies remain compiled in postorder. A possessive repeat passes
through one synthetic branch using the existing atom/group/reference repeat
lowering. That branch does not recursively compile nested source group bodies.
The word runner adds a consuming atomic guard to its existing iterative loop.
Native differential tests also exposed an extra empty iteration in existing
counted group repeats: both the byte emitter and word loop now check progress
on the final required group copy, preserving all earlier required copies.
This matters when a zero-width body updates captures. Directly quantified
assertions keep their separate finite-bound rule. No native fallback, subject
copy, transcoding or retained subject memory is introduced.

The existing 64-level source nesting, 64-static-path, 64-retained-unbounded-repeat,
4,096-weighted-node and 1,024-saved-frame ceilings remain. Atomic commitment does
not erase compile-time complexity charges. Finite atom choices still count as
static paths, so `a{0,64}+` exceeds that path ceiling while `(a){0,64}+` uses the
existing retained group loop. Exact and required unbounded counts retain their
64-count limit. Captures and validation windows from zero-count bodies remain
registered, and unsupported body syntax is still rejected.

Failed or cancelled calls publish no partial capture result. Compiled patterns
own their source and name metadata; returned capture spans remain stable across
later calls, including calls that exhaust the shared frame limit.

## Evidence and remaining scope

339 fixtures across eight compiled defaults and all 64 call masks add 173,568
PCRE2-16 comparisons of status and every capture slot. They cover atomic aliases,
finite/unbounded possessive counts, repeated atomic groups, nested assertions,
captures and references, empty iterations, outer rollback, fixed/variable
lookbehind, Unicode casing/classes, supplementary characters, raw surrogate
units, newline selectors and original caller offsets.

Standalone pinned native preflight produced 100,712 matches and 72,856 misses
with no native errors. Its LF-terminated output SHA-256 is
`d28fdbb822d3dab82ae465f2614faa3b83d43cf1759c5abd23de84ed110f9409`.
Build/provenance is retained in [word-sequence evidence](PCRE-WORD-SEQUENCES.md).

Additional tests pin explicit commitment/capture results, cross-width spans and
values, exact work quotas, cancellation during pruning and undo, frame exhaustion,
source ownership, durable results, compile-time bounds and original diagnostic/UTF
validation offsets. Full six-host build, test and package evidence is recorded
on the pull request. The counted-group correction additionally compares byte
and word capture vectors against native PCRE2-16 for capturing, noncapturing
and atomic groups, required counts one through three, greedy/lazy/possessive
preference, raw/UTF modes and all 64 call policies.

Calls, conditions and verbs are next. Broader newline search, graphemes,
word package consumers and public API/resource qualification remain open.
