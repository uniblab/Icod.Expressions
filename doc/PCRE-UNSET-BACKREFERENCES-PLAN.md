# Unset Backreferences Implementation Plan

Execute inline under the established continuous workflow.
**Goal:** Add native optional empty matching for existing unset capture slots.
**Architecture:** Annotate existing Backreference nodes after name validation;
lower one default-false property into existing Backreference.Second. Reuse slot
lookup, consuming comparison, journal and width dependency traversal.
**Tech Stack:** C#13/net8/9/10; managed RED/GREEN through GitHub CI.
**Spec:** [design](PCRE-UNSET-BACKREFERENCES-DESIGN.md).

## Global constraints

Earlier tests,35 frozen blobs/214 outcomes/120 methods, GNU/BRE/ERE and API/
dependencies stay fixed. UTF-8+LF; no Python, subagents or SDK installs. Existing
isolated worktree; user merges/tags/publishes; Grep last. Keep existing semantic
diagnostic priority for malformed/undefined references; the option creates no group.

## Review focus

1. All-unset succeeds empty without participating; actual empty slots win lookup.
2. Every numeric/named/forward/self form shares policy; missing groups still error.
3. Repeats, assertions, calls/returns, cuts and rollback retain capture conditions.
4. Native consumed-behind rejection reaches zero repeats/calls, but respects skips.
5. UTF/CRLF offsets, ownership and zero-width work/cancellation/quotas stay bounded.

## Task 1: references, expected RED and freeze

Create tests/Pcre/PcreUnsetBackreferenceReferenceTests.cs using Compile/Match/
MarkName and pinned PCRE.NET. Reflect only the missing new option; native flag
0x200 derives independently from the test selection. Pin literal profiles and
selected width errors, all octets, selected UTF offsets, prior policies, full
capture capacity, ownership, diagnostics and shared resource controls. Precompile
supplemental native arrays before managed comparisons.

- [x] Qualify native JIT profiles and exact width errors.
- [ ] Publish tests-only draft and observe clean net8/9/10 expected RED.
- [ ] Default, prior fixtures, exact legacy diagnostics and native oracles must pass.
- [ ] Freeze all new reference hashes before production changes.

## Task 2: annotate and execute through existing paths

PcreByteOptions.cs adds default-false MatchUnsetBackreference. PcreByteParser.cs
copies it readonly and calls new PcreByteParser.UnsetBackreferences.cs helper
EnableUnsetBackreferences(PcreByteAlternatives root) after ResolveNamedReferences
and before condition/call/width/final-target resolution. Helper uses existing
RewriteTree, setting Backreference.UnsetMatchesEmpty via with. Defaults skip it.
PcreByteSyntax.cs adds that init property. PcreByteProgram.cs lowers it into Second.
PcreByteMatcher.cs keeps duplicate participation lookup; null capture advances pc
only for Second=1, otherwise fails. No capture mutation, source read or new path
for participating captures. Lookbehind.cs ReferenceTarget rejects selected nodes
with InvalidSyntax at opening before ordinary dependency checks, reusing all skips.

- [ ] Implement after valid freeze.
- [ ] Qualify every test in18 native results at zero failures/skips/warnings/errors.
- [ ] Verify frozen35/214/120, Unicode generators, workload/parity/API/corpus,
  packaging/package-only smoke and three-OS scripts; note consumer classifier.

## Task 3: author review and readiness

- [ ] Review five boundaries, default root/operand and unchanged earlier fixtures.
- [ ] Verify exact head/base/proposed merge tree and qualification receipts.
- [ ] Update syntax/resource/consumer/status docs, issue7 footer and PR evidence.
- [ ] Mark ready and remove only this tranche ledger; user owns merge/publication.

## Baseline

PR232 merged at49037aef15cce24e7b6c79df7e6f04a928eced1f,
tree636003cf504f4c105e15006cd4f1521067f1f8b5. Main43236233339 and
scripts43236233142 are green;59,682 tests/framework. All246 earlier files stay fixed.

Task1 inventory:1,740 cases include1,652 valid pinned JIT profiles,32 selected
lookbehind width errors,40 legacy diagnostics,4 all-octet,4 UTF-offset,2 prior-
policy,2 UTF-validation,ownership/full-capacity facts and2 work/resource facts.
Expected61,422/framework. Native flag-off research predicts454 primary semantic
differences and zero default differences. Supplemental native arrays compile first.
