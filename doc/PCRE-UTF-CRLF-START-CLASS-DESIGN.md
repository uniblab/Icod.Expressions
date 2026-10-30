# Internal UTF CRLF leading-class candidate entry

## Intent and approach

Continue #7 and bounded leading-class cases from #102 after merged #101.
Restore native JIT LF entry for mandatory positive ASCII bracket starts that
exclude CR. Keep #98 endpoint-only UTF source metadata and #101 byte behavior.

PCRE.NET1.6.0 / PCRE2 10.48 JIT, UTF strict or MATCH_INVALID_UTF, ANYCRLF,
subject `0D0A41`, offset0: `[\x09-\x0c]A` matches1:2, while
`[\x09-\x0e]A` misses. HASCRORLF is0 for both. With start optimization
explicitly disabled the first also misses. This pins optimizer-dependent candidate
entry, not a reason to set source CR/LF metadata from semantic membership.

Pinned source pcre2_jit_compile.c5730..5840 does ordinary CRLF bumpalong;
13812..13825 then selects prefix/first-code-unit/start-bitmap forward search.
scan_prefix stops at a mandatory bitmap class, preserving its first-code-unit
constraint. pcre2_study.c1093..1900 unions start bitmaps from group branches,
handles mandatory classes, and rejects NOT singleton/property/call forms. Thus
an admitted leading class rejects CR before entering the VM; LF can be visited
without the ordinary CR bumpalong step. All alternatives must reject CR.

Approaches: restore the old UTF membership-derived newline flag (would undo #98
and break the CR-including control); clone the native optimizer machinery (broad
separate gate); or retain a bounded leading-class proof on the program. The
implementer's choice under the standing autonomous remit is the last. There is
no new forward scan or skipped VM work.

## Admitted source shapes

Add `bool UtfCrLfStartClass` to PcreByteProgram, computed only for UTF.
A cancellation-aware heap traversal follows the FIRST node of EVERY root branch:

- A nonempty positive PcreByteScalarSet, with no predicates and every normalized
  range in0..127, qualifies only if none contains CR. Do not infer positive class
  eligibility from negated membership or property evaluation. LF may be a range
  interior; explicit source-newline handling remains independent.
- Capturing and noncapturing groups qualify by applying the same rule to every
  body branch; atomic wrappers qualify through their Child. Keep capture bytecode.
- A required variable PcreByteRepeat qualifies through its Child: Minimum>=1 and
  Minimum!=Maximum. Exact1 qualifies too. Other exact repeats and optional repeats
  do not qualify. Wrapping a group follows the same conservative restriction.
- Empty branches, literals, byte classes, negated/scalar-predicate sets, dot,
  assertions, anchors, calls, backreferences, conditions and every other initial
  node return false. Suffix nodes after the first mandatory class do not matter.

Every first class must reject CR, and at least one must contain LF (OR across
classes). If LF is impossible, keep the old skip: unnecessary candidates can
exhaust the default budget on long CRLF-only input.

Bound the proof at1,000 visits, including the root and every worklist node. Native
set_start_bits increments a cumulative group-call count and fails above1,000;
counting additional class/repeat/atomic nodes is deliberately conservative. Fail
the proof beyond the cap and retain the baseline candidate policy. Some complex
native-supported cases can remain outside this proof and stay tracked in #102.

Variable repeats stop native prefix analysis at their mandatory class positions;
exact repeats can let analysis reach later literals. Native
`[\x09-\x0cA]{2}A` on `0D0A4141` misses, while the variable `{2,3}` form
matches1:3. Therefore a blanket mandatory-consumption proof is insufficient.

After a failed CR, with ANYCRLF/no explicit source newline, do not skip LF when
UtfCrLfStartClass is true. CR was already rejected by the qualifying class; this
models native entry without a new scan, comparison exemption or saved state.
Every candidate still runs the existing VM and Step, including observers and
cancellation. UTF validation, fragment barriers, original caller offset/G and
capture indexes remain unchanged. ByteCrLfPrefix and byte metadata are untouched.

## Remaining gates and constraints

This is bounded first-class parity. General fixed/optional repeat, assertion,
call/condition/anchor-prefixed and Unicode/predicate start optimization stays in
#102, coordinated with byte #99. Do not claim general optimizer parity or globally
allow LF. Known positive-Xuc malformed-UTF native defect #100 remains excluded.

Internal C#13, net8/net9/net10, UTF-8 LF; root project/solution, src, tests, samples;
no Python. Depth256/captures256/instructions4096/marks256/savedframes1024/default
work1,000,000 unchanged. No saved spans/interior pointers, frame/journal/opcode
changes, production native/Regex fallback, public API, package/version/dependency,
workflow, release or consumer change. User owns merge; Grep stays last and
CommandFramework removal requires publication and all known consumer migrations.

Native-first literals and all-capture matrices cover class membership/source
shape, wrapper/branch/repeat controls, strict/tolerant UTF, every caller offset,
LF/ANYCRLF, malformed runs/multibyte boundaries, byte controls, G, work exhaustion,
observer continuity and pre/mid cancellation, plus default-budget no-LF classes
on120,000 CRLF pairs and five alternatives with34-deep/220-deep wrappers (native
match/miss). Tests-only draft must fail for the
expected managed mismatch before production edits. No local SDK: six native
Staging jobs, full/frozen214/configured gates, then fresh whole-branch review.
