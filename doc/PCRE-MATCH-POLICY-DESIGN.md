# Per-call PCRE match policy

Base merged PR287 `c04c698a671cc373c2a77e1ffbb52444ba97b989`, tree
`ce94160847b545f90a597d338b6d3684389aeb00`;106,379 prior tests.
Advance P09's internal compile-versus-match contract from Revision4's API
architecture critique and P09 page39. The whole P09 freeze, native16,
P10-P18, remaining P00/P01/Stage C and actual migrations remain open.
No subagents/Python/local SDK installation. Keep old C# tests/A05/#7/#102/footer
fixed. Grep last; user owns merge/tags/publication.

## Internal contract

Add an immutable value PcreMatchPolicy with six additive booleans: Anchored,
EndAnchored, NotEmpty, NotEmptyAtStart, NotBol, NotEol. Add a typed Match overload
taking this value, an original subject memory view, native byte start and token.
The original Match overload keeps its behavior and delegates with default policy.
Per-call restrictions combine with compiled/source restrictions; false never
undoes a compiled/source restriction. Calls neither mutate nor clone the program.
No new public API, dynamic work quota, cache, enumeration, or replacement engine.

Anchor and empty controls reuse existing candidate/acceptance logic, including
tolerant empty fast paths, both ordinary/early ACCEPT exits, physical end checks,
and group-zero signed endpoints. Full validation/cancellation/work charging and
original fragment boundaries still precede successful execution. Runtime anchoring
must participate in existing fragment retention and accelerator eligibility.

NotBol/NotEol affect caret/dollar, including their implicit MatchLine anchors,
not absolute A/Z/z/G anchors. Multiline policies suppress only the physical subject
boundary while retaining internal newlines. Non-multiline dollar including its
before-final-newline alternative is wholly suppressed by NotEol. Preserve source
provenance in the two relevant AST kinds and spare instruction operands so runtime
flags can distinguish caret/dollar from otherwise identical absolute instructions.
No new opcode or instruction count, no proof redesign and no changed Step count.
Compiled disabled anchors retain their existing Fail lowering. Default policy
retains all old instruction semantics and work/limit behavior.

## Native-first catalog and tests

Original pinned [native8 C probe](../tests/Pcre/Native/match-policy-oracle.md)
records all48 definitions times all64 policy masks:3,072 unfiltered outcomes,
1,522 matches/1,550 misses/32 reversed ranges, all JIT0/positive size. Raw SHA256
`a9a8f22f972a03aba26a73e9ed33dcbf70ca61caaabe5d5fe2a87f9a33d06283`.
Record exact capture participation, signed spans and successful marks. Includes
raw NUL/high bytes, strict/tolerant UTF, split tolerant entry, UCP, case/literal/
newline/first-line modes, compiled defaults, source empty policies, ordinary/
early ACCEPT, assertions/reset, SKIP/COMMIT, duplicate names and branch-reset.

48 theories assert all64 independent native outcomes before the missing overload
is reached. A typed package consumer compiles once per definition, overwrites its
caller pattern, uses padded subject views, loops all64 policies and interleaves
the unchanged old entry. Check metadata identity and unchanged default outcomes
after every policy. Expected values never come from the production matcher.

Eleven additional cases check invalid offsets(two), malformed strict input,
strict split starts, pre/running cancellation, exact compiled/runtime work limits,
original-view observation, explicit-default allocation parity,64 concurrent calls
and retained earlier metadata/captures. Total59 additions /106,438 suite cases.
Cancellation propagates; diagnostics stay distinct from no-match. The observed
view and allocation controls are bounded evidence, not a whole release claim.

Publish successful-build tests-only RED with the overload throwing an explicit
NotImplementedException and the policy value fully typed. Require exactly59
missing-overload failures and106,379 passes, zero skips/coded warnings/errors,
and no native mismatch. Reconcile exact identities/causes, then freeze the new
catalog/consumer/assertion/C/XML sources before execution implementation. The
only old-test-file exception adds two Compile links in the test project XML.

## Package and qualification

Append a sixth structured result requiring3,072 protocols; retain all five old
package records and helper/data/assertion blobs. Build library/consumer net8/9/10;
execute real suites/consumers net10 only on six native hosts. Reuse existing
package action/call sites, isolated feed/cache and full compatibility/Gnulib gates.
Do not change dependencies, version, root projects, workflows or release policy.

Review complete branch and baseline five inventories plus this design/plan/small
status addendum before final qualification. Keep large syntax/resource/Grep
inventories base-identical and link the new addendum from roadmap/recent progress.
Require106,438 full/frozen214 cases/120 methods, six package records, all target
outputs and actual configured support/script gates, zero failures/skips/coded
warnings/errors. Record raw allocation totals and exact identities/results/plan
completion externally on PR288; no source/docs/test edits after final GREEN.
Clean only exact disposable pcre288-research after evidence readback, retain repo
and original roadmap, and observe ready/open/unmerged readback. User merges.
