# UTF and UCP start directives implementation plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans inline. Do not use subagents, per user instruction.

**Goal:** Resolve UTF/UTF8/UCP start directives before existing compile validation through one shared settings scan.
**Architecture:** Immutable PcreByteStartSettings carries four effective booleans and original BodyOffset; one scanner replaces the current parser scan. Existing parser/program consume those choices with no runtime changes.
**Tech Stack:** C#13, net8/net9/net10, xUnit, PCRE.NET1.6.0 / PCRE2 10.48 Compiled/JIT.
**Spec:** `doc/PCRE-UTF-START-DIRECTIVES-DESIGN.md`

## Global constraints

- Exact uninterrupted byte0 prefix; UTF/UTF8/UCP enable caller modes, newline/BSR remain independent last-wins; UCP without UTF stays rejected0.
- Resolve before UCP gate/whole-pattern UTF validation; preserve cancellation/invalid-work-limit precedence and original source bytes/indexes.
- No AST/opcode/frame/journal/matcher/width/Unicode-table/limit changes, saved spans/pointers, cloned engines, option mutation or fallback.
- No Python/public API/package/version/dependency/workflow/release/consumer change. Grep last; CommandFramework removal after publication/all-consumer migration. User owns merge.
- Local Debug; six native Staging jobs; no local SDK, managed RED/GREEN via CI. No subagents; separate author self-review; preserve worktree/evidence.

## Review focus

1. UTF must be effective before validation, including caller UCP plus later UTF directive and invalid UTF following unsupported body syntax.
2. All consumers must share the final modes; UTF/UCP remain distinct and global, with ASCII restrictions and subject tolerance preserved.
3. All seven tokens must interleave without duplicate scans or early stop; trivia/options/quotes/unknown tokens end recognition exactly as before.
4. Original byte offsets, invalid-work-limit/pre-cancellation precedence and pending UCP-without-UTF gate must remain explicit.
5. Long prefixes add no VM work or capture/width state; strict/tolerant caller-offset/fragment and resource protocols remain shared.

## Task1: Pin effective-mode behavior

**Files:** Create `tests/Pcre/PcreUtfStartDirectiveReferenceTests.cs`, this design/plan; update PCRE design/matrix/resource/consumer/start-directive docs and pending syntax representatives.
**Interfaces:** Existing Compile/Match and native regex settings, unchanged.

- [ ] Add native-first literal full-capture rows for UTF/UTF8/UCP and mixed newline/BSR, opposite caller defaults, global reset/capture/name/call/behind/ASCII controls; pin strict versus tolerant UTF subjects and every offset over malformed fragments.
- [ ] Add original-byte pattern/body errors, prefix placement/quotes/case/width aliases, UCP-without-UTF rejection, work-limit/pre-cancellation and long-prefix/work equivalence. Native expectations are literal, not computed by production.
- [ ] Self-check design/plan/interfaces; run native checker and whitespace/frozen35 checks, publish tests-only draft and observe expected feature RED with zero build errors before any source change. Move implemented pending-syntax examples to other still-pending directives.

## Task2: One immutable compile-settings phase

**Files:** Create `src/Pcre/PcreByteStartSettings.cs`; modify `src/Pcre/PcreByteCompiler.cs` and `src/Pcre/PcreByteParser.cs`; remove superseded `src/Pcre/PcreByteParser.StartDirectives.cs`.
**Interfaces:** `internal readonly record struct PcreByteStartSettings(bool Utf8, bool Ucp, bool AnyCrLfNewline, bool BsrAnyCrLf, int BodyOffset)` with static `Parse(ReadOnlySpan<byte>, PcreByteOptions, CancellationToken)`. Parser constructor becomes `(byte[] source, bool caseless, bool asciiBsD, PcreByteStartSettings settings, CancellationToken cancellationToken)`; existing Compile/Match unchanged.

- [ ] After observed RED, move/extend prefix recognition into the struct's iterative cancellable Parse. Start from caller defaults, consume exact seven tokens, stop at first other byte; no retained spans and no duplicate scan.
- [ ] Compile validates work limit first, copies source, resolves settings, applies unchanged UCP-without-UTF gate and whole-source UTF validation, then constructs parser/program using the same settings. Parser initializes index=BodyOffset and readonly UTF/UCP/newline fields; remove old scan and effective-setting properties.
- [ ] All six Staging jobs: full suite/frozen214 on net8/net9/net10, no failures/skips/build warnings/errors; configured gates/seven Linux workload equalities. Separate whole-branch self-review, fix material findings RED→GREEN, exact source/tree/head/frozen35/whitespace/clean checks. Update proof/mark ready; never merge.

Self-check: Task1 keeps Compile/Match unchanged; Task2 replaces one internal constructor call and one scanner, not a shared runtime interface. All five focus classes have Task1 coverage. No unresolved design questions or interface conflicts.
