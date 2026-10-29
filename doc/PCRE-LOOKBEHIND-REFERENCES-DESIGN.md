# Internal PCRE consumed lookbehind references

## Goal and scope

Allow existing numbered, absolute/relative numeric and unique named backreferences
to consume within fixed or bounded-variable lookbehind. Use the referenced
capture body's character minimum/maximum; outer capture repetition/optionality
does not change that width. Default unset references still fail at match time.
Forward definitions, captures declared inside assertions, empty bodies and
bounded transitive reference chains participate in the same analysis.

Pinned PCRE.NET 1.6.0 / PCRE2 10.48 Compiled/JIT remains the oracle. Reject
unbounded target bodies, lexical self-references, cyclic consumed dependencies
and consumed references when any `(?|` was parsed, even if it has no captures.
References inside nested lookahead contribute no outer width. Nested behind
validation retains the active capture dependency chain, including when found
inside lookahead; it must not hide recursion by contributing zero width. Duplicate-name J, subroutines and other pending
syntax remain outside this tranche.

## Architecture

Defer general lookbehind metadata until the whole pattern is parsed and names
and numeric targets are resolved. Retain each behind opener in its raw AST node.
Compatible literal assertions keep their existing optimized path. A final pass
must visit every general behind, including bodies erased by a surrounding zero
repeat. Unknown targets retain InvalidSyntax at the reference source offset;
unsupported width/dependency rules use the owning behind opener.

Use shared explicit heap traversal for AST reconstruction and name resolution.
Collect all capture body roots and lexical preorder intervals in a global heap
walk. Reference node positions inside their target capture interval are rejected
before target-width lookup: `(a(?<=\1))` must not slip through merely because the
behind contributes zero to its enclosing group's width.

Extend the existing reference-keyed heap min/max evaluator with consumed-reference
edges to capture body nodes and active-node cycle detection. Cache successful
widths within each evaluation; assertions contribute zero without adding ordinary
lookahead reference edges. Nested behind bodies add validation dependencies to
the same active chain, and failures retain their own assertion opener. Analyze children before applying zero counts, preserving
intermediate 65,535-character failures. The 255 variable limit applies only to
final behind ranges, so a finite wider reference erased by `{0}` may be legal.
Capture count remains at most 256; graph and rewrite state live on the heap,
visit finite source nodes, avoid exponential re-evaluation and check cancellation.
Native's separate 2,000 branch-analysis complexity accounting is not an internal
numeric parity claim; existing managed compile/resource policies remain distinct.

Finalize each behind with the existing per-branch maxima/shared minimum model.
Keep #80's compiler, Reverse/ReverseRange, scoped JIT ends, journal, retry frames
and matching work/cancellation behavior. Referenced widths count Unicode scalars
in UTF mode, while actual comparisons retain source-byte capture spans and local
casing. Case-insensitive references may consume a different byte width from the
captured spelling.

## Alternatives and decision

Closing-time analysis cannot see later definitions or complete name resolution.
Runtime width guessing would change reverse ordering and compiler diagnostics.
A second matcher would duplicate existing transactions and resource rules.
The maintainer agent chooses one global post-parse capture map plus heap dependency
resolution and rewriting under the user's standing authorization to continue.
Only compile-time parsing/analysis changes are needed.

## Constraints and acceptance

Internal managed C# 13, net8/net9/net10; no native/Regex production fallback.
Root project/solution, `/src`, `/tests`, `/samples`, UTF-8 LF, no Python. Local
Debug, six OS/architecture Staging CI jobs. Runtime frames retain integer indexes,
compact enums and the existing byte retry counter; no spans/interior pointers,
per-backtrack capture copies or duplicate width engines. Preserve existing
parser/capture/repeat, compiled instruction/node, saved-path, work and cancellation
ceilings. No public API, package version/dependency, Grep migration or destructive
CommandFramework changes.

Pin native outcomes and every numbered capture before managed comparison for
all reference spellings, forward/dependency chains, empty/unset captures,
assertion-declared captures, outer repeats, nested assertions/scoped ends,
UTF casing widths, malformed/caller/CRLF offsets, erased bodies and 255/65,535
boundaries. Reject lexical self and mutual cycles, global branch reset and
unbounded bodies in controlled diagnostics, including erased references. Exercise
deep named reconstruction and bounded dependency graphs without native stack
recursion. Require fresh review and all six full/frozen suites plus existing
workload/extraction/API/package gates before a ready PR.
