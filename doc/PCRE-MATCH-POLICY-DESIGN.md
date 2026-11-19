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

The existing bounded direct-root terminal optional-literal proof currently runs
only for compiled NOTEMPTY policies. Retain that exact shape/NO_AUTO_POSSESS/target
guard but record eligibility independently of current empty policy; activate its
existing fallback pruning only when the effective compiled-or-call policy needs
it. This is required for assertion-K/greedy-query native parity. Default calls
retain their original fallback, and no program cloning or new instruction occurs.

## Native-first catalog and tests

Original pinned [native8 C probe](../tests/Pcre/Native/match-policy-oracle.md)
records all56 definitions times all64 policy masks:3,584 unfiltered outcomes,
1,874 matches/1,710 misses/80 reversed ranges, all JIT0/positive size. Raw SHA256
`75168c42dd34448d1c4119dccf2193e8a6a27d084789e5ca427d795217cf57d3`.
Record exact capture participation, signed spans and successful marks. Includes
raw NUL/high bytes, strict/tolerant UTF, split tolerant entry, UCP, case/literal/
newline/first-line modes, compiled defaults, source empty policies, ordinary/
early ACCEPT, assertions/reset, SKIP/COMMIT, duplicate names and branch-reset.

56 theories assert all64 independent native outcomes before the missing overload
is reached. A typed package consumer compiles once per definition, overwrites its
caller pattern, uses padded subject views, loops all64 policies and interleaves
the unchanged old entry. Check metadata identity and unchanged default outcomes
after every policy. Expected values never come from the production matcher.

Eleven additional cases check invalid offsets(two), malformed strict input,
strict split starts, pre/running cancellation, exact compiled/runtime work limits,
original-view observation, explicit-default allocation parity,64 concurrent calls
and retained earlier metadata/captures. Total67 additions /106,446 suite cases.
Cancellation propagates; diagnostics stay distinct from no-match. The observed
view and allocation controls are bounded evidence, not a whole release claim.

Publish successful-build tests-only RED with the overload throwing an explicit
NotImplementedException and the policy value fully typed. Require exactly67
missing-overload failures and106,379 passes, zero skips/coded warnings/errors,
and no native mismatch. Reconcile exact identities/causes, then freeze the new
catalog/consumer/assertion/C/XML sources before execution implementation. The
only old-test-file exception adds two Compile links in the test project XML.

## Package and qualification

Append a sixth structured result requiring3,584 protocols; retain all five old
package records and helper/data/assertion blobs. Build library/consumer net8/9/10;
execute real suites/consumers net10 only on six native hosts. Reuse existing
package action/call sites, isolated feed/cache and full compatibility/Gnulib gates.
Do not change dependencies, version, root projects, workflows or release policy.

Review complete branch and baseline five inventories plus this design/plan/small
status addendum before final qualification. Keep large syntax/resource/Grep
inventories base-identical and link the new addendum from roadmap/recent progress.
Require106,446 full/frozen214 cases/120 methods, six package records, all target
outputs and actual configured support/script gates, zero failures/skips/coded
warnings/errors. Record raw allocation totals and exact identities/results/plan
completion externally on PR288; no source/docs/test edits after final GREEN.
Clean only exact disposable pcre288-research after evidence readback, retain repo
and original roadmap, and observe ready/open/unmerged readback. User merges.

## Initial checkpoint and pre-freeze extension

Initial tests-only source5e996457a8e5e67aa3878fc283af1da8a8838927,
tree4d14cc0dc9d9c01cab60dc32c450dc895e22993d, proposed
merge29079c4c9af2b0575e41908c6eb808bcbe8034bd, run44537162508,
Linux ARM64 job135605523578 builds net8/9/10 with zero coded warnings/errors.
It reports exactly59 unused-overload failures/106,379 passes/zero skips/106,438
total. All48 native-definition batches and old-default comparisons pass first;
all failures reach the explicit missing-policy overload, including two expected
cancellation exception-type mismatches. No native mismatch or old failure exists.

Before freezing or implementation, review of the existing compiled empty-policy
proof adds eight optional-query/K/NO_AUTO_POSSESS definitions. Retain every
original definition/assertion/outcome unchanged. All original3,072 raw native
lines are an exact prefix of the new3,584-line output; new totals above include
this addition. The same assertion source now discovers56 definitions, giving
67 tests/106,446 total. Require a fresh refined successful-build RED67 and freeze
the expanded catalog before execution implementation. The initial checkpoint
does not claim to qualify these additional cases.
