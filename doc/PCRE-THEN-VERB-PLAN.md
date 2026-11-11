# THEN implementation plan

Execute inline with immutable tests-first references.
**Goal:** Exact no-argument THEN with lexical sibling retry and scoped cuts.
**Architecture:** Compiler-owned journaled stack-depth marks, existing delayed
cut frames/rollback, all assertion/call catches. **Tech:** C#13/net8/9/10.
**Spec:** [THEN design](PCRE-THEN-VERB-DESIGN.md).

## Constraints and review focus

Preserve prior references, GNU/BRE/ERE, frozen35/frozen214/120, API and dependency
contracts. No Python, installation, production fallback or subagents. User merges.
Review nearest real alternation (not repeat Split or conditional arms), last-
branch outer retries, assertion/callee ownership resets, recursive/returned mark
rollback, rightmost cuts and shared resource/cancellation bounds.

## Task 1: qualify/freeze

Files: new PcreThenVerbReferenceTests; five obsolete unfrozen pending-THEN rows.
Interface: existing compiler/options/matcher and PCRE.NET/native PCRE2 oracle.

- [x] Independently export149 unique literal fixtures in byte/UTF profiles.
- [x] Add all-offset, grammar, shared frame/mark/compile/work/cancel cases.
- [x] Publish tests/design/plan draft; observe clean successful-build RED and
freeze all167 new cases before production changes.

## Task 2: implement

Files: Syntax, Parser/Groups/Lookbehind, Compiler, Program, Matcher.
Interfaces: PcreByteThen; parser HasThen; compiler optional hasThen constructor
argument; Then/ThenMark opcodes and Then frame kind; compiler ThenTarget mark/
has-next context; existing StateKind.Mark journal and scope guards.

- [x] Parse exact THEN, reject direct quantifiers, preserve behind/nullable metadata.
- [x] Emit bounded stack-depth marks and carry nearest true alternation context;
inherit through one-branch groups/conditionals, reset for assertions/callees.
- [x] Save clamped lexical target, catch all active assertions/calls, reuse cut
unwind and selected callee-value transactions; preserve last-branch outer retry.
- [x] Let bounded COMMIT entry proof cross leading THEN; widen no other optimizer.
- [ ] Run unchanged references and all applicable native CI to GREEN.

## Task 3: review/ready

Files: syntax/resource/fixture inventories, roadmap status, issue7 footer.

- [ ] Review all six focus paths and bounds; record exact RED/GREEN evidence.
- [ ] Confirm proposed merge tree, fixed blobs and full qualification; ready PR.

## Tests-first checkpoint

RED656 run43031808664/LinuxARM64 job130560487618 built net8/9/10 with zero
warnings/errors. Each reported46814 cases:159 expected missing-THEN failures,
46655 passes,0 skips. No earlier fixture or native literal oracle failed.
All167 new cases freeze at blob`c60d5da76744c4de93d54ffb6be8103c96538052`
before production changes. Independent native probes also qualify all eight
plain/mixed mark/frame boundary subjects.

The final capture qualification distinguishes lexical traps from forced call
quit even when their numeric target depths coincide: owned THEN uses ordinary
rollback; unowned call cuts retain selected pre-verb values. Native JIT also
uses call-local quit for unowned THEN inside a callee atomic positive assertion.
Outside calls and in negative/conditional/non-atomic assertions, catches remain
local. NO_JIT probes show the interpreter distinction. The saved-frame kind
records ownership without another arena or saved frame.

CI657 narrowed the original167 cases to one selected-capture mismatch. A
separate eight-row PcreThenReturnedCaptureReferenceTests file then exposed five
additional owned-trap rollback failures in clean RED658 (run43033445743,
LinuxARM64 job130566254506):46822 total,6 failures,46816 passes,0 skips and zero
warnings/errors on net8/9/10. Original blob c60d5da76744c4de93d54ffb6be8103c96538052
stays fixed; the eight regressions freeze at4bdcbd6448fb873e29f3ad5dfdfe3679187074f4.
All175 new cases are now fixed before the production capture correction.
