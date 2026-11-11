# Scope-aware PRUNE backtracking

PR216 follows merged PR215. Add exact no-argument `(*PRUNE)` to the existing
managed byte/UTF8 engine. It succeeds without consuming, then cuts earlier
choices only if later failure reaches the saved verb. Root cuts resume normal
candidate advancement; choices to the right may retry normally.

## Scope and architecture

Use one AST leaf/opcode and a saved Prune frame in the existing1024-frame
stack. At entry, find the nearest active call, negative assertion or assertion
condition. Standalone positive assertions do not intercept PRUNE, including
non-atomic assertions. A successful atomic group/assertion prunes its interior
verb frames naturally. On trigger, discard frames to the selected guard, then
reuse ordinary call failure, assertion exhaustion or candidate failure.
Inactive retained call/non-atomic guards are not boundaries: select from active
scope IDs and their saved stack depths. Record assertion guard kind alongside
its existing arena metadata. All scope walks/discards charge work/cancellation;
no separate stack, recursive unwind, fallback or public API.

Alternatives: treating PRUNE as an atomic group misses alternatives to its left;
clearing the whole stack misses negative/conditional assertion and call catches.
Existing guard metadata is the implementation choice under the standing inline
roadmap authorization.

Native JIT has a qualified returned-capture edge: when PRUNE forces a called
group to fail, selected values completed before the verb survive into caller
fallback, including negative/conditional caller assertions. Ordinary completed
captures to the right of PRUNE roll back first; unselected fields always restore.
Preserve only selected value entries in that callee journal interval while
popping the callee guard. Earlier caller checkpoints/fields remain intact.
Calls caught by an inner negative/conditional assertion use ordinary rollback.
This matches pinned native JIT; interpreter results can differ here.

PRUNE has zero consumed width and does not terminate lookbehind width scanning.
Direct quantifiers are invalid; grouped quantifiers use existing lowering.
Quotes, brackets and x-mode headers retain their existing token rules. Named
arguments (including an empty colon), other control verbs and MARK remain
explicitly unsupported. Existing scalar/fragment/newline selection and
optimizer proofs remain conservative; no general optimizer expansion.

## Evidence and gates

Pinned PCRE2 10.48 documentation, matching/JIT source and independent native
probes define the contract. Tests pin root bumpalong/capture spans, alternatives,
atomic/non-atomic/negative/conditional/nested assertions, behind, calls/recursion/
return lists, ACCEPT, K/NOTEMPTY, options/repeats/script runs, selected CRLF and
invalid-UTF offsets, direct-repeat errors, shared saved-frame/compile/work and
cancellation limits. Tests freeze after a clean successful-build RED; only
production changes may resolve behavioral mismatches.

C#13/net8/9/10, no new dependency or production native/System.Regex fallback,
GNU/BRE/ERE, frozen35 and frozen214/120 unchanged. No Python, installations or
subagents. Wider control verbs, UTF C, optimizer/resource conformance, UTF16/API,
realistic consumer performance and release stay open; Grep last.
