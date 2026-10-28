# Internal PCRE branch-reset capture numbering

## Goal and boundary

Advance issue #7 by supporting `(?|...)` in the internal byte/UTF-8 compiler.
It is noncapturing: each direct alternative starts allocating capture numbers
from the same entry value. Captures following the whole group start after the
highest number allocated by any branch. Nested ordinary alternatives continue
numbering normally; nested branch-reset groups maintain their own entry/maxima.

Use the [PCRE2 pattern specification](https://www.pcre.org/current/doc/html/pcre2pattern.html)
and pinned PCRE.NET 1.6.0 / PCRE2 10.48 tests. Numeric and relative references
resolve against the current syntactic capture counter, and final validation uses
the pattern's highest allocated number. A reference reads the latest participating
capture for that number; an unset capture fails. Repeated groups retain earlier
captures when a later branch does not overwrite them, just like ordinary groups.

Names alias numbers globally. Repeating the same name for the same number is
valid without `(?J)`, including nested reset groups. A name cannot identify two
numbers, and a number cannot have two different names. An unnamed declaration
sharing a numbered slot still has any alias assigned to that number elsewhere.
Keep names ordinal, case sensitive, limited to 128 source bytes, and subject to
the current ASCII/UTF category rules.

No public API, package version, runtime dependency, Grep or CommandFramework
changes. Duplicate-name options, subroutine calls, conditionals and general
lookarounds remain pending. Unicode data-version reconciliation and the other
release/migration gates stay open. Do not use subagents.

## Approaches

1. Add branch identities to runtime capture slots: unnecessary extra match state
   and incompatible with PCRE2's shared-number behavior.
2. Rewrite source text or renumber a completed syntax tree: complicates relative
   references, named aliases and original byte diagnostics.
3. **Choose scoped parser numbering.** Save the entry capture count and branch
   maximum in `ParseAlternatives`, reset only at direct alternatives, and restore
   the maximum before returning. Emit existing noncapturing/group nodes. Register
   aliases in both directions to reject inconsistent identities during parsing.

The compiled program, matcher and journal remain unchanged. Frames keep integer
offsets/checkpoints; names and numbering scopes are compilation data only. The
256 capture ceiling applies to the highest slot number, not the total number of
declarations. Pattern depth, repeat marks, bounded bytecode expansion, match work,
saved paths and cancellation retain the existing policies.

## Verification

Compare original byte spans and every numbered capture with pinned PCRE2 for
unequal branch widths, branch order, nesting, numbering after reset groups,
absolute/relative/forward references, named/unnamed aliases, unset and empty
captures, repetition and atomic fallback, quoting/brackets, NUL/high bytes,
UTF/caseless and malformed-subject offsets. Pin exact alias-conflict diagnostics,
capture/depth/expansion ceilings, work limits and cancellation.

Observe test-first failures on a draft PR, then require all six OS/architecture
Staging jobs with full net8/net9/net10 suites, frozen tests and existing
package/workload checks before readiness. The user reviews and merges main.
