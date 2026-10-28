# PCRE literal quoting design

**Status:** issue #7 internal conformance slice. No public PCRE capability or Grep migration claim.

## Intent and approaches

Recognize `\Q...\E` as a literal run in ordinary patterns, bracket classes, and the existing fixed literal lookaround subset. PCRE2 treats a backslash, syntax punctuation, and whitespace between the delimiters literally. An isolated `\E` has no effect; an unterminated `\Q` quotes to the pattern end. In a bracket, this leaves the bracket unclosed if its `]` is quoted. UTF-8 patterns retain scalar literals and authoritative byte spans.

The parser will track whether it is quoting in each syntactic context, consume `\Q`/`\E` without creating VM instructions, and feed quoted bytes or scalars to the existing literal nodes. This preserves quantifier binding to the last literal after `\E` and avoids a second matcher. A preprocessor that rewrites the pattern was considered but would obscure byte diagnostics and context-dependent bracket behavior. Reusing the ordinary escape parser for quoted content was considered but would interpret backslashes as escapes.

## Boundaries

- Keep source-byte offsets, caseless matching, UTF validation, work limits, and cancellation from the current parser and VM.
- Treat special syntax inside quotes as literals, including `|`, `)`, `]`, and `*`; after `\E`, ordinary syntax resumes.
- A hyphen inside the quoted run is a member, not a range operator. A quoted character can be a range endpoint if the hyphen is outside the quoted run; the parser must resume range recognition at `\E` and accept `\Q` immediately after the hyphen.
- Support quotes inside the current fixed literal lookaround body without broadening general lookaround syntax.
- Keep other pending escapes unsupported. No production edits to Grep or CommandFramework.

## Verification

Compare whole-match byte spans with pinned PCRE2 10.48 for byte, UTF-8, caseless, quantifier, bracket, and lookaround cases; assert malformed unterminated bracket diagnostics. Run the complete six-job Staging PR matrix.
