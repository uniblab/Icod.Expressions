# Named PRUNE, COMMIT and THEN design

Continue issue7 after merged221 by accepting names on the three existing
delayed cuts. Pinned PCRE2 10.48 JIT (7978954dbd2efc6f2196869290553cf1871b4ce6,
PCRE.NET1.6.0) reports their latest successful-path label. Their names are not
MARK history and must never become named-SKIP targets. Failed/partial-attempt
label reporting and other verb families remain separate gates.

## Implementation choice

Reuse the existing three cut nodes with optional raw-name memory, the MARK/SKIP
raw-name lexer, successful label slot/journal and cut opcodes. Preserve one
instruction transition and one saved cut frame per named cut. A separate MARK
node would add searchable history and could select the wrong restart target;
a separate label instruction would add work and expansion cost unnecessarily.

The compiler owns the source snapshot. Default names are ordinal raw bytes up
to255 bytes; empty arguments mean unnamed cuts and keep the previous label.
A nonempty raw name beginning with NUL is still named, with an empty visible
label. Reject direct repeats, keep byte diagnostics and zero-width width rules.
Named THEN still activates lexical alternation ownership globally.

Emit immutable NUL-trimmed bytes and label-slot-plus-one in each named cut's
PropertyValue operand (zero for unnamed). Lazily allocate the existing shared
PathMarkSlot. After saving the cut checkpoint, journal/write its instruction+1
as the current successful label. Accept already resolves that instruction's
Bytes. Ordinary/atomic/assertion/callee rollback and successful Return use the
existing journal rules; MARK history frames remain exclusive to actual MARK.

## Native visibility and bounds

In named-SKIP programs, native atomic assertions with lexical named PRUNE/COMMIT
also clear inherited control history. Extend the existing bounded heap metadata
walk to those nonempty raw arguments; THEN already clears history, named or not.
Nested/erased/DEFINE bodies participate, called bodies do not. Non-atomic and
read-only assertion rules, local cuts, selected capture retention and rightmost
cut precedence remain shared. Existing bounded COMMIT entry and PRUNE literal
cache proofs recognize the same typed cuts; issue102 remains open.

Keep1024 saved frames,256 marks,4096 compilation bounds, work and cancellation.
Names must add neither lookup history frames nor a second label slot. The tests
also compare charged transitions with equivalent unnamed cuts. No exported API,
dependency, GNU/BRE/ERE, production fallback or engine architecture change.

## Tests and reference ruling

543 cases include502 literal/profile fixtures independently checked in988 native
executions, plus offset/UTF/CRLF, grammar/raw/NUL/length, scope/capture, history,
quota/expansion/work/cancellation and immutable ownership checks. Native-only
NO_JIT on large all-success quota controls avoids the native default JIT-stack
ceiling; scoped cuts use the pinned JIT oracle. Failed-match native label values
are intentionally excluded because that metadata is not implemented yet.

Six old named/empty-header rejection rows across the PRUNE/COMMIT/THEN files are
obsolete managed capability inventory; retarget them to nonexact headers while
preserving all native matching/capture expectations byte-for-byte outside those
blocks. Three group-alias pending rows and six syntax-inventory uses advance to
still-pending named ACCEPT. The native characterization retains its0:1 result;
its managed opening-byte inventory moves with the new pending input. New and
revised references freeze after clean-build RED, before production changes.
Historical GNU/frozen35/frozen214/120 and earlier native expectations stay fixed.

Use the existing isolated worktree and inline execution, without subagents,
Python or local SDK installation. User owns merges/tags/publication; Grep last.
