# Opt-in literal recovery for bad escapes

Add default-false internal BadEscapeIsLiteral for PCRE2 10.48 extra-option2.
Eligible malformed/unrecognized escape failures become one canonical literal:
the character immediately after the backslash, or a backslash at EOF. Rewind to
just after that character; all remaining source retains ordinary lexing, scopes,
quantifier ownership and original byte offsets. For example a byte-mode invalid
hex value can expose a counted repeat after the literal x. Do not rewrite source.

## Approach

The builder chooses a transactional wrapper around the existing ParseEscape,
renamed ParseEscapeCore. The false path calls the core directly. The selected
path snapshots source index and pending reference/subroutine state, calls the
same core, and handles only InvalidSyntax/UnsupportedSyntax failures whose native
stage permits recovery. Cancellation and resource diagnostics are never caught.
Successful escapes keep their existing nodes and state.

Recovery applies to unknown ASCII letters, malformed data/control/octal/hex/
alternate escapes, ordinary numeric/name-reference syntax, and outside-class
property failures. Native NeverBackslashC is recoverable outside brackets.
Unknown targets resolved after parsing, valid-but-unimplemented UTF C/escaped
UTF, class membership errors, class property failures, compiler guard failures
and resource limits remain errors. Numeric references need a read-only native
grammar check against65535, current group count and native delimiter/sign rules:
the library's256-capture ceiling must not make a valid native reference literal.
The probe moves only a local cursor and polls cancellation.

Literal recovery is suppressed for alternate verb names and all Perl extended
class operands. A scoped suppression flag in ParsePerlClass restores on exit;
UTS classes retain recovery, including nested operands. Only eligibility and
cursor/state rollback are new; reuse canonical literals and existing class,
range, quote, comparison, assertion, work and capture machinery. Keep source
validation and caller UTF/UCP guards before this policy.

## Qualification

Pin native-first spans/captures/labels and compile diagnostics/offsets for all
ASCII escape letters, malformed data tokens, quantifier ownership, references
at native/library bounds, properties, ordinary/Perl/UTS classes, raw/decoded
labels, valid restrictions and numeric policies. Cover case/reset/extended scopes,
UTF/newline/malformed offsets, ownership, transactional state, cancellation and
existing structural/resource limits. Explicitly preserve currently unsupported
valid native syntax, including UTF code-unit execution and ordinary escaped UTF.

Observe clean three-framework tests-only RED, freeze the fixture before production,
then qualify all18 native results and supporting gates. Include all repository
inventories before the final source head; final GREEN/review/base/head/tree/merge
evidence belongs in PR240 and only issue7's roadmap footer. User owns merge/tag/
publication; Grep last. The PR239 tolerant-surrogate exclusions stay explicit.

Baseline merged239 6e0d56f3962100f0f48657d9d7af61d203d46ad3,
tree25c5d9d3c4095f12e45f233234cf862758d2b755;69,855 tests/framework and255
earlier test files. Preserve GNU/BRE/ERE, frozen35/214 outcomes/120 methods,
canonical AST/instruction types/opcodes, matcher/journals/shared limits/public
API/dependencies/workflows. C#13/net8/9/10, UTF8+LF; no Python/subagents/SDK
installs. Reuse the isolated linked worktree and execute inline without pauses.
