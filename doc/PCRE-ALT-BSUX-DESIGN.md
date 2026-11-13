# Alternate backslash-u/U/x grammar

Add default-false internal caller AltBsux, matching PCRE2 10.48 ALT_BSUX.
A complete adjacent four-digit u escape yields one character. U yields literal
uppercase U. An x escape requires two adjacent hex digits. When the required
sequence is incomplete or contains a nonhex character, only the escaped letter
is consumed and the following bytes retain their ordinary grammar.

## Approach

Snapshot the caller flag in the parser. Dispatch only x/u/U tokens to a focused
partial-file reader; validate all required digits with a local cursor before
committing the source index or checking character bounds. U needs no lookahead.
Byte mode caps values at255; UTF rejects surrogate values, including adjacent
surrogate escapes. Return existing literal nodes. No new AST/IR/matcher or source
rewriting: a decoded backslash must never re-tokenize subsequent source.

The standard flag does not enable EXTRA_ALT_BSUX braced-u syntax. A source u or
x fallback followed by a valid counted repeat uses the existing quantifier
reader; non-counted braces remain ordinary text. Classes, ranges, Perl algebra,
case scopes, assertions/calls/captures, widths, source-newline metadata and work
continue through existing paths. Default grammar, inert quotes/comments/raw MARK
names and existing compiler restrictions retain their behavior. Invalid alternate
values use InvalidSyntax at the existing owning escape/class diagnostic boundary.

A replacement pass over pattern text would lose source ownership and could turn
produced backslashes into syntax. A separate matching path would duplicate
already-qualified literal semantics. Both are rejected in favor of the reader.

## Qualification

Pin native-first error/offset and match/capture/label fixtures for byte/UTF/UCP,
malformed-UTF tolerance, casing, extended whitespace, exact/incomplete/malformed
escapes, bounds, braces/repeats and grammar contexts. Cover all256 values,
all selected offsets/newlines/fragments, strict default diagnostics, ownership,
canonical work/cancellation/structural quotas and prior caller prohibitions.
Observe clean tests-only RED, freeze, then implement the flag and reader.
Require all18 native results and supporting gates before readiness. Final source
identity, review and GREEN evidence live on the PR and issue7.

C#13/net8/9/10; UTF8+LF; no Python/subagents/SDK installs. Preserve all249 earlier
test files, frozen35/214 outcomes/120 methods, GNU/BRE/ERE, matcher/API/dependencies/
workflows and current limits. User owns merge/tag/publication; Grep last.
