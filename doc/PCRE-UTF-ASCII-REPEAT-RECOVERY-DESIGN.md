# Internal ASCII UTF root-repeat CRLF recovery

## Intent and native cause

Continue #7/#102 after merged #125. Correct the preexisting false-positive LF
candidate after a direct root-leading positive predicate-free ASCII class PLUS.
Pinned PCRE.NET1.6.0/PCRE2 10.48 JIT, UTF ANYCRLF:
[\x09-\x0c]+A on hex0B0D0A41 from0 misses, but explicit offset2 matches2:2.
On hex0B0D0A0B41 from0 native skips LF and matches the later VT at3:2.

Native pcre2_jit_compile.c detect_early_fail1397ff selects a root-leading class
PLUS as fast_forward_bc_ptr/type_skip for CLASS as well as XCLASS. Iterators
update their consumed-run endpoint; reset13976ff resumes ordinary CRLF bumpalong
there. #125 implemented the instruction marker and integer endpoint for wide
classes only; the width filter in FindUtfCrLfSkipRepeatSet leaves ASCII unmapped.

## Approaches and chosen scope

Implementer choice under standing autonomy: extend the existing marker selection
to ASCII and wide sets equally. Duplicating recovery machinery would add a second
path for the same native opcode behavior; globally disabling LF entry would undo
#103 and #125. Reuse #125's already-charged scalar comparisons and endpoint guard.
Cost: this stays a bounded root-leading class PLUS correction, not full optimizer
parity. Nested/capture-wrapped unbounded iterator provenance remains separate.

## Implementation and invariants

In src/Pcre/PcreByteProgram.cs FindUtfCrLfSkipRepeatSet, remove the wide local,
its range.End>127 aggregation and the wide requirement in the final return.
Return the set when LF is admitted and CR excluded. Keep root one-branch/first-node,
direct class min1/maxint, positive/predicate-free checks and possessive-versus-
explicit-atomic distinction. Keep per-range compile cancellation.

No new instruction field or matcher change: #125 marks scalar copies by AST
identity and retains one per-attempt integer maximum successful endpoint outside
calls/assertions. Its failed-attempt/proof/no-explicit-newline/newline-family/
same-fragment CRLF guard remains. No span/pointer/snapshot or frame/journal change.
All5941 baseline expectations/frozen35 stay unchanged; no raised work/storage limit.
Some wrong successful matches become correct misses or later matches; newly
selected ASCII repeat recovery can reduce charged work after a failed run.
Observed/unobserved exact/exhausted/cancellation controls must agree.

## Verification and remaining work

Native-first literal full spans pin greedy/lazy/possessive PLUS and counted{1,},
caller offset0 versus LF, later VT versus LF winners, nonmember/multibyte prefixes,
literal overlap and suffix captures. Finite/exact1/min2-unbounded/ordinary/atomic/
alternative/grouped-sequence and explicit-newline controls keep their behavior.
Every caller offset over six newlines/UCP/tolerant UTF/malformed fragments; strict
valid boundaries/errors; work/exhaustion/cancellation and no-LF default budget.
No unsafe positive-Xuc malformed probes (#100).

Tests-only draft -> observed successful-build RED with all5941 baseline passing
-> compiler-only marker selection -> six native Staging/net8/net9/net10 full/
frozen214/configured gates, seven workload equalities -> nine-file-or-smaller
whole-branch author review, exact head/tree/log/frozen/whitespace/clean audit and
ready PR. User owns merge; no agents/Python/SDK install. Preserve ignored evidence.

C#13, net8/net9/net10, UTF8 LF; root solution/project, src/tests/samples. No public/
package/version/dependency/workflow/release/consumer/native-or-Regex fallback or
engine clone change. Depth256/captures256/instructions4096/marks256/frames1024/
defaultwork1,000,000/proof1000 unchanged. Broader #102, byte #99 and unsafe #100
remain separate. Grep last; CommandFramework removal after publication and all
known migrations only.
