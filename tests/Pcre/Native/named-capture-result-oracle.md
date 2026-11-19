# Native named-substring result preflight

The original [C probe](named-capture-result-oracle.c) builds against the pinned
PCRE2/SLJIT source/header/static native8 library as in [README](README.md), with
-O2 -std=c11 -Wall -Wextra -Werror. Pins are PCRE2
7978954dbd2efc6f2196869290553cf1871b4ce6 and SLJIT
de0259c7aaf36aa40cba8014f3fad3edde9307f9.

Input is id|mode|pattern-hex|subject-hex|query-name-hex, '-' for empty. Reuse
PR286's214 frozen pattern/subject/mode rows; each named query plus missing/0 gives
976 rows. Mode bits1=UTF,2=UCP,4=literal,8=no-auto-capture,16=duplicate names.
LF/Unicode BSR/nesting512 are explicit. Lines cap65,535 characters including newline.
Queries are native UTF-8 C strings without embedded NUL; managed exact-query NUL
behavior is a separate API control.

Output is id|numbered-captures|kind|selected-span|copied-bytes|lookup-code|JIT-return|
JIT-size. Kinds M/U/A/N mean present/known-unset/absent-name/no-match. The probe
calls pcre2_substring_get_byname and verifies its returned bytes/length against
first-set native table/ovector coordinates. Empty participating values stay M.
Source pcre2_substring.c157..178 confirms this native ordering; duplicate entries can preserve
historical registration order2,1, rather than canonical numeric order1,2.
Every context/program/match data/substring/buffer is released per row.

976 complete outcomes:403 present (259 empty),31 unset,266 absent,276 no-match;
all JIT0/positive size. No outcome is dropped. Raw output SHA256:
`33143a379fbf2c5598648f0ec857f6cc0782e95fd027d31d3518789b2ee049c2`.
Normal CI uses PCRE.NET1.6.0 numbered spans/native name metadata before managed
lookups. The literal selected spans/bytes come independently from the C API probe;
no production named lookup supplies expected values. JIT sizes are host facts,
not thresholds. Production has no native runtime fallback.
