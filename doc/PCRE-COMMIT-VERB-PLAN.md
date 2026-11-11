# COMMIT implementation plan

Execute inline, tests first. Goal: exact no-argument COMMIT with native scope,
root termination and qualified first-code-unit entry. Tech: C#13/net8/9/10.
Spec: [COMMIT design](PCRE-COMMIT-VERB-DESIGN.md).

## Constraints and review focus

Preserve GNU/BRE/ERE, frozen35/frozen214 and all earlier native reference tests.
No dependency/public API/fallback changes, installations, Python or subagents.
User owns merges/tags/publication. Review delayed/rightmost cut choice, local
catches and selected callee captures, UTF malformed lead-byte admission without
consumption, bounded metadata and shared cancellation/resource accounting.

## Task 1: qualify and freeze tests

Files: new PcreCommitVerbReferenceTests; retarget five obsolete unfrozen pending
COMMIT inventory rows. Interface: existing compiler/matcher and PCRE.NET oracle.

- [x] Export independent native byte/UTF literal captures, splitting profile differences.
- [x] Add all-offset UTF/newline, exact grammar and shared resource cases.
- [x] Publish draft tests/design/plan; require clean-build RED and freeze test blob.

## Task 2: implement and qualify

Files: Syntax, Groups/Lookbehind, Program, Matcher. Interface: Commit AST/opcode/
frame kind; optional immutable first-byte metadata; existing journals/guards.

- [x] Lower exact COMMIT, reject direct repeats; preserve behind/nullable analysis.
- [x] Reuse delayed cut and local catch logic; return normal no-match at root.
- [x] Add bounded, cancellable first-code-unit proof and charged candidate filtering.
- [x] Admit matching malformed lead bytes through zero-width fragment candidates.
- [x] Extend existing PRUNE cache only for direct preceding COMMIT if fixed control requires it.
- [ ] Run unchanged references and full applicable native CI to GREEN.

## Task 3: author review and ready

Files: syntax/resource/fixture inventories, roadmap status, issue7 footer.

- [ ] Review scope, rightmost precedence, captures, UTF entry and all bounds.
- [ ] Record exact RED/GREEN evidence; verify source tree and mark PR ready.

## Tests-first checkpoint

Initial RED652 built successfully but exposed three duplicate new InlineData
rows (xUnit1025). Remove only those redundant rows before the clean checkpoint;
no unique expectation changes. RED653 run43025191022 LinuxARM64 job130538242326
then compiled all frameworks with zero warnings/errors. Each reported46647
cases:142 missing-COMMIT failures,46505 passes,0 skips. No prior fixture or
native literal oracle failure. All150 unique new cases freeze at blob
`fba19c8e0627f89056b16302538e3c7da3639b9d` before production changes.

CI654 compiled cleanly and narrowed the implementation to one failing fixed
case: (?<=(a{1,2}(*COMMIT)b))c on aabc. Native first_significant_code skips
lookbehind before finding the outer c, so entry is byte3; NO_START_OPT instead
fails. The bounded proof now skips leading positive lookbehind bodies before
the required outer literal. All150 tests remain unchanged.
