# Internal PCRE named captures implementation plan

**Goal:** Resolve unique named captures and backreferences to the existing
numbered capture VM.

**Architecture:** Parser-only aliases, temporary immutable reference nodes and
a post-parse resolution pass. No runtime lookup or new backtracking state.

**Tech stack:** C# 13, net8.0/net9.0/net10.0; pinned test-only PCRE.NET 1.6.0.

**Spec:** [PCRE-NAMED-CAPTURES-DESIGN.md](PCRE-NAMED-CAPTURES-DESIGN.md).

## Constraints and review focus

- Root project/solution, `/src`, `/tests`, `/samples`; local Debug, CI Staging.
- No public API, package version, consumer or CommandFramework changes.
- No subagents. Execute and review the complete diff directly.
- Names and forward references must not alter numeric capture numbering.
- Rollback, empty/unset captures and K prefixes must keep numbered semantics.
- Validate UTF names by scalar but enforce the length in original bytes.
- Distinguish g-brace backreferences from g-angle/quote subroutine calls.
- Preserve cancellation, capture ceiling and invalid-UTF fragment boundaries.

## Task 1: Pin the contract before implementation

Files: create `tests/Pcre/PcreNamedCaptureReferenceTests.cs` and these design
notes. Test all declaration/reference spellings against the pinned reference,
expected whole spans, every capture, forward/unset paths, name grammar/length,
case-sensitive names under caseless matching, quoting and pending syntax.
Push a draft and confirm feature tests fail because named groups are unsupported.
There is no local .NET SDK; use CI for executable red/green evidence.

## Task 2: Implement parser aliases and resolution

Modify `src/Pcre/PcreByteParser.cs`; create
`src/Pcre/PcreByteParser.NamedCaptures.cs`. Add bounded name parsing, unique
declarations, temporary reference nodes, post-parse numeric resolution, and
explicit bracket/context handling. Reuse `PcreByteGroup` and
`PcreByteBackreference`; leave the matcher and program compiler unchanged.
Remove the obsolete named-group pending row from syntax acceptance tests.
Remove its equivalent row in `PcreGrepConsumerFixtureTests` and update the
consumer fixture inventory.
Update `doc/PCRE-DESIGN.md` and `doc/PCRE-SYNTAX-ACCEPTANCE-MATRIX.md`.

## Task 3: Verify and review

Run `git diff --check`, inspect the whole diff for unintended runtime, capture
numbering, lifetime and consumer changes. Require six successful Staging jobs
on the exact PR head, full suites on all three frameworks, and existing frozen,
workload and package checks. Record red/green evidence in the PR description
and mark ready. The user reviews and merges main.

### Review findings and execution decisions

1. The first native run accepts bracket-literal g/k interpretation; keep this
   pending rather than incorrectly classifying it as invalid named syntax.
2. Pin the non-atomic lookbehind prefix separately and preserve its pending
   diagnostic while recognizing angle-bracket capture names.
3. The invalid-UTF matrix exposed the existing empty-reference offset gap.
   Pin named and numeric `(a?)` references at a continuation byte before changing
   `PcreByteProgram.CanMatchEmptyAtAnyOffset` to include possible empty references.
   This narrowly extends task 2 beyond parser aliases without modifying the VM;
   retain fragment-barrier comparisons and require the entire suite to pass.
