# Caller-selected restricted and Turkish casing

Add default-false internal CaselessRestricted and TurkishCasing for PCRE2 10.48
extra-options0x80 and0x10000. Initialize the existing start-settings choices from
the caller before reading the uninterrupted prefix; exact source directives OR
into those choices. Neither option implicitly enables caseless or UTF matching.
No source rewriting or synthetic prefix is used, preserving original offsets.

## Approach

The existing prefix/scoped comparison machinery already implements both modes.
Seed its two booleans in PcreByteStartSettings.Parse and add two option properties.
This is the selected approach: a new matcher flag would duplicate qualified
state, and rewriting the pattern would move diagnostics and prefix boundaries.
The parser, program compiler, Unicode17 folding, assertions, class closure,
references, calls and bounded optimizer proofs keep their existing consumers.

Restricted casing is the initial lexical r policy; local (?r), (?-r) and (?^)
remain authoritative. Turkish casing remains global across resets, with lexical
r selecting the existing restricted default comparisons. Global caller/source
Turkish plus global caller/source restriction is a native error even if a later
scope unsets r. In8-bit mode Turkish requires effective UTF; UCP-only byte mode
is invalid. Source UTF validation and NeverUTF/UCP priority remain existing;
mode-conflict errors use the consumed-prefix end. No case policy relaxes source
validation, structural/resource limits, cancellation or unsupported syntax.

## Qualification

Pin all four selections with independent caseless/UTF/UCP/tolerant modes;
I/i/İ/ı, K/k/Kelvin and S/s/long-s comparisons; ordinary/Perl/UTS classes,
assertions, references, calls, scopes/reset, word/line modes, prefixes, labels,
diagnostics, malformed offsets, ownership, canonical work and resource bounds.
Observe clean net8/9/10 tests-only RED, then freeze the fixture before source.
Qualify all18 native results and every configured supporting gate. Repository
inventories precede final qualification; final GREEN/review/identities go on
PR242 and only issue7's footer, avoiding another documentation-only CI run.

Baseline merged241 e27a301cc8493ac457db8c2a5f2b95fd58e3f2e8,
treecbdd3ee4735486e5db4bb8a5c885a058ed9dc524;72,857 tests/framework and257
prior test files. Keep those hashes, GNU/BRE/ERE, frozen35/214 outcomes/120methods,
C#13/net8/9/10, UTF8+LF, AST/IR/opcodes/matcher/journals/shared limits/exported
API/dependencies/workflows fixed. Reuse the isolated linked worktree, execute
inline without questions/subagents/SDK installs. User owns merge/tag/publication;
Grep last and existing unsupported boundaries remain explicit.

## Regression evidence and integration

Tests-only [RED733](https://github.com/uniblab/Icod.Expressions/actions/runs/43432119944), source `c77092cbfb760f9c0e1452353d84bc68316773a5`, Linux ARM job131907925887: net8/9/10 each built without compiler warnings/errors and ran678 expected failures/76,852 passes/77,530total/zero skips. All failures were new missing-policy cases (338 spans,321 diagnostics,2 membership,12 offsets,2 work,2 ownership and1 quota); every earlier test and3,995 new controls passed. Fixture `94b6ffd9c834cbbc10ab03d97cf3c98b7e303c08` froze before production and remains unchanged.

The implementation changes only the two planned production files: two documented caller properties and two existing start-setting initializers. Prefix composition, validation and all parser/runtime consumers remain unchanged. Final all18 qualification, author review and exact identities are recorded on PR242/issue7 after the final source head.
