# Caller-selected empty-match rejection

## Intent and scope

Continue issue #7 after merged PR248 by exposing default-false internal
`PcreByteOptions.NotEmpty` and `NotEmptyAtStart`. These are the native match
policies PCRE2_NOTEMPTY (4) and PCRE2_NOTEMPTY_ATSTART (8), selected once for an
immutable compiled internal pattern. They are not new PCRE compile-option bits
or a public per-call match-options API. Native reference fixtures pass these
bits to Match, never to Compile. Source directives remain independent inputs.

The library still builds for net8.0/net9.0/net10.0 in C#13; execute test cases
only on net10.0 across Linux/Windows/macOS x64/ARM64, as requested in PR248.
No SDK installation, Python, subagents or approval pauses. User owns merge,
tag and publication. Grep migrates last; issues #7 and #102 remain open.

## Approaches and choice

Initialize the two existing start-setting booleans from caller options, then
reuse the existing directive parser and program/matcher acceptance path. This
is the chosen approach: it preserves original source ownership, diagnostics,
instruction/capture/depth capacity, scoped success and work accounting.
Prepending synthetic directives would break Literal and source offsets. A new
match-time API or second acceptance path would duplicate policy and expand
the provisional public architecture before that roadmap gate is ready.
Standing authorization covers this bounded extension and inline execution.

## Contract

- NotEmpty rejects a result whose reported group0 endpoints are equal.
- NotEmptyAtStart rejects that result only when its end equals the original
  requested startOffset. Later empty results remain eligible. Reported K
  resets matter; consumed input does not substitute for reported endpoints.
- Both flags may be set; NotEmpty is stronger. Source NOTEMPTY/ATSTART
  directives OR into caller policy and may not unset it. Local option reset,
  recursion and subroutine scopes do not change either global policy.
- Reject only final root acceptance, allowing existing saved paths to retry.
  Inner assertions/callee ACCEPT keep their existing scoped behavior. Cuts,
  labels, captures and candidate restart retain their existing semantics.
- Anchored, EndAnchored, FirstLine, Literal, casing, word/line, newline/BSR,
  strict UTF and tolerant fragments compose through the existing paths.
  Literal permits these match policies; literal-looking directives stay data.
- Cancellation, nonpositive work limits, source validation, subject validation
  and invalid offsets retain their established priority. No early empty
  rejection may bypass validation. No new work or structural quota is spent
  merely for selecting the flags; rejected paths still spend normal VM work.
- Default flags remain false. Own source and selected booleans at compile
  time; later source/options mutation cannot alter a compiled pattern.

## Integration and proof

Production scope: PcreByteOptions.cs (two bool properties) and
PcreByteStartSettings.cs (two caller initializers). The existing settings
constructor carries these values into immutable PcreByteProgram metadata.
No parser body, IR, opcode, matcher, journal, shared resource limit, exported
API, dependency, packaging or workflow change is planned.

PcreCallerEmptyMatchReferenceTests adds 841 cases: 754 protocol groups with
26,212 independently preflighted native10.48/JIT0 matching/offset protocols,
60 preserved compile diagnostics and 27 work/ownership/validation/capacity
cases. Validate every native batch before managed comparison. Keep 462 native
strict split-offset(-36) research rows outside the matching grid; strict
subject/offset diagnostics have separate tests. Native no-match requires -1.
Native spans use EndIndex-Index, retaining signed reset results, all captures
and successful labels. Failed/partial labels remain a separate roadmap gate.

Tests-only RED is expected to expose 538 missing-policy groups, six retry-work
cases and three ownership cases: 547 failures, 87,149 passes, 87,696 total.
All 86,855 earlier tests must pass. Freeze the new fixture after clean RED,
before production; keep all 265 prior test files (260 C# files) hash-identical.
Preserve frozen35 source blobs and 214 outcomes/120 methods per native result.

All seven inventories precede final source qualification. Require six fresh
net10 results at 87,696 each, zero failures/skips/compiler warnings/errors,
all three library outputs on every native host and every configured supporting
gate. Final review/GREEN/source/base/tree/proposed-merge identities belong on
PR249 and only issue7 footer, without a documentation-only CI repeat.

## Remaining work

UTF code-unit execution, callouts, remaining syntax/directives and match/API
choices, tolerant-surrogate exclusions, failed/partial labels, broader native
optimizer/resources (#102), UTF16/exported API, realistic consumer/release
gates and final Grep migration remain open. This is scoped component evidence.

## Tests-first checkpoint

Tests-only [RED757](https://github.com/uniblab/Icod.Expressions/actions/runs/43606687443), source `77824ef8a9f0e3c8b45fd2234ac729f1aedee165`, macOS ARM64 job132482835413: net10 built with zero compiler warnings/errors and ran **547 expected failures / 87,149 passes / 87,696 total / zero skips**. Exactly 538 missing-policy protocol groups, six retry-work and three ownership cases failed after successful native batches; all 86,855 earlier tests and 294 new controls passed. Freeze `e99bbdac4a0c2c33c22d1ffe1234bad7dc25de22` before production. Before clean RED, coverage review added 36 groups/1,300 protocols for assertion resets and caller-only local reset, retaining all original 805 expectations.

Final six-result qualification/review/identities are recorded externally on PR249/issue7 after this source head.

## Qualification finding and corrected tests-only baseline

Initial production CI758/sourcefaf1a8c90a5ec839f1d8daede853aa6451e9e9a2 built with zero warnings/errors; Linux ARM132485083628 ran10 failures/87,686 passes/87,696 total/zero skips. Four unchanged native protocol groups expose lookahead-K plus terminal greedy optional-literal backtracking under NotEmpty. Six work cases contain a harness error: the exact-budget run appends to the observer list before the exhausted budget reads Count again. Snapshot the first count before either run; all assertions, case identities and native expectations remain unchanged. The original freeze cannot certify that faulty later assertion, so restart the tests-only baseline and freeze the corrected measurement before production. Keep all prior265 files fixed; broaden production only after native root-cause proof and corrected clean RED.
