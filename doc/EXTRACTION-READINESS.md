# Extraction readiness

**Status:** preparation only; A05 is not frozen and E00 is not complete.
This file records the handoff boundary for the first source copy. The
authoritative in-place evidence remains in
[`Icod.CommandFramework` PR #16](https://github.com/uniblab/Icod.CommandFramework/pull/16).

| Gate | Present evidence | Needed before the source copy |
|---|---|---|
| A00 source/API | Candidate production SHA `e1e118f18246eed290bee4758325a20e302bfb92`; 32 regex C# files, 10 focused C# test files, two external Text enums; compiled API inventory of 24 top-level public regex types. | Confirm final source SHA and source/test blob manifest. |
| A01 strict POSIX | Draft BRE/ERE contracts and semantic audit on PR #16. | Close or explicitly record standards and independent oracle gaps. |
| A02 GNU syntax | Pinned Gnulib `fb7312fa8d3df29f0ca0678f669b9a5b88a078ec`: 26 syntax bits and 13 named macros mapped. | Resolve profile and option evidence needed for the extraction contract. |
| A03 consumers | CoreUtils, Grep, ProcPs, DiffUtils, and LineEditor call shapes mapped. | Validate consumer-shaped assertions and preserve command policy outside this library. |
| A04 characterization | 110 focused test declarations; a 44-case JSONL parity corpus and three-platform CI in PR #16. | Classify unresolved differentials and pin the exact final test corpus and oracle identities. |
| A05 freeze | No approved baseline yet. | Publish source SHA, API, feature/profile matrices, test blobs/counts, fixture identities, known bugs/deviations, reference revisions, and passing CI. |

The candidate SHA identifies the unchanged **production** engine; the tests and
audit documents on PR #16 are additive. Its branch head is not the final
extraction source identity. The project in this repository currently contains
no copied implementation, API, or tests. The frozen corpus will be
transplanted before the production source, and test expectations will not be
rewritten to make the move pass.

The current engine's empty-anchor capture participation differs from expected
registers in pinned Gnulib `tests/test-regex.c`. A separate GNU grep probe
found an Extended middle-anchor difference. Both remain characterization
items; an extraction must first preserve and label inherited behavior. The
`tac` provider/option mismatch is tracked as
[CoreUtils #190](https://github.com/uniblab/Icod.CoreUtils/issues/190)
for later correction.

The source copy must preserve licensing and attribution, including any
upstream-derived fixtures. The repository's LGPL-3.0 license and the source
project's `LGPL-3.0-or-later` metadata are recorded; per-file provenance is
still under review in A00. No CommandFramework code is removed or redirected
as part of the copy.
