# Internal UTF deep noncapture repeat entry

## Intent and native evidence

Continue #7/#102 after merged #127. Distinguish a known leading repeated class
from native repeat acceleration. A deep positive predicate-free wide class PLUS
can supply native start bits even when the JIT no longer accelerates its run.

Pinned PCRE.NET1.6.0/PCRE2 10.48 JIT, UTF ANYCRLF, S=[\x09-\x0c\x{100}]:
four noncapture groups around S+ followed by A on U0100+CRLF+A miss. Five or
more groups match LF at3:2. On VT+CRLF+VT+A, four groups select later VT3:2;
five or more select LF2:3. Initial CRLF+A matches1:2 at every pinned depth.
Pre-flight45 native observations cover greedy/lazy/possessive and depth4/5/6/34/200.

Native detect_early_fail OP_BRA descent stops at depth>=4. Native set_start_bits
in pcre2_study.c1731ff still accepts property-free positive XCLASS leading-byte
maps through deeper groups within its1000-call ceiling. #127 returns no class
beyond four groups; HasUtfCrLfStartClass conservatively rejects an unrecognized
wide unbounded repeat. That conflates proof availability with iterator acceleration.

## Approaches and implementer choice

Under standing autonomy, return compile-time classification (Set, Skip): recognize
the same mandatory leading class through bounded single-branch noncapture chains,
but mark its scalar instructions only through depth4. Reusing the old selector
without this distinction would either retain the miss or incorrectly skip LF.
A new matcher algorithm would duplicate existing behavior and is unnecessary.
Cost: recognition remains conservative for captures/alternatives/atomic/quantified
group prefixes and broader optimizer contexts; this is not full #102 parity.

## Change and invariants

In src/Pcre/PcreByteProgram.cs rename the private selector to
FindUtfCrLfLeadingRepeat, returning (PcreByteScalarSet? Set, bool Skip).
Iteratively follow noncapture first-node bodies with the existing1000 proof bound,
single/nonempty checks and per-descent cancellation. Keep terminal min1/unbounded,
positive/predicate-free, CR-excluding/LF-admitting and possessive/atomic rules.
Return a recognized set plus Skip=(groupDepth<=4). All failed shapes return default.

Compiler-only fields utfCrLfLeadingRepeatSet and skipUtfCrLfLeadingRepeat receive
that classification before emitting instructions. HasUtfCrLfStartClass recognizes
the selected leaf through AST identity independently of Skip and still performs
its existing1000-node proof. Emit SkipCrLfAfterRepeat only if Skip is true and the
scalar node is that leaf. Roots and depth1..4 retain #125-127 behavior; deeper known
chains get bitmap LF entry with no repeat-endpoint skip.

No matcher/program/instruction layout/decoder/source metadata/Step/capture/frame/
journal or saved-state change. Tuple/bool live only in compilation; no saved
span/pointer/snapshot. No new runtime scan or waived comparison. Parser depth256,
captures256/instructions4096/marks256/frames1024/defaultwork1,000,000/proof1000
remain unchanged. All6108 prior expectations and frozen35 unchanged. Newly admitted
LF attempts can charge additional real VM work; exact/exhausted controls pin it.

## Verification and remaining gates

Native-first full spans for depths4/5/6/34/200 and ASCII/wide, greedy/lazy/possessive/
counted, initial CR/member-prefix/later LF versus VT, head/whole-expression groups,
suffix captures/local case/empty/no-LF/CR-including/explicit-LF/G controls.
Six newlines/UCP/all caller offsets, strict boundaries/tolerant fragments, exact/
exhausted observed work/cancellation, no-LF default budget and parser depth cap.
No unsafe positive-Xuc malformed native probes (#100).

Tests-only draft -> observed successful-build RED with all6108 baseline passing
-> compiler classification -> six native Staging OS/arch/net8/9/10 full/frozen214/
configured gates, seven equal workloads -> whole-branch author review and exact
head/tree/log/frozen/whitespace/clean/test-afterRED audit -> ready PR; user merge.

C#13/net8/net9/net10, UTF8 LF; root solution/project, src/tests/samples. No agents/
Python/SDK install, public/package/version/dependency/workflow/release/consumer/
fallback or engine-clone changes. Broader102/byte99/unsafe100 remain separate.
Grep last; no CommandFramework removal before publication/all known migrations.
