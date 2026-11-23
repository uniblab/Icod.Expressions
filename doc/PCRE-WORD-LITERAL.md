# Native width-16 literal compilation and matching

`PcreWordLiteralCompiler.CompileLiteral` is an experimental internal P10
entry point for case-sensitive **literal** patterns. It shares the existing
`PcreByteLiteral` semantic node and `PcreMatchPolicy`, then executes a native
width-16 literal sequence. There is no second regex grammar or copied
backtracking VM. Regex metacharacters, escapes, options and directives in
the source are ordinary literal text.

Raw mode compares original 16-bit units, including NUL and isolated
surrogates. Strict UTF-16 mode compares complete scalars through `PcreUtf16`.
The compiler owns its literal nodes after compilation; matching reads the
original subject memory without copying or transcoding it. The result is
an offset-only group-zero capture in native 16-bit units. No named captures
exist in this subset.

Compiled and per-call anchoring, end anchoring and empty-result restrictions
combine monotonically. NOTBOL/NOTEOL do not alter literal text. Strict UTF
subject validation starts at the requested offset because literal programs
have no lookbehind, matching native PCRE2. Earlier malformed prefix text is
not inspected. A nonzero offset at a low surrogate is an invalid start;
otherwise malformed suffix text produces an invalid-subject diagnostic.
The existing byte backend's whole-subject validation contract is unchanged.

## Provisional resource and diagnostic contract

Compilation admits at most 4,096 original pattern units and observes a
smaller caller `MaxPatternLength` before accessing source storage. Empty
patterns can have a zero caller limit. A positive match work limit is
required. Cancellation is checked at compiler/matcher entry, during literal
lowering, and before/after each match work step.

Each strict subject-validation scalar, candidate admission, attempted
literal comparison, root completion, and candidate advance costs one step.
Root completion is charged even when a policy rejects the result. Observed
and unobserved runs use the same counter; observers see successive values
starting at one. The default limit is 1,000,000. No host recursion, unbounded
stack or subject-sized auxiliary storage is used by matching. Search is a
simple bounded scalar/unit scan; broader performance work remains open.

Diagnostics distinguish invalid work limits, pattern length admission,
malformed pattern/subject UTF-16, invalid start offsets, and exhausted work.
Offsets are native code units. Quota exhaustion has offset zero. No-match
and errors remain separate; cancellation and observer exceptions propagate.

## Evidence and remaining work

Twenty original-word fixtures cover raw/UTF, supplementary scalars, NUL,
isolated raw surrogates, literal regex syntax, empty patterns, case-sensitive
misses and nonzero offsets. Six compiled default masks and all 64 call masks
produce 7,680 live native16 comparisons. Further tests cover UTF diagnostics,
owned pattern slices, guarded storage admission, exact work limits,
cancellation, maximum literal length, and bounded managed UTF-8/UTF-16 parity
on valid Unicode literals. These coordinate conversions exist only in tests.

The [C oracle](../tests/Pcre/Native/word-literal-oracle.c) used static PCRE2
10.48 at `7978954dbd2efc6f2196869290553cf1871b4ce6`, SLJIT at
`de0259c7aaf36aa40cba8014f3fad3edde9307f9`, and
`cc -O2 -std=c11 -Wall -Wextra -Werror`. In fixture/default/mask order it
recorded 2,968 matches and 4,712 misses with no unexpected errors. Every
compiled row required successful JIT generation and positive JIT size.
LF-terminated output SHA-256:

`001e3f6933a1105655d31a5a3b1868c188f2dc006f751d6053447d1d14571337`

The seven-field input is
`id|utf|compiled-default-mask|call-mask|pattern-word-hex|subject-word-hex|start`.
Hex uses four digits per original word, independent of host endianness;
`-` denotes empty. Output retains match/no-match/compile-error/runtime-error
as separate classes. The existing native16 ABI adapter exposes this literal
mode without changing prior reference calls.

This is a literal P10 execution slice. General regex parsing/lowering,
caseless/tolerant UTF-16, captures, backtracking, the full resource model,
package consumer/API freeze and broad cross-width qualification remain open.
Final managed qualification and source identities are recorded in the PR.
