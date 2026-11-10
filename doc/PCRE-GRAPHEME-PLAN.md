# Grapheme matching implementation plan

**Goal:** Native-compatible atomic `\X` over authoritative bytes.
**Spec:** doc/PCRE-GRAPHEME-DESIGN.md.
**Architecture:** Dedicated consuming node/opcode, pinned break table and
bounded scanner; reuse outer repeats/journal/limits. Inline, no subagents.

## Constraints and review focus

C#13/net8/9/10; no runtime native/regex/host segmentation fallback or new
package. No Python/SDK/PowerShell installation. Preserve frozen35/frozen214,
GNU, original byte offsets, capture state, Step/cancellation. Review RI context
before start and across invalid fragments, cache reuse during backtracking,
EP/Extend/ZWJ state, CRLF search recovery and forward lookahead limits inside
lookbehind. Check that added loop work scales linearly on contiguous RI runs.

## Task1: Native fixtures and RED

Create tests/Pcre/PcreGraphemeReferenceTests.cs, PcreGraphemeFixtureData.cs and
PcreGraphemeLookaheadScopeTests.cs;
force raw data LF framing. Replace three obsolete unfrozen pending-X rows with
other deliberately unsupported constructs; retain bracket/behind diagnostics.
Publish draft, run full CI, require successful-build unsupported-X RED, freeze
new test/data blobs. Expected baseline45,948 cases remain qualified.

## Task2: Data and runtime, GREEN

- Add src/Pcre/PcreUnicodeGraphemes.cs/Data.cs and standalone
  tools/GeneratePcreUnicodeGraphemes with both source hashes and native hash.
  Extend Linux regeneration and packaged Unicode notice.
- Add PcreByteGrapheme node in PcreByteSyntax, parse outside brackets, emit
  Grapheme opcode in PcreByteProgram. Leave consuming lookbehind unsupported;
  zero-count direct atoms follow native erased-node rules where applicable.
- Add focused PcreByteMatcher.Grapheme.cs scanner, partial matcher, shared ref-counter Step
  primitive and two-offset RI memo. Charge every scalar probe/prefix inspection.
  Handle scopes/fragments without new frames or capture mutations. Audit node
  visitors/start proofs conservatively and validate nested forward assertions.
- Update four inventories and current roadmap. Run frozen checks and full CI:
  18 native framework results, all applicable725-consumer compatibility jobs,
  build-script/package/API/extraction/workload and exact data regeneration.
  Review all files, unchanged tests and exact proposed merge tree; record
  evidence and mark ready. User owns merge/publication; Grep last.

## Execution evidence

Test-only a3c2c74 / RED631 run42892405304, Linux x64 job130096658309:
all frameworks compiled with zero warnings/errors. Each suite46006:53
expected new failures,45953 passes,0skips. All45948 baseline cases pass.
New tests/data fixed after RED; additional variable-behind scope fixtures
pin native bounded behavior, alongside the existing fixed-behind fixtures.
No assertion-scope implementation correction is warranted by the oracle.

Scope-complete test-only4685328 / RED632 run42893407574, Linux ARM64
job130100354947: all3 frameworks compile cleanly,46009 cases with54 expected
failures/45955 passes,0skips; all45948 baseline cases pass. Fixed test blobs:
08c832b81db8ed538fafd54a6b8522f6163a02bb (main tests),
45d8d47c0fe67ed3343bb3b6b5a5e2df8d0c82f0 (data),
e450749ae1940ba36775afb9a49df25b8f1be013 (scope tests).
Native probes also reject consuming behind \X even with a zero count;
existing width validation remains appropriate.
