# Nonempty-match start directives

## Intent and native contract

Continue issue #7 after #108 with exact `(*NOTEMPTY)` and
`(*NOTEMPTY_ATSTART)` in the existing uninterrupted, case-sensitive original
byte0 start prefix. Authority: PCRE.NET1.6.0 / PCRE2 10.48, pcre2_compile.c
pso_list flags and pcre2_jit_compile.c empty-match acceptance/backtracking.

Both directives enable independent global flags, in any order/repetition with
the seven existing start tokens. Neither is an ordinary atom or repeatable.
They survive ordinary inline resets and group/call scopes. Unknown, misplaced,
argument-bearing and incomplete forms retain controlled original-byte errors.
Quoted/class text stays ordinary source. No caller options are added.

At whole-match acceptance, compare current position with the reported group0
start, including backtracked K resets. NOTEMPTY rejects equality everywhere.
NOTEMPTY_ATSTART rejects equality only when the empty reported match is at the
original caller start offset. Thus aK on a fails with NOTEMPTY but returns1:0
with NOTEMPTY_ATSTART. Flags accumulate; NOTEMPTY dominates if both are set.
Rejection is ordinary path failure: retry alternatives/lazy repeats and restore
captures/starts/scopes through the existing journal. An atomic group's committed
empty body remains committed. Empty assertion/subroutine success is allowed;
only whole-match acceptance enforces these flags.

Preserve native byte/strict/tolerant UTF candidate protocols: validation before
acceptance, scalar boundaries, requested malformed-byte nullable candidates,
invalid-run gaps and CRLF bumpalong. The tolerant empty-program shortcut must
not bypass either flag; flagged programs use the existing candidate/VM path.
The AST's empty-path metadata remains syntactic, so nullable malformed-byte
candidates remain available for fallback and later-offset empties.

## Approach and boundaries

Codex chooses under the standing autonomous development remit to extend the
immutable start-settings value with two booleans, propagate them through the
program compiler into readonly program metadata, and guard the one Accept
handler. Existing failed state owns all rollback. Restrict the empty-program
shortcut to unflagged programs. No flags in backtrack frames or capture journal.

Alternatives: filtering returned matches would lose same-candidate alternatives
and capture rollback; injecting assertions would incorrectly affect calls or
nested assertions and complicate K semantics. A separate matcher/option clone
adds state without benefit. One final-accept guard is sufficient.

No new AST/opcode/frame/journal/width/optimizer policy or resource limit. Keep
UTF/UCP/newline/BSR validation order and full source metadata. Existing #99/#102
optimizer gaps, #100 unsafe malformed Xuc, other directives/verbs, Unicode-version,
public API/release and consumer conformance stay separate. No package/version/
dependency/workflow/release/consumer changes or native/Regex production fallback.
Grep last; CommandFramework removal requires publication and all known consumers
migrated. User owns merge. C#13/net8/net9/net10, UTF8+LF; no Python or subagents.

## Verification and design self-check

Native-first all-capture literals pin both flags, empty subjects, alternative
fallback/capture rollback, lazy/atomic/assertion/call/conditional/recursive paths,
K reported starts, mixed/repeated prefixes and quoted/class controls. Every
caller offset over byte/tolerant UTF × LF/ANYCRLF covers empty roots, captures,
lookarounds, G/K, CRLF and malformed fragments. Strict UTF errors, source offsets,
long prefix, work/cancellation and invalid-limit precedence complete the boundary.
Tests-only expected RED precedes source edits; no local SDK, managed RED/GREEN
via CI. All six native Staging jobs require full/frozen214 net8/net9/net10,
zero failed/skipped/build warnings/errors and configured gates/seven Linux
workload equalities. Frozen35/whitespace/clean/exact-tree/head proof required.
Separate author whole-branch self-review; no independent-review claim.

Builder self-check: settings are immutable/global; only Accept rejects empty
results; rejected paths share all rollback; flags do not disable nullable search
metadata; the old empty-root fast path is the only direct return needing a guard.
No unresolved design question. Preserve worktree and ignored evidence.
