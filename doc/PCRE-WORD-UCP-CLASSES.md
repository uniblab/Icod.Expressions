# Native 16-bit UCP shorthand and POSIX classes

The internal word sequence compiler now passes caller UCP and the five
individual ASCII restrictions to the shared parser. In UCP mode, `\d`, `\w`
and `\s` and their complements use the shared pinned Unicode membership.
The fourteen supported POSIX names select their Unicode predicates, including
the distinct digit/xdigit restriction. Existing ASCII defaults and `(?aD)`,
`(?aS)`, `(?aW)`, `(?aP)`, `(?aT)` and combined `(?a)` source switches retain
their parser behavior. The corresponding word options select restrictions
without changing the pattern source. Special-property forms such as
`\p{XWD}`, `\p{XAN}`, `\p{XPS}` and `\p{XSP}` share this bounded path.

Raw mode compares 16-bit units, including surrogate units; strict UTF-16
mode compares Unicode scalars. The latter reports supplementary matches in
original two-unit spans. Bracket unions, complements and POSIX negation use
the existing 64-member bound and one-step scalar comparison. Literal mode
remains literal regardless of UCP. Pattern start directives and special
property `\p{XUC}` remain outside the bounded word sequence profile.

## Evidence

Seventy-five source/subject/start fixtures across six compiled defaults and
64 call masks compare 28,800 outcomes with pinned PCRE2-16. The corpus covers
UCP on and off, raw units and strict scalars, all Unicode POSIX kinds,
shorthand complements, mixed sets, mark/number/whitespace boundaries,
supplementary values, raw surrogate starts and inline ASCII switches.
Five managed cross-width examples compare valid Unicode capture coordinates;
five caller ASCII restrictions compare with native inline switches. Exact
work quotas and 64/65 bracket controls check bounded admission.

The development-only [native word oracle](../tests/Pcre/Native/word-literal-oracle.c)
accepts UCP as bit 1 of its second protocol field, alongside UTF as bit 0.
The existing 0/1 modes retain their meanings. The preflight used the pinned
PCRE2 10.48 and SLJIT revisions recorded in the [word sequence
record](PCRE-WORD-SEQUENCES.md). All rows JIT compiled with positive code
size. It produced 21,616 matches and 7,184 misses with no native errors.
The LF-terminated output SHA-256 was:

`ade8a4e8f7217b7da3452f8e29e95586d55fe1dbc266dd5243f673bb29a31193`

The following [caseless increment](PCRE-WORD-CASELESS.md) admits folded
straight literals and bounded classes. Ordered branches and repetition,
durable captures, full strict/tolerant UTF policy and the P10–P12 release
gates remain open.
