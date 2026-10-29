# Internal PCRE general fixed-length lookbehind

Historical tranche boundary: exact atomic-group and atomic-lookaround aliases now
share these existing semantic paths; see `PCRE-GROUP-ALIASES-DESIGN.md`. Earlier
alias-pending notes below describe the original tranche.

## Goal and scope

Extend atomic positive/negative lookbehind beyond literal bodies. Support classes,
dot and non-newline atoms, captures/names, groups, exact consuming repeats, local
options/comments, atomic groups and nested supported assertions. Every direct
lookbehind branch must have one statically known character length; different
top-level branches may have different lengths. Nested consuming alternatives must
have equal lengths. Direct assertion repeats retain #78's finite normalization.

Variable-length consuming bodies, consumed backreferences and consuming `\R`
remain controlled unsupported syntax at the lookbehind opening. References and
variable-length atoms within nested lookahead do not consume outer length and
may use the already supported grammar. General aliases/conditions stay pending.
The oracle is PCRE.NET 1.6.0 / PCRE2 10.48 Compiled/JIT. Its pinned compiler and
regression corpus support bounded-variable `\R` behind despite older prose saying
otherwise; this tranche rejects it because it has more than one possible length.

## Approach

Use the heap parser for both assertion directions. On closing a nonliteral
lookbehind, compute fixed branch lengths with an explicit heap traversal and
reference-keyed node widths. This validation occurs even when an enclosing
quantifier emits zero body copies. Scalar-consuming nodes count one character;
anchors/assertions count zero; sequence adds; nested alternatives must agree;
exact counts multiply. Bounded repeats of zero-width children retain length zero. Unbounded ordinary
zero-width groups remain unsupported, matching the pinned length analyzer; direct
assertion quantifiers have already been normalized to finite bounds. Preserve
their source-unbounded flag: the pinned native length analyzer rejects unbounded
direct nested lookbehind, while ignoring quantifiers on nested lookahead.
Analyze children even under `{0}`; erasing an unbounded group must not bypass
validation. Direct unsupported body escapes retain the legacy behind opening
diagnostic; errors in newly supported nested grammar retain normal source offsets.
New general branch lengths are capped at 65,535 characters with bounded arithmetic.
Unsupported or variable length produces an opening-byte diagnostic. Preserve the
existing literal assertion opcode and its ordinary work/search behavior when
all body nodes remain compatible literals.

Compile general lookbehind with the existing assertion enter/exit guards and
branch compiler, emitting a Reverse instruction at each top-level branch start.
Each retry starts at the original assertion position, rewinds that branch's
character count, then matches forward. UTF rewinds scalar boundaries in the
current valid fragment; byte mode rewinds bytes. Charge scanning work and observe
cancellation. Do not slice the subject or shorten its end: nested lookahead can
inspect and capture beyond the original assertion endpoint. Shared guard exit
restores the original position and atomically retains positive captures; negative
paths and outer retries use the existing capture journal.

Nullable candidate metadata must permit negative and empty assertions in both
directions at requested malformed UTF offsets and later invalid-run candidates.
Reverse never crosses a malformed fragment barrier. Original caller `\G` remains
unchanged and `\K` remains invalid throughout assertion nesting.

## Alternatives and decision

A second recursive matcher would duplicate capture transactions, limits and
width semantics. Subject slicing would change anchors, nested assertions and
capture coordinates. A full bounded-variable reverse search requires additional
length ranges and endpoint checks. The maintainer agent chooses fixed-length
analysis plus the shared VM now; variable-length semantics remain a later tranche.
This is an implementation decision under the user's standing authorization.

## Constraints

Internal managed C# 13, net8/net9/net10; no native or Regex fallback. Root project
and solution, `/src`, `/tests`, `/samples`, UTF-8 LF. No Python. Runtime frames
retain integer positions/checkpoints and compact enums; no stored spans or
interior pointers, per-backtrack capture copies or duplicated width engines.
Existing parser depth/capture/repeat ceilings, 4,096 instruction/node visits,
1,024 saved paths/guards, work and cancellation contracts remain. Local Debug;
six OS/architecture Staging jobs in CI. Public API, version, dependencies, Grep
migration and CommandFramework removal remain unchanged and separately gated.

## Acceptance

Pin explicit native outcomes and all capture spans before managed comparison.
Cover ordered branch lengths, insufficient prefix, positive atomicity, negative
rollback, captures before G0 and beyond the endpoint, local options/comments,
names/branch reset/references after the assertion, nested assertions, direct
quantifiers, UTF scalar and caseless byte-width differences, caller offsets and
malformed/CRLF sweeps. Verify controlled unsupported lengths and original byte
diagnostics, heap depth, resource ceilings, work/cancellation, frozen source,
full six-platform tests and existing workload/extraction/API/package gates.
