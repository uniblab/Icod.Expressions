# Leading PCRE option combinations

**Status:** internal syntax slice for issue #7; no public PCRE API or consumer migration.

## Intent and scope

Grep accepts PCRE2 patterns with combinations of `i`, `m`, and `s` at the
start. The internal parser currently accepts only one such flag. Recognize
one or more leading option groups containing these letters, with at most one
hyphen per group to unset flags, including an override of the caller's
caseless compile option. A flag after the hyphen wins if repeated on both
sides. Keep the existing UTF-only `(?aD)` and `(?-aD)` prefixes and allow
them alongside leading `i`/`m`/`s` groups. Preserve the present pattern-wide
interpretation of these leading options across top-level alternatives.

Reject mid-pattern and scoped options and other PCRE2 option letters
explicitly. Mixed `aD` and `i`/`m`/`s` within a single group remain pending.
This is a bounded extension, not a claim of full PCRE2 option scope.

## Approach

The parser already carries `caseless`, `multiline`, `dotAll`, and `asciiBsD`
while building nodes. Consume consecutive recognized prefix groups before
parsing alternatives; toggle those existing fields in order. Do not add VM
instructions or a second parser. A general option-scope AST would support
mid-pattern flags but is broader than this slice; rewriting pattern text
would obscure diagnostics and byte offsets. The prefix scanner is chosen
because it preserves source positions and keeps unsupported placement clear.

Compare byte-mode and UTF-8 whole-match and capture spans to the pinned
PCRE.NET/PCRE2 reference. Exercise combined, repeated, unset, caller override,
top-level alternative and UTF `aD` order cases, plus rejected placement.
Run the six-job Staging suite and package smoke. `Icod.Grep` and
`Icod.CommandFramework` remain untouched.
