# Internal UTF noncapture group repeat recovery

## Intent and native evidence

Continue #7/#102 after merged #126. Follow a mandatory leading class PLUS through
up to four unquantified single-branch noncapturing groups, selecting the existing
#125/#126 marker and recovery for ASCII or wide positive predicate-free sets.

Pinned PCRE.NET1.6.0/PCRE2 10.48 JIT, UTF ANYCRLF:
(?:[\x09-\x0c]+)A on hex0B0D0A41 from0 misses; five nested noncapture groups
around the same repeat match LF at2:2. The wide \x{100} union has the same four/five
boundary. Initial CRLF+A still enters at LF through four groups. Current selector
examines only the root first node and misses this native group descent.

pcre2_jit_compile.c detect_early_fail OP_BRA/OP_CBRA around1668ff recurses only
while depth<4 and the group terminates in OP_KET rather than a quantified group
ending. It carries previous count0 into a first single-branch group; its leading
class PLUS receives type_skip. Alternatives change the initial count; OP_ONCE
and capture optimization/ref constraints differ and stay outside this extension.

## Approaches and implementer choice

Under standing autonomy, extend the existing selector with bounded iterative group
descent. A new recovery engine/descriptor would duplicate working machinery.
Unbounded flattening would incorrectly mark five-deep/native-nonaccelerated forms.
Choose a local maximum-four noncapture walk; no matcher or saved-state change.
Cost: wide unbounded class repeats beyond this bound and capture/alternate/other
optimizer shapes still retain conservative baseline behavior until separately
pinned. Do not claim general group/iterator parity.

## Implementation and invariants

In src/Pcre/PcreByteProgram.cs FindUtfCrLfSkipRepeatSet, follow the first node
of one nonempty branch. If it is a PcreByteNoncapturingGroup, descend its Body
only while group depth<4; otherwise apply the existing possessive-wrapper/direct
min1/maxint positive predicate-free ScalarSet/CR-excluding/LF-admitting rules.
Cancellation at every descent/range. A loop and integer depth avoid native-stack
recursion and keep all runtime interfaces unchanged.

PcreByteAtomic wraps a repeat directly for possessive syntax, whereas explicit
atomic groups wrap a noncapture group. Do not descend through the latter. Do not
traverse captures, alternative/multiple/empty branches or quantified groups.
The existing maximum1000 first-class proof still decides UtfCrLfStartClass;
selecting the leaf by AST identity admits the wide group forms and marks the same
scalar instruction copies. The existing per-attempt integer endpoint and charged
comparisons recover failed runs; no matcher/Step/capture/frame/journal changes.

All6022 existing expectations and frozen35 unchanged. No new descriptors/state,
waived comparisons, raised resource ceilings, long-lived spans/pointers/snapshots.

## Verification and remaining gates

Native-first full spans cover depths1..4, ASCII/wide, greedy/lazy/possessive/count,
head-only versus entire-expression groups, caller offsets, suffix captures and
later VT/LF winners. ASCII depth5/6 controls pin the acceleration boundary. Wide
depth5 from caller0 remains an existing broader102 gap; explicit LF caller control
can still match. Preserve explicit-atomic/alternative/finite/first-nonmember and
explicit-source-newline distinctions. Six newlines/UCP/all offsets/strict-tolerant
UTF fragments, exact/exhausted observed work/cancellation and no-LF controls.
No unsafe positive-Xuc malformed native probes (#100).

Tests-only draft successful-build RED, baseline6022 pass, fixed test blobs afterRED;
source then six native Staging OS/arch/net8/9/10 full/frozen214/configured gates,
seven equal workloads. Whole-branch author self-review and exact head/tree/log/
frozen/whitespace/clean audit; ready PR; user owns merge. No agents/Python/SDK install.

C#13/net8/net9/net10, UTF8 LF, root solution/project, src/tests/samples. No public/
package/version/dependency/workflow/release/consumer/fallback/engine-clone changes.
Depth256/captures256/instructions4096/marks256/frames1024/defaultwork1,000,000/
proof1000 unchanged; four is a native optimizer boundary, not a raised parser cap.
Broader102, byte99 and unsafe100 separate. Grep last; no CommandFramework removal
before publication and all known consumer migrations.
