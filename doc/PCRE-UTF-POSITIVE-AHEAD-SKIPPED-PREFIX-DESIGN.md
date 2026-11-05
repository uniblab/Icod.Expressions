# Positive lookahead before skipped bitmap prefixes

Continue issue #102 after merged PR #164, preserving its 29,633 full cases,
214 frozen cases and 35 frozen production blobs. Pinned PCRE2 10.48
`set_start_bits` skips a positive lookahead when its next significant opcode
sets a mandatory character. Its inspection skips intermediate assertions and
word boundaries. PR #164 admitted the direct literal case only.

Admit the same direct literal after a finite sequence of unquantified word
boundaries or lookaround assertions in the optional bracket body proof. The
first non-skippable node must be a valid CR-excluding `PcreByteLiteral`.
This is a local, iterative all-branch proof; do not recurse into the leading
node finder or infer a mandatory head from an assertion body. The existing
literal, type and predicate-free range cases without a positive ahead remain.
Every branch must have its own mandatory head. A branch ending in assertions,
an unsupported class after positive ahead, a repeat or an empty branch fails
this extension. Preserve wrapper costs, success-only checkpoint, cancellation,
the 1,000-visit bound, optional count bound and retained owner/skip barriers.

Runtime assertions still execute, capture and restore state. No parser, AST,
emitter, matcher, Step, frame, journal, API, limit, dependency, workflow,
consumer or CommandFramework change is planned. Grep remains last; publication
and all known migrations precede CommandFramework removal.

Native corpus: optional noncapturing, capturing and atomic wrappers; greedy,
lazy and bounded copies; six prefix combinations of positive ahead, negative
ahead, lookbehind and word boundary; ASCII/UCP, strict/tolerant, CRLF and
ordinary subjects, full capture vectors. Pin native JIT and bounded spans.
Publish tests first and confirm a successful-build RED confined to new tests
with the baseline green. Preserve the test blob and implement the guarded proof.
Require final exact-head six-platform net8/9/10 full/frozen results, no test
failure/skip/compiler warning/error, configured gates, native comparisons,
author whole-branch review and exact remote tree before ready PR. Full source-
pinned compatibility and consumer migration remain separate gates.
