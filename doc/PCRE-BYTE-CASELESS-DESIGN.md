# PCRE byte-mode ASCII caseless matching

**Status:** internal syntax slice under [issue #7](https://github.com/uniblab/Icod.Expressions/issues/7).

## Intent and boundary

Accept one leading `(?i)` and compare ASCII letters without regard to case in the existing 8-bit non-UTF matcher. This applies to literal atoms and escapes, bracket literals and ranges (including negation), POSIX upper/lower classes, fixed literal lookarounds, and numbered backreferences. Preserve the actual bytes and byte positions in captures. Bytes above `0x7f` remain exact under this provisional C/POSIX byte convention. Existing ASCII `\w`, `\b`, and other shorthand definitions remain unchanged.

The flag has whole-pattern scope across top-level alternatives and creates no capture. Other option spellings, combinations with `(?s)`, mid-pattern and scoped flags remain unsupported pending a proper option-scope design. An external caseless compile option for Grep `-i`, UTF/UCP folding, and locale-specific tables are later steps. No GNU behavior, public API, Grep, or CommandFramework production code changes here.

## Approaches

- **Selected:** Carry a single compile-time `Caseless` bit on the byte program. Keep original literals and captured bytes; compare ASCII pairs in the matcher and close positive bracket sets over ASCII letter pairs before negating them. This preserves byte offsets and class negation semantics with little extra runtime work.
- Fold pattern literals at parse time into byte sets. This works for atoms but requires another representation for backreferences and lookaround bytes and complicates capture handling.
- Fold the subject before matching. This loses the authoritative original bytes and creates a full-size copy; it is unsuitable for the byte engine.

## Verification

Test exact and caseless behavior separately: alternatives, quantified literals, escaped hex, positive and negated brackets, POSIX upper/lower classes, fixed literal lookarounds, numbered backreferences, NUL and high bytes, unsupported flag placement, and the existing work/cancellation limits. Run the full net8/net9/net10 suite and frozen GNU corpus on all six CI runners, plus parity and package-only smoke.
