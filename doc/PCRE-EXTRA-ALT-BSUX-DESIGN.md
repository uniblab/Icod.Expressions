# Extra alternate backslash grammar

Implement default-false internal ExtraAltBsux for PCRE2 10.48 EXTRA_ALT_BSUX.
It implies standard alternate u/U/x grammar independently of AltBsux and adds
adjacent braced-u hex characters. Complete values use byte255 or UTF10ffff bounds;
UTF surrogates remain invalid. Arbitrarily many leading zeros are allowed.
Whitespace is never part of a braced hex token.

## Approach

Extend the existing focused reader and snapshot both caller policies. Read braced
hex with a local uint cursor, cancellation polls and the native pre-shift32-bit
check. Bounds/surrogates apply only after a complete closing brace, but true32-bit
overflow fails even in a malformed token. Ordinary malformed tokens leave the
source suffix untouched. In a class, return literal u without consuming the brace.
Outside classes, consume the opening brace and return a parser-private marker;
AppendNode flattens it immediately into literal u followed by literal brace,
then applies existing quantifier processing only to the brace. Both literals
retain the atom's case policy. No marker reaches the canonical tree or IR.
This preserves native ESC_ub behavior for whitespace, comma and missing-close
fallbacks and for quantifiers separated by extended trivia.

Source rewriting would lose offsets and could turn decoded backslashes into new
syntax. A synthetic noncapturing group would repeat both fallback characters,
change literal assertion/prefix shapes and add unnecessary scope. A pending global
brace override would couple the escape reader to later tokenization. The private
marker is local to one atom/append operation and uses the existing parser pattern
for temporary nodes. No persistent AST/IR/matcher/journal/work/quota/API changes.

## Qualification

Native-first fixed span/capture/label, compile-error/offset and strict-subject
fixtures cover independent/both flags, default and standard controls, complete/
incomplete/malformed/whitespace tokens, zero padding, uint/scalar boundaries,
classes/ranges/Perl/UTS algebra, quantifier ownership, quoting/comments/control
operands/raw labels, case/reset scopes, calls/assertions and prior restrictions.
Cover all256 values, selected offsets/newlines/fragments, strict UTF errors,
owned source/options and shared work/cancellation/structural quotas. Distinguish
native no-match(-1) from every other negative match result; deduplicate compile
errors independently of subjects. Observe warning-free tests-only RED containing
only missing-feature managed failures, then freeze before production. Qualify all
18 native platform/framework results and every supporting gate. Put final GREEN,
author review and exact source/base/merge identities on the PR and issue7, with
all repo docs included before the qualified commit.

Baseline merged236 9d55f31241df69aa4262eecbd9a60dec3bbc2689,65,470 tests/framework,
250 earlier files under tests. C#13/net8/9/10; UTF8+LF; no Python/subagents/SDK
installs. Preserve frozen35/214 outcomes/120 methods, GNU/BRE/ERE, dependencies,
workflows and public API. User owns merge/tag/publication; Grep stays last.
