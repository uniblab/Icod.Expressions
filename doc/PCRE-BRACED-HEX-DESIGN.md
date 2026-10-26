# Internal PCRE braced hexadecimal escapes

**Status:** internal syntax tranche under issue #7. No Grep or CommandFramework production change.

## Intent and approach

Accept `\x{...}` as a single literal in byte and strict UTF-8 modes, at the ordinary atom position, in ordinary brackets (including range endpoints), and in the existing fixed literal assertions. The parser's one escape path already feeds all those contexts. Accept ASCII hexadecimal digits with optional horizontal space directly after `{` and before `}`; reject an empty value, an interior nonhex digit, or a missing brace. Leading zeros are permitted without integer overflow. Limit the value to 0xff in 8-bit mode, and to Unicode scalar values through U+10FFFF, excluding U+D800–U+DFFF, in UTF-8 mode. Preserve source-byte result spans, caseless handling, work/cancellation budgets, and invalid-subject barriers.

Keep bare `\x` with one or two digits unchanged. Unknown escapes, octal and `\N{U+...}` stay pending. The internal compiler still does not provide a public PCRE capability claim.

## Verification

Add test-first fixtures for byte high/NUL literals, leading zeros, syntax and range errors, UTF two/four-byte scalars, bracket range/negation, literal lookbehind, counted repetition, invalid-subject barriers and compile cancellation. Run six Staging platform jobs, all three frameworks, exact frozen GNU corpus and package smoke. PCRE2 10.48 reference conformance remains an independent migration gate.
