# Internal PCRE brace grammar implementation plan

> **For agentic workers:** Use superpowers:executing-plans for integration,
> a focused fixture worker and one fresh read-only whole-branch review.

**Goal:** Recognize counted quantifiers transactionally and allow ordinary literal braces.

**Architecture:** Syntax-only shared classifier before bounded numeric interpretation;
ordinary literal nodes and existing repetition/compiler/VM paths remain shared.

**Tech Stack:** C#13; net8/net9/net10; xUnit; PCRE.NET1.6.0 / PCRE2 10.48 JIT.

**Spec:** `doc/PCRE-BRACE-GRAMMAR-DESIGN.md`.

## Global constraints

- Root solution/project, src, tests, samples; UTF-8 LF; no Python.
- Local Debug; six-platform PR Staging; no local SDK, executable CI red/green.
- Preserve depth256, capture256, instructions4096, saved paths1024, work1,000,000.
- No new node/opcode/VM state, public API/version/dependency/release/consumer change.
- Existing explicit braced escape/name/property/reference grammars stay strict.

## Review focus

1. Incomplete/malformed forms with huge digit runs remain literal; valid-shaped
   overflow/reversed counts still fail before repeatability or erasure.
2. Every ordinary repeat boundary uses the same classifier, including trailing
   repeats, anchors, bare options and condition assertion completion.
3. A failed brace shape consumes only its literal opening byte; subsequent
   comments/quotes/groups/metacharacters retain their own source grammar.
4. Explicit braced escapes, class members and references do not gain fallback;
   N still rejects malformed braces while permitting real counts/codepoints.
5. Recognition is index-transactional, cancellation-polled and bounded linear;
   UTF byte diagnostics and existing expansion/work limits remain meaningful.

### Task 1: Native contract, fixture and inventory

**Files:** Create `tests/Pcre/PcreBraceGrammarReferenceTests.cs`; update obsolete
brace diagnostics in existing tests and current/historical design, syntax,
resource and consumer inventories.

**Interfaces:** Existing internal Compile/Match/options and pinned native JIT.

- [ ] Native builder check and explicit match/rejection probes for all review risks.
- [ ] Reference fixture with explicit outcomes/groups plus every numbered span,
  source byte diagnostics, contexts and resource/cancellation cases.
- [ ] Publish test/docs-only draft after merged #90. Observe missing-feature red
  with native assertions passing before production edits; record exact counts.

### Task 2: Shared recognition and parser integration

**Files:** Create `src/Pcre/PcreByteParser.Repeats.cs`; modify
`src/Pcre/PcreByteParser.cs` and `src/Pcre/PcreByteParser.Groups.cs`.

**Interfaces:** IsCountedQuantifier(int offset) returns complete grammar shape
without index mutation; existing ParseCountedBounds(int offset), ReadCount and
SkipBraceHorizontalSpace move to the focused partial. No compiler/VM interface change.

- [ ] Implement cancellation-polled local-cursor syntax scan; validate shape before
  numeric parsing and preserve the private non-counted Unsupported contract.
- [ ] Classify every ordinary brace repeat boundary and standalone brace atom;
  preserve literal character-by-character lexing and strict braced token readers.
- [ ] Whitespace/frozen35, exact-tree publish and full CI; investigate/fix concrete
  failures with focused red/green evidence.
- [ ] Fresh whole-branch review; resolve findings, inspect exact final head/tree,
  all six green logs/full/frozen/configured gates; ready PR for user merge.
