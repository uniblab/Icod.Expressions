# Native 16-bit regex source literals

`PcreWordSequenceCompiler.CompileSequence` now admits direct non-ASCII literal
spelling in raw 16-bit and strict UTF-16 patterns. Existing ASCII syntax,
quoted literals, escaped non-ASCII literals, numeric escapes and absolute
assertions continue through the shared `PcreByteParser` grammar. The compiled
sequence still matches the original subject memory directly.

The compiler owns the original `char` pattern. An auxiliary ASCII grammar
stream has exactly one position per original 16-bit code unit. A non-ASCII
unit occupies a placeholder in that stream; the parser reads its actual value
from the original pattern when consuming a literal. Raw mode consumes one
unit, including isolated surrogates. UTF mode consumes one scalar, including
a surrogate pair, after the existing validation pass. The parser therefore
uses original code-unit offsets without a decoded-pattern offset map. Existing
byte patterns use the same parser without this word source. Extended-mode
pattern whitespace also reads the original word value; unsupported bracket
classes reject at their opening token before byte-only class parsing.

Pattern length admission remains measured in original units. Quoted and
escaped literal source uses the same reader. A malformed UTF-16 pattern is
diagnosed at the first invalid source unit before grammar lowering. Unsupported
character classes, captures, repetition, alternation and caseless options keep
their explicit unsupported result. Full PCRE2 diagnostic identity remains a
later gate; the current grammar reports the offending source token's original
code-unit offset.

## Evidence

Eighteen source/subject/start fixtures exercise direct BMP and supplementary
characters, isolated raw high/low surrogates, quoted and escaped forms,
numeric-escape mixtures, absolute assertions and nonzero starts. Each fixture
uses six compiled policy defaults and all 64 call masks for 6,912 live
PCRE2-16 comparisons. Four additional valid-Unicode patterns compare managed
byte and word scalar coordinates. Error cases check that a trailing escape
after a supplementary literal reports the original word offset. Ownership
unsupported-class and wide extended-whitespace cases keep the current bounded
admission contract.

An independent preflight of the existing ordinary-regex native oracle used
the statically linked PCRE2 10.48 and SLJIT revisions recorded in the
[sequence record](PCRE-WORD-SEQUENCES.md). In fixture/default/mask order it
produced 3,120 matches and 3,792 misses, with no native errors and positive
JIT size for every compiled row. The LF-terminated native output SHA-256 was:

`ba391f627ee4cde678eb75830421127e81a11165c03c8bc0b1a622352db6b051`

This increment extends only source literals in the internal word sequence
path. General native-width classes, ordered execution, capture rollback,
Unicode policies and public package qualification follow separately.
