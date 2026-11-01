# Internal PCRE broad byte bitmap repeat prefixes

## Intent and native cause

Continue #7/#99 after #121. Native OP_CLASS/OP_NCLASS followed by a positive exact
CRRANGE can scan through its fixed copies to the next literal. An unquantified
bitmap instead ends scan_prefix after its own position; count1 is elided and
group copies contain unquantified bitmaps. Preserve all three distinctions.

Pinned PCRE.NET1.6.0 / PCRE2 10.48 JIT, byte ANYCRLF: `[\D]{2}A` and
`[^\x00\x01]{2}A` match1:3 on CRLF+BA, `[\s]{2}A` matches1:3 on CRLF+HT+A.
`(?:[\D]){2}A` and direct `\H{2}A` miss on CRLF+BA. UCP can turn bracket
shorthands into XCLASS; local ASCII policies can retain bitmap lowering.

Authority: pinned pcre2_jit_compile.c scan_prefix OP_CLASS/NCLASS6060ff,
CRRANGE exact min/max6228ff and broad count overflow6250ff. MAX_DIFF_CHARS5;
six distinct bytes produce count255 and no selective prefix candidate. Final
literal selection/priorities6547ff and the existing twelve-position window apply.

## Approach and contract

Alternatives: emulate every bitmap/start optimizer now, treat semantic classes
as transparent regardless of source, or admit a conservative source-tagged subset
through the existing proof. Choose the third under standing implementation
authority; broader sparse/metadata work stays #99.

Add Bitmap to existing PcreByteClassPrefix for ordinary bracket classes that
otherwise have None. PrefixAny must explicitly exclude Bitmap: ordinary atoms,
count1 and group-copy bodies keep their prior rejection. Existing None on direct
h/H/v/V lowerings remains None. Existing Exact/Atom/AnyProperty kinds and #121
membership/source metadata remain unchanged. Small bracket encodings can collapse
to literals natively; the new repeat guard never admits these by source tag alone.

Admit direct PcreByteClass Bitmap repeats only for exact min=max>=2, containing
both CR and LF and at least six distinct bytes. Four BitOperations.PopCount calls
count the existing immutable set; no enumeration, allocation or semantic encoding
guess without the bracket provenance. Six or more excludes native literal/small
class collapse and matches native's broad scan count. Requiring CR and LF avoids
admitting new start-bitmap exclusion behavior or needless LF work for these forms.
The existing AddWidth requires total width1..11 and exactly one final literal
within twelve positions. Wrappers around the repeat are allowed; repeated groups
containing ordinary bitmap atoms remain excluded. Counts0/1/variable forms stay.

Use existing restrictive bool/conditional selector, charged comparison, case/UCP,
captures, cancellation and1000 proof visits. No matcher/VM/Step/state/frame/journal,
membership, explicit-newline flag, instruction expansion or limit change.

## Scope / constraints

Sparse bitmaps, byteFF-only small-set optimizations, CR-excluding/non-LF bitmaps,
Any-property bitmap mixtures retaining legacy Atom, source positive-interior
flags, broader optimizer selection and variable/zero/multi-literal/alternative/
assertion/anchor/call/reset forms remain #99. Known broader misses/false positives
remain. UTF #102 and unsafe native #100 separate; no full JIT parity claim.

Internal C#13/net8/net9/net10; UTF8 LF; root project/solution, src/tests/samples.
No subagents/Python/SDK install, public/package/version/dependency/workflow/release/
consumer changes, production native/Regex fallback, engine clones, saved spans/
pointers/snapshots or raised limits. Depth256/captures256/instructions4096/marks256/
frames1024/default work1,000,000; proof1000/window12. Frozen35 intact. Grep last;
CommandFramework removal requires publication and all known migrations. User merge.

## Verification / builder check

Native-first full captures, count1/2/11/12, six-byte whitespace versus sparse
vertical controls, byte shorthand/POSIX/UCP/ASCII distinctions, direct versus group
copies, local case and selector counterexamples. Every offset over six newlines/
two UCP choices, supported tolerant UTF controls, observed/exact/exhausted budgets,
cancellation and compile limits. Tests-only successful-build RED before source;
all5662 baseline cases pass. Six native Staging/net8/9/10 full/frozen214/configured
gates green, zero failed/skipped/build warnings/errors. Author whole-branch review,
exact head/tree/log/frozen/whitespace/clean audit. Interface check: parser produces
Bitmap, atom consumer excludes it, exact-repeat consumer validates source plus
membership/count. All three must change together; existing runtime result shared.
