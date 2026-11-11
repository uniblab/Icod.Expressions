# ACCEPT implementation plan

> Execute inline under the existing tests-first and immutable-oracle contract.

**Goal:** Native-compatible immediate successful scope termination.
**Architecture:** One parsed leaf and instruction; reuse existing assertion exits,
call returns, capture journal and final acceptance. **Tech:** C#13/net8/9/10.
**Spec:** [ACCEPT design](PCRE-ACCEPT-VERB-DESIGN.md).

## Global constraints

Byte spans; no runtime fallback, no dependency/public API changes; no Python,
SDK/PowerShell installation or subagents. Successful-build RED freezes all new
reference tests. Preserve 35 production blobs and 214 frozen results/120 methods.

## Review focus

- An inner assertion must stop before its enclosing call or capture.
- Accepted call choices must not retry after later caller failure.
- Root NOTEMPTY failure must restore synthetic capture closes.
- Unreachable width tails suppress only native width checks, not global errors.
- Early exit from atomic/repeat/script-run bodies must retain valid guard arenas.

### Task 1: qualify behavior and freeze RED tests

**Files:** create `tests/Pcre/PcreAcceptVerbReferenceTests.cs`; retarget ACCEPT
pending rows in `PcreSyntaxAcceptanceTests`/`PcreGroupAliasReferenceTests` to MARK.
**Interface:** existing compiler/options/result and PCRE.NET reference.

- [x] Qualify independent native capture-span literals and malformed cases.
- [x] Add tests for the design/review-focus paths and shared resource limits.
- [x] Publish tests/design/plan to a draft PR; require successful compilation and
expected unsupported-ACCEPT failures in Linux/macOS CI. Record and freeze test blob.

### Task 2: lower ACCEPT and execute scope success

**Files:** parser Groups/Lookbehind/LookbehindResolution, Syntax, Program, Matcher.
**Interface:** `PcreByteAccept` leaf; `EarlyAccept` instruction with immutable
capture numbers and patched assertion exit; call-entry scope/boundary/depth.

- [x] Parse exact ACCEPT and preserve ordinary quantifier validation/lowering.
- [x] Close lexical captures and patch assertion-local exit targets during heap
compilation. Admit direct ACCEPT in width termination and nullable metadata.
- [x] At runtime charge closes/pruning; select current assertion/call/root;
commit accepted scope choices, trim atomic nesting, reuse existing exit paths.
- [ ] Run the unchanged frozen tests and whole native/framework CI; correct only
production code for behavioral mismatches. Require all applicable checks green.

### Task 3: review and document

**Files:** syntax/resource/fixture inventories and roadmap status; issue7 footer.

- [ ] Review final diff for all five review-focus paths, budgets/arena lifetime,
new syntax boundaries, offsets and conservative optimizer behavior.
- [ ] Record RED/GREEN evidence and remaining gates. Verify exact proposed-merge
source tree and mark PR ready; user owns merge/tags/publication.

## Tests-first checkpoint

RED645 (run43004006751) compiled all three frameworks with zero warnings/errors
on Linux ARM64 and macOS ARM64. Each framework reported46266 total:67 expected
new ACCEPT failures,46199 passes,0 skipped. The74 additions include two
already-supported quoted/bracket controls, four unsupported-header controls and
one earlier-unbounded-width rejection. The test blob
`64e372d660775c84e81e1bb4a503bbaa5d155a44` is immutable after that RED.
Implementation proceeds against the same native expectations.
