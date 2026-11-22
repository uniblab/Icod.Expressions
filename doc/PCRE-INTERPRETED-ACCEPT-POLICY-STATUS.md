# Empty-match restrictions and interpreted ACCEPT

With per-call EndAnchored and NotEmpty, PCRE2 matches `(*ACCEPT)|ab`
against `ab` at byte 0 with length 2. The interpreter rejects the empty
acceptance and tries the second alternative. The managed matcher
previously discarded the alternative during the end-anchored check.

Root ACCEPT now uses the common acceptance checks. If the interpreter
disallows an empty result, the match backtracks with its saved frames.
A result that passes the empty-match check and ends short under EndAnchored
still abandons the candidate. The JIT applies the end check first when
EndAnchored was set at compile time. Empty-match restrictions use the
reported match start, which may have moved through `\K`. Ordinary retry
restores captures, markers, and the reported start through the journal.

The [native oracle record](../tests/Pcre/Native/interpreted-accept-policy-oracle.md)
covers 48 definitions and 3,072 outcomes, including named ACCEPT,
synthetic capture closes, compiled and per-call policies, lookbehind,
UTF-8 fragments, and assertion and subroutine controls. Two resource
tests check the shared work limit and cancellation on the fallback path.
The thirty-second package smoke record runs the same catalog.

## Tests-only checkpoint

Commit `011a4f6e3bdfb47702e4f21cafa61b9a5fa413f0` and tree `c224dbd66a382687b39761969dda9ca536e0fa05`
produced 45 failures, 107,622 passes, and zero skips on
[Windows x64](https://github.com/uniblab/Icod.Expressions/actions/runs/44991603187/job/137113661337).
Definitions 1–43 passed their native assertions before failing the
managed comparison. Both resource cases reproduced the missing fallback.
All 107,617 prior tests and five new controls passed.

- `samples/PackageSmoke/PcreInterpretedAcceptPolicyCases.cs` at `fae3d48f5f1529d1b6d071018d9db75ab02ca391`
- `tests/Icod.Expressions.Tests.csproj` at `6d0e2c68136209a734edce7b30c70673d8167476`
- `tests/Pcre/PcreInterpretedAcceptPolicyTests.cs` at `aa5bf12e5eeb0326a51c4d2c03b041ae718b6977`

Final qualification is recorded on [PR #315](https://github.com/uniblab/Icod.Expressions/pull/315).
