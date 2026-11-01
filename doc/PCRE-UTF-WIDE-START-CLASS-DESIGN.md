# Internal UTF wide leading-class candidate entry

## Intent and evidence

Continue #7/#102 after merged #124. Extend the existing positive predicate-free
UTF leading-class proof from ASCII to normalized scalar ranges in0..0x10FFFF.
For (*ANYCRLF)[\\x09-\\x0c\\x{100}]A on hex0D0A41, pinned native JIT finds1:2
with HASCRORLF0. Source inspection predicts the current managed miss because
HasUtfCrLfStartClass rejects range.End>127. Observe managed RED before source.

Pinned PCRE.NET1.6.0/PCRE2 10.48 pcre2_study.c1731ff accepts property-free
positive XCLASS, scans wide lists/ranges into UTF-8 leading-byte bits, then handles
the low map. Non-ASCII UTF-8 leaders do not add CR or LF. Therefore the normalized
scalar membership test for CR/LF remains sufficient in this bounded source shape.
Native disabled-start-optimization controls miss. Pre-flight research2296 native
observations/64 literal assertions pass. Caseless Kelvin/long-s/sharp-s/Greek/
Deseret controls also retain native LF entry; existing case closure stays shared.

## Approaches and implementer choice

A full native optimizer clone would broaden scope. Restoring membership-derived
UTF newline flags would undo #98 and break CR-including controls. Under standing
autonomous implementation authority, extend only the existing bounded range proof.
The tradeoff is conservative coverage: predicates, negation, optional/exact>1
repeats and broader optimizer selection stay separate, even where native matches.

## Change and invariants

In src/Pcre/PcreByteProgram.cs, HasUtfCrLfStartClass accepts normalized ranges
whose start>=0/end<=0x10FFFF instead of requiring end<=127. Retain every other
guard: positive nonempty ScalarSet, empty predicate list, every first class excludes
CR, at least one first class admits LF, same group/branch/atomic/bounded-variable/
exact1 restrictions, same cancellation and cumulative1000 proof visits. A direct
unbounded class PLUS needs the additional recovery below; nested/alternative wide
class PLUS contexts remain outside admission.
A range spanning surrogate values is still a normalized class range; runtime UTF
validation and scalar decoding remain the existing authority. No new decoder.

Existing UtfCrLfStartClass still controls ordinary entry. The first bound-only
implementation exposed five protocol mismatches: S+ A on U0100+CRLF+A natively
misses but the widened Boolean proof matches LF. All5881 baseline tests passed.
Native detect_early_fail (pcre2_jit_compile.c1397ff) marks a direct root-leading
class PLUS as fast_forward_bc_ptr/type_skip; iterator matching records its end,
and reset13976ff restores that end before ordinary CRLF bumpalong.

Mark the compiled scalar instructions for one direct first root min1/unbounded
wide predicate-free CR-excluding/LF-admitting class repeat. A possessive wrapper
contains the repeat directly; explicit atomic groups contain a noncapture group
and are not conflated. Other wide unbounded class-repeat contexts conservatively
fail proof; ASCII admission/work stays unchanged. Existing bounded variable
repeats and repeated groups retain their paths.

One per-attempt integer utfRepeatEnd retains the farthest successful marked scalar
end; it is deliberately not restored on backtracking, like native iterator private
state. Update only outside calls/assertions. Upon whole-attempt failure, when that
end lies at CRLF inside the same fragment, proof is valid and source metadata does
not explicitly mention CR/LF, continue after the pair for CRLF/ANYCRLF/ANY. Every
scalar comparison was already charged; no rescan/free comparison or snapshot.
No parser/AST/membership/source-flag/Step/capture/frame/journal/limit change. The
instruction marker is immutable; the endpoint is an integer, never a saved span. No-LF classes retain prior skip/work. More eligible LF attempts
may charge work; test exact/exhausted observed budgets without raising limits.
All5881 existing test expectations and frozen35 production blobs stay unchanged.

## Tests and remaining gates

New native-first literal spans cover wide scalar boundaries80/FF/100/7FF/800/
FFFF/10000/10FFFF, wide ranges/case closure/wrappers/alternatives/variable/exact1,
CR-including/predicate/negated/G/anchor controls and multibyte capture offsets.
All-offset matrices cover six newlines/UCP/tolerant UTF, strict valid boundaries,
fragment barriers, work/cancellation, no-LF default budget and proof34/220 depths.
No unsafe positive-Xuc malformed-UTF probes (#100). Native-only research of wider
exact/optional starts is not evidence those are admitted here; keep them #102.

Internal C#13/net8/net9/net10, UTF8 LF; root project/solution, src/tests/samples.
No agents/Python/SDK install, public/package/version/dependency/workflow/release/
consumer changes, production native/Regex fallback, duplicated engine or saved
spans/interior pointers/snapshots. Depth256/captures256/instructions4096/marks256/
frames1024/default work1,000,000; proof1000 unchanged. Byte #99 remains unchanged.
Tests-only draft successful-build RED, then the narrow proof/recovery fix, six native Staging
full/frozen214/configured gates green, whole-branch author review and exact
head/tree/log/frozen/whitespace/clean audit, ready PR. User owns merge. Grep last;
CommandFramework removal requires publication and all known consumer migrations.
