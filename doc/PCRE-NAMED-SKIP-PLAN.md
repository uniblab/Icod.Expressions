# Named SKIP implementation plan

Execute inline, tests first, on isolated feature/pcre-named-skip from merged220.
Goal: bounded, native-compatible named SKIP using existing journals and cuts.
Architecture and reference ruling: [design](PCRE-NAMED-SKIP-DESIGN.md).
Tech: C#13/net8/9/10. No subagents, Python, SDK installs or production fallback.
User owns merge, tags and publication.

## Task1: qualify and freeze

Files: PcreNamedSkipReferenceTests; old SKIP nonexact-header inventory;
PcreGroupAliasReferenceTests pending syntax row; design and plan.

- [x] Independently verify199 literal/profile cases with380 native probes.
- [x] Prepare231 cases, raw-byte diagnostics, scope/capture, quota and work checks.
- [x] Verify old SKIP source outside its obsolete header inventory is unchanged.
- [ ] Publish draft tests and require clean-build RED with only new failures.
- [ ] Freeze new references and revised inventories before production changes.

## Task2: implement

Files: Syntax; Parser.PathMarks/Groups/Lookbehind; Compiler; Program; Matcher.
Interfaces: PcreByteNamedSkip(Name), parser HasNamedSkip, SkipNamed opcode,
program TrackPathMarks, FrameKind.PathMark. Defaults preserve existing callers.

- [ ] Share raw-name lexer; distinguish empty argument from NUL-prefix name.
- [ ] Emit immutable name bytes and zero-width metadata; extend bounded proof.
- [ ] Add bounded journaled MARK history only in named-SKIP programs.
- [ ] Implement charged visible-history lookup, scope-local quits and cut reuse.
- [ ] Run unchanged references and all18 native CI results to GREEN.

## Task3: review and ready

Files: syntax/resource/consumer fixture inventories, roadmap and issue7 footer.

- [ ] Review grammar/ownership, duplicate and hidden marks, local cuts/captures,
  rollback and returned callees, shared budgets/work/cancel and bounded study.
- [ ] Verify fixed references, frozen source, full qualification and merge tree.
- [ ] Record evidence and remaining gates, update issue7, ready PR; clean ledger.
