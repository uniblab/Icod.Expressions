# CR and NUL newline conventions

## Contract

Continue issue #7 after #109 with exact `(*CR)` and `(*NUL)` in the shared,
case-sensitive uninterrupted original-byte0 start prefix. Pinned authority:
PCRE.NET1.6.0 / PCRE2 10.48 pso_list and compile newline initialization.

The selected convention is global and last-wins among LF, ANYCRLF, CR and NUL,
independent of UTF/UCP, BSR and NOTEMPTY settings. Caller AnyCrLfNewline still
selects the initial LF/ANYCRLF default; no caller option or public API is added.
Resets/groups/calls cannot change the convention. Misplaced/unknown/incomplete
forms keep original-byte diagnostics; quoted/class directive text is literal.

CR recognizes only U+000D; NUL recognizes only U+0000. Dot and bare N reject that
one character unless dot is under s; bare N still ignores s. Byte/scalar literal,
class, property, shorthand and quoted membership is otherwise unaffected.
Multiline caret recognizes positions after the selected character, with existing
JIT end/fragment rules. Multiline dollar recognizes the selected character/end;
ordinary dollar and Z recognize only one final selected character or end. z and
A remain absolute. Under CR, a CRLF pair is two independent characters: LF is
ordinary, including an explicit caller start between the pair. NUL is a scalar,
not a pattern/subject terminator or invalid-UTF barrier.

Extended x/xx line comments end only at the selected newline. Under CR, consume
CR alone, leaving a following LF to ordinary trivia rules. Under NUL, consume
one raw NUL and continue parsing the remaining original bytes. Existing global
Pattern_White_Space handling and bracket xx rules remain unchanged. R remains
controlled exclusively by BSR; CR/NUL conventions do not change its membership
or atomic CRLF-first behavior. Lookaround/width/capture/call/resource rules remain
shared. Existing LF/ANYCRLF CRLF candidate policies must remain unchanged.

## Chosen approach and alternatives

Codex chooses under the standing autonomous remit to replace the internal
newline boolean in compile settings/program metadata with a four-value enum.
A shared primary-character helper maps CR/NUL/LF; ANYCRLF keeps LF plus existing
CR/pair handling. The parser chooses cached byte dot sets and uses the same
primary character for comment termination; the VM uses it for scalar dot and
anchors. Existing CRLF-specific behavior remains conditional on ANYCRLF.

Adding more booleans permits contradictory modes and makes last-wins scanning
fragile. Cloning CR/NUL matchers or emitting special directive AST/opcodes would
duplicate semantics. A compact immutable enum keeps one engine, no saved spans,
new frames/journal, snapshots or raised limits. CRLF-only and Unicode ANY need
separate sequence/scalar newline rules and remain pending, rejected.

## Files and verification

Create PcreByteNewline.cs; modify shared start settings/compiler, parser and
extended comments, byte sets, program configuration and matcher dot/anchors.
No new AST/opcodes or changes to width lowering, candidate metadata/optimizers,
BSR, UTF validation, public API, package/version/dependencies/workflows/release/
consumers or production native/Regex fallback. No Python or subagents. C#13,
net8/net9/net10, UTF8+LF, root project/solution, src/tests/samples convention.

Native-first all-capture literal cases and byte256/Unicode membership prove
both conventions, dot/N/s/R independence, anchors, scope/captures/calls/behind
widths, prefix interleaving/last-wins and raw-NUL comments. All-offset matrices
cover byte/tolerant UTF, CRLF, NUL and malformed fragments; strict errors,
original-byte grammar diagnostics, work/cancellation and long prefixes pin bounds.
Tests-only expected RED before source edits. No local SDK; managed RED/GREEN via
CI. Require six native Staging full/frozen214 suites on net8/net9/net10, zero
failed/skipped/build warnings/errors and configured gates/seven Linux equalities.
Frozen35/whitespace/clean/exact tree/head proof and separate author self-review.
User owns merge; preserve evidence/worktree.

Existing #99/#102 optimizer parity, #100 unsafe malformed Xuc, Unicode-version,
other newline/directive/verb and public/release/consumer conformance gates remain.
Grep last; CommandFramework removal after publication/all known consumers migrated.
Builder self-check: all newline consumers were enumerated; R does not read the
newline field, old pair-sensitive code retains its ANYCRLF condition, raw NUL
stays length-delimited source. No unresolved design or interface conflict.
