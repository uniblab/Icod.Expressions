# Internal PCRE package consumer gate

Base merged PR282 `83bb99174b86e5e2123b0459fce77a8085528c58`, tree
`1be63937f916527b52dab59830061be2ff9870ff`; 104,715 prior tests.
Advance the package-consumer part of P09, keeping the complete freeze open.
No agents, Python or local SDK installation. Preserve all 375 existing C#
test sources/assertions and A05 source35/corpus. Keep #7/#102 and issue #7
footer fixed; Grep last; user owns merge/tags/publication.

## Scope and internal candidate contract

The existing package-only sample exercises GNU/POSIX public APIs but not the
internal PCRE engine. Add an explicitly named Icod.Expressions.PackageSmoke
friend assembly and exercise the actual NuGet candidate without a project
reference, copied engine source, reflection or PCRE.NET dependency in the app.
The friend is test/sample infrastructure, not a public PCRE API or a stable
external compatibility promise. Do not reuse another tool's friend identity.

| Internal candidate operation | Declared consumer contract |
|---|---|
| PcreByteCompiler.Compile(ReadOnlyMemory<byte>, PcreByteOptions?, CancellationToken) | Owned pattern snapshot; raw/UTF/UCP caller modes; compile diagnostics and cancellation. |
| PcreCompiledBytePattern.Match(ReadOnlyMemory<byte>, int, CancellationToken) | Original visible byte-view coordinates, complete match/no-match or diagnostic; positive work budget and cancellation. |
| PcreByteMatch / capture ranges / MarkName | Numbered native byte offsets, unset slots, successful mark bytes. No subject decoding or borrowed pattern-source lifetime. |
| Unsupported modes/syntax | Existing diagnostic policy, including unsupported UTF code-unit escape; no silent fallback. |

This bounded contract does not freeze every provisional option/type. Callouts,
partial/substitution/iteration, a production native16 backend and broader
security/performance/API release evidence remain separate gates. The observed
view control is specific evidence for its execution path, not a blanket
allocation/performance/no-transcoding release claim.

## Native-first cases and meaningful RED

Reuse exact existing native-first literals: 12 raw cases from
PcreByteCodeUnitReferenceTests.cs blob c349b39208e81533d0fe971b26d4af519720ddd6;
12 UTF and seven UCP cases from PcreDualWidthCases.cs blob
9fab1e91ffa35e712f9fc8277b049fdd1eaa0675; one UCP word-boundary case from
PcrePerlOverlapCases.cs blob 9d17cb50bb731f9e763b7cd58d7626597b07deba.
Patterns, native start offsets, captures and successful marks are unchanged.
These immutable fixtures already have independent pinned PCRE2 10.48/JIT and
six-host oracle evidence. The new tests repeat native8 assertions before
calling the missing consumer helper. No new native semantic expectation is
invented from the consumer implementation.

32 matching/no-match cases cover raw NUL/high bytes, UTF supplementary values,
UCP properties/graphemes/word boundaries, captures, lookarounds, alternatives,
repeats, source modes and marks. Ten managed contract cases cover invalid work
limits, outside start offsets, malformed UTF pattern/subject, work exhaustion
and the deliberately unsupported UTF code-unit escape. They are managed
boundary contracts, not claims of native diagnostic-code identity.

Every runtime case supplies a padded pattern/subject memory slice, with invalid
sentinels outside the visible view. After successful compilation, overwrite
the caller's pattern buffer before matching to check pattern ownership. Match
results must keep view-relative byte offsets and captures. One observed-view
control changes the authoritative original byte at the first work callback;
the comparison must observe it, and sentinels must remain untouched. Two
controls require exact cancellation propagation from compile and match.

45 new tests give 104,760 total. Source-link the two new sample exercise/data
files into the existing test assembly. The sole prior-test-file exception is
tests/Icod.Expressions.Tests.csproj: add those two Compile links; dependency/
framework/project-reference declarations and every old C# test stay fixed.
Publish successful-build tests-only RED with the exercise stub:45 expected
NotImplemented failures /104,715 passes /zero skips. Reconcile every identity
and cause, then freeze the new data/assertions and test-project XML before
implementing only the exercise helper. Freeze that helper afterward.

## Package qualification architecture

