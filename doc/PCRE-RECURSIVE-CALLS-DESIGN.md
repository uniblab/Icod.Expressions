# Internal PCRE recursive and whole-pattern calls

**Status:** issue #7, following merged #96; internal only.

## Intent and scope

Enable recursive numbered/named/relative/g subroutine calls and whole-pattern
R/absolute-zero calls against PCRE.NET1.6.0 / PCRE2 10.48 Compiled/JIT. Reuse
the existing heap VM, integer contexts, shared capture journal and returned
selections. This closes a call-syntax/semantics gap, not public PCRE parity,
release or consumer migration. Unresolved DEFINE-suppressed callable behind
metadata, unsupported verbs/callouts, Unicode parity and release gates remain.

## Contract

Existing exact numbered/named/relative/g call grammar and first syntactic target
resolution remain. Calls may now form direct or mutual cycles, including dead
DEFINE declarations. Do not reject call graphs globally. Preserve global
undefined-reference validation, source capture counts, duplicate/branch-reset
first definitions, definition options and every existing source/compile limit.

Add exact parenthesized `(?R)` and absolute zero `(?0)` with leading-zero
variants; g angle/quoted zero forms also mean whole pattern. Parenthesized R/0
may carry the same nonempty returned lists as other calls. R must be immediately
followed by its closing parenthesis or list opener; R0/Rdigits/trivia suffixes
are malformed call headers. Named g forms can still target a capture named R;
they must not be reinterpreted as whole-pattern calls. Signed zero is invalid.

Whole-pattern target0 calls the complete semantic root with its original options
and anchors. It bypasses G0 capture operations just as numbered callees bypass
their root capture; normal final match reporting and successful K remain. The
current call number0 works with existing R/R0/all-zero ANY conditions; positive
Rdigits/R&name still inspect only the innermost numbered target. Ordinary
lexical capture nesting does not create a call context.

Recursive calls are non-atomic and retry internal alternatives after a caller
failure. Captures inherit current values. On return, unselected numbered values,
all capture starts and private marks restore through existing journal
compensation; selected final values survive. Each outer return independently
filters its own selection. Called root bypass, caller starts, K, assertion/end
scopes, atomic pruning and retry context restoration remain identical to
ordinary calls. No capture snapshots or separate recursion journal.

## Resource and zero-progress behavior

The pinned JIT does not use the interpreter's same-position/last-inspected
character recursion-loop heuristic. Valid recursion can change captures without
advancing the subject, and a first unproductive recursive branch can exhaust
resources before a later successful alternative is reached. Do not add a
same-position rejection or treat resource exhaustion as an ordinary miss.

Every Call consumes the existing shared saved-frame guard; all VM instructions,
return journal entries and retries retain existing work/cancellation accounting.
Unproductive recursion ends with MatchDepthLimitExceeded or the earlier
MatchWorkLimitExceeded, no partial match. These are explicit library budgets,
not numeric parity with PCRE2 JIT stack/match thresholds. No CLR recursive calls
or increased ceilings: savedframes1024/default work1,000,000/cancellation remain.
CallScope stays four integers, at most16KiB integer payload, with stable IDs.

## Approaches and decision

AST inlining cannot represent unbounded recursion and would duplicate semantic
handling. A separate recursive matcher would split journals/conditions/assertion
semantics and require extra state. The maintainer choice is to allow cyclic
integer call edges in the existing program, compiling each referenced body once
and patching entries after every body is emitted.

Remove only the parser's global cycle rejection. Whole-pattern number0 resolves
to a synthetic PcreByteGroup(0,root) in final target collection, reusing the
resolved semantic root without a cyclic object graph. The existing compiler
already emits target.Body followed by Return and preserves call number/selection
payload through patching. Main retains its separate Accept continuation. Runtime
Call/Return, frame layout, arenas, journals and opcodes require no special
recursive variant or additional state.

## Behind widths and validation

Consumed recursive dependencies remain unsupported at the owning behind opener,
including zero repeats and unused conditional branches. Preserve the existing
heap active dependency chain for numbered cycles; consumed whole-pattern calls
reject explicitly before target lookup. Lookahead and DEFINE width bypasses
remain, including recursive calls inside a zero-width ahead. Nested behinds
still validate globally; bounded nonrecursive consumed calls keep #94 behavior
and consumed returned-list calls retain #96 rejection. Final target validation
continues to reject callable unresolved suppressed-behind metadata.

## Delivery and evidence

C#13, net8/net9/net10, UTF-8 LF; root solution/project, src, tests, samples; no
Python. Depth256/captures256/instructions4096/marks256 remain shared. Whole-root
call bodies count against the same emitted instruction/visit budget, including
unexecuted definitions. No runtime native/Regex fallback, cloned width engine,
saved spans/pointers, public API/package/version/dependency/workflow or consumer
change. Grep stays last; CommandFramework removal remains gated on publication
and all known consumers migrated. User owns merge; preserve worktree.

Pinned sources: pcre2pattern.3 recursion and subroutine chapters (3280 onward),
pcre2api.3 DISABLE_RECURSELOOP_CHECK (3033 onward), pcre2_compile.c R/numeric call
headers (5211 onward) and width dependencies (9782 onward), pcre2_match.c
recursive return/root capture handling (6360 onward), and JIT recursion lowering.
Use native literal all-capture outcomes for recursive/mutual/relative/duplicate
calls, whole-root forms, capture-changing zero progress, non-atomic retry,
returned lists, K, assertions/conditions and byte/UTF/G/CRLF/invalid offsets.
Pin library work/frame/cancellation limits separately from native resource errors.

Tests/docs draft precedes observed expected-red CI and production changes.
Fresh independent whole-branch review and all six native Staging full/frozen
net8/9/10 suites plus configured workload/extraction/API/package gates precede
ready status. No local SDK; executable managed verification comes from CI.
