# Mandatory heads in optional capturing groups

Continue #7/#102 after merged PR162 (full27282/frozen214). Extend finite optional
group bitmap proof to numbered/named capturing outer groups. Every branch must
reach a known mandatory literal/type/positive predicate-free range head excluding
CR, through the existing capture/noncapture/atomic wrapper worklist. Runtime
capture numbering, last-iteration values, unset/rollback state, backreferences,
conditionals, body tails and option restoration remain with the existing engine.

## Approach

Widen HasUtfCrLfOptionalGroupBitmap's body selector to PcreByteGroup and
PcreByteNoncapturingGroup. Reuse HasUtfCrLfMandatoryBodyBitmap; a separate capture
walker would duplicate a proof already aware of nested captures. Changing capture
runtime machinery is unnecessary for bitmap selection. This is an implementation
choice under the standing instruction to continue without approval pauses.

Keep the existing noncapturing single-leaf path and charges exact. New capturing
paths use the existing local visit checkpoint and commit it only on success;
failed speculation must not spend the prior nullable fallback budget. No capture
references are retained in VM state by this proof. Native study skips the CBRA
capture-number operand before entering the same bracket-body analysis (pinned
pcre2_study.c, set_start_bits); capture references still have their existing
early-fail effects. Retained=true blocks later PLUS recovery; first-star and
capture-ancestor classification remain unchanged and are tested with references.

The caller retains Minimum0/Maximum1..499 before study/multiply,2*Maximum+1 plus
wrapper/pre-addition checks, shared visits/cancellation and exact/zero rules.
No nullable/repeated/prefix-assertion heads, empty alternatives, unbounded groups
or general tail-return inference. Unsupported availability stays ordinary-only.

## Verification and constraints

C#13/net8/9/10; root solution/project,src/tests/samples,doc; UTF8+LF. No subagents,
Python,SDK/PowerShell installation. No parser/emitter/owner-finder/matcher/Step/
VM/frame/journal/API/workflow/contract/dependency/consumer/CommandFramework changes
or production reference fallback. No spans/interior pointers in backtracking
frames. Limits remain parser256/captures256/instructions4096/marks256/frames1024/
work1000000/proof1000/window12. Keep35 frozen blobs and all27282 baseline cases.

Native first: full capture spans for finite modifiers, numbered/named/branch-
reset aliases, absent and consumed prefixes, last-iteration capture, rollback,
tail failures, backreferences/conditions, scoped casing, UCP/ASCII/property/CR
controls, nested/root alternatives, and PLUS/star/ancestor ownership. Cover six
newlines/UCP/every caller offset/malformed fragments, strict UTF boundaries,
work/default budget/cancellation, count and expansion limits. Ordinary counted
controls use native interpretation in CI for159; bitmap-sensitive cases retain
JIT. Preserve the new test blob after successful-build RED; all baseline cases
must pass before production changes. No edits to earlier test expectations.

Final exact-head qualification: all six native Staging OS/arch jobs/net8/9/10,
full suite and frozen214/120method identities, zero failures/skips/compiler
warnings/errors, all configured gates, seven equal Linux component workloads.
Author whole-branch review, remote exact tree/blob checks, historical inventory/
index preservation and only issue102 recent-footer update precede ready/unmerged
PR. Broader7/99/100/102/native159 and release/Grep gates remain open. Publication
and all known migrations precede CommandFramework removal; Grep remains last.
