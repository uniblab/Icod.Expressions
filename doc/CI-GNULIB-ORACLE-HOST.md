# Preserve the pinned Gnulib oracle host

Main run 44082512644 after merged #268 passed all 98,546 .NET tests on every
native host. Linux x64 then failed in the pinned included-regex oracle:
same-inode.h:88 calls S_TYPEISTMO, unavailable on the new ubuntu-26.04 image.
The identical runner passed on ubuntu-24.04 in main 43978970399, Linux x64
job 133690169965. The new failing job is 134036423412. No engine regression.

## Design

Run the frozen oracle in a dedicated `Pinned Gnulib oracle` job on
ubuntu-24.04. Main always runs it; PRs use the existing full_compatibility
classification. Remove only its former steps from the ubuntu-latest native
main job and PR Compatibility matrix. Keep all six .NET hosts, all framework
builds/net10-only tests and the remaining support checks unchanged.

Changing the Gnulib revision, patching its sources or suppressing compiler
errors would alter the frozen reference environment. Pin only this oracle's
host instead; current .NET Linux coverage remains on ubuntu-latest. Preserve
tools/GnulibRegexOracle/run.sh blob 7511107d8c12d1e98b53c88311c2d44ae0c0c387,
upstream revision fb7312fa8d3df29f0ca0678f669b9a5b88a078ec and
test-regex.c blob 0ab97819d2cdf3b29f5f4b989d1c33e0bd157f36.

The main completion contract now requires eight successful jobs: the previous
six native suites plus Intel aggregate, and the dedicated oracle. Extend the
actual JSON gate tests to reject a missing, renamed, failed, skipped or still
running oracle, alongside all existing native/aggregate rejection cases.

## Implementation and verification plan

- Write the eight-job completion fixture/rejection cases first; observe the
  legacy seven-job jq guard rejecting the otherwise valid new fixture.
- Add the two workflow jobs and update the exact-name/status/count jq guard.
- Run the completion tests, classifier/frozen source/corpus/A05/whitespace
  gates. Review the whole diff inline under standing no-subagent direction.
- Open a PR and require the actual Ubuntu24.04 oracle's original and strict
  capture checks, all three source-pinned Compatibility jobs, six full native
  98,546-test suites/all three library targets and configured support gates.
- Record final source/tree/proposed-merge and CI evidence externally; leave
  merge/publication to the user. Engine/test C# and frozen oracle stay identical.

The prepared mixed UTF Any increment is preserved separately and unpublished
until this main-branch CI repair is merged. #7/#102 stay open; Grep last.

## Prequalification evidence

The revised completion fixture was observed RED against the old seven-job jq
guard (exit 1 on an otherwise successful eight-job record). After extending
the guard, the actual test script passes the valid eight-job record and all
13 rejection cases. Frozen source/corpus/A05 identities, classifier and
whitespace checks pass. All engine and C# test files and the frozen oracle
runner remain unchanged. The only existing test edit is the shell completion
contract test.

Inline author review checks main's unconditional oracle, PR's existing full
compatibility condition, unchanged six-host .NET coverage, exact release
prerequisite names/status/count and preservation of the frozen oracle. No
outstanding finding. Actual host/oracle/compatibility/full-suite qualification
and exact commit/tree identities will be recorded on the PR before ready.
