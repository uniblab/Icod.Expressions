# Mandatory capture prefixes and source-anchor SKIP

[PR #301](https://github.com/uniblab/Icod.Expressions/pull/301) recognizes
source start anchors behind a straight mandatory chain of capture entries
when deciding whether an interpreted unmatched named SKIP can replay.
Native matching infers anchoring through these capturing groups. Looking only
at the first emitted instruction misses that constraint.

The compiler records private immutable evidence by walking leading numbered CaptureStart
instructions, then checking for Start or SearchStart. The emitted program's
existing instruction ceiling bounds the walk, with cancellation checked at
each visited instruction. Choices, optional paths, consuming instructions,
multiline StartLine, and group-zero match-start resets decline the proof. The evidence is used only by the
existing unmatched-name stop check. General candidate selection and other
optimizers retain their current policies.

The pinned native corpus contains 32 definitions by all 64 call masks,
for 2,048 outcomes. It covers ordinary/named/nested captures, empty anchor
captures, supplementary values, and optional, mixed-alternative, consuming,
multiline, known-mark, and successful-forward controls. Two focused tests
exercise exact work exhaustion and cancellation. The suite reaches 106,980
tests. An eighteenth package record checks all 2,048 protocols through the
existing match-policy consumer.

See the [oracle record](../tests/Pcre/Native/capture-anchor-skip-oracle.md).
Observe native-first failures and freeze the catalog, assertions, and test
project before implementation. Final six-host .NET 10 tests, .NET 8/9/10
builds, frozen cases, both API snapshots, package records, and configured
compatibility/support results are recorded on the pull request.

Broader grouped/alternative anchor inference, other driver replay boundaries,
wider P09 conformance/resource/performance work, native UTF-16, later roadmap
stages, and consumer migrations remain open. Grep migrates last.

## Regression checkpoint

At a1e7d59bb4ca547d58a142301151bdb3a55294cd, tree
e078361ba7946214e81f6e956fdcf7e35282507d, Linux x64 reported 27 failures,
106,953 passes, and zero skips in
[run 44835543160](https://github.com/uniblab/Icod.Expressions/actions/runs/44835543160).
The twenty-five affected theories completed their native comparisons before
failing in the managed consumer. The two resource cases failed at their new
no-match assertions. All 106,946 prior tests and seven new controls passed.

Freeze the catalog at Git blob 02ac9a030b2a7181e330f4b6ad2cd25d4ea08b79,
the assertions at 25f71b4ac025a439239b19ca9a4a96bf5f52aeaa, and the test
project at a56a0a03a9db379e43bd265533975e6aaa8725ea before implementation.
