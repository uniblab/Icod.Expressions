# Internal PCRE numeric escapes implementation plan

**Goal:** Resolve decimal references and octal literals with pinned PCRE2 rules.

**Architecture:** Parser-only disambiguation and radix-aware braced literals;
existing numbered capture/literal nodes and VM.

**Tech stack:** C# 13, net8.0/net9.0/net10.0; test-only PCRE.NET 1.6.0.

**Spec:** [PCRE-NUMERIC-ESCAPES-DESIGN.md](PCRE-NUMERIC-ESCAPES-DESIGN.md).

## Constraints and review focus

- Root solution/project, `/src`, `/tests`, `/samples`; Debug locally, Staging CI.
- No consumers, public API, package version, matcher or instruction changes.
- No subagents; author reviews the complete diff.
- Read the whole decimal run; do not split it into a shorter reference plus digits.
- Use the current branch-reset counter, and preserve leading-8/9 forward references.
- Saturate decimal scans safely; octal range checks must precede arithmetic overflow.
- Brackets never create backreferences; assertions accept only resolved literals.
- Preserve source-byte diagnostics and the differing brace-space rules for N/U+.

## Task 1: Reference fixtures before implementation

Create `tests/Pcre/PcreNumericEscapeReferenceTests.cs` and these notes. Compare
whole/capture spans with pinned PCRE2 for the spec's forms. Sweep all bytes;
include UTF scalar bounds, branch-reset disambiguation, 10/80/90/256 references,
literal contexts, invalid forms, long scans, invalid-UTF offsets and resource
behavior. Open a draft and inspect expected CI failures. No local SDK is present.

## Task 2: Numeric parsing

Create `src/Pcre/PcreByteParser.NumericEscapes.cs` with decimal/octal selection,
at-most-three-digit octal and braced-octal helpers. Modify `ParseEscape` to route
numeric digits and o, and remove the numeric guards in byte/UTF bracket readers.
Add a radix-aware braced character-value helper in `PcreByteParser.cs`, keeping
existing hex/N callers and diagnostics. Allow brace-edge space/tab in numeric g.
Resolve references to existing `PcreByteBackreference`, literals to
`PcreByteLiteral`; preserve cancellation and capture/context limits.

Replace obsolete pending numeric rows in backreference, syntax acceptance,
lookaround and Grep fixture tests; retain still-pending g subroutine cases.
Update the overall design, acceptance matrix and consumer fixture inventory.

## Task 3: Verification and review

Run whitespace/frozen-source checks; review the full parser diff against the
spec and failure modes. Require six successful Staging jobs on the exact PR
head, full framework suites and existing package/workload checks. Record
red/green evidence and mark ready. The user reviews and merges main.
