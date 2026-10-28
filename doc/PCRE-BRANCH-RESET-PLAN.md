# Internal PCRE branch-reset implementation plan

**Goal:** Support shared capture numbering inside `(?|...)` using existing VM slots.

**Architecture:** Parser-local numbering scopes and bidirectional name aliases;
existing noncapturing/group/backreference nodes and journaled matching.

**Tech stack:** C# 13, net8.0/net9.0/net10.0, test-only PCRE.NET 1.6.0.

**Spec:** [PCRE-BRANCH-RESET-DESIGN.md](PCRE-BRANCH-RESET-DESIGN.md).

## Constraints and review focus

- Root project/solution, `/src`, `/tests`, `/samples`; local Debug, PR Staging.
- No public API, package, production dependency, consumer or matcher changes.
- No subagents; implement and review directly.
- An earlier wider branch must not inflate the local counter in a later branch.
- The widest branch, including nested resets, determines following numbers.
- Same-name/same-number aliases are valid; conflicting identities are invalid.
- Shorter repeated branches preserve captures not overwritten on that path.
- Shared numbering cannot bypass depth, expansion, work or capture limits.

## Task 1: Reference contract and expected failures

Create `tests/Pcre/PcreBranchResetReferenceTests.cs` and these notes. Pin whole
spans and all capture slots for the syntax, numbering and rollback cases in the
spec. Check valid and invalid names against the native compiler. Include
128/256-slot boundaries, nested depth, expansion limits and match cancellation.
Open a draft and run CI before implementation; there is no local .NET SDK.

## Task 2: Parser numbering and alias registration

Modify `src/Pcre/PcreByteParser.cs`: add an optional branch-reset flag to
`ParseAlternatives(bool inGroup, int depth, bool branchReset = false)`, remember
the entry counter and highest branch result, reset only after a direct branch,
restore the maximum at group end. Recognize `(?|` as noncapturing and pass the
flag to that group's parser call. Relative references continue using groupCount.

Modify `src/Pcre/PcreByteParser.NamedCaptures.cs`: add a reverse numbered-name
map and `RegisterCaptureName(string name, int number, int offset)`. Permit
repeated identical aliases; reject name-to-number or number-to-name conflicts
at the conflicting group's opening byte. Reuse numbered nodes and existing
named-reference resolution; do not add runtime state.

Remove obsolete pending rows from `PcreByteNoncapturingTests`,
`PcreSyntaxAcceptanceTests`, `PcreGrepConsumerFixtureTests` and
`PcreNamedCaptureReferenceTests`. Update the overall design, acceptance matrix,
consumer fixture inventory and the named-capture design's current boundary.

## Task 3: Review and final verification

Run `git diff --check` and the frozen source verifier; inspect the full diff
against the review focus. Require six green Staging jobs on the exact PR head,
full framework suites and existing workload/package checks. Record failing and
passing CI evidence in the PR description and mark ready for the user's review.

## Execution finding: native compiler stack exhaustion

The first implemented run reproduced a process-level stack overflow on macOS
ARM64 net8 in the new positive depth-boundary test. The stack trace identifies
recursive `PcreByteProgramCompiler.CompileNode` / `CompileAlternatives` calls.
Both Linux jobs passed all 1,755 tests; this is a platform stack-budget bug, not
a reason to remove or relax the fixture.

Extend task 2 to `src/Pcre/PcreByteProgram.cs`: drain an explicit compilation
continuation stack, schedule alternatives/sequences and repeat copies lazily,
then patch control flow in finishing continuations. Preserve emitted order,
compile visits/marks, counted diagnostic offsets and cancellation. Keep matcher
frames and instructions unchanged. Extend the regression file with 256 nested
capture/atomic/optional/counted groups and deep named resolution. Require the
same six-platform suite after the compiler fix.
