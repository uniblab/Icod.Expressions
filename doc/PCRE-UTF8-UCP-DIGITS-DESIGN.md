# Internal PCRE UTF-8 decimal digits and Grep's ASCII `\d` policy

## Intent

Cover Grep's existing C.UTF-8 digit fixture in the internal matcher: UCP makes POSIX `[[:digit:]]` Unicode-aware, while Grep's compile option keeps `\d` ASCII until leading `(?-aD)` removes that restriction. This is a preparation step; Grep keeps PCRE2 until the remaining syntax, invalid-UTF, option and performance gates pass.

## Approaches

| Approach | Tradeoff | Decision |
|---|---|---|
| Generalize every UTF bracket class and inline option now | Supports more syntax, but mixes a large bracket parser and scoped option-state refactor into the consumer digit fixture. | Defer. |
| Add scalar digit predicates and a restricted standalone POSIX digit bracket | Covers the actual Grep contract and its negations with byte-accurate spans; mixed UTF bracket sets and mid-pattern/scoped options stay explicit errors. | Choose. |
| Force all digit constructs to the same Unicode predicate | Simpler, but violates Grep's ASCII `\d` behavior under UCP. | Reject. |

## Scope and contracts

- New internal compile options `Ucp` and `AsciiBsD` apply in `Utf8` mode. `Ucp` without `Utf8` is rejected for this internal milestone. UCP enables Unicode decimal digit behavior, but `AsciiBsD` restricts only `\d` and `\D`; it does not restrict POSIX digits or `\p{Nd}`. The default UTF mode has ASCII `\d` without UCP.
- A sole leading `(?aD)` sets and `(?-aD)` clears the ASCII `\d` restriction for the whole pattern. Other placements and combined/scoped option settings remain explicit unsupported syntax. Existing single leading `(?s)`, `(?i)`, `(?m)` behavior remains intact.
- In UTF mode, `\d` and `\D` consume one scalar; an ASCII restriction recognizes U+0030..U+0039 and rejects non-ASCII decimal digits. Without the restriction under UCP they use the Unicode Nd category. Negation includes every other valid scalar, including non-ASCII letters and emoji. `\p{Nd}`, `\P{Nd}` and their braced `^` forms use Nd independent of UCP and ASCII restrictions. The unbraced `\pN` is a different PCRE category and remains unsupported.
- Standalone UTF brackets `[[:digit:]]`, `[[:^digit:]]`, `[^[:digit:]]` and `[^[:^digit:]]` use Nd under UCP and ASCII digits otherwise. POSIX and outer negation combine by XOR. Mixed brackets, ranges and other POSIX names in UTF mode remain unsupported, with no bytewise fallback.
- Preserve ordered backtracking, source-byte match/capture spans, strict UTF validation, cancellation and per-match work accounting. Existing 8-bit byte-mode behavior is unchanged. Category data comes from the target .NET runtime, and PCRE2 Unicode-version parity remains a migration gate.
- Reject `\b` and `\B` explicitly under UCP: the existing ASCII word-boundary instruction would otherwise produce incorrect Unicode results. UTF mode without UCP retains its current ASCII boundary behavior.

## Follow-on

Grep's `MatchInvalidUtf`, Unicode caseless behavior, UTF bracket unions/ranges, wider UCP shorthands, scoped option changes, Unicode-version conformance and performance review remain pending. No Grep or CommandFramework production change occurs here.
