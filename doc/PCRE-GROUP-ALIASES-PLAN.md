# Internal PCRE group aliases implementation plan

> **For agentic workers:** Use superpowers:executing-plans for integration,
> a focused fixture worker and fresh read-only whole-branch review.

**Goal:** Accept nine atomic-group/atomic-lookaround aliases with shared semantics.

**Architecture:** A transactional exact byte-prefix reader selects existing atomic
and signed assertion metadata; the original source indexes and heap group frames
remain authoritative. No semantic lowering or VM changes.

**Tech Stack:** C#13; net8/net9/net10; xUnit; PCRE.NET1.6.0 / PCRE2 10.48 JIT.

**Spec:** `doc/PCRE-GROUP-ALIASES-DESIGN.md`.

## Global constraints

- Root project/solution, src source, tests tests; UTF-8 LF; no Python.
- Local Debug, six PR Staging OS/architecture jobs; no local SDK, CI red/green.
- Preserve instructions4096, depth256, capture256, saved paths1024, work1,000,000.
- No VM/IR/opcode/frame/journal/API/version/dependency/consumer/resource-policy change.
- Preserve all local options, assertion optimization and deferred width rules.

## Review focus

1. Exact colon-terminated headers only; no keyword prefix/case/trivia acceptance.
2. Non-atomic assertions/directives/verbs never silently become atomic aliases.
3. Each alias consumes one depth level, no capture slot, and restores options.
4. Original byte opener and enclosing assertion context survive header consumption.
5. Aliases retain canonical commitment, quantifier and width/capture transactions.

### Task 1: Native contract, regression fixtures and inventories

**Files:** Create `tests/Pcre/PcreGroupAliasReferenceTests.cs`; replace obsolete
alias-pending rows in existing syntax/option/branch-reset/lookaround fixtures;
update current inventories and relevant historical design notices.

**Interfaces:** Existing Compile/Match/options and native Compiled/JIT, independent
expected outcomes/group counts and all capture spans, plus canonical comparisons.

- [ ] Cover all nine aliases and the design's semantic/diagnostic/resource classes,
  with positive and negative controls for all five review risks.
- [ ] Publish test-first draft from merged #86; inspect expected missing-alias red
  CI failures and confirm native expected outcomes before production edits.

### Task 2: Exact alias recognition and shared group construction

**Files:** Create `src/Pcre/PcreByteParser.GroupAliases.cs`; modify
`src/Pcre/PcreByteParser.Groups.cs` only for group-header integration.

**Interfaces:** private bool TryParseGroupAlias(out bool atomic,
out int assertionKind), advancing index past a recognized colon and preserving
it on failure; existing GroupFrame, CurrentGroupOptions and depth guards.

- [ ] Recognize the exact nine prefixes in the design table, with bounded
  cancellation polling and no temporary decoded strings.
- [ ] Snapshot entry options before header processing; aliases suppress capturing,
  retain atomic/kind and original assertion context, then share final guards/frame.
- [ ] Preserve existing unsupported directive fallback, direct behind diagnostics,
  local option restoration and native stack independence.
- [ ] Whitespace/frozen checks; publish implementation, inspect CI and resolve
  concrete failures with focused red/green evidence.
- [ ] Fresh whole-branch review; fix findings and verify exact final head/tree,
  all six Staging jobs/full/frozen counts/configured gates. Finalize PR ready;
  user retains merge control.
