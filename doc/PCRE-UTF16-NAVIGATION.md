# Native UTF-16 navigation foundation

`PcreUtf16` introduces the representation-sensitive leaf operations needed
by the future native width-16 compiler and matcher. It operates directly on
`ReadOnlySpan<char>` / `Span<char>` views. Offsets and widths are native
16-bit code units. There is no whole-input encoding conversion, allocation,
replacement fallback, or new parser/VM implementation.

| Operation | Raw mode | UTF mode |
|---|---|---|
| Read at an offset | One original code unit, including an isolated surrogate | One valid Unicode scalar, consuming one or two units |
| Read before an exclusive end | One original preceding unit | One valid preceding scalar, consuming one or two units |
| Invalid boundary or malformed scalar | Boundary failures return false | Invalid/truncated pairs return false |

Failed reads clear both output values. A supplied span is the complete
navigation boundary: a pair cannot be completed using units outside that
view. Reads do not validate unrelated subject text. Fragment selection,
whole-input validation/error precedence, work charging and cancellation
remain responsibilities of the future compiler/matcher.

The compiled-literal encoder accepts code points through `10FFFF`, including
explicit surrogate values as single raw units. This does not admit isolated
surrogates into strict UTF source or subject text. Negative/out-of-range
values and short destinations are rejected before any write. Successful
encoding modifies only the required one or two destination units.

Managed tests cover every isolated 16-bit value in raw and strict modes,
every supplementary scalar in both directions, every BMP code-point
encoding, sliced/truncated views, invalid offsets, and destination admission.
Native16 dot and captured-lookbehind checks independently compare raw/UTF
consumption and exact isolated-surrogate error codes. No existing byte
compiler, matcher, diagnostic, resource trace, public API or consumer changes.

## Independent native preflight

[utf16-navigation-oracle.c](../tests/Pcre/Native/utf16-navigation-oracle.c)
compiles four programs: forward dot and captured reverse lookbehind, each
raw and UTF. All four require successful JIT compilation and positive JIT
size. The same programs and match data are reused across all BMP code-point
values and every valid supplementary pair. Matches retain original native
offsets and captures; isolated-surrogate errors are expected and checked.

The oracle used static PCRE2 10.48 at
`7978954dbd2efc6f2196869290553cf1871b4ce6`, SLJIT at
`de0259c7aaf36aa40cba8014f3fad3edde9307f9`, and
`cc -O2 -std=c11 -Wall -Wextra -Werror`. It checked 4,456,448 native outcomes:
4,452,352 matches and 4,096 expected strict UTF errors, with no unexpected
outcomes. The 1,114,112 LF-terminated output rows have SHA-256:

`4b1ac65e354d2ae27463880c655573f20bbde7982a80ce13ca9f091211ae72c4`

Each row contains the six-digit code-point value followed by raw forward,
raw reverse, UTF forward and UTF reverse return codes. The output size is
16,719,872 bytes. Surrogate values are emitted as single original units;
values above `FFFF` are emitted as their native surrogate pairs.

This is a P10 building block. The native16 compiler, executable lowering,
matcher, UTF-16 diagnostics/resource policy and managed cross-width release
suite remain open. The existing shared PCRE semantic grammar must remain
shared when width-specific lowering is introduced. Final qualification and
source identities are recorded in the PR.
