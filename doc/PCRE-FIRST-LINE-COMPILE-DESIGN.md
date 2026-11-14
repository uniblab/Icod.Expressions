# Caller-selected first-line compilation

Add default-false internal `PcreByteOptions.FirstLine` for PCRE2_FIRSTLINE
(0x100). Unanchored candidate entry is limited to positions before or at the
first configured newline following the requested byte offset. Matches may
consume past that newline. A requested offset after an earlier newline begins
a new search window. Caller Anchored makes FirstLine irrelevant; EndAnchored
independently constrains root completion. Literal compilation is compatible.

## Approach

Snapshot the option into init-only `PcreByteProgram.FirstLine` after ordinary
compilation. Keep an incremental newline-inspection frontier from startOffset.
Before entering any later candidate, inspect the intervening input with existing
newline and UTF readers; stop if a configured newline precedes that candidate.
Charge each inspected scalar or malformed byte through Step. Check candidates
after fragment/scalar selection and before COMMIT first-byte entry. All later
SKIP, CRLF, inferred-line, fragment and empty-EOF retries pass this gate.

Disable unobserved whole-subject byte/suffix search shortcuts under FirstLine,
so they cannot charge candidates beyond the permitted window or bypass the
new frontier. Keep strict UTF/offset validation before candidate gating. Retain
tolerant empty/raw candidates, full-subject context and all consumption bounds.
Use the effective program newline convention, independently of BSR. CRLF-only
scanning begins at the caller offset, so starting at LF does not include an
earlier CR. UTF ANY recognizes complete newline scalars beginning at or after
the offset; split encodings and malformed fragments retain their existing rules.

Slicing the subject at the newline would reject valid crossing matches and
break offsets/lookbehind. A source wrapper would change recursive root0 and
capacity. An eager full-line scan would spend extra work before an immediate
first-candidate success; the incremental frontier preserves that work trace
while bounding failed byte search at the first newline. No new opcode or IR.

## Scope and qualification

Only PcreByteOptions.cs, PcreByteCompiler.cs, PcreByteProgram.cs and
PcreByteMatcher.cs change in production. Preserve parser/root0/callee IR,
opcodes/scopes/journal/shared ceilings/exported API/dependencies/workflows.
Baseline merged247 `bb6056dfb4aa1fd9e3e1ed149eaaa9004cc35fb0`, tree
`8a1e5efa9cfdc4c2b359ea6c0ed5c57db472950f`:86,250 tests/framework and264
prior test files. Keep all prior hashes, GNU/BRE/ERE, frozen35 production
blobs and214 frozen outcomes/120 methods. C#13/net8/net9/net10, UTF8+LF;
no Python/agents/local SDK installation. Reuse linked worktree; user owns
merge/tag/publication; issue7/102 stay open and Grep last.

Native10.48/JIT0 preflight pins5,689 matching/offset protocols in328 groups,
65 compile diagnostics and9 work cases. Pin crossing/at-newline versus later
candidate starts, all six newline conventions, nonzero/raw/split offsets,
malformed fragments, labels/cuts/ACCEPT/recursion/resets, caller/source modes,
both anchoring flags and literal/casing/word/line composition. Separate41 native
split-offset(-36) rows from successful/no-match protocols; strict whole-subject
validation remains a managed contract. Additional controls cover owned policy,
default behavior,4-step failed byte search at an initial newline (exhaustion at3),
first-success work/exhaustion/cancellation,256 captures/depth and4095+ACCEPT
versus4096 literal instruction capacity. Validate each whole native batch before
managed matching. Watch clean net8/9/10 tests-only RED and freeze the fixture
before production. Qualify all18 results and every configured supporting gate.
All seven inventories precede source qualification; final GREEN/review/source/
base/tree/proposed-merge evidence belongs on PR248 and only issue7 footer,
without documentation-only CI repeats.

References: pinned10.48 pcre2api.html FIRSTLINE/LITERAL; pcre2_jit_compile.c
mainloop_entry, match_end_ptr and reset_match_label; native outcome protocols.
