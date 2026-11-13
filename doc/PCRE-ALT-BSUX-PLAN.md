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
- [x] Publish tests-only draft; inspect net8/9/10 expected RED with no native,
  earlier or legacy-control failures. Freeze final fixture before production.

## Task 2: parser integration

- [x] Add bool AltBsux init property in src/Pcre/PcreByteOptions.cs, default false.
- [x] Snapshot a readonly bool in src/Pcre/PcreByteParser.cs. After reading the
  escape letter, route selected x/u/U to ParseAltBsuxEscape(byte value,int offset).
- [x] Create src/Pcre/PcreByteParser.AltBsux.cs. U returns itself. For x/u,
  require2/4 adjacent hex digits with a local cursor and cancellation polls;
  incomplete/invalid sequences return the letter without consuming digits.
  On success commit index, check byte255/UTF-surrogate bounds, return a literal.
  Existing class-algebra ownership and global UTF/caller validation stay intact.
- [ ] All18 native results pass full/frozen suites with zero failures/skips/build
  warnings/errors; existing Unicode/workload/extraction/POSIX/API/corpus/frozen35/
  package/three smoke/three-OS script gates pass.

## Task 3: review and delivery

- [x] Author review all five boundaries; independent review excluded by user.
- [ ] Update design/matrix/resource/consumer/status inventories and issue7 footer.
- [ ] Verify frozen/earlier hashes, clean tree and source/base/proposed-merge
  identity; record final GREEN/review on PR/issue7, mark ready and remove only this
  tranche ledger. User owns merge.

Native preflight:554 unique profiles:491 accepted JIT,60 compile diagnostics and
3 strict-UTF subject errors; plus1,115 supplemental JIT0 compile profiles
(1,024 all-value,84 offset-pattern,6 structural/work/ownership and1 produced-slash).
586 cases yield65,470/framework. Corrected native comparison predicts410 primary
differences plus20 supplemental cases, all430 expected managed failures.

Clean refined tests-only [RED713](https://github.com/uniblab/Icod.Expressions/actions/runs/43380230552),
source523784aaad7838a9399f745e1d62662ad6ef5052, LinuxARM job131736180443,
built net8/9/10 with0warnings/errors. Each ran65,470 tests:430 expected managed
failures,65,040 passes,0skips. The410 primary differences and20 supplemental
cases failed only for missing alternate grammar; native/default/earlier/UTF
controls passed. Freeze fixture780b5a5583aaef908bcae74bbb4cea48e4cc0d35 before
reapplying production. Earlier RED711/freeze were superseded: three strict-UTF
subject errors were incorrectly called nonmatches. Those exact profiles now pin
native error-4 and managed InvalidUtf8Subject0; no span/compile-error expectation
was weakened. Four redundant compile-error rows were removed earlier. Reader
source is byte-identical to the initial implementation712.

Author review checked all five boundaries and confirmed that only the strict-UTF
protocol classification changed during refreeze. The reader remains unchanged.
Final all18-result qualification and supporting gates, source/base/merge identity,
issue7 footer and readiness are recorded on PR236/issue7 after completion, without
a documentation-only CI repeat. User owns merge/tag/publication; Grep last.
