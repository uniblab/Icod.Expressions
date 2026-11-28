# Native 16-bit named SKIP

Following #370's bare SKIP, this increment adds `(*SKIP:NAME)` to the internal
bounded raw16 and strict UTF16 sequence profile. Names use the original owned
16-bit source, ordinal comparison and native NUL-prefix visibility. An empty raw
argument remains bare SKIP; a nonempty argument beginning with NUL is named SKIP
whose visible lookup name is empty. The existing 4,096-unit pattern limit applies.

## Retained history and dispatch

Only patterns containing a nonempty named SKIP save MARK history frames. These
share the existing 1,024-frame stack with alternatives, captures, calls and cuts.
A history frame records the subject position, original name, capture/label
checkpoint and enclosing scopes. Backtracking pops history without a retry edge.
Successful atomic scopes discard their history. Non-atomic positive assertions
and returned calls retain it. JIT root lookup hides returned-call marks; the
interpreter can resolve them. Named FAIL/ACCEPT expand to MARK history, while
named PRUNE/COMMIT/THEN write successful labels without becoming lookup marks.

Atomic assertions isolate inherited history when their lexical source contains
MARK, THEN or a named label-writing verb. Erased, DEFINE and statically unselected
bodies count; called definitions do not. This metadata is computed from source,
not from paths left after finite lowering. Non-atomic assertions inherit history.

JIT takes local assertion/call quits before resolving names. At root it searches
newest visible MARK first; a missing name continues normally. Interpreted negative
and conditional assertions resolve the name before deciding where failure is
caught. The matched MARK's enclosing assertion can become that boundary, while
an active call remains the outer limit. Unmatched names can propagate past an
assertion to the root driver.

## Ordered replay and anchoring

An unresolved interpreted root SKIP saves a replay checkpoint. If failure reaches
it, the driver retries the same candidate with every named SKIP visit seen so far
suppressed, including previously resolved names. Visits are counted in execution
order, not source order. Captures, labels and scopes start clean on each replay;
the work budget and cancellation token remain shared. Forward SKIP retains the
suppression prefix, and ordinary bumpalong clears it.

Compiled/call-time anchoring and native source-anchor evidence stop replay.
The existing byte source-anchor proof is extracted into a shared helper used by
both widths. It handles groups, alternatives, positive assertions, conditional
prefixes, mandatory repeats, erased prefixes and named control prefixes without
following call edges. This preserves the byte proof and its cancellation/visit
bounds. Bare forward restart behavior remains as qualified in #370.

## Evidence and bounds

`PcreWordNamedSkipTests` contains 1,022 fixtures across eight compiled defaults
and all 64 call masks: 523,264 PCRE2-16 full capture-vector and successful-label
comparisons. Native preflight reports no compile or match errors. The matrix
includes recursive/selected calls, accepted-call labels, fixed/variable lookbehind,
source anchors, repeated replay, original-unit starts, supplementary characters,
raw surrogates, long names, NUL prefixes, CR/NUL newlines and mixed control verbs.

Supplemental tests check exact work budgets, cancellation at every observed
transition, reuse after cancellation/frame exhaustion, whole-source admission,
strict UTF validation, the pattern-length boundary, owned source/results, subject
slices and concurrent reuse. Existing unsupported-syntax sentinels move to script
runs; native reference expectations are unchanged. All 35 A05 source blobs remain
frozen. Managed build, test and package qualification is recorded on the PR.

Native reference: PCRE2 10.48, commit
`7978954dbd2efc6f2196869290553cf1871b4ce6`, with SLJIT
`de0259c7aaf36aa40cba8014f3fad3edde9307f9`.
Protocol SHA-256:
`0b68bcd29d0bc12ebb10b0f281ab140ccd19286995c9f0d5ce097fb1970c2a96`.

## Remaining scope

The next bounded word execution increment is `\K` match-start reset. Graphemes,
script runs, broader newline/option profiles, public word API/package consumers
and cross-width resource/performance qualification remain open. ACCEPT with
selected returns and asserted-only COMMIT retain their explicit qualification
gates. This increment does not declare the complete native width-16 engine done;
Grep integration remains last.
