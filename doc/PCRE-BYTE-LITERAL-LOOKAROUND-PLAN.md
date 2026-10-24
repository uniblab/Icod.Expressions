# PCRE Byte Literal Lookaround Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add an internal, bounded fixed literal lookaround slice to the PCRE byte matcher.

**Architecture:** Parse four assertion prefixes into an owned literal byte sequence, compile one assertion opcode, and compare adjacent input bytes without changing input position or captures.

**Tech Stack:** C# 13, .NET 8/9/10, xUnit.

**Spec:** `doc/PCRE-BYTE-LITERAL-LOOKAROUND-DESIGN.md`

## Global Constraints

- Root solution and project in `/`, implementation in `/src`, tests in `/tests`.
- Internal 8-bit non-UTF API only. No runtime PCRE.NET or System.Text.RegularExpressions.
- Preserve frozen GNU source/tests, exact byte offsets, 1,000,000 default work budget, cancellation, and single library package.
- No Grep switch or CommandFramework production change.

## Review Focus

- At the beginning/end of input, positive and negative assertions differ correctly.
- A failed assertion can backtrack into an outer alternative without leaking captures.
- Embedded NUL and high bytes compare as bytes.
- A long literal cannot bypass the match work limit or cancellation.
- Nonliteral or malformed assertion bodies fail at compile time.

---

### Task 1: Tests and parser

**Files:** `tests/Pcre/PcreByteLookaroundTests.cs`, `src/Pcre/PcreByteSyntax.cs`, `src/Pcre/PcreByteParser.cs`.

**Interfaces:** `PcreByteLiteralAssertion(ReadOnlyMemory<byte> Bytes, bool IsPositive, bool IsBehind)`.

- [ ] Add focused tests for the five review cases, all four prefixes, capture numbering, escapes, and unsupported grammar.
- [ ] Push test-only commit; observe expected failing PR CI because the new syntax is not yet implemented.
- [ ] Parse the four prefixes, literal body, escapes, unclosed and unsupported forms; reject assertion quantifiers.

### Task 2: Bytecode, matcher, and verification

**Files:** `src/Pcre/PcreByteProgram.cs`, `src/Pcre/PcreByteMatcher.cs`, `doc/PCRE-DESIGN.md`.

**Interfaces:** `PcreByteOpcode.AssertLiteral` carries an owned literal and its polarity/direction; matcher charges `Step()` per compared byte.

- [ ] Emit assertion instruction and execute it at the current byte position without changing captures.
- [ ] Push implementation and verify focused/full tests plus six-runner CI, frozen corpus, package and consumer checks.
- [ ] Update supported/rejected syntax table and request independent review.
