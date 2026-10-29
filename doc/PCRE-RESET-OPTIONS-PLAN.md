# Internal PCRE empty and reset options implementation plan

> **For agentic workers:** Use superpowers:executing-plans; root implements the
> parser and a focused fixture worker writes tests. A fresh reviewer checks the
> whole branch before readiness.

**Goal:** Accept empty/reset inline option groups with pinned PCRE2 semantics.

**Architecture:** Extend the transactional compile-time option scanner; reuse
existing immutable node comparison flags and heap group snapshots. Reset clears
only implemented Perl flags, preserving U and ASCII restriction state.

**Tech Stack:** C# 13, net8/net9/net10, xUnit, PCRE.NET 1.6.0 / PCRE2 10.48 JIT.

**Spec:** `doc/PCRE-RESET-OPTIONS-DESIGN.md`.

## Global constraints

- Root project/solution, source in src, tests in tests; UTF-8 LF; no Python.
- Local Debug; six PR Staging jobs across Linux/Windows/macOS and x64/ARM64.
- No local SDK: executable red/green checks use existing CI.
- No public API/version/dependency, compiler/VM or consumer migration changes.
- Preserve heap traversal, integer frame indexes, shared capture journal and all
  existing depth/capture/instruction/saved-path/work/cancellation limits.

## Review focus

1. Reset preserves U and aD even while clearing caller casing and n.
2. Reset x changes trivia handling at the exact header end, including scopes.
3. Empty/minus-only headers create no capture or depth; scoped forms do nest.
4. Later alternatives inherit compile-time changes; enclosing scope restores.
5. Header errors cannot partially mutate state, and byte offsets stay original.

### Task 1: Pin empty/reset grammar and semantic contract

**Files:** Create `tests/Pcre/PcreResetOptionReferenceTests.cs`; modify obsolete
pending cases in `tests/Pcre/PcreOptionScopeReferenceTests.cs`.

**Interfaces:** Existing `PcreByteCompiler.Compile`, `PcreByteOptions` and
`PcreRegex8Bit` Compiled/JIT settings. No production interfaces added.

- [ ] Add explicit native outcomes/group counts and complete capture comparisons
  covering every design verification class and the five review risks.
- [ ] Commit tests/design/plan and publish a draft PR from merged #81.
- [ ] Inspect the first completed CI test log: native expectations pass and new
  managed compile acceptance fails. Preserve exact run/head evidence.

### Task 2: Extend transactional option scanning

**Files:** Modify `src/Pcre/PcreByteParser.cs`, method
`private bool TryParseOptions(int openingOffset, bool scoped)`.

**Interfaces:** Same signature and caller contract; successful headers advance
index and apply flags, failed scans leave state untouched.

- [ ] Accept three-byte `(?)`, zero-letter/empty-unset sections, and a leading
  reset marker; forbid a hyphen after reset or a second hyphen.
- [ ] Clear only caseless/multiline/dotAll/noAutoCapture/extended after a complete
  validated reset, then apply existing explicit option updates. Preserve U/aD.
- [ ] Keep repetition diagnostics and post-header trivia behavior unchanged.
- [ ] Update syntax/design/resource/consumer inventories for this exact subset.
- [ ] Run whitespace/frozen checks; publish the implementation and inspect CI.
- [ ] Obtain a fresh whole-branch read-only review; fix concrete findings with
  focused regressions and repeat affected checks.
- [ ] Verify exact final head, all six successful Staging jobs and full/frozen
  counts per framework, plus existing consumer gates; update PR evidence and
  mark ready. User retains merge control.
