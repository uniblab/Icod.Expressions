# Byte Unicode property design

## Goal and scope

Continue issue #7 after merged #112. Accept the existing explicit general-category
and Any/Xan/Xps/Xsp/Xwd/Xuc property grammar in non-UTF byte mode. A byte value
denotes its same-valued Unicode code point U+0000..U+00FF and consumes one byte,
including NUL and bytes that would be malformed UTF-8. Properties are independent
of UCP, ASCII restrictions and r; i widens Lu/Ll/Lt to Lc before complementation,
as in pinned PCRE2 10.48. Existing UTF semantics remain unchanged.

UCP without UTF remains rejected: it also changes literal/range/reference folding
and word/shorthand/POSIX semantics, and needs a separate complete implementation.
Scripts/binary properties, Unicode-version reconciliation, extended class algebra,
public API/release and consumer migration remain separate gates.

## Approaches and maintainer choice

1. Teach the runtime ScalarClass opcode to consume bytes and create byte bracket
predicate storage. This retains dynamic property lookup but adds runtime paths
and duplicates ordinary byte-set union/negation.
2. Project shared scalar membership over 256 code points during compilation.
This reuses immutable byte sets, one-byte widths, existing class instruction and
all current matching/rollback paths. Chosen by the maintainer under standing
autonomous authorization; user has not separately selected this implementation.

Move the existing pure evaluator (including its two category helpers), unchanged,
from Matcher to PcreScalarClassMembership.Contains. Both UTF execution and byte
property projection call it. No second category table, parser or VM.

## Grammar, sets and provenance

ParseUnicodeProperty resolves the same short/braced loose category/special names,
P/caret complements and category-only namespace aliases. Its two success exits
call a small CreatePropertyClass(PcreByteScalarClass) factory. UTF returns the
semantic scalar node unchanged. Byte mode enumerates 0..255 using Contains with
the current compile-time caseless flag, applies the property's negation, and
returns PcreByteClass. Poll cancellation on every projected byte.

Byte brackets already union escaped PcreByteClass sets and apply outer negation.
Property terms are not literal range endpoints and must not set explicit-CR/LF
source metadata. Standalone property classes retain PrefixAny=false even for Any;
ordinary brackets keep existing member/source provenance and all-byte prefix
handling. Pin observable native CRLF candidates, mixed explicit literals, erased
and DEFINE bodies; broader optimization remains #99/#102.

Category namespace aliases gc:/gc=/General_Category are the existing library
extension, now shared across widths; only categories are accepted after those
prefixes. Native-valid unsupported script/binary forms remain UnsupportedSyntax
at the original escape byte (byte brackets retain their existing escape-owner
policy). Empty/unclosed names and property range endpoints remain InvalidSyntax.
A prefix/source is never decoded or rewritten in byte mode.

## Widths, state and resources

Projection examines exactly256 values per property, under existing source,
capture256, depth256, instruction4096, repeat-mark256, saved-frame1024 and default
work1,000,000 limits. The resulting immutable set is32 bytes. Runtime classification
uses the existing Class opcode and charges the same work as an equivalent ordinary
byte class; no frames/journal/spans/pointers/capture copies or new opcode.
Quantifiers, assertions, calls/recursion, backreferences, captures and one-byte
behind widths follow existing semantic classes. Strict UTF source validation
and tolerant-fragment barriers are unchanged.

C#13; net8/net9/net10; UTF-8 LF; root project/solution, src, tests, samples.
No Python/SDK installation. No package/version/dependency/workflow/public/consumer
changes, engine cloning or production native/Regex fallback. Grep last;
CommandFramework removal after publication and all known consumer migrations.

## Evidence and delivery

Primary pinned sources: PCRE.NET1.6.0 source commit
09639d45d4c03a8269fa2cde6edc7a7383bbc18c; PCRE2 10.48 pcre2pattern.3
property/category section (Lu/Ll/Lt case widening at979..986), pcre2_xclass.c
property membership, compiler property handling. Native10.48 Compiled/JIT is
the behavioral oracle; batch probes validate all256 byte subjects and bound spans.

Tests-only draft precedes observed expected RED, then production. Literal native
membership masks cover every supported category/special property, case modes,
P/caret/bracket complements; capture-span fixtures cover interactions, offsets,
CRLF metadata and bytes that are malformed UTF. Grammar, profile, work/cancellation,
compile/resource and UTF regression gates remain explicit. No unsafe UTF Xuc probes.

Separate design/builder/plan checks and final whole-branch AUTHOR self-review:
no subagents by user instruction, no independent-review claim. All six native
Staging jobs must pass full/frozen214 on net8/9/10, no failed/skipped tests or build
warnings/errors, configured extraction/workload/API/package gates. Verify exact
reviewed head/tree, frozen35, whitespace/clean status before ready; user owns merge.
Preserve worktree and ignored evidence.
