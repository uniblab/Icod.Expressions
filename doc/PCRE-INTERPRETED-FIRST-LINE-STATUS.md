# Interpreted FIRSTLINE after fragment and SKIP restarts

With tolerant UTF-8, FIRSTLINE, and per-call EndAnchored, PCRE2 matches `Y`
at byte 4 of subject hex `610AFF6259`. The malformed byte starts a new
first-line search in the next valid fragment.
With FIRSTLINE and per-call EndAnchored, `a(?s:.)(*SKIP)(*F)|Y` matches at
byte 3 in `a\nbY`. The next first-line scan starts at the admitted SKIP
target, and a failed attempt there can advance to `Y`.

The matcher's first-line scan now begins at each interpreted UTF fragment's
search offset and uses the fragment end as its bound. Finding a newline
ends candidate search for that fragment, after permitting an attempt at the
newline. After a forward SKIP passes the failed-candidate check and CRLF
adjustment, the scan starts at its adjusted target. The two restart paths
share the work counter and cancellation token with the initial attempt.

The [native oracle record](../tests/Pcre/Native/interpreted-first-line-oracle.md)
describes 48 definitions covering 3,072 match-policy outcomes. The cases
include multiple malformed fragments, newline conventions, plain and named
SKIP, captures, nonzero offsets, and per-call and compiled anchoring.
The same catalog supplies the thirtieth package smoke record. Two resource
cases check the exact work budget and cancellation after both restart types.

## Tests-only checkpoint

Commit `c242384df9dcd4a0f8d43a8ee2f92bad4460bdea` and tree `5c0ae771cfedd535b34579a5c689336cfe7b655e`
produced 45 failures, 107,522 passes, and zero skips on
[Windows x64](https://github.com/uniblab/Icod.Expressions/actions/runs/44979787088/job/137077549051).
Definitions 1–43 passed their native assertions before failing the managed
comparison. Both resource cases reproduced the missing match at byte 8.
All 107,517 prior tests and five new controls passed.

- `samples/PackageSmoke/PcreInterpretedFirstLineCases.cs` at `ac9163a5972150b6449cf7b96f779dae1003eee9`
- `tests/Icod.Expressions.Tests.csproj` at `8b54fd7d4110441422d62e33f20d50e64e18d74a`
- `tests/Pcre/PcreInterpretedFirstLineTests.cs` at `c0f99eab61738610041ace29dc6a70197b9fc7df`

Final qualification is recorded on [PR #313](https://github.com/uniblab/Icod.Expressions/pull/313).
