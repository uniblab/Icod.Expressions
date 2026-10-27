# Leading PCRE Option Combinations Implementation Plan

**Goal:** Accept leading combinations and ordered overrides of the currently supported `i`, `m`, `s`, and separate UTF-only `aD` options.

**Architecture:** Scan recognized prefix groups before the existing AST parser. Keep the current pattern-wide option state and byte diagnostics.

**Tech Stack:** C# 13, .NET 8/9/10, xUnit, test-only PCRE.NET 1.6.0.

**Spec:** `doc/PCRE-LEADING-OPTIONS-DESIGN.md`

## Global constraints

- Root project and solution, `src/` source, `tests/` tests, `samples/` samples.
- Internal PCRE API only; no native runtime dependency or consumer migration.
- Keep unsupported mid-pattern/scoped flags explicit and preserve match resource limits.

## Review focus

- A flag set and unset in one group: the unset value wins.
- A caller's caseless option disabled by a leading group.
- Repeated groups with an `aD` prefix before or after the other flags.
- Top-level alternatives inherit leading options.
- Unsupported flags after a valid prefix retain their source-byte diagnostic offset.

### Task 1: Reference tests and parser prefix

**Files:** `tests/Pcre/PcreLeadingOptionCombinationReferenceTests.cs`, four prior unsupported test files, `src/Pcre/PcreByteParser.cs`, `doc/PCRE-DESIGN.md`.

- [ ] Add pinned-reference byte/UTF tests for the review cases, captures and explicit rejection.
- [ ] Run tests and observe failures on currently unsupported prefixes in Staging CI (local .NET SDK is unavailable).
- [ ] Parse repeated leading `i`/`m`/`s` set/unset groups and the existing UTF-only `aD` groups in order.
- [ ] Update superseded unsupported assertions and the feature table.
- [ ] Run six Staging jobs, frozen source, workload smoke and package-only checks; inspect failures and fix regressions.
- [ ] Review the final diff and leave a PR for user merge.
