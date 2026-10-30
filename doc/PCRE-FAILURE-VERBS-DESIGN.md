# Unconditional PCRE failure verbs

## Intent and native contract

Continue issue #7 after #107 with exact no-argument `(*FAIL)` and `(*F)` at any
ordinary atom position. Pinned PCRE.NET1.6.0 / PCRE2 10.48 verbs/META_FAIL/OP_FAIL
and get_branchlength/check_lookbehinds in pcre2_compile.c are the authority.

Both verbs always fail the current path and use ordinary saved-path backtracking:
retry alternatives/calls/assertions and roll back captures/starts/marks through
the existing journal. They consume no subject and never create a successful
empty match. They are not directly repeatable (including zero counts); a group
containing them remains ordinarily repeatable. Token spelling is case-sensitive;
quotes/brackets are ordinary literal source. Named FAIL/F forms imply pre-MARK
in native PCRE2 and remain explicitly pending with other named/control verbs.

All source still parses and participates in global name/reference/call resolution
and lexical newline metadata. A failure ends consumed-width analysis of its
immediate branch: width is the preceding prefix only, later ordinary widths/calls
are not visited. Native skips all nested lookbehinds in that width tail too,
including tails behind a lookahead, like existing width-mode DEFINE suppression.
Global scans outside width mode still validate later lookbehinds. Unbounded or
cyclic consumed widths before failure still reject. Suppression must retain
branch boundaries and reset at alternatives; no global dead-source elimination.

## Approach and alternatives

Chosen by Codex under the autonomous remit: a leaf PcreByteFailure and one Fail
opcode set the existing matcher `failed` flag. No new saved frames, capture state,
VM mode or independent backtracking path. A leaf is necessary to distinguish
nonrepeatable failure syntax and its width-tail rule from ordinary assertions.

Lowering to a consuming empty class would give the wrong width; lowering to a
negative empty assertion would lose native repeatability/branch-width identity.
A shared Fail opcode makes unconditional failure explicit even at end/fragment
boundaries. Ordinary AST visitors retain the leaf and all later source; only
lookbehind width scheduling/summing and width-context suppression stop at it.
Existing compiler newline/prefix/empty-path analyses keep conservative behavior;
no new optimizer parity promise is made.

## Verification and constraints

Native-first literal all-capture spans cover aliases, ordinary branches, capture
rollback, atomic/negative/non-atomic assertions, conditionals, calls/recursion,
repetition groups and zero-width paths. Pin width tails, alternatives and global
versus width-suppressed nested lookbehinds, with invalid prefix widths and global
unresolved-reference controls. Compare byte/UTF/invalid-UTF all caller offsets,
CRLF metadata and UTF start directives. Work/budget/cancellation use the existing
transition/journal paths; compare simple failures with the existing `(?!)` path
and pin controlled instruction expansion/depth outcomes.

Tests-only expected RED precedes source changes; all six native Staging full
net8/net9/net10 suites plus frozen214/configured gates/seven Linux equalities
precede ready status. Verify frozen35/whitespace/clean/exact tree/head. No local
SDK; managed execution via CI. No subagents: separate author design/self-review.

C#13, UTF8+LF; src source, tests tests, doc docs. Local Debug. No Python, public
API, package/version/dependency/workflow/release/consumer changes or fallback.
No retained spans/pointers, snapshots, cloned engine or raised limits. Other
verbs/arguments and #99/#102 optimizer/#100 unsafe native Xuc/Unicode-version
issues remain separate. Grep last; CommandFramework removal only after publication
and all-known-consumer migration. User owns merge; preserve worktree/evidence.
