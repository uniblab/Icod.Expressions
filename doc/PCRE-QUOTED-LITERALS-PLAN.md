# PCRE literal quoting implementation plan

**Goal:** Implement the quoted literal subset against the pinned reference, without a new VM instruction.

**Files:** `src/Pcre/PcreByteParser.cs` owns the lexical state; `tests/Pcre/PcreQuotedLiteralTests.cs` owns targeted checks; `tests/Pcre/PcreReferenceConformanceTests.cs` owns native comparisons; `tests/Pcre/PcreSyntaxAcceptanceTests.cs` removes the now-supported rejection; the PCRE design and syntax matrix state the boundary.

- [x] Add reference and targeted byte/UTF, bracket, quantifier, assertion, and diagnostic tests.
- [x] Verify the expected test failures with a test-only Staging CI run (Linux x64, run 40617597496: 23 expected failures in net8.0).
- [x] Add quoting to ordinary sequence parsing, including transparent closing delimiters before quantifiers.
- [x] Add quoted literal members to byte and UTF brackets, and fixed literal assertions.
- [x] Pin range endpoints across quote boundaries (Linux ARM64 red run 40618128673: 7 expected net8.0 failures) and let quoted literals bound ordinary bracket ranges.
- [ ] Update the syntax inventory and verify all six Staging jobs on the final head.

Use the existing isolated worktree and test-first CI workflow. No subagents.
