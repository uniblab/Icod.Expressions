# PCRE byte-mode noncapturing groups

**Status:** internal syntax slice under [issue #7](https://github.com/uniblab/Icod.Expressions/issues/7). No public API or Grep switch.

## Intent and boundary

Support `(?:...)` in 8-bit non-UTF, non-UCP mode. It groups ordered alternatives and may be repeated greedily or lazily exactly as an ordinary group, but does not allocate a capture number. Capturing groups nested inside it retain their source-order numbers and participate in normal backtracking. `(?i:...)`, lookarounds, atomic/branch-reset groups, and every other `(?...)` form remain explicitly `UnsupportedSyntax`.

## Approach

The parser distinguishes `(?:` before allocating a capture slot and emits a dedicated noncapturing AST node. The program compiler emits its body without capture-start/end instructions, so the existing ordered backtracking engine handles alternatives, repetition, byte spans, cancellation and work limits. Both group types use the same 256-level nesting limit and unclosed-group diagnostic at the opening parenthesis. This makes no change to the GNU BRE/ERE engine or the public package contract.

Alternatives considered: allocating an invisible capture wastes state and shifts numeric references; rewriting noncapturing groups into another syntax loses their quantifier boundary. A dedicated node preserves source structure with no matcher changes.

## Verification

Compare capture numbering, alternatives, nested captures, optional and repeated groups, empty alternatives, NUL/high-byte input, unsupported `(?...)` prefixes and unclosed groups to PCRE2 8-bit behavior. Run the frozen GNU corpus, full .NET 8/9/10 suite and package/consumer checks on the six CI runners. [PCRE2's group specification](https://www.pcre.org/current/doc/html/pcre2pattern.html) defines `(?:...)` as grouping without capture numbering.
