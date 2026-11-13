# Never Callout Implementation Plan

Execute inline under the standing continuous workflow.
**Goal:** Reject manual callout tokens under the native caller prohibition.
**Architecture:** A snapshot boolean gates the existing atom dispatch; the
current frame preserves truncated conditional-header priority.
**Tech stack:** C#13/net8/9/10, pinned PCRE2 10.48, CI RED/GREEN.
**Spec:** [design](PCRE-NEVER-CALLOUT-DESIGN.md).

## Global constraints

Merged240 99a7a99baf52fdcb41db73e0dcfa21dd89c68c0f/tree0f7215505e3a92510a15a5511d74baa0e881cc1e;
71,410 tests/framework,256 earlier files. Preserve GNU/BRE/ERE, frozen35/214
outcomes/120methods, AST/IR/opcodes/matcher/journals/shared limits/exported API,
dependencies/workflows. UTF8+LF; no Python/subagents/SDK installs. Reuse linked
worktree; user owns merge/tag/publication; Grep last. Existing unsupported paths
remain explicit. All repository docs precede qualification; final evidence goes
on PR241/issue7 only, without another documentation-only CI run.

## Review focus

1. Recognize real atom tokens without scanning inert quotes, comments, classes or labels.
2. Preserve source UTF/caller-mode and preceding syntax priority while rejecting before malformed callout arguments.
3. Distinguish a truncated conditional precheck from a dispatched leading conditional callout.
4. Caller policy survives local scopes/reset and does not alter canonical matching, source/options ownership or work.
5. Disabled parsing cannot bypass structural/resource limits or cancellation; default callout execution stays unsupported.

## Task 1: regression contract

**Files:** Create tests/Pcre/PcreNeverCalloutReferenceTests.cs.
**Interfaces:** Optional reflection sets future NeverCallout for executable
RED. Native factory uses extra0x8000 and independent literal errors/spans/labels.

- [x] Preflight native profiles and selected offsets; classify accepted JIT0,
  no-match(-1), native/source errors and deliberate existing internal boundaries.
- [x] Add guard/default/inert controls plus five review boundaries, ownership,
  shared work/cancellation and capture/depth/expansion quota checks.
- [x] Publish tests-only draft PR241, inspect clean net8/9/10 RED with zero
  compiler warnings/errors and only missing-policy failures; freeze the fixture.

## Task 2: parser integration

**Files:** Modify src/Pcre/PcreByteOptions.cs, src/Pcre/PcreByteParser.cs and
src/Pcre/PcreByteParser.Groups.cs; update seven repository doc files.
**Interfaces:** bool NeverCallout init, readonly parser neverCallout snapshot.
ParseAlternatives uses existing current.Quoting/WaitingForCondition/index/source
and Invalid(int offset,string message); no new functions/types.

- [x] Add the option/snapshot. At atom dispatch reject `(?C` only when unquoted
  and not a truncated leading condition (remaining source >=4 for conditions).
- [ ] Include design/matrix/resource/consumer/status inventories before the
  final qualified head. Verify all18 native results and configured supporting
  gates, earlier256 hashes and frozen fixture/source/result identities.

## Task 3: review and delivery

- [ ] Review all five boundaries and whole branch inline; no independent agents.
- [ ] Verify clean tree and exact source/base/tree/proposed-merge identity.
- [ ] Record GREEN/review on PR241 and only issue7's footer; mark ready and
  remove only this plan's ledger. User owns merge/tag/publication.

## Inline progress before qualification

Refined tests-only [RED731](https://github.com/uniblab/Icod.Expressions/actions/runs/43425307698), source `3306ed9677a09d35e3f2cbf7f03cf72e850a5cf5`, Linux ARM job131885343849: net8/9/10 each built without compiler warnings/errors, then ran716 expected failures/72,141 passes/72,857total/zero skips. Only715 new guard diagnostics and one capacity fact failed; all71,410 earlier tests and731 new controls passed. Fixture `a95af4ea2353748653500573cfd680b8a5984ccd` froze before production and remains unchanged. Initial unfrozen RED730 had718 failures because two byte-range offsets incorrectly used the UTF opening convention; correcting only those offsets preserved every case and native expectation.

Task1 and the Task2 parser integration are complete. All seven repository documentation files precede qualification. The all18 GREEN gate and Task3 review/identities/footer/readiness are intentionally recorded on PR241/issue7 after observation, without a documentation-only source push.
