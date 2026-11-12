# CASELESS_RESTRICT directive implementation plan

Execute inline with superpowers:executing-plans. Spec:
[design](PCRE-CASELESS-RESTRICT-DIRECTIVE-DESIGN.md).
Goal: exact leading CASELESS_RESTRICT using the existing restricted-casing path.
Architecture: one bool in PcreByteStartSettings and parser initialization;
no new opcode, runtime path, source rewrite or mark slot. C#13/net8/9/10.

## Global constraints and review focus

Preserve GNU/BRE/ERE, frozen35 source blobs and frozen214 outcomes/120 methods;
all earlier references, exported API and dependencies. No Python, subagents,
SDK installs or production fallback. User owns merge/tag/publication. Use
GitHub CI for managed RED/GREEN; use the existing isolated worktree.
Review mode reset/scoping; exact prefix/byte diagnostics; all comparison/capture
consumers; UTF activation/ownership; shared quotas/work/cancellation. Tests
below cover each class; author review must recheck them before readiness.

## Task1: references and clean RED

Create tests/Pcre/PcreCaselessRestrictDirectiveReferenceTests.cs. Interfaces:
existing Compile/Match/MarkName and PcreByteOptions, pinned PCRE.NET oracle.

- [x] Check333 literal/profile fixtures independently with native PCRE2/JIT.
- [x] Write366 cases, with literals plus all-offset UTF/CRLF, malformed headers,
  byte diagnostics, long-prefix work, 256 capture/mark and ownership controls.
- [x] Publish tests-first draft and observe clean net8/9/10 RED with only
  expected new failures; no earlier/native-oracle failures or build warnings.
- [x] Freeze the new reference blob before production changes.

## Task2: production and qualification

Modify src/Pcre/PcreByteStartSettings.cs: append bool CaselessRestricted to the
record; initialize false, consume exact prefix, return it with other settings.
Modify src/Pcre/PcreByteParser.cs: initialize caselessRestricted from settings
before parsing. Existing Parse/TryParseOptions/CurrentGroupOptions own semantics.

- [x] Implement those settings and initialization changes only.
- [ ] Qualify48488 cases per framework in18 native platform/framework results,
  zero failures/skips/build warnings/errors, frozen214/120 in every result.
- [ ] Verify frozen35, Unicode generators, workload/parity/API/corpus/pack and
  package-only smoke; all3OS build-script tests; existing consumer classifier.

## Task3: author review and ready

- [ ] Review all five focus classes, unchanged earlier blobs and exact diff.
- [ ] Verify source/base/proposed-merge tree and all CI receipts.
- [ ] Update syntax/resource/consumer docs, roadmap/issue7 and PR evidence;
  mark PR ready; remove only this plan's ignored ledger. Grep stays last.

## Baseline

PR223 merged38e42a090438fa41cbc90cfd4e8d04e3025899c3; main43138710909 and
scripts43138710837 are green. Previous qualified source5a0791d7da7fe1bbf135aeb06e1a5228ae84ffbc,
tree0d33e6c78024a7eeaff95614a7bafae6d6c45cd0,48122 tests/framework.

## Frozen RED checkpoint

RED672 run43142893767/LinuxARM64 job130933841433 built net8/9/10 with zero
warnings/errors. Each framework reported48488 cases:351 expected new failures,
48137 passes and0 skips. All failures are in the new file at managed compile/
diagnostic assertions; no earlier fixture or native literal oracle failed.
Freeze all366 cases at5c8ddc61bd74186ed85e1a4e40c82a418a2e934c before production.
