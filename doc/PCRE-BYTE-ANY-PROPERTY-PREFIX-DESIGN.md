# Internal PCRE byte Any-property bracket prefixes

## Intent and native contract

Continue #7/#99 after #119. In byte mode, native bracket compilation handles
PT_ANY without requiring XCLASS property data: positive Any fills the bitmap;
negative Any adds no bits. A resulting full byte bitmap can lower to ALLANY and
its exact repeat can expose the sole final literal. Other property operands still
require XCLASS, even when the projected membership is identical.

Pinned PCRE.NET1.6.0 / PCRE2 10.48 JIT, ANYCRLF, CRLF+BA:
`[\p{Any}]{2}A`, `[\P{^Any}]{2}A`, `[^\P{Any}]{2}A` match1:3;
`[\P{Any}\p{L}\P{L}]{2}A` misses. The first forms match0:3 on CRLF+AA.
Authority: pinned pcre2_compile_class.c PT_ANY handling1449..1456, actual property
emission1459..1469, XCLASS_REQUIRED1713 and final full-bitmap ALLANY1854..1868;
pcre2_jit_compile.c bounded prefix scanning and the existing #117 selector.

## Approach

Alternatives: infer native encoding from membership alone (loses real-property
provenance), independently special-case raw source text (duplicates the parser),
or carry the Any-property distinction from the shared property parser. Choose
the third under standing implementation authority.

Add AnyProperty to the existing class-prefix enum. CreatePropertyClass marks
byte AnyScalar this way; it retains the existing PrefixAny atom admission and
exact direct-property repeat admission. Bracket-item tuples carry separate
PrefixAny and XclassProperty booleans. AnyProperty contributes the former but
not the latter; other admitted property projections contribute both. POSIX
projections retain their existing Unicode-property classification. Literals and
nonproperty bitmap escapes contribute neither. Bracket aggregation retains the
old atom admission, including empty negative-Any classes; only full-byte bitmap
exact-repeat eligibility uses the new XCLASS requirement aggregate.

No membership, case closure, source-newline flags or parsing grammar changes.
The compiler admits direct AnyProperty exact repeats alongside Exact. Existing
width1..11/sole-final-literal/window12/proof1000 checks and #117 selector provide
runtime behavior, work/cancellation and capture spans. #119 group-copy proof
remains shared. Full sets remain conservatively restrictive as before.

## Boundaries and rulings

Do not admit non-full bitmap classes, variable/zero repeats, multiple literals,
alternative/assertion/anchor/call/reset prefixes or wider optimizer forms here.
Keep coupled positive-interior-range metadata/start bitmaps in #99, UTF search
in #102 and unsafe native UTF Xuc in #100. Known broader misses/false positives
remain; this is not full parity or closure of #99.

Native mixed positive Any plus real property can omit the full bitmap when no
ordinary byte item sets HAS_8BIT_CHARS: `[\p{Any}\p{L}]{2}A` misses on CRLF+AA
under both JIT and interpreter. The existing library semantic union differs.
This separate membership/provenance discrepancy stays tracked in #99; do not
change semantic sets to address a bounded prefix proof. Such mixed controls use
known-agreeing subjects only; they are not part of the exhaustive offset matrix.

Internal C#13/net8/net9/net10; UTF8 LF; root project/solution, src/tests/samples.
No subagents/Python/SDK installation, public/package/version/dependency/workflow/
release/consumer changes, matcher/VM/frame/journal edits, saved spans/pointers/
snapshots, engine clones, raised limits or production native/Regex fallback.
Depth256/captures256/instructions4096/marks256/frames1024/default work1,000,000;
proof1000/window12 unchanged. Frozen35 intact. Grep last; CommandFramework removal
requires publication and all known consumer migrations. User owns merge.

## Verification and builder check

Native-first all-capture fixtures: property/caret complements, repeated operands,
literal/bitmap/property mixtures, local i/r/ASCII/UCP, capture/noncapture/atomic
wrappers, direct/grouped forms, lazy/possessive, count1/2/11/12 and miss controls.
Every offset over six newlines/two UCP choices/byte subjects, supported tolerant
UTF controls, shared observed/exact/exhausted budgets and cancellation, empty
class prior work and depth/repeat limits. Successful-build tests-only RED before
production. Six native Staging platforms/net8/9/10 full/frozen214/configured gates
green; zero failures/skips/build warnings/errors. Separate author review and
exact tree/head/log/frozen/whitespace/clean audit. Interface check: new enum kind
is consumed by bracket aggregation and exact-repeat guard, preserving atom and
direct exact behavior. No runtime semantic, width, state or API interface change.
