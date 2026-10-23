# A05 final extraction corpus candidate

**Status:** Prepared for formal review; bounded extraction parity, not strict
POSIX/Gnulib conformance or a stable release. The authoritative in-situ
[A05 candidate and gap ledger](https://github.com/uniblab/Icod.CommandFramework/blob/docs/icod-expressions-a00-in-situ-inventory/doc/Icod.Expressions-A05-Final-Candidate.md)
is in CommandFramework PR #16. The first 193-test/44-case copy-time
checkpoint remains unchanged.

| Candidate input | Frozen identity and result |
|---|---|
| Production source | CommandFramework `e1e118f18246eed290bee4758325a20e302bfb92`; copied regex tree `bfe11d5f5ff6a75d75de68573edea6125706b87e` and two external Text enums. All 35 blobs match the original manifest. |
| Public API | 24 regex types and two Text enums; member-line snapshot SHA-256 `8e64436d919f0b9fba74de88a03c2154960fe33d8a29675d6b360c15786abc82`. The assembly identity differs by design. |
| Focused corpus | Thirteen identical test C# blobs in both repositories: eleven original (193 cases), the consumer supplement `589a52b42d5e3010e9e678f771cc85b156236e0b` (five cases), and A02 characterization `ef9cd9e358460f412d343c91db228c6179c964ff` (16 cases). Total 214 runtime cases. The 44-case JSONL SHA-256 remains `61696259b079b0ef48b33326b51db8a37083e2dd9219825024398786c3cea4ff`. |
| Oracle supplements | The 15-case Linux/glibc runner blob `59a10c631112307159a0682a493062de4cd82c80` matches CommandFramework's. The pinned included-Gnulib capture runner blob is `7511107d8c12d1e98b53c88311c2d44ae0c0c387`. |
| Consumers | An unpublished alpha package and scratch compatibility bridge rerun 611 original cases across five source-pinned consumer repositories on Windows, Linux, and macOS. An old-package-compiled binary resolves all 26 public identities through a thin forwarder on those systems. |

Run `sh tools/verify-a05-candidate.sh` to check the original and additive
blobs, source manifest, focused-file count, and oracle-tool identities. CI
also requires 214 passing focused tests and exact 44-case JSONL on .NET 8,
9, and 10 across Windows, Linux, and macOS. [CI run
40011998105](https://github.com/uniblab/Icod.Expressions/actions/runs/40011998105)
passed all five jobs with the consolidated verifier, [final same-run E02
comparison](E02-Final-Parity-Review.md), cross-platform forwarding check, and
611-case consumer bridge gate on three systems. Its final parity gate also
rejected scratch manifests with an altered source hash and a missing entry.
This satisfies the candidate's executable verification gate; its classified
limits still need formal A05 review.

The [A01–A04 ledger in CommandFramework](https://github.com/uniblab/Icod.CommandFramework/blob/docs/icod-expressions-a00-in-situ-inventory/doc/Icod.Expressions-A05-Final-Candidate.md#classified-limits-of-this-extraction-contract)
classifies the two defined middle-anchor ERE divergences, the missing strict
BRE/ERE surfaces, incomplete Gnulib bit/profile control, locale/collation
limits, bounded oracle coverage, and deferred `tac`/`diff -I` fixes. The
package-backed bridge is a feasibility check; actual consumer migrations and
a distributable cross-platform bridge remain E03 work. Formal A05 acceptance
should use this frozen candidate, the [completed bounded E02 review](E02-Final-Parity-Review.md),
and those explicit limitations. CommandFramework production remains intact.
