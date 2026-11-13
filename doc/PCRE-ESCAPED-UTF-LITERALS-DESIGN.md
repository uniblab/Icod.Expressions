# Escaped UTF-8 literal scalars

Complete ordinary backslash followed by a valid non-ASCII UTF-8 scalar, including
ordinary/Perl/UTS classes and range endpoints. PCRE2 treats these as literal
scalars; only ASCII letters/digits have special escape grammar. Byte mode keeps
its byte-oriented interpretation. Effective UTF still includes caller tolerant
mode and source UTF prefixes, with whole-pattern validation before parsing.

## Approach

At the existing shared escape entry, after the trailing-backslash check and
before consuming an ASCII byte, reuse ParseQuotedLiteral for a non-ASCII scalar
in UTF mode. This reader already consumes one validated complete scalar and
returns the canonical literal node. Remove the obsolete unsupported tail and
update its recovery comment. No source rewriting, opt-in flag, new IR/opcode or
matcher path is needed. Hand-decoding another scalar reader would duplicate the
validated reader; a new flag would invent a native grammar distinction.

All existing scope/case/Unicode17/class algebra, capture/reference/call/assertion,
width/repeat/optimization and work/cancellation consumers receive the ordinary
literal. Existing alternate verb-name UTF decoding remains separate. Quotes,
comments and raw labels keep their own token ownership. Invalid UTF, surrogate
source, ASCII escapes, unsupported UTF-C and other syntax keep existing guards.

## Intentional retirement of pending assertions

Two earlier fixtures explicitly assert that ordinary escaped e-acute is not yet
implemented: PcreBadEscapeLiteralReferenceTests and PcreAltVerbNameReferenceTests.
After a clean tests-only RED freezes the new native-first fixture, replace only
those temporary assertions with native matching checks for the same inputs and
caller selections. Preserve both facts, all existing unrelated assertions and
every source/control input. The other258 earlier test files remain hash-identical.
This is implementation of the pending grammar, not bad-escape recovery.

## Gates and constraints

Baseline merged243 is8d3221a738a4d69639dc1ac11e2d0459bf04c3f0, tree
e2ab37e841f9c0a207a1b9e170051693558ae906;80,734 tests/framework and260 prior
test files. Preserve GNU/BRE/ERE, frozen35 production blobs and214 frozen outcomes
in120 methods. C#13/net8/net9/net10, UTF8+LF; no Python, agents, SDK installs,
runtime oracle fallback, exported API/dependency/workflow changes. Reuse the
linked worktree and execute inline under continuous-development authorization.

Pin scalar/encoding boundaries, multibyte repeats and widths, ordinary/Perl/UTS
classes, casing restrictions/Turkish, options, source prefixes, labels/quotes,
malformed source/subject offsets, owned data, canonical work and resource limits.
Observe clean net8/9/10 RED before production; freeze the new fixture. Qualify all18
native results and every configured supporting gate. All repository inventories
precede final qualification; final GREEN/review/identities go on PR244 and only
issue7's footer. No docs-only source push. User owns merge/tag/publication; Grep
migrates last. UTF-C/callouts and broader optimizer102 remain open.

Reference: pinned PCRE2 10.48 compile escape processing and
https://www.pcre.org/current/doc/html/pcre2pattern.html .

## PR244: ordinary escaped UTF literal scalars

Tests-only [RED739](https://github.com/uniblab/Icod.Expressions/actions/runs/43461757644), source `7198004337030e2d39008bc20843809b0fe5dc4d`, Linux ARM job132010183721: net8/9/10 each built with zero compiler warnings/errors and ran **687 expected failures / 81,040 passes / 81,727 total / zero skips**. Failures were619 primary profiles,35 later-error boundaries,24 offset theories,5 work/ownership cases,2 capacity/depth facts,1 quota and1 strict-subject fact per framework. All80,734 earlier tests and306 new controls passed; every native literal assertion passed. Freeze `1d6cf18667352f51c8d3a245560803af8fe1ab31` before production; no fixture refinements.

The993 new cases include956 primary native profiles (854 matching/query/capture/success-label and102 compile profiles),1,320 selected offset protocols and15 paired work/capacity profiles. Accepted native matching requires JIT0 and negative matching requires no-match(-1). Four UTF-C profiles outside native JIT support remain separate compile/unsupported controls.

Two earlier facts intentionally retire temporary escaped-e-acute UnsupportedSyntax assertions. Their same inputs and caller selections now require pinned native0:2 acceptance and managed parity, with every unrelated quote/C/directive/surrogate guard retained. Both facts remain; the other258 earlier test files stay hash-identical. The frozen new fixture also covers those exact source/caller selections, guarding against a masked regression. No invented caller flag or escape-recovery interpretation is introduced.

Integration reuses ParseQuotedLiteral at shared UTF non-ASCII escape entry after the trailing check. Only one parser branch and the obsolete tail change, plus a recovery comment. All final inventories precede qualification; final all18 GREEN/supporting/author-review/identity evidence goes on PR244/issue7 without another source push for documentation.
