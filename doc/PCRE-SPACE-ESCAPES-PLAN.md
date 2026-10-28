# Whitespace escape implementation plan

**Goal:** Add the four fixed horizontal/vertical whitespace classes without a new VM opcode.

**Spec:** `doc/PCRE-SPACE-ESCAPES-DESIGN.md`

1. Add `tests/Pcre/PcreSpaceEscapeReferenceTests.cs`: exhaustive byte membership and complements, Unicode positive/boundary probes with and without UCP/caseless, bracket unions and invalid ranges, captures/repetition, CRLF offsets, invalid UTF, work limits and cancellation. Verify the expected unsupported-escape failures in test-first Staging CI.
2. Add fixed shared predicates in `src/Pcre/PcreSpaceCharacters.cs`. Add byte sets in `PcreByteSet.cs` and scalar kinds in `PcreByteSyntax.cs`; lower escapes in `PcreByteParser.cs`, and evaluate the scalar kinds in `PcreByteMatcher.cs`. Reuse the horizontal predicate for UCP POSIX `blank`. Preserve current VM work charges and cancellation checks.
3. Update `doc/PCRE-DESIGN.md` and `doc/PCRE-SYNTAX-ACCEPTANCE-MATRIX.md`, review the complete branch, and run the six Staging OS/architecture jobs, including all three target frameworks and package-only consumers. Record CI evidence in the PR.

No subagents. Keep Grep and CommandFramework production paths unchanged. Local .NET SDK is unavailable in this workspace; CI is the executable verification gate.
