# Internal PCRE ASCII restriction families implementation plan

> **For agentic workers:** Use superpowers:executing-plans for parser integration,
> with a focused fixture worker and a fresh read-only whole-branch review.

**Goal:** Complete aD/aS/aW/aP/aT/bare-a inline restriction semantics.

**Architecture:** Replace the parser's aD bool with five native-compatible bits,
reuse heap group snapshots and select existing immutable class/boundary nodes.
No compiler or VM state is added.

**Tech Stack:** C#13, net8/net9/net10, xUnit, PCRE.NET1.6.0 / PCRE2 10.48 JIT.

**Spec:** `doc/PCRE-ASCII-RESTRICTIONS-DESIGN.md`.

## Global constraints

- Root project/solution; src source, tests tests; UTF-8 LF; no Python.
- Local Debug, six PR Staging OS/architecture jobs; no local SDK, CI red/green.
- Preserve caller AsciiBsD bool interface and UCP-without-UTF rejection.
- No public API/version/dependency, VM architecture/frame/journal or consumer changes.
- Preserve depth256, capture256, compiled instructions4096, saved paths1024,
  default match work1,000,000 and cancellation policies.

## Review focus

1. aP sets two bits; aT and aP unsets overlap correctly in both orders.
2. Bare a leaves non-suffix option letters unconsumed and sets all five bits.
3. aW affects b/B; other restrictions and POSIX word do not alter boundaries.
4. Strict ASCII POSIX/shorthand classes under i must not restrict literal folds.
5. Every group restores masks; prior nodes and later alternatives retain the
   established compile-time scope rules, including malformed subject offsets.

### Task 1: Reference contract and inventory

**Files:** Create `tests/Pcre/PcreAsciiRestrictionReferenceTests.cs`; remove
obsolete pending family rows from `PcreAsciiDigitOptionReferenceTests.cs` and
`PcreUtf8SpaceTests.cs`;
update current syntax/design/resource/consumer inventories and historical notes.

**Interfaces:** Existing Compile/Match/options and native Compiled/JIT oracle;
explicit native outcomes then compare all capture byte spans.

- [ ] Cover all design verification classes and the five review risks, including
  POSIX name sweeps, mask overlap controls, b/B offsets and case-fold controls.
- [ ] Publish test-only draft PR from merged #83. Inspect completed CI logs for
  expected managed rejection while native expectations pass; record exact head.

### Task 2: Parser mask and immutable lowering

**Files:** Modify `src/Pcre/PcreByteParser.cs` and
`src/Pcre/PcreByteParser.Groups.cs` only for production.

**Interfaces:** Private enum AsciiRestriction with None, BackslashD/S/W, Posix,
PosixDigit and All; constructor bool unchanged. GroupOptions retains the enum.

- [ ] Seed mask from caller aD; scan a-suffixes/bare a into local set/unset masks;
  apply after full validation. Reset preserves mask and unset wins per bit.
- [ ] Snapshot/restore complete mask at group boundaries.
- [ ] Lower d/s/w and b/B from their respective bits, POSIX from Posix and
  digit/xdigit additionally from PosixDigit; reuse existing predicates/opcodes.
- [ ] Run whitespace/frozen checks; publish implementation for CI green.
- [ ] Fresh whole-branch review; resolve concrete findings with focused evidence.
- [ ] Inspect exact final head/tree and six successful Staging job logs, complete
  full/frozen counts per framework plus configured workload/consumer gates.
- [ ] Record evidence, mark PR ready; user retains merge control.

### Integration regression: POSIX punctuation bound

**Files:** `tests/Pcre/PcreAsciiRestrictionReferenceTests.cs`,
`tests/Pcre/PcreUtf8PosixClassTests.cs`,
`tests/Pcre/PcreUtf8GeneralCategoryTests.cs`, `src/Pcre/PcreByteMatcher.cs` and the
POSIX/design inventories.

- [ ] Pin native baseline and aT punctuation for £/©/× misses, ASCII dollar and
  Unicode punctuation successes; inner/outer negation, case and literal unions.
- [ ] Observe managed failures in CI before changing the predicate.
- [ ] Change only the UnicodePunctuation supplemental-symbol threshold from
  0x100 to0x80, matching all three pinned implementation paths.
- [ ] Run final full matrix and complete the existing review/readiness gate.

This bounded correction follows verified native execution when the pinned
manual prose conflicts; no additional user choice is needed under the selected
reference contract.
