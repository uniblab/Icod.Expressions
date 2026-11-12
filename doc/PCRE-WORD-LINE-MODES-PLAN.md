# Word and Line Compile Modes Implementation Plan

Execute inline with the established continuous workflow.
**Goal:** Add native word/line compilation through existing typed guards/scopes.
**Architecture:** Snapshot entry options before source headers, then wrap the
parsed root before semantic/call/width resolution. Existing nodes, bytecode and
matcher own semantics; line takes precedence. Source bytes are preserved.
**Tech Stack:** C#13/net8/9/10; managed RED/GREEN through GitHub CI.
**Spec:** [design](PCRE-WORD-LINE-MODES-DESIGN.md).

## Global constraints

Preserve earlier tests, GNU/BRE/ERE, 35 frozen source blobs and 214 frozen outcomes
in 120 methods, exported API and dependencies. UTF-8+LF. No Python, subagents,
local SDK installs or runtime fallback. Use the existing isolated worktree.
User merges/tags/publishes; Grep stays last.

## Review focus

1. Outer guard policy uses entry settings; local m/aW/unsets/reset stay inside.
2. All alternatives share the implicit group; cuts/ACCEPT/K retain native scope.
3. Whole-pattern recursion includes guards; named/numbered capture IDs stay fixed.
4. UTF/newline/fragment/start-offset and original diagnostic bytes stay native.
5. Option/source isolation and shared work/cancellation/quotas remain bounded.

## Task 1: references and freeze

Create tests/Pcre/PcreWordLineModeReferenceTests.cs against Compile/Match/MarkName
and PCRE.NET. Reflection permits tests-first builds; native extra bits derive
from selection directly, independently of absent properties. Use 1,543 cases: 1,480 literal
native profile rows plus exact legacy diagnostics, every byte, selected UTF/CRLF
start offsets, old caller policies, owned source/options/labels, full capture
capacity, canonical work/cancellation and source/instruction limits. Precompile
all supplemental native patterns before managed matching.

- [x] Independently qualify all 1,480 profiles with native JIT status zero.
- [ ] Publish tests-only draft and observe net8/9/10 expected RED with zero
  warnings/errors/skips. Defaults, exact diagnostics, earlier tests and all
  native literal/oracle checks must pass; inspect identities and assertion sites.
- [ ] Freeze the new reference hash before production edits.

## Task 2: wrap the semantic root and qualify

Modify src/Pcre/PcreByteOptions.cs with default-false MatchWord and MatchLine;
PcreByteParser.cs copies both readonly and snapshots CurrentGroupOptions before
the leading-option loop. After source diagnostics, before name/call/width
resolution, call a helper from new PcreByteParser.WordLineModes.cs.

Helper WrapWholeMatch(root, entry): return the identical root when both flags
are false. Otherwise build one root branch containing start guard, an existing
PcreByteNoncapturingGroup(root) with entry Caseless/CaselessRestricted metadata,
and end guard. Line uses entry.Multiline to choose line anchors or absolute
start/end-before-final-newline. Word uses ucp and entry AsciiRestrictions.BsW to
choose the existing Unicode/ASCII word boundary predicate. Line wins. Finish
subroutine targets only after wrapping, preserving root recursion. No parser
source offset changes, new AST/opcode or runtime matching flag.

- [ ] Implement those option/root-wrap paths after valid freeze.
- [ ] Qualify all 55,542 tests/framework in 18 native platform/framework results with zero failures,
  skips/build warnings/errors and frozen 214/120 per result.
- [ ] Verify frozen 35, Unicode generators, workload/parity/API/corpus/pack,
  package-only smoke and three-OS scripts; note the existing consumer classifier.

## Task 3: author review and readiness

- [ ] Review all five boundaries, unchanged earlier tests and default root identity.
- [ ] Verify final head/base/proposed merge tree and complete qualification receipts.
- [ ] Update syntax/resource/consumer docs, roadmap status/issue #7 and PR evidence.
- [ ] Mark ready and remove only this tranche ledger. User owns merge/publication.

## Baseline

PR229 merged at a590294fa81439096ba910a953db4be28331b967.
Main 43181932890 and scripts 43181932592 are green; 53,999 tests/framework.
Source c7b924676844f6286b4e058a7853548f1698b0f4,
tree 61031254a98ccc1ec27bfb11712cc30f6fe10551.
