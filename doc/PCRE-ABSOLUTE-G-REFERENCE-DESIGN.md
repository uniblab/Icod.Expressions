# Absolute `\g` backreference slice

**Status:** issue #7 internal syntax tranche. No public PCRE API or Grep
consumer switch is authorized by this design.

## Intent and boundary

PCRE2's [pattern reference](https://www.pcre.org/current/doc/html/pcre2pattern.html)
defines `\gN` and `\g{N}` as unambiguous absolute numeric backreferences;
`\g<...>` is a subroutine call, not the same operation. Add unsigned decimal
references from 1 through the internal capture ceiling of 256 in byte and
UTF-8 modes. Preserve the existing capture journal, ordered backtracking,
caseless comparison, work budget, cancellation, and source-byte spans.

Relative `\g{+N}`/`\g{-N}`, names, subroutine calls and ambiguous bare `\10`
remain unsupported. A zero, empty or nonexistent numeric reference is an
invalid pattern at its opening backslash. A reference within a character
class remains unsupported because it is not a consuming backreference there.

## Approach

Extend `PcreByteParser.ParseEscape` to read an absolute decimal `\g` operand,
then emit the existing `PcreByteBackreference` node and register it for the
post-parse group-count check. This shares VM execution with `\1`…`\9` and
retains the capture ceiling. A separate matcher opcode or parser would
duplicate semantics. Test against the pinned PCRE.NET 1.6.0 / PCRE2 10.48
test-only wrapper, with literal expected spans and diagnostic cases.

## Exit condition

The accepted forms match reference byte spans for a single capture, a tenth
capture, a literal digit after a delimited reference and a UTF-8 caseless
capture. An unset capture misses. Invalid and pending forms do not silently
become literals. All six Staging jobs, frozen-source verification, workload
smoke and the package-only consumer pass before review.
