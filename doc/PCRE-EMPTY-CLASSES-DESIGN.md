# Empty character class compile option design

After merged PR227, add internal caller option `AllowEmptyClass`, default false,
for `PCRE2_ALLOW_EMPTY_CLASS` (0x1). This is a compile option, not pattern syntax.
An initial closing bracket terminates an empty class after the existing optional
negation and permitted trivia: `[]` never matches and `[^]` matches any character,
including newlines. Empty classes remain repeatable, unlike `(*FAIL)`. Default
first-position literal `]` behavior must remain fixed when the option is false.

Native PCRE2 10.48 permits this in ordinary and UTS#18 classes, including nested
UTS operands; it does not permit empty bracket operands inside Perl `(?[...])`
algebra. Quotes, isolated `\E`, empty `\Q\E` runs and xx space/tab obey existing
class-start rules. Source bytes, UTF validation and diagnostic positions stay
unchanged. Negated emptiness uses ALLANY behavior and valid UTF fragments.

## Approach

My implementation choice under the approved inline roadmap: detect an empty
class before ordinary/UTF/UTS dispatch using existing class-negation/trivia
scanning. Restore the index if it is not empty. Lower to existing empty bitmap
class or ALLANY node, matching native lowering. At a nested UTS opening, add an
empty/complemented scalar-set operand through the current bounded term stack.
Perl algebra keeps its current direct reader and grammar. No new AST, opcode,
matcher state or runtime mode is needed.

Simply relaxing closing-bracket conditions would give negated empty classes
ordinary-class metadata instead of native ALLANY semantics. Rewriting the source
would disturb quotes, labels and original offsets. Both are rejected here.

## Evidence and review focus

941 distinct reference cases include independently researched 1,408 native profiles: 818 JIT-valid profiles and 590
error profiles collapsing to 94 distinct compile-error cases. Freeze references
only after clean-build expected RED confirms every failing case depends on the
missing option; default profiles and exact legacy diagnostic controls must pass.
Review five classes: empty versus never-match/zero-repeat behavior; canonical
ALLANY/CRLF/UTF metadata; nested UTS versus Perl algebra grammar; lexical trivia,
quotes and original byte diagnostics; ownership, work/cancellation and quotas.
Each receives direct tests plus native comparison.

Preserve all earlier test files, GNU/BRE/ERE, 35 frozen production blobs and 214
frozen outcomes in 120 methods, exported API and dependencies. C# 13/net8/9/10,
UTF-8+LF. No Python, subagents, local SDK installs or runtime fallback. User owns
merge/tag/publication. Remaining modes/directives/callouts, failed/partial labels,
UTF code-unit escapes, wider optimizer/resources, UTF16/API and realistic
consumer/release qualification remain open; Grep stays last.
