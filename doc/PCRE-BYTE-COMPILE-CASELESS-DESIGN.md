# PCRE byte-mode compile-time caseless option

**Status:** internal option slice under [issue #7](https://github.com/uniblab/Icod.Expressions/issues/7).

## Intent and boundary

Let a caller compile an 8-bit non-UTF pattern with ASCII caseless matching through `PcreByteOptions.Caseless`, without rewriting the pattern to prefix `(?i)`. This is groundwork for Grep's `-P -i` path, but does not migrate Grep. The existing leading `(?i)` still enables caseless matching when the caller's option is false. A leading `(?s)` changes only dot behavior and preserves the caller's caseless setting. No option-unsetting syntax is added; `(?-i)` remains unsupported.

The byte program already carries a single `Caseless` bit used by literal, bracket, fixed-literal lookaround, and numbered-backreference matching. Initialize the parser with the caller's bit and combine a leading `(?i)` with it. Captures retain authoritative original byte offsets; NUL and high bytes remain exact. GNU production behavior, public API, Grep, and CommandFramework production code remain unchanged.

## Approaches

- **Selected:** Add a bool to the existing internal options record and initialize the parser's compile-time case mode from it. This routes bracket construction and program comparisons through the same path as `(?i)` and avoids a second matching mode.
- Rewrite the source pattern to prepend `(?i)` for callers. This would shift byte diagnostic offsets and conflate an API option with user pattern bytes.
- Pass an additional flag directly to the matcher. This would leave bracket classes already compiled without case closure and diverge from literal behavior.

## Verification

Test the option on a plain pattern, leading `(?s)` and `(?i)`, all existing caseless byte comparison sites, default case-sensitive behavior, high bytes/NUL, match offsets/captures, and work/cancellation limits. Run net8/net9/net10 on all six Staging runners with the frozen GNU corpus, extraction parity, and package-only smoke.
