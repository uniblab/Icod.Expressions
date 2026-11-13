# Opt-in surrogate escapes

Add default-false internal `AllowSurrogateEscapes`, corresponding to PCRE2
10.48 `PCRE2_EXTRA_ALLOW_SURROGATE_ESCAPES`. Permit escaped D800..DFFF in UTF8
patterns; preserve byte-mode limits and reject values above10FFFF. Adjacent
surrogates remain separate code points. Source and subject UTF validation stay
strict, and tolerant matching continues to treat their encoded bytes as malformed
fragments. This does not introduce a NO_UTF_CHECK execution mode or UTF16 support.

## Approach

Keep the escaped integer in the existing canonical literal/range vocabulary.
Gate only the three surrogate checks in ordinary braced values and standard/extra
alternate grammar. Snapshot the caller policy in the parser; source reset options
cannot clear it. Existing numeric syntax, overflow, caller restrictions and raw
byte diagnostics keep their order.

The builder's choice is to retain code points rather than rewrite surrogates as
failure nodes or broaden subject decoding. Failure rewriting would lose literal
width, ranges, negation and quoted label identity. Broad decoding would violate
the existing validated UTF contract. Retained integer literals cannot equal a
valid subject scalar. Case folding treats surrogate code points as identity;
valid Unicode17 mappings and out-of-range guards remain unchanged. Existing
range closure already excludes the surrogate gap from valid subject membership.

Literal-only assertion optimization must fall back to existing general assertion
lowering for a surrogate literal, preserving canonical width and avoiding invalid
UTF byte assertions. A focused PcreUtf8.EncodeCodePoint helper writes the native
three-byte surrogate representation only for compiled label data and the existing
COMMIT first-code-unit metadata; other values retain Rune encoding. It never
participates in source or subject validation. Default-false behavior is unchanged.

## Qualification

Pin native-first spans, captures, success labels, compile errors/end offsets and
strict-subject errors. Cover hex/octal/N/alternate u spellings, bounds and malformed
tokens, classes/ranges/algebra/complements, empty/unreachable/repeated branches,
atomic and general assertions, nonpairing, source prefixes, case/UCP/Turkish/reset
scope, decoded/raw/inert names, caller restrictions and whole-pattern validation.
All2048 surrogate label encodings get independent native fingerprints. Selected
all-offset protocols cover six newlines and malformed fragments. Native no-match
is only -1; failed/partial labels remain outside the implemented result contract.

Add tests before production, observe clean three-framework RED, freeze the fixture,
then implement and qualify all18 native framework/platform results and configured
frozen/Unicode/component/extraction/POSIX/API/corpus/package/script gates. Include
all repository inventories before final qualification; final GREEN/review/head/
base/merge evidence belongs in PR239 and only issue7's roadmap footer.

Baseline merged238 c17ea37489f5a550f7952117db11fa1141125f2f, tree
337ea771449a3ee4f2c761958f6ef4a7f3919536;68,025 tests/framework and253 earlier
test files. Preserve GNU/BRE/ERE, frozen35/214 outcomes/120 methods, canonical
AST/IR shape/opcodes/matcher/journals/work/quotas/public API/dependencies/workflows.
C#13/net8/9/10, UTF8+LF, no Python/subagents/SDK installs. Reuse the isolated
worktree. User owns merge/tag/publication; Grep last; wider #102 remains open.
