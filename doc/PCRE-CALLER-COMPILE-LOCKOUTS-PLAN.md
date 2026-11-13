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
- [ ] Publish tests-only draft and inspect net8/9/10 RED: only expected policy
  failures; earlier tests and native controls pass. Freeze final fixture hash.

## Task 2: compiler/parser policy and full GREEN

- [ ] Add bool NeverUtf, NeverUcp, NeverBackslashC init properties in
  src/Pcre/PcreByteOptions.cs; default false.
- [ ] In src/Pcre/PcreByteCompiler.cs, reject settings.Utf8 && options.NeverUtf
  before UTF validation, and settings.Ucp && options.NeverUcp after it. Use
  InvalidSyntax/settings.BodyOffset and specific caller-disabled messages.
- [ ] In src/Pcre/PcreByteParser.cs snapshot NeverBackslashC in a readonly bool;
  check it in ParseEscape C after inBracket rejection, before UTF Unsupported.
  Use InvalidSyntax at the escape opening. No source search or matcher change.
- [ ] Full18 native results: expected test count/frozen214, zero failures/skips/
  compiler warnings/errors. Existing Unicode/workload/extraction/POSIX/API/corpus/
  frozen35/package/three smoke and three-OS script gates pass.

## Task 3: review and delivery

- [ ] Author review of all five boundaries; independent review excluded by user.
- [ ] Update design/matrix/resource/consumer/status inventories and issue7 footer.
- [ ] Verify fixture/earlier hashes, clean tree, source/base/proposed merge identity;
  mark ready after checks and remove only this tranche ledger. User merges.

Native inventory:2,508 pinned profiles (1,821 policy/UTF rejections,120 prior
class rejections,567 accepted JIT profiles);3,072 every-octet preflight profiles
and6 shared-resource/quota native controls pass.2,521 new managed cases produce
expected64,892/framework. All1,653 caller-policy profiles and4 octet theories
should fail on the unchanged baseline; expected RED1,657/63,235passes/framework.
Other unsupported/native-only compile profiles remain outside this increment.
