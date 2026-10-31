# Internal PCRE fixed byte prefix continuation

## Intent and evidence

Continue #7 and bounded #99 after #115. PCRE.NET1.6.0 / PCRE2 10.48 JIT
scans at most12 prefix bytes (`pcre2_jit_compile.c` MAX_N_CHARS at376,
scan_prefix at5877..6340). Byte ALLANY and TYPEEXACT ALLANY advance unknown
positions; ONCE/BRA/CBRA pass through single-branch groups. UTF stops this path.
On CRLF+BA, byte ANYCRLF `\C{2}A` matches1:3. On CRLF+ten Bs+A,
`\C{11}A` matches1:12; with eleven Bs `\C{12}A` misses. Atomic `[^\r]A`
and `\CA` match1:2 on CRLF+A. Explicit offset1 still works beyond the window.

A global LF retry would be wrong: `[^\r]\CA` on CRLF+AA misses in JIT.
Its later literal can pass the native prefix test at CR, then the VM fails its
first class and skips LF. The existing PrefixAny source marker also combines
NOT/ALLANY/PROP/XCLASS, whose exact-repeat encodings differ. Do not generalize
it into an arbitrary repeated class or a multi-position restrictive prefix.

## Selected bounded contract

Extend only the existing compiler proof, retaining the matcher and immutable
ByteCrLfPrefix flag. Traverse one nonempty branch, with ordinary capture,
noncapture and atomic wrappers, on an explicit heap worklist with cancellation.
A qualifying complete expression is:

- One existing PrefixAny class or Any atom followed by exactly one literal; or
- One through eleven unrestricted Any(DotAll=true) byte positions followed by
  exactly one literal. Positions may be separate atoms or exact positive direct
  Any repeats; greediness/possessive wrappers cannot change their fixed width.

Groups around the sequence or individual atoms preserve captures. Exact repeated
capture groups, repeated classes/properties, variable/zero repeats, alternatives,
assertions, anchors, calls, resets, trailing operations and multiple literals do
not enter this proof. Ordinary byte DotAll forms still represented as bitmap
classes remain in #99; this change does not infer native encoding from set equality.
The byte-only caller remains authoritative. One literal must lie within native's
12-position window. Existing restrictive one-position cases cannot be mixed into
longer unrestricted prefixes. At most1000 proof-node visits use the existing
conservative analysis ceiling; wrapper traversal uses no CLR recursion.

For unrestricted prefixes all earlier bytes always match, so a failed CR attempt
means the sole following literal failed. Native prefix scanning therefore rejects
CR before VM entry, permitting LF. For existing one-position restrictive prefixes,
that literal sees LF at CR and cannot match it when explicit-newline=false. The
runtime's existing metadata guard handles explicit CR/LF literals independently.
No suffix outside the proof can fail after a successful literal-prefix test.

## Alternatives and constraints

Do not add a runtime prefix optimizer or general native IR here: those need
separate designs for start bitmaps, source provenance and optimizer selection.
Do not globally retry LF or clear positive interior-range metadata. #99 stays open
for these cases; #102 UTF scanning and #100 unsafe native UTF remain separate.
This is a maintainer scope choice under standing authorization, with the cost that
valid broader native optimizer shapes remain pending. No claim of full JIT parity.

Internal C#13/net8/net9/net10; root project/solution, src/tests/samples, UTF8 LF.
No new AST/opcode/VM state, saved spans/pointers, snapshots, raised limits, native
production fallback, public/package/version/dependency/workflow/release/consumer
changes. Existing work, cancellation, depth/capture/instruction/mark/frame limits
remain. Frozen35 unchanged. No subagents, Python or SDK installation. Grep last;
CommandFramework removal requires publication and migration of all known consumers.

## Verification

Native literal all-capture fixtures, exact counts1/2/11/12/13, split prefix counts,
atomic wrappers and restrictive/variable/trailing controls; six newline conventions,
UCP choices and all caller offsets over empty/NUL/high/CRLF subjects. UTF controls
use supported scalar atoms, never UTF C or unsafe positive Xuc. Work observer,
exact budget boundary, cancellation and compile ceilings remain shared. Tests-only
successful-build RED precedes source. Six native Staging platforms/net8/9/10 must
pass full/frozen214 and configured extraction/workload/API/package gates. Separate
whole-branch author self-review, tree/head/log audit; user owns merge.

Design/builder/plan check: fixed unrestricted positions plus sole literal make the
existing retry flag sufficient; no runtime interface or capture ownership changes.
