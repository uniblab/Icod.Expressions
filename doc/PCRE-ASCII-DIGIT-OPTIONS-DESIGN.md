# Internal PCRE scoped ASCII digit options

**Historical checkpoint:** This document records its original tranche. Current
ASCII option support is in `PCRE-ASCII-RESTRICTIONS-DESIGN.md` and the current
`PCRE-SYNTAX-ACCEPTANCE-MATRIX.md`.

## Intent

Continue issue #7 after #82 by bringing the existing aD restriction into the
ordinary compile-time option grammar. Grep's UTF/UCP default ASCII backslash-d
policy remains intact while patterns can change it locally, combine it with
implemented flags and restore it at group boundaries.

## Scope and approaches

Select the existing transactional option scanner and group snapshots. Extending
the prefix-only special case would duplicate header rules and cannot express
local scope reliably. Runtime option instructions would duplicate immediate
class lowering and require unnecessary matcher state. This choice follows the
standing autonomous implementation remit.

Accept exact `aD` tokens in leading/mid-pattern/scoped headers, set/unset sections,
repeated and combined headers, and reset-plus-letter forms with existing
imnsUx/xx. Both byte mode and UTF mode accept the spelling. Without UCP, aD is
semantically inert; UCP without UTF remains explicitly unsupported. Do not add
bare a, aS/aW/aP/aT, J/r, directives or other syntax families.

## Semantics

- aD restricts only backslash-d/D, including bracket members, under UCP. Unsetting
  it restores Unicode decimal-digit selection under UTF/UCP. Explicit Nd
  properties, POSIX digit classes and word/space classes remain independent.
- Repeated tokens are allowed; an unset token wins over a set token in the same
  header. aD is adjacent and case-sensitive, even under x/i. Unsupported or
  malformed headers cannot partially mutate state. No trivia splits a token.
- Reset preserves aD as implemented in #82; explicit `(?^aD)` enables it. Caret
  remains first and forbids hyphens. Existing x/xx rules remain unchanged.
- Changes apply from their source position, propagate to later alternatives in
  the same group and restore to entry state on every group exit. This includes
  ordinary/noncapturing/atomic/branch-reset groups and assertions, regardless
  of capture suppression or whether a branch matches. Already parsed nodes do
  not change. Bare headers consume no capture or nesting slot.
- Remove the prefix-only aD bypass. All valid standalone headers share existing
  post-header trivia and nonrepeatability diagnostics. Scoped bodies may repeat.
  Diagnostics retain original source byte offsets.

## Implementation

Extend `TryParseOptions(int openingOffset, bool scoped)` in
`src/Pcre/PcreByteParser.cs` with local set/unset aD booleans. Consume D only after
recognizing a; reject other a families. Apply `(asciiBsD || set) && !unset` only
after validating the entire header. Simplify the leading loop to use this reader.
Add AsciiBsD to `GroupOptions`, `CurrentGroupOptions` and restoration in
`src/Pcre/PcreByteParser.Groups.cs`. Existing d/D scalar/class nodes already
capture the correct policy at parse time; compiler, bytecode and VM stay intact.

## Verification

Compare explicit expected match/capture outcomes and all capture byte spans with
PCRE.NET 1.6.0 / PCRE2 10.48 Compiled/JIT. Cover all positions, caller policies,
combined/repeated/unset/reset tokens, alternative propagation, nested restoration,
prior/later nodes, captures/references, assertions and quantifier boundaries.
Include d/D in mixed/negated brackets, independent POSIX/property/word/space
controls, ASCII and multi-byte decimal digits, byte/non-UCP controls, malformed
UTF and CRLF caller offsets. Pin unsupported families, split/case-invalid tokens,
repetition, unclosed groups and original byte diagnostics. Preserve depth256,
capture256, instruction/work/cancellation policies with focused boundary tests.

Use test-first CI because no local SDK is available. Required final checks: all
six Staging platform/architecture jobs, net8/net9/net10, 214 frozen tests per
framework and configured extraction/API/package-only consumer gates. Local
whitespace/frozen-source checks remain required. UTF-8 LF; no Python.

No public API/version/dependency, matcher-frame/journal or consumer changes.
Broader Unicode/performance/release and Grep migration gates remain open;
CommandFramework removal still waits for all known consumer migrations.

## Pinned reference

Existing PCRE.NET 1.6.0 checkout: `src/PCRE/src/pcre2_compile.c`, option scanner
5030–5183 and UCP d/D escape lowering; `src/PCRE/doc/pcre2pattern.3`, internal
option setting 1835–1875. aD uses a separate extra-option mask; reset leaves that
mask intact except for r, which is not implemented here.
