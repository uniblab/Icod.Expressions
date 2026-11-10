# CI framework isolation implementation plan

Goal: shorten Intel CI without losing checks. Spec: CI-TEST-DISCOVERY-DESIGN.md.
Execute inline; no subagents.

1. Add PowerShell behavior checks for selected-framework validation, rejection
   outside the contract, combined-run completeness, optional runner framework/
   result paths and refusal to reuse results. Observe script-test CI RED.
2. Add backward-compatible Framework/ResultsDirectory runner parameters and
   contract-checked Framework in the validator. Preserve default behavior.
3. Add shared Intel composite action; PR/main matrices run three framework jobs.
   Keep five original native jobs and macOS x64 aggregate status. Apply original
   full contract to combined TRX. Update exact main release gating for nine names.
4. Require three-OS script tests, all native suites/frozen214, extraction parity,
   all configured compatibility/package/API/workload gates and frozen35 sources.
   Compare cold Intel timings with baseline.
5. Review defaults, freshness, failure propagation, artifacts and release gate;
   verify exact remote tree and leave ready PR for user review/merge.
