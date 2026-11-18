# UTF UCP xdigit iterator fragment recovery

Continue merged PR277 at `9d4c50f39c9a3ac3e45f0177a784c066b31d634d`,
tree `dcbf170e7184a7fd35237158873678f78759e201`; floor 102,866 tests.
Work inline without subagents, Python or local SDK installation. User owns
merge/tags/publication. Preserve 357 prior test files / 352 C# sources and
frozen A05 source 35. Keep #7/#102 open, Grep last and issue #7 footer unchanged.

## Native cause

PCRE2 10.48 `7978954dbd2efc6f2196869290553cf1871b4ce6` and SLJIT
`de0259c7aaf36aa40cba8014f3fad3edde9307f9` reproduce all 402 merged PR277
groups / 21,952 protocols, including original byte spans and labels.
PCRE.NET 1.6.0 remains the CI oracle. All generated matching protocols use
JIT_COMPLETE return zero and positive JIT sizes.

Under UCP, POSIX xdigit becomes PT_PXXDIGIT / XCLASS, including a complementary
pair. `[[:xdigit:][:^xdigit:]]*?B` on tolerant UTF C3A942 from byte 1 misses
with either NoDotStarAnchor selection; NoStartOptimize permits 2:1. On
C3A9FF42 from byte 0, start study similarly prevents recovery into the later
valid fragment; disabling it selects 3:1. A match within the first fragment
still succeeds: 4142FF42 selects 0:2. This is not ALLANY star anchoring.

Pinned pcre2_compile.c converts POSIX xdigit to PT_PXXDIGIT; pcre2_jit_compile.c
detect_early_fail assigns a direct root CLASS/XCLASS unbounded iterator a
type_skip / fast_forward_bc_ptr, guarded by START_OPTIMIZE. Reset resumes from
its saved iterator pointer. General Unicode category complements can take a
different compile_xclass_matchingpath shortcut in pcre2_jit_char_inc.h.

A controlled research build disables only the detect_early_fail call while
retaining start study and bytecode. Both default and NoDotStarAnchor probes
then select B at byte 2. Restoring the exact source restores the native miss.
This causal build is separate from fixture generation: the final oracle is
rebuilt from unmodified pinned source, verified by git diff.

## Conservative proof and runtime contract

Change only PcreByteProgram.cs and PcreByteMatcher.cs. Add one owned immutable
program bool selected only for tolerant UTF with start study enabled and no
subroutine targets. Require one direct root branch whose first node is an
unbounded repeat with minimum 0 or 1 over a positive ordinary Source:Class
set. A directly wrapped possessive repeat is eligible; enclosing groups,
leading assertions/conditions and root alternatives are outside this proof.
Require a literal continuation immediately after a greedy repeat. A lazy
repeat may have one intervening path mark and/or a capturing group whose
single branch begins with a literal. No-tail and optional continuations are
excluded: their native invalid-byte consumption is a separate boundary.

Precheck a combined 4,096 predicates/ranges. Scan complete lists with per-visit
cancellation. Every predicate must be exact UnicodeHexDigit / null category /
zero property value; require both polarities. Duplicates and literal ranges
entirely inside 0..127 are admitted. Other predicates, outer negation, wider
ranges, finite/minimum-two repeats and class algebra remain excluded.

Bound a whole retained AST walk independently at 4,096 body/branch/node visits
and queued nodes. Permit pure consuming atoms, captures/noncaptures, ordinary
repeats, directly possessive repeats and path marks. Inspect zero-count bodies
too. Reject references, calls, assertions, conditions, cuts, explicit atomic
groups, resets and unknown nodes, even when erased from emitted code. Use a
heap worklist and cancellation, not recursion. Exhaustion declines proof
without new syntax/admission limits. All seven existing ALLANY proof bodies
and their resource contracts remain unchanged.

