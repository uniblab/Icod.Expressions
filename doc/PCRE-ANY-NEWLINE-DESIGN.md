# ANY newline convention and consumed-CR anchor regressions

## Contract

Continue issue #7 after #111 with exact `(*ANY)` in the shared uninterrupted
original-byte0 prefix. Pinned authority: PCRE.NET1.6.0 / PCRE2 10.48 newline
configuration, JIT anynewline/anchor and UTF peek paths.

ANY is last-wins with the existing five conventions, independently of UTF/UCP,
BSR and NOTEMPTY. Newline characters are LF, VT, FF, CR, NEL, and (in UTF mode)
LS/PS. These are exactly the existing fixed vertical-space scalar list; byte
mode recognizes only values representable as single bytes. NUL is ordinary.
Dot/bare N reject those characters; DotAll affects dot only. R remains BSR-owned,
including atomic CRLF-first behavior and its independent membership policy.

Multiline caret tests the preceding decoded scalar within the fragment; dollar
tests a newline scalar at the current position or effective end. Ordinary dollar
and Z allow exactly one final newline: CRLF is length2, other newline scalars use
their encoded width. Native JIT also allows the LF alone after an already consumed
CR, including caret/dollar/Z at that midpoint. CRLF-only still rejects midpoint
anchors. Existing ANYCRLF checks wrongly forbid that consumed-CR midpoint for
caret and dollar/Z; three native-first regressions correct those checks without
changing failure-driven candidate advancement or broader optimization policies.

After a failed CR candidate, ANY retains native pair bumpalong, with existing
source/prefix/start-class exceptions. Byte dotAll+literal can enter LF using the
existing bounded byte Any+literal proof; ordinary ANY dot/N excludes CR and LF,
and UTF dotAll retains native skip. Byte ANY ordinary dot/N use a cached byte
set; byte ANY DotAll reuses the already width-neutral Any leaf/opcode. No clone.

Extended x/xx comments terminate at the selected newline scalar, consuming a
whole UTF scalar or CRLF pair. UTF continuation byte85 inside a non-newline
scalar must not terminate a comment or create an anchor. Byte85 is a newline in
byte mode. NUL never terminates source/comments; original bytes remain length-
delimited and validated. Outside comments Pattern_White_Space remains unchanged.

## Chosen approach

Codex chooses under the standing autonomous remit to append Any to the immutable
enum, reuse PcreSpaceCharacters.IsVertical, and add bounded helpers for ANY
newline byte length at a position and preceding-scalar newline detection. UTF
reverse inspection is capped at three continuation bytes and constrained by the
fragment start. Helpers use synchronous span parameters only; no spans/frames
are retained. Parser byte sets and shared Any/anchor/comment paths consume the
same fixed membership. Existing fixed/variable behind end caps remain active.

Treating ANY as ANYCRLF misses five newline scalars; scanning UTF bytes for85
misreads unrelated characters. New state machines or cloned byte/UTF matchers
would duplicate work/rollback. No new AST/opcode/frame/journal, snapshots, saved
pointers, limits, public API/package/version/dependency/workflow/release/consumer
change or production native/Regex fallback. No Python or subagents.

## Validation and bounds

Native-first literal all-capture fixtures cover membership, multibyte anchors,
comments, continuation controls, prefixes, captures/calls/behind/conditions,
byte/UTF LF entry, and consumed-CR anchor regressions under ANYCRLF. All256 bytes
and Unicode boundary probes, every-offset tolerant UTF/byte matrices, original
source/strict errors, long prefix, work/cancellation cover both semantics and
bounds. Tests-only expected RED before source edits. No local SDK; CI RED/GREEN.
Require six native Staging full/frozen214 suites on net8/net9/net10, zero failed/
skipped/build warnings/errors and configured gates/seven Linux equalities.
Frozen35/whitespace/clean/exact-tree/head proof and separate author self-review.
User merges; preserve evidence. C#13, UTF8+LF, root project/solution/src/tests/samples.

Broader #99/#102 optimization, #100 unsafe malformed Xuc, Unicode-version and
public/release/consumer gates remain separate. Grep last; CommandFramework removal
after publication and all known consumers migrated. Self-check enumerated all
newline consumers and isolated the two old anchor checks; no unresolved interface
or design question. No full PCRE/public release claim from this bounded tranche.
