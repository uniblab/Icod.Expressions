# Optional match-start reset inside assertions

Implement default-false internal AllowLookaroundBackslashK, corresponding to
PCRE2 10.48 extra compile option0x40. A real backslash-K in an active assertion
keeps its existing InvalidSyntax diagnostic unless the caller opts in. The
setting is global and cannot be changed by local options or reset headers.

## Approach

Snapshot one parser boolean and condition the existing assertion prohibition.
The canonical MatchStartReset node already lowers to the shared capture-start
instruction for slot0. Positive assertions retain journaled state; failed and
negative assertions restore it; non-atomic retries and subroutine calls use the
same journal. Reuse those paths rather than add an assertion-only reset opcode
or rewrite source. Production scope is options and parser only unless failing
native-first evidence establishes another required correction.

The native option permits unusual reported spans: a positive lookahead can
return start2/end0, and lookbehind can report a start before the caller offset.
Preserve those endpoints as signed ByteLength=end-start in the existing internal
records, including capture0. Do not clamp or slice that span. The PCRE.NET oracle
clamps Length, so compare its EndIndex minus Index instead. This does not decide
the eventual exported API or introduce enumeration/substitution behavior.

## Boundaries and qualification

Pin atomic/non-atomic positive and negative ahead/behind, aliases, conditional
assertions, nesting, branch rollback, repeated/erased assertions, calls, captures,
references, ACCEPT/cuts/labels, NOTEMPTY policies, strict/tolerant UTF, original
offsets and source/error precedence. Inert quoted/comment/label text and lexical
call targets remain separate from real assertion tokens. Opting in never admits
class K, repeated K, invalid sources, unbounded lookbehind, callout execution or
other unsupported syntax. Capture/depth/expansion/work/cancellation limits stay.

Baseline merged242 e3a772301a021a65797f44d5d397444661532288, tree
525de953b3e6f05f4ef49caf783863bfd2179ace,79,118 tests/framework and259 prior
test files. Preserve their hashes, GNU/BRE/ERE, frozen35 production blobs and214
frozen outcomes/120methods. C#13/net8/9/10, UTF8+LF; no Python, SDK installs,
agents, runtime oracle fallback, exported API/dependency/workflow changes.

Execute inline under the standing continuous-development authorization. Reuse
the linked worktree. Observe clean tests-only RED, freeze the new fixture before
production, then qualify all18 native results and every supporting gate. All
repository docs precede final source qualification; final GREEN/review/identity
evidence belongs on PR243 and only issue7's footer. User owns merge/tag/publish;
Grep migrates last and wider optimizer issue102 remains open.

References: pinned PCRE2 10.48 pcre2_compile.c ESC_K assertion guard; native JIT
and endpoint protocols; https://www.pcre.org/current/doc/html/pcre2api.html and
https://www.pcre.org/current/doc/html/pcre2pattern.html .

## PR243: optional assertion match-start resets

Tests-only [RED737](https://github.com/uniblab/Icod.Expressions/actions/runs/43444152881), source `552f6672bac9d57214d556d73fb76478429cd340`, Linux ARM job131949099519: net8/9/10 each built with zero compiler warnings/errors, then ran **1,406 expected failures / 79,328 passes / 80,734 total / zero skips**. Only missing-policy cases failed:1,366 primary spans,3 error boundaries,24 offset theories,4 work/ownership cases,2 capacity/depth facts,1 quota fact and6 implicit-guard profiles per framework. All79,118 earlier tests and210 new controls passed. All native literal endpoint assertions passed. Freeze `a6cd825ba357ca8be6f11fd3ff788032f0c24217` before production; no fixture refinements were needed.

The1,616 new cases include1,574 primary native profiles (1,510 matching/query/capture/success-label and64 compile profiles),1,032 selected offset protocols and16 supplemental work/capacity/implicit-guard profiles. All accepted native matching uses JIT0 and negative matching requires no-match(-1). Fifty-eight primary profiles report reversed group0 endpoints. Two UTF-C profiles outside native JIT support remain separate compile/unsupported boundaries. Another54 native review probes cover selected reset/i/r scopes, UTF/casing prefixes and lexical callee resets.

Integration changes only options and parser: one default-false property, one readonly snapshot and one existing guard condition. No IR, opcode, matcher, journal or limit changes. All259 earlier files stay fixed. All18 final GREEN/supporting gates and author-review/identity evidence are recorded on PR243 and issue7 after observation, without another source push for documentation.
