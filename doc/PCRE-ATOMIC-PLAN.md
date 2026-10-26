# Internal PCRE atomic control plan

**Goal:** add bounded `(?>...)` and possessive quantifiers to the internal byte matcher.

**Files:** `tests/Pcre/PcreAtomicTests.cs` tests semantics; `src/Pcre/PcreByteSyntax.cs` represents atomic sections; `src/Pcre/PcreByteParser.cs` recognizes syntax; `src/Pcre/PcreByteProgram.cs` emits enter/exit instructions; `src/Pcre/PcreByteMatcher.cs` prunes and restores backtrack state; `doc/PCRE-DESIGN.md` records status.

## Task 1: establish behavioral cases

- Add tests for atomic first-path selection versus ordinary backtracking, nested boundaries, captures, preceding alternatives, byte and UTF offsets, and possessive `*+`, `++`, `?+`, counted forms.
- Verify tests fail against the current internal implementation for `UnsupportedSyntax`, using hosted Staging because this workspace has no .NET SDK.

## Task 2: implement atomic execution

- Parse `(?>...)` as noncapturing atomic body and wrap greedy possessive repeats in an atomic node. Keep `?+` as a suffix, not as lazy `?`. Reject lazy/possessive mixtures and repeated quantifiers.
- Compile atomic enter/exit instructions. On exit, discard only frames created inside that section. A saved alternative restores the active boundary stack with captures, starts and marks.
- Retain the existing work and depth limits and cancellation per instruction.

## Task 3: verify and review

- Run focused tests and full suite in hosted CI on all six RIDs, net8/net9/net10, frozen corpus and package smoke. Compare selected cases against pinned PCRE.NET.
- Self-review the full diff and exact final PR head, then mark ready when all jobs pass. No subagents per user preference.
