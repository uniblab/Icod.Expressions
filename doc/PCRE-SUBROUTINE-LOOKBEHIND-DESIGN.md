# Internal PCRE subroutine widths in lookbehind

**Status:** issue #7, following merged #93; internal only.

## Goal and scope

Allow existing nonrecursive numbered/named/forward/relative subroutine calls to
consume within fixed or bounded-variable lookbehind. Use the first syntactic
capture definition's body, independently of capture participation, exterior
quantifiers, duplicate names or branch-reset declarations. Preserve definition
options, byte captures and #93's non-atomic call/return transactions.

Pinned PCRE.NET1.6.0 / PCRE2 10.48 Compiled/JIT is the oracle. Whole-pattern and
cyclic calls, recursion conditions and returned-capture lists remain pending.
The #93 rejection of callable targets containing unresolved DEFINE-suppressed
general behind metadata also remains; this change must not execute unlowered
assertions. No public API/package/dependency/consumer/workflow changes.

## Approaches and decision

Runtime guessing would change reverse ordering and diagnostic timing. A separate
call-width engine would duplicate reference and nested-assertion rules. Extend
the existing heap dependency evaluator with edges to first-definition group
nodes. This is the maintainer's implementation choice under the standing remit
to continue; the shared call VM needs no change.

## Width analysis

Keep the existing lexical capture/backreference context unchanged. Add a separate
first-definition call-target dictionary during the same global preorder walk.
Backreferences still obey their unique-name/global-branch-reset/lexical-self
restrictions; subroutine calls may use duplicate names and branch-reset slots.
Do not substitute the last capture definition or current participating capture.

For a consumed call, schedule its target PcreByteGroup on the existing heap work
stack. Its full alternatives yield minimum/maximum character lengths; assign
that width to the call node. The existing reference-identity cache and active-node
set handle acyclic chains, mixed call/backreference cycles and nested behind
dependencies. Poll cancellation on every traversal. Ordinary lookahead remains
zero consumed width; nested behind validation retains the active dependency chain.
Global call-cycle rejection remains before this pass with its existing call-byte
diagnostics. Undefined targets remain InvalidSyntax at the actual call opener.

A target's alternatives merge into one range. Differing fixed target alternatives
therefore make the caller branch variable. If any top-level behind branch is
variable, every branch maximum must be at most255. All-fixed top-level branches
retain65535. Measure UTF scalars rather than bytes. Validate finite target widths
before applying a caller's zero repeat, including intermediate65536 overflow;
finite256 target widths erased by {0} can remain legal. Unlimited target bodies
remain UnsupportedSyntax at the owning behind opener, even when erased.
Nested invalid behinds report their own original opener. Native's independent
branch-analysis complexity counter is not a numeric parity promise here.

## Runtime and resources

Existing Reverse/ReverseRange/ReverseEnd lowering preserves longest-first retry,
JIT scoped end caps, malformed-UTF rewind barriers and fixed/variable distinction.
The shared Call/Return implementation restores numbered captures/starts/marks,
keeps internal retries and preserves K/G0. Assertions retain their existing
positive/negative/conditional/atomic/non-atomic transactions. No new opcode,
matcher/frame/arena/journal fields, snapshots, spans or interior pointers.

Depth256, captures256, instructions/compile visits4096, repeat marks256,
savedframes1024, default work1,000,000 and cancellation budgets stay shared.
Out-of-line target bodies still compile within these limits even for erased
calls; width acceptance does not waive the compile budget. Compact acyclic
call targets with widths255*257 allow a65535 boundary fixture within4096
instructions without executing those calls;256*256 must reject before erasure.

## Native contract and acceptance

Pinned pcre2pattern.3 lookbehind subroutine paragraphs permit bounded calls and
exclude recursion. pcre2_compile.c RECURSE_OR_BACKREF_LENGTH (around9850-9907)
selects the first capture, checks lexical/dependency recursion and evaluates
get_grouplength; named calls bypass duplicate-name/number bans on backreferences.
Native probes pin target alternative ranges, distinct fixed branches, first
name/number definitions, suppressed behind dependencies and compact65535/65536.

Tests must compare explicit native outcomes and every capture for existing call
spellings, erased/forward definitions, nested call/reference dependencies,
options, first definitions, root/nested restoration, all assertion polarities,
non-atomic retries and scoped ends, Unicode scalar widths and original byte
positions, malformed UTF/caller starts/G/CRLF, zero-count validation and width/
compile/runtime limits. Independent final review and all six full/frozen CI
jobs plus existing workload/extraction/API/package gates precede ready status.

## Delivery constraints

C#13; net8/net9/net10; root solution/project, src, tests, samples; UTF-8 LF;
no Python or runtime native/Regex fallback. Local Debug; six native Staging PR
jobs. No local SDK, so executable managed red/green comes from CI. Grep remains
last, and CommandFramework removal stays gated on publication and all known
consumer migrations. User owns merge; preserve the worktree for feedback.
