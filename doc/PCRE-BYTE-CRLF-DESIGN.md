# Internal PCRE byte class CRLF continuation

## Intent and boundary

Continue issue #7 and the bounded cases in #99 after #98. Pin PCRE.NET1.6.0 /
PCRE2 10.48 Compiled/JIT byte-class negation metadata and the two-atom byte prefix
that explains the recorded discrepancies. Keep #99 open for broader prefix forms.

## Evidence and approaches

Native subject `0D0A41`, ANYCRLF, requested offset0:
`[^\r]A`, `[^\r-\r]A`, `[\x00-\xff]A` match1:2; `[\x09-\x0e]A`
and `[^\x00\x01]A` miss. All have HASCRORLF=0. Disabling start optimization
makes the first three miss. Their UTF counterparts miss too.

Pinned source pcre2_jit_compile.c5730..5840 performs CRLF bumpalong, while
13812..13825 subsequently selects prefix/first-character/start-bitmap scanning.
scan_prefix treats OP_NOT/NOTI and OP_ALLANY as unknown one-byte positions and
continues to the next literal in byte mode; it stops there in UTF. An ordinary
mandatory bitmap class ends the prefix at that position (6010..6320).
Thus the two-atom optimized byte prefix rejects the initial CR candidate by its
following literal, then enters the VM at LF without bumpalong seeing CR.

Choices considered: change global CRLF skipping (breaks existing UTF and ordinary
class controls); implement all native prefix optimizers (large separate gate);
or retain the smallest source distinction needed for this two-atom consequence.
The implementer's choice is the third under the standing autonomous development
remit. No general performance optimizer is added.

## Contract and implementation

1. Byte bracket classes preserve parsed operand count, sole-literal/equal-range
   shape, and actual literal/range-endpoint CR/LF before folding/deduplication.
   Correct negated metadata to suppress only a single parsed literal/equal range.
   **Retain historical positive interior-range newline flags for now.** Clearing
   them globally introduced lost matches: native `[\x09-\x0c]A`, exact repeated
   ranges, longer all-byte prefixes and atomic wrappers can enter at LF through
   other optimizers. Their source metadata flag is actually0; the old library
   flag preserves these matches accidentally. Some positive interior-range false
   positives, including `[\x09-\x0e]A` on `0D0A41`, therefore remain in #99.
   Complete endpoint-only byte metadata requires native start-bitmap/general
   prefix coverage first. UTF endpoint-only provenance from #98 is unchanged.
2. PcreByteClass gains a default-false `PrefixAny` AST flag: true for a negated
   single parsed literal/equal range (native NOT/NOTI), or a completed all-byte
   class (native ALLANY). Dot/shorthand classes are outside this tranche.
3. PcreByteProgram gains `ByteCrLfPrefix`. Set it only for byte root alternatives
   with exactly one branch of exactly two nodes, first a PrefixAny class, second
   a literal. Single-branch capturing/noncapturing wrappers may enclose the whole
   sequence or either atom; unwrap those only. No repeats, assertions, anchors,
   calls, conditionals, atomic wrappers or alternatives are admitted.
4. On failed CR under ANYCRLF with no explicit-newline metadata, suppress the
   ordinary LF skip when ByteCrLfPrefix is true. With metadata=false the literal
   cannot be CR/LF; native prefix scanning would reject CR because position1 is LF.
   This flag models that semantic consequence only; the existing VM still runs
   each candidate and accounts for every transition/comparison. No new input scan,
   observer gap, allocation, saved frame, span, recursion edge or work exemption.
5. Original subject offsets/captures, caller offset1, G, LF/UTF controls, malformed
   UTF barriers and work/cancellation/resource ceilings remain existing contracts.
   Byte property support and general start-optimization parity remain separate.

## Constraints and verification

Internal C#13 net8/net9/net10, UTF-8 LF. No public API, package, dependency,
workflow, consumer, release changes or native/Regex production fallback. Frozen
35 BRE/ERE files untouched. Existing depth256/captures256/instructions4096/
marks256/savedframes1024/default work1,000,000. User owns merge; CommandFramework
removal remains gated on publication and all consumer migrations; Grep stays last.

Tests pin native-first literal full captures and negation controls, duplicate and
mixed classes, equal/interior/endpoint ranges, quotes/xx/caseless, wrappers and
excluded optimizer shapes, every caller offset, byte LF versus ANYCRLF, UTF and
malformed fragments, observed/unobserved/cancellable matches, work exhaustion and
pre/mid cancellation. The reviewed interior-range match regressions are pinned
without claiming full byte metadata parity. Publish tests-only draft and observe executable expected
red before production. No local SDK; use six native Staging CI jobs and configured
full/frozen/workload/extraction/API/package gates, then fresh whole-branch review.

A new CR-excluding ASCII control also exposed an existing UTF start-bitmap gap:
`[\x09-\x0c]A` on `0D0A41`, offset0, native1:2 but managed miss. It is not
introduced by this byte change. Track it in #102 separately from #99; no claim of general
UTF candidate-optimizer parity. Shared matrix controls use the ordinary
CR-including bitmap; all five byte review regression rows remain explicit.
