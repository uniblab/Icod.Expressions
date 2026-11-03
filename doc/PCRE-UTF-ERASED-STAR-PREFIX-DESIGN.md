# UTF star ownership after erased primitives

## Intent and evidence

Continue #7/#102 after merged140. A direct zero-count primitive/reference leaves
no native opcode. A following supported star is still the first accelerated
iterator and owns type_skip, even though its AST position is not root index0.
Current root-star identity discovery checks index0 only, so it misses bitmap
LF entry after these erased prefixes. Native PCRE.NET1.6.0/PCRE2 10.48 JIT
initial264 checks cover all relevant omitted categories, aliases/UCP and star
families. With S=[\x09-\x0c\x{100}], X{0}B*(S+)A selects CRLF+A1:2,
U0100+CRLF+A3:2 and VT+CRLF+VT+A2:3, but misses B+CRLF+A after the star
consumes B. Erased \r{0} retains ExplicitNewline and permits LF2:2 in that
consumed example. Negated singleton [^\r]{0} does not set that flag.
Native compile/study omits these operands; JIT detect_early_fail still assigns
count0/type_skip to the first surviving star. Retained zero groups and finite
queries have different cache effects and must not be erased by this discovery.

## Approach under standing authorization

Share the existing erased-repeat predicate between leading-node traversal and
root-star discovery. Expand discovery to a cancellation/1000-visit bounded scan
through root branch zero-count primitives only. Stop at the first other node;
record its child only if it is min0/maxint.MaxValue star (unwrap the existing
possessive atom layer). This follows the existing native omission vocabulary
without broadening wrappers or inventing a second optimizer path.

Private static IsUtfCrLfErasedRepeat(PcreByteNode) returns true only for Max0
Child Backreference/Literal/Any/ScalarClass/ScalarSet/NewlineSequence, exactly
matching the old first leading-node switch arm. Existing helper uses it for
cost2/retained false. Root discovery becomes an instance helper, checks CT per
node, charges root1 and2 per erased operand plus possessive wrapper1, and
returns null on the shared MaximumStartClassVisits bound. It never skips a
retained group/assertion/conditional, positive query or unsupported source.

Retain root-identity clearing if no eligible following set PLUS exists, all star
admission/source/casing/UCP/CR/predicate/range/capture/depth/cost/BlocksSkip guards,
and the root-star atomic-following barrier. Reuse existing instruction marker,
per-attempt endpoint/reset and consumed-run recovery unchanged. No matcher/
parser/membership/folding/program/instruction/frame/journal layout/state/Step/
limit/public/package/version/workflow/dependency/consumer changes.
Keep parser256/captures256/instructions4096/marks256/frames1024/work1m/
proof1000/window12. All9912 baseline expectations/frozen35 unchanged.

## Verification and boundaries

Native-first full captures for erased primitive/reference categories and counted/
possessive aliases, sequences of erasures, literal/set/type star families,
greedy/lazy/possessive/counted stars, empty/consumed-to-CR runs, UCP/source/casing,
ExplicitNewline/negated singleton flags, following captures/dependencies/deep/
atomic groups and barrier/mixed/unsupported controls. Retained zero groups stay
barriers, not erased nodes. Wrapper-first-star controls pin ordinary consumption
only; first stars inside wrappers remain separate proof work.
Six newline modes/UCP/every offset/strict-tolerant fragments, exact/exhausted/
observed work/cancellation/default budget/parser257/backtrack-depth rejection
remain behavioral tests. No unsafe Xuc probe/source-text assertions.

Tests-only draft -> successful-build new-class RED/all9912baseline pass -> source
-> six native Staging OS/arch/net8/9/10 full/frozen214/configured gates/7equal
workloads -> wholebranch author review/strict logs/tree/blob/tests-afterRED/
frozen/clean audit -> issue102/PR ready; user merge. No subagents/Python/SDK
installation. C#13/net8/9/10, UTF8 LF, root project/solution,src/tests/samples.
Keep102 open for wrappers/other placement/bracket predicates/group/alternative/
negated/property/call/DEFINE/condition availability; byte99/unsafe100 separate.
No complete PCRE/release/Grep-readiness claim. CF untouched until publication/
all known migrations; Grep last.
