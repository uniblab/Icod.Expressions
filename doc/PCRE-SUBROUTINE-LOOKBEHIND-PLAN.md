# Internal PCRE subroutine lookbehind implementation plan

> **For agentic workers:** Use superpowers:executing-plans for integration,
> one focused fixture worker and one fresh read-only whole-branch review.

**Goal:** Allow bounded nonrecursive calls to consume within lookbehind.

**Architecture:** First-definition call-target map in the global capture context;
shared heap min/max dependencies and existing reverse/call VM paths.

**Tech Stack:** C#13, net8/net9/net10, xUnit, PCRE.NET1.6.0/PCRE2 10.48 JIT.

**Spec:** `doc/PCRE-SUBROUTINE-LOOKBEHIND-DESIGN.md`.

## Global constraints

- Internal only, no API/package/version/dependency/workflow/consumer changes.
- Root project/solution, src, tests, samples; UTF-8 LF; no Python/native fallback.
- Depth256/captures256/instructions4096/marks256/savedframes1024/work1,000,000.
- Variable behind cap255; all-fixed and intermediate finite widths cap65535.
- Local Debug; no SDK here, executable red/green through six-platform Staging CI.
- Existing recursion/returned-capture/unresolved suppressed-behind gates remain.
- No opcode/matcher/frame/journal/compiler-lowering changes expected.

## Review focus

1. Calls select first definitions under duplicate names/branch reset while
   backreferences retain their separate restrictions and lexical context.
2. Mixed call/reference/nested-behind dependencies cannot hide cycles through
   assertions or zero counts, and report the owning original byte opener.
3. Target alternatives become ranges; a single variable top-level branch caps
   all maxima255, while fixed65535 and intermediate overflow remain distinct.
4. Definition bodies preserve root/nested capture restoration, non-atomic retry,
   JIT end caps, assertion guard lifetimes and caller/UTF byte offsets.
5. Unexecuted calls still validate/compile within shared limits; no unresolved
   suppressed assertion metadata becomes executable after width resolution.

### Task 1: Native contract, design and failing fixture

**Files:** Create the design/plan and
`tests/Pcre/PcreSubroutineLookbehindReferenceTests.cs`; replace obsolete pending
rows in `PcreSubroutineReferenceTests.cs`; update design/syntax/resource/consumer
inventories, preserving historical scope notices.

**Interfaces:** Existing internal Compile/Match/options and native compiled JIT.

- [ ] Native rules and actual design/plan builder check; explicit outcomes for
  all five review risks, including compact65535/65536 dependency fixtures.
- [ ] Focused native comparison fixture; original-byte diagnostics and shared
  compile/runtime/cancellation controls; no implementation-mirroring tests.
- [ ] Publish test/docs-only draft; inspect expected missing-feature failures
  in a completed CI job before production edits; record exact evidence.

### Task 2: First-target width dependencies

**Files:** Modify `src/Pcre/PcreByteParser.LookbehindResolution.cs` and
`src/Pcre/PcreByteParser.Lookbehind.cs`.

**Interfaces:** LookbehindContext.CallTargets Dictionary<int,PcreByteGroup>;
PcreByteSubroutineCall.Number schedules and reads that target's cached width.

- [ ] Populate first call definitions in the existing preorder map; keep
  backreference capture intervals/targets unchanged.
- [ ] Schedule/evaluate consumed call target group using the existing heap,
  cache, active cycle set and width checks; retain ahead-zero/nested validation.
- [ ] Whitespace/frozen35/exact-tree checks; publish and inspect full CI.
  Diagnose actual failures and fix with observed focused red/green evidence.
- [ ] Fresh whole-branch review; resolve findings, inspect exact final head/tree,
  all six full/frozen/configured gates; ready PR for the user's merge.
