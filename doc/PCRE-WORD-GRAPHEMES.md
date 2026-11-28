# Native 16-bit grapheme matching

The internal word engine accepts `\X` on raw 16-bit and strict UTF-16 input.
It reads the original char view and returns code-unit capture coordinates.
Raw mode applies Unicode break properties to each individual unit, including
isolated surrogates. UTF-16 combines valid surrogate pairs into scalars.
`\X` remains independent of UCP, case folding and the selected line convention.

## Shared rules and word traversal

Both widths use the existing pinned `PcreUnicodeGraphemes` classification and
join rules. The byte traversal keeps its existing behavior. Word traversal
handles combining marks, Hangul, CRLF, emoji modifiers, extended pictographs
with ZWJ context, and regional-indicator parity in original units. It does not
use host string segmentation or transcode a subject.

A caller can start inside a cluster at a valid scalar boundary. Regional runs
inspect preceding subject context, using the existing UTF validation window for
interpreter dispatch. A match-local memo avoids scanning an entire regional run
again at every candidate or repeated atom. It contains only input-derived bounds
and survives capture rollback without sharing state between matches.

## Atomicity and retries

A consumed cluster has no internal retry edge. Capturing groups, assertions,
calls, conditions and control verbs retain their existing journals and scope.
Greedy JIT-style atom repeats retain consumed endpoints and rewind one complete
cluster. An endpoint list belongs to one repeat entry, is not modified after
that entry is saved, and needs one retry frame regardless of its length.
Terminal repeats without a retry edge need no endpoint list. Lazy repeats grow
by one complete cluster at each retry.

Call-time anchoring and existing scoped-ACCEPT rules select the pinned native
interpreter behavior. Its greedy atom retry scans backward using the Unicode
pair table, without the forward regional-indicator parity and extra EP/ZWJ
context checks. This can combine multiple forward clusters while rewinding.
For example, `(\X*)\X\z` on four regional indicators matches under ordinary
JIT dispatch and fails with call-time anchoring. Required copies establish the
minimum backward boundary. Explicit group repetition keeps group retry behavior.

Finite variable `\X` quantifiers retain their runtime repeat instead of expanding
into independent source paths. This preserves the interpreter's backward retry
semantics. Existing word limits remain: finite bounds and required copies are
at most 64, with the existing source, expansion and shared 1,024-frame caps.
`\X` in a class and a consuming `\X` in lookbehind remain invalid syntax.

## Qualification

`PcreWordGraphemeTests` compares full captures and successful labels with
PCRE2-16 at every valid caller start, under eight compiled policies and all
64 call masks. It covers raw and UTF-16 input, nested execution, source control
verbs, finite and unbounded repeats, and starts inside combining, emoji and
regional sequences. Additional tests check every pinned break-table boundary,
all representative type pairs with UCP on and off, malformed-prefix validation,
selected cross-width coordinates, sliced views, owned results and concurrent
reuse. Long runs verify linear observed work and repetition beyond 1,024 atoms.
Work-limit checks include the exact budget and one less. Cancellation interrupts
every observed transition in both dispatch modes and permits subsequent reuse.

Eight new package cases bring the word smoke result to 58. The former `\X`
unsupported-syntax sentinel now uses an unimplemented script run, preserving its
diagnostic purpose. Unreachable-source admission tests likewise retain a script
run after the newly supported atom.

The reference remains PCRE.NET 1.6.0 with PCRE2 10.48. The native
[forward traversal](https://github.com/PCRE2Project/pcre2/blob/pcre2-10.48/src/pcre2_extuni.c)
and [interpreter repeats](https://github.com/PCRE2Project/pcre2/blob/pcre2-10.48/src/pcre2_match.c)
explain the separately tested retry rules. See the
[oracle provenance](../tests/Pcre/Native/README.md) and
[roadmap](PCRE-ROADMAP-STATUS.md) for the remaining qualification gates.
Native word script runs are the next execution increment.
