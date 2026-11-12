# CASELESS_RESTRICT prefix design

## Intent and scope

After merged PR223, accept exact leading (*CASELESS_RESTRICT) in the existing
uninterrupted global prefix. It initializes the same restricted casing policy
as (?r), without enabling caseless matching. Existing (?i), (?-r), scoped options
and (?^) retain their native behavior. Reuse the single byte/UTF engine and
pinned Unicode17 tables. This is my implementation choice within the approved
syntax roadmap; the user has already authorized continued inline development.

## Approaches

Initialize the existing parser field from start settings: one prefix flag,
no emitted instruction, new runtime mode or source rewriting. This is the
selected approach because (?r) already propagates through literals, classes,
references, scopes, calls and assertions. Rewriting the source to (?r) would
move byte diagnostics and change prefix recognition. A separate runtime flag
would duplicate an already qualified comparison policy.

## Contract

Recognize only the exact uppercase prefix, before ordinary options, comments
or body text. It can repeat and interleave with implemented start directives.
Preserve the original source and BodyOffset, including whole-pattern UTF
validation before body parsing. Initialize parser.CaselessRestricted before
its root options snapshot; ordinary scoped/reset logic remains authoritative.
The directive consumes no capture, mark, instruction or match work allowance.
Prefix scanning continues to poll cancellation. Ownership remains the compiled
pattern's existing source copy. No exported API, dependency, GNU or native
fallback changes. Failed/partial labels and other directives remain separate.

## Evidence and review focus

333 literal/profile fixtures were independently checked with pinned PCRE2
10.48, all with successful native JIT compilation. They cover six caller modes
(byte, UTF, UCP and caseless combinations), prefix order, ASCII/non-ASCII folds,
reset/unset/scoped modes, brackets/Perl algebra/properties, references, recursive
and forward calls, assertions, captures and immediate labels. Additional tests
cover every offset over malformed UTF/CRLF subjects; exact/misplaced/unfinished
headers and original byte diagnostics; UTF activation ordering; duplicate-prefix
work/cancellation; 256 capture/mark and 4096 instruction bounds; quoting and
owned source memory. These are five review classes: mode initialization/reset;
prefix placement/diagnostics; all comparison consumers and captures; UTF/source
ownership; resource and work/cancellation invariants.

Native source: pcre2_compile.c pso_list PSO_XOPT/CASELESS_RESTRICT; native sets
PCRE2_EXTRA_CASELESS_RESTRICT before parse_regex. Combined TURKISH_CASING is a
native compile error, while managed still rejects that pending directive at its
opening. Existing (?r), start-directive and all earlier reference files remain
unchanged. Freeze this new file only after a clean-build expected RED.
