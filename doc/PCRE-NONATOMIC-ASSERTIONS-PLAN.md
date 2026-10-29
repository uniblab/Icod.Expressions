# Internal PCRE non-atomic assertions implementation plan

> **For agentic workers:** Use superpowers:executing-plans for integration,
> a focused fixture worker and fresh read-only whole-branch review.

**Goal:** Implement resumable positive ahead/behind with all six spellings.

**Architecture:** Existing assertion nodes/instructions gain non-atomic metadata;
retained body frames share the journal. Stable integer assertion IDs into a
LIFO scope arena restore context on retry, bounded by existing live guards.

**Tech Stack:** C#13; net8/net9/net10; xUnit; PCRE.NET1.6.0 / PCRE2 10.48 JIT.

**Spec:** `doc/PCRE-NONATOMIC-ASSERTIONS-DESIGN.md`.

## Global constraints

- Root project/solution, src source, tests tests; UTF-8 LF; no Python.
- Local Debug, six PR Staging platforms; no local SDK, CI red/green.
- Preserve depth256, capture256, instructions4096, saved frames1024, work1,000,000.
- Scope count <=live guards<=1024; only integer/enumeration saved-frame data.
- No matcher recursion, capture snapshots, extra engine, public API/version/
  dependency/consumer/resource-policy change.

## Review focus

1. Retry restores scope ID and original behind end cap after success hid them.
2. Nested/sibling inactive scope lifetime follows every guard pop/prune exactly.
3. Capture/mark rollback and atomic/negative/possessive commitments stay correct.
4. Metadata survives literal avoidance, named fixups and deferred behind rewriting.
5. Retained guards share saved-path/work/cancellation limits; arena cannot grow
   with lifetime successes after guards are pruned.

### Task 1: Reference contract and inventories

**Files:** Create `tests/Pcre/PcreNonAtomicAssertionReferenceTests.cs`; replace
obsolete non-atomic-pending rows; update current and historical inventories.

**Interfaces:** Existing internal Compile/Match/options and pinned native JIT;
explicit selected outcomes/group counts then all numbered source-byte spans.

- [ ] Cover design verification classes and all five review risks, with explicit
  atomic/no-retry controls and inner-retry versus outer-rewind capture spans.
- [ ] Publish test-first draft from merged #87. Observe missing-syntax red CI,
  inspect native assertions and record exact evidence before production edits.

### Task 2: Metadata and bounded resumable scopes

**Files:** `src/Pcre/PcreByteSyntax.cs`, `PcreByteParser.Groups.cs`,
`PcreByteParser.GroupAliases.cs`, `PcreByteParser.LookbehindResolution.cs`,
`PcreByteProgram.cs`, `PcreByteMatcher.cs`.

**Interfaces:** optional bool NonAtomic=false on lookahead/behind nodes and group
frame; DeferLookbehind optional nonAtomic; CompileAssertion optional nonAtomic,
flag4 on entry/exit; FrameKind.NonAtomicAssertion; Frame.AssertionScope replaces depth field;
AssertionScope(Boundary,LimitEnd,Parent,ReturnPc,Position); private PopFrame
handles stack pop plus LIFO guard-scope release. Active scope ID=-1 initially.

- [ ] Parse six exact forms, preserve current contexts/limits, bypass literal
  shortcut for non-atomic bodies and retain metadata through behind finalization.
- [ ] Emit non-atomic flag through existing assertion lowering, preserving width
  branch/reverse and direct quantifier machinery.
- [ ] Convert every saved frame/scope lookup to stable IDs; guard checkpoints
  save parent ID, other frames save active ID. PopFrame releases guard scope.
- [ ] Preserve body retries on non-atomic success, hide scope via Parent, and
  restore frame IDs on retry; keep journal/atomic rollback and guard polarity.
- [ ] Route every pop/prune through PopFrame; preserve candidate clearing and
  unchanged atomic/negative behavior with bounded arena storage.
- [ ] Whitespace/frozen checks; publish implementation and inspect CI. Resolve
  concrete failures with focused red/green evidence.
- [ ] Fresh whole-branch review; fix findings, inspect exact final head/tree and
  six green Staging jobs/full/frozen counts/configured gates. Finalize PR ready;
  user retains merge control.
