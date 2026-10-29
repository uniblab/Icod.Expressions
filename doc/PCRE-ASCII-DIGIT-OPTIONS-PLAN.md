# Internal PCRE scoped ASCII digit options implementation plan

> **For agentic workers:** Use superpowers:executing-plans for the parser change,
> with a focused fixture worker and a fresh whole-branch read-only review.

**Goal:** Accept scoped/mixed/local aD settings over the existing option grammar.

**Architecture:** Unify the prefix bypass with transactional header scanning,
then include aD in existing heap group snapshots. Immediate d/D node lowering
retains local policy, so compiler and VM need no state changes.

**Tech Stack:** C#13; net8/net9/net10; xUnit; PCRE.NET1.6.0 / PCRE2 10.48 JIT.

**Spec:** `doc/PCRE-ASCII-DIGIT-OPTIONS-DESIGN.md`.

## Global constraints

- Root project/solution, src source, tests tests; UTF-8 LF; no Python.
- Local Debug; six PR Staging OS/architecture jobs. No local SDK: CI red/green.
- No API/version/dependency, VM/frame/journal, Grep or CommandFramework change.
- Preserve depth256/capture256, compiled-instruction4096, saved-path1024 and
  provisional default match-work1,000,000 policies with cancellation.
- UCP without UTF stays unsupported; only aD of the ASCII option families enters
  this tranche. x/xx and reset rules remain the #82 contract.

## Review focus

1. Every group exit restores caller aD even after branch-local changes.
2. Later alternatives inherit settings while previously parsed classes do not.
3. d/D bracket members change locally; POSIX/properties/word/space do not.
4. Unifying prefix scanning preserves trivia and quantifier source diagnostics.
5. Mixed/reset/duplicate tokens obey exact adjacency and unset precedence.

### Task 1: Establish the reference contract

**Files:** Create `tests/Pcre/PcreAsciiDigitOptionReferenceTests.cs`. Remove only
obsolete aD-pending rows in OptionScope, LeadingOptionCombination, ResetOption
and Utf8Digit fixtures; update current syntax/design/resource/consumer inventories.

**Interfaces:** Existing Compile/Match and options, native Compiled/JIT oracle;
all capture byte spans, independently expected selected outcomes.

- [ ] Add rows/sweeps for all five review risks and design verification classes.
- [ ] Publish a test-only draft PR from merged #82; inspect completed logs for
  expected managed rejection while native expectations pass. Preserve evidence.

### Task 2: Unify option parsing and restoration

**Files:** Modify `src/Pcre/PcreByteParser.cs` and
`src/Pcre/PcreByteParser.Groups.cs`.

**Interfaces:** Existing `TryParseOptions(int,bool)`; add bool AsciiBsD to private
GroupOptions snapshot, construction and restoration; no new external interface.

- [ ] Remove leading aD-only bypass; keep common leading trivia/option loop.
- [ ] Add exact aD token scanning with local set/unset bits and validated apply;
  reset continues to preserve the old policy before explicit changes.
- [ ] Include AsciiBsD in every group entry snapshot/exit restoration.
- [ ] Run whitespace/frozen checks and publish the implementation for green CI.
- [ ] Fresh whole-branch review, resolve concrete findings with focused evidence.
- [ ] Verify exact final remote tree/head, all six successful Staging jobs and
  per-framework full/frozen counts; configured API/consumer/workload gates pass.
- [ ] Finalize PR evidence and mark ready; user retains merge control.
