# PCRE byte-mode leading DotAll option

**Status:** internal syntax slice under [issue #7](https://github.com/uniblab/Icod.Expressions/issues/7). No public API, consumer switch, or package capability claim.

## Intent and scope

Recognize `(?s)` only at byte offset zero. It enables DotAll for every unescaped dot in the remainder of the pattern, including every top-level alternative: dot then matches all 256 byte values, including LF, while preserving byte offsets and the existing work/cancellation limits. The option itself is zero width and does not allocate a capture number. Grep's `-zP` fixtures `(?s)a.b` and `(?s)^a\nb$` motivate this slice. Without the prefix, dot continues to exclude LF. Escaped dots and bracket classes are unchanged.

This is not general inline option support. `(?s)` after any earlier atom, scoped `(?s:...)`, disabling `(?-s)`, other flags, and combined settings remain explicit `UnsupportedSyntax` when encountered as special groups. No PCRE.NET or .NET Regex fallback. Parsing a malformed prefix such as `(?s` remains unsupported until the broader option grammar is designed.

## Approach

The parser consumes one exact leading `(?s)` prefix before parsing alternatives and remembers a compile-time DotAll flag. Dot atoms use an immutable all-byte set when the flag is on. This reuses the existing class instruction and its per-byte work charge; the matcher needs no new state. The alternative would be an option-setting opcode with scope and backtracking state, which is unnecessary for a prefix whose effect is fixed for the whole pattern. PCRE2's [internal option specification](https://www.pcre.org/current/doc/html/pcre2pattern.html) defines `s` as DotAll and describes option propagation across alternatives.

## Verification

Check the Grep-shaped newline fixtures, ordinary dot behavior, top-level alternatives, captures, raw NUL and high bytes, escaped dots/classes, unsupported flag forms, and work/cancellation behavior. Run full net8/net9/net10 CI on six runners and the frozen GNU corpus. This internal slice does not change Grep or CommandFramework production code.
