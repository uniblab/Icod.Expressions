# Caller control of start optimizations

Continue issue7 after merged254 `463ae290d4281445bedb0cfcc724dd4942004047`, tree `6e26348ba1af4c35ec7b547a1dba9d1d7daf224f`, and92,355 tests. Add internal bool NoStartOptimize, defaultfalse, matching PCRE2 10.48's NO_START_OPTIMIZE compile bit0x10000. Disable start-sensitive candidate proofs/accelerators while preserving normal ordered matching, explicit anchors and independent automatic possessification. Native examples: (*COMMIT)A on BA loses its filtered later candidate, and A+(*PRUNE)B|A on AA loses detect_early_fail reuse. FIRSTLINE's forward SKIP path bypasses a later first-unit filter when start study is disabled.

Four production files: PcreByteOptions.cs adds the caller bool outside Literal conflicts (native allows this bit); PcreByteCompiler.cs copies it into program construction; PcreByteProgram.cs suppresses existing UTF start-class/repeat/byte-prefix/required-suffix/COMMIT/PRUNE-cache/line-start proofs and resolves unfiltered FIRSTLINE SKIP entry; PcreByteMatcher.cs disables its two private unobserved start accelerators once at match setup. Carry the bool on the owned program. No new opcode/frame/AST/parser/diagnostic/public API/dependency/workflow. Retain explicit newline provenance, nullable-fragment admission and the independent NoAutoPossess gate. Native JIT detect_early_fail is guarded by OPTIM_START_OPTIMIZE, so the bounded PRUNE cache must be disabled too.

Approaches considered: disable only first-byte COMMIT filtering, which misses CRLF entry, PRUNE cache and FIRSTLINE restart effects; or add a runtime policy branch inside every opcode, which adds work without need. Select existing proof/metadata gates at compile time and two accelerator guards at match setup. Caller false keeps all current behavior. Source (*NO_START_OPT), separate dot-star anchoring, broader default optimizer availability, callouts, UTF code-unit matching and consumer/API/release gates remain later work. Standing autonomous inline authorization applies; no agents/Python/SDK installs/approval pauses. User merges/tags/publishes; Grep last.

Native research pins615 profiles:547 accepted groups/42,792 match-offset protocols and68 diagnostics;882 strict split-offset(-36) rows are separate. Cover byte/strict/tolerant UTF, LF/ANYCRLF/ANY, first-unit/prefix/start-class/repeat filtering, COMMIT/PRUNE/SKIP/named SKIP, FIRSTLINE/Anchored/EndAnchored, signed captures/marks, independent NoAutoPossess, literal compatibility, source/caller lockouts and original-byte length priority. Add28 work/ownership/unobserved-budget/validation/no-read/capacity cases:643 new/92,998 total. Expected tests-only RED:133 required-policy groups plus3 work and3 ownership cases =139 failures/92,859 passes/zero skips/compiler warnings/errors. All92,355 prior tests and504 new controls pass. Native preflights precede managed comparisons. Freeze3 new C# blobs after clean RED; preserve all281 prior test files/276 C# sources and frozen35 production blobs.

Review boundaries: all native-sensitive proofs and private setup accelerators, including detect_early_fail; independent explicit anchors/possessification and intrinsic newline/fragment semantics; FIRSTLINE and forced SKIP entry versus ordinary candidate windows; Literal/length/UTF/lockout priority and compiled source/flag ownership; signed captures/marks, exact observed/unobserved budgets/cancellation and full storage capacities.

All seven inventories precede final source qualification. Require six fresh net10 results with92,998 passes/214 frozen outcomes/120 methods each, all net8/net9/net10 library outputs, zero failures/skips/compiler warnings/errors and supporting gates. Final GREEN/whole-branch author review/exact source/base/tree/proposed-merge evidence belongs on PR255 and issue7 footer only, preserving original2600-character prefix; no docs-only CI repeat. Keep7/102 open; Grep last; user merges/tags/publishes.

## Refined scope before fixture freeze

