# Internal PCRE option scope implementation plan

**Goal:** Compile scoped/mid-pattern i/m/s with PCRE2 inheritance/restoration.

**Architecture:** Parser option scopes and immutable comparison metadata;
existing nodes, VM instructions and integer/checkpoint backtracking frames.

**Tech stack:** C# 13; net8.0/net9.0/net10.0; pinned test-only PCRE.NET 1.6.0.

**Spec:** [PCRE-OPTION-SCOPE-DESIGN.md](PCRE-OPTION-SCOPE-DESIGN.md).

## Constraints and review focus

- Root solution/project, `/src`, `/tests`, `/samples`; local Debug, CI Staging.
- No public API/version/dependency, consumer or backtracking-state changes.
- No subagents; implement and review the complete diff directly.
- A late flag must not change earlier atoms or escape its enclosing group.
- A flag in one branch propagates to later branches even when that branch fails.
- Numeric/named references use their own mode, preserving captures' source bytes.
- UTF folded ranges/properties and fixed assertions must use the local setting.
- Any caseless comparison must disable incompatible whole-program search shortcuts.

## Task 1: Pin scope behavior before implementation

Create `tests/Pcre/PcreOptionScopeReferenceTests.cs` and these notes. Compare
whole and all capture spans with pinned PCRE2, including caller options,
restoration/propagation, byte/UTF comparisons, anchors/dot, quantifiers/atomic
fallback, quoting, invalid-UTF/CRLF offsets and pending syntax. Pin supported
depth and matching work/cancellation. Open a draft and inspect expected CI
failures before implementation; no local .NET SDK is available.

## Task 2: Parser scope and instruction casing

Modify `PcreByteParser.cs`: generalize the i/m/s reader to accept the specified
opening position and scoped/unscoped delimiter, consume standalone settings in
sequences, recognize scoped groups, stamp each atom with its source Caseless
setting before quantifiers, restore entry i/m/s after every group body. Keep
leading aD and unsupported letters separate. Add Caseless init metadata to
`PcreByteNode` in `PcreByteSyntax.cs`; preserve it when resolving named references
in `PcreByteParser.NamedCaptures.cs`.

Modify `PcreByteProgram.cs`: add a Caseless field to comparison instructions,
set it during emission, and derive the whole-program shortcut summary from
emitted flags. Remove the parser-final caseless argument in `PcreByteCompiler.cs`.
Modify `PcreByteMatcher.cs`: semantic comparisons read instruction.Caseless;
the shortcut gate still reads the program summary. Add no option instruction
or frame state. Preserve numbered captures, visits and work steps.

Replace obsolete pending rows in leading-option, noncapturing, syntax and Grep
fixture tests. Update the current design, acceptance and consumer inventories.

## Task 3: Review and verification

Run whitespace and frozen-source checks; review all casing reads, scope exits,
named fixups, alternative propagation and shortcut gates. Require six green
Staging jobs on the exact head, full framework suites and package/workload
checks. Record red/green evidence in the PR and mark ready for the user's merge.
