# Internal PCRE atomic group and lookaround aliases

## Intent and approach

Continue issue #7 after #86 by accepting PCRE2's alternative spellings for
implemented atomic groups and atomic assertions. Match PCRE.NET1.6.0 / PCRE2
10.48 Compiled/JIT using the same syntax tree, compiler and VM. Select a small
transactional group-header reader integrated with existing heap GroupFrame
construction. Rewriting pattern text would obscure source-byte diagnostics;
separate alias nodes or VM opcodes would duplicate identical semantics. This
choice follows the standing autonomous development remit.

## Supported headers

| Alias prefix | Existing semantic form |
|---|---|
| `(*atomic:` | `(?>` |
| `(*pla:`, `(*positive_lookahead:` | `(?=` |
| `(*nla:`, `(*negative_lookahead:` | `(?!` |
| `(*plb:`, `(*positive_lookbehind:` | `(?<=` |
| `(*nlb:`, `(*negative_lookbehind:` | `(?<!` |

The nine exact lowercase prefixes include the colon. No trivia, option folding,
case folding or partial keyword matches inside a header, even under i/x/xx.
Once recognized, normal body trivia/quotes/options apply. An alias allocates no
capture slot and exactly one existing group nesting level. The alias itself is
repeatable under the same atomic-group or direct-assertion rules as its existing
form, including U/lazy/possessive preference and unlimited-assertion normalization.

## Parser integration

Add private TryParseGroupAlias(out bool atomic, out int assertionKind) in a
focused partial parser file. It starts at index immediately after '('; on an
exact match advances past the colon, otherwise leaves index unchanged. Kinds
are existing signed1 ahead/signed2 behind. Check cancellation while trying the
finite prefix set; no string decoding or pattern transformation.

ParseGroupOpening snapshots entry options before reading a header. If an alias
matches, suppress capturing, retain atomic/kind metadata, and set the assertion
opening context to the original '(' byte only if there is no enclosing assertion.
Then use the same depth/capture guards and GroupFrame constructor as canonical
groups. Otherwise preserve the existing unsupported (*...) handling and ordinary
(?...) parsing. All group exits, alternative option propagation, local i/r/J,
named-reference fixups, branch-reset numbering, literal assertion optimization,
deferred behind analysis, capture rollback and source offsets remain shared.

Unclosed recognized bodies use the existing original-source outermost-unclosed/
enclosing-assertion opener policy; K anywhere in an alias
assertion inherits the existing InvalidSyntax assertion-opening context. Unsupported
children in a direct behind retain the existing owning-behind diagnostic mapping.
Malformed/unknown complete (*...) headers remain UnsupportedSyntax at their opener;
native rejects malformed spellings. Existing diagnostics need not mimic native
numeric error codes. Unlimited consumed widths, duplicate named references and
global branch-reset references in behind retain their existing controlled rejection.

## Boundaries and verification

Keep non-atomic napla/naplb, long non-atomic names, (?*/(?<*, script runs, verbs,
new directives and subroutines explicitly unsupported. Do not interpret any as
atomic aliases. The complete supported header grammar is only the table above.

Pin explicit native outcomes/group counts and every numbered byte capture:
all aliases versus canonical forms; atomic commitment and capture rollback;
positive/negative and fixed/variable behind; nested body options and i/r/J;
quantification/empty bodies; UTF bytes, malformed UTF/CRLF caller offsets;
quoting/comments versus header recognition; malformed/unclosed headers and K;
consumed behind restrictions, depth256/capture256, saved-path/work/cancellation.
Replace obsolete alias-pending rows; retain non-atomic and other pending controls.
Update current design/syntax/resource/consumer inventories and historical notices.

No VM/IR/opcode/frame/journal/width-engine/API/version/dependency/release/consumer
change. Instructions4096, depth256, capture256, saved paths1024 and default
work1,000,000 remain. Root project/solution, src, tests, samples; UTF-8 LF; no Python.
Local Debug and six-platform Staging conventions remain. No local SDK: observed
red/green uses CI on net8/net9/net10, full plus214 frozen tests and configured
workload/extraction/API/package-consumer gates. Local whitespace/frozen35 checks.
User retains merge control; Grep and CommandFramework migration remain gated.

## Pinned sources

pcre2pattern.3 2344–2349 and2653–2665: atomic and lookaround equivalence;
pcre2pattern.3 2831–2840: distinct non-atomic forms;
pcre2_compile.c verb-name table and group dispatch: aliases lower to shared
atomic/assertion metadata. Native probes check pinned version and nonzero JIT size.
