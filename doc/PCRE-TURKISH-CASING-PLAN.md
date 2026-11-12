# Turkish casing implementation plan

Execute inline with superpowers:executing-plans. Spec:
[design](PCRE-TURKISH-CASING-DESIGN.md).
Goal: exact UTF Turkish prefix using the single shared casing and matcher paths.
Architecture: global immutable bool, four-scalar fold override gated by lexical
r, existing class closure/runtime consumers and bounded start proofs. C#13/net8/9/10.

## Constraints and review focus

No subagents, Python, SDK installs, exported API/dependency changes or production
fallback. Preserve GNU/BRE/ERE, frozen35 source blobs and frozen214/120 results,
default Unicode17 tables and all earlier references except the one obsolete
pending capability row. Review five classes: every I comparison consumer;
lexical r/reset/definition options; prefix/UTF diagnostic precedence; existing
start proofs/fragment entry; ownership and work/quotas/cancellation.

## Task1: test and freeze

Files: create tests/Pcre/PcreTurkishCasingReferenceTests.cs; retarget one row in
PcreCaselessRestrictDirectiveReferenceTests.cs only. Interfaces: existing
Compile/Match/MarkName, PcreByteOptions and pinned PCRE.NET oracle.

- [x] Independently check1222 literal/profile fixtures with native JIT.
- [x] Write1256 new cases and preserve all earlier assertion source outside
  the one pending inventory row. UTS uses existing PcreOptions bit0x08000000.
- [ ] Publish draft and qualify clean net8/9/10 RED with only expected new
  failures, no earlier/native literal oracle failures and zero build warnings/errors.
- [ ] Freeze new and revised reference blobs before production changes.

## Task2: implement and qualify

Files and interfaces:
- PcreByteStartSettings.cs: append bool TurkishCasing, consume exact prefix.
- PcreByteCompiler.cs: after UTF validation, reject non-UTF/global-r conflict
  with InvalidSyntax at settings.BodyOffset; pass TurkishCasing to compiler.
- PcreByteParser.cs/ClassAlgebra.cs/UtsClass.cs: readonly global bool for all
  CloseRanges calls, preserving ordinary lexical caselessRestricted state.
- PcreUtf8CaseFold.cs: append optional bool turkish=false to Fold, Equals and
  CloseRanges; apply four-scalar rule only when !restricted.
- PcreByteProgram.cs: optional final constructor bool turkishCasing=false,
  readonly field and immutable program TurkishCasing property. Existing optional
  literal bitmap and COMMIT first-byte proofs exclude Turkish I when !r.
- PcreByteMatcher.cs/ClassAlgebra.cs: pass the program global flag through UTF
  literal, set, reference, literal-assertion and scalar-expression comparisons.
  Byte folding remains unchanged because Turkish requires UTF.

- [ ] Implement those settings, common folding/closure/consumer changes and
  the two mode-specific proof exclusions, without new opcodes/AST flags/slots.
- [ ] Qualify49744 tests/framework in18 native results, zero failures/skips/
  build warnings/errors, frozen214/120 in each; frozen35, Unicode/workloads/
  parity/API/corpus/pack/package-only and all3OS script checks.

## Task3: review and ready

- [ ] Author-review every focus class, reference hashes and exact source diff.
- [ ] Verify exact head/base/proposed merge tree and all final CI receipts.
- [ ] Update syntax/resource/consumer docs, roadmap/issue7 and PR qualification;
  ready PR; remove only this plan's ledger. Grep stays last.

## Baseline

PR224 merged2a78fd467d7b3a1adc5e8dfa6c6f8d0171291b49; main43144903778 and
scripts43144903781 are green. Base tree04d5b49234cacc54a62126eff5cadf6bbfe8b6ad;
48488 tests/framework before1256 new controls.
