# Literal reference escapes in ordinary brackets

## Intent and native contract

Close the explicit pending bracket forms of `\g` and `\k` after #105.
Pinned PCRE.NET1.6.0 / PCRE2 10.48 `pcre2_compile.c` check_escape CHAR_g
(lines1721..1747) and class ESC_k (around4490) define both as literal letters
inside ordinary brackets. Only the escaped letter is consumed. Following
braces, numbers, quotes and angle delimiters are ordinary class source;
no capture lookup or subroutine/reference parsing occurs there.

The existing literal bracket paths then own union/negation/ranges, i/r folding,
x/xx trivia, quoting, POSIX unions, source-newline metadata, lookbehind widths,
captures, repetitions and all caller-offset/fragment/work protocols. Outside
brackets the current reference/subroutine grammar and diagnostics are unchanged.
No other unknown escapes become literals, and extended class algebra stays
pending. Original source bytes/diagnostic precedence remain unchanged.

## Approach and scope

Chosen by Codex under the standing autonomous development remit: return the
existing PcreByteLiteral(value) from each existing g/k branch when inBracket,
before reading any delimiter. This is the only needed semantic change.
Rewriting source would complicate offsets; introducing class-specific runtime
nodes would duplicate existing literal semantics and resource accounting.
Neither is justified. No AST/opcode/frame/journal/matcher change or fallback.

Also qualify the three start-directive documentation passages deferred by
#105: inherited direct-lookbehind UnsupportedSyntax can report the enclosing
assertion opening rather than the misplaced directive opening. Preserve the
existing behavior; this is a prose correction, not a diagnostic change.

## Verification and constraints

Literal native-first membership sets and all-capture spans precede managed
comparison. Verify all256 byte values, strict UTF scalar probes, escaped
letters alone and followed by reference-looking delimiters/numbers; positive,
negative, ranges, i/r, x/xx/quotes and POSIX class contexts. Exercise current
capture/call/assertion/width/repetition paths and every offset over tolerant
UTF/CRLF subjects. Pin malformed class offsets and unchanged outside-reference
controls. Work observers/budget/cancellation must equal unescaped class letters.

Tests-only CI must show the expected missing bracket-literal feature before
production changes. Self-review the whole branch separately after implementation
(user explicitly prohibits subagents). Require all six native Staging jobs,
full net8/net9/net10 plus frozen214 and configured gates; exact published tree,
whitespace and frozen35 checks. No local SDK; managed RED/GREEN uses CI.

C#13; source src, tests tests, docs doc. Local Debug. No Python, public API,
package/version/dependency/workflow/release/consumer changes. Other escapes,
Unicode data parity, #99/#102 optimizer gaps and #100 unsafe native Xuc remain
separate. Grep stays last; CommandFramework removal requires publication and
migration of all known consumers. User owns merge. Fixture/helper organization
is left to the implementer; no new engine policy is needed.
