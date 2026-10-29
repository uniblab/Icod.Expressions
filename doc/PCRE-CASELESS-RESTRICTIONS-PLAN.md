# Internal PCRE local caseless restrictions implementation plan

> **For agentic workers:** Use superpowers:executing-plans for integration, a
> focused fixture worker and fresh read-only whole-branch review.

**Goal:** Implement local r comparison policy and pin related POSIX/property
semantics against native Compiled/JIT behavior.

**Architecture:** Compile-time option snapshots annotate immutable syntax and
instructions; one shared restricted fold handles literals/ranges/references and
assertions. No runtime option stack or extra frame/journal state.

**Tech Stack:** C#13; net8/net9/net10; xUnit; PCRE.NET1.6.0 / PCRE2 10.48 JIT.

**Spec:** `doc/PCRE-CASELESS-RESTRICTIONS-DESIGN.md`.

## Global constraints

- Root project/solution, src source, tests tests; UTF-8 LF; no Python.
- Local Debug; six PR Staging OS/architecture jobs; no local SDK, CI red/green.
- Preserve UTF/UCP/AsciiBsD caller interfaces, byte ASCII folding and Turkish I.
- No opcode/frame/journal/API/version/dependency/consumer/resource policy change.
- Preserve depth256, capture256, instructions4096, saved paths1024, default
  work1,000,000, closure bitmap136KiB and cancellation polling.

## Review focus

1. Both closure and runtime lookup use r; negative/mixed ranges retain literals.
2. Named-reference/deferred-behind reconstruction retains both casing flags.
3. Literal assertion shortcut uses effective r only in UTF when i is active.
4. Reset clears r while preserving U/ASCII masks; all group exits restore it.
5. UCP POSIX lower/upper differ from explicit caseless Unicode properties;
   correct the baseline mismatch independently of r.

### Task 1: Reference contract and inventories

**Files:** Create `tests/Pcre/PcreCaselessRestrictionReferenceTests.cs`; replace
obsolete r-pending rows and four UCP POSIX case expectations in existing fixtures;
update current design/syntax/resource/consumer inventories and historical notes.

**Interfaces:** Existing Compile/Match/options; pinned native Compiled/JIT;
independently expected outcomes/group counts then all capture-byte spans.

- [ ] Cover design verification classes and all five review risks, including
  explicit baseline POSIX/property controls with no r in their patterns.
- [ ] Publish test-only draft PR from merged #84. Observe failures from missing
  r and the existing POSIX widening, verify failure stacks, record exact evidence.

### Task 2: Source-local comparison metadata and restricted folds

**Files:** `src/Pcre/PcreByteParser.cs`, `PcreByteParser.Groups.cs`,
`PcreByteParser.NamedCaptures.cs`, `PcreByteParser.LookbehindResolution.cs`,
`PcreByteSyntax.cs`, `PcreByteProgram.cs`, `PcreByteMatcher.cs`,
`PcreUtf8CaseFold.cs`.

**Interfaces:** bool CaselessRestricted on node/instruction; private parser bool
and GroupOptions snapshot; existing Emit adds optional bool; Fold/Equals and
CloseRanges add optional restricted bool defaultfalse. AppendNode receives the
original atom/group r setting alongside existing casing metadata.

- [ ] Scan/set/unset/reset/snapshot r transactionally; retain source-local flags
  through atom/group annotation, named fixup and deferred behind finalization.
- [ ] Forward instruction metadata at comparing nodes; preserve existing summary
  gates and effective literal assertion optimization eligibility.
- [ ] Use restricted folds consistently for compile-time ranges and runtime UTF
  literals/ranges/backreferences/literal assertions; retain byte behavior.
- [ ] Remove only UnicodeLower/Upper caseless widening; explicit category casing
  and ASCII POSIX alpha behavior stay intact.
- [ ] Whitespace/frozen checks; publish implementation, inspect CI and resolve
  concrete failures with focused red/green evidence.
- [ ] Fresh whole-branch review; fix findings and verify exact final head/tree,
  six successful Staging jobs with full/frozen per-framework counts and gates.
- [ ] Record evidence and mark PR ready; user retains merge control.
