# PR CI duration design

## Intent

Shorten PR feedback without losing six native Linux/Windows/macOS x64/ARM64 build and test lanes or the frozen 214-case GNU corpus. Recent PR #13 ran 465 seconds on macOS x64 and 341 seconds on Linux x64, while ARM64 lanes finished in 60–106 seconds. The source-pinned consumer bridge cost 105–225 seconds per x64 host, and the second Linux bridge repeated most of its suites for another 70 seconds.

## Approaches

- Keep all checks on every PR: simplest but repeats original consumer builds and tests across three x64 hosts even when only internal PCRE code changes.
- Cache immutable consumer outputs: can reduce compilation but requires cache lifecycle, host and SDK keys, and careful separation from the fresh candidate package.
- Restrict the expensive source-pinned compatibility checks to PRs that may affect the GNU/public path, run them in independent x64 jobs alongside the six core jobs, and remove the narrower duplicate Linux bridge invocation. Choose this approach. Default to full checks on any uncertain or non-PCRE file, including project, packaging and workflow changes. The compatibility jobs create their own candidate package, avoiding an artifact dependency on the core jobs.

## Behavior and boundaries

Classify PR file paths from a full-history checkout against the event's base SHA using `git diff --name-only --no-renames`. Only `src/Pcre/**`, `tests/Pcre/**`, and `doc/PCRE-*.md` qualify for the short path. Empty lists, any other path, and diff failures take the full path or fail the classifier job. Run pinned Gnulib, final source parity, forwarding smoke and cross-platform consumer bridges in three independent x64 compatibility jobs only for full-path PRs. They overlap the six core jobs and install only the .NET 10 SDK required for the source-pinned consumer suites. Each compatibility runner builds the solution and the parity tool before their `--no-build` checks. The six-lane solution restore/build/full and frozen tests, byte parity, API snapshot, Linux x64 package creation and package-only consumer start independently of classification. The Linux shell bridge's original and bridged suites are covered by the broader cross-platform PowerShell probe; keep its script available for manual use.

Main and tag workflows retain their Release configuration and six jobs. The tag publishing guard still depends on exactly six successful main jobs; the extra PR classification job does not change that gate. No consumer or CommandFramework production code changes.

## Verification

Test classification with internal-only, GNU/public, workflow, mixed, renamed/deleted, and empty path lists. The optimization PR itself changes workflow/scripts, so all three pinned compatibility jobs must run in its hosted CI alongside all six core jobs. Compare the critical path against the measured baseline. The next internal-only PCRE PR will be the hosted short-path measurement.
