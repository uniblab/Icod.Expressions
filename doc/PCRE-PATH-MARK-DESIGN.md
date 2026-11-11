# MARK successful-path byte labels

PR220 follows merged PR219. Implement `(*MARK:NAME)` and `(*:NAME)` with the
last label on a successful matching path. Add byte-preserving MarkName metadata
to the internal PcreByteMatch result; the exported API remains unchanged. Named
SKIP and other verb arguments remain separate gates with their existing fixed
rejection fixtures intact. Failed/partial-attempt label reporting remains an
API/start-optimization gate, as PCRE2 recommends disabling start optimization
when failure marks matter.

## Grammar and architecture

A raw nonempty name ends at the first closing parenthesis. Both headers accept
1..255 bytes in the8-bit engine, including UTF8 multi-byte names and raw octets
in byte mode. Backslash/extended-mode trivia are literal inside names without
ALT_VERBNAMES. Match metadata follows the native NUL-terminated view, including
embedded NUL truncation. Empty/missing/overlong names retain original byte errors;
direct quantifiers reject, grouped repetition is ordinary, and MARK has zero
width without terminating behind tail analysis. Unknown headers stay unsupported.

Use a PcreBytePathMark leaf and PathMark opcode. One lazily allocated slot in
the existing256-mark array stores the latest label instruction index plus one;
zero means no label. Opcode byte operands hold immutable visible name bytes
from the compiler-owned source snapshot. Writes use the existing mark journal,
so branch/assertion/repetition rollback restores prior labels and atomic success
retains accepted labels. Conditional assertion body success retains labels with
captures, including the pinned negative-condition outcome. Standalone negative
and failed-positive assertions discard interior labels.

Successful callee Return skips restoration of this one global path-label slot;
other positional marks still restore. Ordinary/forced callee failure restores
labels through its guard even when selected pre-verb capture values survive.
At Accept, attach the immutable visible byte memory directly to the result.
No label stack, per-name dictionary, per-label backtrack frame, runtime fallback
or new public API. The bounded COMMIT first-code-unit proof crosses MARK, since
native entry optimization precedes label execution at the chosen candidate.

Alternatives considered: one mutable last-label variable without journal loses
rollback; a separate label stack duplicates bounded VM state; one saved frame
per label needlessly consumes the cut/choice quota for successful metadata.
The existing journal plus one shared slot is the implementation choice under
standing authorization to continue the roadmap inline.

## Qualification and bounds

Pinned PCRE2 10.48 get_mark/JIT/grammar and PCRE.NET8-bit Mark spans are oracles.
Independent literal exports cover captures/K, alternatives/repeats, positive/
negative/conditional/non-atomic assertions, behind, returned/recursive calls,
selected capture/cut distinctions, script/grapheme scopes, atomic success,
ACCEPT/NOTEMPTY, mixed controls, raw names, NUL and both headers. All selected
malformed-UTF/CRLF offsets compare full capture and successful label results.
Resource cases pin shared256 marks,4096 expansion, work/cancellation and1025
label writes without consuming saved frames. Source/result name ownership must
survive caller pattern mutation, later matches and compiled-pattern lifetime.

Tests freeze after clean successful-build RED. A test-only reflection bridge
reads the planned internal MarkName property without adding production
scaffolding before RED; it compares actual bytes, not source structure. Preserve
all earlier fixed reference blobs, GNU/BRE/ERE and frozen35/frozen214/120.
C#13/net8/9/10; no Python, installations or subagents. Named verb gates, failure
marks, remaining directives/callouts, UTF C, wider optimizer/resources, UTF16/
public API, consumer performance and release stay open. Grep stays last.
