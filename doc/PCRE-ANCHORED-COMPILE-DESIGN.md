# Caller-selected anchored compilation

Add default-false internal `PcreByteOptions.Anchored` for PCRE2_ANCHORED
(0x80000000). The matcher tries only the caller's requested candidate byte
offset. Backtracking, assertions, recursive calls and alternative selection at
that candidate remain normal. A reported start moved by backslash-K need not
equal the original candidate. Caller literal compilation remains compatible.

## Approach

Snapshot the option into an init-only `PcreByteProgram.Anchored` flag after
ordinary compilation. Keep the parser, root/subroutine bodies, instructions,
scopes and journal unchanged. At matching entry disable unanchored byte/suffix
search shortcuts. After existing UTF/offset validation, constrain candidate
entry to the requested offset, bypass unanchored COMMIT first-byte entry and
return after that candidate fails, before SKIP/CRLF/line-search retries.

A source-level backslash-G wrapper is insufficient: it can change recursive
root0 semantics and consumes grammar/instruction capacity. A separate matcher
would duplicate scopes, captures, UTF and resource policy. The selected
program flag preserves existing VM work for the first attempt and avoids
scanning long byte subjects for later candidates. No new IR node or opcode.

Tolerant UTF preserves existing fragments, zero-width candidates and original
lookbehind context; never slice the subject at the requested offset. Empty
patterns may match at requested raw offsets. Nonempty matching must not jump
past malformed bytes to a later fragment. Strict malformed subjects and split
or invalid offsets retain existing priority. Cancellation/nonpositive work
limits still precede compilation; byte candidate work remains bounded.

## Scope and qualification

Four production files: `PcreByteOptions.cs`, `PcreByteCompiler.cs`,
`PcreByteProgram.cs`, `PcreByteMatcher.cs`. Parser/IR/opcodes/journal/shared
ceilings/exported API/dependencies/workflows remain unchanged. ENDANCHORED and
FIRSTLINE remain separate increments because they constrain different state.

Baseline merged245 `03c105b5a13890059f20ff51a074f67d180a20f6`, tree
`4868b519b9205cf6327fc501f86e041836ccf023`:85,360 tests/framework and262 prior
test files. Preserve every prior hash, GNU/BRE/ERE, frozen35 production blobs
and214 frozen outcomes/120 methods. C#13/net8/net9/net10, UTF8+LF; no Python,
agents or local SDK installation. Reuse the isolated worktree. User owns
merge/tag/publication; issue7/102 stay open and Grep migrates last.

Pin native PCRE2 10.48/JIT0 outcomes before managed matching. Group explicit
protocols by source/caller options, validating every native protocol before
executing its managed group, to retain coverage without thousands of extra
discovery cases. Watch clean tests-only RED on net8/9/10 and freeze the fixture
before production. Cover fixed-candidate failure, recursion/ACCEPT/cuts/labels,
reported resets, prefixes/caller casing/word/line/literal policies, malformed
fragments/offsets, owned state, bounded work/cancellation and full existing
capture/depth/instruction capacity. Qualify all18 native results and every
configured supporting gate. All seven inventories precede qualification;
final GREEN/review/source/base/tree/proposed-merge evidence goes on PR246 and
only issue7 footer, without a documentation-only source push.

References: pinned10.48 `pcre2_jit_compile.c` anchored entry and mainloop gates,
`pcre2_compile.c` public literal masks, and native outcome protocols.
