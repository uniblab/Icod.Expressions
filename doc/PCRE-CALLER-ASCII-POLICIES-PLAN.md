# Caller ASCII Policies Implementation Plan

Execute inline under the established continuous workflow.
**Goal:** Complete caller initialization of the existing five-bit ASCII mask.
**Architecture:** Four default-false options copy into existing parser bits;
source scope, predicates and word wrappers retain ownership.
**Tech Stack:** C#13/net8/9/10; managed RED/GREEN through GitHub CI.
**Spec:** [design](PCRE-CALLER-ASCII-POLICIES-DESIGN.md).

## Global constraints

Earlier tests, GNU/BRE/ERE, 35 frozen blobs and214 outcomes/120 methods remain
fixed, as do API/dependencies. UTF-8+LF; no Python, subagents or local SDK installs.
Existing isolated worktree; user merges/tags/publishes; Grep last.

## Review focus

1. Native Posix alone differs from aP's combined Posix/PosixDigit seed after unsets.
2. MatchWord's outer entry restriction survives contrary body aW settings.
3. All POSIX/algebra predicates remain independent of literal/property folding.
4. Byte/UTF/UCP domains, malformed offsets and original diagnostics stay native.
5. Compiled options/source isolation and shared work/cancellation/quotas hold.

## Task 1: tests-first and freeze

Create tests/Pcre/PcreCallerAsciiPolicyReferenceTests.cs, reusing the existing
Compile/Match/MarkName interfaces and native literal/Compare checks. Reflect only
new flags, while deriving native extra bits independently from the selection.
Include all32 five-bit combinations/modes, native scoped/reset/overlap profiles,
entry-policy wrappers, all POSIX names/octets/offsets, old policies, ownership,
exact diagnostics, work/cancellation and quotas. Native loop arrays precompile.

- [x] Research and pin literal native PCRE2 10.48/JIT outcomes.
- [ ] Publish tests-only draft and observe clean net8/9/10 expected RED.
- [ ] Inspect identities/assertion sites; default/legacy/native checks must pass.
- [ ] Freeze the reference hash before production changes.

## Task 2: initialize and qualify

Modify src/Pcre/PcreByteOptions.cs: AsciiBsS, AsciiBsW, AsciiPosix, AsciiDigit.
Modify PcreByteParser.cs constructor: OR BackslashS, BackslashW, Posix and
PosixDigit independently into the existing mask alongside AsciiBsD. Do not set
PosixDigit implicitly from AsciiPosix. No compiler/scoped parser/runtime changes.

- [ ] Implement these copies after valid freeze.
- [ ] Require all new/old tests in18 native results; zero failures/skips/warnings/errors.
- [ ] Verify frozen214/120 and35, Unicode generators, workload/parity/API/corpus/
  packaging and package-only smoke; three-OS scripts; existing consumer classifier.

## Task 3: review and readiness

- [ ] Author-review all five boundaries, fixed earlier fixtures and native flag masks.
- [ ] Confirm final head/base/proposed merge tree and full qualification receipts.
- [ ] Update syntax/resource/consumer/status docs, issue7 footer and PR evidence.
- [ ] Mark ready; remove only this tranche ledger. User owns merge/publication.

## Baseline

PR230 merged at f0d240b9536a722bc370cc8a859f097f148689b6, tree
d1af825e862a6e385e821a686a5d64782f28b645. Main43212521201 and scripts43212521645
are green; 55,542 tests/framework. All244 earlier test files must stay fixed.

Task1 reference inventory: 2,088 new cases: 1,979 literal native JIT profiles,
60 exact original diagnostics,14 all-octet,14 UTF-fragment/offset,5 POSIX/algebra,
5 old-policy composition,5 UTF-validation,1 ownership and5 work/cancel/limit cases.
Expected total57,630/framework. All32 flag combinations in all4 UTF/UCP modes;
all supplemental native arrays compile before managed comparisons.
