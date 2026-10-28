# PCRE newline sequences

**Scope:** issue #7 internal conformance. No public PCRE API, release or consumer migration.

## Semantics and approach

Outside brackets, `\R` consumes one newline sequence. In byte mode this is CRLF, LF, VT, FF, CR or NEL. UTF mode also includes LS and PS. The CRLF branch is tried first and is atomic: a later failure cannot split it into a CR match followed by an LF match. Ordinary surrounding alternatives and repetitions may still backtrack. UCP, caseless and DotAll do not change membership.

Add the internal `PcreByteOptions.BsrAnyCrLf` compile option to restrict `\R` to CRLF/CR/LF. This is independent of `AnyCrLfNewline`, which controls dot, anchors and candidate search. Pattern-side `(*BSR_...)` settings remain explicitly unsupported. The official [PCRE2 pattern specification](https://www.pcre.org/current/doc/html/pcre2pattern.html), newline sequences section, defines this behavior; tests pin PCRE.NET 1.6.0 / PCRE2 10.48.

Choose a semantic `PcreByteNewlineSequence` node, lowered by the existing program compiler to an atomic CRLF-first alternation and an appropriate single-character class. Keep one lowering path and existing literal/class/atomic opcodes; no runtime-specific matcher or backtracking frame is needed. A dedicated opcode would duplicate existing atomic machinery. Expanding it in the parser would lose the distinction between implicit CR/LF in `\R` and explicitly written newline literals used by the CRLF candidate policy.

## Boundaries

- Byte and UTF paths retain authoritative source-byte spans, capture journaling, invalid-UTF fragment barriers, and ordinary instruction work/cancellation checks.
- `\R` is invalid inside a character class. General lookaround bodies containing `\R` remain unsupported; the existing fixed-literal assertion subset is unchanged.
- `\N`, `\X`, `\C`, named references and remaining directives remain pending.
- `\R{n}` expansion is subject to the existing instruction and compile-visit ceilings.

## Evidence

Test every byte, all Unicode newline members and representative nonmembers, both BSR choices and both newline conventions, atomic splitting, capture rollback, greedy/lazy/possessive/counting behavior, every offset around CRLF and malformed UTF, and resource/cancellation outcomes. Record the test-first and final six-platform CI results in the PR.
