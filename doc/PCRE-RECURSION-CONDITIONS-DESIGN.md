# Internal PCRE recursion conditions

**Status:** issue #7, following merged #94; internal only.

**Historical scope:** Returned-capture lists are added by
`PCRE-RETURNED-CAPTURES-DESIGN.md`; the exclusion below records #95.

**Later scope:** Actual recursive and whole-pattern calls are added by
`PCRE-RECURSIVE-CALLS-DESIGN.md`; their exclusions below are historical.

## Intent and scope

Add `(?(R)yes|no)`, `(?(Rdigits)yes|no)` and `(?(R&name)yes|no)` to the existing
nonrecursive call engine. PCRE's term recursion condition includes any
subroutine-like call. Actual recursive/whole-pattern calls and returned-capture
lists remain separate gates. This feature does not publish a PCRE API or migrate
consumers.

The native oracle is PCRE.NET1.6.0 / PCRE2 10.48 Compiled/JIT. A recursion test
examines only the **innermost active call**, never ordinary lexical capture
nesting, the ancestor call chain or inactive successful call guards. Top-level
conditions are false. Return restores the parent active context; backtracking
into a completed callee restores its saved active call ID.

## Headers and global target resolution

Bare R or R followed solely by ASCII digits first searches all capture names.
If that exact name exists, retain the existing capture-participation condition,
including forward declarations and duplicate-name ANY participation. Wrapped
`<R>` or quoted R names always stay capture tests. Other/lowercase/Unicode-digit
names follow ordinary name rules; no new prefix reservation.

Without a matching capture name, bare R and all-zero numeric suffixes (R0,
R000) mean any active call. A positive R number must identify an existing group
within the library's256-capture ceiling; leading zeros are accepted. Resolve
names before numeric conversion so an existing R257/R99999 name can win.
Forced `R&name` always tests call context: the name must exist and may denote
multiple slots; any slot matching the current call satisfies the condition.
Use existing case-sensitive128-byte name grammar and exact header delimiters;
no embedded trivia, relative signs or Unicode numeric suffix interpretation.
Undefined/malformed targets report InvalidSyntax at the original conditional
opening byte. Existing two-direct-branch/depth/repetition/option rules apply.

## Approaches and decision

A scan of saved frames would confuse returned calls with active calls and add
work proportional to backtracking history. Captures do not represent call
context because the root capture is bypassed and numbered captures restore on
return. Retain the target group number in each existing call-scope arena record
and test the current stable ID directly. This maintainer choice builds on #93's
integer context and leaves full recursion for a later contract.

## Semantic nodes and lowering

Resolve a temporary header into PcreByteRecursionConditional(GroupNumbers,Body),
separate from capture conditions. An empty immutable target list means any call;
positive-number and named targets use existing bounded integer arrays. Traverse
and rewrite every body globally, including erased/unselected syntax, so names,
subroutine dependencies and nested assertions still validate.

Share the existing conditional yes/no branch layout, parameterized by test
opcode and integer targets. RecursionTest jumps to one chosen branch with no
conditional Split and no sibling fallback after branch failure. Both bodies
compile within existing budgets. Calls retain their group number in the
instruction's Second operand while First remains the patched entry address.

CallScope gains one integer Number beside ReturnPc/Parent/ChangeCount. At most
1024 live call guards means at most16KiB integer arena payload, versus12KiB
before this feature. Saved Frame layout and stable assertion/call IDs stay
unchanged; no spans, pointers, capture copies or additional journals. The VM
checks current call presence and each named candidate under shared work and
cancellation accounting. No ancestor scan or capture mutation.

## Widths, metadata and resource policies

Treat recursion conditions like other dynamic conditions: all explicit branches
participate in consumed min/max width analysis, including the unused branch at
top level. Omitted else is runtime-empty but excluded from native width bounds.
Keep DEFINE suppression/global nested assertion validation and target dependency
cycles intact. Runtime-nullability analysis conservatively uses all alternatives
or implicit empty else; explicit CR/LF metadata visits every body.

Depth256/captures256/instructions4096/marks256/savedframes1024/default work
1,000,000/cancellation remain shared. No opcode duplicates, native/Regex fallback,
recursive managed parser/VM stack, public API/package/version/dependency,
workflow or consumer change.

## Native evidence and acceptance

Pinned pcre2pattern.3 Checking for pattern recursion specifies innermost-only
nonrecursive calls and name ambiguity. pcre2_compile.c around6650-6720 resolves
names before numeric fallback and maps R/R0 to ANY; pcre2_match.c around6044-6070
compares current_recurse and duplicate name targets.

Fixtures must pin root/callee/nested-callee contexts, parent restoration after
return and internal retry, inactive guards/atomic and assertion pruning, numeric
and duplicate named targets, global name shadowing/wrapped controls, UTF byte
positions, both explicit branches/omitted else and lookbehind range policies,
K/capture rollback, exact malformed headers, CRLF/malformed UTF/start offsets
and shared depth/compile/guard/work/cancellation gates. Independent review and
all six full/frozen CI jobs plus configured workload/extraction/API/package
checks precede ready status.

## Delivery constraints

C#13, net8/net9/net10; root solution/project, src, tests, samples; UTF-8 LF;
no Python. Local Debug; six native OS/architecture Staging PR jobs. No local SDK:
managed executable red/green comes from CI. Grep stays last; CommandFramework
removal stays gated on publication and migration of every known consumer. User
owns merge and the worktree is preserved for feedback.
