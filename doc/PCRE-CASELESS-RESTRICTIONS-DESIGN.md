# Internal PCRE local caseless restrictions

## Intent and scope

Continue issue #7 after #84 with inline r (CASELESS_RESTRICT), including leading,
local, scoped, mixed, set/unset and reset headers. Keep PCRE.NET1.6.0 / PCRE2 10.48
Compiled/JIT as the oracle and existing ASCII option/UCP gates intact.

## Approach

Select immutable comparison metadata on syntax nodes and emitted instructions.
The parser snapshots r with existing group options; compilation forwards the
local setting; shared UTF fold helpers enforce it. Runtime option stacks would
duplicate source-local lowering and complicate backtracking. A separate matcher
would duplicate the engine. This choice follows the standing autonomous remit.

## Semantics

- r is independent of i: store it while i is off and apply it when a comparison
  is caseless. Repeated flags are allowed; unset wins within a header. Reset ^
  clears r along with implemented Perl flags, while preserving U and ASCII masks.
  Group exits restore r; changes propagate to later alternatives, not prior nodes.
- In UTF mode (with or without UCP), r blocks ASCII/non-ASCII caseless crossings.
  Default pinned Unicode has only two such equivalence groups: S/s/ſ and K/k/K.
  Ordinary ASCII case pairs and non-ASCII pairs (É/é, Σ/σ/ς, titlecase) remain.
  Byte comparisons keep existing ASCII folding. Turkish I policy is unchanged.
- Apply the local setting to literals, literal/range bracket members (including
  negations/unions), numbered/named/forward references and literal/general
  lookarounds. Capture spans remain original subject bytes, even when unmodified
  caseless comparisons consume different byte widths. r does not constrain
  explicit Unicode property/category predicates, POSIX substitutions or word
  classes; those keep their separately defined semantics and ASCII restrictions.
- Optimized literal assertions require matching effective comparison policy:
  compare r in UTF when i is on; in byte mode or with i off r is inert. Otherwise lower the existing
  general assertion body so each literal retains its own policy.

## Implementation

Add private parser caselessRestricted state and GroupOptions snapshot. Extend the
transactional option scanner with set/unset r and reset clearing. Annotate nodes
at their original source locations, including group completions before outer
quantifiers. Add CaselessRestricted to PcreByteNode and PcreByteInstruction, and
forward it through Emit at comparison sites. Preserve both casing flags during
named-reference and deferred-behind reconstruction; record-with rewrites already
preserve inherited metadata. Keep HasCaselessComparisons conservative as before.

Add optional restricted bool arguments to PcreUtf8CaseFold.Equals/Fold/CloseRanges.
Compute the existing canonical fold; under r, a non-ASCII scalar whose key is
ASCII keeps its original scalar as key. The pinned two groups have exactly one
non-ASCII member each, so this partitions them correctly without widening bitmap
keys beyond0x10FFFF or duplicating range storage. Use the same fold in compile-time
range closure and runtime range lookup, references and literal assertions.
Retain the existing bounded136KiB closure bitmap and cancellation polling.

## Baseline POSIX distinction

Native probes also expose a preexisting managed-only fixture mismatch: UCP POSIX
lower/upper remain Ll/Lu under i, while explicit p{Ll/Lu/Lt} properties become
cased-letter predicates. ASCII/non-UCP/aP lower/upper still use caseless ASCII
alpha. Remove only the UnicodeLower/Upper caseless-widening arm in
ScalarClassContains, retaining explicit SpecificCategory casing. Pin native
baseline/ir, negation and property controls before this correction and update
four older incorrect expectations. This is independent of r and is documented
rather than implemented as an r-dependent workaround.

## Verification and boundaries

Native explicit outcomes and all capture-byte parity: both crossing directions,
local/reset/caller i modes, ranges/negation/unions/wide bounds, Unicode/property
controls, named-reference fixups, literal assertion fast paths and general nested
assertions, captures/rollback/repeats, branch propagation, UTF/UCP/non-UCP and byte
modes. Sweep selected malformed UTF/CRLF caller offsets. Pin header diagnostics,
original byte offsets, depth256/capture256, work/cancellation and wide closure
cancellation. No new opcode, runtime frame, journal copy, spans in saved state,
resource policy, API/version/dependency or consumer changes. Unicode data parity
remains provisional. Grep migration and CommandFramework removal remain gated.

No local SDK: executable red/green uses existing CI. Final six Staging platforms,
net8/net9/net10 full plus214 frozen tests and configured workload/API/consumer
gates must pass. Local whitespace/frozen checks; UTF-8 LF; no Python.

## Pinned sources

pcre2api.3 2169–2178 and pcre2_ucd.c117–150: the two default crossing groups;
pcre2_compile.c3060/5042/5112/5840: tracking/reset/scanning/restoration;
pcre2_compile_class.c187–198: restricted class closure; compile.c4105 versus4540:
POSIX versus explicit-property casing. Native probes check nonzero JIT size.
