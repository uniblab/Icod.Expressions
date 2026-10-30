# Internal PCRE returned capture lists

**Status:** issue #7, following merged #95; internal only.

**Later scope:** Actual recursive and whole-pattern calls are added by
`PCRE-RECURSIVE-CALLS-DESIGN.md`; their exclusions below are historical.

## Intent and scope

Support the PCRE2 10.47 returned-capture extension on the existing nonrecursive
call engine, against PCRE.NET1.6.0 / PCRE2 10.48 Compiled/JIT. A caller can retain
selected numbered capture values when its callee returns. Actual recursive and
whole-pattern calls, unresolved DEFINE-suppressed behind targets, public API,
package/release and consumer migration remain separate gates.

## Exact syntax and resolution

Parenthesized calls accept a nonempty list immediately after the target:
`(?1(2))`, `(?+1(<name>))`, `(?-1('name'))`, `(?&target(2,<name>))` and
`(?P>target(2))`. List entries are comma-separated absolute or signed relative
ASCII numbers, angle names or quoted names. No whitespace/comment skipping
occurs within the header, even under x/xx. Leading numeric zeros are accepted;
absolute zero and signed zero are invalid. The Oniguruma g angle/quoted forms
do not gain lists. Existing whole-pattern/cyclic call rejection remains.

Resolve every list globally, including forward names/numbers and unreachable
or erased calls. Relative entries use the source capture count at the call,
with positive count+n and negative count+1-n. Names use the existing exact,
case-sensitive128-byte ASCII/UTF grammar. Under J a name contributes every
associated slot, including branch-reset aliases. Validate each reference,
then sort and deduplicate the resolved selection to at most256 integers.
Malformed/undefined/out-of-range entries report InvalidSyntax at the original
call opening byte; native error numbers/offsets are not copied into our API.

## Return semantics

The callee's root capture is bypassed as before. Listing it does not create a
new capture. Retain final values for listed numbered slots; a nonparticipating
optional capture preserves the existing current value, including an unset
value or a caller's previous capture. Unlisted numbered values, every capture
start and private repeat marks restore through the shared journal. Successful
K/G0 effects remain. Nested calls apply their own list on each return: an outer
caller must also select a nested value to retain it across that outer return.

Retained values remain ordinary journaled state: failed callee alternatives,
later internal retries, caller fallback, assertion and atomic pruning restore
the correct values/checkpoints. Capture and recursion conditions observe the
ordinary resulting values/current call. No sibling fallback is introduced.
Newline/nullability metadata remain properties of the existing target body;
a returned list consumes no subject text. PCRE2 10.48 nevertheless rejects
lists on consumed calls during behind width analysis, including transitive
callee bodies, zero repeats and unselected VERSION branches. Reject at the
owning behind opener. Preserve the existing zero-width lookahead and DEFINE
width-scan bypasses: calls with lists there remain valid, as do callees that
contain already resolved lookbehinds. Do not reject by lexical assertion context.

## Approaches and decision

Capture snapshots would duplicate the existing transaction model and increase
per-call memory. A separate opcode/frame for each returned slot would spend
saved paths without representing a real alternative. The maintainer choice is
an immutable sorted selection carried by the existing call instruction, with
the ordinary return scan omitting compensation for selected capture values.
The existing journal retains all original mutations for retry.

Temporary parser references carry number/name and original call offset.
PcreByteSubroutineCall gains ReturnedCaptures (ReadOnlyMemory<int>), passed
through the existing instruction CaptureNumbers payload. Call.First/Second
remain entry address/target number. At Return, find that payload through the
existing ReturnPc-1 instruction index; no extra call-arena or saved-frame field.
Each inspected journal entry already spends Step and polls cancellation; a
selected Capture check uses binary search over at most256 sorted integers
(at most9 comparisons). CaptureStart and Mark always compensate. No additional
runtime allocation, capture snapshot, journal or ancestor scan is required.

Parsing and global resolution poll cancellation per source token/candidate.
Duplicate source entries may repeat without creating extra returned slots;
temporary reference storage is linear in source entries and the normalized
selection is bounded by the capture ceiling. Preserve heap tree
traversal and existing first-definition/cycle/behind validation. Generalize
capture-name termination only for a parenthesized call's optional list opener;
ordinary names retain their exact existing delimiters.

## Resources and delivery

C#13, net8/net9/net10, UTF-8 LF; root project/solution, src, tests, samples; no
Python. Depth256/captures256/instructions4096/marks256/savedframes1024/default
work1,000,000/cancellation remain shared. The four-integer call arena remains
at most16KiB integer payload. No managed stack recursion, saved spans/pointers,
cloned engines, runtime native/Regex fallback, dependency/workflow/version or
CommandFramework/consumer changes. Grep stays last and removal stays gated on
publication and migration of every known consumer. User owns merge.

## Evidence and acceptance

Primary pinned sources: pcre2pattern.3 returned capture groups (3499 onward),
pcre2_compile.c parse_capture_list (2732 onward) and call parsing (5266 onward),
pcre2_compile_cgroup.c compile_parse_recurse_args (507 onward), pcre2_match.c
recurse_update_offsets (513 onward) and called root bypass (6487 onward).
Native literal all-capture fixtures precede managed assertions. Cover source
grammar/global references, root bypass/preexisting/unset/empty values, duplicate
names/branch reset, nested propagation, retry/rollback/atomic/assertion/K,
behind consumed-list rejection and lookahead/DEFINE bypasses, byte/UTF offsets
and shared resource/cancellation gates.

Publish tests/docs as a draft; observe expected missing-feature red CI before
production implementation. Fresh independent whole-branch review and all six
native OS/architecture Staging jobs must pass full/frozen net8/9/10 suites and
configured workload/extraction/API/package gates before ready. No local SDK;
managed executable verification comes from CI. Preserve worktree for feedback.
