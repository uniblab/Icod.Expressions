# Internal PCRE repeated byte group prefixes

## Intent and native cause

Continue #7/#99 after #118. Native fixed positive group repeats copy the group
body, so prefix scanning can cross each copy's ordinary capture/noncapture/atomic
wrapper and transparent atoms. A directly repeated property class instead emits
XCLASS+CRRANGE and can stop the prefix scan. Preserve this source-shape distinction.

Pinned PCRE.NET1.6.0/PCRE2 10.48 JIT, byte ANYCRLF on CRLF+BA:
`(\C){2}A` matches1:3 with final capture2:1; `([^\r]){2}A` also matches1:3,
but misses on CRLF+AA because the final literal passes at CR before VM failure.
`(?:[\p{L}\P{L}]){2}A` matches LF, while direct `[\p{L}\P{L}]{2}A` misses
on CRLF+BA. Capture/atomic variants agree. Nested repeats and multi-atom bodies
must accumulate real byte offsets, preserving the existing conditional selector.

Authority: pinned pcre2_compile.c positive fixed group replication around7630..7705;
pcre2_jit_compile.c OP_ONCE/BRA/CBRA traversal6034..6058, twelve-position prefix
window, and #117 conditional literal selection. Native probes preserve original
subject bytes/all captures and bound every successful span.

## Approach and contract

Alternatives: build a separate grouped-prefix scanner (duplicates policy), expand
AST/VM groups for matching (unnecessary state/capture changes), or schedule bounded
copies only inside the existing compile-time proof. Choose the third under standing
implementation authority. Runtime instructions/captures are already implemented.

Extend only FindByteCrLfPrefix's iterative worklist. A PcreByteRepeat whose direct
child is PcreByteGroup, PcreByteNoncapturingGroup or PcreByteAtomic may schedule
its child min times when min=max, count1..11. Check cumulative pending proof work
against the existing1000-visit ceiling before enqueue. Push existing immutable
node references; no cloned AST or runtime expansion. Each copy then passes through
the unchanged group/atomic/atom proof in source order. Repeated nodes count their
visits; cancellation remains per pop. Group branch width is never guessed from
the repeat count or from a child's semantic set.

The existing proof still admits only one nonempty branch, transparent byte atoms,
positive direct exact Any/class repeats with #118 source kinds, one through eleven
consumed prefix bytes and exactly one final literal. Other nodes fail the proof.
Nested fixed group repeats can qualify, but actual byte width and total work stay
bounded; no recursive traversal or multiplicative arithmetic. Count>=12 rejects
before enqueue because any qualifying nonempty copy consumes at least one byte.
Empty/assertion-only groups are not admitted. A count1 group may enclose the full
prefix+literal; larger groups containing literals encounter multiple/trailing
semantic nodes and fail the existing sole-literal proof.

Class source kinds are unchanged. A grouped Atom property class is processed once
per copied body; its directly repeated class still cannot pass at count>1. Ordinary
bitmap classes do not gain eligibility merely by being grouped. Existing bool and
optional literal descriptor, local i/r/UCP folding, source flags and #117 charged
comparison supply all runtime behavior. Unrestricted copies remain unconditional;
restrictive copies require actual literal rejection at CR. VM captures preserve
last occurrence and shared rollback; the proof stores no capture state.

## Scope decisions and constraints

Keep variable/zero repeats, alternatives, ordinary bitmaps, specially optimized
property-bracket Any repeats, multiple-literal selection, assertions/anchors/calls/
resets and other optimizer/source-metadata work in #99. Historical positive interior-
range flags remain until coupled start-bitmaps. Cost: broader native-positive cases
still miss and known wider metadata false positives remain. This is a bounded
maintainer scope choice, not full JIT parity or completion of #99. UTF #102 and
unsafe native #100 remain separate; no UTF C or positive Xuc oracle.

Internal C#13/net8/net9/net10; UTF8 LF, root project/solution, src/tests/samples.
No subagents/Python/SDK installation, public/package/version/dependency/workflow/
release/consumer changes, matcher/VM/frame/journal edits, saved spans/pointers/
snapshots, engine clones, raised limits or native/Regex production fallback.
Depth256/captures256/instructions4096/marks256/frames1024/default work1,000,000;
proof1000 visits/window12 unchanged. Frozen35 intact. Grep last; CommandFramework
removal requires publication and all known consumer migrations. User owns merge.

## Verification / builder check

Native positive/miss full-capture pairs, capturing/noncapturing/atomic count1/2/11/12,
last-occurrence/nested captures, multi-atom and nested repeat byte offsets, lazy/
possessive forms, grouped versus direct property class repeats, local case/UCP/
ASCII policies and explicit-source controls. Every caller offset over six newlines/
two UCP choices/byte-NUL-high-CRLF subjects, supported UTF fragment controls and
shared observed/exact/exhausted work, cancellation, deep/compile limits. Tests-only
successful-build RED precedes source. All six native Staging platforms/net8/9/10
full/frozen214/configured gates green, zero failures/skips/build warnings/errors.
Separate whole-branch author review and exact head/tree/log/frozen/whitespace/clean
audit. Builder/interface check: one bounded enqueue helper consumes existing node
references and returns bool; unchanged proof returns existing metadata. No parser,
AST, matcher, capture or width interface conflict. User's no-subagent rule applies.
