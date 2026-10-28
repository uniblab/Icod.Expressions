# Internal PCRE named captures

## Intent and boundary

Advance issue #7 with capture names and references in the internal byte/UTF-8
compiler. Names alias the ordinary left-to-right capture numbers. Match results
continue to expose numbered source-byte spans. This is not a public API or
release change, and does not migrate Grep or change CommandFramework.

Support `(?<name>...)`, `(?'name'...)`, `(?P<name>...)`, and references
`\k<name>`, `\k'name'`, `\k{name}`, `\g{name}`, `(?P=name)`.
Resolve forward names after parsing; an unset target still fails at match time.
Unique names are ordinal and case sensitive, including under caseless matching.
Duplicate names without the pending duplicate-name option are invalid.

Follow the [PCRE2 pattern specification](https://www.pcre.org/current/doc/html/pcre2pattern.html):
names have at most 128 code units (bytes in this 8-bit compiler). Byte names use
ASCII letters, digits and underscore with a nondigit first character. UTF names
also permit Unicode letters and decimal digits, with no leading digit. Unicode
categories use the existing runtime policy; pinned-version table parity remains
a release gate. Escapes are not expanded inside names. Braced references ignore
space/tab after the opening brace and before the closing brace; other delimiters
do not ignore whitespace.

General assertions, duplicate-name options, branch reset, recursion and named
subroutine calls remain pending. In particular `\g<name>` and `\g'name'` are
subroutine syntax, not aliases for `\g{name}`. Literal `\g`/`\k` interpretation
in ordinary brackets remains pending; general lookaround bodies retain
UnsupportedSyntax. This bracket distinction was confirmed by the first reference
run and the pinned 10.48 `pcre2_compile.c` escape handling.

## Approaches

1. Add runtime name lookup and named capture state: unnecessary match overhead
   and a second identity system for captures.
2. Rewrite pattern text into numbered syntax: complicates byte diagnostics,
   quoting, numbering and forward references.
3. **Choose parser resolution to the existing numbered nodes.** A temporary
   immutable named-reference node is resolved after all declarations are known.
   Existing VM instructions, capture journal, limits and matching budgets apply.

Keep name parsing/resolution in a partial parser file. The AST resolution pass
only runs when named references occur, honors cancellation and traverses the
already depth-limited group tree. Names are compilation data only; no names,
interior pointers or spans enter backtracking frames or the compiled program.

## Conformance findings during implementation

The invalid-UTF offset fixture exposed an existing numbered-reference gap:
`(a?)\g{1}` can match an empty capture at a requested continuation byte, but
the program's empty-path analysis excluded all backreferences. Include references
as potentially empty so the existing VM can try the zero-width candidate. The VM
still rejects unset targets and nonempty references across fragment barriers.
The regression is pinned in both named and numbered syntax; no matcher change
or new instruction is needed.

The `(?<*...)` non-atomic lookbehind prefix must retain UnsupportedSyntax.
Disambiguating angle-bracket names therefore excludes `=`, `!` and `*`.

## Evidence

Pin all three declarations and five references, mixed numbering, forward and
unset targets, branch/repetition/atomic rollback, K reporting, UTF scalar widths,
invalid-UTF fragment barriers, quoting, name validation and 128-byte boundaries
against test-only PCRE.NET 1.6.0 / PCRE2 10.48. Compare every numbered capture,
not just the whole match. Preserve compile/capture limits and cancellation.
Six OS/architecture Staging jobs must pass the full net8/net9/net10 suite,
frozen corpus and current package/workload gates before review readiness.
