# Caller-selected first-line compilation

Add default-false internal `PcreByteOptions.FirstLine` for PCRE2_FIRSTLINE
(0x100). Unanchored candidate entry is limited to positions before or at the
first configured newline following the requested byte offset. Matches may
consume past that newline. A requested offset after an earlier newline begins
a new search window. Caller Anchored makes FirstLine irrelevant; EndAnchored
independently constrains root completion. Literal compilation is compatible.

Native JIT explicit forward SKIP is a separate entry path: it can restart even
with caller Anchored. For unanchored FirstLine, native first-unit/prefix filters
can still enforce the window; proven unfiltered entry can bypass it. Preserve
that distinction rather than applying the ordinary bumpalong gate to every
forced restart. Broader optimizer proofs remain tracked by issue102.

## Approach

Snapshot the option into init-only `PcreByteProgram.FirstLine` after ordinary
compilation. Keep an incremental newline-inspection frontier from startOffset.
Before entering any later candidate, inspect the intervening input with existing
newline and UTF readers; stop if a configured newline precedes that candidate.
Charge each inspected scalar or malformed byte through Step. Check candidates
after fragment/scalar selection and before COMMIT first-byte entry. All later
CRLF, inferred-line, fragment and empty-EOF retries pass this gate. Forward SKIP
targets carry a one-candidate forced-entry marker; honor them before anchored
termination. Under FirstLine, bypass the frontier only for a bounded immutable
root-entry proof. The proof recognizes transparent single-branch groups,
opaque MARK/reference/call entry and terminal unrestricted atom/property or
nullable paths, with at most four group levels/1,000 visits. Positive current-
position assertions and restrictive/suffixed prefixes keep the window gate.
The proof uses existing source-shape metadata; unknown broader shapes stay
within the existing issue102 optimizer qualification boundary.

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

Native10.48/JIT0 preflight pins8,041 matching/offset protocols in520 groups,
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
mainloop_entry, match_end_ptr, reset_match/continue_match/reset_match_label and
prefix entry filters; native outcome protocols.

## Composition finding before production

Initial tests-only RED750 (source5fdf2de12b27f50f89151559cf44ae3534946d00,
Linux ARM132428400350) built all three frameworks with zero warnings/errors.
Each ran206 failures/86,457 passes/86,663 total/zero skips. All86,250 earlier
tests passed. Four additional protocol groups exposed the prior anchored-SKIP
gap;199 groups/2 bounded-work cases/1 ownership case exposed FirstLine absence.
Before any production change, expand the fixture from413 to605 cases, preserving
every original expectation and adding forced-entry/filtered-entry controls,
including explicit FirstLine=false anchored-SKIP regressions. Re-run clean RED
and freeze the expanded fixture before implementing either boundary.
