# Named SKIP design

The next issue7 increment implements `(*SKIP:NAME)` against pinned PCRE2 10.48
JIT (commit7978954dbd2efc6f2196869290553cf1871b4ce6; PCRE.NET1.6.0).
It resumes search at the newest matching MARK that is still visible to
backtracking. Missing names preserve ordinary root retries. An empty argument
is unnamed SKIP. Names are ordinal raw bytes, at most255 bytes, with native
NUL-prefix comparison; pattern memory remains owned by the compiler.

## History and scopes

Keep the existing journaled successful label slot. Only programs containing a
nonempty named SKIP additionally save MARK history in the existing bounded
backtrack stack. Each history frame records instruction, position, journal
checkpoint and scopes; popping it restores state and continues failure.
Atomic groups and successful atomic assertions discard their history. Returned
callees retain their successful label, but their saved history is invisible to
caller lookup. Successful non-atomic positive assertions retain visible history.

Reuse the SKIP cut frame and existing scope/capture unwinding. The pinned JIT
performs a local quit in called and negative/conditional assertion scopes before
name lookup, even for a missing name. At root (including an uncaught positive
assertion), search newest-first and charge every inspected frame. A missing name
allocates no cut. A found name supplies its saved subject position; existing
forward-only restart, CRLF, UTF fragment and empty-match behavior then applies.
Do not add an unbounded history arena or per-name mark slots.

## Grammar, metadata and bounds

Share MARK's cancellation-aware raw-name lexer, preserving default verb grammar
and byte diagnostics. Reject direct quantifiers. Named SKIP is zero-width,
nullable and nonterminating for lookbehind validation. Pass global parser
presence to compilation, including occurrences in dead code. Preserve the
shared1024-frame,256-mark,4096-instruction and work/cancellation limits.
Extend only the existing one-root-branch bounded COMMIT first-byte proof across
SKIP/named SKIP/PRUNE prefix controls; the general issue102 optimizer stays open.

## Reference policy and evidence

Historical GNU/BRE/ERE behavior,35 frozen production blobs,214 frozen outcomes
and120 methods remain fixed. Two old SKIP header rejection rows are obsolete
managed capability inventory: retarget them to nonexact headers when promoting
named/empty-colon SKIP. Preserve every native match/capture expectation in that
file byte-for-byte, and retarget one unfrozen group-alias pending syntax row.
The revised inventories and231 new cases freeze after clean-build RED.

199 literal/profile cases were independently checked by380 native executions.
Further cases cover all selected subject offsets, UTF fragments, raw/NUL names,
diagnostics, history visibility, selected callee captures, quota composition,
charged lookup and cancellation. Native-only `(*NO_JIT)` on the large all-success
quota controls avoids the default native JIT stack ceiling; production patterns
have no such directive. Native JIT remains the oracle for all scoped cuts.

Exported API, dependencies and production fallback policy remain unchanged.
This remains the internal experimental matcher; Grep integration stays last.
