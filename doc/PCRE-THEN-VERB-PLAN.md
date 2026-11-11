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
- [ ] Publish tests/design/plan draft; observe clean successful-build RED and
freeze all167 new cases before production changes.

## Task 2: implement

Files: Syntax, Parser/Groups/Lookbehind, Compiler, Program, Matcher.
Interfaces: PcreByteThen; parser HasThen; compiler optional hasThen constructor
argument; Then/ThenMark opcodes and Then frame kind; compiler ThenTarget mark/
has-next context; existing StateKind.Mark journal and scope guards.

- [ ] Parse exact THEN, reject direct quantifiers, preserve behind/nullable metadata.
- [ ] Emit bounded stack-depth marks and carry nearest true alternation context;
inherit through one-branch groups/conditionals, reset for assertions/callees.
- [ ] Save clamped lexical target, catch all active assertions/calls, reuse cut
unwind and selected callee-value transactions; preserve last-branch outer retry.
- [ ] Let bounded COMMIT entry proof cross leading THEN; widen no other optimizer.
- [ ] Run unchanged references and all applicable native CI to GREEN.

## Task 3: review/ready

Files: syntax/resource/fixture inventories, roadmap status, issue7 footer.

- [ ] Review all six focus paths and bounds; record exact RED/GREEN evidence.
- [ ] Confirm proposed merge tree, fixed blobs and full qualification; ready PR.
