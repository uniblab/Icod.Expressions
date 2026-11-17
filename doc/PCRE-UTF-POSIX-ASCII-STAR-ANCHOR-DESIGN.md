# UTF POSIX ASCII unions and star anchoring

Continue merged274 at0d085dd8164950f83d62f9dc815ef24427a15e37,
tree7f2e53d3d53671b33010a1f3dd289c4dd39ab82f; floor101,234 tests.
Execute inline; no subagents or local .NET SDK installation. Development
branch/PR updates authorized; user owns merge/tags/package publication.
Preserve340 prior test files/335 C# sources/frozen35. #7/#102 stay open,
Grep last; leave issue7's existing publication-blocked footer unchanged.

## Native source and executable evidence

Pinned PCRE2 10.48 commit7978954dbd2efc6f2196869290553cf1871b4ce6 with
SLJIT de0259c7aaf36aa40cba8014f3fad3edde9307f9, independently built with JIT,
reproduces all186 merged274 groups/11,090 literal protocols. PCRE.NET1.6.0
remains the CI oracle. All matching JIT_COMPLETE calls return0 with positive
JIT sizes; signed reset spans are preserved.

pcre2_compile_class.c:parse_class adds256..max for META_POSIX_NEG. Native
POSIX ASCII tables supply low-byte bitmap members; complete bitmap plus
wide inclusion lowers to ALLANY. Three positive outer ordinary classes:

- `[[:ascii:]\x80-\x{10FFFF}]`: positive ASCII plus high literal coverage.
- `[[:^ascii:]\x00-\x7F]`: negated ASCII plus low literal coverage.
- `[[:ascii:][:^ascii:]]`: POSIX complements, independently of literals.

Each followed by `*?B` misses on tolerant UTF C3A942 fromoffset1 and FF42
fromoffset0; NoDotStarAnchor matches2:1 and1:1 respectively. Reordered,
split/overlapping/bridged ranges and extra union predicates retain the native
result. Holes at low/high endpoints, interior holes and negative outer
classes do not establish universality. The binary-property form
`[\P{ASCII}\x00-\x7F]` retains XCLASS provenance and matches2:1 with either
policy, despite semantic universality. Do not conflate these source kinds.

Mixed POSIX/binary-property universality and case-closed one-sided range
cohorts remain separate. Their valid aligned controls are pinned without
raw malformed/split parity claims. A POSIX complement pair remains universal
under casing and additional literals; its standalone casing protocols are
native-first matching expectations.

## Compiler contract

Modify only PcreByteProgram.cs. Add a fifth independent compile-local4096
combined predicate/range-pass counter, shared across leading paths. A new
IsUtfAllAny overload first invokes the unchanged established four-counter
overload. Return its success immediately, preserving every old path and all
old predicate/member/coverage/binary-ASCII charges before the new proof.
Only after old failure may a positive Source:Class scalar set attempt the
POSIX proof. Already proven classes consume none of the new budget.

Precheck the whole predicate list plus the first complete range pass using
subtraction; an oversized combined list has no partial new charge. Visit
every admitted predicate with cancellation and one charge. Identify only
Kind:Ascii, Category:null, PropertyValue:0; preserve positive/negative flags.
BinaryProperty is not a POSIX ASCII marker. With no marker, decline. With
no ranges, accept only after the complete predicate scan finds both signs.

For nonempty ranges, precheck every complete pass; cancellation and one
charge per member, scanning past early coverage and early complement pairs.
Both POSIX signs accept after one complete range pass, independently of its
members. Positive-only ASCII starts coveredEnd=127 and needs0x10FFFF; reject
any retained range starting below128 after the complete pass. Negative-only
ASCII starts coveredEnd=-1 and needs127; reject any retained range ending
above127 after the complete pass. Advance when Start<=coveredEnd+1. Accept
only complete coverage; stop on no progress or insufficient whole-pass budget.
Empty ranges never enter the repeated-pass loop. These source-domain guards
prevent one-sided proofs from rescuing an exhausted literal-union budget.

The new helper never alters the four established counters. Its exhaustion
is fallback, not syntax/admission failure. No sorting, allocation, node
mutation or runtime state/work change. Keep entry/global-AST/capture31/32,
cuts/calls/targets/NoDot/COMMIT, source ownership, casing, validation and
cancellation priorities. No parser/AST/VM/API/dependency/workflow/public-limit
change. The original four-counter method body remains byte-for-byte intact.

## Native-first corpus and RED prediction

506 matching groups/30,390 protocols,45 diagnostics and174 supplementary
cases add725 tests, reaching101,959. Native preflight checks88 dynamic
groups/264 literal outcomes, plus24 initial exploration groups/96 outcomes.
Pin all three cohorts, property/source controls, modifiers, captures, marks,
references, cuts, caller policies, strict/tolerant UTF and every admitted
offset. Strict malformed-whole-subject and split rows are excluded before
native matching; broader case/mixed-source research is not a matching claim.

Budget cases include4096/4097 combined visits,63 reversed range passes with
127/128 predicates, one-pass pair coverage with4033/4034 predicates, shared
alternatives, predicate-only4096/4097, complete scans after early pair/range
coverage, independence from all four old budgets and preservation of every
old path after new-budget exhaustion. Already proven old classes bypass new
work; existing predicate and binary-ASCII charges remain observable. Pin
one-sided source-domain ownership, no-progress termination, exact observed/
unobserved work limits/cancellation, sliced source ownership, diagnostics,
instruction4090/4091, capture/depth256 and erased/global-AST fallback.

Predict tests-only RED173:119 matching identities and54 supplementary
identities. The latter comprise24 work,6 ownership,3 combined admission,
2 repeated-pass,1 one-pass-pair,3 shared-alternative,1 predicate-only,
4 old-budget-independence,7 old-path-preservation and3 old-proof-bypass cases.
All101,234 prior tests and552 new controls must pass. Reconcile exact failed
identities and freeze all seven new C# Git blobs before production. Native
assertions precede managed comparison; never edit prior test expectations.

## Delivery gate

Review the whole branch, preserve old/frozen identities and pass local
support/whitespace gates. Update this design, its plan and all five shared
inventories before final qualification. Build net8/net9/net10; execute full
101,959 net10/frozen214 cases across120 methods on six native OS/architecture
hosts, with zero failed/skipped tests and compiler/analyzer warnings/errors.
Require configured support/package gates. Record exact source/base/tree/merge
and final qualification externally on PR275; preserve source/docs after GREEN.
Ready readback and identity-checked scratch cleanup complete delivery. Broader
optimizer parity, release and Grep readiness remain separate.
