# Raw 16-bit native reference bridge

The test adapter calls four existing C exports in PCRE.NET 1.6.0's native
library. It does not call PcreRegex (whose public string constructor always
adds UTF), reinterpret raw subjects as Unicode or implement a managed matcher.
No production dependency or assembly content changes.

Reference source: PCRE.NET tag v1.6.0,
`09639d45d4c03a8269fa2cde6edc7a7383bbc18c`:

- src/PCRE.NET.Native/pcrenet_compile.c
- src/PCRE.NET.Native/pcrenet_match.c
- src/PCRE.NET.Native/pcrenet_info.c
- src/PCRE.NET.Native/pcrenet.h

The wrapper is BSD-3-Clause; ABI declarations are described here for the
existing test dependency, not vendored as production implementation.
The adapter explicitly supplies native flags rather than the managed wrapper's
additional UTF flags. Mode bits remain 1=UTF,2=UCP,4=assertion K, with source
directives allowed to change effective settings. Newline LF, Unicode BSR and
lookbehind255/nesting512 agree with the independent C oracle.

Independent outcomes use [dual-width-oracle.c](dual-width-oracle.c), built
against PCRE2 10.48 `7978954dbd2efc6f2196869290553cf1871b4ce6` and SLJIT
`de0259c7aaf36aa40cba8014f3fad3edde9307f9`; see [build/protocol](README.md).
The same native 16-bit dot on D800 succeeds at0:1 in mode0 and returns UTF
error-24 in mode1. A raw start within a surrogate pair is a code-unit start,
not a Unicode boundary. These are reference contracts for future P10 lowering.

64-bit C structure sizes/selected offsets independently measured from the
pinned declarations:

| C fact | Bytes |
|---|---:|
| compile input / result | 64 /40 |
| match input / result | 80 /16 |
| match settings | 24 |
| compile optimization pointer | 56 |
| compile result name table | 32 |
| match settings /vector /callback | 32 /56 /64 |
| match result mark | 8 |

Only the supported six x64/ARM64 hosts are qualified. The ABI is intentionally
test-only and pinned; it is not a public managed compatibility promise.
Every query releases its acquired module reference, native program and owned
buffers, including compile/match error paths. Returned strings/metadata own
their storage and contain no borrowed pointers. C API facts and native fixture
results are independently pinned before implementing the adapter.
