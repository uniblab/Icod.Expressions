# CRLF-only newline convention

## Contract and intent

Continue issue #7 after #110 with exact `(*CRLF)` in the uninterrupted original
byte0 prefix. Pinned authority: PCRE.NET1.6.0 / PCRE2 10.48 pso_list, fixed
2-code-unit newline configuration and JIT dot/anchor/bumpalong paths.

CRLF is last-wins with LF/ANYCRLF/CR/NUL and independent of UTF/UCP/BSR/NOTEMPTY.
It recognizes only CR immediately followed by LF within the active subject end.
Lone CR/LF are ordinary characters. Dot/bare N refuse the CR of a complete pair;
LF is a matchable character if the VM enters there. DotAll bypasses this check
for dot only. R remains BSR-owned and atomic CRLF-first; its lone CR/LF membership
is unchanged. Byte and UTF use one Any instruction with width-appropriate reading.

Multiline caret requires the preceding complete pair within the fragment, and
retains existing terminal-end rules. Multiline dollar recognizes a complete pair
at the current position or the effective end; ordinary dollar/Z recognize only
one final complete pair or end. Neither matches between CR and LF. A/z/G/K retain
existing semantics. Fixed behinds inspect the original fragment end; bounded
variable behinds honor their existing active end cap. Consequently fixed dot
behind before LF rejects CR, while a variable behind ending before LF can match
CR as a lone character. This is native JIT behavior, not a new width policy.

Extended x/xx line comments terminate only at the full pair, consuming both bytes;
lone CR/LF and raw NUL inside a comment do not end it. Outside comments existing
Pattern_White_Space rules remain unchanged. Quotes/classes preserve literal text;
source is length-delimited and all original bytes remain validated/diagnosed.

After a failed CR candidate, shared bumpalong skips LF of a full pair unless
existing lexical/prefix/start-class metadata allows entry. Native has a width
difference: on CRLF+A, byte `.A` and its bounded capture/noncapture wrappers enter
at LF, whereas UTF `.A` misses from offset0. Extend only the existing byte two-atom
prefix proof to the new byte Any leaf; retain its mandatory literal suffix and
wrapper bounds. UTF proof/policy is unchanged. Explicit caller offset1 permits LF
entry in both modes. Broader optimizer gaps remain #99/#102.

## Chosen approach and alternatives

Codex chooses under the standing autonomous remit to append CrLf to the existing
immutable enum and use a bounded shared pair predicate. The existing Any leaf/
opcode reads a byte or UTF scalar; the parser emits that leaf for byte CRLF dot/N
as well as UTF. The matcher branches only at newline tests. Shared candidate,
backtrack/journal/guard/work/cancellation paths remain. Existing LF/ANYCRLF/CR/NUL
source lowering and matching behavior is preserved.

A static byte dot set cannot distinguish CR before LF from lone CR; treating the
convention as ANYCRLF would reject lone characters and misplace anchors/comments.
A cloned byte/UTF matcher or separate directive AST/opcodes would duplicate state
and semantics. No new saved frame/journal, retained span/pointer, snapshot, engine,
resource limit, public API, package/version/dependency/workflow/release/consumer
change or production native/Regex fallback. Unicode ANY remains pending.

## Verification and boundaries

Native-first all-capture literals pin complete/lone pairs, dot/N/s/R independence,
anchors, calls/captures/conditions, fixed/variable/non-atomic behinds and byte/UTF
prefix differences. Every-offset byte/tolerant UTF matrices cover CRLF fragments,
invalid UTF, explicit LF starts, original caller offsets and nullable candidates.
Byte256/Unicode membership, raw comment patterns, prefix overrides, diagnostics,
long prefix and work/cancellation pin resource and parsing behavior.
Tests-only managed RED before source edits; no local SDK, CI RED/GREEN. Six native
Staging platforms require full/frozen214 net8/net9/net10, zero failures/skips/build
warnings/errors, configured gates and seven Linux workload equalities. Frozen35,
whitespace, clean/exact-tree/head proof and separate author self-review. No agents
or Python. User merges; preserve worktree/evidence. C#13, UTF8+LF, src/tests/samples.

Unicode-version, #99/#102 optimizer, #100 unsafe malformed Xuc and public/release/
consumer gates remain. Grep last; CommandFramework removal after publication and
all known consumers migrated. Builder self-check enumerated every Character/pair
consumer; byte Any keeps width1 and existing instruction work; behind caps and
byte-only prefix proof are explicit. No unresolved design/interface question.
