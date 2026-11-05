# Positive lookahead before mandatory type heads

Continue #102 after merged PR165 (full30065/frozen214). Pinned PCRE2 10.48
`set_start_bits` skips ASSERT and ASSERT_NA across intermediate assertions and
word boundaries when the next significant opcode is a mandatory type such as
HSPACE, DIGIT, WORDCHAR or NOT_WHITESPACE. The existing optional bracket body
proof stops at a positive ahead unless that opcode is a direct literal.

For each optional bracket branch, recognize a positive ahead only when the
first non-skippable node of its finite unquantified prefix is a type already
accepted by `HasUtfCrLfTypeBitmap`. The existing helper restricts admission to
types proven to exclude CR, respecting parser ASCII/UCP classification. Do not
admit arbitrary Unicode properties, negated horizontal space, predicate sets,
general classes, repeats, unsupported or empty branches. The existing literal
case must remain unchanged. This proof is iterative, with cancellation and
bounded pre-addition charges per prefix node. Preserve success-only checkpoints,
all-branch mandatory requirements and existing root-star/PLUS/capture ownership.

Runtime lookaheads and their captures continue through the existing matcher.
No parser, emitter, matcher, Step, frame/journal, API, resource-limit, workflow,
dependency, consumer or CommandFramework changes. Grep remains last; release
and all consumer migrations precede CommandFramework removal.

Native-first matrix: optional capture/noncapture/atomic wrappers, greedy/lazy
counts, assertion/boundary prefixes, horizontal space in ASCII/UCP, ASCII-only
digit/word/nonspace, UCP-excluded controls and CRLF/ordinary subjects. Each
literal fixture pins all capture spans in strict and tolerant UTF, requires
native JIT positive, complete vectors and byte-bounded spans. Publish tests
before production, verify successful-build RED confined to the new class with
all30065 baseline cases green, then freeze the test blob. Final head requires
six OS/arch native jobs/net8/9/10 full+frozen214, no failures/skips/compiler
warnings/errors, all configured component/pack/consumer checks and author
whole-branch review. The full source-pinned Compatibility job can be skipped
by path gate and is not consumer migration evidence. Keep #102 open.
