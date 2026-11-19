# Capture metadata native preflight

The original [C probe](capture-info-oracle.c) uses pinned PCRE2/SLJIT from
[README](README.md). Only native8/JIT is needed. Build with the generated pinned
header and static libpcre2-8.a, using -O2 -std=c11 -Wall -Wextra -Werror.

Input has four fields: id|mode|pattern-hex|subject-hex; '-' is empty. Mode bits
are1=UTF,2=UCP,4=literal,8=no-auto-capture,16=duplicate names. Source options
remain source text. LF/Unicode BSR and nesting512 are explicit. Input lines cap
at65,535 characters including newline. Generate the214 input rows directly from
the frozen PcreCaptureInfoCases catalog; no production helper supplies expected
metadata. Successful output is id|capture-count|raw-name-table|M-or-N|captures|
JIT-return|JIT-size. Name entries retain native order and name-byte hex/number;
all slots including trailing unset are retained. Compile/error statuses remain
distinct. Native contexts, programs, subjects and match data are released per row.

PCRE2 commit7978954dbd2efc6f2196869290553cf1871b4ce6 / SLJIT
commitde0259c7aaf36aa40cba8014f3fad3edde9307f9 produce214 successful compiles,
133 matches/81 misses, all JIT0/positive size. Canonical fixture maps use unique
UTF-16-ordinal names and distinct ascending slots; native UTF-8 table order is
not misrepresented as ordinal order. Include supplemental/BMP ordering witnesses,
branch-reset duplicate aliases,256 slots and128-byte ASCII/UTF names. No native
outcome is discarded. Output SHA256:
`eeefb6b89d903ead1067b14670c09e209cedc3687152756461586f44b24ffdf3`.
JIT sizes are host facts, not portable thresholds. Normal six-host CI uses the
existing PCRE.NET1.6.0 test dependency; package consumer has no native fallback.
