# Extraction readiness

**Status:** A05 extraction inputs are frozen in CommandFramework PR #16;
E00 transplanted their tests and fixtures, and E01 copied the source without
editing the original files. E02 cross-platform parity is in progress.
This file records the handoff evidence for the first source copy. The
authoritative in-place evidence remains in
[`Icod.CommandFramework` PR #16](https://github.com/uniblab/Icod.CommandFramework/pull/16).

| Gate | Present evidence | Remaining work |
|---|---|---|
| A00 source/API | Candidate production SHA `e1e118f18246eed290bee4758325a20e302bfb92`; 32 regex C# files, 10 focused C# test files, two external Text enums; compiled API inventory of 24 top-level public regex types. | Confirm final source SHA and source/test blob manifest. |
| A01 strict POSIX | Draft BRE/ERE contracts and semantic audit on PR #16. | Close or explicitly record standards and independent oracle gaps. |
| A02 GNU syntax | Pinned Gnulib `fb7312fa8d3df29f0ca0678f669b9a5b88a078ec`: 26 syntax bits and 13 named macros mapped. | Resolve profile and option evidence needed for the extraction contract. |
| A03 consumers | CoreUtils, Grep, ProcPs, DiffUtils, and LineEditor call shapes mapped. | Validate consumer-shaped assertions and preserve command policy outside this library. |
| A04 characterization | 110 focused declarations, 193 runtime cases and a 44-case JSONL corpus. [Three-platform CI](https://github.com/uniblab/Icod.CommandFramework/actions/runs/39867051683) passed on the characterized test commit. | Standards/profile oracle and full consumer differentials remain explicit gaps for normalization. |
| A05 freeze | [Extraction baseline](https://github.com/uniblab/Icod.CommandFramework/blob/docs/icod-expressions-a00-in-situ-inventory/doc/Icod.Expressions-A05-Extraction-Baseline.md): source/test blobs, API, fixtures, known gaps, reference pins, and validation. | Preserve this baseline during E01/E02; no CommandFramework removal or dependency switch. |
| E00 transplant | Eleven unchanged test C# blobs, the test README, 44-case JSONL, API snapshot, and their unchanged runner/tool sources copied; `tools/verify-frozen-corpus.sh` checks their identities. | Source was deliberately absent in the E00 commit. The tests and tools now run after E01. |
| E01 source copy | 32 regex C# files and README plus two Text enums retain their A05 blob identities; `tools/verify-frozen-source.sh` checks all 35 paths. One new assembly metadata file grants the new test assembly friend access to inherited internal prepared-input types. | No parser, matcher, option, or source namespace was changed. |
| E02 parity | [Initial E01 CI](https://github.com/uniblab/Icod.Expressions/actions/runs/39868662807) built all three library targets and passed .NET 10 tests and parity on Windows, Linux, and macOS. Local .NET 10 Staging: 193/193 focused runtime tests, 44-case byte-identical JSONL, and all compiled API member lines identical after omitting the assembly/version header. | Run the unchanged tests and parity corpus on .NET 8 and 9 across the same operating systems; keep packaging disabled until checks pass. |

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
