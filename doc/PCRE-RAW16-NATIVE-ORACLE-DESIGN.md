# Direct native 16-bit reference oracle

Base merged PR280 `e934a49fdabb9f88a3f1d7763ee38de2f1956c67`, tree
`0fed61b0c3760cdace4ea2591c67e8d1c595e780`; 104,050 prior tests.
This continues P00/P01 infrastructure. It does not implement a managed
16-bit matcher, close P01/P09-P18 or alter a public API/package contract.
Work inline without agents, Python or SDK installation. Preserve all prior
sources/tests and A05. Keep #7/#102 and issue #7 footer unchanged; Grep last;
user owns merge/tags/publication.

## Native-first scope

The PCRE.NET 1.6.0 public string API always adds UTF. Its existing packaged
native library also exports pcrenet_compile_16, pcrenet_match_16,
pcrenet_pattern_info_16 and pcrenet_code_free_16 without adding UTF. Inspect
pinned wrapper source `09639d45d4c03a8269fa2cde6edc7a7383bbc18c`; call only
these fixed exports from test infrastructure. No private managed reflection,
source patch, unsafe C#, new dependency or test-project/workflow change.

Generate independent raw/UTF outcomes with unmodified PCRE2 10.48
`7978954dbd2efc6f2196869290553cf1871b4ce6`, SLJIT
`de0259c7aaf36aa40cba8014f3fad3edde9307f9` and the frozen #280 C oracle.
52 pattern/subject templates produce 136 protocols: 54 matches, 62 no-matches,
14 compile errors and six match errors. Every compiled probe has JIT return
zero and positive size. Include raw surrogate units and offsets within a
surrogate pair, wide literals/classes, UCP, references, recursion, conditions,
lookarounds, marks/cuts, signed K ranges, source UTF overrides and invalid UTF.
Raw units are not normalized as Unicode scalars.

Reuse all 140 matching and ten diagnostic rows from the frozen #280 catalog
under explicit UTF as an independent ABI/semantics regression. Check captures,
source offsets, marks, named slots, JIT size and actual compiled UTF flag.
Source directives can enable UTF even when the caller mode is raw; do not
assert caller flags as if they were the effective compiled flags.

## ABI and ownership

Extract the structure declarations from the pinned C source and compile a
separate sizeof/offsetof probe against the pinned 16-bit header. The 64-bit
layout is: compile input64/result40, match input80/result16, settings24;
optimization pointer56, name table32, settings32, vector56, callback64, mark8.
The six native CI hosts are all 64-bit; no x86 ABI claim is made.

Use explicitly sequential blittable structures and Cdecl delegates with native
pointers/uint32 fields. Validate the known layout before any native call. Load
only the existing PCRE.NET.Native module through the dependency assembly's
normal runtime resolution; free the acquired module reference after each
query. No broad filesystem search or arbitrary user-supplied symbol loading.

Copy native code-unit values directly into bounded unmanaged buffers, not
through a string API or transcoder. Patterns/subjects are capped at 4,096
units in this test adapter. Reject null arrays and unsupported mode bits before
native entry. Set explicit compile/match limits for controlled fixture work.
Query effective options and JIT size; copy numbered captures, successful marks
and name entries into managed values before freeing any program/buffer.
Preserve unset and reversed ranges; return compile errors and match errors
separately from no-match. No failed/partial-label or callout contract is added.

Use exception-safe cleanup for module, program, pattern, subject, info and
ovector buffers. Guard native counts, lengths and pointer arithmetic. A result
ownership test mutates caller arrays and performs repeated compile-error and
no-match calls before checking the original result. The adapter remains solely
under tests/Pcre and outside the production assembly/package.

## Qualification and freeze

136 new raw/UTF protocols +150 frozen catalog cases +ABI/ownership facts +six
managed-input guards add294 tests:104,344 total. Publish an unimplemented
adapter in a tests-only draft; expect294 missing-adapter failures /104,050
passes /zero skips. Reconcile every failure identity and cause, then freeze
the two fixture/assertion files before implementing only the helper. Freeze
the completed helper too. This RED measures a missing native test adapter,
not a managed matching feature. All production source remains unchanged.

Preserve375 prior test files /368 C# sources, all #280 fixtures/helper and A05
source35/corpus. Review the complete branch, run local support gates and
update this design/plan plus all five inventories before final source upload.
Build net8/net9/net10; execute full net10 and frozen214 cases /120 methods on
all six native hosts, zero failures/skips/coded warnings/errors and all
configured support/package/script gates. Final evidence, cleanup and plan
completion remain external on PR281; no source/docs/test edits after GREEN.
Full P01 closure, Perl/reducer, managed 16-bit/P10-P12 and other roadmap gates
remain separate work.
