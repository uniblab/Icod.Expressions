# Internal PCRE empty and reset option groups

## Intent and scope

Continue the internal PCRE conformance gate after #81 by accepting empty option
settings and the reset prefix over the existing supported option grammar.
The pinned oracle remains PCRE.NET 1.6.0 / PCRE2 10.48 with Compiled/JIT.

Accept `(?)`, `(?-)`, trailing empty unset sections such as `(?i-)`, and scoped
counterparts such as `(?-:a)` and `(?i-:a)`. Accept `(?^)`, `(?^imsnxU)`, and
scoped reset settings. A reset clears i, m, n, s and both x modes before applying
its letters. **U and ASCII restrictions survive reset.** Unsupported J, r and
scoped/mixed ASCII option spellings remain explicit diagnostics. Native reset
also clears r, but this matcher does not accept r yet.

## Approaches

The selected approach extends the existing compile-time option reader, reusing
heap group frames and immutable node metadata. Adding a preprocessing rewrite
would obscure original byte offsets and lexical x boundaries. Runtime option
instructions would duplicate behavior already resolved at compile time.
This implementation choice follows the standing autonomous development remit.

## Semantics and implementation

- Keep option scanning transactional: do not mutate parser state on unsupported
  or malformed headers. Recognize ^ only immediately after `(?`; after ^ no
  hyphen is allowed. Without ^, permit one hyphen even with no letters on
  either side. Whitespace/comments may not split headers, including under x.
- Lower the minimum header length to three bytes so `(?)` is recognized.
- Apply a validated reset to caseless, multiline, dotAll, noAutoCapture and
  extended; leave ungreedy and asciiBsD untouched. Then use existing set/unset
  precedence and x/xx adjacency rules.
- Use existing group entry snapshots for scope restoration before outer
  quantifiers. Bare settings create no AST node, capture or nesting level.
  Compile-time branch propagation and source-byte diagnostics remain intact.
- Reset x takes effect immediately after the header; subsequent spaces and #
  characters become literal unless x is re-enabled. Quoted/bracket text stays
  literal. Reset n restores ordinary capture allocation; reset U preserves the
  current preference, including at quantifiers outside a reset scope.
- Standalone settings cannot be repeated. Malformed/unsupported headers retain
  UnsupportedSyntax at their opener; repeated valid settings use InvalidSyntax
  at the quantifier. Resource limits and cancellation remain unchanged.

## Verification and boundaries

Add explicit native outcome and group-count assertions, plus comparisons of all
capture byte spans, for all reset flags, retained U/ASCII restrictions, local and
caller casing, branch propagation, branch-reset numbering, assertions, references,
lexical transitions, UTF byte widths, malformed subject offsets and quantifiers.
Pin empty-header grammar, invalid/reset-minus forms, depth/capture limits,
compile cancellation and match work/cancellation. Remove obsolete pending rows.
Use test-first CI because no local SDK is available; final verification requires
all six Staging platform/architecture jobs, net8/net9/net10 tests, frozen source,
API/extraction and package-only consumer checks. No workflow changes are needed.

Production parser only; no compiler, VM, frames, capture journal, public API,
package version or runtime dependency changes. Remaining J/r/ASCII options,
non-atomic aliases, conditions/subroutines, Unicode and performance work remain
separate. Grep migration and CommandFramework removal remain gated on release
and all known consumer migrations.

## Reference

Pinned source: `src/PCRE/src/pcre2_compile.c` option scanner, lines 4990–5190;
`src/PCRE/doc/pcre2pattern.3`, internal option setting, lines 1810–1870, in the
existing PCRE.NET 1.6.0 checkout. Source accepts no-letter hyphen forms even
though the prose only explicitly describes `(?)`.
