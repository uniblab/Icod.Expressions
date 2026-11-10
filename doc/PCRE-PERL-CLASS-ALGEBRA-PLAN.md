# Perl class algebra implementation plan

Goal/spec: doc/PCRE-PERL-CLASS-ALGEBRA-DESIGN.md. Inline execution, no subagents.
C#13/net8/9/10, UTF8+LF. No Python/SDK/PowerShell installation, new package or
production fallback. Preserve frozen35 production blobs and frozen214/120 tests.
Baseline46017 tests per framework. User owns merge/tag/publication; Grep last.

1. Publish native fixture and focused test-only draft. Require successful-build
   RED from unsupported extended classes; fix new test/data blobs after RED.
2. Add immutable postfix terms, bounded parser and byte projection. Reuse
   bracket/property readers without changing ordinary grammar. Add single-scalar
   UTF opcode with bounded per-Match scratch; charge all evaluation work and
   cancellation. Audit width, erased nodes and explicit-newline metadata.
3. Update capability/coverage/support inventories and roadmap. Verify full
   applicable matrix,18 native framework results, frozen/extraction/API/package,
   build scripts, all Unicode regenerators and workloads. Author review all
   changes and exact proposed merge tree, record evidence, mark ready.

Review focus: operator precedence/left associativity, operand case folding,
class-level versus predicate complement, mixed Any/property byte projection,
local whitespace/quote scope, invalid source offsets, nesting and flat term
budgets, shared compilation budget, cancellation inside one atom, malformed
fragments and candidate search metadata. UTS#18 activation remains pending.

## Execution evidence

Test-only910b03c / RED636 run42915560079, Linux x64 job130176944661:
all frameworks compile with0 warnings/errors; each suite46069 has52 expected
new failures,46017 baseline passes,0skips. Test blob
fa40f9c570bb7d80b76beb8f6c10f82d7a715059 and fixture blob
cf8ce1099aa863199816e218a24644cd50227b1e remain fixed after RED.
