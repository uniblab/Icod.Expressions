# Caller-selected end-anchored compilation

Add default-false internal `PcreByteOptions.EndAnchored` for
PCRE2_ENDANCHORED (0x20000000). A successful root match must consume through
the physical subject end. Ordinary completion that ends earlier backtracks
normally; root `(*ACCEPT)` that ends earlier abandons the entire candidate
and restarts unanchored search. Assertion and subroutine ACCEPT remain scoped.
The existing caller Anchored flag independently constrains the initial candidate.
Literal compilation is compatible with either or both flags.

## Approach

Snapshot the option into init-only `PcreByteProgram.EndAnchored` after ordinary
compilation. At final ACCEPT, compare consumed position with input.Length
before publishing captures/labels or applying NOTEMPTY. Reject a short normal
completion through the existing failure path, permitting saved alternatives.
After existing synthetic capture closes and scoped ACCEPT handling, a short
root EarlyAccept discards saved choices and cuts before failing this candidate.
The existing candidate reset clears journals/captures/labels/scope arenas.
Disable the tolerant empty-pattern shortcut for EndAnchored, letting shared
candidate traversal find physical EOF. No additional opcode or source wrapper.

Appending backslash-z would change root0 recursion and capacity, and explicit
ACCEPT could bypass it. A fragment-end check would incorrectly accept before
malformed trailing bytes. A reported backslash-K start is not the consumed
end; reversed/empty reported spans cannot substitute for the position check.
Keep whole-subject validation, raw offsets and lookbehind context intact.

## Scope and qualification

Only PcreByteOptions.cs, PcreByteCompiler.cs, PcreByteProgram.cs and
PcreByteMatcher.cs change in production. Preserve parser, root0/callee IR,
opcodes, scopes/journal, shared ceilings, exported API, dependencies and workflows.
Baseline merged246 `153273df6fd58266599e0dc495f15da4f51b3bda`, tree
`62a2e3ee4994368a362a044ab98dc67f02aab3a9`:85,685 tests/framework and263
prior test files. Keep all prior hashes, GNU/BRE/ERE, frozen35 production
blobs and214 frozen outcomes/120 methods. C#13/net8/net9/net10; UTF8+LF;
no Python/agents/local SDK installation. Reuse linked worktree. User owns
merge/tag/publication; issue7/102 remain open; Grep last.

Native PCRE2 10.48/JIT0 preflight covers4,956 explicit matching/offset protocols
in482 source/options groups,65 diagnostic controls and nine first-success
work cases. Additional tests pin owned policy/source/labels, default behavior,
strict subject/offset validation, exact work/exhaustion/cancellation and full
256 capture/depth and4095+ACCEPT instruction capacity. Native -36 split-offset
errors are excluded from matching protocols and retained as validation tests.
Validate every native protocol in a group before managed matching. Watch clean
tests-only net8/9/10 RED and freeze the fixture before production. Qualify all18
native results and every supporting gate. All seven inventories precede final
source qualification; final GREEN/review/source/base/tree/proposed-merge evidence
belongs on PR247 and only issue7 footer, without a documentation-only CI repeat.

References: pinned10.48 pcre2api.html ENDANCHORED and public literal mask;
pcre2_jit_compile.c compile_fail_accept_matchingpath, normal matchingpath and
restart_match/do_reset_match; native protocols distinguish completion paths.
FIRSTLINE and other deferred policies remain separate increments.

## Tests-first checkpoint

Tests-only [RED748](https://github.com/uniblab/Icod.Expressions/actions/runs/43585144086), source `7140659a6924969d2fa5dd4ea658cfc49bcc2719`, Linux ARM job132417911226: net8/net9/net10 each built with zero compiler warnings/errors and ran **385 expected failures / 85,865 passes / 86,250 total / zero skips**. Failures were384 missing-policy protocol groups and1 ownership case/framework, all after successful native assertions. All85,685 earlier tests and180 new controls passed. Freeze `2e39a5a936a8358973932a230d53f37abd7c9199` before production; no fixture correction or prior-test change was needed.

The565 tests retain4,956 explicit protocols in482 groups,65 compile diagnostics and18 supplemental cases. Final qualification/review/identities are external on PR247/issue7.
