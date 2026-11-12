# Escaped CR compile option design

## Intent and native contract

After merged PR #226, add internal caller option EscapedCrIsLf, false by default,
for PCRE2_EXTRA_ESCAPED_CR_IS_LF (0x10). Native check_escape changes the positive
escape-table value for the textual r escape from 13 to 10. It does not change raw
CR, backslash followed by raw CR, hex/braced-octal/control CR, quoted text, raw
verb names, R semantics or the independent newline convention. It does not
introduce a pattern-side directive. Canonical LF must participate in ranges,
class metadata, captures, assertions, calls and repeats exactly as existing n.

## Selected approach

Copy one readonly bool from PcreByteOptions into the existing parser. Select LF
instead of CR in the existing simple-escape table, preserving the complete
source bytes and diagnostic indices. Every downstream typed node, width,
class/range/UTF metadata, compiler and matcher path remains shared. This is my
implementation choice under the approved inline roadmap. Rewriting source would
change byte offsets, quotes and raw verb names; a runtime mode/opcode would
repeat a compile-time choice and add unnecessary state.

## Reference evidence and review focus

1495 controls include 1500 independently checked native PCRE2 10.48 profiles:
1474 JIT-valid profiles and 26 expected descending-range error profiles,
which reduce to six distinct compile-error theory cases. Four byte/UTF/
flag modes span basic escapes, source CR, quotes, classes/ranges/Perl algebra,
scopes, assertions/calls/references, quantifiers, labels and six newline policies;
additional UTS profiles use native activation. Extra controls check all 256 byte
values, every selected malformed-UTF/CRLF offset, other caller policies, UTF
validation and original byte diagnostics, owned source/options/verb names,
canonical instruction/work equality, cancellation and resource ceilings.

Review five input classes: mapping only r and preserving other CR spellings;
canonical range/class/newline metadata; lexical comments/quotes and raw names;
original source/UTF validation offsets; caller isolation and bounded work/state.
Each has direct controls. Keep all existing references unchanged; freeze the new
file only after clean-build expected RED before production changes.

Preserve GNU/BRE/ERE, 35 frozen source blobs/214 frozen outcomes/120 methods,
exported API and dependencies. No Python, subagents, SDK installs or runtime
fallback. User owns merge/tag/publication. Other directives/compile modes,
failed/partial labels, UTF C, wider optimizer/resources, UTF16/API and realistic
consumer/release qualification remain open; Grep stays last.
