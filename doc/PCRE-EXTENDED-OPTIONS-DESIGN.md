# Internal PCRE extended options and comments

## Goal

Advance issue #7 with pattern-side x/xx option scopes and comments, using
PCRE.NET 1.6.0 / PCRE2 10.48 as the test-only oracle. Reuse the existing parser,
syntax nodes, compiler and matcher. Retain original source-byte diagnostics.

## Semantics and boundaries

x ignores unescaped pattern whitespace outside classes and permits # comments
through the next configured newline or end of pattern. xx additionally ignores
unescaped literal SP/HT inside ordinary brackets. Escaped characters and
backslash-Q/backslash-E quoted regions remain literal. Ordinary (?#...) comments
work independently of x and end at the first closing parenthesis; their body
has no escape or nested-group semantics. Comments create no capture or atom.

Flags use the existing set/unset/scoped reader, group restoration and compile-
time propagation across alternatives. Unsetting either x or xx clears both.
An explicit x-only setting downgrades an active xx; an unrelated flag leaves
the mode intact. Only adjacent xx within one setting selects the wider class
mode, so xix is x while xxx is xx. Internal caller-side options
are unchanged. Comments and ignored space may separate atoms, quantifiers and
suffixes where PCRE2 allows, but cannot split group headers, options, escapes,
names, POSIX/property tokens or the numeric contents of counted bounds.
Counted bounds retain the existing independent edge SP/HT rule.

Use the pinned Unicode Pattern_White_Space set outside classes, including the
UTF codepoints supported by PCRE2, rather than runtime Unicode categories. Raw byte 0x85 is ignored even without
UTF; in non-UTF C2 85, C2 remains literal and 85 is ignored. NBSP and EM SPACE
remain literal; invalid UTF pattern bytes still fail global validation.
The internal LF and AnyCRLF newline conventions control # comment termination;
BSR and DotAll do not. Ordinary classes keep #, parentheses and comment-looking
text literal. xx class skipping must preserve leading negation/closing-bracket
rules, range endpoints, trailing hyphens, quoted/escaped spaces and POSIX tokens.
Empty backslash-Q/backslash-E pairs and orphan backslash-E markers do not occupy
the initial class position before or after negation; nonempty quotes remain
literal. Independent review identified this boundary and pinned regressions.
Fixed-literal assertions inherit the active lexical mode; broader assertion
bodies and option changes inside them remain pending.

Empty/reset options, J and extra flags, mixed/scoped aD, extended class algebra,
general assertions, full literal-brace grammar, Unicode table parity, public
API/release and consumer migration gates remain separate. No public/version/
dependency/consumer or VM-frame changes. Keep 256-level heap parser nesting,
capture/expansion limits, cancellation, work accounting and UTF fragment rules.

## Approaches

1. Remove whitespace/comments before parsing: loses source locations and can
   change quoted, escaped, class, header and numeric-token meanings.
2. Add a second lexer/parser path for extended patterns: duplicates grammar and
   risks divergence for captures and byte/UTF modes.
3. **Choose source-local trivia readers.** Add a parser partial for lexical
   skipping at established token boundaries. Share it between group traversal,
   atom suffixes and literal assertions; use a class-only SP/HT reader for xx.
   Keep original indexes and check cancellation while scanning comments/space.

Snapshot the extended mode in the existing heap GroupOptions. Avoid runtime
option instructions, retained spans, interior pointers or cloned engines.
Bracket range lookahead must recognize the next nonignored endpoint without
turning a trailing hyphen into a range or consuming a closing bracket.

## Verification

Test first against the pinned oracle: x/xx transitions, scope/alternative
propagation, capture/reference spans, line and parenthesized comments, all
pattern-whitespace members and selected nonmembers, byte/UTF brackets/ranges,
quoting/escaped spaces, quantified atoms/suffixes, counted bounds, fixed
assertions, LF/AnyCRLF, invalid UTF/CRLF offsets, depth, diagnostics, work and
cancellation. Replace obsolete pending rows, retain other pending families.
Require six green Staging jobs on the final head, net8/net9/net10, frozen,
workload, API/extraction and package-only consumer checks. No local SDK exists.
