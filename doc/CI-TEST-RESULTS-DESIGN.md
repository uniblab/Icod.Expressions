# Concurrent CI tests and frozen result validation

Approved direction: keep six native OS/architecture jobs and net8/9/10 coverage; remove duplicate frozen execution and run target frameworks concurrently. Local Debug, PR Staging, main/tag Release and single-library packaging stay configured.

Shared packaging scripts run one SDK multi-target test invocation with explicit TestTfmsInParallel=true and framework-specific TRX names. A repository contract enables strict verification in a fresh unique directory. Expressions pins three frameworks, a full-suite floor25373, and all120 frozen method identities with their214 case counts. Result IDs join definitions to passed results; duplicate/missing/failed/skipped/malformed/extra-framework results fail. No tests or library code change. The template has no Expressions namespace/count assumptions and preserves no-test solutions when no contract is supplied.

Use synthetic TRX and a fake dotnet command to test failure propagation, freshness, identities, counts, and concurrency arguments on Windows/Linux/macOS. Run Expressions real six-platform CI and compare elapsed times with PR158 run512. Both repositories receive identical shared scripts. User merges both PRs; no publication.
