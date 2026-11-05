# Recent UTF proof progress

Continues [issue102](https://github.com/uniblab/Icod.Expressions/issues/102).
Its description reached GitHub's body limit. Earlier history remains there;
this index preserves the recent links and gives subsequent tranches room to grow.

The counts describe each tranche's committed test corpus. Open the linked PR
for its current review status, native checks, exact source commit and final CI
audit. A row is not a claim that an unreviewed PR is merged or release-ready.

| PR | New cases | Full suite | Scope |
| --- | ---: | ---: | --- |
| [147](https://github.com/uniblab/Icod.Expressions/pull/147) | 950 | 17256 | Finite and empty nullable-group continuation |
| [148](https://github.com/uniblab/Icod.Expressions/pull/148) | 913 | 18169 | Nullable alternative-group continuation |
| [149](https://github.com/uniblab/Icod.Expressions/pull/149) | 1203 | 19372 | Finite group-count study |
| [150](https://github.com/uniblab/Icod.Expressions/pull/150) | 1117 | 20489 | Guarded positive group counts |
| [151](https://github.com/uniblab/Icod.Expressions/pull/151) | 1443 | 21932 | Unbounded nullable group loops |
| [152](https://github.com/uniblab/Icod.Expressions/pull/152) | 1414 | 23346 | Word-boundary PLUS proofs and first required-copy ownership |
| [153](https://github.com/uniblab/Icod.Expressions/pull/153) | 179 | 23525 | Boundaries before general starter classes |
| [154](https://github.com/uniblab/Icod.Expressions/pull/154) | 199 | 23724 | Erased primitives before general starter classes |
| [155](https://github.com/uniblab/Icod.Expressions/pull/155) | 223 | 23947 | Retained zero groups/assertions before general starter classes |
| [156](https://github.com/uniblab/Icod.Expressions/pull/156) | 377 | 24324 | Finite optional primitives and proven root stars before general starter classes |
| [157](https://github.com/uniblab/Icod.Expressions/pull/157) | 297 | 24621 | Bitmap-skipped negative lookahead and lookbehind prefixes |
| [158](https://github.com/uniblab/Icod.Expressions/pull/158) | 752 | 25373 | Exact assertion copies and optional negative-ahead class bodies |
| [161](https://github.com/uniblab/Icod.Expressions/pull/161) | 1092 | 26465 | Mandatory bitmap heads in optional negative lookahead and finite scoped groups |
| [162](https://github.com/uniblab/Icod.Expressions/pull/162) | 817 | 27282 | Mandatory heads across optional group alternatives and nested bodies |
| [163](https://github.com/uniblab/Icod.Expressions/pull/163) | 987 | 28269 | Mandatory heads in finite optional numbered and named captures |
| [164](https://github.com/uniblab/Icod.Expressions/pull/164) | 1364 | 29633 | Atomic/skipped bracket prefixes and equivalent negative-count exits |
| [165](https://github.com/uniblab/Icod.Expressions/pull/165) | 432 | 30065 | Positive ahead through skipped assertion and boundary prefixes |

Before a tranche is ready: six native OS/architecture Staging jobs, full suite
and frozen214 on net8/9/10, no failed/skipped tests or compiler warnings/errors,
all configured gates, seven equal component workloads, immutable tests and
frozen35 source checks. Full source-pinned Compatibility has its own configured
path gate; a skipped job is not a completed full qualification.

## Preserved issue102 footer

Copied verbatim before replacing only that footer with a link to this index:

```markdown


## Recent progress

[147](https://github.com/uniblab/Icod.Expressions/pull/147); [148](https://github.com/uniblab/Icod.Expressions/pull/148); [149](https://github.com/uniblab/Icod.Expressions/pull/149); [150](https://github.com/uniblab/Icod.Expressions/pull/150); [151](https://github.com/uniblab/Icod.Expressions/pull/151); [152](https://github.com/uniblab/Icod.Expressions/pull/152); [153](https://github.com/uniblab/Icod.Expressions/pull/153).

Six native jobs/net8/9/10; full/frozen214; zero test failures/skips/build warnings/errors. Audits/scopes/counts: PRs; #102 open.
```

Issue102 remains open for wider bitmap availability and unsupported placements;
unsafe UTF100 remains separate. No full PCRE, release or Grep readiness claim.
Publication and all known consumer migrations precede CommandFramework removal;
Grep remains last.
