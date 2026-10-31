# Byte UCP Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: superpowers:executing-plans inline. No subagents by user instruction.

**Goal:** Correct UCP byte classification/boundaries and caseless comparisons.
**Architecture:** Existing256-bit projection/shared POSIX kinds, existing Unicode fold helper, one immutable program setting. Existing byte VM/widths/state remain.
**Tech Stack:** C#13, net8/net9/net10, xUnit, PCRE.NET1.6.0/PCRE2 10.48 JIT.
**Spec:** doc/PCRE-BYTE-UCP-DESIGN.md

## Global Constraints

No agents/Python/SDK install. UTF-8 LF, root project/solution, src/tests/samples.
No API/package/version/dependency/workflow/release/consumer/fallback changes.
Existing depth256/captures256/instructions4096/marks256/frames1024/work1,000,000.
Grep last; CommandFramework untouched. User owns merge; preserve worktree/evidence.

## Review Focus

1. Raw high/continuation bytes and NUL stay width1; byte b/B must never enter UTF reverse decoding.
2. All byte case pairs, complements/ranges/backrefs and optimized literal ahead/behind share UCP folding while non-UCP remains ASCII.
3. aD/aS/aW/aP/aT/reset/overlap and UCP POSIX lower/upper retain exact native predicate and prefix provenance.
4. Caller/prefix UCP and i/scoped/reset/call modes remain distinct from UTF/source validation and capture-name grammar.
5. Work/cancellation and compile/frame limits retain accounting; existing UTF classification/case paths stay unchanged.

## Task1: Native contract/tests and observed RED

**Files:** New tests/Pcre/PcreByteUcpReferenceTests.cs, design/plan; UtfStartDirective, Utf8Digit, ByteProperty, AsciiRestriction and AsciiDigit historical gates; canonical inventories.
**Interfaces:** Existing Compile/Match/native reference only.

- [ ] Literal native masks: predicates and all256 case pairs. Test byte literals/classes/equal ranges/backrefs/optimized assertions; boundary adjacency uses independent word masks. Focus1..3.
- [ ] Literal spans: local/caller modes, resets/repeats/captures/retries/calls/behind, UTF/raw-source and CRLF source distinctions. Every-offset matrix and grammar/profile/resource/work/cancellation cover focus4..5.
- [ ] Replace UCP rejection gates with positive byte contracts; preserve counts and early diagnostic precedence. Update canonical inventories.
- [ ] Native checker/design-builder/plan-interface self-check/frozen35/whitespace, commit/publish tests-only draft, observe successful compilation and expected RED before source.

## Task2: Shared classification/folding implementation

**Files:** Compiler, Parser, Set, Program, Matcher, CaseFold under src/Pcre.
**Interfaces:** Append bool Ucp to immutable Program and final bool ucp to ProgramCompiler ctor. CreatePosixClass(string,bool,int)->PcreByteScalarClass; CreatePropertyClass(PcreByteScalarClass,bool prefixAny=true)->PcreByteNode; Set.UnicodeCaseClosure(CancellationToken,bool restricted=false)->PcreByteSet. Byte comparison helper adds unicode/restricted booleans.

- [ ] Remove UCP gate; pass resolved UCP to program metadata. Share POSIX kind table; byte UCP predicates project via existing evaluator with native provenance, ASCII restrictions stay bitmaps in brackets.
- [ ] Byte UnicodeWord boundaries classify bytes; UTF decoding paths unchanged. Use existing Unicode fold for UCP byte literal/backref/fixed-assertion comparisons and cancellable256-key literal/range closure. Existing non-UCP/predicate casing retained.
- [ ] Full/frozen214 on six native Staging platforms/net8/9/10; zero failed/skipped/build warnings/errors, configured gates/seven workload equalities. Fix material findings with observed RED before correction.
- [ ] Separate whole-branch author self-review, exact reviewed head/tree/log audit, frozen35/whitespace/clean proof. Update PR and mark ready; never merge.

Pre-flight: one ProgramCompiler caller, four projected predicate families share existing
membership/factory and byte bracket tuple; fixed assertion/readers stay byte-aware.
POSIX resolver extraction must preserve scanner-specific diagnostic ownership. No
saved-frame or public interface conflicts. Maintainer choices within standing authority.

First implementation run41162203194 Linuxx64/net8 passed all399 new cases;
4fail/5149pass/5153total came from two old UCP-rejection assertions repeated
by two parameter rows. Replace them with positive ASCII-restricted byte-UCP
checks; production remains unchanged. No count changes.
