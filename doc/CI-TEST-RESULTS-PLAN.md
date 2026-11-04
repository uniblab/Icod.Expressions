# CI test result implementation plan

Execute inline; no subagents. Spec: CI-TEST-RESULTS-DESIGN.md.

- [ ] Add shared Invoke-Tests.ps1 and VerifyTestResults.ps1, with a generic JSON contract and synthetic PowerShell tests for valid, missing, malformed, duplicate, stale, skipped/failed, extra-framework and wrong-identity results, plus command failure and concurrency arguments.
- [ ] Pin Expressions frozen120-method/214-case manifest from unchanged legacy sources. Replace the three serial full/frozen workflow loops with one shared runner invocation, preserving configurations and six native matrices. Keep Release package production/publication dependencies intact.
- [ ] Update both template PR workflow copies and VerifyDistribution.ps1 to call the shared runner; document opt-in contracts. Add platform-independent script-test CI and identical tests/scripts to both PRs.
- [ ] Parse YAML, inspect full diffs, verify35frozen blobs and unchanged test/source files. Publish coordinated draft PRs, qualify all native Expressions jobs and all template script tests, compare timings, then leave ready/unmerged PRs for user review.
