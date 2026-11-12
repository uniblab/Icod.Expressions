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
