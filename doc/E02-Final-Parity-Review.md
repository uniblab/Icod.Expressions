# E02 final corpus parity review

**Result:** Exact extraction parity is established within the frozen A05
corpus. This is a bounded old/new comparison, not a strict POSIX or exact
Gnulib conformance decision. Formal A05 acceptance and the later
CommandFramework ownership change remain separate reviews.

## Same-run evidence

The [FinalParity gate](../tools/FinalParity/run.ps1) fetches the in-situ
CommandFramework audit commit `532850103e69e5d3d09367c64f929ddecaf1fe69`
into ignored scratch space. Its production regex tree has identity
`bfe11d5f5ff6a75d75de68573edea6125706b87e`; the underlying production
source snapshot remains `e1e118f18246eed290bee4758325a20e302bfb92`.
The gate verifies all 35 source/README/Text-enum blobs against the extraction
manifest and all 13 focused C# test blobs between the two commits.

In the same CI job, the original CommandFramework test project and the
Expressions test project each run the final focused corpus on .NET 10. Their
TRX files must each contain exactly 214 passing cases with no failures or
skips, and the sorted test names and outcomes must agree. Each version of
the parity runner independently verifies its output against the same frozen
44-case JSONL. This exercises the recorded diagnostics, match results,
captures, offsets, byte surfaces, malformed input, and selected resource
outcomes without rewriting an expectation.

[CI run 40007473350](https://github.com/uniblab/Icod.Expressions/actions/runs/40007473350)
passed all five jobs, including this gate in Staging on Windows, macOS, and
Linux. The Expressions corpus and JSONL also passed on .NET 8 and 9 in the
existing matrix; the old CommandFramework focused test project targets only
.NET 10. The compiled public API member-line comparison passed with the
intentional assembly identity change.

## Scope of the decision

This evidence supports **zero observed differences in the final A05
extraction corpus** and the unchanged source/API identities. Identical test
names and pass results do not compare every intermediate value beyond the
assertions; the JSONL supplies exact results for its 44 selected cases.
The old engine remains available as an oracle for later changes.

The [A05 limit ledger](https://github.com/uniblab/Icod.CommandFramework/blob/docs/icod-expressions-a00-in-situ-inventory/doc/Icod.Expressions-A05-Final-Candidate.md#classified-limits-of-this-extraction-contract)
still governs interpretation: inherited ERE middle-anchor divergence,
missing strict BRE/ERE surfaces, incomplete GNU syntax/profile controls,
locale and collation boundaries, and finite differential coverage.
The three-platform 611-case scratch consumer gate is compatibility evidence,
not a distributable bridge or a consumer dependency migration. `tac` and
direct `diff -I` corrections remain deferred to
[CoreUtils #190](https://github.com/uniblab/Icod.CoreUtils/issues/190) and
[DiffUtils #8](https://github.com/uniblab/Icod.DiffUtils/issues/8).

**Review disposition:** E02 passes for the frozen, bounded A05 corpus. A05
formal acceptance should explicitly carry these classified limits forward.
No CommandFramework production source was changed for this gate.
