# No-argument THEN alternation cuts

PR219 follows merged PR218. Exact `(*THEN)` saves a delayed cut. On failure it
abandons choices within its lexical alternative and retries that group's next
branch; on its last branch it fails the group and permits earlier outer retries.
Groups without | and conditional yes/no arms do not own alternations. With no
owning alternation, THEN behaves as PRUNE. Assertions catch THEN and subroutines cannot target caller groups, subject to
the pinned JIT local-call exception described below.
Rightmost cuts, atomic success, captures/K and native selected callee returns
retain the existing transactional behavior. Named THEN/MARK remain pending.

## Architecture and approaches

Use one journaled stack-depth checkpoint per compiled real alternation in a
program that contains THEN. Parser records exact THEN presence; programs without
it keep their existing bytecode and resources. The compiler carries the nearest
alternation's mark and whether its branch has a next sibling. Single-branch
wrappers and conditional arms inherit it. Assertion bodies and each separately
compiled callee reset ownership; assertion continuations restore the exterior
owner before compiling conditional yes/no arms.

THEN saves an absolute live-frame boundary: owner entry plus one retained next-
branch frame, or owner entry alone for the last branch. With no owner it saves
the newest active call/assertion catch boundary (zero at root). Scope boundaries
clamp the target so it cannot escape an assertion/call. Reuse the shared cut
unwind and selected pre-verb callee journal filtering. Ordinary branch retry
restores captures/starts/marks. The new checkpoint stores backtrack.Count in an
existing mark slot; it creates no extra saved frame or runtime arena. Repeats,
recursive calls and returned callee checkpoints use existing mark transactions.
The optional first-code-unit proof for COMMIT crosses leading THEN where native
entry precedes both verbs. Other optimizer shapes remain issue102.

Alternatives considered: using PRUNE for THEN loses sibling retry; retaining an
arbitrary recent Split confuses repeat choices and nested/completed groups; a
new runtime group-guard arena adds persistent state and frames. Lexical compiler
ownership plus journaled depth marks directly identifies the native boundary.
This is the implementation choice under standing authorization to continue inline.

## Bounds and qualification

THEN frames share the1024 saved-frame budget with PRUNE/SKIP/COMMIT and ordinary
choices. Alternation checkpoints share the256 mark budget with script-run and
nullable progress state; their opcode and compilation share the4096 expansion
budget. Every marker, checkpoint, scope walk and discarded frame charges shared
work/cancellation. Exact direct quantifiers reject; grouped repeats remain
ordinary. Width is zero without terminating behind tail analysis.

Pinned PCRE2 10.48 documentation, compiler/JIT traps and independent native
literal captures qualify root/transparent/nested/last alternatives, earlier
outer retries, conditionals, positive/negative/conditional/non-atomic assertions,
behind, calls/recursion/selected returns, atomic/repeated/script/grapheme scopes,
mixed verbs, K/ACCEPT/NOTEMPTY, options/quotes, and all selected malformed UTF/
CRLF offsets. Resource cases pin frame, expansion and shared mark boundaries.
New tests freeze after clean successful-build RED. No changes to prior fixed
fixtures, GNU/BRE/ERE, frozen35/frozen214/120, public API, dependencies or runtime
fallback. C#13/net8/9/10; no Python, installs or subagents. Named verbs/MARK,
remaining directives/callouts, UTF C, optimizer/resource conformance, UTF16/API,
consumer performance and release remain gated. Grep stays last; user merges.

## Selected callee captures and native JIT local quit

Lexically owned THEN cuts, including a last branch whose numeric target equals
the call boundary, perform ordinary group rollback and must not preserve
selected pre-verb callee values. Their saved-frame kind records lexical ownership;
numeric boundary equality alone cannot identify a forced call quit. Unowned
THEN does use the call quit and retains selected values as PRUNE does.

Inside a called group, native JIT also sends unowned THEN in an atomic positive
assertion to its local call quit, abandoning callee alternatives and retaining
selected pre-verb values. Standalone positives outside calls and negative,
conditional and non-atomic assertions keep their normal catches. NO_JIT probes
confirm that the interpreter differs here; pinned JIT behavior is the oracle.
CI657 exposed the original positive-assertion fixture mismatch. Author review
added eight separate native regression rows without editing the original167
fixtures; RED658 pins five owned-cut capture failures plus the existing fixture
before the production-only correction.
