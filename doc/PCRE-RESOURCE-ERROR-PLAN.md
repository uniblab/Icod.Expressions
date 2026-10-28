# PCRE Resource and Error Contract Implementation Plan

**Goal:** Freeze a distinct saved-path limit fixture and reconcile internal diagnostics with Grep's observed command contract.

**Architecture:** A focused test exercises many lazy optional choices without a large subject or runtime clock. The contract document maps existing diagnostics to their current tests and identifies the future Grep adapter obligations.

**Tech Stack:** C# 13, .NET 8/9/10, xUnit, six Staging runners.

**Spec:** `doc/PCRE-RESOURCE-ERROR-CONTRACT.md`

## Global constraints

- Internal PCRE API only; no Grep or CommandFramework code changes.
- Keep work exhaustion, saved-path exhaustion, no-match and cancellation distinct.
- Native PCRE2 JIT depth limits are not numerically comparable with internal frames.

## Review focus

- Exactly 1,024 saved lazy paths can succeed and 1,025 return `MatchDepthLimitExceeded`.
- A smaller step budget returns `MatchWorkLimitExceeded` first.
- Cancellation remains an exception before a later depth limit.
- Compile caps do not prevent the intended match-depth fixture from compiling.
- The Grep-facing status mapping is a documented future adapter requirement, not a claim that this PR migrates Grep.

### Task 1: Saved-path fixture and contract ledger

**Files:** `tests/Pcre/PcreResourceContractTests.cs`, `doc/PCRE-RESOURCE-ERROR-CONTRACT.md`, `doc/PCRE-DESIGN.md`, `doc/PCRE-SYNTAX-ACCEPTANCE-MATRIX.md`.

- [x] Add the saved-path boundary, work-limit priority and cancellation fixtures.
- [ ] Run the new tests on Staging; investigate any outcome that differs from the documented provisional policy.
- [x] Link the Grep-facing error contract and outstanding adapter gate from the design documents.
- [ ] Verify all six Staging jobs, frozen source, workload smoke and package-only consumer.
- [ ] Review the final diff and leave a PR for user merge.
