# PCRE byte-mode dollar before final LF

**Status:** internal conformance correction under [issue #7](https://github.com/uniblab/Icod.Expressions/issues/7).

## Intent and boundary

In the 8-bit non-UTF matcher, make the unqualified `$` assertion succeed at absolute subject end or immediately before a single final LF byte, without consuming the LF. This follows PCRE2's default LF newline convention. `\z` continues to require absolute end; `\Z` continues to allow the final LF position. `^` still tests absolute start. A nonzero search offset does not redefine the subject end. `(?s)` does not affect `$`.

Other newline conventions, `PCRE2_DOLLAR_ENDONLY`, multiline mode, and match-time `NOTEOL` remain pending. GNU/POSIX behavior, public API, Grep, and CommandFramework production code remain unchanged.

## Approaches

- **Selected:** Parse `$` to the existing zero-width `PcreByteEndBeforeFinalLf` node and opcode already used by `\Z`; keep `\z` on the strict End opcode. This reuses tested LF/end, work-limit, and backtracking behavior.
- Add a separate dollar opcode with the same LF behavior now. This would duplicate logic before any option-dependent distinction exists.
- Change the End opcode itself. This would incorrectly broaden `\z` and alter strict subject-anchor behavior.

## Verification

Test final LF, multiple final LFs, empty input, an embedded LF, `$` followed by an LF atom, nonzero search offsets, bracket literal dollar, direct quantifier diagnostic, NUL/high bytes, cancellation/work bounds, and `\z` strictness. Run all six Staging platforms with net8/net9/net10, frozen GNU corpus, extraction parity, and package-only smoke.
