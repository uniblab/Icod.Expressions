# Alternate Backslash Grammar Implementation Plan

Execute inline under the standing continuous workflow.
**Goal:** Honor default-false AltBsux through canonical literal parsing.
**Architecture:** Snapshot one caller flag; a bounded adjacent-digit reader
returns existing literals with original source indexes and validation order.
**Tech stack:** C#13/net8/9/10; local pinned native research, managed CI RED/GREEN.
**Spec:** [design](PCRE-ALT-BSUX-DESIGN.md).

## Global constraints

Baseline merged235 a5521bba5e4b15346ee0f972118c19c68a37c9a2,64,884 tests/framework;
main43374278672/scripts43374278547 green. Preserve249 earlier tests, frozen35/
214 outcomes/120 methods, GNU/BRE/ERE, matcher/API/dependencies/workflows/limits.
UTF8+LF; no Python/subagents/SDK installs; reuse isolated worktree; user merges,
tags and publishes; Grep last. EXTRA_ALT_BSUX and surrogate-escape policy remain
outside this increment.

## Review focus

1. Incomplete digits never consume partially or cause premature range errors.
2. Class/range and fallback braces retain existing diagnostic and repeat ownership.
3. Canonical literals retain captures, case scopes, assertions/calls and byte offsets.
4. Inert text and escape-produced backslashes cannot become new syntax.
5. Ownership/cancellation/quotas and earlier caller prohibitions remain shared.

## Task 1: native-first regression contract

Create tests/Pcre/PcreAltBsuxReferenceTests.cs with literal native match/error
profiles, default controls, all256 values and selected offset/newline/fragment
protocols. Native arrays compile before managed matching. Add default diagnostic,
ownership, canonical work/limit, caller-lockout and structural-boundary controls.
Use optional reflection for the new flag until it exists so RED builds normally.

- [x] Preflight every primary/supplemental native profile and expected case count.
- [ ] Publish tests-only draft; inspect net8/9/10 expected RED with no native,
  earlier or legacy-control failures. Freeze final fixture before production.

## Task 2: parser integration

- [ ] Add bool AltBsux init property in src/Pcre/PcreByteOptions.cs, default false.
- [ ] Snapshot a readonly bool in src/Pcre/PcreByteParser.cs. After reading the
  escape letter, route selected x/u/U to ParseAltBsuxEscape(byte value,int offset).
- [ ] Create src/Pcre/PcreByteParser.AltBsux.cs. U returns itself. For x/u,
  require2/4 adjacent hex digits with a local cursor and cancellation polls;
  incomplete/invalid sequences return the letter without consuming digits.
  On success commit index, check byte255/UTF-surrogate bounds, return a literal.
  Existing class-algebra ownership and global UTF/caller validation stay intact.
- [ ] All18 native results pass full/frozen suites with zero failures/skips/build
  warnings/errors; existing Unicode/workload/extraction/POSIX/API/corpus/frozen35/
  package/three smoke/three-OS script gates pass.

## Task 3: review and delivery

- [ ] Author review all five boundaries; independent review excluded by user.
- [ ] Update design/matrix/resource/consumer/status inventories and issue7 footer.
- [ ] Verify frozen/earlier hashes, clean tree and source/base/proposed-merge
  identity; record final GREEN/review on PR/issue7, mark ready and remove only this
  tranche ledger. User owns merge.

Native preflight:554 independently pinned primary profiles (494 accepted,60
diagnostics), plus1,115 supplemental JIT0 compile profiles (1,024 all-value,84
offset-pattern,6 structural/work/ownership and1 produced-slash control).
586 new managed cases yield65,470/framework. Independent native flag-off
comparison predicts410 primary differences;4 octet/12 offset theories and4
work/ownership/quota/caller-policy facts should add20 expected failures.
Expected RED430/65,040passes/65,470total per framework, with all249 earlier
files and default/UTF/native controls passing.

Initial tests-only e6daceb exposed four duplicate error rows after subject-only
profiles collapsed to identical compile diagnostics. Remove those redundant rows
and rerun warning-free RED before fixture freeze; production remains unchanged.
