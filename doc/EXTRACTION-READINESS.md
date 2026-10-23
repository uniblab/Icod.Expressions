# Extraction readiness

**Status:** The first copy-time corpus and source identities are pinned in
CommandFramework PR #16. A [final A05 corpus candidate](Icod.Expressions-A05-Final-Candidate.md)
now consolidates 214 focused cases and the classified limits, while formal
acceptance remains open. E00 transplanted that corpus, E01 copied the source
without editing the original files, and the [final E02 same-run comparison](E02-Final-Parity-Review.md)
established parity within the pinned corpus. The E03 alpha
package-only consumer check passed; the compatibility bridge and downstream
migrations remain separate work. The source copy happened before all A01–A04
audit gates closed; it does not advance the later CommandFramework ownership
change or a stable release.
This file records the handoff evidence for the first source copy. The
authoritative in-place evidence remains in
[`Icod.CommandFramework` PR #16](https://github.com/uniblab/Icod.CommandFramework/pull/16).

| Gate | Present evidence | Remaining work |
|---|---|---|
| A00 source/API | Candidate production SHA `e1e118f18246eed290bee4758325a20e302bfb92`; 32 regex C# files, 10 focused C# test files, two external Text enums; compiled API inventory of 24 top-level public regex types. | Confirm final source SHA and source/test blob manifest. |
| A01 strict POSIX | Draft BRE/ERE contracts and semantic audit on PR #16. | Close or explicitly record standards and independent oracle gaps. |
| A02 GNU syntax | Pinned Gnulib `fb7312fa8d3df29f0ca0678f669b9a5b88a078ec`: 26 syntax bits and 13 named macros mapped; 16 additive managed cases exercise selected two-polarity options and coupled behavior. | Resolve remaining profile composition and missing-control evidence. Exact profile implementation is N01, after extraction parity. |
| A03 consumers | CoreUtils, Grep, ProcPs, DiffUtils, and LineEditor call shapes mapped. The [cross-platform pinned gate](E03-Compatibility-Bridge.md#cross-platform-pinned-consumer-gate) reruns 287 CoreUtils (174 regex callers plus 113 shared-Text consumers), 24 ProcPs, 62 DiffUtils, 113 Grep, and 238 LineEditor original cases before and after scratch bridge substitution on Windows, macOS, and Linux ([CI run 40014111537](https://github.com/uniblab/Icod.Expressions/actions/runs/40014111537), 724 total per pass). | Audit uncovered options and callers, including CoreUtils Shared's standalone Text API tests; `tac` is deferred to [CoreUtils #190](https://github.com/uniblab/Icod.CoreUtils/issues/190) and direct DiffUtils `diff -I` to [DiffUtils #8](https://github.com/uniblab/Icod.DiffUtils/issues/8). A distributable bridge and actual migrations remain separate work. |
| A04 characterization | 110 focused declarations, 193 runtime cases and a 44-case JSONL corpus. [Three-platform CI](https://github.com/uniblab/Icod.CommandFramework/actions/runs/39867051683) passed on the characterized test commit. | Standards/profile oracle and full consumer differentials remain explicit gaps for normalization. |
| A05 candidate | The [copy-time checkpoint](https://github.com/uniblab/Icod.CommandFramework/blob/docs/icod-expressions-a00-in-situ-inventory/doc/Icod.Expressions-A05-Extraction-Baseline.md) is preserved. The [final candidate](Icod.Expressions-A05-Final-Candidate.md) freezes 13 focused test files, 214 cases, the original API/source/44-case JSONL, and 15-case oracle supplement with a classified gap ledger. [CI run 40011998105](https://github.com/uniblab/Icod.Expressions/actions/runs/40011998105) passed the consolidated verifier, final E02 comparison, scratch-manifest negative controls, and all five jobs. | Review the classified A01–A04 limits before formal A05 acceptance; retain CommandFramework production intact. |
| E00 transplant | Eleven unchanged test C# blobs, the test README, 44-case JSONL, API snapshot, and their unchanged runner/tool sources copied; `tools/verify-frozen-corpus.sh` checks their identities. | Source was deliberately absent in the E00 commit. The tests and tools now run after E01. |
| E01 source copy | 32 regex C# files and README plus two Text enums retain their A05 blob identities; `tools/verify-frozen-source.sh` checks all 35 paths. One new assembly metadata file grants the new test assembly friend access to inherited internal prepared-input types. | No parser, matcher, option, or source namespace was changed. |
| E02 final corpus parity | [CI run 40011998105](https://github.com/uniblab/Icod.Expressions/actions/runs/40011998105) passed a [same-run old/new comparison](E02-Final-Parity-Review.md) on Windows, macOS, and Linux: 35 source blobs and 13 test blobs match, all 214 focused test identities/outcomes pass on both .NET 10 assemblies, and both runners verify the identical 44-case JSONL. Scratch manifests with a wrong hash and missing entry are rejected. The extracted assembly also passes its corpus on .NET 8 and 9 across all three systems; public API member lines match the pinned snapshot. | Preserve the old engine as an oracle for later normalization. This bounded parity result does not settle strict POSIX/GNU conformance, uncharacterized behavior, or the separate ownership change. |
| E03 package candidate | The unpublished `0.1.0-alpha.1` package passes the package-only smoke on .NET 8, 9, and 10. [CI run 40014111537](https://github.com/uniblab/Icod.Expressions/actions/runs/40014111537) passed an old-client, 26-type forwarding smoke and the 724-case scratch consumer gate before and after substitution on Windows, Linux, and macOS. | Review the [compatibility bridge](E03-Compatibility-Bridge.md), audit uncovered callers and options, then decide the separate CommandFramework ownership change after final parity. The package is not published. |

Five additional consumer-shaped tests were added in CommandFramework and copied
unchanged into Expressions after the first checkpoint. They make the current
focused count **198** on local .NET 10. Their shared blob
`589a52b42d5e3010e9e678f771cc85b156236e0b` and scope are recorded in
the [A04 supplement](https://github.com/uniblab/Icod.CommandFramework/blob/docs/icod-expressions-a00-in-situ-inventory/doc/Icod.Expressions-A04-Consumer-Shape-Supplement.md).
The original 193-test/44-fixture checkpoint is unchanged.
[CI run 39872443907](https://github.com/uniblab/Icod.Expressions/actions/runs/39872443907)
passed the expanded 198-test gate on each supported framework across all
three operating systems. None of the five tests proves full command-level
consumer compatibility.

Sixteen further A02 syntax-option cases in
`IcodExpressionsA02SyntaxOptionCharacterizationTests.cs` have the same Git blob
`ef9cd9e358460f412d343c91db228c6179c964ff` in both repositories.
Together the focused directory has 214 runtime cases locally on .NET 10.
The original copy-time corpus and five-case supplement retain their identities.
These managed cases do not establish exact Gnulib named-profile conformance.

A separate [15-case Linux/glibc differential](https://github.com/uniblab/Icod.CommandFramework/blob/docs/icod-expressions-a00-in-situ-inventory/doc/Icod.Expressions-A04-POSIX-Glibc-Differential.md)
compares ASCII BRE/ERE whole matches and capture spans with the inherited
engine. The C# runner source has identical Git blob
`59a10c631112307159a0682a493062de4cd82c80` in both repositories;
only its project reference changes. Thirteen samples agree. Two defined ERE
middle-anchor cases diverge under Issue 8 §9.4.9: glibc finds no match while
the inherited Extended parser treats those anchor characters as literals.
glibc and the inherited engine both return a participating empty
capture for two anchor-branch cases. The pinned Gnulib source table lists an
absent group but explicitly tolerates a different runtime result. The
[included Gnulib oracle runner](../tools/GnulibRegexOracle/run.sh) builds
the exact pinned engine and observes `(0,0)` for both ERE and two analogous
BRE cases. This resolves the apparent source-table discrepancy for those
inputs, without establishing strict POSIX conformance or broad GNU profile
parity.

The source SHA identifies the unchanged **production** engine; the tests and
audit documents on PR #16 are additive. Its branch head is not the extraction
source identity. This repository contains copied tests, fixtures, and the
unchanged production source. No expectation was rewritten. The test project
now belongs to the solution, and the parity and API tools reference the new
assembly. The public namespace is temporarily inherited to avoid semantic
changes during E01/E02. Only the package/assembly identity and test friend
assembly declaration differ from the old implementation; API member lines
match. The first snapshot header records the expected change from
`Icod.CommandFramework 2.2.1.0` to `Icod.Expressions 0.1.0.0`.

## Source dependency closure probe

The in-place regex tree has a direct compile dependency on
`Icod.CommandFramework.Text.TextDecodingMode` and
`Icod.CommandFramework.Text.InvalidEncodingPolicy`, which are outside its
directory. To reproduce a standalone compile check against a local checkout
of the candidate source commit, run from this repository:

```sh
dotnet build tools/SourceClosure/SourceClosure.csproj -c Release \
  -p:CommandFrameworkRoot=/absolute/path/to/Icod.CommandFramework
```

At source commit `e1e118f18246eed290bee4758325a20e302bfb92`, the 32 regex
C# files plus these two enums compile with SDK 10.0.402, zero warnings and
zero errors, without the rest of CommandFramework or its `Icod.Path` package
reference. This earlier **compile dependency check** did not by itself
establish public API, runtime behavior, multi-target support, extraction
parity, or licensing closure. The probe still links original local files and
is separate from the copy now present in the library project.

The pinned Gnulib test table's absent anchor capture is a tolerated expected
value; the included engine actually agrees with the inherited participating
empty capture in the four probed cases. A separate GNU grep probe found an
Extended middle-anchor difference. E01 preserves and labels the inherited
behavior. The
`tac` provider/option mismatch is tracked as
[CoreUtils #190](https://github.com/uniblab/Icod.CoreUtils/issues/190)
for later correction.

The source copy must preserve licensing and attribution, including any
upstream-derived fixtures. The repository's LGPL-3.0 license and the source
project's `LGPL-3.0-or-later` metadata are recorded; per-file provenance is
still under review in A00. No CommandFramework code is removed or redirected
as part of the copy.
