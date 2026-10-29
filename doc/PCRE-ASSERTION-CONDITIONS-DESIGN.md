# Internal PCRE assertion conditions

## Intent and choice

Continue issue #7 after merged #89 with traditional atomic assertion conditions:
`(?(?=...)yes|no)`, negative ahead, positive/negative behind and their exact
short/long alpha aliases. Follow pinned PCRE.NET1.6.0 / PCRE2 10.48 Compiled/JIT.
Standing autonomous authorization covers this bounded internal extension.

Reuse existing assertion guards, integer scope arena and capture journal. A
conditional guard resumes the opposite branch when its body exhausts; successful
body completion commits captures and jumps to the body-match branch. An ordinary
Split around an assertion would incorrectly retry the other conditional branch
after a selected branch fails. Cloning a matcher or snapshotting captures would
add a second state path; neither is needed.

## Native contract

- Atomic positive/negative ahead and behind are allowed as the condition. All
  four symbolic and eight exact short/long aliases use the existing assertion
  parser and width resolver. Non-atomic conditions and a quantifier attached
  directly to the condition assertion are invalid. Nested non-atomic assertions
  inside an ordinary condition body remain allowed.
- The conditional and its condition assertion each consume one source nesting
  level, and neither allocates a capture slot. Bodies preserve existing global
  numbering, named references, branch-reset, option scoping and two-direct-branch
  rules. The condition header does not admit trivia before its assertion prefix.
- Condition evaluation consumes no subject. Matching positive bodies select yes;
  matching negative bodies select no. Exhausted bodies select the other branch.
  A missing else is runtime empty. Selected branch failure cannot fall back to
  the sibling; outer retries can restore state and reevaluate the condition.
- Crucially, captures from a matching condition body survive for BOTH polarities,
  including negative-condition else. Failed bodies restore captures, starts,
  repeat marks and atomic depth through the shared journal. Success is atomic:
  choices and nested resumable scopes inside the condition are pruned.
- Condition bodies retain assertion context: K is invalid there. K in yes/no
  branches remains allowed unless an enclosing assertion forbids it. Assertions
  use original subject/fragment/caller offset and existing variable-behind caps.
- Validate every assertion/body even under {0} or an unselected branch. Behind
  width uses explicit yes/no branches only; omitted runtime else is not appended
  to the AST. The condition contributes zero width but nested behinds validate.
- Callouts, recursion/pseudo-conditions, other pending syntax, public API and
  consumers stay separate gates. Malformed supported headers are InvalidSyntax;
  unsupported nested constructs retain the existing controlled diagnostics.

## Parsing and semantic representation

Add PcreByteAssertionConditional(Assertion,Body). GroupFrame gains an outer
assertion-condition marker and a stored condition node, plus a flag identifying
an inner condition assertion. On the outer `( ? (` header (without spaces), leave
index at the inner `(`. The heap parser opens the actual assertion normally,
checks atomic assertion kind, and stores its completed node instead of appending
it as a consuming body node. Do not invoke AppendNode/quantifier normalization
for that condition. Reject attached quantifiers at their source byte; run the
existing counted-bound reader for braces so recognized counts reject as invalid
while already-pending malformed brace literals stay pending. Disable the literal
assertion fast path for condition frames
so lowering always has its general Body and resolved behind metadata. Restore
options between the condition and yes/no bodies as for any assertion group.

Tree traversal/rewrites visit Assertion and Body. Named/capture-condition fixups
and deferred behind analysis therefore share the existing complete lexical tree.
Width evaluation schedules the condition assertion for nested validation but
returns GroupWidth(Body) over explicit branches. Nullable analysis includes the
implicit empty else; newline analysis visits the condition and selected bodies.

## Compilation and VM

Compile the general condition Body with existing AssertionEnter/AssertionExit and
flags: positive1, variable-behind2; add conditional8. The enter First pc is the
body-failure branch (no for positive, yes for negative); the exit First pc is the
body-match branch. Compile yes/no sequences plus one end jump, with no condition
Split. Heap continuations patch targets after emission. Existing reverse lowering
and per-branch end checks are unchanged.

One new ConditionalAssertion frame kind shares the existing five integer fields.
Entry creates the same guard-owned scope. Body failure uses RestoreFrameState and
resumes guard.Pc without propagating assertion failure. Body success prunes to
and pops the guard, restores caller position/scope, preserves captures for both
polarities and jumps to exit.First. PopFrame recognizes the new guard kind for
LIFO arena release. No scope, frame, journal payload fields or limits change.

## Verification and boundaries

Test-first fixture compares explicit native outcomes/group counts and every
numbered source-byte capture: all twelve spellings; empty, matching and failing
negative captures; branch commitment/outer reevaluation; nested conditions,
atomic/non-atomic transactions, repeats, options/references and UTF offsets;
fixed/variable/omitted-else widths; malformed UTF/CRLF/G offsets; invalid
non-atomic/repeated conditions; K contexts; byte diagnostics and depth/capture/
instruction/saved-path/work/cancellation gates. Publish tests in a draft PR and
observe missing-feature red CI before source edits; then implementation, fresh
whole-branch review and all six Staging jobs on net8/net9/net10 full+214 frozen.
No local SDK: managed executable red/green uses CI; local whitespace/frozen35.

Root solution/project, src, tests, samples; UTF-8 LF; C#13; no Python. Local Debug;
PR Staging; main/tag Release. Internal only: no public API, version, dependency,
release, Grep or CommandFramework ownership change. Existing depth256/capture256,
instructions4096, saved paths1024 and default work1,000,000/cancellation remain.
User owns merge.

## Pinned sources

pcre2pattern.3 Assertion conditions (captures retained for both polarities);
pcre2_compile.c conditional parse and atomic-assertion validation; pcre2_match.c
OP_COND/OP_SCOND condition assertion dispatch. Native probes verify10.48 and
nonzero JIT size for accepted patterns.
