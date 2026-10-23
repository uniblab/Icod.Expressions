# Extraction readiness

**Status:** The first copy-time corpus and source identities are pinned in
CommandFramework PR #16, while formal A05 acceptance remains open. E00
transplanted that corpus, E01 copied the source without editing the original
files, and E02 established parity within the pinned corpus. The E03 alpha
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
| A02 GNU syntax | Pinned Gnulib `fb7312fa8d3df29f0ca0678f669b9a5b88a078ec`: 26 syntax bits and 13 named macros mapped. | Resolve profile and option evidence needed for the extraction contract. |
| A03 consumers | CoreUtils, Grep, ProcPs, DiffUtils, and LineEditor call shapes mapped. The [package-backed consumer gate](E03-Compatibility-Bridge.md#reproducible-package-backed-consumer-gate) reruns 50 CoreUtils `expr`, 24 ProcPs, 62 DiffUtils, and 113 Grep tests on hosted Linux. An additional 238 LineEditor tests passed locally with the temporary forwarder; expanded CI is pending. | Validate other CoreUtils commands and remaining caller coverage; direct DiffUtils `diff -I` is tracked in [DiffUtils #8](https://github.com/uniblab/Icod.DiffUtils/issues/8). Build a distributable, cross-platform bridge only after final parity and preserve command policy outside this library. |
| A04 characterization | 110 focused declarations, 193 runtime cases and a 44-case JSONL corpus. [Three-platform CI](https://github.com/uniblab/Icod.CommandFramework/actions/runs/39867051683) passed on the characterized test commit. | Standards/profile oracle and full consumer differentials remain explicit gaps for normalization. |
| A05 candidate | [Copy-time checkpoint](https://github.com/uniblab/Icod.CommandFramework/blob/docs/icod-expressions-a00-in-situ-inventory/doc/Icod.Expressions-A05-Extraction-Baseline.md): source/test blobs, API, fixtures, known gaps, reference pins, and validation. | Close A01–A04 behavior-or-gap coverage and declare a final A05 corpus. Preserve this first checkpoint; no CommandFramework removal or dependency switch. |
| E00 transplant | Eleven unchanged test C# blobs, the test README, 44-case JSONL, API snapshot, and their unchanged runner/tool sources copied; `tools/verify-frozen-corpus.sh` checks their identities. | Source was deliberately absent in the E00 commit. The tests and tools now run after E01. |
| E01 source copy | 32 regex C# files and README plus two Text enums retain their A05 blob identities; `tools/verify-frozen-source.sh` checks all 35 paths. One new assembly metadata file grants the new test assembly friend access to inherited internal prepared-input types. | No parser, matcher, option, or source namespace was changed. |
| E02 copy-time parity | [CI run 39870012051](https://github.com/uniblab/Icod.Expressions/actions/runs/39870012051) passed all five jobs: exactly 193 focused tests and the 44-case byte-identical JSONL on .NET 8, 9, and 10 across Windows, Linux, and macOS; source and fixture blobs and compiled API member lines match the first checkpoint. | Extend the in-situ evidence and rerun exact parity against the final A05 corpus. Keep the old implementation as an oracle while later normalization and consumer migration proceed. |
| E03 package candidate | [CI run 39869640279](https://github.com/uniblab/Icod.Expressions/actions/runs/39869640279) passed. It packed `0.1.0-alpha.1` and ran an independent package consumer on .NET 8, 9, and 10, with no CommandFramework dependency. | Review the [compatibility bridge](E03-Compatibility-Bridge.md), validate all A03 consumers, and decide the separate CommandFramework ownership change. The package is not published. |

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

A separate [nine-case Linux/glibc differential](https://github.com/uniblab/Icod.CommandFramework/blob/docs/icod-expressions-a00-in-situ-inventory/doc/Icod.Expressions-A04-POSIX-Glibc-Differential.md)
compares ASCII BRE/ERE whole matches and capture spans with the inherited
engine. The C# runner source has identical Git blob
`bec36b11aa2bd1617e191ac49703244a03dcc10b` in both repositories;
only its project reference changes. Seven ordinary standards-facing samples
agree. glibc and the inherited engine both return a participating empty
capture for two anchor-branch cases where the pinned Gnulib source test
expects an absent capture. That disagreement remains unresolved; the tool
is a Linux/glibc observation, not a pinned Gnulib oracle or conformance claim.

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

The current engine's empty-anchor capture participation differs from expected
registers in pinned Gnulib `tests/test-regex.c`. A separate GNU grep probe
found an Extended middle-anchor difference. Both remain characterization
items; E01 preserves and labels the inherited behavior. The
`tac` provider/option mismatch is tracked as
[CoreUtils #190](https://github.com/uniblab/Icod.CoreUtils/issues/190)
for later correction.

The source copy must preserve licensing and attribution, including any
upstream-derived fixtures. The repository's LGPL-3.0 license and the source
project's `LGPL-3.0-or-later` metadata are recorded; per-file provenance is
still under review in A00. No CommandFramework code is removed or redirected
as part of the copy.
