# Finite optional prefixes before UTF starter classes

## Intent and approach
Continue #7/#102 after merged #155. Native set_start_bits accumulates the bitmap
of supported QUERY/UPTO literal, class and type opcodes before proceeding to a
mandatory starter. The PLUS nullable walker already checks that bounded atom
list, CR exclusion and source-sensitive caseless bitmap availability. The
general starter-class proof still duplicates only the zero-count/boundary subset.

Reuse FindUtfCrLfLeadingNode with an optional allowUnboundedOptionalRepeats bool
defaulting true. Thread it into CanCrossUtfCrLfOptionalRepeat; finite repeats
remain admitted, while unbounded repeats also require the new flag and their
existing owner/BlocksSkip guard. HasUtfCrLfStartClass calls the shared walker
with this flag equal to utfCrLfLeadingRepeatSet is not null. This replaces the
duplicate general scan and admits only finite optional atoms in that path.
Keep all accepted atom kinds, retained state, range/source guards, cost accounting
and exhausted-branch rejection. This does not prove group-return continuation
before plain classes or grant any new star/PLUS recovery ownership.

Considered duplicating the optional atom switch or admitting every nullable AST
node. Duplication risks divergent native source classification; general
nullability does not prove bitmap availability or JIT recovery ownership.
Chosen shared walker is an implementer decision under the standing instruction
to continue autonomously. Existing C#13/compiler architecture remains.

## Constraints and verification
Root project/solution,src/tests/samples,UTF8 LF; net8/9/10. No subagents/Python/
SDK installation. Parser256/captures256/instructions4096/marks256/frames1024/
work1m/proof1000/window12 unchanged. Preserve all23947 baseline expectations and
35 frozen production blobs. No parser/emitter/matcher/Step/VM/journal/API/limit/
workflow/package/dependency/consumer/CommandFramework changes.

Native-first captures for greedy/lazy/possessive query/bounded repeats, literals,
ASCII types/UCP distinction,horizontal space,sets/caseless source distinctions,
mixed zero/boundary prefixes,whole-expression wrappers,required repeats,
alternatives and dependencies. Explicit CR/property/negation/caseless/unbounded/
nullable-group/exact-large/anchor controls. Six-newline/UCP/every-offset tolerant
fragments,strict UTF validation,work exact/exhausted/observed,cancellation,
default budget and long ordinary-prefix controls. No bitmap parity claim beyond
the conservative1000 visit ceiling.

Observe successful-build new-class RED with all23947 baseline tests passing
before source changes; tests immutable afterward. Audit six native Staging jobs,
full/frozen214/net8/9/10,zero failures/skips/compiler warnings/errors and configured
gates/seven equal component workloads. Author whole-branch review,exact remote
tree/blobs/frozen/clean audit before ready unmerged PR. Append one progress-index
row; update only issue102 footer after CI. Preserve all earlier history/index
block. Keep102/unsafe100/release/Grep gates open. Publication and all known
consumer migrations precede CommandFramework removal; Grep last.
