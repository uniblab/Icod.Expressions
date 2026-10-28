# PCRE Syntax Acceptance Matrix Implementation Plan

**Goal:** Record the supported PCRE subset and reject representative pending syntax at its opening byte.

**Architecture:** Keep a behavior inventory linked to existing conformance tests. Add a focused compiler diagnostic matrix and classify `(*...)` before the ordinary group parser treats its `*` as a quantifier.

**Tech Stack:** C# 13, .NET 8/9/10, xUnit, pinned PCRE.NET 1.6.0 test oracle.

**Spec:** `doc/PCRE-SYNTAX-ACCEPTANCE-MATRIX.md`

## Global constraints

- Internal PCRE path only; preserve the GNU/BRE/ERE frozen source and behavior.
- Keep valid pending syntax distinct from malformed syntax when the family is recognizable.
- No PCRE.NET runtime dependency or consumer migration.

## Review focus

- A leading or nonzero-offset `(*...)` item returns `UnsupportedSyntax` at the opening parenthesis.
- A prior leading `(?i)` does not shift the `(*...)` diagnostic offset.
- Existing malformed group diagnostics remain `InvalidSyntax`.
- Unsupported escapes and group families do not become literals or successful patterns.
- The inventory describes a finite observed consumer floor, not complete PCRE2 acceptance.

### Task 1: Pin syntax boundary and update classifier

**Files:** `tests/Pcre/PcreSyntaxAcceptanceTests.cs`, `src/Pcre/PcreByteParser.cs`, `doc/PCRE-DESIGN.md`.

- [x] Add the representative valid-pending syntax diagnostic table and malformed-group control.
- [x] Run the new tests and observe the `(*...)` mismatch before changing the parser: Staging net8.0 reported 11 directive cases as `InvalidSyntax` where `UnsupportedSyntax` was expected; the pinned reference accepted `(*FAIL)|a` after correcting an initially wrong `(*SKIP)` fixture.
- [x] Classify the `(*...)` family at its opening parenthesis, retaining existing diagnostics for other groups.
- [x] Add the acceptance-matrix link to the roadmap and check the document against test evidence.
- [ ] Run the Staging six-job suite, frozen corpus, workload smoke and package-only consumer.
- [ ] Review the final diff and leave a PR for user merge.
