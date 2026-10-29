# Internal PCRE assertion conditions implementation plan

> **For agentic workers:** Use superpowers:executing-plans for integration,
> one focused fixture worker and one fresh read-only whole-branch review.

**Goal:** Support traditional atomic lookahead/lookbehind conditions internally.

**Architecture:** Heap nested assertion parsing plus explicit yes/no lowering;
existing guard/journal/arena gets a conditional kind without new payload fields.

**Tech Stack:** C#13; net8/net9/net10; xUnit; PCRE.NET1.6.0 / PCRE2 10.48 JIT.

**Spec:** `doc/PCRE-ASSERTION-CONDITIONS-DESIGN.md`.

## Global constraints

- Root project/solution, src, tests, samples; UTF-8 LF; no Python.
- Local Debug; six-platform PR Staging; no local SDK, observed CI red/green.
- Preserve depth256, capture256, instructions4096, saved paths1024, work1,000,000.
- No recursion/saved spans/capture snapshots/new frame or scope fields; no public
  API/version/dependency/release/consumer change. User owns merge.

## Review focus

1. Matching NEGATIVE body preserves captures in else; failed body rolls back.
2. Chosen branch failure cannot select its sibling; outer retries reevaluate.
3. Conditional guard/arena release remains LIFO under nested resumable/atomic
   success, body exhaustion, outer failure and repeated sibling conditions.
4. Condition adds zero behind width but nested behinds validate; omitted else
   affects runtime nullability independently from explicit native width analysis.
5. Both source nesting levels, options/names/numbering, K contexts and exact
   prefixes/invalid repeats preserve original byte diagnostics and resource gates.

### Task 1: Native contract and test-first fixtures

**Files:** Create `tests/Pcre/PcreAssertionConditionReferenceTests.cs`; update
`PcreCaptureConditionReferenceTests.cs` pending assertions; current/historical
syntax, design, resource and consumer inventories.

**Interfaces:** Existing Compile/Match/options, pinned JIT oracle and byte captures.

- [ ] Verify native outcomes and rejection dispositions for the review risks.
- [ ] Write fixture with explicit outcomes plus numbered capture comparison,
  nested transactions/widths/offsets and resource gates.
- [ ] Publish tests/docs only as draft after merged #89. Observe missing-feature
  red, inspect native assertions, record counts before production edits.

### Task 2: Parsing, traversal, compiler and VM integration

**Files:** Modify `src/Pcre/PcreByteSyntax.cs`, `PcreByteParser.Groups.cs`,
`PcreByteParser.TreeRewrite.cs`, `PcreByteParser.Lookbehind.cs`,
`PcreByteProgram.cs`, `PcreByteMatcher.cs`.

**Interfaces:** PcreByteAssertionConditional(Assertion,Body); GroupFrame marker,
inner condition flag and stored node. Compiler conditional assertion lowering
with flags8 and existing First/Second fields. ConditionalAssertion guard kind.

- [ ] Heap parse inner assertion without consuming a body capture or quantifier;
  preserve context, options and two-direct-branch validation; force general AST.
- [ ] Visit/rebuild assertion and branches; schedule assertion validation while
  calculating explicit branch widths, nullable else and newline metadata.
- [ ] Compile assertion body and two branch targets using heap continuations;
  existing reverse lowering, no conditional Split or added payload fields.
- [ ] Enter/complete/exhaust conditional guard using shared scopes/journal;
  preserve captures on body success for both signs; release arena through PopFrame.
- [ ] Local whitespace/frozen35, exact-tree publish; full CI, diagnose concrete
  failures and verify fixes with red/green evidence.
- [ ] Fresh whole-branch review, resolve findings, inspect exact final head/tree
  and all six green job logs/full/frozen/configured gates; PR ready for user merge.
