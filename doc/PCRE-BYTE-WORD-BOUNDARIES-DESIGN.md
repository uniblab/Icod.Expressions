# PCRE byte-mode ASCII word boundaries

**Status:** internal syntax slice under [issue #7](https://github.com/uniblab/Icod.Expressions/issues/7). No public API, consumer switch, or PCRE-capable package claim.

## Intent and scope

Support `\b` and `\B` outside bracket classes in the 8-bit non-UTF, non-UCP matcher. A word boundary is the zero-width position where the preceding and following bytes differ in ASCII `\w` membership (`[A-Za-z0-9_]`). Missing bytes at either input edge count as non-word; `\B` is the complement. Evaluate the actual preceding byte even when a search starts at a nonzero offset. NUL and bytes 0x80–0xFF remain non-word in this mode. Inside brackets, `\b` continues to mean backspace; `\B` remains an unsupported class escape. Repetition of a boundary remains invalid syntax.

## Approach

Add one boundary AST node and bytecode instruction with a polarity bit. At match time, check the two adjacent bytes against the existing immutable `PcreByteSet.Word` and advance the program counter without consuming input. Charge attempted adjacent-byte classifications to the existing work/cancellation budget. Existing backtracking snapshots handle failed paths; capture state is unchanged.

This shares the same non-UCP ASCII word definition as the implemented `\w`/`\W` classes, so boundary decisions cannot drift from class membership. It does not introduce Unicode mode or locale-sensitive byte tables. PCRE2's [simple assertion specification](https://www.pcre.org/current/doc/html/pcre2pattern.html) defines `\b`, `\B`, and boundary edges. Unicode/UCP word boundaries require a later implementation and conformance gate.

## Verification

Pin word/non-word transitions, both input edges and empty input, nonzero search offsets, capture rollback, NUL/high bytes, bracket backspace and unsupported class escape, invalid repetition, and work/cancellation behavior. Run net8/net9/net10 tests on six CI runners, frozen GNU corpus and package-only smoke. Grep and CommandFramework production code remain untouched.
