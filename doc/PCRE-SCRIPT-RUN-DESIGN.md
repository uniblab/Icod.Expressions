# Native-compatible script-run groups

Implement internal (*script_run:...)/(*sr:...) and
(*atomic_script_run:...)/(*asr:...) over authoritative byte/UTF8 subjects.
Validation checks the consumed region, not captures or match-start resets.
Empty/single-scalar spans are valid even for Unknown; longer spans reject
Unknown, intersect primary-or-extension script maps excluding Common/Inherited,
follow pinned native Han transitions, and keep decimal digits in one set of10.
Native10.48 has observable directional Han edge cases; preserve them rather
than substituting a generic Unicode security algorithm. Case/UCP/ASCII flags
change the body normally but never the subject-property validation itself.

Use a transparent AST wrapper, existing heap group frames and journaled marks.
Emit entry Mark, body, then ScriptRunCheck; validation failure retries the body.
For ASR put the existing atomic wrapper INSIDE the script-run node, so it commits
before validation. A mark restored by call/branch/assertion rollback preserves
outer starts through recursion. Reuse the256 shared mark budget and4096 compile
budget; charge every scalar decode/classification and cancellation in the scan.
No scan allocations or new matcher checkpoint kind. Width and all parser tree
rewrites traverse the wrapper; conservative optimizer proofs stay conservative.

Extend GeneratePcreUnicodeScripts using its existing three source hashes and
native whole-scalar fingerprint: derive301 run descriptors and1892 start ranges
with primary ID and three ulong script-map words. Runtime binary searches this
compressed pinned data. Derive decimal set zero from the pinned general-category
range and offset modulo10, covering all77 digit families including adjacent
families. No host Unicode, native runtime fallback or new package.

This approach reuses validated state/Unicode ownership. Per-scalar scanning of
all175 script tables would add avoidable work; a separate run VM/journal would
duplicate rollback machinery. Follow standing implementation authorization.
Preserve frozen35/frozen214/120, earlier test/data blobs, GNU and public APIs.
Grep migration remains last; other matrix/API/performance/release gates remain.

Independent native JIT masks cover301 descriptor pairs,5111 boundary scalars in
both orders against20 contexts,77x77 digit-set pairs at zero/nine endpoints,
20-cubed context triples and every byte pair. Live PCRE.NET also checks masks.
Focused tests pin all four aliases, empty/single rules, rollback/recursion,
atomic validation placement, captures, behind, invalid UTF, source/state/work
limits and local options. The known native-oracle correction exception permits
only explicitly documented incorrect native assumptions exposed in CI.
