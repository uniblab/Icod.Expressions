# Caller Compile Restrictions Implementation Plan

Execute inline under the standing continuous workflow.
**Goal:** Honor native NeverUtf/NeverUcp/NeverBackslashC caller restrictions.
**Architecture:** Add default-false internal flags, stage UTF/UCP validation at
existing compiler boundaries, and gate canonical C tokens in the parser.
**Tech stack:** C#13/net8/9/10; native research locally, managed RED/GREEN in CI.
**Spec:** [design](PCRE-CALLER-COMPILE-LOCKOUTS-DESIGN.md).

## Global constraints

Preserve all248 earlier tests,35 frozen blobs/214 outcomes/120 methods,
GNU/BRE/ERE, matcher/IR/API/dependencies/workflows and existing quotas.
UTF8+LF; no Python/subagents/SDK installs. Reuse the isolated worktree;
user owns merge/tag/publication; Grep last. Baseline is merged PR234,
ebdc4d9712f342b959878d3798073bf21d60d076,62,371 tests/framework.

## Review focus

1. Implicit tolerant UTF and simultaneous restrictions retain native precedence.
2. Prefix-end and owning-escape diagnostics retain original byte positions.
3. Quotes/comments/control operands/labels cannot become prohibited C tokens.
4. DEFINE/zero repeats/failed tails and late unresolved names cannot bypass policy.
5. Allowed patterns keep source ownership, cancellation, work and structural quotas.

## Task 1: native contracts and expected RED

Create tests/Pcre/PcreCallerCompileLockoutReferenceTests.cs with independently
pinned native rejection and accepted span/label profiles, all restriction/mode
combinations, UTF-invalid and prior class controls. Native error code/end-offset
checks run before managed checks. The option builder uses optional reflection
until the production flags exist, so RED tests exercise missing policy behavior
rather than fail to build. Add every-octet and shared-resource/ownership controls.

- [x] Preflight native profiles and supplemental loop arrays; pin expected count.
- [x] Publish tests-only draft and inspect net8/9/10 RED: only expected policy
  failures; earlier tests and native controls pass. Freeze final fixture hash.

## Task 2: compiler/parser policy and full GREEN

- [x] Add bool NeverUtf, NeverUcp, NeverBackslashC init properties in
  src/Pcre/PcreByteOptions.cs; default false.
- [x] In src/Pcre/PcreByteCompiler.cs, reject settings.Utf8 && options.NeverUtf
  before UTF validation, and settings.Ucp && options.NeverUcp after it. Use
  InvalidSyntax/settings.BodyOffset and specific caller-disabled messages.
- [x] In src/Pcre/PcreByteParser.cs snapshot NeverBackslashC in a readonly bool;
  check it in ParseEscape C after inBracket rejection, before UTF Unsupported.
  Use InvalidSyntax at the escape opening. No source search or matcher change.
- [ ] Full18 native results: expected test count/frozen214, zero failures/skips/
  compiler warnings/errors. Existing Unicode/workload/extraction/POSIX/API/corpus/
  frozen35/package/three smoke and three-OS script gates pass.

## Task 3: review and delivery

- [x] Author review of all five boundaries; independent review excluded by user.
- [ ] Update design/matrix/resource/consumer/status inventories and issue7 footer.
- [ ] Verify fixture/earlier hashes, clean tree, source/base/proposed merge identity;
  mark ready after checks and remove only this tranche ledger. User merges.

Native inventory:2,499 pinned profiles (1,821 policy/UTF rejections,120 prior
class rejections,558 accepted JIT profiles);3,072 every-octet preflight profiles
and6 shared-resource/quota native controls pass.2,513 new managed cases produce
expected64,884/framework. All1,653 caller-policy profiles and4 octet theories
should fail on the unchanged baseline; expected RED1,657/63,227passes/framework.
Other unsupported/native-only compile profiles remain outside this increment.

Tests-only706 built cleanly but exposed legacy fixture assumptions:60 Perl
algebra controls/framework use the existing expression-opening offset,9 native
erased UTF-C successes remain unsupported here, and1 native consumed UTF-C
behind is error136 even without JIT. These controls were corrected before
freeze; all1,653 caller-policy profiles remain unchanged, as do248 earlier files
and all production source. Expected refined RED remains1,657/framework.

An explicit erased UTF-C boundary control is retained in the supplementary
theory, alongside bare and consumed-behind C; total2,513 new cases.

## Execution record

Valid RED708/run43369721925, LinuxARM job131702547003, source
1b50d1a6d4e5c0950c45d83a3d6d5d06b5684b5d: each net8/9/10 has
64,884total/1,657expectedfail/63,227pass/0skip/0buildwarnings/errors.
All failures are1,653 primary caller-policy profiles and4 octet theories; native
assertions, legacy/UTF/resource/ownership controls and248 earlier files pass.
Freeze68cd3d1c943c27bfd82ac7d67a3968ae80c17db8 before source changes.

The planned three-file implementation is18 added lines. Author review covers all
five boundaries; no new unresolved findings. Execution evidence for the remaining
GREEN/delivery gates, including the final reviewed head and merge tree, is kept
on [PR235](https://github.com/uniblab/Icod.Expressions/pull/235) and issue7.
