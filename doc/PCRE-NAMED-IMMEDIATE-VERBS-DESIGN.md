# Named ACCEPT and FAIL design

Continue issue7 after merged222 with ACCEPT:NAME, FAIL:NAME and F:NAME. Pinned
PCRE2 10.48 (7978954dbd2efc6f2196869290553cf1871b4ce6, PCRE.NET1.6.0) expands
nonempty arguments to an actual MARK immediately before acceptance/failure.
This differs from the delayed cut labels completed in222. Empty arguments mean
unnamed verbs. Failed/partial-attempt result labels remain a separate API gate.

## Implementation choice

Add optional raw-name memory to the existing Accept/Failure leaves and reuse
the shared cancellation-aware lexer. Emit the existing PathMark opcode before
EarlyAccept/Fail for nonempty names. Reuse the shared lazy label slot, journal,
authentic named-SKIP history frames and resource checks; add no special runtime
label path or engine fallback. Reusing delayed-cut label operands would omit
authentic MARK history. A parser-created wrapper group would complicate direct
FAIL repeat rejection, ACCEPT widths and capture-close context unnecessarily.

Names retain raw-byte255 limits, NUL views and owned source memory; a nonempty
raw NUL prefix is still a MARK. Direct FAIL/F repeats remain invalid. ACCEPT
quantifiers apply to the whole implicit MARK+ACCEPT operation, so omitted and
zero-count accepts write no label. Existing capture closes, caller/assertion
exits, NOTEMPTY, K, script-run bypass, recursion/returned values and termination
of consumed-width branches stay shared. Bare unterminated F/FAIL diagnostics
remain byte1; only named colon headers use the shared end-byte diagnostic.

The bounded lexical atomic-assertion history walk recognizes nonempty names on
both leaves, including erased/nested/DEFINE bodies. Runtime erasure does not
erase this lexical isolation. Retain existing1024 frames/256 marks/4096 compiler
bounds, work and cancellation. Scoped acceptance/negative failure must release
implicit history while preserving accepted labels or rolling failed labels back.
Existing bounded PRUNE/COMMIT proofs retain their typed-node behavior.

## Reference scope and policy

312 cases include266 literal/profile fixtures checked by526 independent native
executions. Most use native JIT; six non-atomic ACCEPT fixtures (12 profiles)
produce PCRE2_ERROR_JIT_UNSUPPORTED and therefore use the normal native matcher,
as the existing compiled PCRE.NET oracle does. This is an explicit finite
qualification, not a general claim about all JIT-unsupported combinations.
Large all-success quota controls use native-only NO_JIT to avoid the default
native stack ceiling; production patterns contain no such directive.

New cases cover successful labels, quantified/omitted acceptance, capture/scope
transactions, MARK visibility, UTF/CRLF offsets, raw/NUL/grammar/errors, history
composition and reclamation, slot/expansion/work/cancellation and ownership.
Failed-match native label values are excluded until that metadata is supported.

Two obsolete ACCEPT rejection rows retarget to nonexact headers. Two obsolete
named-FAIL inventory rows retarget to valid pending directives that still fail
natively; all native match/capture source outside those blocks remains fixed.
Group-alias pending entries advance to directives, syntax entries to callouts;
the native0:1 characterization and original opening offset remain unchanged.
All new/revised references freeze after clean-build RED before production.
Earlier native expectations, frozen35/frozen214/120, GNU/BRE/ERE, exported API and
dependencies remain fixed. Use existing isolation, inline work, no subagents,
Python or SDK installs. User owns integration/publication; Grep remains last.
