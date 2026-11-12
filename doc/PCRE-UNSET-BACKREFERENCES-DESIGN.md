# Unset backreference compile option

Continue after merged PR232 with internal default-false MatchUnsetBackreference
(PCRE2_MATCH_UNSET_BACKREF,0x200). A reference to an existing nonparticipating
capture succeeds without consumption. It does not create a capture or change
participation, so capture conditions remain false. Existing empty captures still
participate. Duplicate names still select the first participating declaration,
without comparison fallback; only all-unset candidates gain the empty path.
Missing/invalid references remain compile errors with existing diagnostic priority.

## Approach

My choice under the approved inline roadmap is a selected semantic annotation
pass after global numeric/name validation and before call/width/final-target
resolution. Reuse RewriteTree to mark all existing Backreference nodes with one
default-false UnsetMatchesEmpty init property, preserving source scope/casing
and all numeric/named aliases. Defaults avoid the pass and keep the same root.
Lower the property into the unused Backreference Second operand.

The matcher keeps declaration-order participation lookup, then advances pc for
an unset capture only when Second=1. No capture/journal mutation or subject read
is needed; existing consuming and empty participating comparisons remain shared.
A program-global runtime flag would duplicate policy outside the typed node;
separate opcodes or synthetic captures would duplicate guards or alter conditions.
Those alternatives are rejected.

Native width analysis rejects consumed backreferences under this option, even
when the group has a known zero/fixed width or the reference is repeated zero
times. Add that check at existing ReferenceTarget, before ordinary dependency
validation. Existing width traversal handles called bodies and nested assertions;
lookahead, DEFINE and content after FAIL retain their existing skipping rules.
Preserve the established global syntax/reference validation order and byte
diagnostics. No grammar, source rewrite, new AST node/opcode or runtime mode.

Selected nullable references also make some line-anchored bodies empty. Preserve
native STARTLINE search after a split ANY UTF newline by permitting the existing
line-start entry proof when a bounded zero-consumption path through selected
backreferences is established. Inspect only pure capture/group/repeat/anchor and
consuming instructions; reject calls, assertions, conditions and verbs outside
that proof. Require a selected reference and an empty accepting path. Existing
first-code-unit priority stays authoritative for a mandatory leading literal.
Defaults retain the original proof. Nonnullable bodies whose first consuming
instruction is behind an optional capture/reference remain in wider optimizer
work (#102). This adds compiler metadata only, reusing the matcher candidate exclusion and physical-EOF rules.

## Evidence and review boundaries

Pin native PCRE2 10.48/JIT literals for optional/forward/self/relative/named/
duplicate/branch-reset references, participating versus unset slots, conditions,
greedy/lazy/possessive nullable repeats, assertions/calls/recursion/returned
captures, cuts/ACCEPT/K, UTF casing and independent quoted/bracket escapes.
Pin selected consumed-lookbehind rejections and unselected native controls.
Supplement all octets, malformed-UTF/CRLF offsets, old caller policies, full
capture capacity, owned source/options/labels, exact legacy diagnostics, zero-
width charged work, cancellation and quotas. Native loop arrays compile first.
Require valid clean tests-only RED before freezing, then all18 native results.

Five boundaries: unset versus participating/empty duplicate captures; numeric/
named/forward/self aliases and missing references; rollback/calls/repeats and
conditions; consumed lookbehind versus nonconsuming/skipped contexts; UTF offset,
ownership and shared work/cancellation/resource bounds.

Preserve earlier tests,35 frozen blobs and214 frozen outcomes/120 methods,
GNU/BRE/ERE, API/dependencies. C#13/net8/9/10,UTF-8+LF; no Python, subagents or
SDK installs. User merges/tags/publishes; Grep remains last. Other modes/syntax,
optimizer/resources, UTF16/API and realistic consumer/release gates remain open.
