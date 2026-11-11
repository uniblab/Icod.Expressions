# No-argument COMMIT control

PR218 follows merged PR217. Exact `(*COMMIT)` saves a bounded cut, acting only
when later failure reaches that frame. A root cut terminates the entire search.
Active call, negative and conditional assertion catches behave like PRUNE/SKIP;
positive and non-atomic assertions propagate cuts. Successful atomic/assertion
scopes discard their interior frames. The rightmost triggered cut determines
behavior, so executing COMMIT must not eagerly set a global flag.

Reuse existing capture/mark/start journals, selected pre-verb callee values,
heap scope arenas, shared1024-frame and work/cancellation budgets. Add one
leaf/opcode/frame kind. Root termination occurs only after charged frame
rollback/discard. Parsing rejects direct quantification; grouped repeats remain
valid. Behind analysis treats COMMIT as zero width without hiding its tail.

## Native first-code-unit entry

Native PCRE2 skips leading candidates before entering `(*COMMIT)abc`: xyzabc
matches at3, while xaabc fails after entering at the first a. A whole literal
search is wrong. UTF likewise uses the first encoded byte: leading COMMIT/é
skips x but enters at ê (both start C3) and can terminate there. Tolerant UTF
must admit a matching malformed lead byte as a zero-width-only candidate;
consumption remains barred by the existing fragment protocol.

Add optional immutable first-byte metadata only for programs containing COMMIT.
Prove a single root branch through bounded one-branch capture/noncapture groups,
required repeats, atomic wrappers, positive lookahead, empty groups and K, with
a preceding COMMIT. Stop on alternatives, optional repeats, classes, calls,
anchors, negative/conditional/behind assertions and unknown nodes. Bound both
visits and group descent; poll compile cancellation. Case-sensitive literals
supply one first byte. ASCII caseless pairs qualify except Unicode K/S complex
casesets without r. General non-ASCII caseless/start optimization stays issue102.
Charge every runtime prefix check and inspect input length, not fragment end.

A mixed COMMIT/PRUNE control also pins the existing greedy literal cache when
COMMIT directly precedes PRUNE: the later PRUNE overrides COMMIT, and native
skips re-entering the consumed run before its fallback. Extend only that exact
existing bounded proof if required by the frozen test; no general optimizer.

Alternatives considered: a permanent global flag breaks scope catches and
rightmost-cut precedence; treating COMMIT as PRUNE incorrectly retries later
starts; unconditionally entering a leading COMMIT contradicts documented native
start optimization. The existing cut path plus bounded entry proof is chosen
under the standing authorization to continue development inline.

## Evidence and gates

Pinned PCRE2 10.48 pcre2pattern, matcher and JIT code plus independent native
literal capture exports qualify byte/UTF/UCP cases. Tests cover root/scope cuts,
calls/recursion/returned lists, rollback/K, atomic/repeated/script-run/grapheme
interactions, mixed verbs, first-byte optimization, CRLF and every selected
malformed-UTF offset, grammar, compile/frame/work limits and cancellation.
Freeze all new reference tests after successful-build RED; production changes
then meet those fixed expectations. Preserve prior fixtures, frozen35 GNU
blobs and frozen214 results/120 methods. C#13/net8/9/10; no production fallback,
new dependency, Python, installation or subagents. Named verbs/MARK, THEN and
other directives/callouts, UTF C, wider optimizer/resources, UTF16/API,
performance and release remain gated. Grep stays last; user merges/publishes.
