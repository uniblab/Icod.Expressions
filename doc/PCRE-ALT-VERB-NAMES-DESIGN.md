# Alternate control-verb names

Implement default-false internal AltVerbNames for PCRE2 10.48 ALT_VERBNAMES.
Decode literal escapes and local Q/E quoting in MARK/shorthand, named SKIP,
PRUNE/COMMIT/THEN and ACCEPT/FAIL/F names. Only literal data escapes are valid;
character types, anchors and references are invalid name syntax. Names remain
case-sensitive regardless of regex casing. Extended/More whitespace and comments
are ignored only with this caller policy, outside quotes; escaped whitespace and
decoded backslashes are data. Raw UTF characters and escaped UTF literals retain
byte-authoritative label encoding.

## Approach

Snapshot the caller bool and gate ReadControlVerbName to a focused partial-file
reader. Keep the default raw reader untouched. Reuse canonical literal escape
parsing for controls/numeric/octal/hex/N/U/u, including the existing temporary
alternate literal-brace marker, and reject non-data escape kinds before they can
invoke token policies such as NeverBackslashC. Decode escaped UTF source scalars
locally because ordinary regex escaped-UTF support remains a separate boundary.
Keep name Q/E state local; it cannot leak to outer regex quoting. Advance raw UTF
at character boundaries before considering extended trivia.

Add allowInlineComments=true to the private SkipIgnoredPatternText helper. Names
call it with false: inline (?#...) is label data, while x-mode # comments still
use the shared newline policy. This avoids duplicating whitespace/newline logic
without allowing name text to enter the regex grammar.

The255-byte limit and MARK's nonempty requirement count original source bytes,
including escapes, quote delimiters and ignored trivia, and apply only at the
real unquoted closing parenthesis. Earlier escape errors retain precedence over
length. Preserve a nonempty argument that decodes empty with the existing NUL
label representation: raw NUL and decoded-empty names have equivalent native
CString identity, distinct from an absent/empty colon argument. This preserves
named SKIP and implicit MARK transactions without new AST/IR/matcher flags.

Source rewriting would lose raw quotas/offsets, re-tokenize decoded delimiters
and misclassify empty arguments. A separate label runtime would duplicate the
already-qualified name/cut journals. Reuse owned byte names and canonical nodes.
Five production files: options, parser snapshot, raw-reader gate, optional trivia
parameter and the new decoder partial. No matcher/IR/journal/public API/dependency/
workflow/shared-limit changes.

## Qualification

Pin native-first byte spans/captures/success labels and compile errors/offsets;
distinguish no-match(-1) from strict UTF errors and never assert failed labels as
an implemented result. Cover all named verbs, escaped closes/quotes, true/decoded
empty arguments and NUL identity, raw255/256 limits/error priority, literal and
forbidden/numeric escapes, caller escape policies, byte/UTF/UCP/casing/extended,
comments/newlines, scope restoration, source/options ownership and shared work/
cancellation/structural quotas. Preflight every native profile, observe warning-
free tests-only RED with only managed missing-policy failures, then freeze. Run
all18 native framework/platform results and every supporting gate. Include all
repository docs before qualification; final GREEN/review/base/head/merge identity
is recorded on the PR and issue7 without a documentation-only CI repeat.

Baseline merged237 f9b28d2a9a31342a46b3e92679d82ce11f8f5028:66,451tests/framework,
251 earlier files under tests. C#13/net8/9/10; UTF8+LF; no Python/subagents/SDK
installs. Preserve frozen35/214 outcomes/120 methods, GNU/BRE/ERE, all existing
limits and unsupported boundaries. User merges/tags/publishes; Grep last.