Matching retains whole-subject validation and all its Step charges. Reuse the
existing fragment loop with one integer cutoff: a valid requested scalar can
search its containing valid fragment; an invalid or split requested byte can
try only that byte's existing zero-width candidate. Return no match before a
candidate beyond the cutoff. Do not infer anchoring or create candidates.
NoStartOptimize bypasses the proof; NoDotStarAnchor is independent. Strict
UTF diagnostics, normal valid-subject work, caller anchors/empty policies,
owned source/options/captures/marks and cancellation retain their contracts.

No opcode, frame/journal, parser, AST layout, public API, dependency, workflow
or public quota change; no new matching allocation or rescan. Existing
instruction handling and Step placement are unchanged. Broader iterator/source
provenance and group/assertion/call/cut shapes remain #102 work.
Programs containing surrogate literals stay outside the new proof.

## Native-first fixtures and qualification

428 groups / 22,936 native protocols, 15 diagnostics and 42 supplementary cases
add 485 tests, reaching 103,351. Native shadow runs with start study disabled
independently predict 116 matching failures; 16 supplementary failures are
four fragment-work, six ownership and six admitted list boundaries. Expect
132 failures / 103,219 passes, zero skips/compiler warnings/errors; all
102,866 old tests and 353 new controls must pass. Forty additional native
supplementary literal outcomes are preflighted, including 4,096/4,097 visits.

Cover greedy/lazy/possessive stars/plus, duplicate/reordered/ASCII-range members,
UCP/casing/restriction/source prefixes, NoStart/NoDot, byte/strict/tolerant UTF,
every admitted byte offset, malformed/split bytes, NUL/U+10FFFF, captures/marks,
caller anchoring/empty policies, exact observed/unobserved work and cancellation,
sliced ownership, instruction 4,090/4,091 and capture/depth 256 capacities.
Strict exclusions are 1,440 malformed-whole-subject and 540 split-offset rows.
Proof-declined controls use valid aligned subjects; do not claim general raw
fragment parity for wider shapes or over-budget proofs.

Tests-only RED and exact reconciliation precede freezing every new C# blob and
production. Whole-branch author review and all seven inventories precede final
qualification. Build net8/net9/net10; execute full net10 and frozen 214 cases /
120 methods on six native hosts, with zero failures/skips/compiler warnings/
errors and all configured support/package/script gates. Final evidence and
plan completion remain external on PR278 without edits after final GREEN.

## Refined tests-only scope

Initial source `f649ce3dbf4fdd54e67c7178104715a0d40ac1e5`, tree
`8df00fbc8d1dbdd196282c811cf3ac8199563e2a`, proposed merge
`fde454694c81e5838028f02029c9c6083f407ab0`, run 44221313205 / Linux x64
job 134523576996: 158 failures / 103,193 passes / 103,351 total, zero skips,
native assertion frames or compiler/analyzer warning/error codes. All old
tests pass. The 116 predicted matching and all 16 supplementary failures are
present, plus 26 independent native invalid-consumption profiles.

Greedy marked continuations (20 profiles) and no-tail empty-rejection cases
(6 profiles) can natively report a match starting inside a valid scalar, also
when start study is disabled. For example, UCP `[[:xdigit:][:^xdigit:]]*`
followed by MARK/captured B on C3A942 from byte 1 reports 1:2 natively, while
the managed fragment-preserving path reports 2:1. This increment must not
reinterpret malformed bytes as consumable Unicode scalars. Preserve these
exact raw protocols on the immutable initial source above; no old expectation
is changed. Restrict 28 unfrozen marked-greedy/no-tail cohorts (including two
already passing no-tail controls) to valid aligned subjects, removing 924 raw
research rows. Their remaining native expected spans are unchanged.

The refined 22,936 protocols retain all 116 iterator-recovery witnesses and
all 16 supplementary failures. Before production, require a fresh successful-
build RED of exactly 132 managed failures / 103,219 passes, no native failures,
and freeze all five new C# blobs. Narrow the compiler proof's continuation
shape as specified above; broader native invalid consumption remains research.
