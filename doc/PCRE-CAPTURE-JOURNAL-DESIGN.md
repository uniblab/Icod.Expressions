# PCRE backtracking state journal

**Status:** internal matcher refactor under issue #7. No public API, package version, or consumer changes.

## Intent

The byte matcher currently clones the entire captures, capture starts, repetition marks, and atomic-depth array at every `Split`. This makes allocation grow with the number of captures times the number of saved paths. Keep the ordered VM and its match-work and frame limits, but store offsets and journal checkpoints in saved frames. Restore only state that actually changed after a split.

## Design

Keep the working capture, start, and mark arrays for each candidate start. Each mutation while a saved path exists appends a compact value entry containing the prior value, slot, and kind. A split frame stores the program counter, byte position, journal length, and current atomic nesting depth. On failure, pop the frame and undo entries back to its checkpoint in reverse order. Mutations made with no saved path need no undo entry; clear stale entries once atomic pruning discards every saved frame.

Atomic scopes continue to store the backtracking-stack boundary. A frame inside a completed atomic scope is pruned, so restoration can truncate atomic nesting to the saved depth. No frame stores a span or interior pointer; its persistent state consists of integers only. The existing match-work limit bounds the number of instruction mutations; the 1024-frame limit remains unchanged. This is a performance improvement, not a change to the provisional limits.

## Evidence and bounds

Add focused tests for nested capture rollback, repetition marks, atomic pruning with outer fallback, and fresh candidate starts. Add an allocation regression using many captures and saved choices: on a warmed matching run, the journal must stay below 350 KB of current-thread allocation, a threshold that the old per-split array snapshots exceed. Existing PCRE2 whole-match and capture reference tests, the frozen GNU corpus, and six-target Staging CI remain required. The test threshold is for this synthetic workload, not a Grep performance claim; compare Grep's documented workloads later.
