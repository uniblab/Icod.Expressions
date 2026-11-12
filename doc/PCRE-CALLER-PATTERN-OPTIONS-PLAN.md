# Caller Pattern Options Implementation Plan

Execute inline with the established continuous workflow.
**Goal:** Initialize existing pattern-option parsing from internal caller flags.
**Architecture:** Seven default-false PcreByteOptions bools initialize existing
scoped parser fields. One options argument replaces positional bool plumbing;
all values are copied at construction. Existing nodes/compiler/matcher remain.
**Tech Stack:** C#13/net8/9/10; managed tests through GitHub CI.
**Spec:** [design](PCRE-CALLER-PATTERN-OPTIONS-DESIGN.md).

## Global constraints

Preserve earlier tests, GNU/BRE/ERE, 35 frozen source blobs and 214 frozen outcomes
in 120 methods, exported API and dependencies. UTF-8+LF. No Python, subagents,
local SDK installs or runtime fallback. Work in the existing isolated checkout.
User merges/tags/publishes; Grep stays last.

## Review focus

1. Caller flags seed existing scopes; local unsets/restoration/reset remain native.
2. NoAutoCapture retains named numbering; duplicate names and reference choices hold.
3. Dot/anchors/repeats retain newline/UTF/CRLF candidate and greediness behavior.
4. x/xx lexing preserves quotes/raw labels and original diagnostic bytes.
5. Options are copied, and shared work/cancellation/capture/depth/expansion gates hold.

## Task 1: references and freeze

Create tests/Pcre/PcreCallerPatternOptionReferenceTests.cs. Consume existing
Compile/Match/MarkName and PCRE.NET. Reflection enables tests-first builds;
native flags derive directly from the pinned mask, never from absent properties.
Use 1,046 tests: 988 literal profile rows and 10 distinct compile-error cases researched as
1,018 native profiles, covering all 128 combinations in byte/UTF. Add exact
legacy diagnostics, all-byte and UTF/CRLF offset controls, policy composition,
whole-pattern UTF validation, ownership, work/cancellation and resource checks.
Precompile every native pattern in supplemental loops before managed matching.

- [x] Independently qualify 1,018 native profiles and deduplicate rows.
- [ ] Publish draft tests-only source and observe clean net8/9/10 RED.
  Expected: only missing-caller-option failures; zero warnings/errors/skips;
  default profiles, native literals, exact diagnostics and all earlier tests pass.
- [ ] Freeze the reference hash after checking failure identities and sites.

## Task 2: initialize and qualify

Modify src/Pcre/PcreByteOptions.cs: bool DotAll, Multiline, NoAutoCapture,
Ungreedy, DuplicateNames, Extended and ExtendedMore (all false).
Modify PcreByteParser.cs constructor to
PcreByteParser(byte[] source, PcreByteOptions options,
PcreByteStartSettings settings, CancellationToken cancellationToken).
Copy all existing primitive options, initialize existing five boolean state fields
and choose ExtendedMode.More if ExtendedMore, Extended if Extended, else None.
Retain prefix-derived settings and cancellation. No options reference is stored.
Modify PcreByteCompiler.cs, the sole constructor caller, to pass options once.
No changes to scope/reset/group parsing, bytecode or matcher behavior.

- [ ] Implement only the option/plumbing/state initialization after valid freeze.
- [ ] Qualify all 53,999 tests/framework in 18 native platform/framework results with zero
  failures/skips/build warnings/errors and frozen 214/120 in each.
- [ ] Verify frozen 35, Unicode generators, workload/parity/API/corpus/pack,
  package-only smoke and three-OS scripts; note existing consumer classifier.

## Task 3: review and ready

- [ ] Author-review all five boundaries, old option copies and earlier tests.
- [ ] Verify final head/base/proposed merge tree and qualification receipts.
- [ ] Update syntax/resource/consumer docs, roadmap/issue #7 and PR evidence.
- [ ] Mark ready and remove only this tranche ledger. User owns merge/publication.

## Baseline

PR228 merged at 209dd96c3b30eeadec8dfc431f2ca04ca6192354.
Main 43176216559 and scripts 43176216531 are green; 52,953 tests/framework.
Source 44365071cd96b64a54a266db47fa03b198b9bdb0,
tree 66e3913d5e03b500ded7b08cb3a2e506b271133a.
