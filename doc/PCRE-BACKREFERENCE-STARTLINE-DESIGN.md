# Backreference barriers to first-character search

Close the concrete nonnullable ANY/newline entry gap recorded in PR233, and
the equivalent default-mode case with participating empty captures. PCRE2
compile_branch disables inferred first characters at an opaque backreference;
scan_prefix cannot cross that reference before consuming a character. With
every root branch requiring a leading caret, JIT therefore uses STARTLINE
before FIRSTMAP. Starting inside a UTF NEL/LS/PS must skip that newline's end.
For ANY/multiline ^()backref1B on LS+B, offset1 misses while offset3 matches
3:1 with capture1=3:0, with either unset-reference policy. A mandatory leading
B preserves native first-character search and may match3:1 from offset1.

## Approach

My choice under the continuous inline roadmap is to extend the existing bounded
compiler proof. Rename HasNullableUnsetBackreferencePath to
HasBackreferenceStartBarrier. Require a canonical reference among the same pure
instruction set, independently of Second. Follow zero-consumption edges until
Backreference or Accept; consuming instructions stop that path. Keep existing
Split/Jump/empty-Progress handling, 1,000 instruction/visit ceiling, heap storage,
cancellation and the existing AST first-caret proof for every branch. Default
and selected references share native optimization opacity. Mandatory leading
literals/classes keep their priority. No matcher, parser, AST/IR shape, capture
journal, work charging, source ownership, flags, API, dependency or quota changes.

Alternative runtime newline exceptions would duplicate compiler policy and can
admit wrong candidates. A complete reproduction of native optimizer selection
would expand this bounded fix into the wider #102 project. Both are rejected.

## Qualification and limits

Pin native JIT literals for split and boundary offsets, numeric/name/relative/
forward/self aliases, empty/unset/duplicate captures, repeats, root wrappers and
qualified/unqualified alternatives. Mandatory leading literals/classes and
unsupported proof operations must preserve existing search. Supplement every
selected UTF offset, malformed fragments, strict valid boundaries, casing/UCP,
owned source/options, deep groups, full capture capacity, near-boundary proof
size, canonical work/cancellation and unchanged compile quotas. Native arrays
compile before managed comparisons. Observe clean expected tests-only RED;
freeze before source changes; qualify all18 native results and supporting gates.

Keep the same conservative instruction envelope; calls/assertions/conditions/
verbs, over-budget programs, other optimizer shapes and #102 remain open.
Preserve all247 earlier test files,35 frozen blobs/214 outcomes/120 methods,
GNU/BRE/ERE, exported API/dependencies. C#13/net8/9/10; UTF8+LF; no Python,
subagents or SDK installs. User merges/tags/publishes; Grep migration stays last.

## Qualification correction after CI703

CI703 at110dcbe built cleanly but retained six failures/framework, all frozen
caseless ^[BC]()backref1 split-offset profiles. Native JIT scan_prefix collects
the class first; fast_forward_first_n_chars can select only a one/two-character
position unless a longer prefix supplies a search range. Caseless [BC] has four
ASCII members, so it cannot supply that pair; the following reference terminates
prefix scanning. STARTLINE then wins over FIRSTMAP. Case-sensitive [BC] retains
its two-character scan and must continue matching the split-offset control.

Keep the existing pure-IR eligibility and1,000-instruction bound. After the
zero-consumption proof fails, admit a linear prefix containing exactly one
positive, predicate-free ASCII ScalarSet with more than two distinct members,
followed by a reference through captures/anchors/atomic boundaries. Reject
branches, loops, additional consuming atoms, non-ASCII ranges, predicates and
Turkish casing in this supplementary proof. The root first-caret proof remains
mandatory. No matcher or test changes; frozen949-case fixture stays
f78f3d24ff62419bfefd9672a902259138fbbb7e. The six already-failing cases provide
RED for this compiler correction. Broader prefix shapes remain in #102.
