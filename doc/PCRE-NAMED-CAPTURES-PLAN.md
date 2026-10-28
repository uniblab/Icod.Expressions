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
Update `doc/PCRE-DESIGN.md` and `doc/PCRE-SYNTAX-ACCEPTANCE-MATRIX.md`.

## Task 3: Verify and review

Run `git diff --check`, inspect the whole diff for unintended runtime, capture
numbering, lifetime and consumer changes. Require six successful Staging jobs
on the exact PR head, full suites on all three frameworks, and existing frozen,
workload and package checks. Record red/green evidence in the PR description
and mark ready. The user reviews and merges main.
