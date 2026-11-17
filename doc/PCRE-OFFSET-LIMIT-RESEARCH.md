# Deferred caller offset-limit research

Investigated before PR274; no OffsetLimit API or implementation is included.
Keep this separate from the ASCII/high-range ALLANY proof.

Sources: PCRE2 10.48 commit7978954dbd2efc6f2196869290553cf1871b4ce6,
SLJIT de0259c7aaf36aa40cba8014f3fad3edde9307f9; PCRE.NET1.6.0
commit09639d45d4c03a8269fa2cde6edc7a7383bbc18c. PCRE.NET exposes
PcreMatchSettings.OffsetLimit and PcreOptions.UseOffsetLimit. Read its
PcreMatchSettings.cs and the pinned native pcre2_compile.c:is_anchored and
pcre2_jit_compile.c:mainloop_entry before designing the contract.

The inclusive limit bounds candidate starts, not match ends or the reported
start after backslash-K. A separate compile flag admits JIT limit handling.
Native JIT probes (LF/BSR Unicode, JIT_COMPLETE0, positive JIT sizes) on
subject4142, limit0, no match flags:

| Pattern | Compile options | Caller start | Native spans |
| --- | --- | ---: | --- |
| B | USE_OFFSET_LIMIT | 1 | no match |
| B | USE_OFFSET_LIMIT, ANCHORED | 1 | 1:1 |
| B | USE_OFFSET_LIMIT, FIRSTLINE | 0 | 1:1 |
| (?s).*B | USE_OFFSET_LIMIT, UTF, MATCH_INVALID_UTF | 1 | 1:1 |
| A\KB | USE_OFFSET_LIMIT | 0 | 1:1 |

Pinned JIT handles FIRSTLINE before an else-if USE_OFFSET_LIMIT branch;
do not assume the two limits are simply combined. Explicit and inferred
anchoring can ignore the limit. Native inferred anchoring covers more than
the existing dot-star proof: absolute/start anchors, nested groups, positive
ahead and two-branch conditions need their own pinned analysis. Backend
fallback and strict invalid-UTF priority also need separate evidence before
public admission. Existing managed whole-subject strict validation remains.

Future work needs native-first byte/UTF, aligned/split/malformed, anchoring,
FIRSTLINE, backslash-K, JIT/interpreter and resource/cancellation cases;
tests-only RED/freeze and the normal six-host qualification. Do not convert
these preliminary native probes into managed compatibility claims.
