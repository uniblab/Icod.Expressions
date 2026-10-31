# Internal PCRE byte CRLF prefix selection

## Intent and native cause

Continue #7/#99 after merged #116. A fixed byte prefix may contain restrictive
native NOT/PROP/XCLASS/ANY positions, not just unrestricted ALLANY. With exactly
one following literal, native JIT scans for that literal at its fixed byte offset.
A CR candidate rejected by that test never enters the VM, so LF remains a search
candidate. If the test passes and a prefix atom fails in the VM, ordinary CRLF
bumpalong skips LF. Byte `[^\r]\CA` on CRLF+BA matches1:3; on CRLF+AA it misses.
Explicit offset1 matches both. Atomic/capture wrappers preserve this distinction.

Authority: pinned PCRE.NET1.6.0/PCRE2 10.48 JIT; pcre2_jit_compile.c scan_prefix
5877..6369, MAX_N_CHARS12 at376, sole-character choice/fast_forward_first_char2
around6450..6515 and6620..6675, bumpalong around5730..5840. PrefixAny marks native
unknown one-byte positions; mandatory bitmap classes stop native prefix scanning.
No multiple-literal selection or general start optimizer is inferred here.

## Approach and contract

Choices considered: globally retry LF (known false matches), implement all native
start optimizers (separate larger gate), or add one conditional literal descriptor
to the existing bounded proof. Choose the third under standing implementation
authority. It fixes the subject-dependent consequence while preserving old work
for the already admitted unconditional subset.

1. Reuse #116's one-pass, iterative, cancellation-aware single-branch proof,
   wrapper handling,1000 visit ceiling and12-byte window. Allow separate Any or
   PrefixAny class atoms in any order, and positive direct exact Any repeats
   (DotAll true or false). Sum one through eleven byte positions, then exactly
   one literal, and no trailing semantic operation. Any(false) is restrictive;
   every PrefixAny class is conservatively restrictive regardless of membership.
2. Preserve existing `ByteCrLfPrefix` bool for unrestricted prefixes and every
   one-position prefix. Those cases keep the same VM work/candidate behavior.
   Longer restrictive qualifying prefixes instead return an optional immutable
   `PcreBytePrefixLiteral` value: ByteOffset1..11, Value0..255, Caseless and
   CaselessRestricted. One private proof returns both results; no second traversal.
   UTF compilation produces neither result. This is program metadata, not saved
   VM state, an AST node, capture snapshot, pointer or span.
3. Only after a failed CR attempt within a CRLF pair, where existing newline/
   explicit-source/byte/UTF-start-class guards permit normal LF skipping, inspect
   the descriptor. Charge one `Step()` before attempting its bounded comparison.
   Compare using existing ScalarEquals and program.Ucp, preserving local i/r
   flags. If that literal matches at start+ByteOffset, skip LF; if it mismatches
   or is beyond the fragment end, retain LF as the next candidate. Check offset
   against fragment.End-start before addition/read. Other candidates and all
   preexisting unconditional proofs incur no additional comparison/work.
4. The existing VM still performs every admitted candidate and capture mutation;
   all its work remains charged. One extra literal test on each eligible failed
   CR is intentional and charged identically with/without an observer. Work
   exhaustion/cancellation can occur there and return the established diagnostic/
   exception. No native work-count equality is claimed. No new frame/journal path.

## Scope decisions and constraints

Keep source provenance explicit: repeated PrefixAny classes and repeated groups
remain outside this tranche because NOTEXACT/TYPEEXACT/XCLASS+CRRANGE encodings
need distinct analysis. Variable/zero repeats, ordinary bitmap classes, alternatives,
assertions, anchors, calls/resets, multiple literals and trailing operations remain
#99. Retain historical positive interior-range metadata until start bitmaps are
implemented together. UTF search #102 and unsafe native #100 stay separate. Cost:
these broader native-positive cases can still miss; this is not full JIT parity.

Internal C#13/net8/net9/net10; UTF8 LF, root project/solution, src/tests/samples.
No subagents/Python/SDK installation. Existing depth256/captures256/instructions4096/
marks256/frames1024/default work1,000,000 remain. No public/package/version/dependency/
workflow/release/consumer changes, production native/Regex fallback, snapshots,
engine clones or raised limits. Frozen35 unchanged. Grep last. CommandFramework
removal requires publication and migration of all known consumers. User owns merge.

## Verification and builder check

Native literal full-capture positive/miss counterparts, ordinary/atomic wrappers,
transparent class/property mixtures, CRLF dot/N exact repeats, case/UCP/i/r,
1/2/11/12 boundaries and explicit-source controls. Reintroduce every-offset
protocols for the two #116 research forms; include six newline conventions,
two UCP choices, malformed-UTF-looking byte subjects and supported UTF scalar
controls. No unsafe UTF C/Xuc probe. Observer/exact and tiny budgets, cancellation
at the new comparison, deep wrappers and compile ceilings; tests-only successful-
build RED precedes source. All six native Staging jobs/net8/9/10 full/frozen214
and configured gates must pass. Separate whole-branch author self-review and exact
head/tree/log audit. Interface/builder check: one proof supplies bool+optional
metadata; shared ScalarEquals/Step supply folding, budget and cancellation; bounds
exclude overflow/overread. No capture or width interface conflict.
