# SKIP implementation plan

> Execute inline with tests-first, immutable native-reference fixtures.

**Goal:** Native-compatible no-argument SKIP root restart and scoped cuts.
**Architecture:** reuse PRUNE guards/journal; one leaf/opcode/frame and candidate
restart integer. **Tech:** C#13/net8/9/10.
**Spec:** [SKIP design](PCRE-SKIP-VERB-DESIGN.md).

## Global constraints

No public/dependency/production fallback changes. Preserve GNU/BRE/ERE,
frozen35/frozen214 and prior fixed reference blobs. No Python, installations
or subagents. User owns merge/tag/publication.

## Review focus

- K must not change the actual-candidate comparison for restart.
- Equal/backward/EOF targets must terminate and preserve nullable matches.
- Forward CRLF restart must bypass normal bumpalong and repeat recovery.
- Scope catches/atomic pruning and selected callee values must match PRUNE.
- Mixed SKIP/PRUNE frames must use the same bounded state/work budget.

### Task 1: qualify and freeze tests

**Files:** new `tests/Pcre/PcreSkipVerbReferenceTests.cs`; retarget three unfrozen
pending-SKIP rows in `PcreSyntaxAcceptanceTests` to native-valid COMMIT forms.
**Interface:** existing compiler/options/match results and PCRE.NET.

- [x] Export independent native literal spans, including PRUNE comparison controls.
- [x] Add all-offset, exact grammar, mixed/shared frame and resource cases.
- [x] Publish tests/design/plan draft; observe successful-build RED for missing
SKIP only and freeze the new reference test blob.

### Task 2: implement and verify

**Files:** Syntax, Groups/Lookbehind, Program and Matcher.
**Interface:** `PcreByteSkip`, `Skip` opcode/frame and per-candidate restart.

- [ ] Parse/lower exact SKIP; reject direct repeats and preserve width/nullable metadata.
- [ ] Reuse scope target selection, journal/filtering and frame discard semantics.
- [ ] At root only, resume strictly forward targets directly at candidate entry;
use existing normal advancement otherwise. Preserve scalar/fragment barriers.
- [ ] Add the bounded root literal-PLUS/PRUNE/single suffix and same-literal
fallback memo proof required by the frozen PRUNE comparison control; use two
constant-time memo instructions without saved frames.
- [ ] Run unchanged reference tests and full applicable native CI to GREEN.

### Task 3: review and report

**Files:** syntax/resource/fixture inventories, roadmap status and issue7 footer.

- [ ] Review all five focus paths, bounds, offsets and native backend distinctions.
- [ ] Record exact RED/GREEN evidence and remaining gates; verify proposed merge
source tree and mark PR ready after checks. User merges/tags/publishes.

## Tests-first checkpoint

RED650 run43016405038 LinuxARM64 job130508489235 compiled all frameworks with
zero warnings/errors. Each reported46497 total:129 new failures,46368 passes
and0 skips.128 failures are missing SKIP; one new PRUNE comparison control
exposes native JIT repeat-cache parity (native NO_START_OPT probe removes that
result, confirming its optimizer origin). No prior test or native oracle failed.
All140 new cases freeze at blob`092823b6c667859899f16ea07f0dd52e1a499b6b`.
