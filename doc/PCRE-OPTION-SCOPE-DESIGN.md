# Internal PCRE i/m/s option scope

## Goal and semantics

Advance issue #7 with mid-pattern `(?ims-ims)` and noncapturing
`(?ims-ims:...)` groups for the already implemented i/m/s flags. Preserve the
leading option groups, caller caseless setting and leading-only UTF aD toggles.
Use the [PCRE2 option rules](https://www.pcre.org/current/doc/html/pcre2pattern.html)
and pinned PCRE.NET 1.6.0 / PCRE2 10.48 fixtures.

Options are compilation settings. A change applies to following atoms and
later alternatives within its current group, including branches abandoned at
match time. It does not retroactively affect earlier atoms. Every ordinary,
named, noncapturing, branch-reset, atomic or scoped group restores the entry
i/m/s settings on exit. A scoped group first changes its body's settings and
then restores its caller's state. Setting and unsetting a flag together leaves
it unset. Name identity remains ordinal and case sensitive.

Multiline and DotAll continue to select existing anchor/dot nodes while parsing.
Caseless must be retained on individual comparison atoms: literals, byte/UTF
classes, properties, fixed literal assertions and numeric/named references.
Case sensitivity at the reference controls comparison, independently of the
mode in which its capture was produced. Results retain authoritative byte spans.

## Approaches

1. Runtime push/pop option instructions and frame state: unnecessary for static
   PCRE2 scope, adds work and rollback rules.
2. Clone matcher/programs per option region: duplicates the engine and makes
   captures, boundaries and resource accounting harder to preserve.
3. **Choose immutable atom/instruction metadata.** Record Caseless on parsed
   nodes at their source location. Carry it through named-reference resolution
   and into comparison instructions. Matcher comparisons read the instruction's
   flag. No option opcode or backtracking-frame field is required.

Save/restore parser flags around group bodies and leave them in effect across
direct alternatives. Skip standalone option settings as syntax, not matchable
nodes. Parse scoped settings through the same reader as leading/unscoped ones.
Stamp atoms before wrapping quantifiers; child atoms retain their own settings.
Byte and UTF bracket construction uses the active parser settings as before.

The program-level casing summary only gates existing search shortcuts: it is
true when any comparison instruction is caseless. Disable the old exact-case
shortcuts conservatively for mixed programs. Keep suffix-shape, empty-path and
explicit-newline analysis over existing nodes. Work budgets and instruction
order remain unchanged for already supported leading-option patterns.

## Boundaries and gates

Only i/m/s is added here. Empty option groups, circumflex resets, n/x/xx/J/U,
mixed/scoped aD and other extra flags remain pending. General assertions,
including option changes inside their bodies, remain pending; a scope around
an existing fixed literal assertion is supported. Unsupported flags retain
controlled diagnostics. Direct repetition of standalone settings is invalid.

Keep internal APIs, 256 nesting/capture limits, compile-expansion limits,
cancellation, match work, saved paths and invalid-UTF barriers. No public API,
version, runtime dependency, Grep or CommandFramework changes. No subagents.

Pin nested scope restoration, alternative propagation, captures/references,
UTF folds/properties/classes, assertions, dot/anchors, atomic and repeated
groups, quoting, caller options, invalid UTF/CRLF offsets, depth and work. Require
test-first evidence and six green Staging jobs with net8/net9/net10, frozen,
workload and package gates before review readiness. Release/migration gates stay open.
