# Internal PCRE ASCII restriction families

## Intent and scope

Continue issue #7 after #83 by completing the ASCII restriction option family:
existing aD plus aS, aW, aP, aT and bare a in leading/local/scoped/mixed headers.
Preserve source-byte spans, compile-time option scope and pinned PCRE2 behavior.
PCRE.NET1.6.0 / PCRE2 10.48 Compiled/JIT remains the test oracle.

## Approach

Select a private five-bit parser mask and existing immutable predicates. Separate
high-level aP/aT booleans cannot reproduce their overlapping native masks. Runtime
option state would duplicate source-local lowering and require unnecessary VM
changes. This choice follows the standing autonomous implementation remit.

The bits are BackslashD, BackslashS, BackslashW, Posix and PosixDigit. Seed only
BackslashD from existing caller AsciiBsD. aD/S/W select their respective bit; aT
selects PosixDigit; aP selects **Posix and PosixDigit**; bare a selects all five.
No new caller option properties are needed. UCP without UTF remains unsupported.

## Semantics

- Each a consumes a following uppercase D/S/W/P/T if present; otherwise it means
  all restrictions and leaves the following character for normal option parsing.
  Thus ai combines all restrictions with i, while aDP has an invalid trailing P.
  Header trivia/case rules remain exact. Repeated settings are valid.
- Accumulate set/unset masks transactionally, then apply `(old | set) & ~unset`.
  Unset wins per bit, independently of source order. In particular, aP-aT keeps
  Posix, while aT-aP clears PosixDigit. Bare a participates in the same algebra.
- Reset ^ preserves all restriction bits; explicit letters then apply. Existing
  caret/hyphen, empty-header, x/xx and nonrepeatability rules remain unchanged.
- Under UTF/UCP, restricted d/D, s/S and w/W use existing ASCII predicates;
  unrestricted forms retain their Unicode predicates. aW also selects ASCII
  classification for b/B. Digit/space restrictions do not change word boundaries.
- Posix disables Unicode substitution for every ordinary POSIX class. PosixDigit
  alone disables it for digit and xdigit. The overlap matters even if aP and aT
  are subsequently unset separately. ASCII lower/upper under i include ASCII
  letters only; aP/aW do not restrict unrelated literal/range case folding.
- Explicit Unicode properties and h/H/v/V/R remain independent. POSIX word/space
  and backslash w/s are independent except when bare a sets both restrictions.
- Group entry/exit snapshots retain the mask for every group/assertion kind.
  Later alternatives inherit changes; prior nodes do not change. Byte and
  UTF/non-UCP modes accept the options without widening Unicode matching.

## Implementation

In `src/Pcre/PcreByteParser.cs`, replace the private asciiBsD state with the
five-bit enum/mask, preserving the constructor's existing bool parameter.
Extend TryParseOptions a-token scanning and accumulate masks; retain transactional
apply and reset preservation. Add a small mask query helper. Select existing
predicates in ParseEscape and ParseUtf8PosixItem, and choose b/B's existing
UnicodeWord flag using BackslashW. In `PcreByteParser.Groups.cs`, snapshot and
restore the mask instead of its former single bit. No syntax node, bytecode,
compiler, matcher, runtime frame, journal or resource policy changes.

## Verification and boundaries

Test explicit native selected outcomes/group counts and all capture byte spans.
Cover the five individual bits and bare a; overlapping aP/aT set/unset and reset
algebra; all14 POSIX names, negations/unions, local scopes/alternatives, caller aD
preservation, mixed existing flags, Unicode word boundaries, captures/references,
assertions, repetition, UTF widths and byte/non-UCP controls. Pin strict ASCII
classes under i against Kelvin/long-s while literal/range folding still works.
Compare selected malformed-UTF/CRLF caller offsets for classes and b/B. Check
exact token diagnostics, unsupported J/r, original byte offsets, depth256,
capture256, bare-header nonnesting, work and cancellation.

Test-first CI is required because no local SDK is available. Final gate: six
Staging OS/architecture jobs; full net8/net9/net10 plus214 frozen tests per
framework; configured workload/extraction/API/package-only consumer checks.
Local whitespace/frozen-source checks, UTF-8 LF and no Python remain requirements.

No public API/version/dependency or consumer changes. Unicode/version parity,
remaining syntax/performance/release and Grep migration remain separate gates.
CommandFramework removal still waits for all known consumer migrations.

## Pinned source

`src/PCRE/src/pcre2_compile.c`: handle_escdsw2890–2940; POSIX4100–4140;
option scanner5036–5142; word-boundary lowering8370–8385. Native compile/match
probes request JIT_COMPLETE and confirm strict ASCII versus literal folding and
mask-overlap controls. Current .NET Unicode predicates remain provisional.
