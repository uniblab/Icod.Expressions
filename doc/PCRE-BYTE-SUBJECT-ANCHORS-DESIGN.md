# PCRE byte-mode subject anchors

**Status:** internal syntax slice under [issue #7](https://github.com/uniblab/Icod.Expressions/issues/7). No public API or consumer migration.

## Intent and boundary

Support `\A`, `\Z`, and `\z` outside bracket classes in the 8-bit non-UTF matcher. `\A` tests absolute subject start; `\z` tests absolute subject end; `\Z` tests the absolute end or the position immediately before a single final LF byte. These assertions consume no bytes and retain authoritative byte positions/captures. A nonzero search start does not redefine either subject edge. The initial byte-mode newline convention is LF; other PCRE2 newline conventions remain pending.

The current foundation's `^` and `$` remain unchanged. In particular, `$` currently tests absolute end; adjusting it to full PCRE2 option-dependent semantics is a later review. Direct repetition of the new assertions is invalid syntax. Inside brackets, `\A`, `\Z`, and `\z` remain unsupported escapes, rather than literal characters.

## Approach

Reuse the existing absolute Start and End bytecode instructions for `\A` and `\z`. Add one zero-width instruction for `\Z` with an LF-before-end check. Parser and matcher use the same provisional work/cancellation accounting as existing anchors; failures flow through the ordinary backtracking stack. This avoids an additional state machine and keeps the GNU matcher untouched. PCRE2's [simple assertion specification](https://www.pcre.org/current/doc/html/pcre2pattern.html) distinguishes `\A`, `\Z`, and `\z`.

## Verification

Pin empty input, one and multiple trailing LFs, high/NUL bytes, absolute behavior under nonzero start offsets, capture rollback, bracket rejection, direct quantifier diagnostics, and resource/cancellation limits. Run net8/net9/net10 on all six CI runners, frozen GNU corpus, parity and package-only smoke. Grep and CommandFramework production code remain unchanged.
