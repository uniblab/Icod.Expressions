# PCRE byte atoms: dot and escapes

**Status:** implementation slice stacked on [the internal PCRE foundation](https://github.com/uniblab/Icod.Expressions/pull/8), under [issue #7](https://github.com/uniblab/Icod.Expressions/issues/7). The compiled API remains internal, with no Grep or CommandFramework production change.

## Intent and scope

Implement the first character-consuming atoms beyond literal bytes in the existing ordered bytecode matcher. This slice uses 8-bit non-UTF mode with the default LF newline convention and no UCP or inline options. Match results remain authoritative byte offsets, with the existing work, stack and cancellation limits.

Supported syntax in this slice:

- Dot matches any byte except LF (0x0A). It can be quantified and captured like any existing literal.
- `\d`, `\D`, `\s`, `\S`, `\w`, `\W` use non-UCP ASCII sets and their byte complements. `\d` is `0` through `9`; `\w` is ASCII letters, digits and `_`; `\s` is space, tab, LF, VT, FF and CR.
- `\a`, `\e`, `\f`, `\n`, `\r`, `\t` produce their ASCII control bytes. Backslash before a non-ASCII-alphanumeric byte produces that literal byte, including escaped metacharacters and backslash.
- `\x` consumes zero to two following ASCII hex digits and produces one byte; no hex digits means NUL. The braced form `\x{...}` remains explicitly unsupported. A third character stays in the input pattern, so `\xFG` means 0x0F followed by `G`.

An unmatched trailing backslash is `InvalidSyntax` at its byte offset. Other letter/digit escapes remain `UnsupportedSyntax` at the backslash, including backreferences, Unicode properties, `\h`, `\v`, `\R` and octal forms. Bracketed character classes, POSIX classes, UTF/UCP and DOTALL stay in later slices. A valid construct outside this subset must never be silently treated as a literal or handed to another regex runtime.

## Approach

| Approach | Consequence | Decision |
|---|---|---|
| Add separate interpreter branches for each shorthand | Small parser change, but matching and inversion rules spread across the VM. | Avoid. |
| Compile dot and shorthand classes to an immutable 256-bit set | One class opcode checks exactly one source byte; complements include NUL and high bytes, and future bracket classes can reuse it. | Choose. |
| Reuse the GNU BRE/ERE class matcher | Shares some code but couples independent semantics and risks changing the frozen GNU implementation. | Avoid. |

The parser creates literal or byte-set AST nodes. Bytecode emission adds a class opcode carrying its set; the VM counts one instruction and one byte comparison as work. The compiled program owns its sets and never retains caller pattern storage. Existing literal, capture, repeat and ordered alternative execution stays intact.

## Reference and verification

Pin examples against the PCRE2 standard matcher in 8-bit non-UTF mode with default LF newline before implementing. Test dot against LF, CR and NUL; ASCII shorthand boundaries and high bytes; escaped punctuation; `\x`, `\xF`, `\xFF`, `\xFG`; trailing slash and pending escapes; quantified classes; capture spans; cancellation and work-limit accounting. The reference engine is used only to establish expected fixtures, not at runtime. [PCRE2 syntax](https://www.pcre.org/current/doc/html/pcre2syntax.html) and [pattern semantics](https://www.pcre.org/current/doc/html/pcre2pattern.html) govern this slice.

CI must pass the full net8/net9/net10 suite on Linux, Windows and macOS, preserve the exact 214-case extraction corpus, and pack one library. Do not publish a PCRE-capable release or migrate Grep based on this subset.
