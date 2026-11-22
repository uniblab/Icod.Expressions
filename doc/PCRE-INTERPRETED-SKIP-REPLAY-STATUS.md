# Ordered replay for interpreted named SKIP

With subject `ac`, pattern `(*MARK:A)a(*SKIP:A)(?:c(*SKIP:missing)x|x)|ac`,
and per-call `EndAnchored`, native PCRE2 matches the fallback at offset zero.
The failed unresolved SKIP makes the driver retry the candidate with both
named SKIP visits suppressed. The managed matcher previously ignored only
the unresolved name and unwound the earlier resolved SKIP before reaching
the fallback.

The interpreter counts named SKIP visits in execution order. An unresolved
root verb saves a replay frame, which triggers only if failure reaches it.
The driver then uses the full visit count as the suppression prefix. Each
retry initializes candidate captures, labels, and scopes and shares the
original work counter, cancellation token, and bounded backtracking stack.
A forward SKIP carries the prefix to its target. Ordinary bumpalong clears
it. Anchoring and the first-line and CRLF checks govern retry admission.

The [native oracle record](../tests/Pcre/Native/interpreted-skip-replay-oracle.md)
describes 48 definitions and 3,072 outcomes. They cover repeated retries,
capture and label rollback, nested assertions, callable groups, forward
SKIP, original byte offsets, malformed UTF fragments, and `FIRSTLINE`.
Two resource cases check the shared work budget and cancellation during a
match that requires multiple retries. A twenty-eighth package smoke record
exercises the same outcomes against the packed library.

## Tests-only checkpoint

Commit `4b5f85a69d35cfc0108dcbc6b826636c860bc36d` and tree `52a8b2752f41028ec6b86e44218f0edb0cf42516`
produced 43 failures, 107,424 passes, and zero skips on
[Windows x64](https://github.com/uniblab/Icod.Expressions/actions/runs/44968101979/job/137041595906) and
[macOS ARM64](https://github.com/uniblab/Icod.Expressions/actions/runs/44968101979/job/137041595963).
Definitions 1–41 passed their native assertions before failing the managed
comparison. The two resource cases reproduced the missing fallback match.
All 107,417 existing tests and seven new controls passed.

- `samples/PackageSmoke/PcreInterpretedSkipReplayCases.cs` at `57ac8ab475260c6e30e2685b10ee124aa5bd9ed2`
- `tests/Icod.Expressions.Tests.csproj` at `47d0caea0cf044d256a44b8e79e88d1961c20934`
- `tests/Pcre/PcreInterpretedSkipReplayTests.cs` at `bd576d258c9360dea862d90e0abdfcb977bb9f13`

Final qualification is recorded on [PR #311](https://github.com/uniblab/Icod.Expressions/pull/311).
