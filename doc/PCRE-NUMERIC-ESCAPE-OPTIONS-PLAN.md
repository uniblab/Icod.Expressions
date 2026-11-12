# Numeric escape options implementation plan

Execute inline with superpowers:executing-plans. Spec:
[design](PCRE-NUMERIC-ESCAPE-OPTIONS-DESIGN.md).
Goal: caller-selected Python octal and bare-zero rejection through shared parsing.
Architecture: two readonly parser flags, constant three-octal peek, existing
bounded decimal scan/backreference resolution and octal value/digit checks.
Tech: C#13/net8/9/10; GitHub native CI only for managed builds.

## Constraints and review focus

No Python, subagents, SDK installs, exported API/dependency changes or runtime
fallback. Keep every earlier reference and default option behavior, GNU/BRE/ERE,
frozen35 and frozen214 outcomes/120 methods. User owns merge/tag/publication.
Review full-decimal/three-octal consumption; zero/class/quote boundaries;
UTF/range/diagnostic precedence; capture/width semantics; source/options ownership
and bounded work/cancellation. Native PCRE2 10.48 is the semantic authority.

## Task1: test and freeze

Create tests/Pcre/PcreNumericEscapeOptionReferenceTests.cs. Consume existing
Compile/Match/MarkName and PcreByteOptions. Test helper reflection lets the
pre-implementation commit build, following the existing UTS option precedent.
Native extras use existing enum casts0x2000/0x4000; UTS uses PcreOptions0x08000000.

- [x] Independently qualify688 profiles (534 JIT-valid,154 native compile errors).
- [x] Write709 cases including all octets, capture counts/forward references,
  all selected offsets, classes/algebra, diagnostics, ownership and limits.
- [x] Publish draft; observe clean net8/9/10 RED with expected new-only failures,
  no earlier/native literal oracle failure and zero build warnings/errors.
- [x] Freeze the new reference blob before production changes.

## Task2: implementation and qualification

Modify src/Pcre/PcreByteOptions.cs: bool PythonOctal and NoBackslashZero, false.
Modify PcreByteCompiler.cs/PcreByteParser.cs: append optional parser ctor flags,
copy readonly fields; pass flags from caller options. Existing UTF validation,
scoped options and all typed AST/runtime interfaces remain unchanged.
Modify PcreByteParser.NumericEscapes.cs: recognize exactly three leading octal
characters before scanning Python decimal references; force reference selection
otherwise. Track octal digit count, reject one-digit zero under NoBackslashZero,
and reject Python unbraced values>255. Braced parsing stays unchanged.

- [x] Implement those options, plumbing and shared numeric rules only.
- [ ] Qualify50453 tests/framework in18 native platform/framework results,
  zero failures/skips/build warnings/errors and frozen214/120 in every result.
- [ ] Verify frozen35, Unicode generators, workload/parity/API/corpus/pack and
  package-only smoke plus all3OS build-script checks; note consumer classifier.

## Task3: author review and ready

- [ ] Review every focus class, unchanged earlier tests and exact source diff.
- [ ] Verify head/base/proposed merge tree and final qualification receipts.
- [ ] Update syntax/resource/consumer docs, roadmap/issue7 and PR evidence;
  ready PR; remove only this plan's ledger. Grep stays last.

## Baseline

PR225 mergedb9701c0fd4112205cb4ceb9988062642f0062c97; main43149468277 and
scripts43149468312 are green. Source89c44bf29f90fbfdf3db168151c13644748a39bd,
tree7e14c9df5333f916b5d4caee62f1bb32b1b10113,49744 tests/framework before709.

## Frozen RED checkpoint

RED676 run43154186921/LinuxARM64 job130970169309 built net8/9/10 with zero
warnings/errors. Each had50453 cases:149 expected new failures,50304 passes,
0 skips. All failures are new managed compile/diagnostic/comparison/ownership
assertions; no earlier fixture or native literal oracle failed. Freeze709 cases
atc169f5dfd97ec55a26bfbc2c0e4a538f5109be80 before production. All earlier
reference files remain unchanged.

## Diagnostic correction after first implementation

CI677 compiled cleanly; its20 remaining failures are frozen numeric error-offset
controls in Perl/UTS classes. Both class parsers catch every InvalidSyntax and
replace its original byte with the outer opening. That also masked existing
default-byte unbraced range errors in the new controls. Preserve a narrow
IsNumericEscape tag for unbraced-range/bare-zero exceptions through those two
wrappers; other class grammar errors still report their original outer opening.
This intentionally improves those numeric diagnostic offsets, including default
byte-mode oversized octals, without changing matching or earlier references.
The709-case blob stays fixed. Full50453/18 qualification remains required.
