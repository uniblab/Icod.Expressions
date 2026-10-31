# Byte code-unit escape design

Continue issue #7 after merged #114 with byte-mode `\C`: consume exactly one
arbitrary byte, including NUL, CR/LF and non-UTF bytes. UCP, i/r/ASCII restrictions,
DotAll, newline convention and BSR do not change membership or width. Repeats,
captures/references, calls, assertions and behind width rules reuse existing paths.

## Approach and profile boundary

Pinned PCRE2 10.48 pcre2_compile.c8352..8367 lowers non-UTF C to OP_ALLANY.
A new opcode/frame or a separate raw-byte engine would duplicate existing Any.
Maintainer choice under standing authority: ParseEscape recognizes uppercase C,
rejects bracket use with InvalidSyntax at the escape, keeps UTF UnsupportedSyntax,
and otherwise returns PcreByteAny(DotAll:true). This retains the byte ALLANY prefix
proof already consumed by HasByteCrLfPrefix; no source newline flag is invented.

UTF C is a separate design: native cannot JIT it, can enter a scalar interior in
strict mode via its interpreter, and forbids it in UTF behind. This tranche does
not change scalar/fragment invariants or reinterpret it as a whole UTF character.
Caller Utf8/MatchInvalidUtf and source UTF/UTF8 all keep explicit rejection, with
existing owning-behind diagnostic behavior. Byte UCP remains fully supported.

Quoted/escaped backslashes and literal C remain ordinary; lowercase c remains the
printable-ASCII control escape. Bracket C is invalid even in UTF, as native syntax
requires; UTF whole-source validation still precedes parsing. No source rewriting.

## Constraints and delivery

C#13, net8/net9/net10, UTF-8 LF, root project/solution and src/tests/samples.
No agents/Python/SDK install. No new VM state/opcode, captures snapshots, copied
engine, raised limits or production native/Regex fallback. Depth256/captures256/
instructions4096/marks256/frames1024/defaultwork1,000,000 remain shared.
No public API/package/version/dependency/workflow/release/consumer changes.
Grep last; CommandFramework removal after publication and all known consumers.

Native byte masks across all256 values/modes and literal spans pin membership,
width, captures, rollback/calls/behind and CRLF prefix behavior. Every-offset six-
newline matrices include malformed UTF-looking bytes, NUL and requested offsets.
Grammar/quote/control/UTF gates and resource/work/cancellation boundaries remain.
Replace historical byte-C rejection representatives with pending UTF-C/X controls,
retaining test counts/diagnostic ownership. Tests-only draft and observed RED
precede source; separate author self-review (no independent claim), all six native
Staging full/frozen214 net8/9/10 suites/configured gates and exact-head/tree audit
precede ready. Preserve worktree/evidence; user owns merge.