Initial tests-only source `eae3ceb877c63df9ba37c9962f51f63c04c67a28`, tree `f312812a8a637963e0aae6ce28bbde025e28f8e7`, [run43704205944](https://github.com/uniblab/Icod.Expressions/actions/runs/43704205944), Linux x64 job132781680384, reported144 managed failures/92,869 passes/93,013 total, zero skips/compiler warnings/errors. Every native preflight passed. Fourteen default-mode research profiles hit pre-existing start-study/recovery gaps; the independent dot-star tolerant-UTF profile hit an existing inferred-anchoring gap in both flag modes. Those15 profiles remain permanently recorded in that original tests-only commit/run and in the table below under the wider issue102 work. No production was changed. Default-mode coverage in this increment is restricted to existing proof availability, preserving all281 earlier test files.

The retained NoStartOptimize=true COMMIT/alternative cases at a requested UTF continuation byte belong to the new unfiltered policy: a zero-width COMMIT must be able to cut before consumption fails. Resolve synthetic requested-byte admission from hasCommit when start study is disabled, without changing the default-mode availability boundary. Four UTF optional-class profiles already followed the unoptimized path before this option; retain them as controls. Add Unicode line-separator partial-offset rows to exercise STARTLINE suppression directly. All retained original native expected rows and all assertion/helper code remain unchanged; no new C# blob is frozen until refined RED is clean.

| Deferred research pattern | Mode | NoStartOptimize | Subject/requested byte | Native index | Merged-base index |
| --- | ---: | --- | --- | ---: | ---: |
| `(?s).*A` | 5 | false | `C3A941` at1 | -1 | 2 |
| `(?s).*A` | 5 | true | `C3A941` at1 | -1 | 2 |
| `(*ANY)[\t-\f]*A` | 0 | false | `0D0A0D0A41` at0 | 4 | 3 |
| `(*ANYCRLF)([\t-\f]+)A` | 0 | false | `0D0A0D0A41` at0 | -1 | 3 |
| `(*ANYCRLF)[\t-\f]*A` | 0 | false | `0D0A0D0A41` at0 | 4 | 3 |
| `(*ANY)(*COMMIT)(?:A|B)` | 5 | false | `C3A941` at1 | -1 | 2 |
| `(*ANY)[\t-\f]*A` | 1 | false | `410D0A41` at1 | 2 | 3 |
| `(*ANYCRLF)[\t-\f]+A` | 0 | false | `0D0A0D0A41` at0 | -1 | 3 |
| `(*ANY)[\t-\f]+A` | 0 | false | `0D0A0D0A41` at0 | -1 | 3 |
| `(*ANY)[\t-\f]*A` | 5 | false | `410D0A41` at1 | 2 | 3 |
| `(*COMMIT)(?:A|B)` | 5 | false | `C3A941` at1 | -1 | 2 |
| `(*ANYCRLF)(*COMMIT)(?:A|B)` | 5 | false | `C3A941` at1 | -1 | 2 |
| `(*ANYCRLF)[\t-\f]*A` | 1 | false | `410D0A41` at1 | 2 | 3 |
| `(*ANYCRLF)[\t-\f]*A` | 5 | false | `410D0A41` at1 | 2 | 3 |
| `(*ANY)([\t-\f]+)A` | 0 | false | `0D0A0D0A41` at0 | -1 | 3 |

Mode1 is UTF,4 is tolerant UTF,5 combines them; the table is evidence of deferred default optimizer/independent anchoring availability, not an expected result for the new flag. Native byte offsets remain authoritative.

## Observed refined RED and fixture freeze

Tests-only [RED](https://github.com/uniblab/Icod.Expressions/actions/runs/43705844206), source `16ffee63cd595525faf1bb508e8b0846282fb007`, tree `c578625e6bc73410bb84cce9fd0d843b10a9d367`, Linux x64 job132786612009: **139 managed failures / 92,859 passes / 92,998 total / zero skips/compiler warnings/errors**. All139 failure entries belong to this fixture:133 protocol groups,3 unfiltered-COMMIT budget/cancellation cases and3 compiled source/flag/mark ownership cases. Every failed protocol group reaches Compare after its complete native batch; supplementary failures likewise follow native assertions. No native preflight failed. All92,355 prior tests and504 new controls pass; production is identical to merged254.

Freeze before production: reference `f21b21d5ca1fe4901e2d7fbcc7350006f827e360`; part1 `1f9cccab76cc643654273f18f5811d8dd0fc0673`; part2 `a25523381d8a84b3079f1aab580ab6b3d5269eda`. Preserve all281 prior test files/276 C# sources and frozen35 production blobs. The initial research gaps above remain outside this increment; none of the frozen expectations or helpers may be rewritten to obtain GREEN.
