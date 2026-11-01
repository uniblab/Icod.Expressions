# Internal PCRE byte-FF bitmap repeat prefixes

## Intent and native cause

Continue #7/#99 after #122. Native scan_prefix class handling assigns broad255
when the bitmap's byteFF bit is set, independently of its cardinality. Extend
#122's existing broad-bitmap guard for that precise case.

Pinned PCRE.NET1.6.0 / PCRE2 10.48 JIT, byte ANYCRLF: let F be the ordinary
bracket `[^\x00-\x09\x0b-\x0c\x0e-\xfe]`, whose byte membership is CR/LF/FF.
F{2}A matches1:3 on CRLF+FF+A, but a group containing one F repeated twice misses.
Count1/unquantified F also stops prefix scanning. A corresponding three-byte set
containing FE but excluding FF remains a sparse bitmap, even when FF appears as
a source operand. The distinction is the effective bitmap bit, not source text.

Authority: pinned pcre2_jit_compile.c class scan6255ff checks bytes[31]&0x80 before
the normal MAX_DIFF_CHARS5 overflow path. Exact CRRANGE continues and count1/atoms
terminate as pinned in #122. Final sole literal/window12 and selector unchanged.

## Approach and contract

Alternatives: enumerate all sparse bitmap optimizers, match raw source FF tokens,
or extend the existing Bitmap-provenance broad guard with effective FF membership.
Choose the third under standing implementation authority. Native source makes this
another broad255 path, requiring no new optimizer or runtime state.

Keep Bitmap provenance, exact direct min=max>=2, CR and LF membership, total width
1..11, sole final literal/window12, proof1000 and ordinary/group-copy exclusions.
IsBroadBitmap accepts Contains(0xFF) OR popcount>=6 after requiring CR/LF. The old
cardinality path stays byte-for-byte intact; the FF path short-circuits four
popcounts. No parser/AST/source-flag/set/matcher/VM/Step/capture/journal/frame,
instruction expansion, cancellation or resource-limit change.

## Scope and constraints

Sparse bitmaps without FF, CR-excluding/non-LF sets, legacy Atom mixtures, coupled
positive-interior flags, variable/zero and multi-literal/alternative/assertion/
anchor/call/reset forms and general optimizer availability remain #99. Known
broader native-positive misses/false positives remain; no full JIT parity claim.
UTF #102 and unsafe native #100 separate; UTF scalar/fragment rules untouched.

Internal C#13/net8/net9/net10; UTF8 LF; root project/solution, src/tests/samples.
No subagents/Python/SDK install, public/package/version/dependency/workflow/release/
consumer changes, production native/Regex fallback, engine clones, saved spans/
pointers/snapshots or raised limits. Depth256/captures256/instructions4096/marks256/
frames1024/default work1,000,000; proof1000/window12. Frozen35 intact. Grep last;
CommandFramework removal requires publication and all known migrations. User merge.

## Verification / builder check

Native-first literal full captures, byteFF versus FE membership/source controls,
count0/1/2/11/12, direct versus copied group, wrappers, local case/UCP, sole-literal
selector counterexamples and actual offsets. Six newline/every-offset byte and
supported tolerant UTF controls; shared work/exhaustion/charge/cancellation and
compile ceilings. Sparse no-FF/ordinary prior work stays pinned. Tests-only
successful-build RED before source; all5734 baseline cases pass. Six native
Staging/net8/9/10 full/frozen214/configured gates green, zero failed/skipped/build
warnings/errors. Author review and exact head/tree/log/frozen/whitespace/clean audit.
Builder/interface check: one existing local bool helper changes its broad predicate;
no producer or runtime consumer interface changes. Native bit bound is byte255.
