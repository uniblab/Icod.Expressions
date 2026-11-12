# Turkish casing design

## Intent and boundaries

After merged PR224, accept exact leading (*TURKISH_CASING) in the existing
uninterrupted prefix. PCRE2's 8-bit engine requires UTF; UCP-only byte mode is
invalid, as is combining global CASELESS_RESTRICT. Global validation follows
whole-pattern UTF validation and reports the consumed prefix end. Local (?r)
is permitted: it overrides Turkish folding with the existing default restricted
policy. Resetting/unsetting r re-exposes Turkish folding; (?^) does not clear
the global Turkish choice. The directive does not itself enable caseless.

## Approach

Keep the pinned default Unicode17 data unchanged. Add a per-pattern global bool
and extend shared Fold/Equals/CloseRanges helpers with an optional bool. When
Turkish is active and lexical r is false, fold I/ı to U+0131 and i/İ to U+0069;
every other scalar uses the current table. Parser class closure and runtime
literal/set/algebra/reference/assertion consumers receive the same mode. Store
it once in the compiled program; no new AST flag, opcode, slot or journal field.
This is my implementation choice under the approved inline syntax roadmap.
A separate table duplicates unchanged data; host culture would lose pinned
semantics and concurrency safety; a second matcher duplicates the engine.

## Native optimizer boundary

Turkish I pairs use native property casesets. Existing optional-literal CRLF
bitmap and COMMIT first-byte proofs must exclude those I operands when Turkish
is active and lexical r is false. Preserve all other proofs and their visit
bounds. Native controls cover optional literals/singleton classes, wrappers,
COMMIT and every selected malformed-UTF/CRLF offset. Wider optimizer parity
remains issue102; this is the mode-specific correction to existing proofs.

## Qualification and fixed references

1256 new cases include1222 literal/profile fixtures independently checked with
pinned PCRE2 10.48; all compile with native JIT. They cover all four I scalars,
case flags, ordinary/Perl/UTS classes, properties, references, scoped/reset r,
recursive/forward calls, lookarounds, captures, immediate labels, prefix order,
UTF conflicts/diagnostics, offsets, quotas/work/cancellation and ownership.
Retarget exactly one obsolete pending-Turkish row in the prior CASELESS_RESTRICT
inventory to the nonexact TURKISH_CASINGX header; preserve its opening20/native
rejection and every other literal, capture and assertion. Freeze new/revised
blobs after clean RED, before production. GNU/frozen35/214/120, default Unicode
data, exported API and dependencies stay fixed. No native fallback. User owns
merge/tag/publication; Grep stays last.

Review five input classes: asymmetric I pairs across all comparison consumers;
lexical r/reset and definition options; prefix/UTF diagnostic precedence;
mode-specific start proofs/CRLF fragments; global ownership and bounded work/
state/cancellation. Each has controls in the new fixture file.
