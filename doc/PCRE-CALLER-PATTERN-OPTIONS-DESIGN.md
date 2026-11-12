# Caller pattern options design

After merged PR228, expose seven internal default-false caller options already
supported by inline syntax: DotAll, Multiline, NoAutoCapture, Ungreedy,
DuplicateNames, Extended and ExtendedMore. They map PCRE2 flags s/m/n/U/J/x/xx.
ExtendedMore implies extended parsing. Local unset/scoped options and group
restoration continue to use existing parser snapshots; (?^) resets m/s/n/x but
preserves U/J, exactly as the current inline implementation does. Named captures
remain captures under NoAutoCapture, including their numbering and references.

## Approach

My choice under the approved inline roadmap is to initialize the existing parser
fields from PcreByteOptions. Replace the parser's growing boolean constructor
argument list with its existing options record, read only during construction;
copy all old/new primitive values and retain no options reference. The compiler
is the only direct constructor caller. Keep prefix resolution and whole-pattern
UTF validation before parsing. Shared option scopes, nodes, lowering and matcher
already implement the behavior; no new AST/opcode/runtime state is needed.

Prepending synthetic inline headers would alter source offsets and prefix rules.
Adding seven more positional boolean arguments would obscure the existing
constructor contract. Both alternatives are rejected. Existing Caseless,
AsciiBsD, numeric/empty-class/UTS/escaped-CR policies must retain their copies.

## Evidence and review focus

1,046 distinct tests include independently checked 1,018 native PCRE2 10.48 profiles: 988 JIT-valid profiles
and 30 error profiles represented by 10 distinct compile-error cases. Every one
of 128 flag combinations is covered in byte and UTF modes. Explicit fixtures
pin local unsets, reset preservation of U/J, capture numbering, duplicate-name
references/conditions, greedy/lazy/possessive repeats, dot/anchors, xx class
spaces, quoting and raw labels. Supplement with malformed-UTF/CRLF start offsets,
existing caller-policy composition, original diagnostics, source/options ownership,
canonical work equality, cancellation and existing resource ceilings. Precompile
all native loop patterns before any managed matching so oracle errors cannot hide.

Review five boundaries: caller versus scoped/reset state; unnamed/named/duplicate
capture identity; newline/UTF/CRLF and greediness metadata; x/xx trivia/quotes/raw
labels and byte diagnostics; copied options and shared work/cancellation/quotas.
Freeze references only after clean-build expected RED, with every default/native
literal/exact legacy diagnostic/earlier-reference check passing. No weakened tests.

Preserve all earlier tests, GNU/BRE/ERE, 35 frozen production blobs, 214 frozen
outcomes in 120 methods, exported API and dependencies. C#13/net8/9/10, UTF-8+LF.
No Python, subagents, local SDK installs or runtime fallback. User merges/tags/
publishes. Other modes/directives/callouts, failed/partial labels, UTF code-unit
escapes, wider optimizer/resources, UTF16/API and realistic consumer/release
qualification remain open. Grep stays last.
