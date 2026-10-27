# PCRE backtracking state journal plan

**Goal:** remove full state clones on every internal PCRE split while preserving byte-level capture and rollback semantics.

**Architecture:** retain mutable candidate arrays and record prior values in a value-type journal only while frames exist. Saved frames hold integers: instruction and byte positions, journal checkpoint, and atomic nesting depth. Undo on backtracking and clear discarded history when no saved frame remains.

**Tech Stack:** C# 13, net8/net9/net10, xUnit, pinned test-only PCRE.NET oracle.

**Spec:** `doc/PCRE-CAPTURE-JOURNAL-DESIGN.md`

## Global constraints

- No production PCRE.NET or `System.Text.RegularExpressions` fallback.
- Keep GNU/POSIX matching and all consumer repositories untouched.
- Build Debug locally where available; Staging in PR CI across six OS/architecture targets.
- No subagents.

## Review focus

- A failed nested branch restores participating, empty, and unset groups.
- Repeated groups restore both start positions and captures on backtracking.
- A completed atomic scope prunes inner choices without suppressing an outer fallback.
- Unanchored candidate starts do not inherit prior captures or repetition marks.
- Cancellation, work, and depth limits preserve their diagnostics and accounting.

## Task 1: characterize rollback and allocation

**Files:** create `tests/Pcre/PcreCaptureJournalTests.cs`.

- [ ] Add behavioral tests covering the first four review cases.
- [ ] Add a warmed per-thread allocation test with 96 captures and 500 ordered choices; bound the matching call below 350 KB.
- [ ] Run the focused tests on baseline and observe the allocation case fail (CI if local .NET is unavailable).

## Task 2: replace snapshots with undo entries

**Files:** modify `src/Pcre/PcreByteMatcher.cs`.

- [ ] Append prior capture, start, and mark values while saved paths exist.
- [ ] Store integer checkpoints and atomic nesting depth in value-type frames.
- [ ] Undo on failure; clear stale journal entries after pruning all frames.
- [ ] Run focused and full suites; inspect exact CI head on six targets.

## Task 3: document and review

**Files:** modify `doc/PCRE-DESIGN.md`.

- [ ] State that the internal byte VM uses journalled rollback and that workload performance remains an exit gate.
- [ ] Self-review the diff, verify CI, then report the PR without claiming Grep readiness.
