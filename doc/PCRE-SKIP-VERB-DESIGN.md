# No-argument SKIP restart control

PR217 follows merged PR216. Add exact no-argument `(*SKIP)` to the managed
byte/UTF8 engine. Like PRUNE it saves a bounded cut and acts only when later
failure reaches the verb. At root, a strictly forward verb position becomes
the next candidate. Equal/backward positions use ordinary candidate advancement.
The comparison uses the actual candidate start, not K's reported match start.

## Architecture and native restart distinction

Reuse PRUNE's active call/negative/conditional assertion catches, journal
rollback/selected pre-verb callee values, atomic pruning and guard arenas. Add a
SKIP leaf/opcode/frame kind to select the root restart behavior; both verbs
share the1024 saved-frame budget. Root remembers at most one integer restart
per attempt. No new stack, runtime fallback, public API or dependency.

Native JIT strictly forward SKIP jumps to candidate entry, bypassing ordinary
CRLF bumpalong/repeat recovery. Thus SKIP may enter LF after CR even without
explicit newline source metadata; the equivalent PRUNE can skip that LF. Pin
this with newline-independent dotAll/word-boundary fixtures under ANYCRLF,
CRLF and ANY. A forward target is already a consumed scalar boundary in UTF;
existing fragment barriers still prohibit malformed-byte consumption. Reaching
fragment end/EOF must retry nullable alternatives there. Behind targets may
be equal to or earlier than the current start and must never create a loop.

Alternatives: treating SKIP as PRUNE loses consumed-region exclusion; changing
the reported start or bytecode jump cannot control the next search candidate.
The existing cut and candidate loops are the implementation choice under the
standing authorization to continue the roadmap inline.

Exact parsing, nonrepeatable direct verbs, grouped repetition, zero consumed
width and nonterminating behind width scans follow PRUNE. Named SKIP (including
empty colon), MARK, other verbs and directives remain unsupported. Existing
optimizer proofs remain conservative; no general optimization expansion.

## Evidence and gates

Pinned PCRE2 10.48 `pcre2pattern.3`, `pcre2_match.c` and JIT reset/control paths
are the reference. Independently exported literal captures cover byte/UTF,
root forward/equal/backward and EOF restart, quoted-region exclusion, K,
positive/negative/conditional/non-atomic assertions, behind, calls/recursion/
selected return lists, atomic/repeat/script-run/grapheme interactions and
mixed PRUNE/SKIP cuts. Selected malformed-UTF and CRLF subjects test every byte
offset. Shared frame/compilation/work/cancellation limits remain enforced.

Tests freeze after a clean successful-build RED; implementation then changes
production only to meet those fixed expectations. Preserve C#13/net8/9/10,
GNU/BRE/ERE, frozen35 and frozen214/120, and all earlier reference fixtures.
No Python, SDK/PowerShell installation or subagents. Wider verbs/MARK/directives,
UTF C, optimizer/resource conformance, UTF16/API, realistic consumer performance
and release stay open. Grep stays last; user owns merge/tag/publish.
