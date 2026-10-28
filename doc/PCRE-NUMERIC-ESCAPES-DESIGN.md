# Internal PCRE decimal references and octal escapes

## Goal and rules

Advance issue #7 by implementing default PCRE2 10.48 decimal/octal
disambiguation and explicit octal literals in the internal byte/UTF-8 parser.
Use the [pattern specification](https://www.pcre.org/current/doc/html/pcre2pattern.html)
and pinned `pcre2_compile.c` from `PCRE2Project/pcre2`, tag `pcre2-10.48`,
with PCRE.NET 1.6.0 as the test-only oracle.

- Outside brackets, read the whole decimal run after a nonzero digit. It is a
  backreference when the number is less than 10, its first digit is 8 or 9, or
  that many capture slots have already been allocated at this syntactic point.
  Otherwise consume up to three octal digits as a literal, leaving the rest
  for ordinary parsing. Forward references must name a slot in the final pattern.
- A leading zero always starts an octal literal of at most three digits.
- Inside ordinary brackets, 0–7 start octal literals of at most three digits;
  escaped 8/9 are literal digit members, never references.
- Explicit `\o{...}` requires octal digits, permits space/tab after `{` and
  before `}`, and accepts leading zeros. Byte values stop at 255; UTF values
  must be Unicode scalars. Bare octals can reach 0777 only in UTF mode.
- Numeric `\g{...}` references also ignore space/tab at brace edges, including
  signed relative references; interior whitespace remains invalid.

Branch-reset groups use the current branch's capture counter for decimal
disambiguation, not an earlier branch's maximum. Numeric references reuse
existing numbered capture nodes and the 256-slot ceiling. Octals reuse literal
nodes, including bracket ranges/unions, fixed literal assertions, caseless/UTF
matching, repetition and original byte spans. Actual backreferences in general
assertions remain unsupported. Quoted digits and `\o` stay literal under Q/E.

## Approaches and architecture

1. Treat all digit runs as references: simpler but changes valid PCRE2 octals.
2. Rewrite pattern bytes into g/x forms: complicates source-byte offsets and
   capture context, especially branch resets and quoting.
3. **Choose parser disambiguation.** Scan decimal digits with a saturating
   accumulator and cancellation checks, then consume the chosen reference or
   octal spelling. Put the helpers in `PcreByteParser.NumericEscapes.cs`.
   Share braced hex/octal range, delimiter and scalar validation through a
   radix-aware helper, retaining N{U+...}'s existing whitespace boundary.

No matcher, instruction, frame, public API, package version, production
dependency, Grep or CommandFramework changes. Python-style octal, NO_BS0 and
other extra compile modes remain pending; there is no fallback engine. Keep
the existing controlled compile/match limits, cancellation and frozen corpus.
Do not use subagents.

## Evidence and exit gate

Pin every byte value in both octal spellings and atom/bracket contexts, UTF
scalars and limits, fixed assertions, ranges, counts and quoting. Compare all
numbered captures for decimal-reference selection, forward/unset targets,
rollback, capture-count boundaries and branch-reset context. Include malformed
octals, long digit/zero runs, exact byte diagnostics and malformed-UTF offsets.
Observe the initial expected failures, then require six green Staging jobs
across net8/net9/net10 with existing frozen, workload and package gates.
This remains an internal conformance increment; release/migration gates stay open.
