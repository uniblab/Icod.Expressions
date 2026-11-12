# Anchor Compile Options Implementation Plan

Execute inline under the established continuous workflow.
**Goal:** Add native caller DollarEndOnly and AltCircumflex through shared anchors.
**Architecture:** Shared parser helpers lower dollar to absolute end or existing
line/end-before-newline nodes. One init property on LineAnchor lowers into the
existing StartLine operand; only its selected endpoint bound changes.
**Tech Stack:** C#13/net8/9/10; managed RED/GREEN through GitHub CI.
**Spec:** [design](PCRE-ANCHOR-COMPILE-OPTIONS-DESIGN.md).

## Global constraints

Earlier tests,35 frozen blobs and214 outcomes/120 methods, GNU/BRE/ERE and
API/dependencies stay fixed. UTF-8+LF; no Python, subagents or local SDK installs.
Existing isolated worktree; user merges/tags/publishes; Grep last.

## Review focus

1. Global flags compose with local m/unsets/reset and MatchLine entry guards.
2. Final LF/CR/CRLF/ANY/NUL/Unicode newlines preserve exact endpoint semantics.
3. Assertions/reverse/end limits and malformed-UTF fragments keep native context.
4. Original source diagnostics, capture/call/cut scopes and owned settings hold.
5. Search candidates, work/cancellation and existing quotas stay shared/bounded.

## Task 1: tests-first and freeze

Create tests/Pcre/PcreAnchorCompileOptionReferenceTests.cs using Compile/Match/
MarkName and pinned PCRE.NET. Reflect only missing new flags; native selection
bits are derived independently. Add primary JIT literals plus every-octet and
selected UTF/CRLF offsets, diagnostics, prior policies, ownership and resource
controls. Supplemental native arrays precompile before managed comparisons.

- [x] Research and pin native PCRE2 10.48/JIT profiles.
- [x] Publish tests-only draft and inspect clean net8/9/10 expected RED.
- [x] Defaults, legacy fixtures, source diagnostics and native oracles must pass.
- [x] Freeze the new reference before production edits.

## Task 2: lower canonical anchors and qualify

Modify PcreByteOptions.cs: two default-false flags. PcreByteParser.cs copies them
readonly and ParseAtom calls new partial PcreByteParser.Anchors.cs helpers
CreateStartAnchor(bool multiline)/CreateEndAnchor(bool multiline). Update
PcreByteParser.WordLineModes.cs to reuse these helpers with entry.Multiline.
PcreByteSyntax.cs adds default-false AllowSubjectEnd to existing LineAnchor.
PcreByteProgram.cs emits First=1 only for selected multiline start anchors.
PcreByteMatcher.cs StartLine retains all checks and permits the selected subject
endpoint when First=1, including native physical-EOF retry after a split Unicode
newline. Keep nonterminal/default partial-newline candidate exclusions. No new
runtime flag/opcode, source rewrite or width rules.

- [x] Implement after valid freeze.
- [ ] Qualify all tests in18 native results with zero failures/skips/warnings/errors.
- [ ] Verify frozen214/120 and35, Unicode generators, workload/parity/API/corpus,
  packaging/package-only smoke and three-OS scripts; note consumer classifier.

## Task 3: author review and readiness

- [ ] Review five boundaries, default operands and unchanged earlier fixtures.
- [ ] Confirm head/base/proposed merge tree and complete qualification receipts.
- [ ] Update syntax/resource/consumer/status docs, issue7 footer and PR evidence.
- [ ] Mark ready and remove only this tranche ledger; user owns merge/publication.

## Baseline

PR231 merged at a897e8a5663850f0c3b9fd2da2968321ed0f2989, tree
46fc7ff3a5967f3e406710b20ec35aedbaf35920. Main43219511682 and scripts43219511702
are green;57,630 tests/framework. All245 earlier test files remain fixed.

Task1 inventory:2,052 new cases:1,984 pinned native JIT profiles,40 original
diagnostics,8 every-octet,8 selected UTF-fragment/offset,3 prior-policy,3 whole-UTF,
3 ownership and3 canonical work/cancel/quota controls. Expected59,682/framework.
Additional72 literal profiles and the offset loop pin variable-lookbehind
reverse endpoint behavior. Native flag-off research is repeated below. All supplemental native arrays compile first.

## Valid RED and freeze receipt

Expanded tests-only source2d166f0ecd5c6f4b8b0047b2007fbd99ccf11d61,
tree80aa5beb49ef09951cf212b484967e35e349cccf, retained merged PR231 production.
CI695/run43228870315/Linux ARM64 job131219356070: each net8/9/10 had59,682 tests,
245 expected option failures,59,437 passes, zero skips and zero warnings/errors.
Primary230 exactly match native flag-off research. Defaults, original diagnostics,
UTF validation, canonical work/cancel/limits, earlier fixtures and native oracles
passed. Freeze2,052-case blob a3e651d589d160f68c574911733e70ad73e4baca
before production. Initial694 was cancelled before freeze to add72 variable-
lookbehind profiles and three offset-loop patterns. No fixture edits after freeze.

## Runtime endpoint correction

CI696/source11ce5fca1895033920bc694392084202c5ba9173 built cleanly and passed
59,678/59,682 per framework. Four frozen offset-loop cases expose native physical-
EOF retry after starting inside final LS/PS. Native fast_forward_newline lastchar
jumps to an EOF candidate; alternate OP_CIRCM may accept backward context there.
Allow only StartLine.First=1 at input.Length to bypass excludedLineStart, retaining
all nonterminal/default exclusions and other checks. Existing CI696 failures are
RED for this correction; frozen reference a3e651d589d160f68c574911733e70ad73e4baca
is unchanged. Final full-suite qualification must include the correction.
