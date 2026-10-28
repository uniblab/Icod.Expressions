# Non-newline escape implementation plan

**Spec:** `doc/PCRE-NON-NEWLINE-DESIGN.md`

1. Add `tests/Pcre/PcreNonNewlineReferenceTests.cs`: all 256 bytes, Unicode probes, independent newline/DotAll/UCP/BSR settings, counted forms, codepoint literals/ranges/assertions, captures, every offset around CRLF/invalid UTF, diagnostics and resource controls. Replace obsolete `\N` rejection assertions with still-pending escapes. Verify the expected missing-syntax failures in Staging CI.
2. Factor the existing dot node factory in `src/Pcre/PcreByteParser.cs` and reuse it with DotAll off. Disambiguate `U+` codepoints from counted repetitions, reuse braced-hex validation, and reject bare bracket forms. No new VM instruction or runtime dependency.
3. Update `doc/PCRE-DESIGN.md` and `doc/PCRE-SYNTAX-ACCEPTANCE-MATRIX.md`; review the complete branch and run all six Staging OS/architecture jobs, including net8/net9/net10 tests, frozen corpus, workload smoke and package-only consumers. Record evidence in the PR.

No subagents. Keep Grep and CommandFramework unchanged. The local .NET SDK is unavailable; executable verification runs in CI.
