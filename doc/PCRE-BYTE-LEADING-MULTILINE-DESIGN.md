# PCRE byte-mode leading multiline option

**Status:** internal syntax slice under [issue #7](https://github.com/uniblab/Icod.Expressions/issues/7).

## Intent and boundary

Accept one leading `(?m)` in the 8-bit non-UTF matcher under the current LF newline convention. Within the entire pattern, `^` succeeds at absolute subject start and immediately after an internal LF byte; it does not succeed after a final LF at subject end. `$` succeeds at absolute end or immediately before any LF byte. Both are zero-width. `\A`, `\Z`, and `\z` retain their existing absolute-subject semantics. A caller's internal `PcreByteOptions.Caseless` setting remains active when the pattern starts with `(?m)`.

The first slice supports one leading option only; combined, scoped, repeated, and mid-pattern options remain unsupported. Other newline conventions, `ALT_CIRCUMFLEX`, `DOLLAR_ENDONLY`, `NOTBOL`, and `NOTEOL` are pending. GNU production behavior, public API, Grep, and CommandFramework production code remain unchanged.

## Approaches

- **Selected:** Parse multiline `^`/`$` into distinct zero-width line-anchor nodes and bytecode instructions while keeping `\A`, `\Z`, and `\z` on their existing subject-anchor instructions. This pins the current option at compile time without changing matcher state or backtracking.
- Store a global multiline flag on every program and branch in the existing Start/End operations. This would couple absolute backslash anchors to option-sensitive operations, increasing the chance of broadening `\A` or `\z`.
- Pre-split the subject on LF. This loses authoritative byte offsets/captures and changes backtracking across line boundaries.

## Verification

Use PCRE2's documented LF line-anchor behavior and a local PCRE2 oracle for internal/final newline positions. Test top-level alternatives, nonzero search offsets, empty input, literal newline after `$`, raw NUL/high bytes, bracket literals, capture rollback, direct repetition diagnostics, interaction with caller caseless mode, and work/cancellation limits. Run all six Staging runners for net8/net9/net10, frozen GNU corpus, extraction parity, and package-only smoke.
