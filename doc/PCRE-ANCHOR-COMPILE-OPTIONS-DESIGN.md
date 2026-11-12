# Anchor compile options

Continue after merged PR231 with internal default-false DollarEndOnly
(PCRE2_DOLLAR_ENDONLY,0x10) and AltCircumflex (PCRE2_ALT_CIRCUMFLEX,0x200000).
DollarEndOnly makes non-multiline dollar require the selected subject end,
without changing backslash-Z/z. Multiline dollar ignores it. AltCircumflex
permits multiline caret after a final newline; non-multiline caret is unchanged.
Both global compile flags survive local m unsets/reset; local multiline state
selects each source anchor. MatchLine's guards use its existing entry snapshot.

## Approach

My choice under the approved inline roadmap is canonical anchor lowering.
Private CreateStartAnchor(bool multiline) and CreateEndAnchor(bool multiline)
serve ordinary source and implicit line wrappers. DollarEndOnly selects the
existing absolute-end node outside multiline. Existing LineAnchor gains only a
default-false AllowSubjectEnd init property, lowered into the unused First operand
of StartLine. The matcher extends the existing end bound for First=1, preserving
newline recognition, fragment start/context, excluded candidate and Step charging.
No new AST node, opcode or program-global runtime mode is needed.

A separate opcode would duplicate the canonical guard; filtering completed
matches would lose assertion/backtracking and capture/cut behavior. Rewriting
source would break prefixes and original-byte diagnostics. These are rejected.

## Native evidence and boundaries

PCRE2 10.48 JIT OP_DOLL tests endonly only for non-multiline dollar; OP_CIRCM skips
its STR_END rejection with alt_circumflex. Pin literal JIT spans/captures/labels
for all four selections, initial/local/reset m, six newline conventions, empty
subjects/lines, absolute anchors, assertions/lookbehind/calls, cuts/ACCEPT/K and
line-wrapper entry policies. Supplement every byte, every selected malformed-UTF/
CRLF/Unicode-newline start offset, exact diagnostics, old caller policies,
owned flags/source/raw labels, canonical work/cancellation and resource limits.
Precompile supplemental native arrays before managed comparisons. Freeze only
after clean expected tests-only RED, with default/legacy/native controls passing.

Five review boundaries: local multiline versus global flags and wrapper entry;
final CRLF/ANY/Unicode newline endpoints; assertion/reverse/fragment endpoint
context; source offsets, captures/calls/cuts and option/source ownership; shared
candidate search, work, cancellation and existing limits.

Preserve earlier tests,35 frozen production blobs and214 frozen outcomes in120
methods, GNU/BRE/ERE, exported API and dependencies. C#13/net8/9/10,UTF-8+LF.
No Python, subagents or SDK installs. User owns merges/tags/publication.
Remaining syntax/modes, optimizer/resources, UTF16/API and consumer/release gates
remain open. Grep stays last.
