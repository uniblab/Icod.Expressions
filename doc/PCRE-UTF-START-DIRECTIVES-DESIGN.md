# UTF and UCP start directives

## Intent and contract

Continue issue #7 after #106 with exact start directives `(*UTF)`, the 8-bit
alias `(*UTF8)`, and `(*UCP)`, selecting modes already supported as internal
caller options. Primary authority is pinned PCRE.NET1.6.0 / PCRE2 10.48,
`pcre2_compile.c` pso_list/skipatstart and UTF validation around10634..10675.

All seven implemented start directives share one uninterrupted, case-sensitive
prefix from original byte0. UTF/UTF8 enable UTF; UCP enables UCP without implying
UTF. These enables combine with caller defaults and cannot unset them. Newline
and BSR keep independent last-wins overrides. Any supported ordering, repetition
or interleaving is accepted. Ordinary options, trivia, quotes or other source
end the prefix; misplaced/unknown forms retain existing diagnostics.

Select final settings before the existing UCP-without-UTF gate and whole-pattern
UTF validation. Thus a UTF directive can enable a caller's UCP setting, including
when UCP appears earlier in the prefix; a UCP directive alone in byte mode still
returns controlled UnsupportedSyntax at0. That capability gap is unchanged.
Caller MatchInvalidUtf still implies UTF and alone controls subject tolerance;
UTF directives do not enable tolerance. Strict UTF pattern validation always
checks original bytes and retains existing precedence over ordinary body syntax.
Invalid-work-limit and pre-cancellation precedence stay unchanged.

All parser/program consumers receive the same effective mode choices: literal
and class decoding, shorthand/POSIX/property policy, names, widths, captures,
backreferences/calls, search/fragment boundaries and matching diagnostics. These
settings are global and survive inline resets, groups and calls. Original bytes
are retained with one body offset; no slicing/rewriting or directive AST nodes.

## Approach and alternatives

Chosen by Codex under the standing autonomous development remit: extract the
existing four-token parser scan into immutable compile settings, extending it
with the three new tokens. One readonly record struct stores UTF, UCP, newline,
BSR and BodyOffset. The compiler resolves it once, validates using it, then passes
it to the parser and existing program compiler. The parser initializes its
original index and readonly mode fields from this value.

Keeping the scan in Parse would select UTF too late. Scanning twice would risk
different prefix boundaries; constructing a mutable parser before validation
would complicate readonly mode/phase guarantees. Cloning caller options adds
allocation and obscures the original source position. A compile-only struct
keeps a single phase and no additional matching state. Transient span arguments
are permitted in synchronous scan helpers; no spans/interior pointers are kept.

## Files, bounds and validation

Create PcreByteStartSettings.cs; modify compiler/parser configuration and remove
the superseded parser prefix partial. No matcher, AST, opcode, frame, journal,
Unicode table, width algorithm or resource limit changes. Existing start-directive
fixtures must remain green. New native-first literal fixtures pin mode enabling,
caller combinations, ASCII restrictions, mixed prefix ordering, original-byte
errors, quotes/misplacement, strict/tolerant UTF subjects, captures/names/calls/
behind widths, long prefix and work/cancellation equivalence with caller options.

No subagents, per user instruction: self-check design/plan and separately review
the full branch. Observe tests-only RED before source edits; require all six
native Staging full net8/net9/net10 suites plus frozen214/configured gates and
seven Linux workload equalities; frozen35, whitespace, clean/exact tree checks.
No local SDK: managed executable RED/GREEN is verified in CI. Preserve evidence.

C#13, UTF8+LF; root project/solution, src source, tests tests, samples samples.
Local Debug; PR Staging. No Python, public API, package/version/dependency/workflow/
release/consumer changes or production fallback. UCP without UTF, other newline/
limit/optimization directives, #99/#102 optimizer parity, Unicode-version and #100
unsafe native Xuc issues remain separate. Grep last; CommandFramework removal
requires publication and migration of all known consumers. User owns merge.
Builder owns private helper names/fixture organization, not new engine policies.
