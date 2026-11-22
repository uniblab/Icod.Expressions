# Named control verb prefixes and unmatched named SKIP

With subject `ac`, pattern `(*MARK:entry)\A(?:a(*SKIP:missing)b|ac)`,
and per-call `EndAnchored`, native interpreted matching reports no match.
The managed source-anchor scan previously stopped at MARK and allowed the
fallback branch to match after the unresolved named SKIP.

The compiler now advances past MARK and named COMMIT, PRUNE, SKIP, and THEN
while it checks each leading branch for a start anchor. The cut nodes that
also represent unnamed verbs qualify when their stored name is nonempty.
This includes a name beginning with NUL, whose visible label is empty.
The scan checks cancellation and charges each visited node under the existing
compile ceiling. The resulting proof feeds the interpreted unmatched-name
check. Verb instructions and label handling are applied during matching.

The [native oracle record](../tests/Pcre/Native/verb-prefix-anchor-skip-oracle.md)
describes 48 definitions and 3,072 outcomes. It covers every supported named
prefix form, assertion bodies, source alternatives, preserved labels, and
original byte offsets. Controls cover empty arguments, mixed anchor paths,
and a named SKIP on a successful continuation. Two resource cases exercise
exact work limits and cancellation. A twenty-seventh package smoke record
verifies the same outcomes through the packed library.

## Tests-only checkpoint

Commit `7ef70f2cf0eee81039403870c57593e6b4876ec1` and tree `e8d32910e9aa755ad0a740a75903e9f004a8d2ee`
produced 43 failures, 107,374 passes, and zero skips on
[Linux ARM64](https://github.com/uniblab/Icod.Expressions/actions/runs/44964863340/job/137031482202),
[Linux x64](https://github.com/uniblab/Icod.Expressions/actions/runs/44964863340/job/137031482297), and
[Windows x64](https://github.com/uniblab/Icod.Expressions/actions/runs/44964863340/job/137031482132).
Definitions 1–41 passed all native assertions before the managed comparison
failed. Both resource cases reproduced the unexpected fallback match. All
107,367 existing tests and seven new controls passed.

- `samples/PackageSmoke/PcreVerbPrefixAnchorSkipCases.cs` at `6dafb7035ed7384eeaca7d328e78cdd5f2547026`
- `tests/Icod.Expressions.Tests.csproj` at `1caa438b4f9cd8bd566c30a065260906aa5bd798`
- `tests/Pcre/PcreVerbPrefixAnchorSkipTests.cs` at `fae18ead08e47d492660c1cfb2d232b11bd00bb3`

Final qualification is recorded on [PR #310](https://github.com/uniblab/Icod.Expressions/pull/310).
