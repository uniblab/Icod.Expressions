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

- [ ] Preflight native profiles and selected offsets; classify accepted JIT0,
  no-match(-1), native/source errors and deliberate existing internal boundaries.
- [ ] Add guard/default/inert controls plus five review boundaries, ownership,
  shared work/cancellation and capture/depth/expansion quota checks.
- [ ] Publish tests-only draft PR241, inspect clean net8/9/10 RED with zero
  compiler warnings/errors and only missing-policy failures; freeze the fixture.

## Task 2: parser integration

**Files:** Modify src/Pcre/PcreByteOptions.cs, src/Pcre/PcreByteParser.cs and
src/Pcre/PcreByteParser.Groups.cs; update seven repository doc files.
**Interfaces:** bool NeverCallout init, readonly parser neverCallout snapshot.
ParseAlternatives uses existing current.Quoting/WaitingForCondition/index/source
and Invalid(int offset,string message); no new functions/types.

- [ ] Add the option/snapshot. At atom dispatch reject `(?C` only when unquoted
  and not a truncated leading condition (remaining source >=4 for conditions).
- [ ] Include design/matrix/resource/consumer/status inventories before the
  final qualified head. Verify all18 native results and configured supporting
  gates, earlier256 hashes and frozen fixture/source/result identities.

## Task 3: review and delivery

- [ ] Review all five boundaries and whole branch inline; no independent agents.
- [ ] Verify clean tree and exact source/base/tree/proposed-merge identity.
- [ ] Record GREEN/review on PR241 and only issue7's footer; mark ready and
  remove only this plan's ledger. User owns merge/tag/publication.
