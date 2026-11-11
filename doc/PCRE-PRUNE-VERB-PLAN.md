# PRUNE implementation plan

> Execute inline, tests-first, preserving the immutable native-oracle contract.

**Goal:** No-argument PRUNE with native scope catches and capture transactions.
**Architecture:** leaf/opcode/saved frame; existing guard/journal/candidate loops.
**Tech:** C#13/net8/9/10. **Spec:** [PRUNE design](PCRE-PRUNE-VERB-DESIGN.md).

## Global constraints

No public/dependency/fallback changes; frozen35 and214 results/120 methods intact;
no Python, SDK/PowerShell installation or subagents. User owns merge/tag/publish.

## Review focus

- Retained inactive call/assertion guards must not intercept a later PRUNE.
- Nearest negative/conditional assertion must catch before an enclosing call.
- Accepted atomic/assertion scopes must discard interior PRUNE frames.
- Callee PRUNE must preserve only native selected pre-verb values on forced fail.
- Candidate bumpalong, scalar barriers and resource limits remain bounded.

### Task 1: qualify and freeze tests

**Files:** new `tests/Pcre/PcrePruneVerbReferenceTests.cs`; retarget the unfrozen
pending PRUNE row in `PcreSyntaxAcceptanceTests` to still-unsupported SKIP.
**Interface:** existing compiler/options/match/capture results and PCRE.NET.

- [x] Pin native literal spans, byte/UTF profiles and callee returned-value edges.
- [x] Add offset, grammar, saved-frame, compile/work/cancellation cases.
- [x] Publish draft tests/design/plan; observe successful-build RED caused only
by missing PRUNE support. Freeze the new test blob.

### Task 2: implement and verify

**Files:** Syntax, parser Groups/Lookbehind, Program and Matcher.
**Interface:** `PcreBytePrune`; `Prune` opcode/frame; assertion arena guard kind.

- [ ] Parse exact header; reject direct repetition and preserve zero-width scans.
- [ ] Emit one instruction and retain nullable/metadata behavior conservatively.
- [ ] Push a bounded scope-selected frame. On trigger, charge discard work,
retain native selected callee values, then use existing guard rollback/failure.
- [ ] Run unchanged reference tests and complete native/framework CI to GREEN.

### Task 3: review and publish evidence

**Files:** syntax/resource/fixture inventories, roadmap status and issue7 footer.

- [ ] Review all five focus paths against native source and final diff.
- [ ] Document exact RED/GREEN source/tree, native limitations and remaining gates.
- [ ] Verify exact proposed merge tree; mark PR ready after all applicable checks.

## Tests-first checkpoint

RED648 run43010950288 compiled net8/9/10 without warnings/errors. Linuxx64
job130490125587 reported46357 total per framework:83 expected new PRUNE failures,
46274 passes and0 skips; no unrelated or native-oracle failures. All91 additions
freeze at test blob`49bc810efdb1830b7bd49e52e6392f76b1c0bebd`.
