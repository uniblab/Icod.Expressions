# PCRE2 reference capture parity plan

**Goal:** compare supported numbered capture behavior with Grep's pinned PCRE.NET reference.

**Files:** add `tests/Pcre/PcreReferenceCaptureTests.cs`; update `doc/PCRE-DESIGN.md`; keep the production project untouched unless a demonstrated mismatch requires a focused fix and regression test.

## Task 1: capture oracle

- Use the same byte/UTF/UCP/ASCII-\\d/invalid-UTF options as the existing reference matrix.
- For each explicit fixture, assert native match and `Groups.Count`, then every group's `Success`, byte `Index` and `Length`. Compare the internal group's nullable byte span to the same expected value.
- Include unset versus empty, rollback, repetition, atomic, byte and UTF cases, and source-byte start offsets.

## Task 2: verify

- Push a draft PR and run Staging on six OS/architecture targets and .NET 8/9/10, including the frozen 214-case corpus and package-only smoke.
- If a fixture differs, check the pinned wrapper semantics and options before changing production behavior. Self-review the final diff and exact CI head. No subagents per user preference.
