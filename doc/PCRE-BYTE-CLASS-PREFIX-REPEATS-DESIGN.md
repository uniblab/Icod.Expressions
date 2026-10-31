# Internal PCRE repeated byte class prefixes

## Intent and evidence

Continue #7/#99 after #117. Native byte NOT/NOTI and direct character types/properties
can expose a following literal through exact repetition. XCLASS uses a trailing
CRRANGE opcode that stops native prefix scanning after a repeated property bracket.
Source provenance must distinguish these cases even when their byte sets agree.

Pinned PCRE.NET1.6.0/PCRE2 10.48 JIT, byte ANYCRLF:
`[^\r]{2}A` on CRLF+BA matches1:3, on CRLF+AA misses; ordinary/atomic wrappers and
fixed lazy/possessive counts preserve those outcomes. `\p{Any}{2}A` and
`[\s\S]{2}A` (non-UCP) match LF on CRLF+BA. The equal-membership property union
`[\p{L}\P{L}]{2}A` misses there, and matches0:3 on CRLF+AA. UCP can turn bracket
shorthands into property operands, so all-byte membership alone is insufficient.

Authority: pinned pcre2_compile.c single-source negation shortcut6350..6410,
direct property/Any8310..8337, class repeat dispatch7413..7443;
pcre2_compile_class.c property XCLASS1713ff versus full-bitmap ALLANY1854..1868;
pcre2_jit_compile.c byte scan_prefix NOTEXACT/TYPEEXACT6167..6194 and XCLASS6067ff.
Ordinary bitmap repeat/start-bit optimizers are not modeled by set equality.

## Selected design

Use a source enum on existing PcreByteClass, not a second engine or runtime path:
`PcreByteClassPrefix { None, Atom, Exact }`. Replace constructor's bool PrefixAny
with default-None PrefixKind; retain computed PrefixAny for existing bracket
aggregation and atom proof. None stops the proof; Atom admits one occurrence;
Exact additionally admits positive direct fixed repetitions. This removes the
ambiguity in #117 without adding a program/VM field or changing matching sets.

Parser ownership:

- A negated single parsed literal/equal range is Exact (NOT/NOTI/NOTPROP).
  Duplicate source operands remain distinct; semantic deduplication is not proof.
- A completed all-byte bracket without property operands is Exact (ALLANY).
  Any property-bearing bracket stays Atom, including conservative bracket-Any
  cases whose special native optimization remains a separate follow-up.
- Direct supported property escapes are Exact. Direct d/D/s/S/w/W and dot/N
  lowerings are Exact even when their existing semantic representation is a byte
  bitmap: native uses transparent type opcodes, not ordinary class encoding.
  Existing UCP/ASCII bracket policies remain authoritative; bracket shorthand
  unions do not inherit direct-type provenance. h/H/v/V remain outside this proof.
- Other brackets are None unless their previous property/PrefixAny distinction
  admits Atom. Preserve explicit-newline provenance and historical range flags.

Compiler ownership: reuse the existing one-pass1000-visit/twelve-position proof,
ordinary/atomic single-branch wrappers and #117 bool/optional literal selector.
A direct class repeat qualifies when min=max>0 and kindExact; kindAtom additionally
qualifies at count1 because native removes its repeat opcode. Counts sum1..11
before exactly one final literal. Treat class positions conservatively restrictive;
the existing conditional comparison handles a literal passing at CR followed by
class failure. Existing Any repeats remain unchanged. No repeat child-group
unwrapping or class expansion during analysis; reject other bounds/shapes.

## Alternatives, scope and cost

Reject inferring repeatability from semantic membership: the property-union
counterexample would gain false LF matches. A complete native optimizer IR is a
larger #99 gate. The selected enum is a maintainer implementation choice under
standing authorization; it records only the distinction needed now. Cost: repeated
property brackets (including specially optimized bracket-Any), ordinary bitmap
classes, repeated groups, variable/zero repetitions, multiple literals, assertions/
anchors/calls/resets and other optimizer/source-metadata cases remain #99. Some
valid native-positive shapes still miss. Keep #99 open; no full JIT parity claim.

No matcher/Step/ScalarEquals/frame/journal/limit change. New qualifying restrictive
prefixes use #117's one charged literal comparison after eligible failed CR; old
proofs retain their classifications/work. UTF never enters this byte proof; #102
and unsafe native #100 remain separate. No native/Regex production fallback.
Internal C#13/net8/net9/net10, UTF8 LF; root project/solution, src/tests/samples.
No agents/Python/SDK installation/public/package/version/dependency/workflow/release/
consumer changes, saved spans/pointers/snapshots, engine clones or raised limits.
Depth256/captures256/instructions4096/marks256/frames1024/default work1,000,000.
Frozen35 intact. Grep last; CommandFramework removal requires publication and all
known consumer migrations. User owns merge.

## Verification / design-builder check

Native full-capture positive/miss pairs, count1/2/11/12, lazy/possessive exacts,
negated singleton/equal-range versus duplicates/ordinary bitmaps, direct property/
ASCII/UCP shorthand versus property brackets, all-byte controls and direct dot/N.
Every caller offset over six newlines/two UCP choices and byte/NUL/high/CRLF subjects;
supported UTF controls never probe unsafe UTF C/Xuc. Observer/exact/exhausted budgets,
cancellation and deep/resource ceilings stay shared. Tests-only successful-build
RED precedes source; all six native Staging platforms/net8/9/10 full/frozen214 and
configured gates green, zero failures/skips/build warnings/errors. Separate whole-
branch author review; exact head/tree/log/frozen35/whitespace/clean audit.
Builder/interface check: enum is AST provenance only; existing aggregation reads
computed PrefixAny; one compiler repeat arm consumes kind/count, existing selector
and matcher do all runtime work. Record rewrites retain fields. No width/state or
consumer interface conflict. Prior atom admission must remain unchanged.