After RED, grant only the explicit sample friend in src/Properties/AssemblyInfo.cs,
set its AssemblyName, and add PCRE execution after the existing GNU/POSIX
assertions without editing those assertions. The matcher/compiler/Unicode
source remains byte-identical. Pcre subtree baseline:
`cbe160ce6605be45840127ce36ba5aca09b66bf7`.

A shared composite action packs the already-built library into a fresh
RUNNER_TEMP directory, restores the sample only from that feed with an
isolated package cache, builds its net8/net9/net10 targets, then runs net10.
The sample emits one structured pcre-package-smoke result:42 protocol cases,
one observed-view control, two cancellation controls, zero failures. It has
only the existing Icod.Expressions PackageReference; tests alone use PCRE.NET.
Package identity, dependencies, version and release-publication policy stay fixed.

Apply the action to all five native matrix jobs in PR/main and the shared
native macOS x64 action. Preserve main's existing package artifact verification
and the release workflow. The updated sample also participates in release's
existing isolated package-only check. No new CI jobs/matrix axes are needed;
source-pinned compatibility/Gnulib now run because metadata/project/workflow
paths changed. Record those actual gates rather than claiming path skips.

## Qualification and review

Whole-branch author review and this design/plan plus all five shared inventories
precede final source qualification. Preserve legacy source35/API/parity and all
old test assertions; review the explicit metadata/XML/sample/workflow changes.
Build all three library and consumer targets; run full net10 (104,760), frozen
214 cases /120 methods and the package consumer on all six native hosts with
zero failures/skips/coded warnings/errors. All configured compatibility,
Gnulib, Unicode/workload/parity/API/package/script gates must pass. Record final
source/tree/base/merge identities, plan completion, cleanup and ready readback
externally on PR283. No source/docs/test edits after final GREEN.

## Observed RED and assertion freeze

Tests-only source cd1c46a300311daabd472b685f6c1a49eb128d89, tree
d32647db1c90b984f75981902ba5d26b1e994ff8; proposed merge
b8b8b071861d5e8f65800163a479bda4cf898f59. Run44407114945: Linux x64
135165197265, Linux ARM64 135165197125 and Windows x64 135165197353 build
all three targets with zero coded warnings/errors, then report45 failed /
104,715 passed /104,760 total /zero skips. Every failure reaches a missing
consumer method: matching32/diagnostics10/view1/cancellation2; both cancellation
rows are expected exception-type mismatches. All native literal assertions pass.
No old C# case fails. Freeze before implementation:

- samples/PackageSmoke/PcreSmokeCases.cs: b53df8ef92c9335678df5fa168aded6a00912714.
- tests/Pcre/PcrePackageConsumerTests.cs: 174963a4c96e7fcce71916be9f7b4f5935374185.
- tests/Icod.Expressions.Tests.csproj: 1b425befc99db5459b6fea3a7f0dca95fed313df.

## Completed-helper freeze and prequalification review

Completed exercise blob 58e8c2a22ce3ab78d62bf0a77f8924d7263fdaa5. All six RED hosts report the same45
identities/causes with all32 native literal checks passing and104,715 old tests
green. The new case/assertion/XML blobs above remain identical.

Reviewed the complete 19-file branch: explicit friend identity and source
visibility, package-only reference, original sample assertions, owned pattern
and visible subject views, capture/mark/diagnostic projection, observed callback
and exact cancellation paths. Reviewed both native matrix call sites plus the
shared Intel action, unique feed/cache creation, no-build candidate pack,
restore failure guards, all-framework consumer build, net10 execution and
strict single structured-result validation. Release workflow and main artifact
verification are unchanged. No project/native dependency leaks into the sample.

All375 old C# test blobs remain fixed; only the documented test-project XML
links change among385 prior test files. The Pcre subtree remains
cbe160ce6605be45840127ce36ba5aca09b66bf7; the sole production source change
is the sample friend attribute outside the frozen legacy source35. Public
API/parity/A05 must still pass CI. Local A05 identity, classifier, main-job
prerequisite (eight required/thirteen rejection controls) and whitespace gates
pass. No local SDK/PowerShell/YAML parser is installed; real hosted builds,
action execution and all configured support gates supply final qualification.

This design/plan and all five shared inventories were reviewed before final
source upload. Final source/tree/base/merge identities, gate outcomes, cleanup
and ready readback stay external on PR283, with no source/docs/test edits
after final GREEN. P09 remains a bounded increment, not a complete freeze.
