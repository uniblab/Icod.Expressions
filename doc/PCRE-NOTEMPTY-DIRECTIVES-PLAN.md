# Nonempty-match directives implementation plan

> **For agentic workers:** Use superpowers:executing-plans inline. No subagents, per user instruction.

**Goal:** Implement NOTEMPTY/NOTEMPTY_ATSTART through final acceptance and shared rollback.
**Architecture:** Two immutable start/program flags; Accept rejects empty reported spans under the native rules. Flagged tolerant empty programs take the existing VM candidate path.
**Tech Stack:** C#13, net8/net9/net10, xUnit, PCRE.NET1.6.0 / PCRE2 10.48 Compiled/JIT.
**Spec:** `doc/PCRE-NOTEMPTY-DIRECTIVES-DESIGN.md`

## Global constraints

- Exact uninterrupted original-byte0 prefix; flags accumulate, survive resets; no new caller options.
- Whole-match empty reported span only, K-aware and original caller offset; shared failed state/journal.
- Preserve nullable AST/fragment metadata, validation precedence, source positions, widths and optimizer policy.
- No AST/opcode/frame/journal/new limits/fallback/public API/package/dependency/workflow/release/consumer changes, Python or subagents.
- Local Debug; six native Staging jobs; no local SDK, managed RED/GREEN via CI. Grep last; CommandFramework removal after publication/all-consumer migration. User merges.

## Review focus

1. Empty reported match after K differs from consumed length; ATSTART compares the original caller offset, not the current candidate or fragment start.
2. Whole-match rejection retries alternatives/repeats and rolls captures back; assertion/call empty success and atomic commitment remain ordinary.
3. Tolerant empty-root shortcut must not bypass flags; retain nullable invalid-run/gap/context candidates and CRLF/UTF advancement.
4. Prefix settings accumulate in either order/interleaving and survive inline reset; grammar and whole-source UTF diagnostics retain original offsets.
5. Rejection stays budgeted/cancellable, no new saved state; unflagged and nonempty behavior retains existing work protocols.

## Task1: Native contract and failing fixtures

**Files:** Create `tests/Pcre/PcreNotEmptyDirectiveReferenceTests.cs`, design/plan; update PCRE design/matrix/resource/consumer inventories.
**Interfaces:** Existing Compile/Match and pinned native reference APIs only.

- [ ] Write native-first literal all-capture expectations and all-offset byte/tolerant UTF × LF/ANYCRLF matrices; pin five review-focus classes including grammar, strict errors and resource protocols.
- [ ] Run bounded native checker, design/plan self-check, frozen35/whitespace. Publish tests-only draft and observe successful compilation plus expected feature failures before source edits.

## Task2: Settings and final-accept guard

**Files:** `src/Pcre/PcreByteStartSettings.cs`, `PcreByteCompiler.cs`, `PcreByteProgram.cs`, `PcreByteMatcher.cs`.
**Interfaces:** Add bool NotEmpty/NotEmptyAtStart to immutable settings/program; pass through existing program-compiler constructor. Compile/Match signatures unchanged.

- [ ] Scan exact new tokens, set independent flags and pass them into readonly program metadata. No parser/AST/frame mutation.
- [ ] Restrict tolerant empty-program shortcut to unflagged programs. In Accept, when position==captureStarts[0] and (NotEmpty || NotEmptyAtStart && position==startOffset), set failed=true and break; otherwise retain existing acceptance. Existing failed-state machinery retries and restores captures.
- [ ] Require all six Staging full/frozen214 suites on three frameworks, zero failed/skipped/build warnings/errors, configured gates/seven Linux equalities. Separate whole-branch author self-review; material fixes RED→GREEN; exact head/tree/clean/frozen35/whitespace proof. Update proof/mark ready; do not merge.

Self-check: Task1 uses existing APIs; Task2 carries two immutable flags and changes only final acceptance plus one bypass. Every review-focus class has Task1 fixtures. No interface conflict; preserve evidence.
