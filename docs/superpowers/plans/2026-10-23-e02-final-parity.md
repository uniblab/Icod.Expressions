# E02 Final Corpus Comparison Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Compare the final A05 corpus against the frozen CommandFramework audit commit and the extracted Expressions assembly in the same three-platform CI run.

**Architecture:** A PowerShell 5.1-compatible script fetches the pinned in-situ audit commit into ignored scratch space, checks source and focused test identities, and runs both test assemblies and JSONL verifiers. The Staging CI matrix invokes it without changing either production tree.

**Tech Stack:** PowerShell 5.1 syntax, Git, .NET 10, xUnit TRX, GitHub Actions.

**Spec:** `doc/Icod.Expressions-A05-Final-Candidate.md` and the approved roadmap, E02 exact extraction parity gate.

## Global Constraints

- CommandFramework production remains intact; fetch its audit commit read-only.
- C# 13 and cmd/sh/PowerShell 5.1-compatible tooling; no Python or C in the library.
- Keep the 35 production blobs, 13 focused test blobs, and 44-case JSONL unchanged.
- Compare 214 focused outcomes and diagnostic/match JSONL in Staging on Windows, macOS, and Linux.

## Review Focus

- A moved old source or test file must make identity verification fail before the test run.
- A filtered old test suite that omits cases must fail the 214-case count.
- A failing or skipped case on either side must fail parity.
- Equal pass counts with different test names must fail the normalized TRX comparison.
- A JSONL change on either side must fail the common frozen-baseline verification.

---

### Task 1: Same-run old/new parity gate

**Files:** Create `tools/FinalParity/run.ps1`; modify `.github/workflows/pull-request.yml`.

**Interfaces:** `run.ps1` has no parameters. It returns zero only when the pinned old commit's source and 13 focused test hashes match the candidate, both 214-case test runs pass with identical normalized test names/outcomes, and both runners verify the frozen JSONL. CI invokes it in the Staging matrix after building Expressions.

- [ ] Write the identity and count checks before the comparison steps; verify they reject a mismatched expected hash/count through a deliberately altered scratch input in CI or a local PowerShell runtime.
- [ ] Fetch audit commit `532850103e69e5d3d09367c64f929ddecaf1fe69` and check its 35 mapped source hashes, 13 focused test hashes, and regex tree `bfe11d5f5ff6a75d75de68573edea6125706b87e`.
- [ ] Run the old focused test filter and the new full test project, require 214 passes each, and compare sorted TRX test names/outcomes.
- [ ] Run the old and new JSONL tools with `--verify` against `doc/Icod.Expressions-A04-Parity-Baseline.jsonl`.
- [ ] Run the Staging CI matrix; inspect each job and correct only the gate, preserving frozen production and corpus identities.

### Task 2: Record the verified E02 boundary

**Files:** Modify `doc/EXTRACTION-READINESS.md`, `doc/Icod.Expressions-A05-Final-Candidate.md`, and PR #1 body.

**Interfaces:** Use the green run from Task 1. State that old CommandFramework is .NET 10-only, the new project also runs on .NET 8/9/10, and both share the 44-case frozen JSONL. Leave strict POSIX/GNU normalization and CommandFramework ownership changes open.

- [ ] Record the exact commit and run evidence, and identify what the comparison does not prove.
- [ ] Re-run `sh tools/verify-a05-candidate.sh`, `git diff --check`, and the final CI before claiming completion.
