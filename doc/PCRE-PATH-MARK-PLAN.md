# MARK successful-path implementation plan

Execute inline with immutable tests-first references.
**Goal:** Native successful-path byte labels for MARK and its shorthand.
**Architecture:** One shared journaled label slot, immutable opcode name bytes,
callee-return exception and internal result metadata. **Tech:** C#13/net8/9/10.
**Spec:** [MARK design](PCRE-PATH-MARK-DESIGN.md).

## Constraints and review focus

Preserve all earlier reference blobs (including named-verb rejection cases),
GNU/frozen35/frozen214/120, exported API and dependencies. No fallback, Python,
installation or subagents. User owns merges/tags/publication.
Review raw-byte/UTF length and NUL views, assertion/conditional label outcomes,
callee success versus forced-failure selected captures, zero-width repeat/atomic
rollback, label memory lifetime, and shared slot/work/compilation bounds.

## Task 1: qualify and freeze

Files: new PcrePathMarkReferenceTests; four obsolete unfrozen pending-MARK
inventory cases in PcreSyntaxAcceptanceTests/PcreGroupAliasReferenceTests.
Interface: existing compile/match boundaries; test reflection reads future
internal MarkName metadata; PCRE.NET Mark spans and native get_mark qualify it.

- [x] Export161 literal native cases and add all-offset, name grammar, raw byte,
resource/work/cancel and immutable ownership cases (196 new cases total).
- [ ] Publish draft tests/design/plan; require clean-build RED, then freeze all
new references before production changes.

## Task 2: implement

Files: Syntax; new Parser.PathMarks partial plus Groups/Lookbehind; Program;
Matcher; Result. Interfaces: PcreBytePathMark(Name), PathMark opcode,
PcreByteProgram.PathMarkSlot (-1 when absent), internal PcreByteMatch.MarkName
(ReadOnlyMemory<byte>, empty when no visible label).

- [ ] Parse both raw-name headers with 1..255 byte validation, original errors,
direct-repeat rejection and zero-width/nonterminating metadata.
- [ ] Allocate one shared mark slot lazily; emit immutable NUL-trimmed name bytes.
- [ ] Journal latest instruction+1, keep path slot across successful Return,
restore on ordinary/cut failure; attach memory at Accept without another stack.
- [ ] Cross MARK in the existing bounded COMMIT entry proof; add no wider optimizer.
- [ ] Run unchanged references and full applicable native CI to GREEN.

## Task 3: review and ready

Files: syntax/resource/fixture inventories, roadmap status and issue7 footer.

- [ ] Review six focus areas, fixed blobs and proposed merge tree.
- [ ] Record RED/GREEN evidence and remaining gates; ready PR after full checks.
