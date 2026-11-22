# Subject bounds in interpreted variable lookbehind

With per-call EndAnchored, PCRE2 matches `(?<=a{1,2}(?=(b)))b` against `ab`
at byte 1 and captures the trailing `b`. The nested lookahead can read that
byte even though the lookbehind must finish immediately before it. Using
the saved position as the subject end prevents the lookahead from seeing
the byte and also changes end assertions such as `\z`.

The matcher now uses real subject and valid UTF-8 fragment bounds inside
interpreted variable lookbehind. It compares the final position of each
ranged branch with the position saved on entry to the assertion. This
also lets ordinary greedy repeats backtrack to that position. A possessive
repeat that overruns it causes the branch to fail. The matcher uses the
saved position from the assertion scope and the shared work counter,
cancellation token, and capture journal.

The [native oracle record](../tests/Pcre/Native/interpreted-lookbehind-end-oracle.md)
describes 48 definitions and 3,072 match-policy outcomes. They cover
atomic, non-atomic, negative, and conditional assertions, nested
lookarounds, word and end checks, UTF byte offsets, and malformed
fragments. The thirty-first package smoke record runs the same outcomes.
Two resource cases exercise work limits and cancellation after a nested
lookahead reads beyond the saved position.

## Tests-only checkpoint

Commit `187cd9b58c4a6676071b4aaa1ad809b58c3f9790` and tree `45c0e2b8942153f70392c0170434371c644dd53c`
produced 45 failures, 107,572 passes, and zero skips on
[Linux x64](https://github.com/uniblab/Icod.Expressions/actions/runs/44982356942/job/137085408290).
Definitions 1–43 passed all native assertions before failing the managed
comparison. Both resource cases reproduced the missing match. All
107,567 prior tests and five new controls passed.

- `samples/PackageSmoke/PcreInterpretedLookbehindEndCases.cs` at `264e58740b569175dcffee60c1a039cb8e49c79f`
- `tests/Icod.Expressions.Tests.csproj` at `2e87e9657cea5d46c64421b84c4d666b5cbc0122`
- `tests/Pcre/PcreInterpretedLookbehindEndTests.cs` at `be6dbe84ef2f0f79c20661bda12d211ab3173ea0`

Final qualification is recorded on [PR #314](https://github.com/uniblab/Icod.Expressions/pull/314).
