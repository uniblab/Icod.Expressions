# Internal PCRE direct assertion repeats

## Intent and scope

Close the direct lookaround-quantifier gap after general atomic lookahead. Support
`*`, `+`, `?`, exact/ranged/open counted bounds, lazy suffixes and possessive
suffixes on positive/negative general lookahead and existing fixed-literal
lookbehind. The oracle remains PCRE.NET 1.6.0 / PCRE2 10.48 Compiled/JIT.

PCRE2 normalizes an unlimited assertion maximum to minimum plus one:
`*` becomes `{0,1}`, `+` becomes `{1,2}`, and `{3,}` becomes `{3,4}`.
Finite counts must execute every required iteration even though the assertion
restores its subject position. Positive captures can change across iterations
through references to previous captures. Each assertion invocation is atomic;
ordinary greedy/lazy repeat choices can still be retried. Possessive repeats
commit all repeat choices. Negative assertions never retain captures.

## Architecture and alternatives

Use the existing finite counted-repeat compiler after parser normalization.
Attach the original quantifier offset to every direct assertion repeat, so
`+` cannot accidentally enter the ordinary unbounded zero-progress lowering and
expansion diagnostics identify the correct source byte. Preserve ordinary
repeat lowering and its progress guards for groups wrapping assertions.

A new assertion-repeat VM opcode would duplicate finite repeat choice and
journal handling. Ordinary unbounded lowering would stop on unchanged subject
positions and lose capture side effects. Neither fits the existing architecture.
This implementation choice is the maintainer agent's decision under the user's
standing authorization to continue development.

The shared assertion guards and capture journal preserve prior iteration state
on failed body branches, optional-repeat retries, negative assertions and outer
alternatives. No new frame fields, spans, subject slices or engine copies are
needed. Literal assertions retain their optimized instruction when unquantified.

## Constraints and limits

- Internal managed implementation only; no public API, package version, dependency
  or consumer migration changes. No destructive CommandFramework changes.
- Root project/solution, source in `/src`, tests in `/tests`, samples in `/samples`.
- C# 13, net8/net9/net10; local Debug and six OS/architecture Staging CI jobs.
- Existing parser depth/capture/count ceilings, 4,096 compiled instruction ceiling,
  1,024 saved-path ceiling, work budget and cancellation contracts remain.
- Counts above 65,535 remain invalid; a normalized maximum of 65,536 is an
  internal bound and must encounter the existing expansion ceiling safely.
- General lookbehind/length rules, assertion aliases and conditions remain pending.
- No Python. Production uses no native or System.Text.RegularExpressions fallback.

## Acceptance and review focus

Compare explicit outcomes and every numbered capture span with the compiled
oracle. Exercise capture growth that distinguishes `{3,4}` from a progress loop;
zero-count capture numbering; greedy/lazy/U and possessive behavior; prior values
restored after failed optional iterations and outer alternatives; nesting,
branch reset/names/options; byte and UTF subjects, malformed islands and every
selected caller offset. Pin malformed quantifier and expansion diagnostics.
Verify cancellation and work limits, the frozen source gate, full suite and
existing workload/extraction/package consumer gates on all six Staging jobs.
