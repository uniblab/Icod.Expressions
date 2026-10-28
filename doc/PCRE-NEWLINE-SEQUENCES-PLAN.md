# Newline sequence implementation plan

**Spec:** `doc/PCRE-NEWLINE-SEQUENCES-DESIGN.md`

1. Reserve the internal `BsrAnyCrLf` option without wiring behavior; add `tests/Pcre/PcreNewlineSequenceReferenceTests.cs` to compile against that interface and pin the missing matcher behavior. Update the obsolete `\R` rejection cases to still-pending escapes. Confirm test-first Staging failures are caused by unsupported `\R`.
2. Add the semantic node in `src/Pcre/PcreByteSyntax.cs`, parse it in `PcreByteParser.cs` with explicit bracket rejection, and pass BSR configuration from `PcreByteCompiler.cs` to `PcreByteProgramCompiler`. Lower it through existing atomic, literal and class nodes. Preserve the distinction between implicit and explicit newlines in compiler metadata.
3. Update the syntax matrix and overall PCRE design. Review the complete branch, run all six Staging jobs across net8/net9/net10, and record test counts, frozen corpus, workload smoke and package-only consumer results in the PR.

No subagents. Existing root project/solution, `/src` and `/tests` layout remains. Grep and CommandFramework remain unchanged. The local .NET SDK is unavailable, so executable verification runs in CI.
