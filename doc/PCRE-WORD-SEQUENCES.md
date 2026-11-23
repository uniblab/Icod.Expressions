# Native width-16 sequences through the shared parser

`PcreWordSequenceCompiler.CompileSequence` introduces a bounded shared-grammar
entry point alongside `PcreWordLiteralCompiler.CompileLiteral`. Both produce
`PcreWordCompileResult` / `PcreCompiledWordPattern` and use `PcreWordOptions`.
These experimental internal names replace the earlier literal-only names;
the explicit literal entry point and its behavior remain intact.

The sequence binding currently admits ASCII source units. It copies those
units one for one into the existing `PcreByteParser`; source byte indices and
original word indices therefore coincide. There is no second regex parser.
Wide values are available through shared numeric escapes. Non-ASCII regex
source returns `UnsupportedSyntax` at its original code-unit offset; the
literal entry continues to accept the full raw16/UTF-16 source domain.

The shared parser now accepts an internal code-unit width, defaulting to
eight. Numeric character limits are `FF` for raw8, `FFFF` for raw16 and
`10FFFF` for UTF. Existing UTF surrogate admission and Python-octal rules
remain in force. Byte parser calls keep the default width and behavior.

Lowering admits one top-level sequence of case-sensitive literal nodes,
absolute start/end anchors (`\A`/`\z`) and requested-start assertions (`\G`).
Quoting, escaped punctuation, simple control escapes, hexadecimal and octal
escapes use the shared grammar. Every capture declaration is rejected,
including declarations erased from executable source paths. Other nodes,
alternatives, source start directives and caseless literal comparisons return
`UnsupportedSyntax`. Shared parser syntax failures map to provisional word
diagnostics at the corresponding original code-unit offset. Full native
diagnostic identity/priority remains a later gate.

The existing word executor now dispatches these shared nodes. Each literal
comparison or zero-width assertion costs one work step. Candidate admission,
completion, scalar validation and advancement retain the literal execution
contract; the prior literal work traces are unchanged. Source, subject and
result ownership rules, pattern ceilings, cancellation and native word offsets
remain as documented in [the literal execution record](PCRE-WORD-LITERAL.md).
No subject is copied or transcoded for matching.

## Evidence

Twenty source/subject fixtures, six compiled default masks and all 64 call
masks give 7,680 live native16 outcomes. They cover both raw and UTF modes,
nonzero offsets, absolute assertions, wide raw escapes, isolated raw surrogate
values, supplementary scalars, quoted metacharacters and control escapes.
Additional cases cover raw/UTF numeric domain errors, explicit unsupported
lowering, managed UTF-8/UTF-16 scalar-coordinate parity, observed/unobserved
exact budgets, ownership, length admission and cancellation. All earlier
literal cases remain unchanged.

The existing [native word oracle](../tests/Pcre/Native/word-literal-oracle.c)
has a compile-time `ORACLE_LITERAL` switch, defaulting to its original literal
behavior. With `-DORACLE_LITERAL=0`, the same seven-field protocol performs
ordinary regex compilation. The independent sequence preflight used static
PCRE2 10.48 at `7978954dbd2efc6f2196869290553cf1871b4ce6`, SLJIT at
`de0259c7aaf36aa40cba8014f3fad3edde9307f9`, and
`cc -O2 -std=c11 -Wall -Wextra -Werror`. Every compiled row required successful
JIT generation and positive JIT size. Fixture/default/mask ordering produced
4,640 matches and 3,040 misses, with no errors. LF-terminated output SHA-256:

`6de5b678f4d59dd1b8433f88bce46ff7845681de7027df77f7bf6d5edfac374c`

This is an initial shared-parser binding. General native-width source reading,
classes, grouping, captures, alternation/repetition/backtracking, caseless and
tolerant UTF-16, full diagnostics/resources, package consumers and broad
cross-width release qualification remain open. Final managed qualification
and source identities are recorded in the PR.
