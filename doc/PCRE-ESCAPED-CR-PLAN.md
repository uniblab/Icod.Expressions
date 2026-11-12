# Escaped CR compile option implementation plan

Execute inline. [Design](PCRE-ESCAPED-CR-DESIGN.md).
Goal: caller-selected r-to-LF parsing through the existing simple-escape path.
Architecture: one default-false caller bool, readonly parser copy and canonical
literal selection. Preserve source bytes and shared typed/metadata/runtime paths.
Tech: C# 13, net8/9/10; managed RED/GREEN in GitHub CI.

## Constraints and review focus

Preserve earlier references, GNU/BRE/ERE, 35 frozen production blobs, 214 frozen
outcomes in 120 methods, exported API and dependencies. No Python, subagents,
SDK installs or runtime fallback. User owns merge/tag/publication. Review only-r
mapping versus other CR spellings; canonical class/range/newline metadata;
comments/quotes/raw names; original byte/UTF diagnostics; owned source/options,
bounded work/state and cancellation. Grep stays last.

## Task 1: references and freeze

Create tests/Pcre/PcreEscapedCrReferenceTests.cs against existing Compile/Match/
MarkName and PCRE.NET. Reflection enables tests-first compilation. Native extra
flag 0x10 composes with existing Python/NO_BS0/UTS flags where applicable.

- [x] Independently qualify 1,500 native profiles: 1,474 JIT-valid and 26 errors.
- [x] Write 1,495 distinct cases, including six unique compile-error cases and
  all-byte, malformed-UTF/CRLF offsets, diagnostics, ownership and resource checks.
- [x] Observe valid clean-build RED 682 with only option-dependent failures.
- [x] Freeze corrected reference before restoring production implementation.

## Task 2: implement and qualify

Files: PcreByteOptions.cs adds EscapedCrIsLf; PcreByteCompiler.cs forwards it;
PcreByteParser.cs appends optional escapedCrIsLf=false, copies a readonly field,
and selects LF for the positive CR simple-escape value. Preserve other escapes,
quotes, names, source and diagnostic indices. No AST, opcode or runtime change.

- [x] Restore the identical three-file implementation tested by CI 681.
- [ ] Qualify all 51,948 tests/framework in 18 native platform/framework results,
  zero failures/skips/build warnings/errors and frozen 214/120 in each.
- [ ] Verify frozen 35, Unicode generators, workload/parity/API/corpus/pack,
  package-only smoke and three-OS script checks; note consumer classifier.

## Task 3: author review and ready

- [ ] Review all five focus classes, earlier files and exact source diff.
- [ ] Verify head/base/proposed merge tree and final qualification receipts.
- [ ] Update issue #7 and PR evidence, mark ready and remove only this ledger.

## Valid baseline and reference receipt

PR226 merged at fa0d79c4368f7fb65784a7191c3bf02034a83aac;
main 43159771095 and scripts 43159771272 are green. Baseline: 50,453 tests.

Initial CI 679 exposed 20 duplicate diagnostic rows, removed. The RED 680/freeze
receipt is invalidated: CI 681 passed 51,946/51,948 per framework and exposed
two new default-byte diagnostic expectations at opening 0 instead of existing
hyphen 3. Correct only those two exact offsets and the method name, preserving
strict assertions, native profiles and production diagnostic behavior.

Restore production exactly to merged PR226 for tests-only source
f0384874b3a39adb2128db895a7709b5e53abfcc,
tree d38d7eb13bb9c997a0e35972b6c56e0e153af655.
RED 682/run 43166167501, Linux ARM64 job 131008465287: net8/9/10 each
51,948 tests, 354 option-dependent new failures, 51,594 passes and zero skips,
build warnings or errors. No default-profile, diagnostic, earlier-reference or
native literal-oracle failure. Mac x64 net10 independently gives the same result.
Freeze all 1,495 cases at 02a1e6f7874e39f406b91ee7496b71fc51ade294 before
restoring the identical implementation from 0f14cd460c55183412c0028140589b2a33218027.
