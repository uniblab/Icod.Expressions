# Match-position implementation plan

**Spec:** `doc/PCRE-MATCH-POSITIONS-DESIGN.md`

1. Add `tests/Pcre/PcreMatchPositionReferenceTests.cs`: explicit native/managed spans and captures, requested-offset anchoring, reset rollback through branches/atomic/repeat paths, every malformed-UTF/CRLF offset, strict UTF diagnostics, invalid brackets/quantifiers/assertions and work/cancellation gates. Remove obsolete `\G`/`\K` pending-syntax cases and confirm expected test-first CI failures.
2. Add nodes in `src/Pcre/PcreByteSyntax.cs` and parser handling/diagnostics in `PcreByteParser.cs`. Add search-start lowering in `PcreByteProgram.cs`; lower resets to capture-start slot zero. Teach empty-path metadata about the new nodes. Initialize and report slot zero in `PcreByteMatcher.cs`; compare `\G` with the original match offset. Preserve existing journal/frame types.
3. Update the PCRE design and syntax matrix, review the full branch, and run all six Staging jobs with net8/net9/net10 suites, frozen corpus, workload smoke and package-only consumers. Record evidence in the PR.

No subagents. No Grep or CommandFramework changes. The local .NET SDK is unavailable; executable verification runs in CI.
