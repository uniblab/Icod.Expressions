# Native 16-bit dot in straight sequences

`PcreWordSequenceCompiler.CompileSequence` now admits the shared `PcreByteAny`
node for ordinary dot in its internal straight-sequence subset. The parser
emits that node for both word modes, avoiding the byte-domain bitmap used by
raw 8-bit dot. The word matcher reads the original subject directly.

Raw mode consumes one 16-bit unit, including an isolated surrogate. Strict
UTF-16 mode consumes one validated scalar, so a supplementary character takes
two source units. With the current LF newline setting, dot rejects LF and
accepts CR; an inline `(?s)` enables dot-all and permits LF. End of input
cannot satisfy dot. Existing strict UTF subject validation precedes search,
and each dot attempt charges one work step. Compile and per-call anchoring,
end anchoring and empty-result policies continue to use the word matcher.

The raw 16-bit `\C` escape currently yields the same shared node in the byte
grammar. Its word binding explicitly rejects that escape until the code-unit
escape policy is reviewed separately. Character classes and general
alternation, repetition and capture execution remain outside this subset.

## Evidence

Twenty-two source/subject/start fixtures cover raw and UTF supplementary
subjects, isolated raw surrogates, LF, CR, CRLF, dot-all, empty input, anchors,
ASCII/BMP mixtures and inline option changes. Six compiled defaults and all
64 call masks yield 8,448 live native PCRE2-16 comparisons. Four valid Unicode
patterns also compare managed byte and word scalar coordinates. Exact quota
tests pin one dot step in both modes; a separate test requires explicit `\C`
rejection.

An independent preflight used the ordinary-regex native oracle and static
PCRE2 10.48/SLJIT revisions recorded in the [word sequence
record](PCRE-WORD-SEQUENCES.md). In fixture/default/mask order, it recorded
5,920 matches and 2,528 misses, with no native errors and positive JIT size
for each compiled row. The LF-terminated output SHA-256 was:

`74fbcc82c77fc2a0da962dfec196581cc7e336f28ec3ad78ffa9a08271bdf293`

General newline conventions, word classes, ordered execution, captures,
lookbehind and public package qualification remain later roadmap work.
